﻿using System;
using System.Xml.Serialization;

namespace Digipost.Api.Client.Domain.Enums
{
    public enum CountryIdentifier
    {
        /// <summary>
        ///     Country name in Norwegian or English.
        /// </summary>
        Country,

        /// <summary>
        ///     Country code according to the ISO 3166-1 alpha-2 standard.
        /// </summary>
        Countrycode
    }
}