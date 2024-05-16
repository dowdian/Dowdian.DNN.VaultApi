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

using Azure.Security.KeyVault.Secrets;
using Microsoft.Identity.Client;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Dowdian.Modules.DnnVaultApi.Providers
{
    public class AzureKeyVaultCertificationHelper
    {
        public static X509Certificate2 FindCertificateByThumbprint(string thumbprint)
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