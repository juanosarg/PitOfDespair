using RimWorld;
using Verse;
using Verse.AI;

namespace PitOfDespair
{
    public class JobDriver_HaulToPit : JobDriver_HaulToContainer
    {
        public CompPit Transporter
        {
            get
            {
                return (base.Container == null) ? null : base.Container.TryGetComp<CompPit>();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.initialCount, "initialCount", 0, false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.A), this.job, 1, -1, null);
            this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.B), this.job, 1, -1, null);
            return true;
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            ThingCount thingCount = LoadPitJobUtility.FindThingToLoad(this.pawn, base.Container.TryGetComp<CompPit>());
            this.job.targetA = thingCount.Thing;
            this.job.count = thingCount.Count;
            this.initialCount = thingCount.Count;
            this.pawn.Reserve(thingCount.Thing, this.job, 1, -1, null, true);
            foreach (Pawn p in this.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
            {
                bool flag3 = p.needs != null && p.needs.mood != null && p.needs.mood.thoughts != null;
                if (flag3)
                {
                    p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("PD_ThrownPrisonerIntoPit"), null);
                    p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("PD_ThrownPrisonerIntoPitImLovinIt"), null);

                }
            }
        }

        public int initialCount;
    }
}
