﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Digipost.Api.Client.Domain;

namespace Digipost.Api.Client.Handlers
{
    internal class AuthenticationHandler : DelegatingHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AuthenticationHandler(ClientConfig clientConfig, X509Certificate2 businessCertificate, string url,
            HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            ClientConfig = clientConfig;
            Url = url;
            BusinessCertificate = businessCertificate;
            Method = WebRequestMethods.Http.Get;
        }

        private ClientConfig ClientConfig { get; }

        private string Url { get; }

        private X509Certificate2 BusinessCertificate { get; }

        private string Method { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var date = DateTime.UtcNow.ToString("R");
            var senderId = ClientConfig.SenderId;

            request.Headers.Add("X-Digipost-UserId", senderId);
            request.Headers.Add("Date", date);
            request.Headers.Add("Accept", DigipostVersion.V6);
            request.Headers.Add("User-Agent", GetAssemblyVersion());

            string contentHash = null;

            if (request.Content != null)
            {
                Method = WebRequestMethods.Http.Post;
                var contentBytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                contentHash = ComputeHash(contentBytes);
                request.Headers.Add("X-Content-SHA256", contentHash);
            }

            var signature = ComputeSignature(Method, Url, date, contentHash, senderId, BusinessCertificate, ClientConfig.LogRequestAndResponse);
            request.Headers.Add("X-Digipost-Signature", signature);

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private static string GetAssemblyVersion()
        {
            var netVersion = Assembly
                .GetExecutingAssembly()
                .GetReferencedAssemblies().First(x => x.Name == "System.Core").Version.ToString();

            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

            return string.Format("digipost-api-client-dotnet/{0} (.NET/{1})", assemblyVersion, netVersion);
        }

        internal static string ComputeHash(byte[] inputBytes)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
            var hashedBytes = hashAlgorithm.ComputeHash(inputBytes);

            return Convert.ToBase64String(hashedBytes);
        }

        internal static string ComputeSignature(string method, string uri, string date, string contentSha256Hash,
            string userId, X509Certificate2 businessCertificate, bool LogRequestAndResponse)
        {
            const string parameters = "";

            if (LogRequestAndResponse)
            {
                Log.Debug("Compute signature, canonical string generated by .NET Client:");
                Log.Debug("=== SIGNATURE DATA START===");
            }

            string messageHeader;

            if (contentSha256Hash != null)
            {
                messageHeader = method.ToUpper() + "\n" +
                                "/" + uri.ToLower() + "\n" +
                                "date: " + date + "\n" +
                                "x-content-sha256: " + contentSha256Hash + "\n" +
                                "x-digipost-userid: " + userId + "\n" +
                                parameters + "\n";
            }
            else
            {
                messageHeader = method.ToUpper() + "\n" +
                                "/" + uri.ToLower() + "\n" +
                                "date: " + date + "\n" +
                                "x-digipost-userid: " + userId + "\n" +
                                parameters + "\n";
            }

            if (LogRequestAndResponse)
            {
                Log.Debug(messageHeader);
                Log.Debug("=== SIGNATURE DATA END ===");
            }

            var rsa2 = RsaCryptoServiceProvider(businessCertificate);

            var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(messageHeader));
            var signature = rsa2.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));
            var base64Signature = Convert.ToBase64String(signature);

            return base64Signature;
        }

        internal static RSACryptoServiceProvider RsaCryptoServiceProvider(X509Certificate2 businessCertificate)
        {
            var rsa = businessCertificate.PrivateKey as RSACryptoServiceProvider;
            var rsa2 = new RSACryptoServiceProvider();

            try
            {
                var privateKeyBlob = rsa.ExportCspBlob(true);
                rsa2.ImportCspBlob(privateKeyBlob);
            }
            catch (Exception e)
            {
                Log.Warn(e.Message);
                throw new CryptographicException(
                    "Exception while exporting CspBlob. Check if certificate is exportable.");
            }
            return rsa2;
        }
    }
}