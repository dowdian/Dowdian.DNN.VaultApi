using System;
using System.Collections.Generic;
using DotNetNuke.Framework;
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
        /// <returns></returns>
        KeyValuePair<string, string> GetSecret(string secretName);

        /// <summary>
        /// Create a secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretValue"></param>
        /// <returns></returns>
        bool CreateSecret(string secretName, string secretValue);

        /// <summary>
        /// Soft delete the secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        bool DeleteSecret(string secretName);

        /// <summary>
        /// Restore a deleted secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        bool RestoreSecret(string secretName);

        /// <summary>
        /// Purge a deleted secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        bool PurgeSecret(string secretName);

        /// <summary>
        /// Update the secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretValue"></param>
        /// <returns></returns>
        bool UpdateSecret(string secretName, string secretValue);

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
    public class SecretsRepository : ServiceLocator<ISecretsRepository, SecretsRepository>, ISecretsRepository
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        private LocalKeyVaultProvider localKeyVaultProvider;
        private AzureKeyVaultProvider azureKeyVaultProvider;

        /// <summary>
        /// SecretsRepository
        /// </summary>
        public SecretsRepository()
        {
            localKeyVaultProvider = new LocalKeyVaultProvider();
        }

        public KeyValuePair<string, string> GetSecret(string secretName)
        {
            // see if the secret name can be found in the configuration section. If not, return the secret from the Azure Key Vault
            var localSecret = localKeyVaultProvider.GetSecret(secretName);
            if (localSecret.Key != string.Empty)
            {
                return localSecret;
            }

            return azureKeyVaultProvider.GetSecret(secretName);
        }

        public bool CreateSecret(string secretName, string secretValue)
        {
            return azureKeyVaultProvider.CreateSecret(secretName, secretValue);
        }

        public bool DeleteSecret(string secretName)
        {
            return azureKeyVaultProvider.DeleteSecret(secretName);
        }

        /// <inheritdoc/>
        public bool RestoreSecret(string secretName)
        {
            return azureKeyVaultProvider.RestoreSecret(secretName);
        }

        public bool PurgeSecret(string secretName)
        {
            return azureKeyVaultProvider.PurgeSecret(secretName);
        }

        public bool UpdateSecret(string secretName, string secretValue)
        {
            return azureKeyVaultProvider.UpdateSecret(secretName, secretValue);
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

        protected override Func<ISecretsRepository> GetFactory()
        {
            return () => new SecretsRepository();
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
    }
}