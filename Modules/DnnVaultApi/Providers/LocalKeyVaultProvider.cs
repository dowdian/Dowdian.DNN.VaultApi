// <copyright file="QuidProKeys.cs" company="Dowdian SRL">
// Copyright (c) Dowdian SRL. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;
using DotNetNuke.Services.Exceptions;
using Dowdian.Modules.DnnVaultApi.Models;

namespace Dowdian.Modules.DnnVaultApi.Providers
{
    /// <summary>
    /// This class reads the defined config section (if available) and stores it locally in the configurationSection variable.
    /// This config data is available by calling QuidProKeys.GetKeys().
    /// </summary>
    public class LocalKeyVaultProvider : KeyVaultProviderBase
    {
        private Configuration configuration;

        private LocalSecretsSection localSecretsSection;

        // A prefix to add to the setting names to avoid conflicts with other secrets.
        private string prefix = "Dowdian.Modules.DnnVaultApi.";

        /// <summary>
        /// Constructor
        /// </summary>
        public LocalKeyVaultProvider()
        {
            configuration = WebConfigurationManager.OpenWebConfiguration("~/DesktopModules/MVC/Dowdian.Modules.DnnVaultApi");

            if (configuration == null)
            {
                throw new Exception("Configuration not found.");
            }

            localSecretsSection = (LocalSecretsSection)configuration.GetSection("appSecrets");

            if (localSecretsSection == null)
            {
                localSecretsSection = new LocalSecretsSection();
                configuration.Sections.Add("appSecrets", localSecretsSection);
                configuration.Save();
            }

            this.IsInitialized = true;
        }

        /// <summary>
        /// Use this to get a specific key/value pair from the encrypted configuration section.
        /// </summary>
        /// <returns>Dictionary of string, string</returns>
        public override KeyValuePair<string, string> GetSecret(string secretName)
        {
            foreach (LocalSecretElement key in localSecretsSection.Secrets)
            {
                if (key.Name == secretName)
                {
                    return new KeyValuePair<string, string>(key.Name, key.Secret);
                }
            }

            return new KeyValuePair<string, string>();
        }

        public override bool CreateSecret(KeyValuePair<string, string> secret)
        {
            try
            {
                var newSecret = new LocalSecretElement
                {
                    Name = secret.Key,
                    Secret = secret.Value
                };
                return localSecretsSection.Secrets.Add(newSecret);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }
        }

        public override bool UpdateSecret(KeyValuePair<string, string> secret)
        {
            try
            {
                var localSecret = localSecretsSection.Secrets.Get(secret.Key);
                var deleted = (bool)localSecret.ElementInformation.Properties["Deleted"].Value;
                if (localSecret != null && !deleted)
                {
                    localSecret.Secret = secret.Value;
                    localSecretsSection.Secrets.Update(localSecret);
                    return true;
                }

                throw new Exception("Secret not found.");
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }
        }

        public override bool DeleteSecret(string secretName)
        {
            try
            {
                var secret = localSecretsSection.Secrets.Get(secretName);
                var deleted = (bool)secret.ElementInformation.Properties["Deleted"].Value;
                if (secret != null && !deleted)
                {
                    secret.ElementInformation.Properties["Deleted"].Value = true;
                    return true;
                }

                if (deleted)
                {
                    throw new Exception("Secret already deleted.");
                }

                throw new Exception("Secret not found.");
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }
        }

        public override bool RestoreSecret(string secretName)
        {
            try
            {
                var secret = localSecretsSection.Secrets.Get(secretName);
                var deleted = (bool)secret.ElementInformation.Properties["Deleted"].Value;
                if (secret != null && deleted)
                {
                    secret.ElementInformation.Properties["Deleted"].Value = false;
                    return true;
                }

                if (!deleted)
                {
                    throw new Exception("Secret already restored.");
                }

                throw new Exception("Secret not found.");
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }
        }

        public override bool PurgeSecret(string secretName)
        {
            try
            {
                var secret = localSecretsSection.Secrets.Get(secretName);
                var deleted = (bool)secret.ElementInformation.Properties["Deleted"].Value;
                if (secret != null && deleted)
                {
                    localSecretsSection.Secrets.Delete(secretName);
                    return true;
                }

                if (!deleted)
                {
                    throw new Exception("Secret not deleted.");
                }

                throw new Exception("Secret not found.");
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }
        }

        public override List<string> GetSettingNames()
        {
            return new List<string>
            {
                "ExchangeThumbprint"
            };
        }

        public override bool ConfirmSettings(Dictionary<string, string> settings)
        {
            var thumbprint = settings["ExchangeThumbprint"];
            var certificateProvider = new CertificateProvider();
            var cert = certificateProvider.FindCertificateByThumbprint(thumbprint);

            return cert != null;
        }

        public override bool SaveSettings(Dictionary<string, string> settings)
        {
            if (this.ConfirmSettings(settings))
            {
                foreach (var setting in settings)
                {
                    var secret = localSecretsSection.Secrets.Get(setting.Key);
                    if (secret != null)
                    {
                        secret.Secret = setting.Value;
                        localSecretsSection.Secrets.Update(secret);
                    }
                    else
                    {
                        localSecretsSection.Secrets.Add(new LocalSecretElement
                        {
                            Name = setting.Key,
                            Secret = setting.Value
                        });
                    }
                }

                configuration.Save();
                return true;
            }

            Exceptions.LogException(new Exception("Invalid settings"));
            return false;
        }

        // Provider specific methods

        public bool EncryptAppSecrets()
        {
            try
            {
                var moduleConfig = WebConfigurationManager.OpenWebConfiguration("~/DesktopModules/DnnVaultApi");
                var appSecretsSection = moduleConfig.GetSection("appSecrets");
                if (appSecretsSection == null)
                {
                    var appSecrets = new LocalSecretsSection();
                    moduleConfig.Sections.Add("appSecrets", appSecrets);
                    moduleConfig.Save();
                }

                // Encrypt the appSecrets section.
                if (!appSecretsSection.SectionInformation.IsProtected)
                {
                    appSecretsSection.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                    appSecretsSection.SectionInformation.ForceSave = true;
                    moduleConfig.Save();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }

            return true;
        }

        public bool DecryptAppSecrets()
        {
            throw new NotImplementedException();
        }

        public bool EncryptConnectionStrings()
        {
            throw new NotImplementedException();
        }

        public bool DecryptConnectionStrings()
        {
            throw new NotImplementedException();
        }
    }
}