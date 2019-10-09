using System.Collections.Generic;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using Multiplayer.API;

namespace EnhancedDevelopment.ReverseCycleCooler
{
    [StaticConstructorOnStartup]
    public class Building_ReverseCycleCooler : Building_Cooler
    {
        private static Texture2D UI_ROTATE_RIGHT;
        private static Texture2D UI_TEMPERATURE_COOLING;
        private static Texture2D UI_TEMPERATURE_HEATING;
        private static Texture2D UI_TEMPERATURE_AUTO;
        
        private const float HeatOutputMultiplier = 1.25f;
        private const float EfficiencyLossPerDegreeDifference = 0.007692308f;
        private const float TemperatureDiffThreshold = 40.0f;
        private const float UnknownConst_2 = 4.16666651f;

        private EnumCoolerMode m_Mode = EnumCoolerMode.Cooling;

        static Building_ReverseCycleCooler()
        {
            UI_ROTATE_RIGHT = ContentFinder<Texture2D>.Get("UI/RotRight", true);
            UI_TEMPERATURE_COOLING = ContentFinder<Texture2D>.Get("UI/Temperature_Cooling", true);
            UI_TEMPERATURE_HEATING = ContentFinder<Texture2D>.Get("UI/Temperature_Heating", true);
            UI_TEMPERATURE_AUTO = ContentFinder<Texture2D>.Get("UI/Temperature_Auto", true);

            if (MP.enabled)
            {
                if (!MP.API.Equals("0.1"))
                {
                    Log.Error("ReverseCycleCooler: MP API version mismatch. This mod is designed to work with MPAPI version 0.1");
                }
                else
                {
                    MP.RegisterAll();
                    //Log.Message("ReverseCycleCooler: MP init");
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void TickRare()
        {
            if (!compPowerTrader.PowerOn)
            {
                return;
            }
            
            IntVec3 intVec3_1 = Position + IntVec3Utility.RotatedBy(IntVec3.South, Rotation);
            IntVec3 intVec3_2 = Position + IntVec3Utility.RotatedBy(IntVec3.North, Rotation);
            bool flag = false;
            if (!GenGrid.Impassable(intVec3_2, Map) && !GenGrid.Impassable(intVec3_1, Map))
            {
                float temperature1 = GridsUtility.GetTemperature(intVec3_2, Map);
                float temperature2 = GridsUtility.GetTemperature(intVec3_1, Map);


                //Check for Mode
                bool _cooling = true;
                if (m_Mode == EnumCoolerMode.Cooling)
                {
                    _cooling = true;
                }
                else if(m_Mode == EnumCoolerMode.Heating)
                {
                    _cooling = false;
                }
                else if (m_Mode == EnumCoolerMode.Auto)
                {
                    if (temperature1 > compTempControl.targetTemperature)
                    {
                        //Log.Message("Auto Cooling");
                        _cooling = true;
                    }
                    else
                    {
                        //Log.Message("Auto Heating");
                        _cooling = false;
                    }
                }

                float a;
                float energyLimit;
                float _TemperatureDifferance;
                float num2;

                if (_cooling)
                {
                    //Log.Message("Cooling");
                    _TemperatureDifferance = temperature1 - temperature2;
                    if (temperature1 - TemperatureDiffThreshold > _TemperatureDifferance)
                        _TemperatureDifferance = temperature1 - TemperatureDiffThreshold;
                    num2 = 1.0f - _TemperatureDifferance * EfficiencyLossPerDegreeDifference;
                    if (num2 < 0.0f)
                        num2 = 0.0f;
                    energyLimit = (float)(compTempControl.Props.energyPerSecond * (double)num2 * UnknownConst_2);
                    a = GenTemperature.ControlTemperatureTempChange(intVec3_1, Map, energyLimit, compTempControl.targetTemperature);
                    flag = !Mathf.Approximately(a, 0.0f);
                }
                else
                {
                    //Log.Message("Heating");
                    _TemperatureDifferance = temperature1 - temperature2;
                    if (temperature1 + TemperatureDiffThreshold > _TemperatureDifferance)
                        _TemperatureDifferance = temperature1 + TemperatureDiffThreshold;
                    num2 = 1.0f - _TemperatureDifferance * EfficiencyLossPerDegreeDifference;
                    if (num2 < 0.0f)
                        num2 = 0.0f;
                    energyLimit = (float)(compTempControl.Props.energyPerSecond * -(double)num2 * UnknownConst_2);
                    a = GenTemperature.ControlTemperatureTempChange(intVec3_1, Map, energyLimit, compTempControl.targetTemperature);
                    flag = !Mathf.Approximately(a, 0.0f);
                }

                if (flag)
                {
                    GridsUtility.GetRoomGroup(intVec3_2, Map).Temperature -= a;
                    GenTemperature.PushHeat(intVec3_1, Map, (float)(+(double)energyLimit * HeatOutputMultiplier));
                }
            }


            CompProperties_Power props = compPowerTrader.Props;
            if (flag)
            {
                compPowerTrader.PowerOutput = -props.basePowerConsumption;
            }
            else
            {
                compPowerTrader.PowerOutput = -props.basePowerConsumption * compTempControl.Props.lowPowerConsumptionFactor;
            }

            compTempControl.operatingAtHighPower = flag;
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


            {
                Command_Action act = new Command_Action
                {
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    action = () => ChangeRotation(),
                    icon = UI_ROTATE_RIGHT,
                    defaultLabel = "Rotate",
                    defaultDesc = "Rotates",
                    activateSound = SoundDef.Named("Click")
                };
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }


            if (m_Mode == EnumCoolerMode.Cooling)
            {
                Command_Action act = new Command_Action
                {
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    action = () => ChangeMode(),
                    icon = UI_TEMPERATURE_COOLING,
                    defaultLabel = "Cooling",
                    defaultDesc = "Cooling",
                    activateSound = SoundDef.Named("Click")
                };
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }


            if (m_Mode == EnumCoolerMode.Heating)
            {
                Command_Action act = new Command_Action
                {
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    action = () => ChangeMode(),
                    icon = UI_TEMPERATURE_HEATING,
                    defaultLabel = "Heating",
                    defaultDesc = "Heating",
                    activateSound = SoundDef.Named("Click")
                };
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }


            if (m_Mode == EnumCoolerMode.Auto)
            {
                Command_Action act = new Command_Action
                {
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    action = () => ChangeMode(),
                    icon = UI_TEMPERATURE_AUTO,
                    defaultLabel = "Auto",
                    defaultDesc = "Auto",
                    activateSound = SoundDef.Named("Click")
                };
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }
        }

        public void ChangeRotation()
        {
            //Log.Error("Rotation");

            if (Rotation.AsInt == 3)
            {
                Rotation = new Rot4(0);
            }
            else
            {
                Rotation = new Rot4(Rotation.AsInt + 1);
            }

            // Tell the MapDrawer that here is something thats changed
            Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things, true, false);

            //this.Rotation.Rotate(RotationDirection.Clockwise);
            //this.Rotation.Rotate(RotationDirection.Clockwise);

        }

        [SyncMethod]
        public void ChangeMode()
        {
            if (m_Mode == EnumCoolerMode.Cooling)
            {
                m_Mode = EnumCoolerMode.Heating;
            }
            else if (m_Mode == EnumCoolerMode.Heating)
            {
                m_Mode = EnumCoolerMode.Auto;
            }
            else if (m_Mode == EnumCoolerMode.Auto)
            {
                m_Mode = EnumCoolerMode.Cooling;
            }
        }

        public override string GetInspectString()
        {

            StringBuilder stringBuilder = new StringBuilder();
            
            if (m_Mode == EnumCoolerMode.Cooling)
            {
                stringBuilder.AppendLine("Mode: Cooling");
            }
            else if (m_Mode == EnumCoolerMode.Heating)
            {
                stringBuilder.AppendLine("Mode: Heating");
            }
            else if (m_Mode == EnumCoolerMode.Auto)
            {
                stringBuilder.AppendLine("Mode: Auto");
            }

            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString();
        }


        //Saving game
        public override void ExposeData()
        {
            base.ExposeData();

            // Scribe_Deep.LookDeep(ref shieldField, "shieldField");
            // Scribe_Values.LookValue(ref m_Mode, "m_Mode");
            //Scribe_Collections.LookList<Thing>(ref listOfBufferThings, "listOfBufferThings", LookMode.Deep, (object)null);
            Scribe_Values.Look(ref m_Mode, "m_Mode");
        }
    }

    public enum EnumCoolerMode
    {
        //Default Mod, Cooling to a specific temperature.
        Cooling,
        //Heating to a specific temperature.
        Heating,
        //Targeting a specific temperature and Heating or Cooling to reach it.
        Auto
    }
}
