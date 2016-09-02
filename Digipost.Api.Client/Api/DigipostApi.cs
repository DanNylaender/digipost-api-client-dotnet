﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml;
using ApiClientShared;
using ApiClientShared.Enums;
using Common.Logging;
using Digipost.Api.Client.Action;
using Digipost.Api.Client.Domain;
using Digipost.Api.Client.Domain.DataTransferObjects;
using Digipost.Api.Client.Domain.Exceptions;
using Digipost.Api.Client.Domain.Identify;
using Digipost.Api.Client.Domain.Search;
using Digipost.Api.Client.Domain.SendMessage;
using Digipost.Api.Client.Domain.Utilities;
using Digipost.Api.Client.Extensions;
using Digipost.Api.Client.XmlValidation;

namespace Digipost.Api.Client.Api
{
    internal class DigipostApi : IDigipostApi
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int _minimumSearchLength = 3;

        private IDigipostActionFactory _digipostActionFactory;

        public DigipostApi(ClientConfig clientConfig, X509Certificate2 businessCertificate)
        {
            ClientConfig = clientConfig;
            BusinessCertificate = businessCertificate;
        }

        public DigipostApi(ClientConfig clientConfig, string thumbprint)
        {
            ClientConfig = clientConfig;
            BusinessCertificate = CertificateUtility.SenderCertificate(thumbprint, Language.English);
        }

        private ClientConfig ClientConfig { get; }

        private X509Certificate2 BusinessCertificate { get; }

        public IDigipostActionFactory DigipostActionFactory
        {
            get { return _digipostActionFactory ?? (_digipostActionFactory = new DigipostActionFactory()); }
            set { _digipostActionFactory = value; }
        }

        public IMessageDeliveryResult SendMessage(IMessage message)
        {
            var messageDelivery = SendMessageAsync(message);

            if (messageDelivery.IsFaulted && messageDelivery.Exception != null)
                throw messageDelivery.Exception.InnerException;

            return messageDelivery.Result;
        }

        public async Task<IMessageDeliveryResult> SendMessageAsync(IMessage message)
        {
            Log.Debug($"Outgoing Digipost message to Recipient: {message.DigipostRecipient}");

            var uri = new Uri("messages", UriKind.Relative);

            var messageDeliveryResultTask = GenericPostAsync<MessageDeliveryResultDataTransferObject>(message, uri);

            if (messageDeliveryResultTask.IsFaulted && messageDeliveryResultTask.Exception != null)
                throw messageDeliveryResultTask.Exception.InnerException;

            var messageDeliveryResult = DataTransferObjectConverter.FromDataTransferObject(await messageDeliveryResultTask.ConfigureAwait(false));

            Log.Debug($"Response received for message to recipient, {message.DigipostRecipient}: '{messageDeliveryResult.Status}'. Will be available to Recipient at {messageDeliveryResult.DeliveryTime}.");

            return messageDeliveryResult;
        }

        public IIdentificationResult Identify(IIdentification identification)
        {
            return IdentifyAsync(identification).Result;
        }

        public async Task<IIdentificationResult> IdentifyAsync(IIdentification identification)
        {
            Log.Debug($"Outgoing identification request: {identification}");

            var uri = new Uri("identification", UriKind.Relative);

            var identifyResponse = GenericPostAsync<IdentificationResultDataTransferObject>(identification, uri);

            if (identifyResponse.IsFaulted)
            {
                var exception = identifyResponse.Exception.InnerException;

                Log.Warn($"Identification failed, {exception}");

                if (identifyResponse.Exception != null)
                    throw identifyResponse.Exception.InnerException;
            }

            var identificationResultDataTransferObject = await identifyResponse.ConfigureAwait(false);
            var identificationResult = DataTransferObjectConverter.FromDataTransferObject(identificationResultDataTransferObject);

            Log.Debug($"Response received for identification to recipient, ResultType '{identificationResult.ResultType}', Data '{identificationResult.Data}'.");

            return identificationResult;
        }

        public ISearchDetailsResult Search(string search)
        {
            return SearchAsync(search).Result;
        }

        public async Task<ISearchDetailsResult> SearchAsync(string search)
        {
            Log.Debug($"Outgoing search request, term: '{search}'.");

            search = search.RemoveReservedUriCharacters();
            var uri = new Uri($"recipients/search/{Uri.EscapeUriString(search)}", UriKind.Relative);

            if (search.Length < _minimumSearchLength)
            {
                var emptyResult = new SearchDetailsResult();
                emptyResult.PersonDetails = new List<SearchDetails>();

                var taskSource = new TaskCompletionSource<ISearchDetailsResult>();
                taskSource.SetResult(emptyResult);
                return await taskSource.Task.ConfigureAwait(false);
            }

            var searchDetailsResult = await GenericGetAsync<SearchDetailsResult>(uri).ConfigureAwait(false);

            Log.Debug($"Response received for search with term '{search}' retrieved: '{searchDetailsResult.PersonDetails.Count}' entries.");

            return searchDetailsResult;
        }

        private Task<T> GenericPostAsync<T>(IRequestContent content, Uri uri)
        {
            var action = DigipostActionFactory.CreateClass(content, ClientConfig, BusinessCertificate, uri);

            ValidateXml(action.RequestContent);

            var responseTask = action.PostAsync(content);
            return GenericSendAsync<T>(responseTask);
        }

        private Task<T> GenericGetAsync<T>(Uri uri)
        {
            var action = DigipostActionFactory.CreateClass(ClientConfig, BusinessCertificate, uri);
            var responseTask = action.GetAsync();

            return GenericSendAsync<T>(responseTask);
        }

        private async Task<T> GenericSendAsync<T>(Task<HttpResponseMessage> responseTask)
        {
            var responseTaskResult = await responseTask.ConfigureAwait(false);

            var responseContent = await ReadResponse(responseTaskResult).ConfigureAwait(false);

            if (!responseTaskResult.IsSuccessStatusCode)
            {
                var emptyResponse = string.IsNullOrEmpty(responseContent);

                if (!emptyResponse)
                    ThrowNotEmptyResponseError(responseContent);
                else
                {
                    ThrowEmptyResponseError(responseTaskResult.StatusCode);
                }
            }
            return HandleSuccessResponse<T>(responseContent);
        }

        internal static void ValidateXml(XmlDocument document)
        {
            if (document.InnerXml.Length == 0)
            {
                return;
            }

            var xmlValidator = new ApiClientXmlValidator();
            string validationMessages;
            var isValidXml = xmlValidator.Validate(document.InnerXml, out validationMessages);

            if (!isValidXml)
            {
                throw new XmlException($"Xml was invalid. Stopped sending message. Feilmelding: '{validationMessages}'");
            }
        }

        private static async Task<string> ReadResponse(HttpResponseMessage requestResult)
        {
            var contentResult = await requestResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            return contentResult;
        }

        private static void ThrowNotEmptyResponseError(string responseContent)
        {
            var error = SerializeUtil.Deserialize<Error>(responseContent);
            throw new ClientResponseException("Error occured, check inner Error object for more information.", error);
        }

        private static void ThrowEmptyResponseError(HttpStatusCode httpStatusCode)
        {
            throw new ClientResponseException((int) httpStatusCode + ": " + httpStatusCode);
        }

        private static T HandleSuccessResponse<T>(string responseContent)
        {
            return SerializeUtil.Deserialize<T>(responseContent);
        }
    }
}