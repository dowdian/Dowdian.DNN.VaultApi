// <copyright file="DowdianApiController.cs" company="Dowdian SRL">
// Copyright (c) Dowdian SRL. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DotNetNuke.Services.Exceptions;
using Dowdian.Modules.DnnVaultApi.Models;
using Dowdian.Modules.DnnVaultApi.Repositories;
using static Dowdian.Modules.DnnVaultApi.Repositories.SecretsRepository;

namespace Dowdian.Modules.DnnVaultApi.Providers
{
    /// <summary>
    /// AzureKeyVaultCertificationHelper
    /// </summary>
    public class AzureKeyVaultProvider : KeyVaultProviderBase
    {
        private SecretClient client;

        public AzureKeyVaultProvider()
        {
            client = GetClient();
            if (client != null)
            {
                this.IsInitialized = true;
            }
        }

        /// <summary>
        /// CreateSecret
        /// </summary>
        /// <param name="secret">KeyValuePair(string, string) secret</param>
        public override bool CreateSecret(KeyValuePair<string, string> secret)
        {
            try
            {
                client.SetSecret(secret.Key, secret.Value);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// GetSecret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        public override KeyValuePair<string, string> GetSecret(string secretName)
        {
            var secret = client.GetSecret(secretName);
            return new KeyValuePair<string, string> (secret.Value.Name, secret.Value.Value);
        }

        /// <summary>
        /// UpdateSecret
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretValue"></param>
        public override bool UpdateSecret(KeyValuePair<string, string> secret)
        {
            return CreateSecret(secret);
        }

        /// <summary>
        /// DeleteSecret
        /// </summary>
        /// <param name="secretName"></param>
        public override bool DeleteSecret(string secretName)
        {
            try
            {
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
        public override bool RestoreSecret(string secretName)
        {
            try
            {
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
        public override bool PurgeSecret(string secretName)
        {
            try
            {
                client.PurgeDeletedSecret(secretName);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }

            return true;
        }

        public override List<string> GetSettingNames()
        {
            return new List<string>
            {
                "SignatureThumbprint",
                "AzureTenantId",
                "AzureClientApplicationId",
                "AzureKeyVaultUri"
            };
        }

        public override bool ConfirmSettings(Dictionary<string, string> settings)
        {
            var signatureThumbprint = settings["SignatureThumbprint"];
            var azureTenantId = settings["AzureTenantId"];
            var azureClientApplicationId = settings["AzureClientApplicationId"];
            var azureKeyVaultUri = settings["AzureKeyVaultUri"];

            var secretClient = GetClient(signatureThumbprint, azureTenantId, azureClientApplicationId, azureKeyVaultUri);
            if (secretClient == null)
            {
                return false;
            }

            return true;
        }

        public override bool SaveSettings(Dictionary<string, string> settings)
        {
            var localKeyVaultProvider = new LocalKeyVaultProvider();
            if (this.ConfirmSettings(settings))
            {
                foreach (var setting in settings)
                {
                    var secret = localKeyVaultProvider.GetSecret(setting.Key);
                    if (!string.IsNullOrEmpty(secret.Key))
                    {
                        localKeyVaultProvider.UpdateSecret(setting);
                    }
                    else
                    {
                        localKeyVaultProvider.CreateSecret(setting);
                    }
                }

                return true;
            }

            Exceptions.LogException(new Exception("Settings are not valid."));
            return false;
        }

        // Private methods

        private SecretClient GetClient()
        {
            try
            {
                var localKeyVaultProvider = new LocalKeyVaultProvider();
                var thumbPrint = localKeyVaultProvider.GetSecret("SignatureThumbprint").Value;
                var tenantId = localKeyVaultProvider.GetSecret("AzureTenantId").Value;
                var clientApplicationId = localKeyVaultProvider.GetSecret("AzureClientApplicationId").Value;
                var keyVaultUri = localKeyVaultProvider.GetSecret("AzureKeyVaultUri").Value;

                var certificateProvider = new CertificateProvider();
                var cert = certificateProvider.FindCertificateByThumbprint(thumbPrint);

                return new SecretClient(new Uri(keyVaultUri), new ClientCertificateCredential(tenantId, clientApplicationId, cert));
            }
            catch
            {
                return null;
            }

        }

        private SecretClient GetClient(string thumbPrint, string tenantId, string clientApplicationId, string keyVaultUri)
        {
            var certificateProvider = new CertificateProvider();
            var cert = certificateProvider.FindCertificateByThumbprint(thumbPrint);

            return new SecretClient(new Uri(keyVaultUri), new ClientCertificateCredential(tenantId, clientApplicationId, cert));
        }

    }
}