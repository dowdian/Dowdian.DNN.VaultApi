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
            var SecretsRepository = new SecretsRepository(host);
            SecretsRepository.CreateSecret(secret);
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
            var SecretsRepository = new SecretsRepository(host);
            var secret = SecretsRepository.GetSecret(secretName);
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
            var SecretsRepository = new SecretsRepository(host);
            SecretsRepository.UpdateSecret(secret);
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
            var SecretsRepository = new SecretsRepository(host);
            SecretsRepository.DeleteSecret(secretName);
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
            var SecretsRepository = new SecretsRepository(host);
            SecretsRepository.RestoreSecret(secretName);
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
            var SecretsRepository = new SecretsRepository(host);
            SecretsRepository.PurgeSecret(secretName);
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
            var localSecretsRepository = new SecretsRepository(SecretHost.Local);

            // Loop through all the possible SecretHosts
            foreach (SecretHost host in Enum.GetValues(typeof(SecretHost)))
            {
                // Get the setting names for the current SecretHost
                var secretsRepository = new SecretsRepository(host);

                // Get the setting names for the current SecretHost
                var settingNames = secretsRepository.GetSettingNames();

                // Loop through all the setting names and get the value for each setting
                foreach (var settingName in settingNames)
                {
                    // Get the value for the setting from the module web.config file
                    var secret = localSecretsRepository.GetSecret($"Dowdian.Modules.DnnVaultApi.{settingName}");

                    // Add the setting name and value to the settings dictionary
                    settings.Add(settingName, secret.Value);
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
        public HttpResponseMessage TestDnnVaultSettings(Dictionary<string, string> settings)
        {
            var hosts = new Dictionary<string, bool>();

            // Loop through all the possible SecretHosts
            foreach (SecretHost host in Enum.GetValues(typeof(SecretHost)))
            {
                var SecretsRepository = new SecretsRepository(SecretHost.Local);
                hosts.Add(host.ToString(), SecretsRepository.ConfimSettings(settings));
            }

            var answer = JsonConvert.SerializeObject(hosts);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // because there is no Authorize attribute, this method defaults to requiring a host user
        public HttpResponseMessage UpdateDnnVaultSettings(Dictionary<string, string> settings)
        {
            var repostioryList = new List<SecretsRepository> {
                new SecretsRepository(SecretHost.Local),
                new SecretsRepository(SecretHost.Azure),
            };

            foreach (var secretsRepository in repostioryList)
            {
                var SecretsRepository = new SecretsRepository(SecretHost.Local);
                foreach (var setting in settings)
                {
                    var newSetting = new KeyValuePair<string, string>($"Dowdian.Modules.DnnVaultApi.{setting.Key}", setting.Value);
                    SecretsRepository.UpdateSecret(newSetting);
                }
            }

            var answer = JsonConvert.SerializeObject($"The settings have been successfully updated.");
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        // because there is no Authorize attribute, this method defaults to requiring a host user
        public HttpResponseMessage EncryptConnectionStrings()
        {
            var SecretsRepository = new SecretsRepository(SecretHost.Local);
            SecretsRepository.EncryptConnectionStrings();
            var answer = JsonConvert.SerializeObject($"The connection strings have been successfully encrypted.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        // because there is no Authorize attribute, this method defaults to requiring a host user
        public HttpResponseMessage DecryptConnectionStrings()
        {
            var SecretsRepository = new SecretsRepository(SecretHost.Local);
            SecretsRepository.DecryptConnectionStrings();
            var answer = JsonConvert.SerializeObject($"The connection strings have been successfully decrypted.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        // because there is no Authorize attribute, this method defaults to requiring a host user
        public HttpResponseMessage EncryptAppSecrets()
        {
            var SecretsRepository = new SecretsRepository(SecretHost.Local);
            SecretsRepository.EncryptAppSecrets();
            var answer = JsonConvert.SerializeObject($"The appSecrets section in the module web.config file has been successfully encrypted.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        // because there is no Authorize attribute, this method defaults to requiring a host user
        public HttpResponseMessage DecryptAppSecrets()
        {
            var SecretsRepository = new SecretsRepository(SecretHost.Local);
            SecretsRepository.DecryptAppSecrets();
            var answer = JsonConvert.SerializeObject($"The appSecrets section in the module web.config file has been successfully decrypted.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }
    }
}