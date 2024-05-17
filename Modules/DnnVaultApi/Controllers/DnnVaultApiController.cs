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

            var response2 = this.Request.CreateResponse(HttpStatusCode.OK);
            response2.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response2;
        }

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
            var answer = JsonConvert.SerializeObject("Fuck yeah!!! That worked!");

            var response2 = this.Request.CreateResponse(HttpStatusCode.OK);
            response2.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response2;
        }
    }
}