using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            public const string ChatMessages = Chats + "{chatID}/messages/{start}/{end}";
            public const string RequestTokens = Users + "{address_id}/requesttokens";
        }

        RestClient _client { get; }
        JsonSerializerSettings _jsonSerializerSettings;

        ConverseDatabase _database { get; set; }

        public SyncServerConnection()
        {
            _client = new RestClient("http://ec2-52-28-62-110.eu-central-1.compute.amazonaws.com");

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Populate,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
            };
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

                var result = JsonConvert.DeserializeObject<TokenRequestResponse>(response.Content, _jsonSerializerSettings);
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

                var chats = JsonConvert.DeserializeObject<List<ChatEntry>>(response.Content, _jsonSerializerSettings);
                if (chats != null)
                {
                    foreach (var chat in chats)
                    {
                        var x = await _database.Chats.Update(chat);
                    }
                }
                return chats;
            }
            catch (JsonException e)
            {
                return null;
            }
        }

        public async Task<ChatEntry> GetChatAsync(int chatId, string userAddressOrId)
        {
            try
            {
                var request = new RestRequest(Endpoints.Chats + $"{chatId}/{userAddressOrId}", dataFormat: DataFormat.Json);
                var response = await _client.ExecuteGetTaskAsync(request);

                var chat = JsonConvert.DeserializeObject<ChatEntry>(response.Content, _jsonSerializerSettings);
                if (chat != null)
                {
                    await _database.Chats.Update(chat);
                }
                return chat;
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
                var request = new RestRequest(Endpoints.ChatMessages, dataFormat: DataFormat.Json);
                request.AddUrlSegment("chatID", chatId);
                request.AddUrlSegment("start", start);
                request.AddUrlSegment("end", end);
                var response = await _client.ExecuteGetTaskAsync(request);

                var messages = JsonConvert.DeserializeObject<ChatMessages>(response.Content, _jsonSerializerSettings);

                if (messages != null)
                {
                    await _database.ChatMessages.Update(messages);
                    await _database.ChatMessages.Update(messages);
                }

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

                var user = JsonConvert.DeserializeObject<UserInfo>(response.Content, _jsonSerializerSettings);

                if (user != null)
                {
                    await _database.Users.Update(user);
                }
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

                var group = JsonConvert.DeserializeObject<GroupInfo>(response.Content, _jsonSerializerSettings);

                if (group != null)
                {
                    await _database.Groups.Update(group);
                }
                return group;
            }
            catch (JsonException e)
            {
                return null;
            }
        }
    }
}
