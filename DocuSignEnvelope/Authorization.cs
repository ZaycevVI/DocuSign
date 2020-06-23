using System;
using System.Linq;
using System.Security.Authentication;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Newtonsoft.Json;

namespace DocuSignEnvelope
{
    public class AuthProvider
    {
        private readonly DocuSignApiConfiguration configuration = new DocuSignApiConfiguration();

        private string InitDocuSignApi(IApiAccessor apiAccessor)
        {
            var client = new ApiClient(configuration.AuthConfiguration.BaseUrl);
            Configuration.Default.ApiClient = client;

            var authHeader = new AuthHeader
            {
                UserName = configuration.AuthConfiguration.Username,
                Password = configuration.AuthConfiguration.Password,
                IntegratorKey = configuration.AuthConfiguration.IntegratorKey,
            };

            DocuSign.eSign.Client.Configuration.Default.AddDefaultHeader("X-DocuSign-Authentication",
                JsonConvert.SerializeObject(authHeader));

            AuthenticationApi authApi = new AuthenticationApi();
            LoginInformation loginInfo = authApi.Login();
            var loginAccount = loginInfo.LoginAccounts.First();

            if (loginAccount == null)
            {
                throw new AuthenticationException("DocuSign API is not authenticated.");
            }

            string[] separatingStrings = { "/v2" };
            var urlParts = loginAccount.BaseUrl.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);
            var baseUrl = urlParts.Length > 0 && !string.IsNullOrWhiteSpace(urlParts[0])
                ? urlParts[0]
                : "https://www.demo.docusign.net/restapi";
            var apiClient = new ApiClient(baseUrl);
            apiAccessor.Configuration.ApiClient = apiClient;

            return loginAccount.AccountId;
        }

        public V WithDocuSignAuth<T, V>(T apiAccessor, Func<T, string, V> apiCall)
            where T : IApiAccessor
        {
            var accountId = InitDocuSignApi(apiAccessor);

            return apiCall(apiAccessor, accountId);
        }
    }


    public class AuthHeader
    {
        [JsonProperty("Username")]
        public string UserName { get; set; }

        [JsonProperty("Password")]
        public string Password { get; set; }

        [JsonProperty("IntegratorKey")]
        public string IntegratorKey { get; set; }
    }

    public class DocuSignApiAuthConfiguration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string IntegratorKey { get; set; }
        public string BaseUrl { get; set; }
    }

    public class DocuSignApiConfiguration
    {
        public DocuSignApiAuthConfiguration AuthConfiguration =>
            new DocuSignApiAuthConfiguration
            {
                Username = "goldlike@mail.ru",
                Password = "liverpool1892",
                IntegratorKey = "3c6872c5-e78b-443b-88b6-e69f8905c72f",
                BaseUrl = "https://demo.docusign.net/restapi/",
            };
    }
}
