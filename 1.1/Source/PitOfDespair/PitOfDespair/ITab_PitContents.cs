using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace PitOfDespair
{
    public class ITab_PitContents : ITab
    {
        public ITab_PitContents()
        {
            this.size = new Vector2(520f, 450f);
            this.labelKey = "PD_TabPrisonerContents";
        }

        public CompPit Transporter
        {
            get
            {
                return base.SelThing.TryGetComp<CompPit>();
            }
        }

        public override bool IsVisible
        {
            get
            {
                return this.Transporter != null && (this.Transporter.LoadingInProgressOrReadyToLaunch || this.Transporter.innerContainer.Any);
            }
        }

        protected override void FillTab()
        {
            this.thingsToSelect.Clear();
            Rect outRect = new Rect(default(Vector2), this.size).ContractedBy(10f);
            outRect.yMin += 20f;
            Rect rect = new Rect(0f, 0f, outRect.width - 16f, Mathf.Max(this.lastDrawnHeight, outRect.height));
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, rect, true);
            float num = 0f;
            this.DoItemsLists(rect, ref num);
            this.lastDrawnHeight = num;
            Widgets.EndScrollView();
            if (this.thingsToSelect.Any<Thing>())
            {
                ITab_Pawn_FormingCaravan.SelectNow(this.thingsToSelect);
                this.thingsToSelect.Clear();
            }
        }

        private void DoItemsLists(Rect inRect, ref float curY)
        {
            CompPit transporter = this.Transporter;
            Rect position = new Rect(0f, curY, (inRect.width - 10f) / 2f, inRect.height);
            float a = 0f;
           /* GUI.BeginGroup(position);
            Widgets.ListSeparator(ref a, position.width, "ItemsToLoad".Translate());
            bool flag = false;
            if (transporter.leftToLoad != null)
            {
                for (int i = 0; i < transporter.leftToLoad.Count; i++)
                {
                    TransferableOneWay t = transporter.leftToLoad[i];
                    if (t.CountToTransfer > 0 && t.HasAnyThing)
                    {
                        flag = true;
                        this.DoThingRow(t.ThingDef, t.CountToTransfer, t.things, position.width, ref a, delegate (int x)
                        {
                            t.ForceTo(t.CountToTransfer - x);
                            this.EndJobForEveryoneHauling(t);
                        });
                    }
                }
            }
            if (!flag)
            {
                Widgets.NoneLabel(ref a, position.width, null);
            }
            GUI.EndGroup();*/
            Rect position2 = new Rect(0f, curY, (inRect.width - 10f), inRect.height);
            float b = 0f;
            GUI.BeginGroup(position2);
            Widgets.ListSeparator(ref b, position2.width, "PD_PrisonersInThePit".Translate());
            bool flag2 = false;
            for (int j = 0; j < transporter.innerContainer.Count; j++)
            {
                Thing t = transporter.innerContainer[j];
                flag2 = true;
                ITab_PitContents.tmpSingleThing.Clear();
                ITab_PitContents.tmpSingleThing.Add(t);
                this.DoThingRow(t.def, t.stackCount, ITab_PitContents.tmpSingleThing, position2.width, ref b, delegate (int x)
                {
                    Thing thing;
                    GenDrop.TryDropSpawn(t.SplitOff(x), this.SelThing.Position, this.SelThing.Map, ThingPlaceMode.Near, out thing, null, null);
                    Pawn pawn = t as Pawn;
                    System.Random rand = new System.Random();
                    if (pawn != null)
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("PD_ThrownIntoPit"), null);
                        HealthUtility.DamageUntilDowned(pawn, false);
                        BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
                        double number = rand.NextDouble();
                        if (number > 0.75 && number < 0.85) {
                           
                            Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Dementia, pawn, brain);
                            if (!pawn.health.WouldDieAfterAddingHediff(hediff))
                            {
                                pawn.health.AddHediff(hediff, null, null, null);
                            }
                        } else if (number > 0.85 && number < 0.95)
                        {
                            Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("PD_Psychosis"), pawn, brain);
                            if (!pawn.health.WouldDieAfterAddingHediff(hediff))
                            {
                                pawn.health.AddHediff(hediff, null, null, null);
                            }
                        }
                        else if (number > 0.95)
                        {
                            Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("PD_BrainEatingParasites"), pawn, brain);
                            if (!pawn.health.WouldDieAfterAddingHediff(hediff))
                            {
                                pawn.health.AddHediff(hediff, null, null, null);
                            }
                        }
                    }
                });
                ITab_PitContents.tmpSingleThing.Clear();
            }
            if (!flag2)
            {
                Widgets.NoneLabel(ref b, position.width, null);
            }
            GUI.EndGroup();
            curY += Mathf.Max(a, b);
        }

        private void SelectLater(List<Thing> things)
        {
            this.thingsToSelect.Clear();
            this.thingsToSelect.AddRange(things);
        }

        private void DoThingRow(ThingDef thingDef, int count, List<Thing> things, float width, ref float curY, Action<int> discardAction)
        {
            Rect rect = new Rect(0f, curY, width, 28f);
            if (count != 1)
            {
                Rect butRect = new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f);
                if (Widgets.ButtonImage(butRect, CaravanThingsTabUtility.AbandonSpecificCountButtonTex))
                {
                    Find.WindowStack.Add(new Dialog_Slider("RemoveSliderText".Translate(thingDef.label), 1, count, discardAction, int.MinValue));
                }
            }
            rect.width -= 24f;
            Rect butRect2 = new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f);
            if (Widgets.ButtonImage(butRect2, CaravanThingsTabUtility.AbandonButtonTex))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmRemoveItemDialog".Translate(thingDef.label), delegate
                {
                    discardAction(count);
                }, false, null));
            }
            rect.width -= 24f;
            if (things.Count == 1)
            {
                Widgets.InfoCardButton(rect.width - 24f, curY, things[0]);
            }
            else
            {
                Widgets.InfoCardButton(rect.width - 24f, curY, thingDef);
            }
            rect.width -= 24f;
            if (Mouse.IsOver(rect))
            {
                GUI.color = ITab_PitContents.ThingHighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            if (thingDef.DrawMatSingle != null && thingDef.DrawMatSingle.mainTexture != null)
            {
                Rect rect2 = new Rect(4f, curY, 28f, 28f);
                if (things.Count == 1)
                {
                    Widgets.ThingIcon(rect2, things[0], 1f);
                }
                else
                {
                    Widgets.ThingIcon(rect2, thingDef);
                }
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_PitContents.ThingLabelColor;
            Rect rect3 = new Rect(36f, curY, rect.width - 36f, rect.height);
            string str;
            if (things.Count == 1)
            {
                str = things[0].LabelCap;
            }
            else
            {
                str = GenLabel.ThingLabel(thingDef, null, count).CapitalizeFirst();
            }
            Text.WordWrap = false;
            Widgets.Label(rect3, str.Truncate(rect3.width, null));
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect, str);
            if (Widgets.ButtonInvisible(rect, false))
            {
                this.SelectLater(things);
            }
            if (Mouse.IsOver(rect))
            {
                for (int i = 0; i < things.Count; i++)
                {
                    TargetHighlighter.Highlight(things[i], true, true, false);
                }
            }
            curY += 28f;
        }

        private void EndJobForEveryoneHauling(TransferableOneWay t)
        {
            List<Pawn> allPawnsSpawned = base.SelThing.Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                if (allPawnsSpawned[i].CurJobDef == DefDatabase<JobDef>.GetNamed("PD_HaulToPit"))
                {
                    JobDriver_HaulToPit jobDriver_HaulToTransporter = (JobDriver_HaulToPit)allPawnsSpawned[i].jobs.curDriver;
                    if (jobDriver_HaulToTransporter.Transporter == this.Transporter)
                    {
                        if (jobDriver_HaulToTransporter.ThingToCarry != null && jobDriver_HaulToTransporter.ThingToCarry.def == t.ThingDef)
                        {
                            allPawnsSpawned[i].jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                        }
                    }
                }
            }
        }

        private Vector2 scrollPosition;

        private float lastDrawnHeight;

        private List<Thing> thingsToSelect = new List<Thing>();

        private static List<Thing> tmpSingleThing = new List<Thing>();

        private const float TopPadding = 20f;

        private const float SpaceBetweenItemsLists = 10f;

        private const float ThingRowHeight = 28f;

        private const float ThingIconSize = 28f;

        private const float ThingLeftX = 36f;

        private static readonly Color ThingLabelColor = ITab_Pawn_Gear.ThingLabelColor;

        private static readonly Color ThingHighlightColor = ITab_Pawn_Gear.HighlightColor;
    }
}
