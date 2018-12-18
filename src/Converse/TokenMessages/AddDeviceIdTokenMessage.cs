using System;
using Converse.Helpers;
using Newtonsoft.Json;

namespace Converse.TokenMessages
{
    public class AddDeviceIdTokenMessage : TokenMessage
    {
        public AddDeviceIdTokenMessage()
        {
        }

        public override int Type => AppConstants.TokenMessageTypes.User.AddDeviceId;

        [JsonProperty("device_id")]
        public byte[] DeviceID { get; set; }

    }
}
