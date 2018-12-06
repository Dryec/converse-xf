using System;
using System.Threading.Tasks;
using Converse.Database;
using Xamarin.Forms;

namespace Converse.Services
{
    public class TransactionsQueueService
    {
        TronConnection _tronConnection { get; set; }
        ConverseDatabase _database { get; set; }

        public TransactionsQueueService()
        {

        }

        public void Start(TronConnection tronConnection, ConverseDatabase database)
        {
            _tronConnection = tronConnection ?? throw new ArgumentNullException(nameof(tronConnection));
            _database = database ?? throw new ArgumentNullException(nameof(database));

            Task.Run(QueueTask);
        }

        async Task QueueTask()
        {
            while (true)
            {
                try
                {
                    var pendingTransaction = await _database.PendingTransactions.GetFirst();
                    if (pendingTransaction != null)
                    {
                        var transaction = pendingTransaction.ToTransaction();
                        if (transaction != null)
                        {
                            var result = await _tronConnection.Client.BroadcastTransactionAsync(transaction);
                            switch (result.Code)
                            {
                                case Protocol.Return.Types.response_code.BandwithError:
                                    break;
                                case Protocol.Return.Types.response_code.ServerBusy:
                                    break;
                                case Protocol.Return.Types.response_code.OtherError:
                                    break;
                                case Protocol.Return.Types.response_code.Success:
                                case Protocol.Return.Types.response_code.Sigerror:
                                case Protocol.Return.Types.response_code.ContractValidateError:
                                case Protocol.Return.Types.response_code.ContractExeError:
                                case Protocol.Return.Types.response_code.DupTransactionError:
                                case Protocol.Return.Types.response_code.TaposError:
                                case Protocol.Return.Types.response_code.TooBigTransactionError:
                                case Protocol.Return.Types.response_code.TransactionExpirationError:
                                default:
                                    await _database.PendingTransactions.Delete(pendingTransaction.ID);
                                    break;
                            }
                            // TODO MessageService broadcast result
                        }
                        else
                        {
                            await _database.PendingTransactions.Delete(pendingTransaction.ID);
                        }
                    }
                }
                catch (Exception e)
                {
                    Device.BeginInvokeOnMainThread(() => throw e);
                }
                await Task.Delay(500);
            }
        }
    }
}
