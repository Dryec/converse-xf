using System;
using System.Threading.Tasks;
using Client;
using Converse.Helpers;
using Google.Protobuf;
using Newtonsoft.Json;
using Protocol;

namespace Converse.TokenMessages
{
    public abstract class TokenMessage : ITokenMessage
    {
        [JsonProperty("type")]
        public abstract int Type { get; }

        public async Task<TransactionExtention> CreateTransactionAsync(WalletClient client, string sender, string receiver)
        {
            var contract = new TransferAssetContract
            {
                Amount = 1,
                AssetName = ByteString.CopyFromUtf8(AppConstants.TokenName),
                OwnerAddress = ByteString.CopyFrom(WalletAddress.Decode58Check(sender)),
                ToAddress = ByteString.CopyFrom(WalletAddress.Decode58Check(receiver))
            };

            var transaction = await client.TransferAssetAsync(contract);
            transaction.Transaction.RawData.Data = ByteString.CopyFromUtf8(JsonConvert.SerializeObject(this));

            return transaction;
        }
    }
}
