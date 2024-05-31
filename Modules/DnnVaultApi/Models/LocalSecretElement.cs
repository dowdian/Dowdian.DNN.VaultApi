// <copyright file="QuidProElementCollection.cs" company="Dowdian SRL">
// Copyright (c) Dowdian SRL. All rights reserved.
// </copyright>

using System.Configuration;

namespace Dowdian.Modules.DnnVaultApi.Models
{
    /// <summary>
    /// Extend the ConfigurationElement class.  This class represents a single element in the collection.
    /// Create a property for each xml attribute in your element.
    /// Decorate each property with the ConfigurationProperty decorator.  See MSDN for all available options.
    /// </summary>
    public class LocalSecretElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets Secret
        /// </summary>
        [ConfigurationProperty("key", IsRequired = true)]
        public string Name
        {
            get { return (string)this["key"]; }
            set { this["key"] = value; }
        }

        /// <summary>
        /// Gets or sets name
        /// </summary>
        [ConfigurationProperty("value", IsRequired = true)]
        public string Secret
        {
            get { return (string)this["value"]; }
            set { this["value"] = value; }
        }
    }
}