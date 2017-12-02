using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System.Timers;
using UnityEngine;

namespace BulldozerMod
{
    public class AutobulldozeMod : IUserMod
    {
        public static GameObject modObject;

        public string Name
        {
            get { return "Automatic Bulldoze - Original By Sadler"; }
        }
        public string Description
        {
            get { return "Automatically destroys abandoned and burned buildings"; }
        }

    }

    public class ThreadingTestMod : ThreadingExtensionBase
    {
        public static AudioGroup nullAudioGroup;
        bool m_initialized = false;

        public bool init()
        {
            if (m_initialized == true) return true;
            UIComponent bullBar = UIView.Find("BulldozerBar");
            if (bullBar == null) return false;

            GameObject obDemolishAbandoned = new GameObject();
            UIButton checkDemolishAbandoned = obDemolishAbandoned.AddComponent<UIButton>();

            checkDemolishAbandoned.transform.parent = bullBar.transform;
            checkDemolishAbandoned.transformPosition = new Vector3(-1.0f, 0.0f);
            checkDemolishAbandoned.text = "Abandoned";

            nullAudioGroup = new AudioGroup(0, new SavedFloat("NOTEXISTINGELEMENT", Settings.gameSettingsFile, 0, false));

            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "[Autobulldoze] Initialized");
            m_initialized = true;
            return true;
        }

        public override void OnCreated(IThreading threading)
        {
            m_initialized = false;
            BulldozerPanelInterface.initialized = false;
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            init();
            BulldozerPanelInterface.init();
        }
        private int GetBuildingRefundAmount(ushort building)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if (Singleton<SimulationManager>.instance.IsRecentBuildIndex(instance.m_buildings.m_buffer[(int)building].m_buildIndex))
                return instance.m_buildings.m_buffer[(int)building].Info.m_buildingAI.GetRefundAmount(building, ref instance.m_buildings.m_buffer[(int)building]);
            else
                return 0;
        }


        public static void DispatchAutobulldozeEffect(BuildingInfo info, Vector3 pos, float angle, int length)
        {
            EffectInfo effect = Singleton<BuildingManager>.instance.m_properties.m_bulldozeEffect;
            if (effect == null) return;
            InstanceID instance = new InstanceID();
            EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(Matrix4x4.TRS(Building.CalculateMeshPosition(info, pos, angle, length), Building.CalculateMeshRotation(angle), Vector3.one), info.m_lodMeshData);
            Singleton<EffectManager>.instance.DispatchEffect(effect, instance, spawnArea, Vector3.zero, 0.0f, 1f, nullAudioGroup);
        }


        private void DeleteBuildingImpl(ushort building, bool showEffect, DemolishCounter.DemolishType DemolishType)
        {
            if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)building].m_flags != Building.Flags.None)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingInfo info = instance.m_buildings.m_buffer[(int)building].Info;
                if (info.m_buildingAI.CheckBulldozing(building, ref instance.m_buildings.m_buffer[(int)building]) == ToolBase.ToolErrors.None)
                {
                    int buildingRefundAmount = this.GetBuildingRefundAmount(building);
                    if (buildingRefundAmount != 0)
                        Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.RefundAmount, buildingRefundAmount, info.m_class);
                    Vector3 pos = instance.m_buildings.m_buffer[(int)building].m_position;
                    float angle = instance.m_buildings.m_buffer[(int)building].m_angle;
                    int length = instance.m_buildings.m_buffer[(int)building].Length;
                    instance.ReleaseBuilding(building);
                    DemolishCounter.AddToCount(DemolishType, 1);
                    if (info.m_class.m_service > ItemClass.Service.Office)
                        Singleton<CoverageManager>.instance.CoverageUpdated(info.m_class.m_service, info.m_class.m_subService, info.m_class.m_level);
                    if (showEffect) DispatchAutobulldozeEffect(info, pos, angle, length);
                }
            }
        }

        public void demolishBuilding(ushort index, bool isAuto)
        {
            SimulationManager simManager = Singleton<SimulationManager>.instance;
            BuildingManager buildManager = Singleton<BuildingManager>.instance;

            if (index >= buildManager.m_buildings.m_buffer.Length)
            {
                UnityEngine.Debug.LogWarning("Autodemolish: building " + index + " not exists.");
                return;
            }


            Building build = buildManager.m_buildings.m_buffer[index];


            bool needToDemolish = false;
            DemolishCounter.DemolishType DemolishType = DemolishCounter.DemolishType.Null;
            if (BulldozerPanelInterface.b_demolishAbandoned && ((build.m_flags & Building.Flags.Abandoned) != Building.Flags.None))
            {
                DemolishType = DemolishCounter.DemolishType.Abandon;
                needToDemolish = true;
            }
            else if (BulldozerPanelInterface.b_demolishBurned && ((build.m_flags & Building.Flags.BurnedDown) != Building.Flags.None))
            {
                DemolishType = DemolishCounter.DemolishType.Burned;
                needToDemolish = true;
            }

            if (needToDemolish)
            {
                DeleteBuildingImpl(index, true, DemolishType);
                return;
            }
        }


        public override void OnAfterSimulationTick()
        {
            SimulationManager simManager = Singleton<SimulationManager>.instance;
            if (simManager.SimulationPaused) return;
            BuildingManager buildManager = Singleton<BuildingManager>.instance;
            for (ushort i = (ushort)(simManager.m_currentTickIndex % 1000); i < buildManager.m_buildings.m_buffer.Length; i += 1000)
            {
                demolishBuilding(i, true);
                Building build = buildManager.m_buildings.m_buffer[i];
            }
            DemolishCounter.UpdateTooltip();
        }
    }
}
