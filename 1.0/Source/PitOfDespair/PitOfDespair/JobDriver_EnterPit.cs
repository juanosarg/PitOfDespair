﻿using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PitOfDespair
{
    public class JobDriver_EnterPit : JobDriver
    {
        public CompPit Transporter
        {
            get
            {
                Thing thing = this.job.GetTarget(this.TransporterInd).Thing;
                if (thing == null)
                {
                    return null;
                }
                return thing.TryGetComp<CompPit>();
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(this.TransporterInd);
            this.FailOn(() => !this.Transporter.LoadingInProgressOrReadyToLaunch);
            yield return Toils_Goto.GotoThing(this.TransporterInd, PathEndMode.Touch);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    CompPit transporter = this.Transporter;
                    this.pawn.DeSpawn(DestroyMode.Vanish);
                    transporter.GetDirectlyHeldThings().TryAdd(this.pawn, true);
                }
            };
            yield break;
        }

        private TargetIndex TransporterInd = TargetIndex.A;
    }
}
