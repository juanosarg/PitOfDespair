using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace PitOfDespair
{
    public class ITab_FilteredRefuel : ITab
    {
        private IStoreSettingsParent SelStoreSettingsParent
        {
            get
            {
                return ((ThingWithComps)base.SelObject).GetComp<CompFilteredRefuelable>();
            }
        }

        public override bool IsVisible
        {
            get
            {
                return this.SelStoreSettingsParent.StorageTabVisible;
            }
        }

        public ITab_FilteredRefuel()
        {
            this.size = ITab_FilteredRefuel.WinSize;
            this.labelKey = "PD_FeedPrisoners";
        }

        protected override void FillTab()
        {
            IStoreSettingsParent selStoreSettingsParent = this.SelStoreSettingsParent;
            StorageSettings storeSettings = selStoreSettingsParent.GetStoreSettings();
            Rect rect = new Rect(0f, 0f, ITab_FilteredRefuel.WinSize.x, ITab_FilteredRefuel.WinSize.y).ContractedBy(10f);
            GUI.BeginGroup(rect);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(rect)
            {
                height = 32f
            }, "PD_FeedPrisonersLabel".Translate());
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            ThingFilter thingFilter = null;
            bool flag = selStoreSettingsParent.GetParentStoreSettings() != null;
            if (flag)
            {
                thingFilter = selStoreSettingsParent.GetParentStoreSettings().filter;
            }
            Rect rect2 = new Rect(0f, 40f, rect.width, rect.height - 40f);
            ThingFilterUI.DoThingFilterConfigWindow(rect2, ref this.scrollPosition, storeSettings.filter, thingFilter, 8, null, null, false, null, null);
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.StorageTab, KnowledgeAmount.FrameDisplayed);
            GUI.EndGroup();
        }

        private const float TopAreaHeight = 35f;

        private Vector2 scrollPosition = default(Vector2);

        private static readonly Vector2 WinSize = new Vector2(300f, 480f);
    }
}
