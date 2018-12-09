using System;
using System.Linq;
using System.Threading.Tasks;
using Bitcoin.BIP39;
using Client;
using Converse.Helpers;
using Converse.TokenMessages;
using Converse.Tron;
using Crypto;
using Google.Protobuf;
using Newtonsoft.Json;
using Protocol;

namespace Converse.Services
{
    public class TronConnection
    {
        public WalletClient Client { get; private set; }
        public WalletSolidityClient SolidityClient { get; private set; }
        public WalletExtensionClient ExtentionClient { get; private set; }

        public TronConnection()
        {

        }

        public void Connect()
        {
            var fullNode = AppConstants.DefaultFullNodeIP
                                       + ":"
                                       + AppConstants.DefaultFullNodePort.ToString();
            var solidityNode = AppConstants.DefaultSolidityNodeIP
                                           + ":"
                                           + AppConstants.DefaultSolidityNodePort.ToString();

            Client = new WalletClient(fullNode);
            SolidityClient = new WalletSolidityClient(solidityNode);
            ExtentionClient = new WalletExtensionClient(solidityNode);
        }

        public async Task<TransactionExtention> CreateTransactionFromTokenMessageAsync(string sender, string receiver, ITokenMessage message)
        {
            var contract = new TransferAssetContract
            {
                Amount = 1,
                AssetName = ByteString.CopyFromUtf8(AppConstants.TokenName),
                OwnerAddress = ByteString.CopyFrom(WalletAddress.Decode58Check(sender)),
                ToAddress = ByteString.CopyFrom(WalletAddress.Decode58Check(receiver))
            };

            var transaction = await Client.TransferAssetAsync(contract);
            if (transaction.Result.Result)
            {
                transaction.Transaction.RawData.Data = ByteString.CopyFromUtf8(JsonConvert.SerializeObject(message));
            }

            return transaction;
        }

        public async Task<TransactionExtention> CreateTransactionAsync(string sender, string receiver, string data)
        {
            var contract = new TransferAssetContract
            {
                Amount = 1,
                AssetName = ByteString.CopyFromUtf8(AppConstants.TokenName),
                OwnerAddress = ByteString.CopyFrom(WalletAddress.Decode58Check(sender)),
                ToAddress = ByteString.CopyFrom(WalletAddress.Decode58Check(receiver))
            };

            var transaction = await Client.TransferAssetAsync(contract);
            if (transaction.Result.Result)
            {
                transaction.Transaction.RawData.Data = ByteString.CopyFromUtf8(data);
            }

            return transaction;
        }

    }
}
