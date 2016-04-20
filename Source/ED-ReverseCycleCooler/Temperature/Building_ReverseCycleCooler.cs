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
        private static Texture2D UI_TEMPERATURE_COOLING;
        private static Texture2D UI_TEMPERATURE_HEATING;
        private static Texture2D UI_TEMPERATURE_AUTO;
        
        private const float HeatOutputMultiplier = 1.25f;
        private const float EfficiencyLossPerDegreeDifference = 0.007692308f;

        //Change to enum later?
        private enumCoolerMode m_Mode = enumCoolerMode.Cooling;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            UI_ROTATE_RIGHT = ContentFinder<Texture2D>.Get("UI/RotRight", true);
            UI_TEMPERATURE_COOLING = ContentFinder<Texture2D>.Get("UI/Temperature_Cooling", true);
            UI_TEMPERATURE_HEATING = ContentFinder<Texture2D>.Get("UI/Temperature_Heating", true);
            UI_TEMPERATURE_AUTO = ContentFinder<Texture2D>.Get("UI/Temperature_Auto", true);
        }

        public override void TickRare()
        {
            if (!this.compPowerTrader.PowerOn)
            {
                return;
            }

            IntVec3 intVec3_1 = this.Position + Gen.RotatedBy(IntVec3.South, this.Rotation);
            IntVec3 intVec3_2 = this.Position + Gen.RotatedBy(IntVec3.North, this.Rotation);
            bool flag = false;
            if (!GenGrid.Impassable(intVec3_2) && !GenGrid.Impassable(intVec3_1))
            {
                float temperature1 = GridsUtility.GetTemperature(intVec3_2);
                float temperature2 = GridsUtility.GetTemperature(intVec3_1);


                //Check for Mode
                bool _cooling = true;
                if (this.m_Mode == enumCoolerMode.Cooling)
                {
                    _cooling = true;
                }
                else if(this.m_Mode == enumCoolerMode.Heating)
                {
                    _cooling = false;
                }
                else if (this.m_Mode == enumCoolerMode.Auto)
                {
                    Log.Message("T1: " + temperature1 + "T2: " + temperature2 + "TT: " + this.compTempControl.targetTemperature);
                    if (temperature1 > this.compTempControl.targetTemperature)
                    {
                        Log.Message("Auto Cooling");
                        _cooling = true;
                    }
                    else
                    {
                        Log.Message("Auto Heating");
                        _cooling = false;
                    }
                }

                float a = 0.0f;
                float energyLimit = 0.0f;

                if (_cooling)
                {
                    //Log.Message("Cooling");
                    float _TemperatureDifferance = temperature1 - temperature2;
                    if ((double)temperature1 - 40.0 > (double)_TemperatureDifferance)
                        _TemperatureDifferance = temperature1 - 40f;
                    float num2 = (float)(1.0 - (double)_TemperatureDifferance * (1.0 / 130.0));
                    if ((double)num2 < 0.0)
                        num2 = 0.0f;
                    energyLimit = (float)((double)this.compTempControl.Props.energyPerSecond * (double)num2 * 4.16666650772095);
                    a = GenTemperature.ControlTemperatureTempChange(intVec3_1, energyLimit, this.compTempControl.targetTemperature);
                    flag = !Mathf.Approximately(a, 0.0f);
                }
                else
                {
                    //Log.Message("Heating");
                       float _TemperatureDifferance = temperature1 - temperature2;
                    if ((double)temperature1 + 40.0 > (double)_TemperatureDifferance)
                        _TemperatureDifferance = temperature1 + 40f;
                    float num2 = (float)(1.0 - (double)_TemperatureDifferance * (1.0 / 130.0));
                    if ((double)num2 < 0.0)
                        num2 = 0.0f;
                    energyLimit = (float)((double)this.compTempControl.Props.energyPerSecond * -(double)num2 * 4.16666650772095);
                    //energyLimit = (float)((double)this.compTempControl.Props.energyPerSecond * 4.16666650772095 * -1);
                    a = GenTemperature.ControlTemperatureTempChange(intVec3_1, energyLimit, this.compTempControl.targetTemperature);
                    flag = !Mathf.Approximately(a, 0.0f);
                    Log.Message("TempDiff: " + _TemperatureDifferance + " num2: " + num2 + " EnergyLimit: " + energyLimit + " a: " + a);
                }

                if (flag)
                {
                    GridsUtility.GetRoom(intVec3_2).Temperature -= a;
                    GenTemperature.PushHeat(intVec3_1, (float)(+(double)energyLimit * 1.25));
                }
            }


            CompProperties_Power props = this.compPowerTrader.Props;
            if (flag)
                this.compPowerTrader.PowerOutput = -props.basePowerConsumption;
            else
                this.compPowerTrader.PowerOutput = -props.basePowerConsumption * this.compTempControl.Props.lowPowerConsumptionFactor;
            this.compTempControl.operatingAtHighPower = flag;
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


            if (this.m_Mode == enumCoolerMode.Cooling)
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.ChangeMode();
                act.icon = UI_TEMPERATURE_COOLING;
                act.defaultLabel = "Cooling";
                act.defaultDesc = "Cooling";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }


            if (this.m_Mode == enumCoolerMode.Heating)
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.ChangeMode();
                act.icon = UI_TEMPERATURE_HEATING;
                act.defaultLabel = "Heating";
                act.defaultDesc = "Heating";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }


            if (this.m_Mode == enumCoolerMode.Auto)
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.ChangeMode();
                act.icon = UI_TEMPERATURE_AUTO;
                act.defaultLabel = "Auto";
                act.defaultDesc = "Auto";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }
        }

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

        public void ChangeMode()
        {
            if (this.m_Mode == enumCoolerMode.Cooling)
            {
                this.m_Mode = enumCoolerMode.Heating;
            }
            else if (this.m_Mode == enumCoolerMode.Heating)
            {
                this.m_Mode = enumCoolerMode.Auto;
            }
            else if (this.m_Mode == enumCoolerMode.Auto)
            {
                this.m_Mode = enumCoolerMode.Cooling;
            }
        }

        public override string GetInspectString()
        {

            StringBuilder stringBuilder = new StringBuilder();
            
            if (this.m_Mode == enumCoolerMode.Cooling)
            {
                stringBuilder.AppendLine("Mode: Cooling");
            }
            else if (this.m_Mode == enumCoolerMode.Heating)
            {
                stringBuilder.AppendLine("Mode: Heating");
            }
            else if (this.m_Mode == enumCoolerMode.Auto)
            {
                stringBuilder.AppendLine("Mode: Auto");
            }

            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString();
        }
    }

    public enum enumCoolerMode
    {
        //Default Mod, Cooling to a specific temperature.
        Cooling,
        //Heating to a specific temperature.
        Heating,
        //Targeting a specific temperature and Heating or Cooling to reach it.
        Auto
    }
}
