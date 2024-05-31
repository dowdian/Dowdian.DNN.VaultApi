using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace Dowdian.Modules.DnnVaultApi.Providers
{
    public class CertificateProvider
    {
        public X509Certificate2 FindCertificateByThumbprint(string thumbprint)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (col == null || col.Count == 0)
                {
                    throw new Exception("ERROR: Certificate not found with thumbprint");
                }
                return col[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                store.Close();
            }
        }
    }
}