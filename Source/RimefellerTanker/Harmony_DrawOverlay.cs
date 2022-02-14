using HarmonyLib;
using Rimefeller;
using Verse;

namespace RimefellerTanker
{
    [HarmonyPatch(typeof(MapComponent_Rimefeller), nameof(MapComponent_Rimefeller.MapComponentUpdate))]
    internal static class Harmony_DrawOverlay
    {
        public static void Prefix(MapComponent_Rimefeller __instance)
        {
            if (!__instance.MarkTowersForDraw) return;

            foreach (var pipeNet in __instance.PipeNets)
            {
                foreach (var thing in pipeNet.PipedThings)
                {
                    var comp = thing.GetComp<CompRimefellerTanker>();
                    if (comp != null) comp.drawOverlay = true;
                }
            }
        }
    }
}
