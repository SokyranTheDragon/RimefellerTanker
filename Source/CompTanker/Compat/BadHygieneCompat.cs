using System;
using HarmonyLib;
using Verse;

namespace CompTanker.Compat
{
    [HotSwappable]
    public static class BadHygieneCompat
    {
        public static bool IsActive { get; private set; }

        private static Type compPipeType;
        private static FastInvokeHandler pipeNetGetter;

        private static FastInvokeHandler pushWaterMethod;
        private static FastInvokeHandler pullWaterMethod;

        private static Type mapComponentType;
        private static AccessTools.FieldRef<object, bool> markTowerForDrawField;

        public static void Init()
        {
            if (IsActive) return;

            var type = compPipeType = AccessTools.TypeByName("DubsBadHygiene.CompPipe");
            pipeNetGetter = MethodInvoker.GetHandler(AccessTools.PropertyGetter(type, "pipeNet"));

            type = AccessTools.TypeByName("DubsBadHygiene.PlumbingNet");
            pushWaterMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "PushWater"));
            pullWaterMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "PullWater"));

            type = mapComponentType = AccessTools.TypeByName("DubsBadHygiene.MapComponent_Hygiene");
            markTowerForDrawField = AccessTools.FieldRefAccess<bool>(type, "MarkTowersForDraw");

            IsActive = true;
        }

        public static void HandleTick(CompTanker tanker)
        {
            if (IsActive)
                HandleTickPrivate(tanker);
        }

        private static void HandleTickPrivate(CompTanker tanker)
        {
            var compPipe = tanker.parent.GetComp(compPipeType);
            if (compPipe == null) return;

            if (tanker.isDraining)
            {
                if (tanker.storedAmount <= 0)
                {
                    tanker.isDraining = false;
                    return;
                }

                var num = Math.Min(tanker.storedAmount, tanker.Props.drainAmount);
                if (num > 0)
                {
                    tanker.storedAmount -= num;
                    tanker.storedAmount += (float)pushWaterMethod(pipeNetGetter(compPipe), (float)num);
                }
            }
            else if (tanker.isFilling)
            {
                if (tanker.storedAmount >= tanker.Props.storageCap)
                {
                    tanker.isFilling = false;
                    return;
                }

                var num = Math.Min(tanker.Props.storageCap - tanker.storedAmount, tanker.Props.fillAmount);
                num = Math.Max(num, 0);

                if ((bool)pullWaterMethod(pipeNetGetter(compPipe), (float)num, 0))
                    tanker.storedAmount += num;
            }
        }

        public static void MarkForDrawing(Map map)
        {
            var mapComp = map.GetComp(mapComponentType);
            if (mapComp != null)
                markTowerForDrawField(mapComp) = true;
        }
    }
}
