using System.Configuration;

public class ConfigurationProtectedDataSection : ConfigurationSection
{
    [ConfigurationProperty("providers", IsDefaultCollection = false)]
    [ConfigurationCollection(typeof(ProviderSettingsCollection),
        AddItemName = "add",
        ClearItemsName = "clear",
        RemoveItemName = "remove")]
    public ProviderSettingsCollection Providers
    {
        get
        {
            return (ProviderSettingsCollection)base["providers"];
        }
    }
}
