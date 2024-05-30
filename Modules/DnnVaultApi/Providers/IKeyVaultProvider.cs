using System.Collections.Generic;

namespace Dowdian.Modules.DnnVaultApi.Providers
{
    /// <summary>
    /// Interface for Key Vault Providers
    /// </summary>
    public interface IKeyVaultProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretValue"></param>
        bool CreateSecret(string secretName, string secretValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        KeyValuePair<string, string> GetSecret(string secretName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretValue"></param>
        /// <returns></returns>
        bool UpdateSecret(string secretName, string secretValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secretName"></param>
        bool DeleteSecret(string secretName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        bool RestoreSecret(string secretName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        bool PurgeSecret(string secretName);
    }
}