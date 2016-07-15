﻿using System.Security.Cryptography.X509Certificates;
using ApiClientShared;
using Digipost.Api.Client.Action;
using Digipost.Api.Client.Tests.Integration;

using Xunit;
using Assert = Xunit.Assert;

namespace Digipost.Api.Client.Tests.Unittest
{
    
    public class DigipostActionFactoryTests
    {
        
        public class CreateClassMethod
        {
            internal ResourceUtility ResourceUtility;

            public CreateClassMethod()
            {
                ResourceUtility = new ResourceUtility("Digipost.Api.Client.Tests.Resources");
            }

            [Fact]
            public void ReturnsProperMessageAction()
            {
                //Arrange
                var factory = new DigipostActionFactory();
                var message = DomainUtility.GetSimpleMessageWithRecipientById();

                //Act
                var action = factory.CreateClass(message, new ClientConfig(), new X509Certificate2(), "uri");
                //Assert
                
                Assert.Equal(typeof(MessageAction), action.GetType());
            }

            [Fact]
            public void ReturnsProperIdentificationAction()
            {
                //Arrange
                var factory = new DigipostActionFactory();
                var identification = DomainUtility.GetPersonalIdentification();
                        
                //Act
                var action = factory.CreateClass(identification, new ClientConfig(), new X509Certificate2(), "uri");

                //Assert
                Assert.Equal(typeof(IdentificationAction), action.GetType());
            }
        }
    }
}
