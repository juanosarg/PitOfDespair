using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PitOfDespair
{
    public static class LoadPitJobUtility
    {
        public static bool HasJobOnTransporter(Pawn pawn, CompPit transporter)
        {
            return  transporter.AnythingLeftToLoad && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && pawn.CanReach(transporter.parent, PathEndMode.Touch, pawn.NormalMaxDanger(), false, TraverseMode.ByPawn) && LoadPitJobUtility.FindThingToLoad(pawn, transporter).Thing != null;
        }

        public static Job JobOnTransporter(Pawn p, CompPit transporter)
        {
            return new Job(DefDatabase<JobDef>.GetNamed("PD_HaulToPit"), LocalTargetInfo.Invalid, transporter.parent)
            {
                ignoreForbidden = true
            };
        }

        public static ThingCount FindThingToLoad(Pawn p, CompPit transporter)
        {
            LoadPitJobUtility.neededThings.Clear();
            List<TransferableOneWay> leftToLoad = transporter.leftToLoad;
            LoadPitJobUtility.tmpAlreadyLoading.Clear();
           // Log.Message(leftToLoad.ToString());
            if (leftToLoad != null)
            {
                List<Pawn> allPawnsSpawned = transporter.Map.mapPawns.AllPawnsSpawned;
                for (int i = 0; i < allPawnsSpawned.Count; i++)
                {
                    if (allPawnsSpawned[i] != p)
                    {
                        if (allPawnsSpawned[i].CurJobDef == DefDatabase<JobDef>.GetNamed("PD_HaulToPit"))
                        {
                            JobDriver_HaulToPit jobDriver_HaulToTransporter = (JobDriver_HaulToPit)allPawnsSpawned[i].jobs.curDriver;
                            if (jobDriver_HaulToTransporter.Container == transporter.parent)
                            {
                                TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatchingDesperate(jobDriver_HaulToTransporter.ThingToCarry, leftToLoad, TransferAsOneMode.PodsOrCaravanPacking);
                                if (transferableOneWay != null)
                                {
                                    int num = 0;
                                    if (LoadPitJobUtility.tmpAlreadyLoading.TryGetValue(transferableOneWay, out num))
                                    {
                                        LoadPitJobUtility.tmpAlreadyLoading[transferableOneWay] = num + jobDriver_HaulToTransporter.initialCount;
                                    }
                                    else
                                    {
                                        LoadPitJobUtility.tmpAlreadyLoading.Add(transferableOneWay, jobDriver_HaulToTransporter.initialCount);
                                    }
                                }
                            }
                        }
                    }
                }
                for (int j = 0; j < leftToLoad.Count; j++)
                {
                    TransferableOneWay transferableOneWay2 = leftToLoad[j];
                    int num2;
                    if (!LoadPitJobUtility.tmpAlreadyLoading.TryGetValue(leftToLoad[j], out num2))
                    {
                        num2 = 0;
                    }
                    if (transferableOneWay2.CountToTransfer - num2 > 0)
                    {
                        for (int k = 0; k < transferableOneWay2.things.Count; k++)
                        {
                            LoadPitJobUtility.neededThings.Add(transferableOneWay2.things[k]);
                        }
                    }
                }
            }
            if (!LoadPitJobUtility.neededThings.Any<Thing>())
            {
                LoadPitJobUtility.tmpAlreadyLoading.Clear();
                return default(ThingCount);
            }
            Thing thing = GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.Touch, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, (Thing x) => LoadPitJobUtility.neededThings.Contains(x) && p.CanReserve(x, 1, -1, null, false), null, 0, -1, false, RegionType.Set_Passable, false);
            if (thing == null)
            {
                foreach (Thing thing2 in LoadPitJobUtility.neededThings)
                {
                    Pawn pawn = thing2 as Pawn;
                    if (pawn != null && pawn.IsPrisoner && p.CanReserveAndReach(pawn, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
                    {
                        LoadPitJobUtility.neededThings.Clear();
                        LoadPitJobUtility.tmpAlreadyLoading.Clear();
                        return new ThingCount(pawn, 1);
                    }
                }
            }
            LoadPitJobUtility.neededThings.Clear();
            if (thing != null)
            {
                TransferableOneWay transferableOneWay3 = null;
                for (int l = 0; l < leftToLoad.Count; l++)
                {
                    if (leftToLoad[l].things.Contains(thing))
                    {
                        transferableOneWay3 = leftToLoad[l];
                        break;
                    }
                }
                int num3;
                if (!LoadPitJobUtility.tmpAlreadyLoading.TryGetValue(transferableOneWay3, out num3))
                {
                    num3 = 0;
                }
                LoadPitJobUtility.tmpAlreadyLoading.Clear();
                return new ThingCount(thing, Mathf.Min(transferableOneWay3.CountToTransfer - num3, thing.stackCount));
            }
            LoadPitJobUtility.tmpAlreadyLoading.Clear();
            return default(ThingCount);
        }

        private static HashSet<Thing> neededThings = new HashSet<Thing>();

        private static Dictionary<TransferableOneWay, int> tmpAlreadyLoading = new Dictionary<TransferableOneWay, int>();
    }
}
