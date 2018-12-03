using System;

namespace Converse.Helpers
{
    public static class AppConstants
    {
        public static string TokenName = "Converse";

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
