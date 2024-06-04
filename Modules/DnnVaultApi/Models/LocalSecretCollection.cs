// <copyright file="SecretElementCollection.cs" company="Dowdian SRL">
// Copyright (c) Dowdian SRL. All rights reserved.
// </copyright>

using System;
using System.Configuration;
using System.Runtime.CompilerServices;

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

        internal bool Add(LocalSecretElement newSecret)
        {
            if (this.Contains(newSecret.Name))
            {
                return false;
            }

            this.BaseAdd(newSecret);

            return true;
        }

        internal LocalSecretElement Get(string name)
        {
            foreach (LocalSecretElement key in this)
            {
                if (key.Name == name)
                {
                    return key;
                }
            }

            return null;
        }

        internal bool Update(LocalSecretElement secret)
        {
            foreach (LocalSecretElement key in this)
            {
                if (key.Name == secret.Name)
                {
                    key.Secret = secret.Secret;
                    return true;
                }
            }

            return false;
        }

        internal void Delete(string name)
        {
            foreach (LocalSecretElement key in this)
            {
                if (key.Name == name)
                {
                    key.ElementInformation.Properties["Deleted"].Value = true;
                    return;
                }
            }
        }

        internal bool Purge(string name)
        {
            foreach (LocalSecretElement key in this)
            {
                if (key.Name == name)
                {
                    this.BaseRemove(key);
                    return true;
                }
            }

            return false;
        }

        internal bool Restore(string name)
        {
            foreach (LocalSecretElement key in this)
            {
                if (key.Name == name)
                {
                    key.ElementInformation.Properties["Deleted"].Value = false;
                    return true;
                }
            }

            return false;
        }

        internal bool Contains(string name)
        {
            foreach (LocalSecretElement key in this)
            {
                if (key.Name == name)
                {
                    return true;
                }
            }

            return false;
        }
    }
}