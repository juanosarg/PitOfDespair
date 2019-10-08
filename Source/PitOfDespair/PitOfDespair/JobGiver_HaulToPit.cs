using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PitOfDespair
{
    public class JobGiver_HaulToPit : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            int transportersGroup = pawn.mindState.duty.transportersGroup;
            PitUtility.GetTransportersInGroup(transportersGroup, pawn.Map, JobGiver_HaulToPit.tmpTransporters);
            for (int i = 0; i < JobGiver_HaulToPit.tmpTransporters.Count; i++)
            {
                CompPit transporter = JobGiver_HaulToPit.tmpTransporters[i];
                if (LoadPitJobUtility.HasJobOnTransporter(pawn, transporter))
                {
                    return LoadPitJobUtility.JobOnTransporter(pawn, transporter);
                }
            }
            return null;
        }

        private static List<CompPit> tmpTransporters = new List<CompPit>();
    }
}
