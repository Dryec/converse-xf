using System;
using System.Diagnostics;
using System.Globalization;
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
            var p14DateTime = new DateTime(2019, 1, 5, 12, 0, 0, DateTimeKind.Utc).ToUniversalTime();
            var isProposal14Active = DateTime.UtcNow > p14DateTime;
            Debug.WriteLine(isProposal14Active ? "Using Token ID" : "Using Token Name");
            if (!isProposal14Active)
            {
                Debug.WriteLine($"Time until #14 is active: {(p14DateTime - DateTime.UtcNow).ToString()}");
            }

            var contract = new TransferAssetContract
            {
                Amount = 1,
                AssetName = ByteString.CopyFromUtf8(isProposal14Active ? AppConstants.TokenID : AppConstants.TokenName),
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
            var p14DateTime = new DateTime(2019, 1, 5, 12, 0, 0, DateTimeKind.Utc).ToUniversalTime();
            var isProposal14Active = DateTime.UtcNow > p14DateTime;
            Debug.WriteLine(isProposal14Active ? "Using Token ID" : "Using Token Name");
            if(!isProposal14Active)
            {
                Debug.WriteLine($"Time until #14 is active: {(p14DateTime - DateTime.UtcNow).ToString()}");
            }

            var contract = new TransferAssetContract
            {
                Amount = 1,
                AssetName = ByteString.CopyFromUtf8(isProposal14Active ? AppConstants.TokenID : AppConstants.TokenName),
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
