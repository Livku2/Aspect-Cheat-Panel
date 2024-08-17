using BepInEx;
using BepInEx.Configuration;
using System.IO;

namespace Aspect.Plugin
{
    [BepInPlugin("config.manager", "Config Manager", "0.0.1")]
    public class ConfigManager : BaseUnityPlugin
    {
        #region Config Entries
        // General
        public static ConfigEntry<bool> OCULUSMODE;
        public static ConfigEntry<bool> SAFEMODE;

        // Movement
        public static ConfigEntry<float> SUPERMONKEYSPEED;
        public static ConfigEntry<float> IRONMONKEYSPEED;

        // Rig
        public static ConfigEntry<bool> CREATERIG;
        #endregion

        // Load configs when the plugin is getting loaded
        public void Awake()
        {
            // Create custom file
            var customFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "AspectCheatPanel.cfg"), true);

            // General configs
            OCULUSMODE = customFile.Bind("General", "OculusMode", false, "Turns on oculus mode, which means that the menulib will be designed to run on the oculus version.");
            SAFEMODE = customFile.Bind("General", "SafeMode", true, "Turns on 'Anticheat Disconnect' and makes risky mods unavailable.");

            // Movement configs
            SUPERMONKEYSPEED = customFile.Bind("Movement", "SupermonkeySpeed", 16f, "Speed to supermonkey mods.");
            IRONMONKEYSPEED = customFile.Bind("Movement", "IronMonkeySpeed", 20f, "Speed to supermonkey mods.");

            // Rig configs
            CREATERIG = customFile.Bind("Rig", "CreateRig", true, "Create a rig when disabling the active one.");
        }
    }
}
