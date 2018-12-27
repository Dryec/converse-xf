using System;
using Converse.Helpers;
using Newtonsoft.Json;

namespace Converse.TokenMessages
{
    public class LeaveGroupTokenMessage : TokenMessage
    {
        public LeaveGroupTokenMessage()
        {
        }

        public override int Type => AppConstants.TokenMessageTypes.Group.Leave;
    }
}
