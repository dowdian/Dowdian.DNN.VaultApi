using System.Configuration;

namespace Dowdian.Modules.DnnVaultApi.Models
{
    /// <summary>
    /// AppSecretsSection
    /// </summary>
    public class LocalSecretsSection : ConfigurationSection
    {
        /// <summary>
        /// Gets decorate the property with the tag for your collection.
        /// </summary>
        [ConfigurationProperty("secrets")]
        public LocalSecretCollection Secrets
        {
            get { return (LocalSecretCollection)this["secrets"]; }
        }
    }
}