﻿using System;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Digipost.Api.Client.Handlers;

namespace Digipost.Api.Client
{
    public  abstract class DigipostAction
    {
        private readonly string _uri;
        public ClientConfig ClientConfig { get; set; }
        public X509Certificate2 PrivateCertificate { get; set; }

        protected DigipostAction(ClientConfig clientConfig, X509Certificate2 privateCertificate, string uri)
        {
            _uri = uri;
            ClientConfig = clientConfig;
            PrivateCertificate = privateCertificate;
        }

        protected Task<HttpResponseMessage> SendAsync(string xml, HttpContent content) 
        {
            Logging.Log(TraceEventType.Information, "> Starting to build request ...");
            var loggingHandler = new LoggingHandler(new HttpClientHandler());
            var authenticationHandler = new AuthenticationHandler(ClientConfig, PrivateCertificate, _uri, loggingHandler);
            Logging.Log(TraceEventType.Information, " - Initializing HttpClient");
            using (var client = new HttpClient(authenticationHandler))
            {
                Logging.Log(TraceEventType.Information, " - Sending request.");
                client.Timeout = TimeSpan.FromMilliseconds(ClientConfig.TimeoutMilliseconds);
                client.BaseAddress = new Uri(ClientConfig.ApiUrl.AbsoluteUri);
                Logging.Log(TraceEventType.Information, " - Request sent.");

                return client.PostAsync(_uri, content);
            }            
        }

    }

}
