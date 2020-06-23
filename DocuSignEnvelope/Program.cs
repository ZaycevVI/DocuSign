using System;
using System.Collections.Generic;
using System.IO;
using DocuSign.eSign.Api;
using DocuSign.eSign.Model;

namespace DocuSignEnvelope
{
    class Program
    {
        private static readonly EnvelopesApi EnvelopesApi = new EnvelopesApi();
        private static readonly AuthProvider AuthProvider = new AuthProvider();
        private static readonly Signer Signer1 = SignerFactory.Create("goldlike@mail.ru", "Vasiliy Pupkin");
        private static readonly Signer Signer2 = SignerFactory.Create("zaycevvi@mail.ru", "Uladzimir Zaitsau");
        private const string CallbackUrl = "http://8de2706b.ngrok.io";
        
        private static readonly Recipients Recipients = new Recipients
        {
            Signers = new List<Signer> 
            {
                Signer1, 
                //Signer2
            }
        };

        static void Main(string[] args)
        {
            var envelopeId = CreateEnvelope();

            Console.WriteLine($"Envelope with EnvelopeId={envelopeId} was successfully created.");

            // Step 3. create the recipient view, the Signing Ceremony
            var viewRequest1 = MakeRecipientViewRequest(Signer1);
            //var viewRequest2 = MakeRecipientViewRequest(Signer2);
            
            var results1 = AuthProvider.WithDocuSignAuth(EnvelopesApi, 
                (api, accountId) => api.CreateRecipientView(accountId, envelopeId, viewRequest1));
            //var results2 = AuthProvider.WithDocuSignAuth(EnvelopesApi, 
            //    (api, accountId) => api.CreateRecipientView(accountId, envelopeId, viewRequest2));

            Console.WriteLine("===========================================");
            Console.WriteLine($"Url:{results1.Url}");
            Console.WriteLine("===========================================");
            //Console.WriteLine($"Url:{results2.Url}");
            //Console.WriteLine("===========================================");

            Console.ReadLine();

            AuthProvider.WithDocuSignAuth(EnvelopesApi, (api, accountId) => SaveDocuments(api, accountId, envelopeId));
        }

        private static string CreateEnvelope()
        {
            var envelope = MakeEnvelopeDefinition();

            var envelopeSummary = AuthProvider.WithDocuSignAuth(EnvelopesApi, (api, accId) => api.CreateEnvelope(accId, envelope));

            return envelopeSummary.EnvelopeId;
        }

        private static EnvelopeDefinition MakeEnvelopeDefinition(string signerEmail = null, string signerName = null)
        {
            var envelopeDefinition = new EnvelopeDefinition { EmailSubject = "Please sign this document" };

            var documents = DocumentBuilder
                .Builder
                .WithDocument("D:\\Docu.docx")
                .WithDocument("D:\\Docu2.docx")
                .WithDocument("D:\\logo.png")
                .Build();

            envelopeDefinition.Documents = documents;

            Signer1.Tabs = CreateTabs(Signer1);
            //Signer2.Tabs = CreateTabs(Signer2);

            envelopeDefinition.Recipients = Recipients;

            // Request that the envelope be sent by setting |status| to "sent".
            // To request that the envelope be created as a draft, set to "created"
            envelopeDefinition.Status = "sent";

            return envelopeDefinition;
        }


        private static Tabs CreateTabs(Signer signer)
        {
            SignHere signHere = new SignHere
            {
                AnchorString = "eSignSignHere",
                AnchorIgnoreIfNotPresent = "true",
                RecipientId = signer.RecipientId
            };

            FullName fullName = new FullName
            {
                AnchorString = "eSignFullName",
                AnchorIgnoreIfNotPresent = "true",
                RecipientId = signer.RecipientId
            };

            return new Tabs
            {
                SignHereTabs = new List<SignHere> { signHere },
                FullNameTabs = new List<FullName> { fullName }
            };
        }

        private static RecipientViewRequest MakeRecipientViewRequest(Signer signer)
        {
            return new RecipientViewRequest
            {
                ReturnUrl = CallbackUrl,
                AuthenticationMethod = "none",
                Email = signer.Email,
                UserName = signer.Name,
                ClientUserId = signer.ClientUserId
            };
        }

        private static bool SaveDocuments(EnvelopesApi envelopesApi, string accountId, string envelopeId)
        {
            var docList = envelopesApi.ListDocuments(accountId, envelopeId);

            for (var i = 0; i < docList.EnvelopeDocuments.Count; i++)
            {
                // GetDocument() API call returns a MemoryStream
                var docStream = (MemoryStream)envelopesApi.GetDocument(accountId, docList.EnvelopeId, docList.EnvelopeDocuments[i].DocumentId);
                // let's save the document to local file system
                var filePath = "D:\\signed\\" + Path.GetRandomFileName() + ".pdf";
                var fs = new FileStream(filePath, FileMode.Create);
                docStream.Seek(0, SeekOrigin.Begin);
                docStream.CopyTo(fs);
                fs.Close();
                Console.WriteLine("Envelope Document {0} has been downloaded to:  {1}", i, filePath);
            }

            return true;
        }
        
    }
}
