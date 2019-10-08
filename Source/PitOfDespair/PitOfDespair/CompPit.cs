using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace PitOfDespair
{
    [StaticConstructorOnStartup]
    public class CompPit : ThingComp, IThingHolder
    {

        public int groupID = -1;

        public ThingOwner innerContainer;

        public List<TransferableOneWay> leftToLoad;

        public string buildingGod = "none";

        private CompLaunchable cachedCompLaunchable;

        private bool notifiedCantLoadMore;

        public bool prisonerthrowingdone = false;

       

        private static readonly Texture2D CancelLoadCommandTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);

        private static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/PD_ThrowPrisoner", true);

        private static readonly Texture2D TentacledAspectTex = ContentFinder<Texture2D>.Get("UI/Commands/PD_CommandTentacled", true);

        private static readonly Texture2D CatAspectTex = ContentFinder<Texture2D>.Get("UI/Commands/PD_CommandCats", true);

        private static readonly Texture2D NormalAspectTex = ContentFinder<Texture2D>.Get("UI/Commands/PD_CommandNormal", true);


        private static List<CompPit> tmpTransportersInGroup = new List<CompPit>();

        public CompPit()
        {
            this.innerContainer = new ThingOwner<Thing>(this);
            
        }

        public CompProperties_Pit Props
        {
            get
            {
                return (CompProperties_Pit)this.props;
            }
        }

        public Map Map
        {
            get
            {
                return this.parent.MapHeld;
            }
        }

        public bool AnythingLeftToLoad
        {
            get
            {
                return this.FirstThingLeftToLoad != null;
            }
        }

        public bool LoadingInProgressOrReadyToLaunch
        {
            get
            {
                return this.groupID >= 0;
            }
        }

        public bool AnyInGroupHasAnythingLeftToLoad
        {
            get
            {
                return this.FirstThingLeftToLoadInGroup != null;
            }
        }

        public CompLaunchable Launchable
        {
            get
            {
                if (this.cachedCompLaunchable == null)
                {
                    this.cachedCompLaunchable = this.parent.GetComp<CompLaunchable>();
                }
                return this.cachedCompLaunchable;
            }
        }

        public Thing FirstThingLeftToLoad
        {
            get
            {
                if (this.leftToLoad == null)
                {
                    return null;
                }
                for (int i = 0; i < this.leftToLoad.Count; i++)
                {
                    if (this.leftToLoad[i].CountToTransfer != 0 && this.leftToLoad[i].HasAnyThing)
                    {
                        return this.leftToLoad[i].AnyThing;
                    }
                }
                return null;
            }
        }

        public Thing FirstThingLeftToLoadInGroup
        {
            get
            {
                List<CompPit> list = this.TransportersInGroup(this.parent.Map);
                for (int i = 0; i < list.Count; i++)
                {
                    Thing firstThingLeftToLoad = list[i].FirstThingLeftToLoad;
                    if (firstThingLeftToLoad != null)
                    {
                        return firstThingLeftToLoad;
                    }
                }
                return null;
            }
        }

        public bool AnyInGroupNotifiedCantLoadMore
        {
            get
            {
                List<CompPit> list = this.TransportersInGroup(this.parent.Map);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].notifiedCantLoadMore)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool AnyPawnCanLoadAnythingNow
        {
            get
            {
                if (!this.AnythingLeftToLoad)
                {
                    return false;
                }
                if (!this.parent.Spawned)
                {
                    return false;
                }
                List<Pawn> allPawnsSpawned = this.parent.Map.mapPawns.AllPawnsSpawned;
                for (int i = 0; i < allPawnsSpawned.Count; i++)
                {
                    if (allPawnsSpawned[i].CurJobDef == DefDatabase<JobDef>.GetNamed("PD_HaulToPit"))
                    {
                        CompPit transporter = ((JobDriver_HaulToPit)allPawnsSpawned[i].jobs.curDriver).Transporter;
                        if (transporter != null && transporter.groupID == this.groupID)
                        {
                            return true;
                        }
                    }
                    if (allPawnsSpawned[i].CurJobDef == DefDatabase<JobDef>.GetNamed("PD_EnterPit"))
                    {
                        CompPit transporter2 = ((JobDriver_EnterPit)allPawnsSpawned[i].jobs.curDriver).Transporter;
                        if (transporter2 != null && transporter2.groupID == this.groupID)
                        {
                            return true;
                        }
                    }
                }
                List<CompPit> list = this.TransportersInGroup(this.parent.Map);
                for (int j = 0; j < allPawnsSpawned.Count; j++)
                {
                    if (allPawnsSpawned[j].mindState.duty != null && allPawnsSpawned[j].mindState.duty.transportersGroup == this.groupID)
                    {
                        CompPit compTransporter = JobGiver_EnterPit.FindMyTransporter(list, allPawnsSpawned[j]);
                        if (compTransporter != null && allPawnsSpawned[j].CanReach(compTransporter.parent, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
                        {
                            return true;
                        }
                    }
                }
                for (int k = 0; k < allPawnsSpawned.Count; k++)
                {
                    if (allPawnsSpawned[k].IsColonist)
                    {
                        for (int l = 0; l < list.Count; l++)
                        {
                            if (LoadPitJobUtility.HasJobOnTransporter(allPawnsSpawned[k], list[l]))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.groupID, "groupID", 0, false);
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Collections.Look<TransferableOneWay>(ref this.leftToLoad, "leftToLoad", LookMode.Deep, new object[0]);
            Scribe_Values.Look<bool>(ref this.notifiedCantLoadMore, "notifiedCantLoadMore", false, false);
            Scribe_Values.Look<bool>(ref this.prisonerthrowingdone, "prisonerthrowingdone", false, false);

        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public override void CompTick()
        {
            //Log.Message(this.leftToLoad.Count.ToString());
            base.CompTick();
            this.innerContainer.ThingOwnerTick(true);
            if (this.parent.IsHashIntervalTick(60)) {

               
                    for (int i = 0; i < this.innerContainer.Count; i++)
                    {
                        Pawn pawn = this.innerContainer[i] as Pawn;
                        if (pawn != null && !pawn.Dead && pawn.needs.food != null)
                        {
                            if (this.parent.GetComp<CompRefuelable>().HasFuel) {
                                pawn.needs.food.CurLevel = 0.1f;
                            }
                            else {
                                pawn.needs.food.CurLevel = pawn.needs.food.CurLevel * 0.99f;
                            }
                           
                            
                        }
                    }

                
                
            }
           

            /*if (this.Props.restEffectiveness != 0f)
            {
                for (int i = 0; i < this.innerContainer.Count; i++)
                {
                    Pawn pawn = this.innerContainer[i] as Pawn;
                    if (pawn != null && !pawn.Dead && pawn.needs.rest != null)
                    {
                        pawn.needs.rest.TickResting(this.Props.restEffectiveness);
                    }
                }
            }*/
            if (this.parent.IsHashIntervalTick(60) && this.parent.Spawned && this.LoadingInProgressOrReadyToLaunch && this.AnyInGroupHasAnythingLeftToLoad && !this.AnyInGroupNotifiedCantLoadMore && !this.AnyPawnCanLoadAnythingNow)
            {
                //this.notifiedCantLoadMore = true;
               
               

                Messages.Message("PD_FinishedLoadingPit".Translate(), this.parent, MessageTypeDefOf.CautionInput, true);
                this.leftToLoad.Clear();

                prisonerthrowingdone = false;
            }
        }

        public List<CompPit> TransportersInGroup(Map map)
        {
            if (!this.LoadingInProgressOrReadyToLaunch)
            {
                return null;
            }
            PitUtility.GetTransportersInGroup(this.groupID, map, CompPit.tmpTransportersInGroup);
            return CompPit.tmpTransportersInGroup;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
            if (!prisonerthrowingdone)
            {
                Command_LoadPit loadGroup = new Command_LoadPit();
                int selectedTransportersCount = 0;
                for (int i = 0; i < Find.Selector.NumSelected; i++)
                {
                    Thing thing = Find.Selector.SelectedObjectsListForReading[i] as Thing;
                    if (thing != null && thing.def == this.parent.def)
                    {
                        CompLaunchable compLaunchable = thing.TryGetComp<CompLaunchable>();
                        if (compLaunchable == null || (compLaunchable.FuelingPortSource != null && compLaunchable.FuelingPortSourceHasAnyFuel))
                        {
                            selectedTransportersCount++;
                        }
                    }
                }
                loadGroup.defaultLabel = "PD_ThrowPrisoner".Translate();
                loadGroup.defaultDesc = "PD_ThrowPrisonerDesc".Translate();
                loadGroup.icon = CompPit.LoadCommandTex;
                loadGroup.transComp = this;
               
                
                yield return loadGroup;
            }

            Command_Action PD_ChangeAspectTentacled = new Command_Action();
            PD_ChangeAspectTentacled.action = delegate
            {
                Messages.Message("PD_ConsecratingCthulhu".Translate(), this.parent, MessageTypeDefOf.CautionInput, true);
                this.buildingGod = "cthulhu";
                DoSmokePuff();
                Graphic graphic=this.parent.Graphic;
            };
            PD_ChangeAspectTentacled.defaultLabel = "PD_ConsecrateCthulhu".Translate();
            PD_ChangeAspectTentacled.defaultDesc = "PD_ConsecrateCthulhuDesc".Translate();
            PD_ChangeAspectTentacled.icon = TentacledAspectTex;
            yield return PD_ChangeAspectTentacled;

            Command_Action PD_ChangeAspectCats = new Command_Action();
            PD_ChangeAspectCats.action = delegate
            {
                Messages.Message("PD_ConsecratingBast".Translate(), this.parent, MessageTypeDefOf.CautionInput, true);
                this.buildingGod = "bast";
                DoSmokePuff();
                Graphic graphic = this.parent.Graphic;

            };
            PD_ChangeAspectCats.defaultLabel = "PD_ConsecrateBast".Translate();
            PD_ChangeAspectCats.defaultDesc = "PD_ConsecrateBastDesc".Translate();
            PD_ChangeAspectCats.icon = CatAspectTex;
            yield return PD_ChangeAspectCats;

            Command_Action PD_ChangeAspectNone = new Command_Action();
            PD_ChangeAspectNone.action = delegate
            {
                Messages.Message("PD_UnConsecrating".Translate(), this.parent, MessageTypeDefOf.CautionInput, true);
                this.buildingGod = "none";
                DoSmokePuff();
                Graphic graphic = this.parent.Graphic;

            };
            PD_ChangeAspectNone.defaultLabel = "PD_UnConsecrate".Translate();
            PD_ChangeAspectNone.defaultDesc = "PD_UnConsecrateDesc".Translate();
            PD_ChangeAspectNone.icon = NormalAspectTex;
            yield return PD_ChangeAspectNone;

            yield break;
        }

        public void DoSmokePuff()
        {
            IntVec3 position = this.parent.Position;
            Map map = this.parent.Map;
            float statValue = 1.5f;
            DamageDef smoke = DamageDefOf.Smoke;
            Thing instigator = null;
            ThingDef gas_Smoke = ThingDefOf.Gas_Smoke;
            GenExplosion.DoExplosion(position, map, statValue, smoke, instigator, -1, -1f, null, null, null, null, gas_Smoke, 1f, 1, false, null, 0f, 1, 0f, false);

        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (this.CancelLoad(map))
            {
                Messages.Message("PD_ThrowPrisonerCancelled".Translate(), MessageTypeDefOf.NegativeEvent, true);
            }
            


            this.innerContainer.TryDropAll(this.parent.Position, map, ThingPlaceMode.Near, null, null);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
          
        }

        public override string CompInspectStringExtra()
        {
            return "PD_PrisonersInside".Translate() + ": " + this.innerContainer.ContentsString.CapitalizeFirst();
        }

        public void AddToTheToLoadList(TransferableOneWay t, int count)
        {
            if (!t.HasAnyThing || t.CountToTransfer <= 0)
            {
                return;
            }
            if (this.leftToLoad == null)
            {
                this.leftToLoad = new List<TransferableOneWay>();
            }
            if (TransferableUtility.TransferableMatching<TransferableOneWay>(t.AnyThing, this.leftToLoad, TransferAsOneMode.PodsOrCaravanPacking) != null)
            {
                Log.Error("Transferable already exists.", false);
                return;
            }
            TransferableOneWay transferableOneWay = new TransferableOneWay();
            this.leftToLoad.Add(transferableOneWay);
            transferableOneWay.things.AddRange(t.things);
            transferableOneWay.AdjustTo(count);
        }

        public void Notify_ThingAdded(Thing t)
        {
            this.SubtractFromToLoadList(t, t.stackCount);
        }

        public void Notify_ThingAddedAndMergedWith(Thing t, int mergedCount)
        {
            this.SubtractFromToLoadList(t, mergedCount);
        }

        public bool CancelLoad()
        {
            return this.CancelLoad(this.Map);
        }

        public bool CancelLoad(Map map)
        {
            if (!this.LoadingInProgressOrReadyToLaunch)
            {
                return false;
            }
            this.TryRemoveLord(map);
            List<CompPit> list = this.TransportersInGroup(map);
            for (int i = 0; i < list.Count; i++)
            {
                list[i].CleanUpLoadingVars(map);
            }
            this.CleanUpLoadingVars(map);
            return true;
        }

        public void TryRemoveLord(Map map)
        {
            if (!this.LoadingInProgressOrReadyToLaunch)
            {
                return;
            }
            Lord lord = PitUtility.FindLord(this.groupID, map);
            if (lord != null)
            {
                map.lordManager.RemoveLord(lord);
            }
        }

        public void CleanUpLoadingVars(Map map)
        {
            this.groupID = -1;
            this.innerContainer.TryDropAll(this.parent.Position, map, ThingPlaceMode.Near, null, null);
            if (this.leftToLoad != null)
            {
                this.leftToLoad.Clear();
            }
        }

        private void SubtractFromToLoadList(Thing t, int count)
        {
            if (this.leftToLoad == null)
            {
                return;
            }
            TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatchingDesperate(t, this.leftToLoad, TransferAsOneMode.PodsOrCaravanPacking);
            if (transferableOneWay == null)
            {
                return;
            }
            transferableOneWay.AdjustBy(-count);
            if (transferableOneWay.CountToTransfer <= 0)
            {
                this.leftToLoad.Remove(transferableOneWay);
            }
            if (!this.AnyInGroupHasAnythingLeftToLoad)
            {
                Messages.Message("PD_FinishedLoadingPit".Translate(), this.parent, MessageTypeDefOf.TaskCompletion, true);
                prisonerthrowingdone = false;

            }
        }

        private void SelectPreviousInGroup()
        {
            List<CompPit> list = this.TransportersInGroup(this.Map);
            int num = list.IndexOf(this);
            CameraJumper.TryJumpAndSelect(list[GenMath.PositiveMod(num - 1, list.Count)].parent);
        }

        private void SelectAllInGroup()
        {
            List<CompPit> list = this.TransportersInGroup(this.Map);
            Selector selector = Find.Selector;
            selector.ClearSelection();
            for (int i = 0; i < list.Count; i++)
            {
                selector.Select(list[i].parent, true, true);
            }
        }

        private void SelectNextInGroup()
        {
            List<CompPit> list = this.TransportersInGroup(this.Map);
            int num = list.IndexOf(this);
            CameraJumper.TryJumpAndSelect(list[(num + 1) % list.Count].parent);
        }

       
    }
}
