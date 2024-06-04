using System;
using System.Configuration;

public class ProviderSettingsCollection : ConfigurationElementCollection
{
    protected override ConfigurationElement CreateNewElement()
    {
        return new ProviderSettings();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
        return ((ProviderSettings)element).Name;
    }

    internal ProviderSettings Get(string v)
    {
        return (ProviderSettings)BaseGet(v);
    }

    internal void Add(ConfigurationElement v)
    {
        BaseAdd(v);
    }
}
