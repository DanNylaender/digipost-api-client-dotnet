﻿using Digipost.Api.Client.Digipost.Api.Client;
using DigipostApiClientShared;
using DigipostApiClientShared.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Digipost.Api.Client
{

    public class AuthenticationHandler : DelegatingHandler
    {
        private ClientConfig ClientConfig { get; set; }
        private string URI { get; set; }



        public AuthenticationHandler(ClientConfig clientConfig ,string uri,HttpMessageHandler innerHandler)
            : base(innerHandler)
        
        {
            this.ClientConfig = clientConfig;
            this.URI = uri;
        }

        

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {

            Logging.Log(TraceEventType.Information, " AuthenticationHandler > sendAsync() - Start!");
            const string method = "POST";
            const string uri = "messages";
            var date = DateTime.UtcNow.ToString("R");
            var technicalSender = ClientConfig.TechnicalSenderId;
            var multipartContent = await request.Content.ReadAsByteArrayAsync();
            
            Logging.Log(TraceEventType.Information, " - Hashing byteStream of body content");
            var computeHash = ComputeHash(multipartContent);

            request.Headers.Add("X-Digipost-UserId", ClientConfig.TechnicalSenderId);
            request.Headers.Add("Date", date);
            request.Headers.Add("Accept", "application/vnd.digipost-v6+xml");
            request.Headers.Add("X-Content-SHA256", computeHash);
            request.Headers.Add("X-Digipost-Signature", ComputeSignature(method, uri, date, computeHash, technicalSender));

            
            return await base.SendAsync(request, cancellationToken);
        }


        private static string ComputeHash(Byte[] inputBytes)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
            var hashedBytes = hashAlgorithm.ComputeHash(inputBytes);

            return Convert.ToBase64String(hashedBytes);
        }

        private static string ComputeSignature(string method, string uri, string date, string sha256Hash, string userId)
        {
            const string parameters = ""; //HttpUtility.UrlEncode(request.RequestUri.Query).ToLower();

            Debug.WriteLine("Canonical string generated by .NET Client:");
            Debug.WriteLine("===START===");

            var s = method.ToUpper() + "\n" +
                    "/" + uri.ToLower() + "\n" +
                    "date: " + date + "\n" +
                    "x-content-sha256: " + sha256Hash + "\n" +
                    "x-digipost-userid: " + userId + "\n" +
                    parameters + "\n";

            Debug.Write(s);
            Debug.WriteLine("===SLUTT===");


            var rsa = GetCert().PrivateKey as RSACryptoServiceProvider;
            var privateKeyBlob = rsa.ExportCspBlob(true);
            var rsa2 = new RSACryptoServiceProvider();
            rsa2.ImportCspBlob(privateKeyBlob);

            var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
            var signature = rsa2.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));

            return Convert.ToBase64String(signature);
        }

        private static X509Certificate2 GetCert()
        {
            var storeMy = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            storeMy.Open(OpenFlags.ReadOnly);
            const string thumbprint = "F7DE9C384EE6D0A81DAD7E8E60BD3776FA5DE9F4";

            return CertificateUtility.SenderCertificate(thumbprint, Language.English);
        }
    }
}
