using System;
using Converse.Helpers;
using Newtonsoft.Json;

namespace Converse.TokenMessages
{
    public class ChangeGroupNameTokenMessage : TokenMessage
    {
        public ChangeGroupNameTokenMessage()
        {
        }

        public override int Type => AppConstants.TokenMessageTypes.Group.ChangeName;

        [JsonProperty("name")]
        public byte[] Name { get; set; }
    }
}
