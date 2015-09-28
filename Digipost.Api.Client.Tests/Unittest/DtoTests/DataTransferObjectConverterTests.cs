﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Digipost.Api.Client.Domain;
using Digipost.Api.Client.Domain.DataTransferObjects;
using Digipost.Api.Client.Domain.Enums;
using Digipost.Api.Client.Domain.Identify;
using Digipost.Api.Client.Domain.Print;
using Digipost.Api.Client.Domain.SendMessage;
using Digipost.Api.Client.Domain.Utilities;
using Digipost.Api.Client.Tests.CompareObjects;
using Digipost.Api.Client.Tests.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digipost.Api.Client.Tests.Unittest.DtoTests
{
    [TestClass]
    public class DataTransferObjectConverterTests
    {
        readonly Comparator _comparator = new Comparator();

        [TestClass]
        public class ToDataTransferObjectMethod : DataTransferObjectConverterTests
        {

            [TestMethod]
            public void IdentificationById()
            {
                //Arrange
                Identification source = new Identification(new RecipientById(IdentificationType.OrganizationNumber, "123456789"));
                IdentificationDataTransferObject expectedDto = new IdentificationDataTransferObject(IdentificationChoiceType.OrganisationNumber, "123456789");
                
                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);

                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void IdentificationByNameAndAddress()
            {
                //Arrange
                Identification source = new Identification(
                    new RecipientByNameAndAddress("Ola Nordmann", "Osloveien 22", "0001", "Oslo")
                    {
                        AddressLine2 = "Adresselinje2",
                        BirthDate = DateTime.Today,
                        PhoneNumber = "123456789",
                        Email = "tull@epost.no"
                    }
                  );

                IdentificationDataTransferObject expectedDto = new IdentificationDataTransferObject(
                   new RecipientByNameAndAddressDataTranferObject("Ola Nordmann", "0001", "Oslo", "Osloveien 22")
                    {
                        AddressLine2 = "Adresselinje2",
                        BirthDate = DateTime.Today,
                        PhoneNumber = "123456789",
                        Email = "tull@epost.no"
                    }
               );

                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);

                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void RecipientByNameAndAddress()
            {
                //Arrange
                var birthDate = DateTime.Now;

                var source = new RecipientByNameAndAddress(
                    fullName: "Ola Nordmann", 
                    addressLine1: "Biskop Gunnerus Gate 14", 
                    postalCode: "0001", 
                    city: "Oslo")
                {
                    AddressLine2 = "Etasje 15",
                    BirthDate = birthDate,
                    PhoneNumber = "123456789",
                    Email = "email@test.no"
                };

                RecipientDataTransferObject expectedDto = new RecipientDataTransferObject(
                    new RecipientByNameAndAddressDataTranferObject(
                        fullName:"Ola Nordmann", 
                        postalCode:"0001", 
                        city:"Oslo", 
                        addressLine1:"Biskop Gunnerus Gate 14"
                        )
                    {
                        AddressLine2 = "Etasje 15",
                        BirthDate = birthDate,
                        PhoneNumber = "123456789",
                        Email = "email@test.no"
                    });
                
                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);

                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void RecipientById()
            {
                //Arrange
                RecipientById source = new RecipientById(
                    IdentificationType.DigipostAddress,
                    "ola.nordmann#2233"
                    );
                
                RecipientDataTransferObject expectedDto = new RecipientDataTransferObject(
                    IdentificationChoiceType.DigipostAddress, 
                    "ola.nordmann#2233");

                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);

                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void Document()
            {
                //Arrange
                IDocument source = new Document("TestSubject", "txt", new byte[2], AuthenticationLevel.Password, SensitivityLevel.Sensitive, new SmsNotification(3));
                DocumentDataTransferObject expectedDto = new DocumentDataTransferObject("TestSubject","txt", new byte[2], AuthenticationLevel.Password, SensitivityLevel.Sensitive, new SmsNotificationDataTransferObject(3));
                expectedDto.Guid = source.Guid;

                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);

                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void Message()
            {
                //Arrange
                var source = DomainUtility.GetMessageWithBytesAndStaticGuidRecipientById();

                var expectedDto = DomainUtility.GetMessageDataTransferObjectWithBytesAndStaticGuidRecipientById();


                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);

                //Assert

                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void MessageWithPrintDetailsAndRecipientById()
            {
                //Arrange
                var printDetails = DomainUtility.GetPrintDetails();
                var source = DomainUtility.GetMessageWithBytesAndStaticGuidRecipientById();
                source.PrintDetails = printDetails;

                var expectedDto = DomainUtility.GetMessageDataTransferObjectWithBytesAndStaticGuidRecipientById();
                expectedDto.RecipientDataTransferObject.PrintDetailsDataTransferObject =
                    DomainUtility.GetPrintDetailsDataTransferObject();


                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);

                //Assert

                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void MessageWithPrintDetailsAndRecipientByNameAndAddress()
            {
                //Arrange
                var printDetails = DomainUtility.GetPrintDetails();
                var source = DomainUtility.GetMessageWithBytesAndStaticGuidRecipientByNameAndAddress();
                source.PrintDetails = printDetails;

                var expectedDto = DomainUtility.GetMessageDataTransferObjectWithBytesAndStaticGuidRecipientNameAndAddress();
                expectedDto.RecipientDataTransferObject.PrintDetailsDataTransferObject =
                    DomainUtility.GetPrintDetailsDataTransferObject();

                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);

                //Assert

                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
            }


            [TestMethod]
            public void ForeignAddress()
            {
                //Arrange
                ForeignAddress source = new ForeignAddress(
                   CountryIdentifier.Country,
                   "NO",
                   "Adresselinje1",
                   "Adresselinje2",
                   "Adresselinje3",
                   "Adresselinje4"
                   );

                ForeignAddressDataTransferObject expectedDto = new ForeignAddressDataTransferObject(
                   CountryIdentifier.Country,
                   "NO",
                   "Adresselinje1",
                   "Adresselinje2",
                   "Adresselinje3",
                   "Adresselinje4"
                   );

                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);

                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void NorwegianAddress()
            {
                //Arrange
                NorwegianAddress source = new NorwegianAddress("0001", "Oslo", "Addr1", "Addr2", "Addr3");

                NorwegianAddressDataTransferObject expectedDto = new NorwegianAddressDataTransferObject("0001", "Oslo", "Addr1", "Addr2", "Addr3");
                
                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);

                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void PrintRecipientFromForeignAddress()
            {
                //Arrange
                PrintRecipient source = new PrintRecipient(
                    "Name",
                    new ForeignAddress(
                        CountryIdentifier.Country,
                        "NO",
                        "Adresselinje1",
                        "Adresselinje2",
                        "Adresselinje3",
                        "Adresselinje4"
                        ));

                PrintRecipientDataTransferObject expectedDto = new PrintRecipientDataTransferObject("Name", new ForeignAddressDataTransferObject(
                        CountryIdentifier.Country,
                        "NO",
                        "Adresselinje1",
                        "Adresselinje2",
                        "Adresselinje3",
                        "Adresselinje4"
                        ));
                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);
                
                //Assert
                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void PrintRecipientFromNorwegianAddress()
            {
                //Arrange
                PrintRecipient source = new PrintRecipient(
                    "Name",
                 new NorwegianAddress("0001", "Oslo", "Addr1", "Addr2", "Addr3"));

                PrintRecipientDataTransferObject expectedDto = new PrintRecipientDataTransferObject("Name", new NorwegianAddressDataTransferObject(
                        "0001", "Oslo", "Addr1", "Addr2", "Addr3"));
                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);

                //Assert
                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void PrintReturnRecipientFromForeignAddress()
            {
                //Arrange
                PrintReturnRecipient source = new PrintReturnRecipient(
                    "Name",
                    new ForeignAddress(
                        CountryIdentifier.Country,
                        "NO",
                        "Adresselinje1",
                        "Adresselinje2",
                        "Adresselinje3",
                        "Adresselinje4"
                        ));

                PrintReturnRecipientDataTransferObject expectedDto = new PrintReturnRecipientDataTransferObject("Name", new ForeignAddressDataTransferObject(
                        CountryIdentifier.Country,
                        "NO",
                        "Adresselinje1",
                        "Adresselinje2",
                        "Adresselinje3",
                        "Adresselinje4"
                        ));
                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);

                //Assert
                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void PrintReturnRecipientFromNorwegianAddress()
            {
                //Arrange
                PrintReturnRecipient source = new PrintReturnRecipient(
                    "Name",
                 new NorwegianAddress("0001", "Oslo", "Addr1", "Addr2", "Addr3"));

                PrintReturnRecipientDataTransferObject expectedDto = new PrintReturnRecipientDataTransferObject("Name", new NorwegianAddressDataTransferObject(
                        "0001", "Oslo", "Addr1", "Addr2", "Addr3"));
                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);

               //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void PrintDetails()
            {
                //Arrange
                PrintDetails source = new PrintDetails(
                    new PrintRecipient(
                        "Name", 
                        new NorwegianAddress("0001", "Oslo", "Addr1", "Addr2", "Addr3")),
                        new PrintReturnRecipient(
                            "ReturnName", 
                            new NorwegianAddress("0001", "OsloRet", "Addr1Ret", "Addr2Ret", "Addr3Ret")));

                var expectedDto = new PrintDetailsDataTransferObject(
                     new PrintRecipientDataTransferObject(
                         "Name", 
                         new NorwegianAddressDataTransferObject("0001", "Oslo", "Addr1", "Addr2", "Addr3")),  
                         new PrintReturnRecipientDataTransferObject(
                             "ReturnName", 
                             new NorwegianAddressDataTransferObject("0001", "OsloRet", "Addr1Ret", "Addr2Ret", "Addr3Ret")));

                //Act
                var actualDto = DataTransferObjectConverter.ToDataTransferObject(source);
                
                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actualDto, out differences);
                Assert.AreEqual(0, differences.Count());
                Assert.IsNull(DataTransferObjectConverter.ToDataTransferObject((IPrintDetails) null));
            }

            [TestMethod]
            public void SmsNotification()
            {
                //Arrange
                var atTimes = new List<DateTime>{ DateTime.Now, DateTime.Now.AddHours(3)};
                var afterHours = new List<int>(){4,5};

                var source = new SmsNotification();
                source.NotifyAfterHours.AddRange(afterHours);
                source.NotifyAtTimes.AddRange(atTimes);

                var expectedDto = new SmsNotificationDataTransferObject();
                expectedDto.NotifyAfterHours.AddRange(afterHours);
                expectedDto.NotifyAtTimes.AddRange(atTimes.Select(a => new ListedTimeDataTransferObject(a)));

                //Act
                var actual = DataTransferObjectConverter.ToDataTransferObject(source);
                
                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expectedDto, actual, out differences);
                Assert.AreEqual(0, differences.Count());
            }
        }

        [TestClass]
        public class FromDataTransferObjectMethod : DataTransferObjectConverterTests
        {
            [TestMethod]
            public void IdentificationResultFromPersonalIdentificationNumber()
            {
                //Arrange
                IdentificationResultDataTransferObject source = new IdentificationResultDataTransferObject();
                source.IdentificationResultCode = IdentificationResultCode.Digipost;
                source.IdentificationValue = null;
                source.IdentificationResultType = IdentificationResultType.DigipostAddress;

                IdentificationResult expected = new IdentificationResult(IdentificationResultType.DigipostAddress, "");

                //Act
                var actual = DataTransferObjectConverter.FromDataTransferObject(source);

                //Assert
                Assert.AreEqual(source.IdentificationResultType, expected.ResultType);
                Assert.AreEqual("", actual.Data);
                Assert.AreEqual(null, actual.Error);
            }

            [TestMethod]
            public void IdentificationResultFromPersonByNameAndAddress()
            {
                //Arrange
                IdentificationResultDataTransferObject source = new IdentificationResultDataTransferObject();
                source.IdentificationResultCode = IdentificationResultCode.Digipost;
                source.IdentificationValue = "jarand.bjarte.t.k.grindheim#8DVE";
                source.IdentificationResultType = IdentificationResultType.DigipostAddress;

                IdentificationResult expected = new IdentificationResult(IdentificationResultType.DigipostAddress, "jarand.bjarte.t.k.grindheim#8DVE");

                //Act
                IIdentificationResult actual = DataTransferObjectConverter.FromDataTransferObject(source);

                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expected, actual, out differences);
                Assert.AreEqual(0, differences.Count());

                Assert.AreEqual(source.IdentificationValue, actual.Data);
                Assert.AreEqual(source.IdentificationResultType, actual.ResultType);
                Assert.AreEqual(null, actual.Error);
            }

            [TestMethod]
            public void IdentificationResultFromUnknownDigipostAddress()
            {
                //Arrange
                IdentificationResultDataTransferObject source = new IdentificationResultDataTransferObject();
                source.IdentificationResultCode = IdentificationResultCode.Unidentified;
                source.IdentificationValue = "NotFound";
                source.IdentificationResultType = IdentificationResultType.UnidentifiedReason;

                IdentificationResult expected = new IdentificationResult(IdentificationResultType.UnidentifiedReason, "NotFound");

                //Act
                var actual = DataTransferObjectConverter.FromDataTransferObject(source);

                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expected, actual, out differences);
                Assert.AreEqual(0, differences.Count());

                Assert.AreEqual(source.IdentificationResultType, actual.ResultType);
                Assert.AreEqual(null, actual.Data);
                Assert.AreEqual(source.IdentificationValue.ToString(),actual.Error.ToString());
            }

            [TestMethod]
            public void Document()
            {
                //Arrange
                DocumentDataTransferObject source = new DocumentDataTransferObject("TestSubject", "txt", new byte[2], AuthenticationLevel.Password, SensitivityLevel.Sensitive, new SmsNotificationDataTransferObject(3));
                
                IDocument expected = new Document("TestSubject", "txt", new byte[2], AuthenticationLevel.Password, SensitivityLevel.Sensitive, new SmsNotification(3));
                expected.Guid = source.Guid;
                
                //Act
                var actual = DataTransferObjectConverter.FromDataTransferObject(source);

                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expected, actual, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void Message()
            {
                //Arrange
                var deliverytime = DateTime.Now.AddDays(3);

                MessageDataTransferObject sourceDto = new MessageDataTransferObject(
                    new RecipientDataTransferObject(
                        IdentificationChoiceType.DigipostAddress,
                        "Ola.Nordmann#34JJ"
                        ),
                    new DocumentDataTransferObject("TestSubject", "txt", new byte[3]), "SenderId")
                {
                    Attachments = new List<DocumentDataTransferObject>
                    {
                        new DocumentDataTransferObject("TestSubject attachment", "txt", new byte[3])
                        {
                            Guid = "attachmentGuid"
                        }
                    },
                    DeliveryTime = deliverytime
                };
                
                Message expected = new Message(
                    new RecipientById(
                        IdentificationType.DigipostAddress,
                        "Ola.Nordmann#34JJ"
                        ),
                    new Document("TestSubject", "txt", new byte[3]))
                {
                    SenderId = "SenderId",
                    Attachments = new List<IDocument>()
                    {
                        new Document("TestSubject attachment", "txt",  new byte[3])
                        {
                            Guid = "attachmentGuid"
                        }
                    },
                    DeliveryTime = deliverytime,
                    PrimaryDocument = { Guid = sourceDto.PrimaryDocumentDataTransferObject.Guid }
                };

                //Act
                var actual = DataTransferObjectConverter.ToDataTransferObject(expected);

                //Assert

                IEnumerable<IDifference> differences;
                _comparator.AreEqual(sourceDto, actual, out differences);
                Assert.AreEqual(0, differences.Count());
            }

            [TestMethod]
            public void SmsNotification()
            {   
                //Arrange
                var atTimes = new List<DateTime> { DateTime.Now, DateTime.Now.AddHours(3) };
                var afterHours = new List<int>() { 4, 5 };

                var sourceDto = new SmsNotificationDataTransferObject();
                sourceDto.NotifyAfterHours.AddRange(afterHours);
                sourceDto.NotifyAtTimes.AddRange(atTimes.Select(a => new ListedTimeDataTransferObject(a)));

                var expected = new SmsNotification();
                expected.NotifyAfterHours.AddRange(afterHours);
                expected.NotifyAtTimes.AddRange(atTimes);

                //Act
                var actual = DataTransferObjectConverter.FromDataTransferObject(sourceDto);

                //Assert
                IEnumerable<IDifference> differences;
                _comparator.AreEqual(expected, actual, out differences);
                Assert.AreEqual(0, differences.Count());
            }
        }
    }
}
