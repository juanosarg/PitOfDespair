using RimWorld;
using Verse.AI;
using Verse.AI.Group;
using Verse;


namespace PitOfDespair
{
    public class LordToil_LoadAndEnterPit : LordToil
    {
        public LordToil_LoadAndEnterPit(int transportersGroup)
        {
            this.transportersGroup = transportersGroup;
        }

        public override bool AllowSatisfyLongNeeds
        {
            get
            {
                return false;
            }
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                PawnDuty pawnDuty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("PD_LoadAndEnterPit"));
                pawnDuty.transportersGroup = this.transportersGroup;
                this.lord.ownedPawns[i].mindState.duty = pawnDuty;
            }
        }

        private int transportersGroup = -1;
    }
}