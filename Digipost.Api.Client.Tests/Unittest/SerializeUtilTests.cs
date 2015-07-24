﻿using System;
using System.Globalization;
using Digipost.Api.Client.Domain;
using Digipost.Api.Client.Domain.Enums;
using Digipost.Api.Client.Domain.Print;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digipost.Api.Client.Tests.Unittest
{
    [TestClass]
    public class SerializeUtilTests
    {

        [TestClass]
        public class DeserializeMethod
        {
            [TestMethod]
            public void ReturnsProperDeserializedMessageWithInvoice()
            {
                //Arrange
                var messageBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><message xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><recipient><name-and-address><fullname>Ola Nordmann</fullname><addressline1>Colletts gate 68</addressline1><postalcode>0460</postalcode><city>Oslo</city></name-and-address><print-details><recipient><name>Ola Nordmann</name><norwegian-address><addressline1>Collettsgate 68</addressline1><addressline2>Leil h401</addressline2><addressline3>dør 2</addressline3><zip-code>0460</zip-code><city>Oslo</city></norwegian-address></recipient><return-address><name>Ola Digipost</name><foreign-address><addressline1>svenskegatan 1</addressline1><addressline2> leil h101</addressline2><addressline3>pb 12</addressline3><addressline4>skuff 3</addressline4><country>SE</country></foreign-address></return-address><post-type>B</post-type><color>MONOCHROME</color></print-details></recipient><primary-document><uuid>1222222</uuid><subject>Subject</subject><file-type>txt</file-type><sms-notification><after-hours>2</after-hours></sms-notification><authentication-level>TWO_FACTOR</authentication-level><sensitivity-level>SENSITIVE</sensitivity-level></primary-document><attachment xsi:type=""invoice""><uuid>123456</uuid><subject>Subject</subject><file-type>txt</file-type><sms-notification><after-hours>2</after-hours></sms-notification><authentication-level>TWO_FACTOR</authentication-level><sensitivity-level>SENSITIVE</sensitivity-level><kid>123123123123</kid><amount>100</amount><account>18941362738</account><due-date>2018-01-01</due-date></attachment></message>";
                var printDetails = new PrintDetails(new PrintRecipient("Ola Nordmann", new NorwegianAddress("0460", "Oslo", "Collettsgate 68", "Leil h401", "dør 2")), new PrintReturnAddress("Ola Digipost", new ForeignAddress(CountryIdentifier.Country, "SE", "svenskegatan 1", " leil h101", "pb 12", "skuff 3")));
                var recipient =
                    new Recipient(
                        new RecipientByNameAndAddress("Ola Nordmann", "0460", "Oslo", "Colletts gate 68"), printDetails);
                var document = new Document("Subject", "txt", ByteUtility.GetBytes("test"), AuthenticationLevel.TwoFactor,
                    SensitivityLevel.Sensitive) { Guid = "1222222", SmsNotification = new SmsNotification(2) };

                var attachment =  new Invoice("Subject", "txt", ByteUtility.GetBytes("test"),100,"18941362738",DateTime.Parse("2018-01-01"),"123123123123", AuthenticationLevel.TwoFactor,
                    SensitivityLevel.Sensitive) { Guid = "123456", SmsNotification = new SmsNotification(2) };


                var messageTemplate = new Message(recipient, document);
                messageTemplate.Attachments.Add(attachment);

                //Act
                var deserializedMessageBlueprint = SerializeUtil.Deserialize<Message>(messageBlueprint);
                document.ContentBytes = null;   //Bytes are not included as a part of XML (XmlIgnore)
                attachment.ContentBytes = null; //Bytes are not included as a part of XML (XmlIgnore)
                
                //Assert
                Comparator.LookLikeEachOther(messageTemplate, deserializedMessageBlueprint);
            }

            [TestMethod]
            public void ReturnsProperDeserializedMessageWithSenderOrganization()
            {
                //Arrange
                var messageBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><message xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><sender-organization><organization-id>12345678911</organization-id><part-id>ViktigperAvdelingen</part-id></sender-organization><recipient><name-and-address><fullname>Ola Nordmann</fullname><addressline1>Colletts gate 68</addressline1><postalcode>0460</postalcode><city>Oslo</city></name-and-address><print-details><recipient><name>Ola Nordmann</name><norwegian-address><addressline1>Collettsgate 68</addressline1><addressline2>Leil h401</addressline2><addressline3>dør 2</addressline3><zip-code>0460</zip-code><city>Oslo</city></norwegian-address></recipient><return-address><name>Ola Digipost</name><foreign-address><addressline1>svenskegatan 1</addressline1><addressline2> leil h101</addressline2><addressline3>pb 12</addressline3><addressline4>skuff 3</addressline4><country>SE</country></foreign-address></return-address><post-type>B</post-type><color>MONOCHROME</color></print-details></recipient><primary-document><uuid>1222222</uuid><subject>Subject</subject><file-type>txt</file-type><sms-notification><after-hours>2</after-hours></sms-notification><authentication-level>TWO_FACTOR</authentication-level><sensitivity-level>SENSITIVE</sensitivity-level></primary-document></message>";
                var printDetails = new PrintDetails(new PrintRecipient("Ola Nordmann", new NorwegianAddress("0460", "Oslo", "Collettsgate 68", "Leil h401", "dør 2")), new PrintReturnAddress("Ola Digipost", new ForeignAddress(CountryIdentifier.Country, "SE", "svenskegatan 1", " leil h101", "pb 12", "skuff 3")));
                var recipient =
                    new Recipient(
                        new RecipientByNameAndAddress("Ola Nordmann", "0460", "Oslo", "Colletts gate 68"), printDetails);
                var document = new Document("Subject", "txt", ByteUtility.GetBytes("test"), AuthenticationLevel.TwoFactor,
                    SensitivityLevel.Sensitive) { Guid = "1222222", SmsNotification = new SmsNotification(2) };

                var messageTemplate = new Message(recipient, document, new SenderOrganization("12345678911", "ViktigperAvdelingen"));

                //Act
                var deserializedMessageBlueprint = SerializeUtil.Deserialize<Message>(messageBlueprint);
                document.ContentBytes = null;   //Bytes are not included as a part of XML (XmlIgnore)

                //Assert
                Comparator.LookLikeEachOther(messageTemplate, deserializedMessageBlueprint);
                

            }

            [TestMethod]
            public void ReturnsProperDeserializedMessageWithSenderOrganizationId()
            {
                //Arrange
                var messageBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><message xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><sender-id>1237732</sender-id><recipient><name-and-address><fullname>Ola Nordmann</fullname><addressline1>Colletts gate 68</addressline1><postalcode>0460</postalcode><city>Oslo</city></name-and-address><print-details><recipient><name>Ola Nordmann</name><norwegian-address><addressline1>Collettsgate 68</addressline1><addressline2>Leil h401</addressline2><addressline3>dør 2</addressline3><zip-code>0460</zip-code><city>Oslo</city></norwegian-address></recipient><return-address><name>Ola Digipost</name><foreign-address><addressline1>svenskegatan 1</addressline1><addressline2> leil h101</addressline2><addressline3>pb 12</addressline3><addressline4>skuff 3</addressline4><country>SE</country></foreign-address></return-address><post-type>B</post-type><color>MONOCHROME</color></print-details></recipient><primary-document><uuid>1222222</uuid><subject>Subject</subject><file-type>txt</file-type><sms-notification><after-hours>2</after-hours></sms-notification><authentication-level>TWO_FACTOR</authentication-level><sensitivity-level>SENSITIVE</sensitivity-level></primary-document></message>";
                var printDetails = new PrintDetails(new PrintRecipient("Ola Nordmann", new NorwegianAddress("0460", "Oslo", "Collettsgate 68", "Leil h401", "dør 2")), new PrintReturnAddress("Ola Digipost", new ForeignAddress(CountryIdentifier.Country, "SE", "svenskegatan 1", " leil h101", "pb 12", "skuff 3")));
                var recipient =
                    new Recipient(
                        new RecipientByNameAndAddress("Ola Nordmann", "0460", "Oslo", "Colletts gate 68"), printDetails);
                var document = new Document("Subject", "txt", ByteUtility.GetBytes("test"), AuthenticationLevel.TwoFactor,
                    SensitivityLevel.Sensitive) { Guid = "1222222", SmsNotification = new SmsNotification(2) };

                var messageTemplate = new Message(recipient, document, 1237732);
                
                //Act
                var deserializedMessageBlueprint = SerializeUtil.Deserialize<Message>(messageBlueprint);
                document.ContentBytes = null;   //Bytes are not included as a part of XML (XmlIgnore)

                //Assert
                Comparator.LookLikeEachOther(messageTemplate, deserializedMessageBlueprint);
            }

            [TestMethod]
            public void ReturnProperDeserializedDocument()
            {
                //Arrange
                const string documentBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><document xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><uuid>123456</uuid><subject>Subject</subject><file-type>txt</file-type><sms-notification><after-hours>2</after-hours></sms-notification><authentication-level>TWO_FACTOR</authentication-level><sensitivity-level>SENSITIVE</sensitivity-level></document>";
                var document = new Document("Subject", "txt", ByteUtility.GetBytes("test"), AuthenticationLevel.TwoFactor,
                    SensitivityLevel.Sensitive) { Guid = "123456", SmsNotification = new SmsNotification(2) };

                //Act
                var deserializedDocumentBlueprint = SerializeUtil.Deserialize<Document>(documentBlueprint);
                document.ContentBytes = null;    //Bytes are not included as a part of XML (XmlIgnore)

                //Assert
                Comparator.LookLikeEachOther(document, deserializedDocumentBlueprint);
            }

            [TestMethod]
            public void ReturnProperDerializedRecipient()
            {
                //Arrange
                const string recipientBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><message-recipient xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><name-and-address><fullname>Ola Nordmann</fullname><addressline1>Colletts gate 68</addressline1><postalcode>0460</postalcode><city>Oslo</city></name-and-address><print-details><recipient><name>Ola Nordmann</name><norwegian-address><addressline1>Collettsgate 68</addressline1><addressline2>Leil h401</addressline2><addressline3>dør 2</addressline3><zip-code>0460</zip-code><city>Oslo</city></norwegian-address></recipient><return-address><name>Ola Digipost</name><foreign-address><addressline1>svenskegatan 1</addressline1><addressline2> leil h101</addressline2><addressline3>pb 12</addressline3><addressline4>skuff 3</addressline4><country>SE</country></foreign-address></return-address><post-type>B</post-type><color>MONOCHROME</color></print-details></message-recipient>";
                var printDetails = new PrintDetails(new PrintRecipient("Ola Nordmann", new NorwegianAddress("0460", "Oslo", "Collettsgate 68", "Leil h401", "dør 2")), new PrintReturnAddress("Ola Digipost", new ForeignAddress(CountryIdentifier.Country, "SE", "svenskegatan 1", " leil h101", "pb 12", "skuff 3")));
                var recipient =
                    new Recipient(
                        new RecipientByNameAndAddress("Ola Nordmann", "0460", "Oslo", "Colletts gate 68"), printDetails);

                //Act
                var deserializedRecipientBlueprint = SerializeUtil.Deserialize<Recipient>(recipientBlueprint);

                //Assert
                Comparator.LookLikeEachOther(recipient, deserializedRecipientBlueprint);

            }

            [TestMethod]
            public void ReturnsProperDeserializedIdentificationByNameAndAddress()
            {
                //Arrange
                const string identificationBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><identification xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><name-and-address><fullname>Ola Nordmann</fullname><addressline1>Postgirobygget 16</addressline1><postalcode>0001</postalcode><city>Oslo</city></name-and-address></identification>";
                var identification = new Identification(IdentificationChoice.DigipostAddress, new RecipientByNameAndAddress("Ola Nordmann", "0001", "Oslo", "Postgirobygget 16"));

                //Act
                var deserializedIdentificationBlueprint = SerializeUtil.Deserialize<Identification>(identificationBlueprint);

                //Assert
                Comparator.LookLikeEachOther(identification, deserializedIdentificationBlueprint);
            }
            
            [TestMethod]
            public void ReturnsProperDeserializedIdentificationByDigipostAddress()
            {
                //Arrange
                const string identificationBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><identification xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><digipost-address>ola.nordmann#123abc</digipost-address></identification>";
                var identification = new Identification(IdentificationChoice.DigipostAddress, "ola.nordmann#123abc");
                
                //Act
                var deserializedIdentificationBlueprint = SerializeUtil.Deserialize<Identification>(identificationBlueprint);

                //Assert
                Comparator.LookLikeEachOther(identification, deserializedIdentificationBlueprint);
            }

            [TestMethod]
            public void ReturnsProperDeserializedInvoice()
            {
                //Arrange
                const string invoiceBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><invoice xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><uuid>123456</uuid><subject>Subject</subject><file-type>txt</file-type><sms-notification><after-hours>2</after-hours></sms-notification><authentication-level>TWO_FACTOR</authentication-level><sensitivity-level>SENSITIVE</sensitivity-level><kid>123123123123</kid><amount>100</amount><account>18941362738</account><due-date>2018-01-01</due-date></invoice>";
                var invoice = new Invoice("Subject", "txt", ByteUtility.GetBytes("test"), 100, "18941362738", DateTime.Parse("2018-01-01"), "123123123123", AuthenticationLevel.TwoFactor,
                    SensitivityLevel.Sensitive) { Guid = "123456", SmsNotification = new SmsNotification(2) };

                //Act
                var deserializedInvoice = SerializeUtil.Deserialize<Invoice>(invoiceBlueprint);

                //Assert
                Comparator.LookLikeEachOther(invoice, deserializedInvoice);
            }
        }

        [TestClass]
        public class SerializeMethod
        {
            [TestMethod]
            public void ReturnProperSerializedMessageWithInvoice()
            {
                //Arrange
                const string messageBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><message xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><recipient><name-and-address><fullname>Ola Nordmann</fullname><addressline1>Colletts gate 68</addressline1><postalcode>0460</postalcode><city>Oslo</city></name-and-address><print-details><recipient><name>Ola Nordmann</name><norwegian-address><addressline1>Collettsgate 68</addressline1><addressline2>Leil h401</addressline2><addressline3>dør 2</addressline3><zip-code>0460</zip-code><city>Oslo</city></norwegian-address></recipient><return-address><name>Ola Digipost</name><foreign-address><addressline1>svenskegatan 1</addressline1><addressline2> leil h101</addressline2><addressline3>pb 12</addressline3><addressline4>skuff 3</addressline4><country>SE</country></foreign-address></return-address><post-type>B</post-type><color>MONOCHROME</color></print-details></recipient><primary-document><uuid>03fdf738-5c7e-420d-91e2-2b95e025d635</uuid><subject>Subject</subject><file-type>txt</file-type><sms-notification><after-hours>2</after-hours></sms-notification><authentication-level>TWO_FACTOR</authentication-level><sensitivity-level>SENSITIVE</sensitivity-level></primary-document><attachment><uuid>c75d8cb9-bd47-4ae7-9bd3-92105d12f49a</uuid><subject>attachment</subject><file-type>txt</file-type><authentication-level>TWO_FACTOR</authentication-level><sensitivity-level>SENSITIVE</sensitivity-level></attachment></message>";
                var printDetails = new PrintDetails(new PrintRecipient("Ola Nordmann", new NorwegianAddress("0460", "Oslo", "Collettsgate 68", "Leil h401", "dør 2")), new PrintReturnAddress("Ola Digipost", new ForeignAddress(CountryIdentifier.Country, "SE", "svenskegatan 1", " leil h101", "pb 12", "skuff 3")));
                var recipient =
                    new Recipient(
                        new RecipientByNameAndAddress("Ola Nordmann", "0460", "Oslo", "Colletts gate 68"), printDetails);
                var document = new Document("Subject", "txt", ByteUtility.GetBytes("test"), AuthenticationLevel.TwoFactor,
                    SensitivityLevel.Sensitive) { SmsNotification = new SmsNotification(2), Guid = "03fdf738-5c7e-420d-91e2-2b95e025d635" };
                var attachment = new Document("attachment", "txt", ByteUtility.GetBytes("test"), AuthenticationLevel.TwoFactor,
                    SensitivityLevel.Sensitive) { Guid = "c75d8cb9-bd47-4ae7-9bd3-92105d12f49a" };
                var message = new Message(recipient, document);
                message.Attachments.Add(attachment);

                //Act
                var serialized = SerializeUtil.Serialize(message);
                
                //Assert
                Assert.IsNotNull(serialized);
                Assert.AreEqual(messageBlueprint, serialized);

            }

            [TestMethod]
            public void ReturnsProperSerializedMessageWithSenderOrganization()
            {
                //Arrange
                var messageBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><message xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><sender-organization><organization-id>12345678911</organization-id><part-id>ViktigperAvdelingen</part-id></sender-organization><recipient><name-and-address><fullname>Ola Nordmann</fullname><addressline1>Colletts gate 68</addressline1><postalcode>0460</postalcode><city>Oslo</city></name-and-address><print-details><recipient><name>Ola Nordmann</name><norwegian-address><addressline1>Collettsgate 68</addressline1><addressline2>Leil h401</addressline2><addressline3>dør 2</addressline3><zip-code>0460</zip-code><city>Oslo</city></norwegian-address></recipient><return-address><name>Ola Digipost</name><foreign-address><addressline1>svenskegatan 1</addressline1><addressline2> leil h101</addressline2><addressline3>pb 12</addressline3><addressline4>skuff 3</addressline4><country>SE</country></foreign-address></return-address><post-type>B</post-type><color>MONOCHROME</color></print-details></recipient><primary-document><uuid>1222222</uuid><subject>Subject</subject><file-type>txt</file-type><sms-notification><after-hours>2</after-hours></sms-notification><authentication-level>TWO_FACTOR</authentication-level><sensitivity-level>SENSITIVE</sensitivity-level></primary-document></message>";
                var printDetails = new PrintDetails(new PrintRecipient("Ola Nordmann", new NorwegianAddress("0460", "Oslo", "Collettsgate 68", "Leil h401", "dør 2")), new PrintReturnAddress("Ola Digipost", new ForeignAddress(CountryIdentifier.Country, "SE", "svenskegatan 1", " leil h101", "pb 12", "skuff 3")));
                var recipient =
                    new Recipient(
                        new RecipientByNameAndAddress("Ola Nordmann", "0460", "Oslo", "Colletts gate 68"), printDetails);
                var document = new Document("Subject", "txt", ByteUtility.GetBytes("test"), AuthenticationLevel.TwoFactor,
                    SensitivityLevel.Sensitive) { Guid = "1222222", SmsNotification = new SmsNotification(2) };

                var messageTemplate = new Message(recipient, document, new SenderOrganization("12345678911", "ViktigperAvdelingen"));

                //Act
                var serializedMessage = SerializeUtil.Serialize(messageTemplate);

                //Assert
                Assert.IsNotNull(serializedMessage);
                Assert.AreEqual(messageBlueprint, serializedMessage);


            }

            [TestMethod]
            public void ReturnsProperSerializedMessageWithSenderOrganizationId()
            {
                //Arrange
                var messageBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><message xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><sender-id>1237732</sender-id><recipient><name-and-address><fullname>Ola Nordmann</fullname><addressline1>Colletts gate 68</addressline1><postalcode>0460</postalcode><city>Oslo</city></name-and-address><print-details><recipient><name>Ola Nordmann</name><norwegian-address><addressline1>Collettsgate 68</addressline1><addressline2>Leil h401</addressline2><addressline3>dør 2</addressline3><zip-code>0460</zip-code><city>Oslo</city></norwegian-address></recipient><return-address><name>Ola Digipost</name><foreign-address><addressline1>svenskegatan 1</addressline1><addressline2> leil h101</addressline2><addressline3>pb 12</addressline3><addressline4>skuff 3</addressline4><country>SE</country></foreign-address></return-address><post-type>B</post-type><color>MONOCHROME</color></print-details></recipient><primary-document><uuid>1222222</uuid><subject>Subject</subject><file-type>txt</file-type><sms-notification><after-hours>2</after-hours></sms-notification><authentication-level>TWO_FACTOR</authentication-level><sensitivity-level>SENSITIVE</sensitivity-level></primary-document></message>";
                var printDetails = new PrintDetails(new PrintRecipient("Ola Nordmann", new NorwegianAddress("0460", "Oslo", "Collettsgate 68", "Leil h401", "dør 2")), new PrintReturnAddress("Ola Digipost", new ForeignAddress(CountryIdentifier.Country, "SE", "svenskegatan 1", " leil h101", "pb 12", "skuff 3")));
                var recipient =
                    new Recipient(
                        new RecipientByNameAndAddress("Ola Nordmann", "0460", "Oslo", "Colletts gate 68"), printDetails);
                var document = new Document("Subject", "txt", ByteUtility.GetBytes("test"), AuthenticationLevel.TwoFactor,
                    SensitivityLevel.Sensitive) { Guid = "1222222", SmsNotification = new SmsNotification(2) };

                var messageTemplate = new Message(recipient, document, 1237732);

                //Act
                var serializedMessage = SerializeUtil.Serialize(messageTemplate);
          
                //Assert
                Assert.IsNotNull(serializedMessage);
                Assert.AreEqual(messageBlueprint, serializedMessage);
            }


            [TestMethod]
            public void ReturnProperSerializedDocument()
            {
                //Arrange
                const string documentBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><document xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><uuid>123456</uuid><subject>Subject</subject><file-type>txt</file-type><sms-notification><after-hours>2</after-hours></sms-notification><authentication-level>TWO_FACTOR</authentication-level><sensitivity-level>SENSITIVE</sensitivity-level></document>";
                var document = new Document("Subject", "txt", ByteUtility.GetBytes("test"), AuthenticationLevel.TwoFactor,
                    SensitivityLevel.Sensitive) { Guid = "123456", SmsNotification = new SmsNotification(2) };

                //Act
                var serialized = SerializeUtil.Serialize(document);

                //Assert
                Assert.IsNotNull(serialized);
                Assert.AreEqual(documentBlueprint, serialized);
            }

            [TestMethod]
            public void ReturnProperSerializedRecipient()
            {
                //Arrange
                const string recipientBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><message-recipient xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><name-and-address><fullname>Ola Nordmann</fullname><addressline1>Colletts gate 68</addressline1><postalcode>0460</postalcode><city>Oslo</city></name-and-address><print-details><recipient><name>Ola Nordmann</name><norwegian-address><addressline1>Collettsgate 68</addressline1><addressline2>Leil h401</addressline2><addressline3>dør 2</addressline3><zip-code>0460</zip-code><city>Oslo</city></norwegian-address></recipient><return-address><name>Ola Digipost</name><foreign-address><addressline1>svenskegatan 1</addressline1><addressline2> leil h101</addressline2><addressline3>pb 12</addressline3><addressline4>skuff 3</addressline4><country>SE</country></foreign-address></return-address><post-type>B</post-type><color>MONOCHROME</color></print-details></message-recipient>";
                var printDetails = new PrintDetails(new PrintRecipient("Ola Nordmann", new NorwegianAddress("0460", "Oslo", "Collettsgate 68", "Leil h401", "dør 2")), new PrintReturnAddress("Ola Digipost", new ForeignAddress(CountryIdentifier.Country, "SE", "svenskegatan 1", " leil h101", "pb 12", "skuff 3")));
                var recipient =
                    new Recipient(
                        new RecipientByNameAndAddress("Ola Nordmann", "0460", "Oslo", "Colletts gate 68"), printDetails);

                //Act
                var serialized = SerializeUtil.Serialize(recipient);
                
                //Assert
                Assert.IsNotNull(serialized);
                Assert.AreEqual(recipientBlueprint, serialized);

            }

            [TestMethod]
            public void ReturnsProperSerializedIdentificationByNameAndAddress()
            {
                //Arrange
                const string identificationBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><identification xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><name-and-address><fullname>Ola Nordmann</fullname><addressline1>Postgirobygget 16</addressline1><postalcode>0001</postalcode><city>Oslo</city></name-and-address></identification>";
                var identification = new Identification(IdentificationChoice.DigipostAddress, new RecipientByNameAndAddress("Ola Nordmann", "0001", "Oslo", "Postgirobygget 16"));

                //Act
                var deserializedIdentificationBlueprint = SerializeUtil.Deserialize<Identification>(identificationBlueprint);

                //Assert
                Comparator.LookLikeEachOther(identification, deserializedIdentificationBlueprint);
            }

            [TestMethod]
            public void ReturnsProperSerializedIdentificationByDigipostAddress()
            {
                //Arrange
                const string identificationBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><identification xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><digipost-address>ola.nordmann#123abc</digipost-address></identification>";
                var identification = new Identification(IdentificationChoice.DigipostAddress, "ola.nordmann#123abc");

                //Act
                var serializedIdentification = SerializeUtil.Serialize(identification);

                //Assert
                Assert.IsNotNull(serializedIdentification);
                Assert.AreEqual(identificationBlueprint, serializedIdentification);
            }

            [TestMethod]
            public void ReturnProperSerializedInvoice()
            {
                //Arrange
                const string invoiceBlueprint = @"<?xml version=""1.0"" encoding=""utf-8""?><invoice xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://api.digipost.no/schema/v6""><uuid>123456</uuid><subject>Subject</subject><file-type>txt</file-type><sms-notification><after-hours>2</after-hours></sms-notification><authentication-level>TWO_FACTOR</authentication-level><sensitivity-level>SENSITIVE</sensitivity-level><kid>123123123123</kid><amount>100</amount><account>18941362738</account><due-date>2018-01-01</due-date></invoice>";
                var invoice = new Invoice("Subject", "txt", ByteUtility.GetBytes("test"),100,"18941362738",DateTime.Parse("2018-01-01"),"123123123123", AuthenticationLevel.TwoFactor,
                    SensitivityLevel.Sensitive) { Guid = "123456", SmsNotification = new SmsNotification(2) };

                //Act
                var serializedIdentification = SerializeUtil.Serialize(invoice);

                //Assert
                Assert.IsNotNull(serializedIdentification);
                Assert.AreEqual(invoiceBlueprint, serializedIdentification);
            }
        }

    }
}
