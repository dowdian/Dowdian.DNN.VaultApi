// <copyright file="DowdianApiController.cs" company="Dowdian SRL">
// Copyright (c) Dowdian SRL. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace Dowdian.Modules.DnnVaultApi.Providers
{
    /// <summary>
    /// BitwardenKeyVaultCertificationHelper
    /// </summary>
    public class BitwardenKeyVaultProvider : KeyVaultProviderBase
    {
        public BitwardenKeyVaultProvider()
        {
            this.IsInitialized = false;
        }

        /// <summary>
        /// GetSecret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        public override KeyValuePair<string, string> GetSecret(string secretName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// UpdateSecret
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretValue"></param>
        public override bool UpdateSecret(KeyValuePair<string, string> secret)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// DeleteSecret
        /// </summary>
        /// <param name="secretName"></param>
        public override bool DeleteSecret(string secretName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// RestoreSecret
        /// </summary>
        /// <param name="secretName"></param>
        public override bool RestoreSecret(string secretName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// PurgeSecret
        /// </summary>
        /// <param name="secretName"></param>
        public override bool PurgeSecret(string secretName)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GetSettings()
        {
            return new Dictionary<string, string>();
        }

        public override bool ConfirmSettings(Dictionary<string, string> settings)
        {
            return true;
        }

        public override bool SaveSettings(Dictionary<string, string> settings)
        {
            return true;
        }
    }
}