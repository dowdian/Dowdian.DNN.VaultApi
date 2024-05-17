//using Azure.Identity;
//using Azure.Security.KeyVault.Secrets;
//using System;
//using System.Configuration;
//using System.Threading.Tasks;

//namespace Dowdian.Modules.DowdianDnnVaultApi.Providers
//{
//    public class AzureKeyVaultCertificationHelper
//    {
//        public static async Task<string> GetAccessToken(string authority, string resource, string scope)
//        {
//            var credential = new DefaultAzureCredential();
//            var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { scope }));

//            return token.Token;
//        }

//        public static string GetKeyVaultSecret(string secretNode)
//        {
//            var secretClient = new SecretClient(new Uri(ConfigurationManager.AppSettings["VaultUrl"]), new DefaultAzureCredential());

//            var secretUri = $"{ConfigurationManager.AppSettings["VaultUrl"]}/secrets/{secretNode}";
//            var secret = secretClient.GetSecret(secretUri);

//            return secret.Value.Value;
//        }
//    }
//}

// <copyright file="DowdianApiController.cs" company="Dowdian SRL">
// Copyright (c) Dowdian SRL. All rights reserved.
// </copyright>

using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Dowdian.Modules.DnnVaultApi.Providers
{
    /// <summary>
    /// AzureKeyVaultCertificationHelper
    /// </summary>
    public static class AzureKeyVaultProvider
    {
        /// <summary>
        /// GetSecret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        public static string GetSecret(string secretName)
        {
            var client = GetClient();
            var secret = client.GetSecret(secretName);
            return secret.Value.Value;
        }

        /// <summary>
        /// CreateSecret
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretValue"></param>
        public static void CreateSecret(string secretName, string secretValue)
        {
            var client = GetClient();
            client.SetSecret(secretName, secretValue);
        }

        /// <summary>
        /// DeleteSecret
        /// </summary>
        /// <param name="secretName"></param>
        public static void DeleteSecret(string secretName)
        {
            var client = GetClient();
            client.StartDeleteSecret(secretName);
        }

        /// <summary>
        /// PurgeSecret
        /// </summary>
        /// <param name="secretName"></param>
        public static void PurgeSecret(string secretName)
        {
            var client = GetClient();
            client.PurgeDeletedSecret(secretName);
        }

        /// <summary>
        /// UpdateSecret
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretValue"></param>
        public static void UpdateSecret(string secretName, string secretValue)
        {
            CreateSecret(secretName, secretValue);
        }

        private static SecretClient GetClient()
        {
            var thumbPrint = ConfigurationManager.AppSettings["Thumbprint"];
            var tenantId = ConfigurationManager.AppSettings["TenantId"];
            var clientApplicationId = ConfigurationManager.AppSettings["ClientApplicationId"];
            var keyVaultUri = ConfigurationManager.AppSettings["KeyVaultUri"];

            var cert = FindCertificateByThumbprint(thumbPrint);
            var httpClient = new HttpClient(new HttpClientHandler
            {
                DefaultProxyCredentials = CredentialCache.DefaultCredentials,
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls
            });

            return new SecretClient(new Uri(keyVaultUri), new ClientCertificateCredential(tenantId, clientApplicationId, cert));
        }

        private static X509Certificate2 FindCertificateByThumbprint(string thumbprint)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (col == null || col.Count == 0)
                {
                    throw new Exception("ERROR: Certificate not found with thumbprint");
                }
                return col[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                store.Close();
            }
        }
    }
}