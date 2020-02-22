using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PitOfDespair
{
    [StaticConstructorOnStartup]
    public class Command_LoadPit : Command
    {
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            if (this.transporters == null)
            {
                this.transporters = new List<CompPit>();
            }
            if (!this.transporters.Contains(this.transComp))
            {
                this.transporters.Add(this.transComp);
            }
           
            for (int j = 0; j < this.transporters.Count; j++)
            {
                if (this.transporters[j] != this.transComp)
                {
                    if (!this.transComp.Map.reachability.CanReach(this.transComp.parent.Position, this.transporters[j].parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
                    {
                        Messages.Message("MessageTransporterUnreachable".Translate(), this.transporters[j].parent, MessageTypeDefOf.RejectInput, false);
                        return;
                    }
                }
                transporters[j].prisonerthrowingdone = true;
            }
            
            Find.WindowStack.Add(new Dialog_LoadPit(this.transComp.Map, this.transporters));
        }

        public override bool InheritInteractionsFrom(Gizmo other)
        {
            Command_LoadPit command_LoadToTransporter = (Command_LoadPit)other;
            if (command_LoadToTransporter.transComp.parent.def != this.transComp.parent.def)
            {
                return false;
            }
            if (this.transporters == null)
            {
                this.transporters = new List<CompPit>();
            }
            this.transporters.Add(command_LoadToTransporter.transComp);
            return false;
        }

        public CompPit transComp;

        private List<CompPit> transporters;

        private static HashSet<Building> tmpFuelingPortGivers = new HashSet<Building>();
    }
}
