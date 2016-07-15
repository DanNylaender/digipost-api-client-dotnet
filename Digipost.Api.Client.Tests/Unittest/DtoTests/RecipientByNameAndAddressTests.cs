﻿using System.Collections.Generic;
using System.Linq;
using Digipost.Api.Client.Domain.Enums;
using Digipost.Api.Client.Domain.Print;
using Digipost.Api.Client.Domain.SendMessage;
using Digipost.Api.Client.Tests.CompareObjects;
using Digipost.Api.Client.Tests.Integration;
using Xunit;

namespace Digipost.Api.Client.Tests.Unittest.DtoTests
{
    
    public class RecipientByNameAndAddressTests
    {
        
        public class ConstructorMethod : RecipientByNameAndAddressTests
        {
            Comparator _comparator = new Comparator();

            [Fact]
            public void SimpleConstructor()
            {
                //Arrange
                const string fullName = "Ola Nordmann";
                const string addressLine1 = "Biskop Gunnerus Gate 14";
                const string postalCode = "0001";
                const string city = "Oslo";

                RecipientByNameAndAddress recipientByNameAndAddress = new RecipientByNameAndAddress(fullName, addressLine1, postalCode, city);

                //Act

                //Assert
                Assert.Equal(fullName, recipientByNameAndAddress.FullName);
                Assert.Equal(postalCode, recipientByNameAndAddress.PostalCode);
                Assert.Equal(city, recipientByNameAndAddress.City);
                Assert.Equal(addressLine1, recipientByNameAndAddress.AddressLine1);
            }
        }
    }
}
