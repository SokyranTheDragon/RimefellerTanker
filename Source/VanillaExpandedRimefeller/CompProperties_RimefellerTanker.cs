using Rimefeller;
using Verse;

namespace RimefellerTanker
{
    internal class CompProperties_RimefellerTanker : CompProperties
    {
        public TankClass contents = TankClass.Fuel;
        public double storageCap = 10000;
        public double fillAmount = 0.5;
        public double drainAmount = 0.5;

        public CompProperties_RimefellerTanker() => compClass = typeof(CompRimefellerTanker);
    }
}
