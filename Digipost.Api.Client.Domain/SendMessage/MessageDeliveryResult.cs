﻿using System;
using System.Collections.Generic;
using Digipost.Api.Client.Domain.Enums;

namespace Digipost.Api.Client.Domain.SendMessage
{
    public class MessageDeliveryResult : IMessageDeliveryResult
    {
        public string MessageId { get; set; }

        public long SenderId { get; set; }

        public MessageStatus Status { get; set; }

        public DateTime? DeliveryTime { get; set; }

        public DeliveryMethod DeliveryMethod { get; set; }

        public IDocument PrimaryDocument { get; set; }

        public IEnumerable<IDocument> Attachments { get; set; }
    }
}