using System;
using System.Threading.Tasks;
using Client;
using Converse.Database;
using Converse.Helpers;
using Converse.TokenMessages;
using Converse.Tron;
using Google.Protobuf;
using Grpc.Core;
using Protocol;
using Xamarin.Forms;

namespace Converse.Services
{
    public class TokenMessagesQueueService
    {
        TronConnection _tronConnection { get; set; }
        ConverseDatabase _database { get; set; }
        WalletManager _walletManager { get; set; }

        public TokenMessagesQueueService()
        {

        }

        public void Start(TronConnection tronConnection, ConverseDatabase database, WalletManager walletManager)
        {
            _tronConnection = tronConnection ?? throw new ArgumentNullException(nameof(tronConnection));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _walletManager = walletManager ?? throw new ArgumentNullException(nameof(walletManager));

            Task.Run(QueueTask);
        }

        public async Task<int> AddAsync(string sender, string receiver, TokenMessage tokenMessage)
        {
            return await _database.PendingTokenMessages.Insert(sender, receiver, tokenMessage);
        }

        public async Task<bool> WaitForAsync(int pendingId, int maxTimes = 99)
        {
            var tries = 0;
            while (true)
            {
                tries++;
                var transaction = await _database.PendingTokenMessages.Get(pendingId);
                if (transaction == null)
                {
                    return true;
                }
                if (tries > maxTimes)
                {
                    return false;
                }
                await Task.Delay(550);
            }
        }

        async Task QueueTask()
        {
            while (true)
            {
                if (_walletManager.Wallet != null)
                {
                    var tokenAmount = await _walletManager.Wallet.GetConverseTokenAmountAsync(_tronConnection);
                    if (tokenAmount > 0)
                    {
                        try
                        {
                            var pendingTokenMessage = await _database.PendingTokenMessages.GetFirst();
                            if (pendingTokenMessage != null)
                            {
                                try
                                {
                                    // TODO check for exceptions - internet off, wrong node ip…
                                    var transaction = await _tronConnection.CreateTransactionAsync(pendingTokenMessage.Sender, pendingTokenMessage.Receiver, pendingTokenMessage.Data);

                                    if (transaction != null && transaction.Result.Result)
                                    {
                                        _walletManager.Wallet.SignTransaction(transaction.Transaction);
                                        var result = await _tronConnection.Client.BroadcastTransactionAsync(transaction.Transaction);
                                        switch (result.Code)
                                        {
                                            case Protocol.Return.Types.response_code.BandwithError:
                                                break;
                                            case Protocol.Return.Types.response_code.ServerBusy:
                                                break;
                                            case Protocol.Return.Types.response_code.OtherError:
                                                break;
                                            case Protocol.Return.Types.response_code.ContractValidateError:
                                                break;
                                            case Protocol.Return.Types.response_code.Success:
                                            case Protocol.Return.Types.response_code.Sigerror:
                                            case Protocol.Return.Types.response_code.ContractExeError:
                                            case Protocol.Return.Types.response_code.DupTransactionError:
                                            case Protocol.Return.Types.response_code.TaposError:
                                            case Protocol.Return.Types.response_code.TooBigTransactionError:
                                            case Protocol.Return.Types.response_code.TransactionExpirationError:
                                            default:
                                                await _database.PendingTokenMessages.Delete(pendingTokenMessage.ID);
                                                break;
                                        }
                                        // TODO MessageService broadcast result
                                    }
                                    else
                                    {
                                        await _database.PendingTokenMessages.Delete(pendingTokenMessage.ID);
                                    }
                                }
                                catch (RpcException ex)
                                {
                                    switch (ex.StatusCode)
                                    {
                                        case StatusCode.OK:
                                            break;
                                        case StatusCode.Cancelled:
                                            break;
                                        case StatusCode.DataLoss:
                                            break;
                                        case StatusCode.Unknown:
                                            break;
                                        case StatusCode.ResourceExhausted:
                                            break;
                                        case StatusCode.Aborted:
                                            break;
                                        case StatusCode.Unavailable:
                                            break;
                                        // TODO check which code may need a retry as well and which not
                                        case StatusCode.InvalidArgument:
                                        case StatusCode.DeadlineExceeded:
                                        case StatusCode.NotFound:
                                        case StatusCode.AlreadyExists:
                                        case StatusCode.PermissionDenied:
                                        case StatusCode.Unauthenticated:
                                        case StatusCode.FailedPrecondition:
                                        case StatusCode.OutOfRange:
                                        case StatusCode.Unimplemented:
                                        case StatusCode.Internal:
                                        default:
                                            Device.BeginInvokeOnMainThread(() => throw ex);
                                            break;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Device.BeginInvokeOnMainThread(() => throw e);
                        }
                    }
                }
                await Task.Delay(500);
            }
        }
    }
}
