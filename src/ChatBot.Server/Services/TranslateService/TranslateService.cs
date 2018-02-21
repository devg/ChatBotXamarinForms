﻿using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace ChatBot.Server.Services.AnalyticsService
{
    public class TranslateService
    {

        #region Properties

        private static HttpClient _client;
        public static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                    _client.DefaultRequestHeaders.Add(AppSettings.OcpApimSubscriptionKeyHeader, AppSettings.TranslatorKey);
                }
                return _client;
            }
        }
        #endregion

        public static async Task<string> Translate(string inputText, string inputLocale, string outputLocale)
        {
            try
            {
                string uri =
                    $"{AppSettings.TranslatorHost}{AppSettings.TranslatorPath}Translate?text" +
                    $"={HttpUtility.UrlEncode(inputText)}&from={inputLocale}&to={outputLocale}";

                HttpResponseMessage response = await Client.GetAsync(uri);
                Stream stream = await response.Content.ReadAsStreamAsync();
                Encoding encode = Encoding.GetEncoding("utf-8");

                StreamReader translatedStream = new StreamReader(stream, encode);
                XmlDocument xTranslation = new XmlDocument();
                xTranslation.LoadXml(translatedStream.ReadToEnd());

                return xTranslation.InnerText;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<string> DetermineLanguageAsync(string input)
        {
            try
            {
                string uri = $"{AppSettings.TranslatorHost}{AppSettings.TranslatorPath}Detect?text=" + HttpUtility.UrlEncode(input);
                
                HttpResponseMessage response = await Client.GetAsync(uri);
                string result = await response.Content.ReadAsStringAsync();

                var content = XElement.Parse(result).Value;
                return content;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        
    }
}