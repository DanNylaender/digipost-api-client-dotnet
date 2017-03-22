﻿using System.Threading.Tasks;
using Digipost.Api.Client.Domain.Identify;
using Digipost.Api.Client.Domain.Search;
using Digipost.Api.Client.Domain.SendMessage;

namespace Digipost.Api.Client.Api
{
    internal interface IDigipostApi
    {
        IMessageDeliveryResult SendMessage(IMessage messageDataTransferObject);

        Task<IMessageDeliveryResult> SendMessageAsync(IMessage messageDataTransferObject);

        IIdentificationResult Identify(IIdentification identification);

        Task<IIdentificationResult> IdentifyAsync(IIdentification identification);

        Task<ISearchDetailsResult> SearchAsync(string search);

        ISearchDetailsResult Search(string search);
    }
}