using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using UnityEngine;


namespace BulldozerMod
{
    class DemolishCounter
    {
        public enum DemolishType
        {
            Null, Abandon, Burned
        }
        static int TotalAbdandonedDemolished_Count = 0;
        static int TotalBurnedDemolished_Count = 0;

        static List<DemolishEvent> DemolishEvents = new List<DemolishEvent>();
        static UIComponent BulldozerButton;
        static bool Initialized;

        public static bool ToolTipInit()
        {
            if (Initialized) return true;
            UIView uiv = GameObject.FindObjectOfType<UIView>();
            if (uiv == null)
            {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "ui null");
                return false;
            }
            BulldozerButton = UIView.Find("BulldozerButton");
            if (BulldozerButton == null)
            {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "Button null");
                return false;
            }
            Initialized = true;
            return true;
        }

        public static void UpdateTooltip()
        {
            if (!ToolTipInit()) return;
            //
            //Use panel interface instead
            if (BulldozerPanelInterface.b_demolishAbandoned || BulldozerPanelInterface.b_demolishBurned)
            {
                int CurrentMinuteAbandonCounter = 0;
                int CurrentMinuteBurnCounter = 0;
                int RemoveToRange = 0;
                for (int i = 0; i < DemolishEvents.Count; i++)
                {
                    DemolishEvent CurrentEvent = DemolishEvents[i];
                    if (CurrentEvent.Time < DateTime.Now.AddMinutes(-1))
                    {
                        RemoveToRange++;
                    }
                    else
                    {
                        if (CurrentEvent.Type == DemolishType.Abandon) CurrentMinuteAbandonCounter++;
                        if (CurrentEvent.Type == DemolishType.Burned) CurrentMinuteBurnCounter++;
                    }
                }
                if (RemoveToRange > 0) DemolishEvents.RemoveRange(0, RemoveToRange);
                string ToolTipString = String.Format("Bulldozer\nAbandoned: {0}/m\nBurned: {1}/m", CurrentMinuteAbandonCounter, CurrentMinuteBurnCounter);
                BulldozerButton.tooltip = ToolTipString;
            }
            else
            {
                BulldozerButton.tooltip = "Bulldozer";
            }
        }
        public static void AddToCount(DemolishType DType, int Count)
        {
            DemolishEvent DemoEvent;
            switch (DType)
            {
                case DemolishType.Abandon:
                    TotalAbdandonedDemolished_Count += Count;
                    DemoEvent = new DemolishEvent(DateTime.Now, DemolishType.Abandon, Count);
                    break;
                case DemolishType.Burned:
                    TotalBurnedDemolished_Count += Count;
                    DemoEvent = new DemolishEvent(DateTime.Now, DemolishType.Abandon, Count);
                    break;
                case DemolishType.Null:
                default: return;
            }
            DemolishEvents.Add(DemoEvent);
        }
        private struct DemolishEvent
        {
            public DemolishEvent(DateTime _Time, DemolishType _Type, int _Count)
            {
                Time = _Time;
                Type = _Type;
                Count = _Count;
            }
            public DateTime Time { get; private set; }
            public DemolishType Type { get; private set; }
            public int Count { get; private set; }
        }
    }
}
