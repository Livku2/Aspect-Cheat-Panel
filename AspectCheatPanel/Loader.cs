using UnityEngine;
using HarmonyLib;
using System.Reflection;
namespace Aspect
{
    public class Loader
    {
        private static Harmony harmony;
        public void Load()
        {
            Aspect.Plugin.Plugin.Patched = true;
            if (harmony == null)
            {
                harmony = new Harmony(Aspect.Plugin.Plugin.modGUID);
            }
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            GameObject obj = new GameObject();
            obj.AddComponent<Plugin.Plugin>();
        }
    }
}
