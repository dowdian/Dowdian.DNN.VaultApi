using System;
using System.Collections.Generic;
using Dowdian.Modules.DnnVaultApi.Providers;

namespace Dowdian.Modules.DnnVaultApi.Repositories
{
    /// <summary>
    /// IEncryptionRepository
    /// </summary>
    public partial interface ISecretsRepository
    {
        /// <summary>
        /// Get a secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns>KeyValuePair(string, string)</returns>
        KeyValuePair<string, string> GetSecret(string secretName);

        /// <summary>
        /// Update the secret
        /// </summary>
        /// <param name="secret">KeyValuePair(string, string) secret</param>
        /// <returns>bool</returns>
        bool UpdateSecret(KeyValuePair<string, string> secret);

        /// <summary>
        /// Soft delete the secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns>bool</returns>
        bool DeleteSecret(string secretName);

        /// <summary>
        /// Restore a deleted secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns>bool</returns>
        bool RestoreSecret(string secretName);

        /// <summary>
        /// Purge a deleted secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns>bool</returns>
        bool PurgeSecret(string secretName);

        /// <summary>
        /// This method will create the appSecrets section in the module Web.config file
        /// if it does not already exist and confirm that is encrypted.
        /// </summary>
        /// <returns>bool</returns>
        bool EncryptAppSecrets();

        /// <summary>
        /// This method will decrypt the appSecrets section in the module Web.config file, 
        /// leaving your secrets exposed in plain text.
        /// </summary>
        /// <returns>bool</returns>
        bool DecryptAppSecrets();

        /// <summary>
        /// Encrypt the connection strings in the site Web.config file.
        /// </summary>
        /// <returns>bool</returns>
        bool EncryptConnectionStrings();

        /// <summary>
        /// Decrypt the connection strings in the site Web.config file, 
        /// leaving your connection strings exposed in plain text.
        /// </summary>
        /// <returns></returns>
        bool DecryptConnectionStrings();
    }

    /// <inheritdoc/>
    public class SecretsRepository : ISecretsRepository
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        private LocalKeyVaultProvider localKeyVaultProvider;
        private KeyVaultProviderBase _secretProvider;

        /// <summary>
        /// A list of the possible secret hosts.
        /// </summary>
        public enum SecretHost
        {
            /// <summary>
            /// This secret is stored locally in the module's web.config file.
            /// </summary>
            Local,
            /// <summary>
            /// This secret is stored in the Azure Key Vault.
            /// </summary>
            Azure,
            /// <summary>
            /// This secret is stored in the Bitwarden Vault.
            /// Not yet supported.
            /// </summary>
            Bitwarden,
        }

        public bool IsInitialized => _secretProvider.IsInitialized;

        public bool ConnectionStringsEncrypted => localKeyVaultProvider.ConnectionStringsEncrypted;

        public bool AppSecretsEncrypted => localKeyVaultProvider.AppSecretsEncrypted;

        /// <summary>
        /// SecretsRepository
        /// </summary>
        public SecretsRepository(SecretHost host)
        {
            localKeyVaultProvider = new LocalKeyVaultProvider();
            switch (host)
            {
                case SecretHost.Local:
                    _secretProvider = localKeyVaultProvider;
                    break;
                case SecretHost.Azure:
                    _secretProvider = new AzureKeyVaultProvider();
                    break;
                case SecretHost.Bitwarden:
                    _secretProvider = new BitwardenKeyVaultProvider();
                    break;
                default:
                    throw new ArgumentException("Invalid secret host");
            }
        }

        public KeyValuePair<string, string> GetSecret(string secretName)
        {
            // see if the secret name can be found in the configuration section. If not, return the secret from the Azure Key Vault
            var secret = _secretProvider.GetSecret(secretName);
            if (secret.Key != string.Empty)
            {
                return secret;
            }

            return _secretProvider.GetSecret(secretName);
        }

        public bool UpdateSecret(KeyValuePair<string, string> secret)
        {
            return _secretProvider.UpdateSecret(secret);
        }

        public bool DeleteSecret(string secretName)
        {
            return _secretProvider.DeleteSecret(secretName);
        }

        /// <inheritdoc/>
        public bool RestoreSecret(string secretName)
        {
            return _secretProvider.RestoreSecret(secretName);
        }

        public bool PurgeSecret(string secretName)
        {
            return _secretProvider.PurgeSecret(secretName);
        }

        public Dictionary<string, string> GetSettings()
        {
            return _secretProvider.GetSettings();
        }

        public bool ConfimSettings(Dictionary<string, string> settings)
        {
            return _secretProvider.ConfirmSettings(settings);
        }

        public bool SaveSettings(Dictionary<string, string> settings)
        {
            return _secretProvider.SaveSettings(settings);
        }

        public bool EncryptAppSecrets()
        {
            return localKeyVaultProvider.EncryptAppSecrets();
        }

        public bool DecryptAppSecrets()
        {
            return localKeyVaultProvider.DecryptAppSecrets();
        }

        public bool EncryptConnectionStrings()
        {
            return localKeyVaultProvider.EncryptConnectionStrings();
        }

        public bool DecryptConnectionStrings()
        {
            return localKeyVaultProvider.DecryptConnectionStrings();
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
    }
}