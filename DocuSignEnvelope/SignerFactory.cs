using DocuSign.eSign.Model;

namespace DocuSignEnvelope
{
    public class SignerFactory
    {
        private static int count = 1;

        public static Signer Create(string email, string name) 
            => new Signer(Email: email, Name: name, RecipientId: count.ToString(), ClientUserId: $"test{count++}");
    }
}