using System;
using System.Diagnostics;
using System.Threading;
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
        public static bool IsRunning { get; set; }

        static TronConnection _tronConnection { get; set; }
        static ConverseDatabase _database { get; set; }
        static WalletManager _walletManager { get; set; }
        static SyncServerConnection _syncServerConnection { get; set; }

        public TokenMessagesQueueService()
        {

        }

        ~TokenMessagesQueueService()
        {

        }

        public void Start(TronConnection tronConnection, ConverseDatabase database, WalletManager walletManager, SyncServerConnection syncServerConnection)
        {
            _tronConnection = tronConnection ?? throw new ArgumentNullException(nameof(tronConnection));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _walletManager = walletManager ?? throw new ArgumentNullException(nameof(walletManager));
            _syncServerConnection = syncServerConnection ?? throw new ArgumentNullException(nameof(syncServerConnection));

            if (!IsRunning)
            {
                IsRunning = true;

                Task.Run(QueueTask);
            }
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
                    try
                    {
                        // Get first pending transaction
                        var pendingTokenMessage = await _database.PendingTokenMessages.GetFirst(_walletManager.Wallet.Address);
                        if (pendingTokenMessage != null)
                        {
                            // Check token amount
                            var tokenAmount = await _walletManager.Wallet.GetConverseTokenAmountAsync(_tronConnection);
                            if (tokenAmount <= AppConstants.RequestTokenLimit)
                            {
                                // Request token
                                var result = await _syncServerConnection.RequestTokens(_walletManager.Wallet.Address);
                                if (result != null)
                                {
                                    switch (result.Result)
                                    {
                                        case Models.TokenRequestResponse.ResultType.Transferred:
                                            tokenAmount = await _walletManager.Wallet.GetConverseTokenAmountAsync(_tronConnection);
                                            break;
                                        case Models.TokenRequestResponse.ResultType.HasEnough:
                                            break;
                                        case Models.TokenRequestResponse.ResultType.MaximumReached:
                                            break;
                                        case Models.TokenRequestResponse.ResultType.ServerError:
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }

                            if (tokenAmount > 0)
                            {
                                try
                                {
                                    // TODO check for exceptions - internet off, wrong node ip…
                                    var transaction = await _tronConnection.CreateTransactionAsync(
                                                                                    pendingTokenMessage.Sender.Equals("_") ? _walletManager.Wallet.Address : pendingTokenMessage.Sender, 
                                                                                    pendingTokenMessage.Receiver, pendingTokenMessage.Data);

                                    if (transaction != null)
                                    {
                                        var result = transaction.Result;
                                        if (result.Result)
                                        {
                                            _walletManager.Wallet.SignTransaction(transaction.Transaction);
                                            result = await _tronConnection.Client.BroadcastTransactionAsync(transaction.Transaction);
                                        }

                                        Debug.WriteLine($"{result.Code.ToString()} : {result.Message.ToStringUtf8()}", "MessageQueue");

                                        switch (result.Code)
                                        {
                                            case Protocol.Return.Types.response_code.BandwithError:
                                                MessagingCenter.Send(this, AppConstants.MessagingService.BandwidthError);
                                                break;
                                            case Protocol.Return.Types.response_code.ServerBusy:
                                                break;
                                            case Protocol.Return.Types.response_code.OtherError:
                                                break;
                                            case Protocol.Return.Types.response_code.ContractValidateError: // TODO Could break queue when wallet address changes
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
                    }
                    catch (Exception e)
                    {
                        Device.BeginInvokeOnMainThread(() => throw e);
                    }

                }
                await Task.Delay(150);
            }
        }
    }
}
