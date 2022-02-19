using System;
using System.Collections;
using System.Linq;
using HarmonyLib;
using Verse;

namespace CompTanker.Compat
{
    [HotSwappable]
    public static class VanillaFurnitureExpandedPowerCompat
    {
        public static bool IsActive { get; private set; }

        private static Type compGasType;
        private static FastInvokeHandler gasNetGetter;

        private static FastInvokeHandler storeGasMethod;
        private static FastInvokeHandler drawGasMethod;
        private static AccessTools.FieldRef<object, IList> storagesField;

        private static AccessTools.FieldRef<object, float> storedField;

        private static AccessTools.FieldRef<object, int> capacityField;

        public static void Init()
        {
            if (IsActive) return;

            var type = compGasType = AccessTools.TypeByName("GasNetwork.CompGas");
            gasNetGetter = MethodInvoker.GetHandler(AccessTools.PropertyGetter(type, "Network"));

            type = AccessTools.TypeByName("GasNetwork.GasNet");
            storeGasMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "Store"));
            drawGasMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "Draw"));
            storagesField = AccessTools.FieldRefAccess<IList>(type, "storages");

            storedField = AccessTools.FieldRefAccess<float>(AccessTools.TypeByName("GasNetwork.CompGasStorage"), "_stored");

            capacityField = AccessTools.FieldRefAccess<int>(AccessTools.TypeByName("GasNetwork.CompProperties_GasStorage"), "capacity");

            IsActive = true;
        }

        public static void HandleTick(CompTanker tanker)
        {
            if (IsActive)
                HandleTickPrivate(tanker);
        }

        private static void HandleTickPrivate(CompTanker tanker)
        {
            var compPipe = tanker.parent.GetComp(compGasType);
            if (compPipe == null) return;

            if (tanker.isDraining)
            {
                if (tanker.storedAmount <= 0)
                {
                    tanker.isDraining = false;
                    return;
                }

                var num = Math.Min(tanker.storedAmount, tanker.Props.drainAmount);
                num = Math.Min(num, storagesField(gasNetGetter(compPipe)).Cast<ThingComp>().Sum(s => capacityField(s.props) - storedField(s)));
                num = Math.Max(num, 0);
                if (num > 0)
                {
                    tanker.storedAmount -= num;
                    storeGasMethod(gasNetGetter(compPipe), (float)num);
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
                num = Math.Min(num, storagesField(gasNetGetter(compPipe)).Cast<ThingComp>().Sum(s => storedField(s)));
                num = Math.Max(num, 0);
                if (num > 0)
                {
                    tanker.storedAmount += num;
                    drawGasMethod(gasNetGetter(compPipe), (float)num);
                }
            }
        }
    }
}
