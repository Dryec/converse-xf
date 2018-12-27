using System;
using Converse.Helpers;
using Newtonsoft.Json;

namespace Converse.TokenMessages
{
    public class JoinGroupTokenMessage : TokenMessage
    {
        public JoinGroupTokenMessage()
        {
        }

        public override int Type => AppConstants.TokenMessageTypes.Group.Join;
    }
}
