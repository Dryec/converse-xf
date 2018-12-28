using System;
using Converse.Helpers;
using Newtonsoft.Json;

namespace Converse.TokenMessages
{
    public class AddUserToGroupTokenMessage : TokenMessage
    {
        public AddUserToGroupTokenMessage()
        {
        }

        public override int Type => AppConstants.TokenMessageTypes.Group.AddUser;

        [JsonProperty("address")]
        public byte[] Address { get; set; }

        [JsonProperty("private_key")]
        public byte[] PrivateKey { get; set; }
    }
}
