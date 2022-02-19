using System.Linq;
using CompTanker.Compat;
using HarmonyLib;
using JetBrains.Annotations;
using Multiplayer.API;
using Verse;

namespace CompTanker
{
    [UsedImplicitly]
    public class CompTankerMod : Mod
    {
        public CompTankerMod(ModContentPack content) : base(content)
        {
            if (MP.enabled)
                MP.RegisterAll();

            if (IsModLoaded("dubwise.dubsbadhygiene"))
                BadHygieneCompat.Init();
            if (IsModLoaded("dubwise.rimefeller"))
                RimefellerCompat.Init();
            if (IsModLoaded("vanillaexpanded.vfepower"))
                VanillaFurnitureExpandedPowerCompat.Init();

            LongEventHandler.ExecuteWhenFinished(() => new Harmony("Dra.CompTankerMod").PatchAll());

#if DEBUG
            ReferenceBuilder.Restore(content);
#endif
        }

        private static bool IsModLoaded(string s)
            => LoadedModManager.RunningMods
                .Any(x => x.PackageId.ToLower().NoModIdSuffix() == s);
    }
}