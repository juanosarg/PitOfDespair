using RimWorld;
using Verse;

namespace PitOfDespair
{
    public class CompProperties_Pit : CompProperties
    {
        public CompProperties_Pit()
        {
            this.compClass = typeof(CompPit);
        }

        public float massCapacity = 150f;

        public float restEffectiveness;
        public int maxPrisoners = 0;

    }
}