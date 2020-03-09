using System;
using RimWorld;
using Verse;

namespace PitOfDespair
{
    public class CompFilteredRefuelable : CompRefuelable, IStoreSettingsParent
    {


        public StorageSettings inputSettings;

        private CompFlickable flickComp;

        private Building_Pit pit;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
           // this.flickComp = this.parent.GetComp<CompFlickable>();
            bool flag = this.inputSettings == null;
            if (flag)
            {
                this.inputSettings = new StorageSettings(this);
                bool flag2 = this.parent.def.building.defaultStorageSettings != null;
                if (flag2)
                {
                    this.inputSettings.CopyFrom(this.parent.def.building.defaultStorageSettings);
                }
            }
            this.pit = (Building_Pit)this.parent;
        }

        public override string CompInspectStringExtra()
        {
            string text = string.Concat(new string[]
                        {
                this.Props.FuelLabel,
                ": ",
                this.Fuel.ToStringDecimalIfSmall(),
                " / ",
                this.Props.fuelCapacity.ToStringDecimalIfSmall()
                        });
            if (!this.Props.consumeFuelOnlyWhenUsed && this.HasFuel)
            {
                int numTicks = (int)(this.Fuel / this.Props.fuelConsumptionRate * 60000f);
                text = text + " (" + numTicks.ToStringTicksToPeriod() + ")";
            }
            if (!this.HasFuel && !this.Props.outOfFuelMessage.NullOrEmpty())
            {
                text += string.Format("\n{0} ({1}x {2})", this.Props.outOfFuelMessage, this.GetFuelCountToFullyRefuel(), this.Props.fuelFilter.AnyAllowedDef.label);
            }
            if (this.Props.targetFuelLevelConfigurable)
            {
                text = text + "\n" + "ConfiguredTargetFuelLevel".Translate(this.TargetFuelLevel.ToStringDecimalIfSmall());
            }
            bool flagPawns = false;
            for (int i = 0; i < this.pit.GetComp<CompPit>().innerContainer.Count; i++)
            {
                Pawn pawn = this.pit.GetComp<CompPit>().innerContainer[i] as Pawn;
                if (pawn != null && !pawn.Dead && pawn.needs.food != null)
                {
                    flagPawns = true;
                }
            }
            if (!this.HasFuel && flagPawns)
            {
                text = text + "\n"+ "PD_NoFoodInThePit".Translate();
            }
            return text;
           
          
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look<StorageSettings>(ref this.inputSettings, "inputSettings", new object[0]);
        }

        public override void CompTick()
        {
            bool flagPawns = false;
            for (int i = 0; i < this.pit.GetComp<CompPit>().innerContainer.Count; i++)
            {
                Pawn pawn = this.pit.GetComp<CompPit>().innerContainer[i] as Pawn;
                if (pawn != null && !pawn.Dead && pawn.needs.food != null)
                {
                    flagPawns = true;
                }
            }


            bool flag = !base.Props.consumeFuelOnlyWhenUsed && this.pit != null && flagPawns;
            if (flag)
            {
                base.ConsumeFuel(this.ConsumptionRatePerTick);
            }
        }

        private float ConsumptionRatePerTick
        {
            get
            {
                return base.Props.fuelConsumptionRate / 60000f;
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
        }

        public StorageSettings GetStoreSettings()
        {
            return this.inputSettings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return this.parent.def.building.fixedStorageSettings;
        }

        public bool StorageTabVisible
        {
            get
            {
                return true;
            }
        }

        public ThingFilter FuelFilter
        {
            get
            {
                return this.inputSettings.filter;
            }
        }


    }
}
