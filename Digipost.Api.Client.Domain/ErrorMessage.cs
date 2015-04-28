﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Digipost.Api.Client.Domain
{
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://api.digipost.no/schema/v6")]
    [XmlRoot("error",Namespace = "http://api.digipost.no/schema/v6", IsNullable = false)]
    public class Error
    {
        private Error()
        {
        }

        [XmlElement("error-code")]
        public string Errorcode { get; set; }

        [XmlElement("error-message")]
        public string Errormessage { get; set; }

        [XmlElement("error-type")]
        public string Errortype { get; set; }

        [XmlElement("link")]
        public List<Link> Link { get; set; }
    }
}
