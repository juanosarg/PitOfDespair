using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace PitOfDespair
{
    public static class PitUtility
    {
        public static void GetTransportersInGroup(int transportersGroup, Map map, List<CompPit> outTransporters)
        {
            outTransporters.Clear();
            if (transportersGroup < 0)
            {
                return;
            }
            List<Thing> list = map.listerThings.ThingsOfDef(ThingDef.Named("PD_PitOfDespair"));
            for (int i = 0; i < list.Count; i++)
            {
                CompPit compTransporter = list[i].TryGetComp<CompPit>();
                if (compTransporter.groupID == transportersGroup)
                {
                    outTransporters.Add(compTransporter);
                }
            }
        }

        public static Lord FindLord(int transportersGroup, Map map)
        {
            List<Lord> lords = map.lordManager.lords;
            for (int i = 0; i < lords.Count; i++)
            {
                LordJob_LoadAndEnterPit lordJob_LoadAndEnterTransporters = lords[i].LordJob as LordJob_LoadAndEnterPit;
                if (lordJob_LoadAndEnterTransporters != null && lordJob_LoadAndEnterTransporters.transportersGroup == transportersGroup)
                {
                    return lords[i];
                }
            }
            return null;
        }

        public static bool WasLoadingCanceled(Thing transporter)
        {
            CompPit compTransporter = transporter.TryGetComp<CompPit>();
            return compTransporter != null && !compTransporter.LoadingInProgressOrReadyToLaunch;
        }
    }
}
