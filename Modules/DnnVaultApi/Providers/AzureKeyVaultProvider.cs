// <copyright file="DowdianApiController.cs" company="Dowdian SRL">
// Copyright (c) Dowdian SRL. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DotNetNuke.Services.Exceptions;

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

        public override Dictionary<string, string> GetSettings()
        {
            var settings = new Dictionary<string, string>();
            var names = new List<string>
            {
                "SignatureThumbprint",
                "AzureTenantId",
                "AzureClientApplicationId",
                "AzureKeyVaultUri"
            };

            foreach (var name in names)
            {
                var localKeyVaultProvider = new LocalKeyVaultProvider();
                var settingSecret = localKeyVaultProvider.GetSecret($"{base.vaultSettingsPrefix}{name}");
                settings.Add(name, settingSecret.Value);
            }
            return settings;
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
                    var secret = localKeyVaultProvider.GetSecret($"{base.vaultSettingsPrefix}{setting.Key}");
                    localKeyVaultProvider.UpdateSecret(new KeyValuePair<string, string>(secret.Key, setting.Value));
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
                var thumbPrint = localKeyVaultProvider.GetSecret($"{base.vaultSettingsPrefix}SignatureThumbprint").Value;
                var tenantId = localKeyVaultProvider.GetSecret($"{base.vaultSettingsPrefix}AzureTenantId").Value;
                var clientApplicationId = localKeyVaultProvider.GetSecret($"{base.vaultSettingsPrefix}AzureClientApplicationId").Value;
                var keyVaultUri = localKeyVaultProvider.GetSecret($"{base.vaultSettingsPrefix}AzureKeyVaultUri").Value;

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