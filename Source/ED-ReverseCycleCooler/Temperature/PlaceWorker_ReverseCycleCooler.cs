using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EnhancedDevelopment.ReverseCycleCooler
{
    class PlaceWorker_ReverseCycleCooler : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol)
        {
            Map currentMap = Find.CurrentMap;
            IntVec3 intVec = center + IntVec3.South.RotatedBy(rot);
            IntVec3 intVec2 = center + IntVec3.North.RotatedBy(rot);
            List<IntVec3> list = new List<IntVec3>
            {
                intVec
            };
            GenDraw.DrawFieldEdges(list, Color.magenta);
            list = new List<IntVec3>
            {
                intVec2
            };
            GenDraw.DrawFieldEdges(list, Color.white);
            RoomGroup roomGroup = intVec2.GetRoomGroup(currentMap);
            RoomGroup roomGroup2 = intVec.GetRoomGroup(currentMap);
            if (roomGroup != null && roomGroup2 != null)
            {
                if (roomGroup == roomGroup2 && !roomGroup.UsesOutdoorTemperature)
                {
                    GenDraw.DrawFieldEdges(roomGroup.Cells.ToList(), new Color(1f, 0.7f, 0f, 0.5f));
                }
                else
                {
                    if (!roomGroup.UsesOutdoorTemperature)
                    {
                        GenDraw.DrawFieldEdges(roomGroup.Cells.ToList(), Color.white);
                    }
                    if (!roomGroup2.UsesOutdoorTemperature)
                    {
                        GenDraw.DrawFieldEdges(roomGroup2.Cells.ToList(), Color.magenta);
                    }
                }
            }
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            IntVec3 c = center + IntVec3.South.RotatedBy(rot);
            IntVec3 c2 = center + IntVec3.North.RotatedBy(rot);
            if (!c.Impassable(map) && !c2.Impassable(map))
            {
                return true;
            }
            return "MustPlaceCoolerWithFreeSpaces".Translate();
        }
    }
}
