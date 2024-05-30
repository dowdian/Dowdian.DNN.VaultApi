// <copyright file="DowdianApiController.cs" company="Dowdian SRL">
// Copyright (c) Dowdian SRL. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DotNetNuke.Services.Exceptions;

namespace Dowdian.Modules.DnnVaultApi.Providers
{
    /// <summary>
    /// AzureKeyVaultCertificationHelper
    /// </summary>
    public class AzureKeyVaultProvider : IKeyVaultProvider
    {
        /// <summary>
        /// GetSecret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        public KeyValuePair<string, string> GetSecret(string secretName)
        {
            var client = GetClient();
            var secret = client.GetSecret(secretName);
            return new KeyValuePair<string, string> (secret.Value.Name, secret.Value.Value);
        }

        /// <summary>
        /// CreateSecret
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretValue"></param>
        public bool CreateSecret(string secretName, string secretValue)
        {
            try
            {
                var client = GetClient();
                client.SetSecret(secretName, secretValue);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// DeleteSecret
        /// </summary>
        /// <param name="secretName"></param>
        public bool DeleteSecret(string secretName)
        {
            try
            {
                var client = GetClient();
                client.StartDeleteSecret(secretName);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// RestoreSecret
        /// </summary>
        /// <param name="secretName"></param>
        public bool RestoreSecret(string secretName)
        {
            try
            {
                var client = GetClient();
                client.StartRecoverDeletedSecret(secretName);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// PurgeSecret
        /// </summary>
        /// <param name="secretName"></param>
        public bool PurgeSecret(string secretName)
        {
            try
            {
                var client = GetClient();
                client.PurgeDeletedSecret(secretName);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// UpdateSecret
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretValue"></param>
        public bool UpdateSecret(string secretName, string secretValue)
        {
            return CreateSecret(secretName, secretValue);
        }

        private SecretClient GetClient()
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

        private X509Certificate2 FindCertificateByThumbprint(string thumbprint)
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