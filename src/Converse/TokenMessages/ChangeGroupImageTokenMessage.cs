using System;
using Converse.Helpers;
using Newtonsoft.Json;

namespace Converse.TokenMessages
{
    public class ChangeGroupImageTokenMessage : TokenMessage
    {
        public ChangeGroupImageTokenMessage()
        {
        }

        public override int Type => AppConstants.TokenMessageTypes.Group.ChangeImage;

        [JsonProperty("image")]
        public byte[] ImageUrl { get; set; }

        [JsonProperty("clear")]
        public bool Clear { get; set; }
    }
}
