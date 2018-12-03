using System;
using Converse.Helpers;
using Newtonsoft.Json;

namespace Converse.TokenMessages
{
    public class ChangeNameTokenMessage : TokenMessage
    {
        public ChangeNameTokenMessage()
        {
        }

        public override int Type => AppConstants.TokenMessageTypes.User.ChangeName;

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
