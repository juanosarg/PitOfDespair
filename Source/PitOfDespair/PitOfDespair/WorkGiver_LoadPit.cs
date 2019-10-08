using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace PitOfDespair
{
    public class WorkGiver_LoadPit : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(ThingDef.Named("PD_PitOfDespair"));
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompPit transporter = t.TryGetComp<CompPit>();
            return LoadPitJobUtility.HasJobOnTransporter(pawn, transporter);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompPit transporter = t.TryGetComp<CompPit>();
            return LoadPitJobUtility.JobOnTransporter(pawn, transporter);
        }
    }
}
