using System;

namespace Converse.Helpers
{
    public static class AppConstants
    {
        public const string TokenName = "Converse";

        public const string DefaultFullNodeIP = "54.236.37.243";
        public const int DefaultFullNodePort = 50051;
        public const string DefaultSolidityNodeIP = "47.89.187.247";
        public const int DefaultSolidityNodePort = 50051;

        public const string PropertyAddress = "TFnJbbEXKWVNz84L9ysbWMvJGD2v8seZu8";
        public const int RequestTokenLimit = 20;

        public const string DefaultStatusMessage = "Hey, I'm new here!";

        public static class TokenMessageTypes
        {
            public static class User
            {
                public const int ChangeName = 1;
                public const int ChangeStatus = 2;
                public const int ChangeProfilePicture = 3;
                public const int BlockUser = 4;
                public const int SendMessage = 5;
            }
        }

        public static class Keys
        {
            public static class User
            {
                public const string Mnemonic = "mnemonic";
                public const string Name = "name";
                public const string Email = "email";
            }
        }
    }
}
