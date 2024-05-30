// <copyright file="SecretElementCollection.cs" company="Dowdian SRL">
// Copyright (c) Dowdian SRL. All rights reserved.
// </copyright>

using System.Configuration;

namespace Dowdian.Modules.DnnVaultApi.Models
{
    /// <summary>
    /// Extend the ConfigurationElementCollection class.
    /// Decorate the class with the class that represents a single element in the collection.
    /// </summary>
    [ConfigurationCollection(typeof(LocalSecretElement))]
    public class LocalSecretCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// SecurityKeyElement
        /// </summary>
        /// <param name="index">int index</param>
        /// <returns>Security Key Element</returns>
        public LocalSecretElement this[int index]
        {
            get
            {
                return (LocalSecretElement)this.BaseGet(index);
            }

            set
            {
                if (this.BaseGet(index) != null)
                {
                    this.BaseRemoveAt(index);
                }

                this.BaseAdd(index, value);
            }
        }

        /// <summary>
        /// ConfigurationElement
        /// </summary>
        /// <returns>Configuration Element</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new LocalSecretElement();
        }

        /// <summary>
        /// GetElementKey
        /// </summary>
        /// <param name="element">ConfigurationElement element</param>
        /// <returns>object</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LocalSecretElement)element).Name;
        }
    }
}