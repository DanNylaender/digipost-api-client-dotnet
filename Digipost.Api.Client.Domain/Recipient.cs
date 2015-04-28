﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using Digipost.Api.Client.Domain.Enums;
using Digipost.Api.Client.Domain.Print;

namespace Digipost.Api.Client.Domain
{
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(TypeName = "message-recipient", Namespace = "http://api.digipost.no/schema/v6")]
    [XmlRoot("message-recipient", Namespace = "http://api.digipost.no/schema/v6", IsNullable = false)]
    public class Recipient
    {
        [XmlElement("digipost-address", typeof (string))]
        [XmlElement("name-and-address", typeof (RecipientByNameAndAddress))]
        [XmlElement("organisation-number", typeof (string))]
        [XmlElement("personal-identification-number", typeof (string))]
        [XmlChoiceIdentifier("IdentificationType")]
        public object IdentificationValue { get; set; }

        private PrintDetails _printdetails;

        private Recipient() { /**Must exist for serialization.**/ }

        public Recipient(RecipientByNameAndAddress recipientByNameAndAddress, PrintDetails printDetails = null)
        {

            IdentificationValue = recipientByNameAndAddress;
            IdentificationType = IdentificationChoice.NameAndAddress;
            Printdetails = printDetails;
        }

        public Recipient(IdentificationChoice identificationChoice, string id)
        {
            if (identificationChoice == IdentificationChoice.NameAndAddress)		
                throw new ArgumentException(string.Format("Not allowed to set identification choice by {0} " +		
                                                          "when using string as id",		
                                                          IdentificationChoice.NameAndAddress.ToString()));
            IdentificationValue = id;
            IdentificationType = identificationChoice;
        }

        public Recipient(PrintDetails printDetails)
        {
            Printdetails = printDetails;
        }

        [XmlElement("print-details")]
        public PrintDetails Printdetails
        {
            get
            {
                return _printdetails;
            }
            set
            {
                _printdetails = value;
            }
        }

        [XmlIgnore]
        public IdentificationChoice IdentificationType { get; private set; }
    }
}