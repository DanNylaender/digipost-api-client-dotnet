﻿using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Digipost.Api.Client.Common;
using Microsoft.Extensions.Logging;

namespace Digipost.Api.Client.Internal
{
    internal class AuthenticationHandler : DelegatingHandler
    {
        private static ILogger<DigipostClient> _logger;

        public AuthenticationHandler(ClientConfig clientConfig, X509Certificate2 businessCertificate, HttpMessageHandler innerHandler) : base(innerHandler)
        {
            ClientConfig = clientConfig;
            BusinessCertificate = businessCertificate;
            Method = WebRequestMethods.Http.Get;
        }

        private ClientConfig ClientConfig { get; }

        private X509Certificate2 BusinessCertificate { get; }

        private string Method { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var date = DateTime.UtcNow.ToString("R");
            var brokerId = ClientConfig.Broker.Id.ToString();

            request.Headers.Add("X-Digipost-UserId", brokerId);
            request.Headers.Add("Date", date);
            request.Headers.Add("Accept", DigipostVersion.V7);
            request.Headers.Add("User-Agent", GetAssemblyVersion());
            Method = request.Method.ToString();

            string contentHash = null;

            if (request.Content != null)
            {
                var contentBytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                contentHash = ComputeHash(contentBytes);
                request.Headers.Add("X-Content-SHA256", contentHash);
            }

            var signature = ComputeSignature(Method, request.RequestUri, date, contentHash, brokerId, BusinessCertificate, ClientConfig.LogRequestAndResponse);
            request.Headers.Add("X-Digipost-Signature", signature);

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private static string GetAssemblyVersion()
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

            return $"digipost-api-client-dotnet/{assemblyVersion} (netcore/{GetNetCoreVersion()})";
        }
        
        private static string GetNetCoreVersion()
        {
            try
            {
                var assembly = typeof(GCSettings).GetTypeInfo().Assembly;
                var assemblyPath = assembly.CodeBase.Split(new[] {'/', '\\'}, StringSplitOptions.RemoveEmptyEntries);
                var netCoreAppIndex = Array.IndexOf(assemblyPath, "Microsoft.NETCore.App");

                if (netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2)
                {
                    return assemblyPath[netCoreAppIndex + 1];
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return "AssemblyVersionNotFound";
        }

        internal static string ComputeHash(byte[] inputBytes)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
            var hashedBytes = hashAlgorithm.ComputeHash(inputBytes);

            return Convert.ToBase64String(hashedBytes);
        }

        internal static string ComputeSignature(string method, Uri uri, string date, string contentSha256Hash,
            string userId, X509Certificate2 businessCertificate, bool logRequestAndResponse)
        {
            var uriParts = new UriParts(uri);

            if (logRequestAndResponse)
            {
                _logger.LogDebug("Compute signature, canonical string generated by .NET Client:");
                _logger.LogDebug("=== SIGNATURE DATA START===");
            }

            string messageHeader;

            if (contentSha256Hash != null)
            {
                messageHeader = method.ToUpper() + "\n" +
                                uriParts.AbsoluteUri + "\n" +
                                "date: " + date + "\n" +
                                "x-content-sha256: " + contentSha256Hash + "\n" +
                                "x-digipost-userid: " + userId + "\n" +
                                uriParts.Parameters + "\n";
            }
            else
            {
                messageHeader = method.ToUpper() + "\n" +
                                uriParts.AbsoluteUri + "\n" +
                                "date: " + date + "\n" +
                                "x-digipost-userid: " + userId + "\n" +
                                uriParts.Parameters + "\n";
            }

            if (logRequestAndResponse)
            {
                _logger.LogDebug(messageHeader);
                _logger.LogDebug("=== SIGNATURE DATA END ===");
            }

            var rsa2 = businessCertificate.GetRSAPrivateKey();

            var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(messageHeader));
            var signature = rsa2.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var base64Signature = Convert.ToBase64String(signature);

            return base64Signature;
        }

        private class UriParts
        {
            public UriParts(Uri uri)
            {
                var datUri = uri.IsAbsoluteUri ? uri.AbsolutePath : "/" + uri.OriginalString;
                AbsoluteUri = datUri.ToLower();
                Parameters = uri.Query.Length > 0 ? uri.Query.Substring(1) : "";
            }

            public string AbsoluteUri { get; }

            public string Parameters { get; }
        }
    }
}
