// <copyright file="DowdianApiController.cs" company="Dowdian SRL">
// Copyright (c) Dowdian SRL. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using Dowdian.Modules.DnnVaultApi.Repositories;
using Newtonsoft.Json;
using static Dowdian.Modules.DnnVaultApi.Repositories.SecretsRepository;

namespace Dowdian.Modules.DnnVaultApi.Controllers
{
    /// <summary>
    /// ClientPortalApiController
    /// </summary>
    public class DnnVaultApiController : DnnApiController
    {
        #region Contructors
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Initializes a new instance of the <see cref="DnnVaultApiController"/> class.
        /// </summary>
        public DnnVaultApiController()
        {
        }

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion Contructors

        // Manage your secrets

        /// <summary>
        /// Create a Secret
        /// </summary>
        /// <param name="host"></param>
        /// <param name="secret"></param>
        /// <returns>HttpResponseMessage</returns>
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage CreateSecret(SecretHost host, KeyValuePair<string, string> secret)
        {
            var secretsRepository = new SecretsRepository(host);
            secretsRepository.UpdateSecret(secret);
            var answer = JsonConvert.SerializeObject($"A new secret with the name {secret.Key} has been successfully created in the Vault.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Get a Secret
        /// </summary>
        /// <param name="host"></param>
        /// <param name="secretName"></param>
        /// <returns>HttpResponseMessage</returns>
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage GetSecret(SecretHost host, string secretName)
        {
            var secretsRepository = new SecretsRepository(host);
            var secret = secretsRepository.GetSecret(secretName);
            var answer = JsonConvert.SerializeObject(secret);

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Update a Secret
        /// </summary>
        /// <param name="host"></param>
        /// <param name="secret">KeyValuePair(string, string)</param>
        /// <returns>HttpResponseMessage</returns>
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage UpdateSecret(SecretHost host, KeyValuePair<string, string> secret)
        {
            var secretsRepository = new SecretsRepository(host);
            secretsRepository.UpdateSecret(secret);
            var answer = JsonConvert.SerializeObject($"The secret with the name {secret.Key} has been successfully updated with a new value.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Delete a Secret
        /// </summary>
        /// <param name="host"></param>
        /// <param name="secretName"></param>
        /// <returns></returns>
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage DeleteSecret(SecretHost host, string secretName)
        {
            var secretsRepository = new SecretsRepository(host);
            secretsRepository.DeleteSecret(secretName);
            var answer = JsonConvert.SerializeObject($"The secret with the name {secretName} has been successfully soft-delete from the Vault.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Restore a Secret
        /// </summary>
        /// <param name="host"></param>
        /// <param name="secretName"></param>
        /// <returns></returns>
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage RestoreSecret(SecretHost host, string secretName)
        {
            var secretsRepository = new SecretsRepository(host);
            secretsRepository.RestoreSecret(secretName);
            var answer = JsonConvert.SerializeObject($"The soft-deleted secret with the name {secretName} has been successfully restored into the Vault.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Purge a Secret
        /// </summary>
        /// <param name="host"></param>
        /// <param name="secretName"></param>
        /// <returns></returns>
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage PurgeSecret(SecretHost host, string secretName)
        {
            var secretsRepository = new SecretsRepository(host);
            secretsRepository.PurgeSecret(secretName);
            var answer = JsonConvert.SerializeObject($"The secret with the name {secretName} has been successfully purged from the Vault.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        // Manage the API settings

        [HttpGet]
        [ValidateAntiForgeryToken]
        // because there is no Authorize attribute, this method defaults to requiring a host user
        public HttpResponseMessage GetDnnVaultSettings()
        {
            var settings = new Dictionary<string, string>();

            // Loop through all the possible SecretHosts and collect their settings
            foreach (SecretHost host in Enum.GetValues(typeof(SecretHost)))
            {
                var secretsRepository = new SecretsRepository(host);
                var hostSettings = secretsRepository.GetSettings();
                foreach (var setting in hostSettings)
                {
                    settings.Add(setting.Key, setting.Value);
                }
            }

            var answer = JsonConvert.SerializeObject(settings);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // because there is no Authorize attribute, this method defaults to requiring a host user
        public HttpResponseMessage ReceiveDnnVaultSettings([FromBody] VaultSettingsModel vaultSettings)
        {
            var hosts = new Dictionary<string, bool>();

            var thisHost = (SecretHost)Enum.Parse(typeof(SecretHost), vaultSettings.Host);
            var secretsRepository = new SecretsRepository(thisHost);

            if (vaultSettings.Action == "test"  )
            {
                hosts.Add(thisHost.ToString(), secretsRepository.ConfimSettings(vaultSettings.Settings));
            }
            else if (vaultSettings.Action == "save")
            {
                hosts.Add(thisHost.ToString(), secretsRepository.SaveSettings(vaultSettings.Settings));
            }

            var answer = JsonConvert.SerializeObject(hosts);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        // because there is no Authorize attribute, this method defaults to requiring a host user
        public HttpResponseMessage ToggleConnectionStringEncryption()
        {
            var result = string.Empty;
            var secretsRepository = new SecretsRepository(SecretHost.Local);
            if (secretsRepository.ConnectionStringsEncrypted)
            {
                secretsRepository.DecryptConnectionStrings();
                result = "decrypted";
            }
            else
            {
                secretsRepository.EncryptConnectionStrings();
                result = "encrypted";
            }

            var answer = JsonConvert.SerializeObject($"The connection strings have been successfully {result}.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        // because there is no Authorize attribute, this method defaults to requiring a host user
        public HttpResponseMessage ToggleAppSecretsEncryption()
        {
            var result = string.Empty;
            var secretsRepository = new SecretsRepository(SecretHost.Local);
            if (secretsRepository.AppSecretsEncrypted)
            {
                secretsRepository.DecryptAppSecrets();
                result = "decrypted";
            }
            else
            {
                secretsRepository.EncryptAppSecrets();
                result = "encrypted";
            }

            var answer = JsonConvert.SerializeObject($"The appSecrets section in the module web.config file has been successfully {result}.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }
    }
}