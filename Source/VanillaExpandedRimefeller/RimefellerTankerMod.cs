using HarmonyLib;
using JetBrains.Annotations;
using Multiplayer.API;
using Verse;

namespace RimefellerTanker
{
    [UsedImplicitly]
    public class RimefellerTankerMod : Mod
    {
        public RimefellerTankerMod(ModContentPack content) : base(content)
        {
            if (MP.enabled) MP.RegisterAll();
            new Harmony("RimefellerTanker.RimefellerTankerMod").PatchAll();

#if DEBUG
            ReferenceBuilder.Restore(content);
#endif
        }
    }
}