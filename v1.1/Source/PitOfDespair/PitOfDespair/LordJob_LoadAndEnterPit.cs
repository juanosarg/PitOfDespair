using RimWorld;
using Verse;
using Verse.AI.Group;

namespace PitOfDespair
{
    public class LordJob_LoadAndEnterPit : LordJob
    {
        public LordJob_LoadAndEnterPit()
        {
        }

        public LordJob_LoadAndEnterPit(int transportersGroup)
        {
            this.transportersGroup = transportersGroup;
        }

        public override bool AllowStartNewGatherings
        {
            get
            {
                return false;
            }
        }

        public override bool AddFleeToil
        {
            get
            {
                return false;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.transportersGroup, "transportersGroup", 0, false);
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_LoadAndEnterPit startingToil = new LordToil_LoadAndEnterPit(this.transportersGroup);
            stateGraph.StartingToil = startingToil;
            LordToil_End toil = new LordToil_End();
            stateGraph.AddToil(toil);
            return stateGraph;
        }

        public int transportersGroup = -1;
    }
}
