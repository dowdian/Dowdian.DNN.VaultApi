using System.Collections.Generic;

namespace Dowdian.Modules.DnnVaultApi.Providers
{
    /// <summary>
    /// Base class for Key Vault Providers
    /// </summary>
    public abstract class KeyVaultProviderBase
    {
        public abstract bool CreateSecret(string secretName, string secretValue);

        public abstract KeyValuePair<string, string> GetSecret(string secretName);

        public abstract bool UpdateSecret(string secretName, string secretValue);

        public abstract bool DeleteSecret(string secretName);

        public abstract bool RestoreSecret(string secretName);

        public abstract bool PurgeSecret(string secretName);

        // You can add additional non-abstract methods here
    }
}
