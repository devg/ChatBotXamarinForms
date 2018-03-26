﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChatBot.Clients.Core.Models;
using Newtonsoft.Json;

namespace ChatBot.Clients.Core.Services.Bot
{
    public class BotService : IBotService
    {
        private string botUriChat = "";

        #region Properties

        private HttpClient _client;

        public HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                    _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AppSettings.DirectLineKey);
                }
                return _client;
            }
        }

        private BotMessage _conversation;

        public BotMessage Conversation
        {
            get { return _conversation; }
            set { _conversation = value; }
        }

        #endregion

        public async Task<Activity> Connect()
        {
            try
            {
                StringContent content = new StringContent("", Encoding.UTF8, "application/json");
                string result = await PostAsync(AppSettings.BaseBotEndPointAddress, content);
                var conversationResponse = JsonConvert.DeserializeObject<Conversation>(result);
                botUriChat = String.Format(AppSettings.BaseBotUriChat, conversationResponse.ConversationId);
                AppSettings.ConversationId = conversationResponse.ConversationId;
                var activitiesReceived = await Client.GetAsync(botUriChat);
                var activitiesReceivedData = await activitiesReceived.Content.ReadAsStringAsync();

                Conversation = JsonConvert.DeserializeObject<BotMessage>(activitiesReceivedData);

                var activity = Conversation.Activities.FirstOrDefault();
                if (activity != null)
                {
                    return activity;
                }
                else
                {
                    return SetExceptionMessage();
                }

            }
            catch (Exception ex)
            {
                return SetExceptionMessage();
            }
        }

        public async Task<Activity> SendMessage(Activity message)
        {
            BotMessage conversation;
            var contentPost = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
            var postResult = JsonConvert.DeserializeObject<ConversationId>(await PostAsync(botUriChat, contentPost));

            string jsonResultFromBot = await Client.GetStringAsync(botUriChat + "?watermark=" + Conversation.Watermark);
            conversation = JsonConvert.DeserializeObject<BotMessage>(jsonResultFromBot);
            if (conversation != null)
            {
                return conversation.Activities.Last();
            }
            else
            {
                return SetExceptionMessage();
            }

        }

        private async Task<string> PostAsync(string uri, HttpContent content)
        {
            try
            {
                HttpResponseMessage response = await Client.PostAsync(uri, content);
                Stream stream = await response.Content.ReadAsStreamAsync();
                StreamReader readStream = new StreamReader(stream, Encoding.UTF8);
                string result = readStream.ReadToEnd();
                return result;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private static Activity SetExceptionMessage()
        {
            return new Activity()
            {
                Text = "Something went wrong!",
                From = new UserMessage() { Id = "", Name = "ChatBotXamarin" }
            };
        }
    }
}
