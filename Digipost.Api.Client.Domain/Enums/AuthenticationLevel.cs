﻿using System;
using System.Xml.Serialization;

namespace Digipost.Api.Client.Domain.Enums
{
    public enum AuthenticationLevel
    {
        /// <summary>
        ///     Default. Social security number and password is required to open the letter.
        /// </summary>
        Password,

        /// <summary>
        ///     Two factor authentication will be required to open the letter.
        /// </summary>
        TwoFactor
    }
}