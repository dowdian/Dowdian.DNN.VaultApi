// <copyright file="DowdianApiController.cs" company="Dowdian SRL">
// Copyright (c) Dowdian SRL. All rights reserved.
// </copyright>

using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DotNetNuke.Web.Api;
using Dowdian.Modules.DnnVaultApi.Providers;
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
        public HttpResponseMessage GetSecret()
        {
            // authenticate to Azure Key Vault using the certificate with thumbprint "76AB32E6B23CEE6881C7B4AABF068E44D76750A6"
            var cert = AzureKeyVaultCertificationHelper.FindCertificateByThumbprint("76AB32E6B23CEE6881C7B4AABF068E44D76750A6");
            var client = new SecretClient(new Uri("https://dowdiankeyvault.vault.azure.net/"), new ClientCertificateCredential(ConfigurationManager.AppSettings["TenantId"], ConfigurationManager.AppSettings["ClientApplicationId"], cert));

            // get the secret value for "DowdianTestValue"
            KeyVaultSecret secret = client.GetSecret("DowdianTestValue");
            string secretValue = secret.Value;

            var answer = JsonConvert.SerializeObject(secret);

            var response2 = this.Request.CreateResponse(HttpStatusCode.OK);
            response2.Content = new StringContent(answer, System.Text.Encoding.UTF8, "application/json");
            return response2;
        }


    }
}