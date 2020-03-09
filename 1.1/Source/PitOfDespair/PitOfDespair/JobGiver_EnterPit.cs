using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PitOfDespair
{
    public class JobGiver_EnterPit : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            //Log.Message("Trying to give job");
            int transportersGroup = pawn.mindState.duty.transportersGroup;
            List<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                if (allPawnsSpawned[i] != pawn && allPawnsSpawned[i].CurJobDef == DefDatabase<JobDef>.GetNamed("PD_HaulToPit"))
                {
                    CompPit transporter = ((JobDriver_HaulToPit)allPawnsSpawned[i].jobs.curDriver).Transporter;
                    if (transporter != null && transporter.groupID == transportersGroup)
                    {
                        return null;
                    }
                }
            }
            PitUtility.GetTransportersInGroup(transportersGroup, pawn.Map, JobGiver_EnterPit.tmpTransporters);
            CompPit compTransporter = JobGiver_EnterPit.FindMyTransporter(JobGiver_EnterPit.tmpTransporters, pawn);
            JobGiver_EnterPit.tmpTransporters.Clear();
            if (compTransporter == null || !pawn.CanReach(compTransporter.parent, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
            {
                return null;
            }
            return new Job(DefDatabase<JobDef>.GetNamed("PD_EnterPit"), compTransporter.parent);
        }

        public static CompPit FindMyTransporter(List<CompPit> transporters, Pawn me)
        {
            for (int i = 0; i < transporters.Count; i++)
            {
                List<TransferableOneWay> leftToLoad = transporters[i].leftToLoad;
                if (leftToLoad != null)
                {
                    for (int j = 0; j < leftToLoad.Count; j++)
                    {
                        if (leftToLoad[j].AnyThing is Pawn)
                        {
                            List<Thing> things = leftToLoad[j].things;
                            for (int k = 0; k < things.Count; k++)
                            {
                                if (things[k] == me)
                                {
                                    return transporters[i];
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static List<CompPit> tmpTransporters = new List<CompPit>();
    }
}
