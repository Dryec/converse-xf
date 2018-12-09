using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Converse.Models;
using Newtonsoft.Json;
using RestSharp;

namespace Converse.Services
{
    public class SyncServerConnection
    {
        public static class Endpoints
        {
            public const string Api = "api/";
            public const string Users = Api + "users/";
            public const string Groups = Api + "groups/";
            public const string Chats = Api + "chats/";
        }

        RestClient _client { get; }

        public SyncServerConnection()
        {
            _client = new RestClient("http://ec2-52-28-62-110.eu-central-1.compute.amazonaws.com");
        }

        public async Task<List<ChatEntry>> GetChatsAsync() // TODO params
        {
            try
            {
                var request = new RestRequest(Endpoints.Chats + "all/15", dataFormat: DataFormat.Json);

                var response = await _client.ExecuteGetTaskAsync(request);

                var chats = JsonConvert.DeserializeObject<List<ChatEntry>>(response.Content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return chats;
            }
            catch (JsonException e)
            {
                return null;
            }
        }

        public async Task<ChatMessages> GetChatMessagesAsync() // TODO params
        {
            try
            {
                var request = new RestRequest("data/converse_test/chatmessages.json", dataFormat: DataFormat.Json);

                var response = await _client.ExecuteGetTaskAsync(request);

                var messages = JsonConvert.DeserializeObject<ChatMessages>(response.Content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return messages;
            }
            catch (JsonException e)
            {
                return null;
            }
        }

        /*public async Task<UserInfo> GetUserInfoAsync(string address)
        {

        }

        public async Task<UserInfo> GetUserInfoAsync(int userId)
        {

        }
        public async Task<GroupInfo> GetGroupInfoAsync(string address)
        {

        }

        public async Task<GroupInfo> GetGroupInfoAsync(int groupId)
        {

        }*/

    }
}
