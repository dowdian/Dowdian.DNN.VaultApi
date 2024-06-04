using System.Collections.Generic;

public class VaultSettingsModel
{
    public string Host { get; set; }
    public string Action { get; set; }
    public Dictionary<string, string> Settings { get; set; }
}
