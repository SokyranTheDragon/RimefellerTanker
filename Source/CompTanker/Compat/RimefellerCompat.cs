using System;
using HarmonyLib;
using Verse;

namespace CompTanker.Compat
{
    [HotSwappable]
    public static class RimefellerCompat
    {
        public static bool IsActive { get; private set; }

        private static Type compPipeType;
        private static FastInvokeHandler pipeNetGetter;

        private static FastInvokeHandler pushFuelMethod;
        private static FastInvokeHandler pullFuelMethod;
        private static FastInvokeHandler pushOilMethod;
        private static FastInvokeHandler pullOilMethod;

        private static Type mapComponentType;
        private static AccessTools.FieldRef<object, bool> markTowerForDrawField;

        public static void Init()
        {
            if (IsActive) return;

            var type = compPipeType = AccessTools.TypeByName("Rimefeller.CompPipe");
            pipeNetGetter = MethodInvoker.GetHandler(AccessTools.PropertyGetter(type, "pipeNet"));

            type = AccessTools.TypeByName("Rimefeller.PipelineNet");
            pushFuelMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "PushFuel"));
            pullFuelMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "PullFuel"));
            pushOilMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "PushCrude"));
            pullOilMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "PullOil"));

            type = mapComponentType = AccessTools.TypeByName("Rimefeller.MapComponent_Rimefeller");
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
                    tanker.storedAmount += tanker.Props.contents switch
                    {
                        TankType.Fuel => (double)pushFuelMethod(pipeNetGetter(compPipe), num),
                        TankType.Oil => (double)pushOilMethod(pipeNetGetter(compPipe), num),
                        _ => num,
                    };
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

                var success = tanker.Props.contents switch
                {
                    TankType.Fuel => (bool)pullFuelMethod(pipeNetGetter(compPipe), num),
                    TankType.Oil => (bool)pullOilMethod(pipeNetGetter(compPipe), num),
                    _ => false,
                };

                if (success)
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
