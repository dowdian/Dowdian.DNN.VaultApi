// <copyright file="QuidProKeys.cs" company="Dowdian SRL">
// Copyright (c) Dowdian SRL. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;
using Dowdian.Modules.DnnVaultApi.Models;

namespace Dowdian.Modules.DnnVaultApi.Providers
{
    /// <summary>
    /// This class reads the defined config section (if available) and stores it locally in the configurationSection variable.
    /// This config data is available by calling QuidProKeys.GetKeys().
    /// </summary>
    public class LocalKeyVaultProvider : IKeyVaultProvider
    {
        ///// <summary>
        ///// The QuidPro Configuration File
        ///// </summary>
        //private Configuration configurationFile = WebConfigurationManager.OpenWebConfiguration("~/DesktopModules/MVC/Dowdian.Modules.DnnVaultApi");

        ///// <summary>
        ///// The QuidPro Configuration Section
        ///// </summary>
        //private SecretsSection configurationSection = (SecretsSection)configurationFile.GetSection("appSecrets");

        /// <summary>
        /// The element collection that make up the QuidPro Configuration Section
        /// </summary>
        /// <returns>Dictionary of string, string</returns>
        public Dictionary<string, string> GetSecrets()
        {
            var secrets = new Dictionary<string, string>();
            //foreach (SecretElement key in configurationSection.Secrets)
            //{
            //    secrets.Add(key.Name, key.Secret);
            //}

            return secrets;
        }

        /// <summary>
        /// The element collection that make up the QuidPro Configuration Section
        /// </summary>
        /// <returns>Dictionary of string, string</returns>
        public KeyValuePair<string, string> GetSecret(string secretName)
        {
            var tinyConfigurationFile = WebConfigurationManager.OpenWebConfiguration("~/DesktopModules/MVC/Dowdian.Modules.DnnVaultApi");
            var tinyConfigurationSection = (LocalSecretsSection)tinyConfigurationFile.GetSection("appSecrets");

            foreach (LocalSecretElement key in tinyConfigurationSection.Secrets)
            {
                if (key.Name == secretName)
                {
                    return new KeyValuePair<string, string>(key.Name, key.Secret);
                }
            }

            return new KeyValuePair<string, string>();
        }

        public bool CreateSecret(string secretName, string secretValue)
        {
            throw new NotImplementedException();
        }

        public bool UpdateSecret(string secretName, string secretValue)
        {
            throw new NotImplementedException();
        }

        public bool DeleteSecret(string secretName)
        {
            throw new NotImplementedException();
        }

        public bool RestoreSecret(string secretName)
        {
            throw new NotImplementedException();
        }

        public bool PurgeSecret(string secretName)
        {
            throw new NotImplementedException();
        }
    }
}