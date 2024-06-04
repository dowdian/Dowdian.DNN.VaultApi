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
        private Configuration appConfiguration;

        private LocalSecretsSection localSecretsSection;

        public bool ConnectionStringsEncrypted { get; private set; }
        public bool AppSecretsEncrypted { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public LocalKeyVaultProvider()
        {
            appConfiguration = WebConfigurationManager.OpenWebConfiguration("~");

            if (appConfiguration == null)
            {
                throw new Exception("Configuration not found.");
            }

            // Check to see if the connectionStrings section is encrypted.
            ConnectionStringsEncrypted = appConfiguration.GetSection("connectionStrings").SectionInformation.IsProtected;

            // Get the appSecrets section.
            localSecretsSection = (LocalSecretsSection)appConfiguration.GetSection("appSecrets");

            // If the appSecrets section does not exist, create it.
            if (localSecretsSection == null)
            {
                localSecretsSection = new LocalSecretsSection();
                appConfiguration.Sections.Add("appSecrets", localSecretsSection);
                appConfiguration.Save();
            }

            // Check to see if the appSecrets section is encrypted.
            AppSecretsEncrypted = localSecretsSection.SectionInformation.IsProtected;

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

        public override bool UpdateSecret(KeyValuePair<string, string> secret)
        {
            try
            {
                var result = false;
                var localSecret = localSecretsSection.Secrets.Get(secret.Key);
                if (localSecret == null)
                {
                    localSecret = new LocalSecretElement
                    {
                        Name = secret.Key,
                        Secret = secret.Value
                    };
                    result = localSecretsSection.Secrets.Add(localSecret);
                }
                else
                {
                    var deleted = (bool)localSecret.ElementInformation.Properties["Deleted"].Value;
                    if (!deleted)
                    {
                        localSecret.Secret = secret.Value;
                        result = localSecretsSection.Secrets.Update(localSecret);
                    }
                }

                appConfiguration.Save();

                return result;
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
                    appConfiguration.Save();
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
                    appConfiguration.Save();
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
                    appConfiguration.Save();
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

        public override Dictionary<string, string> GetSettings()
        {
            var settings = new Dictionary<string, string>();
            var names = new List<string>
            {
                "ExchangeThumbprint",
            };

            foreach (var name in names)
            {
                var settingSecret = this.GetSecret($"{base.vaultSettingsPrefix}{name}");
                settings.Add(name, settingSecret.Value);
            }

            return settings;
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
                // encrypt the web.config using the thumbprint
                var thumbprint = settings["ExchangeThumbprint"];
                var certificateProvider = new CertificateProvider();
                var cert = certificateProvider.FindCertificateByThumbprint(thumbprint);

                // programatically add or update configProtectedData section to the web.config in the "~/DesktopModules/DnnVaultApi" folder.
                // The result is a web.config that looks like this:
                // <configProtectedData>
                //  <providers>
                //    <add  name = "CustomProvider"
                //          thumbprint = "THIS0IS0NOT0REAL0THIS0IS0A0SAMPLE0000000"
                //          storeLocation = "LocalMachine"
                //          type = "Pkcs12ProtectedConfigurationProvider.Pkcs12ProtectedConfigurationProvider, PKCS12ProtectedConfigurationProvider, Version=1.0.1.0, Culture=neutral, PublicKeyToken=455a6e7bdbdc9023" />
                //  </ providers>
                // </ configProtectedData>
                var configProtectedData = appConfiguration.GetSection("configProtectedData") as ProtectedConfigurationSection;
                if (configProtectedData != null)
                {
                    var provider = configProtectedData.Providers["DnnVaultApiProvider"];
                    if (provider == null)
                    {
                        provider = new ProviderSettings("DnnVaultApiProvider", "Pkcs12ProtectedConfigurationProvider.Pkcs12ProtectedConfigurationProvider, PKCS12ProtectedConfigurationProvider, Version=1.0.1.0, Culture=neutral, PublicKeyToken=455a6e7bdbdc9023");
                        configProtectedData.Providers.Add(provider);
                    }

                    provider.Parameters["thumbprint"] = thumbprint;
                    provider.Parameters["storeLocation"] = "LocalMachine";
                }

                appConfiguration.Save();

                // We'll keep this in two places to make the rest of settings management easier.
                this.UpdateSecret(new KeyValuePair<string, string>($"{base.vaultSettingsPrefix}ExchangeThumbprint", thumbprint));

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
                var appSecretsSection = appConfiguration.GetSection("appSecrets");
                if (appSecretsSection == null)
                {
                    var appSecrets = new LocalSecretsSection();
                    appConfiguration.Sections.Add("appSecrets", appSecrets);
                    appConfiguration.Save();
                }

                // Encrypt the appSecrets section.
                if (!appSecretsSection.SectionInformation.IsProtected)
                {
                    appSecretsSection.SectionInformation.ProtectSection("DnnVaultApiProvider");
                    appSecretsSection.SectionInformation.ForceSave = true;
                    appConfiguration.Save();
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
            try
            {
                var appSecretsSection = appConfiguration.GetSection("appSecrets");
                if (appSecretsSection == null)
                {
                    var appSecrets = new LocalSecretsSection();
                    appConfiguration.Sections.Add("appSecrets", appSecrets);
                    appConfiguration.Save();
                }

                // Encrypt the appSecrets section.
                if (appSecretsSection.SectionInformation.IsProtected)
                {
                    appSecretsSection.SectionInformation.UnprotectSection();
                    appSecretsSection.SectionInformation.ForceSave = true;
                    appConfiguration.Save();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }

            return true;
        }

        public bool EncryptConnectionStrings()
        {
            try
            {
                var connectionStringsSection = appConfiguration.GetSection("connectionStrings");

                // Encrypt the connectionStrings section.
                if (!connectionStringsSection.SectionInformation.IsProtected)
                {
                    connectionStringsSection.SectionInformation.ProtectSection("DnnVaultApiProvider");
                    connectionStringsSection.SectionInformation.ForceSave = true;
                    appConfiguration.Save();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }

            return true;
        }

        public bool DecryptConnectionStrings()
        {
            try
            {
                var connectionStringsSection = appConfiguration.GetSection("connectionStrings");

                // Encrypt the connectionStrings section.
                if (connectionStringsSection.SectionInformation.IsProtected)
                {
                    connectionStringsSection.SectionInformation.UnprotectSection();
                    connectionStringsSection.SectionInformation.ForceSave = true;
                    appConfiguration.Save();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }

            return true;
        }
    }
}