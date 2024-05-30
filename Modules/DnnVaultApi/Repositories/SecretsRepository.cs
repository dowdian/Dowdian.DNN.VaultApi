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
        /// <returns></returns>
        string GetSecret(string secretName);

        /// <summary>
        /// Create a secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretValue"></param>
        /// <returns></returns>
        string CreateSecret(string secretName, string secretValue);

        /// <summary>
        /// Use this to decrypt the inputText vavlue
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        string DeleteSecret(string secretName);

        /// <summary>
        /// Restore the secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        string RestoreSecret(string secretName);

        /// <summary>
        /// Purge the secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        string PurgeSecret(string secretName);

        /// <summary>
        /// Update the secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretValue"></param>
        /// <returns></returns>
        string UpdateSecret(string secretName, string secretValue);
    }

    /// <inheritdoc/>
    public class SecretsRepository
    {
        private LocalKeyVaultProvider localKeyVaultProvider;
        private AzureKeyVaultProvider azureKeyVaultProvider;

        /// <summary>
        /// SecretsRepository
        /// </summary>
        public SecretsRepository()
        {
            localKeyVaultProvider = new LocalKeyVaultProvider();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void CreateSecret(string secretName, string secretValue)
        {
            azureKeyVaultProvider.CreateSecret(secretName, secretValue);
        }

        /// <inheritdoc/>
        public void DeleteSecret(string secretName)
        {
            azureKeyVaultProvider.DeleteSecret(secretName);
        }

        /// <inheritdoc/>
        public void RestoreSecret(string secretName)
        {
            azureKeyVaultProvider.RestoreSecret(secretName);
        }

        /// <inheritdoc/>
        public void PurgeSecret(string secretName)
        {
            azureKeyVaultProvider.PurgeSecret(secretName);
        }

        /// <inheritdoc/>
        public void UpdateSecret(string secretName, string secretValue)
        {
            azureKeyVaultProvider.UpdateSecret(secretName, secretValue);
        }
#pragma warning restore SA1600 // Elements should be documented
    }
}