using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace EnhancedDevelopment.ReverseCycleCooler
{
    class PlaceWorker_ReverseCycleCooler : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        {
            
            Map _VisibleMap = Find.VisibleMap;

            IntVec3 loc1 = center + IntVec3Utility.RotatedBy(IntVec3.South, rot);
            IntVec3 loc2 = center + IntVec3Utility.RotatedBy(IntVec3.North, rot);
            GenDraw.DrawFieldEdges(new List<IntVec3>()
      {
        loc1
      }, Color.magenta);
            GenDraw.DrawFieldEdges(new List<IntVec3>()
      {
        loc2
      }, Color.white);
            Room room1 = GridsUtility.GetRoom(loc2, _VisibleMap);
            Room room2 = GridsUtility.GetRoom(loc1, _VisibleMap);
            if (room1 == null || room2 == null)
                return;
            if (room1 == room2 && !room1.UsesOutdoorTemperature)
            {
                GenDraw.DrawFieldEdges(Enumerable.ToList<IntVec3>(room1.Cells), new Color(1f, 0.7f, 0.0f, 0.5f));
            }
            else
            {
                if (!room1.UsesOutdoorTemperature)
                    GenDraw.DrawFieldEdges(Enumerable.ToList<IntVec3>(room1.Cells), Color.white);
                if (room2.UsesOutdoorTemperature)
                    return;
                GenDraw.DrawFieldEdges(Enumerable.ToList<IntVec3>(room2.Cells), Color.magenta);
            }
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            if (GenGrid.Impassable(center + IntVec3Utility.RotatedBy(IntVec3.South, rot), map) || GenGrid.Impassable(center + IntVec3Utility.RotatedBy(IntVec3.North, rot), map))
                return (AcceptanceReport)Translator.Translate("MustPlaceCoolerWithFreeSpaces");
            return (AcceptanceReport)true;
        }
    }
}
