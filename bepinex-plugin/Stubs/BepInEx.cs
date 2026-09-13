namespace BepInEx
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class BepInPluginAttribute : System.Attribute
    {
        public BepInPluginAttribute(string guid, string name, string version)
        {
        }
    }

    public class BaseUnityPlugin : UnityEngine.MonoBehaviour
    {
        public BepInEx.Configuration.ConfigFile Config { get; } = new BepInEx.Configuration.ConfigFile("stub.cfg");
        public BepInEx.Logging.Logger Logger { get; } = new BepInEx.Logging.Logger();
    }

    namespace Configuration
    {
        public class ConfigFile
        {
            public ConfigFile(string path)
            {
            }

            public ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string? description = null)
            {
                return new ConfigEntry<T>(defaultValue);
            }
        }

        public class ConfigEntry<T>
        {
            public ConfigEntry(T value)
            {
                Value = value;
            }

            public T Value { get; set; }
        }
    }

    namespace Logging
    {
        public class Logger
        {
            public void LogInfo(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message) { }
        }
    }
}

namespace UnityEngine
{
    public static class Mathf
    {
        public static float Max(float a, float b) => a > b ? a : b;
    }

    public class MonoBehaviour
    {
        public void StartCoroutine(System.Collections.IEnumerator routine)
        {
        }
    }

    public class WaitForSeconds
    {
        public WaitForSeconds(float seconds)
        {
        }
    }

    public class ZNet
    {
        public static ZNet instance { get; } = new ZNet();

        public System.Collections.Generic.List<PlayerInfo> GetPlayerList() => new System.Collections.Generic.List<PlayerInfo>();
    }

    public class PlayerInfo
    {
        public string m_name { get; set; } = string.Empty;
    }
}
