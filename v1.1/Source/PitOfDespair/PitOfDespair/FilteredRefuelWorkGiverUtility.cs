using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace PitOfDespair
{
    public static class FilteredRefuelWorkGiverUtility
    {
        public static bool CanRefuel(Pawn pawn, Thing t, bool forced = false)
        {
            CompFilteredRefuelable compRefuelable = t.TryGetComp<CompFilteredRefuelable>();
            bool flag2 = compRefuelable == null || compRefuelable.IsFull;
            bool result;
            if (flag2)
            {
                result = false;
            }
            else
            {
                bool flag = !forced;
                bool flag3 = flag && !compRefuelable.ShouldAutoRefuelNow;
                if (flag3)
                {
                    result = false;
                }
                else
                {
                    bool flag4 = !t.IsForbidden(pawn);
                    if (flag4)
                    {
                        LocalTargetInfo target = t;
                        bool flag5 = pawn.CanReserve(target, 1, -1, null, forced);
                        if (flag5)
                        {
                            bool flag6 = t.Faction != pawn.Faction;
                            if (flag6)
                            {
                                return false;
                            }
                            bool flag7 = FilteredRefuelWorkGiverUtility.FindBestFuel(pawn, t) == null;
                            if (flag7)
                            {
                                ThingFilter fuelFilter = t.TryGetComp<CompFilteredRefuelable>().FuelFilter;
                                JobFailReason.Is("PD_NoFood".Translate(fuelFilter.Summary), null);
                                return false;
                            }
                            bool flag8 = t.TryGetComp<CompFilteredRefuelable>().Props.atomicFueling && FilteredRefuelWorkGiverUtility.FindAllFuel(pawn, t) == null;
                            if (flag8)
                            {
                                ThingFilter fuelFilter2 = t.TryGetComp<CompFilteredRefuelable>().FuelFilter;
                                JobFailReason.Is("PD_NoFood".Translate(fuelFilter2.Summary), null);
                                return false;
                            }
                            return true;
                        }
                    }
                    result = false;
                }
            }
            return result;
        }

        public static Job RefuelJob(Pawn pawn, Thing t, bool forced = false, JobDef customRefuelJob = null, JobDef customAtomicRefuelJob = null)
        {
            bool flag = !t.TryGetComp<CompFilteredRefuelable>().Props.atomicFueling;
            Job result;
            if (flag)
            {
                Thing t2 = FilteredRefuelWorkGiverUtility.FindBestFuel(pawn, t);
                result = new Job(customRefuelJob ?? JobDefOf.Refuel, t, t2);
            }
            else
            {
                List<Thing> source = FilteredRefuelWorkGiverUtility.FindAllFuel(pawn, t);
                Job job = new Job(customAtomicRefuelJob ?? JobDefOf.RefuelAtomic, t);
                job.targetQueueB = (from f in source
                                    select new LocalTargetInfo(f)).ToList<LocalTargetInfo>();
                result = job;
            }
            return result;
        }

        private static Thing FindBestFuel(Pawn pawn, Thing refuelable)
        {
            ThingFilter filter = refuelable.TryGetComp<CompFilteredRefuelable>().FuelFilter;
            Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && filter.Allows(x);
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            ThingRequest bestThingRequest = filter.BestThingRequest;
            PathEndMode peMode = PathEndMode.ClosestTouch;
            TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            Predicate<Thing> validator = predicate;
            return GenClosest.ClosestThingReachable(position, map, bestThingRequest, peMode, traverseParams, 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        }

        private static List<Thing> FindAllFuel(Pawn pawn, Thing refuelable)
        {
            int quantity = refuelable.TryGetComp<CompFilteredRefuelable>().GetFuelCountToFullyRefuel();
            ThingFilter filter = refuelable.TryGetComp<CompFilteredRefuelable>().FuelFilter;
            Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && filter.Allows(x);
            IntVec3 position = refuelable.Position;
            Region region = position.GetRegion(pawn.Map, RegionType.Set_Passable);
            TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParams, false);
            List<Thing> chosenThings = new List<Thing>();
            int accumulatedQuantity = 0;
            RegionProcessor regionProcessor = delegate (Region r)
            {
                List<Thing> list = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
                for (int i = 0; i < list.Count; i++)
                {
                    Thing thing = list[i];
                    bool flag2 = validator(thing);
                    if (flag2)
                    {
                        bool flag3 = !chosenThings.Contains(thing);
                        if (flag3)
                        {
                            bool flag4 = ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, PathEndMode.ClosestTouch, pawn);
                            if (flag4)
                            {
                                chosenThings.Add(thing);
                                accumulatedQuantity += thing.stackCount;
                                bool flag5 = accumulatedQuantity >= quantity;
                                if (flag5)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            };
            RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 99999, RegionType.Set_Passable);
            bool flag = accumulatedQuantity >= quantity;
            List<Thing> result;
            if (flag)
            {
                result = chosenThings;
            }
            else
            {
                result = null;
            }
            return result;
        }
    }
}
