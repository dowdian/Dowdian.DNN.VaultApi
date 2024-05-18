// <copyright file="DowdianApiController.cs" company="Dowdian SRL">
// Copyright (c) Dowdian SRL. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using Dowdian.Modules.DnnVaultApi.Repositories;
using Newtonsoft.Json;

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

        /// <summary>
        /// Create a Secret
        /// </summary>
        /// <returns>HttpResponseMessage</returns>
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage CreateSecret(string secretName, string secretValue)
        {
            var SecretsRepository = new SecretsRepository();
            SecretsRepository.CreateSecret(secretName, secretValue);
            var answer = JsonConvert.SerializeObject($"A new secret with the name {secretName} has been successfully created in the Vault.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Get a Secret
        /// </summary>
        /// <returns>HttpResponseMessage</returns>
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage GetSecret(string secretName)
        {
            var SecretsRepository = new SecretsRepository();
            var secret = SecretsRepository.GetSecret(secretName);
            var answer = JsonConvert.SerializeObject(secret);

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Update a Secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretValue"></param>
        /// <returns></returns>
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage UpdateSecret(string secretName, string secretValue)
        {
            var SecretsRepository = new SecretsRepository();
            SecretsRepository.UpdateSecret(secretName, secretValue);
            var answer = JsonConvert.SerializeObject($"The secret with the name {secretName} has been successfully updated with a new value.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Delete a Secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage DeleteSecret(string secretName)
        {
            var SecretsRepository = new SecretsRepository();
            SecretsRepository.DeleteSecret(secretName);
            var answer = JsonConvert.SerializeObject($"The secret with the name {secretName} has been successfully soft-delete from the Vault.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Restore a Secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage RestoreSecret(string secretName)
        {
            var SecretsRepository = new SecretsRepository();
            SecretsRepository.RestoreSecret(secretName);
            var answer = JsonConvert.SerializeObject($"The soft-deleted secret with the name {secretName} has been successfully restored into the Vault.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Purge a Secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage PurgeSecret(string secretName)
        {
            var SecretsRepository = new SecretsRepository();
            SecretsRepository.PurgeSecret(secretName);
            var answer = JsonConvert.SerializeObject($"The secret with the name {secretName} has been successfully purged from the Vault.");

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response;
        }
    }
}