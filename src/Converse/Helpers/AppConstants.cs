using System;
using System.Text;

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
        public static readonly byte[] PropertyAddressPublicKey = { 0x04, 0xCF, 0xA8, 0x1E, 0x3D, 0x60, 0x3D, 0x76, 0x93, 0x1E, 0x85, 0x69, 0x95, 0x64, 0x5A, 0x68, 0x9C, 0x11, 0x36, 0x83, 0xA2, 0xD5, 0xDF, 0xA8, 0x54, 0xC0, 0xB7, 0x8F, 0x7D, 0x59, 0x9A, 0xCB, 0x44, 0x7B, 0x71, 0x27, 0x76, 0xB7, 0x5B, 0xD3, 0x46, 0xC0, 0x99, 0xCD, 0xCE, 0x5E, 0x41, 0x15, 0xCC, 0x37, 0x4E, 0xF3, 0x47, 0xDF, 0xE5, 0x9B, 0x21, 0xAA, 0x0A, 0x74, 0xE1, 0xE1, 0x23, 0xA8, 0x8B };
        public const int RequestTokenLimit = 20;

        public const int MaxMessageLength = 512;

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
                public const int AddDeviceId = 6;
            }
        }

        public static class FCM
        {

            public static class Topics
            {
                public const string Update = "update";
            }

            public static class Types
            {
                public const string Message = "msg";
                public const string UpdateUser = "update_user";
            }
        }

        public static class Keys
        {
            public static class User
            {
                public const string Mnemonic = "mnemonic";
                public const string PrivateKey = "privkey";
                public const string Name = "name";
                public const string Email = "email";
                public const string ProfileImageUrl = "profile_image_url";
            }
        }
    }
}
