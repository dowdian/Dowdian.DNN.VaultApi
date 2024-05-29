using System.Configuration;
using System.Web.Configuration;
using System.Xml;
using DotNetNuke.Entities.Modules;

namespace Dowdian.Modules.DnnVaultApi.Components
{
    /// <summary>
    /// 
    /// </summary>
    public class DnnVaultApiController : IUpgradeable
    {
        /// <summary>
        /// UpgradeModule
        /// </summary>
        /// <param name="version">string version</param>
        /// <returns>string</returns>
        public string UpgradeModule(string version)
        {
            // https://www.dnnsoftware.com/wiki/iupgradeable
            // https://www.dnnsoftware.com/forums/threadid/498462/scope/posts/how-to-run-code-on-extension-installation-upgrade
            // https://www.rahulsingla.com/blog/2014/10/dnn-implementing-iupgradeable-for-a-modules-business-controller-class/
            // https://github.com/dowdian/PolyDeploy/blob/master/PolyDeploy/Components/FeatureController.cs
            string returnMessage;

            switch (version)
            {
                case "01.00.00":
                    // Configure the application for first time use.
                    returnMessage = this.Upgrade_01_00_00();
                    break;

                default:
                    returnMessage = $"No upgrade logic for {version}.";
                    break;
            }

            return returnMessage;
        }

        private string Upgrade_01_00_00()
        {
            // Configure the application for first time use.
            //Add the assemblyBinding nodes to the web.config file.
            Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
            ConfigurationSection runtimeSection = config.GetSection("runtime");
            if (runtimeSection != null)
            {
                // Get the XML representation of the runtime section.
                var runtimeXml = new XmlDocument();
                runtimeXml.LoadXml(runtimeSection.SectionInformation.GetRawXml());

                // Create a namespace manager to handle the XML namespaces.
                var nsManager = new XmlNamespaceManager(runtimeXml.NameTable);
                nsManager.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");

                // Get the assemblyBinding element.
                var assemblyBindingElement = runtimeXml.SelectSingleNode("/runtime/asm:assemblyBinding", nsManager);

                if (assemblyBindingElement != null)
                {
                    // Store the current InnerXml.
                    var currentInnerXml = assemblyBindingElement.InnerXml;

                    // Clear the InnerXml.
                    assemblyBindingElement.InnerXml = "";

                    // Prepend the new dependentAssembly elements.
                    PrependNewNodeIfNotExists(assemblyBindingElement, "System.Text.Encodings.Web", "cc7b13ffcd2ddd51", "0.0.0.0-32767.32767.32767.32767", "4.0.5.1", nsManager);
                    PrependNewNodeIfNotExists(assemblyBindingElement, "System.Threading.Tasks.Extensions", "cc7b13ffcd2ddd51", "0.0.0.0-32767.32767.32767.32767", "4.2.0.1", nsManager);
                    PrependNewNodeIfNotExists(assemblyBindingElement, "Microsoft.Identity.Client", "0a613f4dd989e8ae", "0.0.0.0-32767.32767.32767.32767", "4.61.0.0", nsManager);
                    PrependNewNodeIfNotExists(assemblyBindingElement, "Microsoft.Bcl.AsyncInterfaces", "cc7b13ffcd2ddd51", "0.0.0.0-32767.32767.32767.32767", "8.0.0.0", nsManager);
                    PrependNewNodeIfNotExists(assemblyBindingElement, "System.Runtime.CompilerServices.Unsafe", "b03f5f7f11d50a3a", "0.0.0.0-6.0.0.0", "6.0.0.0", nsManager);
                    PrependNewNodeIfNotExists(assemblyBindingElement, "System.Diagnostics.DiagnosticSource", "cc7b13ffcd2ddd51", "0.0.0.0-8.0.0.1", "8.0.0.1", nsManager);
                    PrependNewNodeIfNotExists(assemblyBindingElement, "System.Memory", "cc7b13ffcd2ddd51", "0.0.0.0-4.0.1.2", "4.0.1.2", nsManager);
                    PrependNewNodeIfNotExists(assemblyBindingElement, "System.Web.Http", "31bf3856ad364e35", "0.0.0.0-32767.32767.32767.32767", "5.3.0.0", nsManager);
                    PrependNewNodeIfNotExists(assemblyBindingElement, "Azure.Core", "92742159e12e44c8", "0.0.0.0-32767.32767.32767.32767", "1.39.0.0", nsManager);

                    // Append the original InnerXml.
                    assemblyBindingElement.InnerXml += currentInnerXml;

                    // Update the raw XML of the runtime section.
                    runtimeSection.SectionInformation.SetRawXml(runtimeXml.OuterXml);

                    // Save the changes to the Web.config file.
                    config.Save();
                }
            }
            return "Configured the application for first time use.";
        }

        private void PrependNewNodeIfNotExists(XmlNode assemblyBindingElement, string name, string token, string oldVersion, string newVersion, XmlNamespaceManager nsManager)
        {
            var existingNode = assemblyBindingElement.SelectSingleNode($"asm:dependentAssembly[asm:assemblyIdentity/@name='{name}' and asm:bindingRedirect/@newVersion='{newVersion}']", nsManager);

            if (existingNode == null)
            {
                assemblyBindingElement.InnerXml = MyNewNode(name, token, oldVersion, newVersion) + assemblyBindingElement.InnerXml;
            }
        }

        private string MyNewNode(string name, string token, string oldVersion, string newVersion)
        {
            return $"<dependentAssembly>" +
                   $"<assemblyIdentity name=\"{name}\" publicKeyToken=\"{token}\" culture=\"neutral\" />" +
                   $"<bindingRedirect oldVersion=\"{oldVersion}\" newVersion=\"{newVersion}\" />" +
                   "</dependentAssembly>";
        }
    }
}