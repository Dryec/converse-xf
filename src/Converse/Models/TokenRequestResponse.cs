using System;
using Newtonsoft.Json;

namespace Converse.Models
{
    public class TokenRequestResponse
    {
        public enum ResultType
        {
            Transferred,
            HasEnough,
            MaximumReached,
            ServerError,
        }

        [JsonProperty("result")]
        public ResultType Result { get; set; }

        [JsonProperty("txid")]
        public string TransactionID { get; set; }
    }
}
