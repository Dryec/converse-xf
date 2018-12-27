using System;
using Converse.Helpers;
using Newtonsoft.Json;

namespace Converse.TokenMessages
{
    public class SendGroupMessageTokenMessage : TokenMessage
    {
        public SendGroupMessageTokenMessage()
        {
        }

        public override int Type => AppConstants.TokenMessageTypes.Group.Message;

        [JsonProperty("message")]
        public byte[] Message { get; set; }
    }
}
