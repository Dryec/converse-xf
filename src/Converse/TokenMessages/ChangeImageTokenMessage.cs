using System;
using Converse.Helpers;
using Newtonsoft.Json;

namespace Converse.TokenMessages
{
    public class ChangeImageTokenMessage : TokenMessage
    {
        public ChangeImageTokenMessage()
        {
        }

        public override int Type => AppConstants.TokenMessageTypes.User.ChangeProfilePicture;

        [JsonProperty("clear")]
        public bool Clear { get; set; }

        [JsonProperty("image")]
        public string ImageUrl { get; set; }

    }
}
