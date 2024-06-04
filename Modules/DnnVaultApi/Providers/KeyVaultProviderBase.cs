using System.Collections.Generic;

namespace Dowdian.Modules.DnnVaultApi.Providers
{
    /// <summary>
    /// Base class for Key Vault Providers
    /// </summary>
    public abstract class KeyVaultProviderBase
    {
        // A prefix to add to the setting names to avoid conflicts with other secrets.
        internal string vaultSettingsPrefix = "Dowdian.Modules.DnnVaultApi.";

        public bool IsInitialized { get; protected set; } = false;

        public abstract KeyValuePair<string, string> GetSecret(string secretName);

        public abstract bool UpdateSecret(KeyValuePair<string, string> secret);

        public abstract bool DeleteSecret(string secretName);

        public abstract bool RestoreSecret(string secretName);

        public abstract bool PurgeSecret(string secretName);

        public abstract Dictionary<string, string> GetSettings();

        public abstract bool ConfirmSettings(Dictionary<string, string> settings);

        public abstract bool SaveSettings(Dictionary<string, string> settings);
    }
}
