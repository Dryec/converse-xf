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
    }
}
