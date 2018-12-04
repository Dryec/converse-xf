using System;

namespace Converse.Helpers
{
    public static class AppConstants
    {
        public static string TokenName = "Converse";

        public const string DefaultFullNodeIP = "54.236.37.243";
        public const int DefaultFullNodePort = 50051;
        public const string DefaultSolidityNodeIP = "47.89.187.247";
        public const int DefaultSolidityNodePort = 50051;

        public static class TokenMessageTypes
        {
            public static class User
            {
                public static int ChangeName = 1;
                public static int ChangeStatus = 2;
                public static int ChangeProfilePicture = 3;
                public static int BlockUser = 4;
                public static int SendMessage = 5;
            }
        }
    }
}
