using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Enhanced_Development.Temperature
{
    public class Building_ReverseCycleCooler : Building_Cooler
    {
        private static Texture2D UI_ROTATE_RIGHT;
        
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            UI_ROTATE_RIGHT = ContentFinder<Texture2D>.Get("UI/RotRight", true);
        }

       /* public override void TickRare()
        {
            if (!this.compPowerTrader.PowerOn)
            {
                return;
            }
            IntVec3 intVec3_1 = this.Position + Gen.RotatedBy(IntVec3.south, this.Rotation);
            IntVec3 intVec3_2 = this.Position + Gen.RotatedBy(IntVec3.north, this.Rotation);

        }*/

        public override IEnumerable<Gizmo> GetGizmos()
        {
            //Add the stock Gizmoes
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }


            if (true)
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.ChangeRotation();
                act.icon = UI_ROTATE_RIGHT;
                act.defaultLabel = "Rotate";
                act.defaultDesc = "Rotates";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }
        }


        /*
        public override IEnumerable<Command> GetCommands()
        {
            IList<Command> CommandList = new List<Command>();
            IEnumerable<Command> baseCommands = base.GetCommands();

            if (baseCommands != null)
            {
                CommandList = baseCommands.ToList();
            }

            if (true)
            {
                //Upgrading
                Command_Action command_Action_AddResources = new Command_Action();

                command_Action_AddResources.defaultLabel = "Rotate";

                command_Action_AddResources.icon = UI_ADD_RESOURCES;
                command_Action_AddResources.defaultDesc = "Rotate";

                command_Action_AddResources.activateSound = SoundDef.Named("Click");
                command_Action_AddResources.action = new Action(this.ChangeRotation);

                CommandList.Add(command_Action_AddResources);
            }


            return CommandList.AsEnumerable<Command>();
            //return compPowerTrader.CompGetCommandsExtra();
            //return base.GetCommands();
        }
        */
        public void ChangeRotation()
        {
            //Log.Error("Rotation");

            if (this.Rotation.AsInt == 3)
            {
                this.Rotation = new Rot4(0);
            }
            else
            {

                this.Rotation = new Rot4(this.Rotation.AsInt + 1);
            }


            // Tell the MapDrawer that here is something thats changed
            Find.MapDrawer.MapMeshDirty(Position, MapMeshFlag.Things, true, false);
            
            //this.Rotation.Rotate(RotationDirection.Clockwise);
            //this.Rotation.Rotate(RotationDirection.Clockwise);

        }

    }
}
