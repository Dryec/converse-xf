using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Converse.Database;
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
            public const string RequestTokens = Users + "{address_id}/requesttokens";
        }

        RestClient _client { get; }
        ConverseDatabase _database { get; set; }

        public SyncServerConnection()
        {
            _client = new RestClient("http://ec2-52-28-62-110.eu-central-1.compute.amazonaws.com");
        }

        public void SetDatabase(ConverseDatabase database)
        {
            _database = database;
        }

        public async Task<TokenRequestResponse> RequestTokens(object addressOrId)
        {
            try
            {
                var request = new RestRequest(Endpoints.RequestTokens, dataFormat: DataFormat.Json);
                request.AddUrlSegment("address_id", addressOrId);
                var response = await _client.ExecuteGetTaskAsync(request);

                var result = JsonConvert.DeserializeObject<TokenRequestResponse>(response.Content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return result;
            }
            catch (JsonException e)
            {
                return null;
            }
        }

        public async Task<List<ChatEntry>> GetChatsAsync(object addressOrId)
        {
            try
            {
                var request = new RestRequest(Endpoints.Chats + "all/" + addressOrId, dataFormat: DataFormat.Json);
                var response = await _client.ExecuteGetTaskAsync(request);

                var chats = JsonConvert.DeserializeObject<List<ChatEntry>>(response.Content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                if (chats != null)
                {
                    foreach (var chat in chats)
                    {
                        await _database.Chats.Update(chat);
                    }
                }
                return chats;
            }
            catch (JsonException e)
            {
                return null;
            }
        }

        public async Task<ChatMessages> GetMessagesAsync(int chatId, int start, int end)
        {
            try
            {
                var request = new RestRequest(Endpoints.Chats + $"{chatId}/{start}/{end}/", dataFormat: DataFormat.Json);
                var response = await _client.ExecuteGetTaskAsync(request);

                var messages = JsonConvert.DeserializeObject<ChatMessages>(response.Content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return messages;
            }
            catch (JsonException e)
            {
                return null;
            }
        }

        public async Task<UserInfo> GetUserAsync(object addressOrId)
        {
            try
            {
                var request = new RestRequest(Endpoints.Users + addressOrId, dataFormat: DataFormat.Json);
                var response = await _client.ExecuteGetTaskAsync(request);

                var user = JsonConvert.DeserializeObject<UserInfo>(response.Content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                await _database.Users.Update(user);
                return user;
            }
            catch (JsonException e)
            {
                return null;
            }
        }

        public async Task<GroupInfo> GetGroupAsync(string addressOrId)
        {
            try
            {
                var request = new RestRequest(Endpoints.Groups + addressOrId, dataFormat: DataFormat.Json);
                var response = await _client.ExecuteGetTaskAsync(request);

                var group = JsonConvert.DeserializeObject<GroupInfo>(response.Content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                await _database.Groups.Update(group);
                return group;
            }
            catch (JsonException e)
            {
                return null;
            }
        }
    }
}
