using System;
using Converse.Helpers;
using Newtonsoft.Json;

namespace Converse.TokenMessages
{
    public class SendMessageTokenMessage : TokenMessage
    {
        public SendMessageTokenMessage()
        {
        }

        public override int Type => AppConstants.TokenMessageTypes.User.SendMessage;

        [JsonProperty("message")]
        public byte[] Message { get; set; }
    }
}
