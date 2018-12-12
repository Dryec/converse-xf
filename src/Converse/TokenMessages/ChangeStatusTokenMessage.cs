using System;
using Converse.Helpers;
using Newtonsoft.Json;

namespace Converse.TokenMessages
{
    public class ChangeStatusTokenMessage : TokenMessage
    {
        public ChangeStatusTokenMessage()
        {
        }

        public override int Type => AppConstants.TokenMessageTypes.User.ChangeStatus;

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
