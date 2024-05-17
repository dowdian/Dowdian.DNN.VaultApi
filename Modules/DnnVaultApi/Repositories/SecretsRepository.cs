using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
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
        /// <inheritdoc/>
        public string GetSecret(string secretName)
        {
            return AzureKeyVaultProvider.GetSecret(secretName);
        }

        /// <inheritdoc/>
        public void CreateSecret(string secretName, string secretValue)
        {
            AzureKeyVaultProvider.CreateSecret(secretName, secretValue);
        }

        /// <inheritdoc/>
        public void DeleteSecret(string secretName)
        {
            AzureKeyVaultProvider.DeleteSecret(secretName);
        }

        /// <inheritdoc/>
        public void PurgeSecret(string secretName)
        {
            AzureKeyVaultProvider.PurgeSecret(secretName);
        }

        /// <inheritdoc/>
        public void UpdateSecret(string secretName, string secretValue)
        {
            AzureKeyVaultProvider.UpdateSecret(secretName, secretValue);
        }
#pragma warning restore SA1600 // Elements should be documented
    }
}