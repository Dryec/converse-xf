using System;
using Converse.Helpers;
using Newtonsoft.Json;

namespace Converse.TokenMessages
{
    public class ChangeGroupDescriptionTokenMessage : TokenMessage
    {
        public ChangeGroupDescriptionTokenMessage()
        {
        }

        public override int Type => AppConstants.TokenMessageTypes.Group.ChangeDescription;

        [JsonProperty("description")]
        public byte[] Description { get; set; }
    }
}
