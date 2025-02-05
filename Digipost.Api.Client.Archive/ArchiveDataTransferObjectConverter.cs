using System;
using System.Linq;
using Digipost.Api.Client.Common;
using V8;

namespace Digipost.Api.Client.Archive
{
    internal static class ArchiveDataTransferObjectConverter
    {
        public static V8.Archive ToDataTransferObject(Archive a)
        {
            var dto = new V8.Archive()
            {
                Name = a.Name,
                Sender_Id = a.Sender.Id,
                Sender_IdSpecified = true
            };

            foreach (var ad in a.ArchiveDocuments.Select(ToDataTransferObject))
            {
                dto.Documents.Add(ad);
            }

            return dto;
        }

        public static Archive_Document ToDataTransferObject(ArchiveDocument ad)
        {
            var dto = new Archive_Document()
            {
                Uuid = ad.Id.ToString(),
                File_Name = ad.FileName,
                File_Type = ad.FileType,
                Content_Type = ad.ContentType,
                Referenceid = ad.ReferenceId,
                Archived_Time = ad.ArchiveTime,
                Deletion_Time = ad.DeletionTime
            };

            foreach (var attribute in ad.Attributes)
            {
                dto.Attributes.Add(new Archive_Document_Attribute(){Key = attribute.Key, Value = attribute.Value});
            }

            if (ad.ContentHash != null)
            {
                dto.Content_Hash = new Content_Hash()
                {
                    Value = ad.ContentHash.Value,
                    Hash_Algorithm = ad.ContentHash.HashAlgoritm
                };
            }

            return dto;
        }

        public static ArchiveDocument FromDataTransferObject(Archive_Document ad)
        {
            return new ArchiveDocument(
                new Guid(ad.Uuid),
                ad.File_Name,
                ad.File_Type,
                ad.Content_Type
            )
            {
                ReferenceId = ad.Referenceid,
                ContentHash = ad.Content_Hash == null ? null : new ContentHash {HashAlgoritm = ad.Content_Hash.Hash_Algorithm, Value = ad.Content_Hash.Value},
                ArchiveTime = ad.Archived_Time,
                DeletionTime = ad.Deletion_Time,
                Attributes = ad.Attributes.ToDictionary(ada => ada.Key, ada => ada.Value),
                Links = DataTransferObjectConverter.FromDataTransferObject(ad.Link)
            };
        }

        public static Archive FromDataTransferObject(V8.Archive a)
        {
            return new Archive(new Sender(a.Sender_Id), a.Name)
            {
                ArchiveDocuments = a.Documents.Select(FromDataTransferObject).ToList(),
                Links =DataTransferObjectConverter.FromDataTransferObject(a.Link)
            };
        }

        public static ArchiveDocumentContent FromDataTransferObject(Archive_Document_Content result)
        {
            return new ArchiveDocumentContent(result.Content_Type, new Uri(result.Uri, UriKind.Absolute));
        }
    }
}
