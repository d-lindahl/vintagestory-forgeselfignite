using HarmonyLib;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace forgeselfignite
{
    public class Core : ModSystem
    {
        private Harmony harmony;

        public override void Start(ICoreAPI api)
        {
            try
            {
                ForgeSelfIgniteConfig forgeSelfIgniteConfig = api.LoadModConfig<ForgeSelfIgniteConfig>("forgeSelfIgniteConfig.json");
                if (forgeSelfIgniteConfig != null)
                {
                    api.Logger.Notification("Mod Config successfully loaded.");
                    ForgeSelfIgniteConfig.Current = forgeSelfIgniteConfig;
                }
                else
                {
                    api.Logger.Notification("No Mod Config specified. Falling back to default settings");
                    ForgeSelfIgniteConfig.Current = ForgeSelfIgniteConfig.GetDefault();
                }
            }
            catch
            {
                ForgeSelfIgniteConfig.Current = ForgeSelfIgniteConfig.GetDefault();
                api.Logger.Error("Failed to load custom mod configuration. Falling back to default settings!");
            }
            finally
            {
                api.StoreModConfig<ForgeSelfIgniteConfig>(ForgeSelfIgniteConfig.Current, "forgeSelfIgniteConfig.json");
            }
            this.harmony = new Harmony("quaan.forgeSelfIgnite");
            this.harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override void Dispose()
        {
            this.harmony.UnpatchAll(this.harmony.Id);
            base.Dispose();
        }
    }

    public class ForgeSelfIgniteConfig
    {
        public static ForgeSelfIgniteConfig Current { get; set; }

        public int forgeSelfIgniteTemperature;

        public static ForgeSelfIgniteConfig GetDefault()
        {
            return new ForgeSelfIgniteConfig()
            {
                forgeSelfIgniteTemperature = 500
            };
        }
    }

    [HarmonyPatch(typeof(BlockEntityForge))]
    public class ForgeSelfIgnitePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnCommonTick", new Type[] { typeof(float) })]
        public static void SelfIgniteIfFueldAndHot(BlockEntityForge __instance, float dt)
        {
            if (__instance.Api.Side == EnumAppSide.Server)
            {
                if (__instance.Contents != null && __instance.CanIgnite && __instance.Contents.Collectible.GetTemperature(__instance.Api.World, __instance.Contents) > ForgeSelfIgniteConfig.Current.forgeSelfIgniteTemperature)
                {
                    __instance.GetType().GetMethod("TryIgnite", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, null);
                }
            }
        }
    }
}
