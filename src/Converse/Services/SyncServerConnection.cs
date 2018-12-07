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
        RestClient _client { get; }

        public SyncServerConnection()
        {
            _client = new RestClient("https://www.tron-society.com");
        }

        public async Task<List<ChatEntry>> GetChatsAsync() // TODO params
        {
            try
            {
                var request = new RestRequest("data/converse_test/chats.json", dataFormat: DataFormat.Json);

                var response = await _client.ExecuteGetTaskAsync(request);

                var chats = JsonConvert.DeserializeObject<List<ChatEntry>>(response.Content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return chats;
            }
            catch (JsonException)
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
    }
}
