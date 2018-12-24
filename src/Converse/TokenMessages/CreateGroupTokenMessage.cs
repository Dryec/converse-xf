using System;
using Converse.Helpers;
using Newtonsoft.Json;

namespace Converse.TokenMessages
{
    public class CreateGroupTokenMessage : TokenMessage
    {
        public CreateGroupTokenMessage()
        {
        }

        public override int Type => AppConstants.TokenMessageTypes.Group.Create;

        [JsonProperty("address")]
        public byte[] GroupAddress { get; set; }

        [JsonProperty("public_key")]
        public byte[] PublicKey { get; set; }

        [JsonProperty("private_key")]
        public byte[] PrivateKey { get; set; }

        [JsonProperty("name")]
        public byte[] Name { get; set; }

        [JsonProperty("description")]
        public byte[] Description { get; set; }

        [JsonProperty("image")]
        public byte[] ImageUrl { get; set; }

        [JsonProperty("is_public")]
        public bool IsPublic { get; set; }
    }
}
