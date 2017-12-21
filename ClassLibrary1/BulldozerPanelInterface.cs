using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace BulldozerMod
{
    class BulldozerPanelInterface
    {
        public static bool initialized = false;
        public static UIView uiView;

        public static UIButton demolishAbandonedButton;
        public static UIButton demolishBurnedButton;
        public static UIButton demolishNowButton;

        public static SavedBool b_demolishAbandoned = new SavedBool("ModDemolishAbandoned", Settings.gameSettingsFile, true, true);
        public static SavedBool b_demolishBurned = new SavedBool("ModDemolishBurned", Settings.gameSettingsFile, true, true);
        public static SavedBool b_demolishAutomatically = new SavedBool("ModDemolishAutomatically", Settings.gameSettingsFile, true, true);

        public static void initButton(UIButton button, bool isCheck)
        {
            string sprite = "SubBarButtonBase";//"ButtonMenu";
            string spriteHov = sprite + "Hovered";
            //demolishAbandonedButton.colorizeSprites = true;
            button.normalBgSprite = spriteHov;
            //demolishAbandonedButton.spritePadding = new RectOffset(5, 5, 2, 2);
            button.disabledBgSprite = spriteHov;// + "Disabled";
            button.hoveredBgSprite = spriteHov;// + "Hovered";
            button.focusedBgSprite = spriteHov;// + "Focused";
            button.pressedBgSprite = sprite + "Pressed";
            button.textColor = new Color32(255, 255, 255, 255);

        }
        public static void updateCheckButton(UIButton button, bool isActive)
        {
            Color32 inactiveColor = new Color32(64, 64, 64, 255);
            Color32 activeColor = new Color32(255, 64, 64, 255);
            Color32 whiteColor = new Color32(255, 255, 255, 255);
            Color32 textColor = new Color32(255, 255, 255, 255);
            Color32 textColorDis = new Color32(128, 128, 128, 255);

            Color32 HoverInactiveColor = new Color32(74, 74, 74, 255);
            Color32 HoverActiveColor = new Color32(240, 64, 64, 255);

            if (isActive == true)
            {
                button.color = activeColor;
                button.focusedColor = activeColor;
                button.hoveredColor = HoverActiveColor;
                button.pressedColor = activeColor;
                button.textColor = textColor;
            }
            else
            {
                button.color = inactiveColor;
                button.focusedColor = inactiveColor;
                button.hoveredColor = HoverInactiveColor;
                button.pressedColor = inactiveColor;
                button.textColor = textColorDis;
            }

            button.Unfocus();
        }
        public static void init()
        {
            if (initialized) return;

            uiView = GameObject.FindObjectOfType<UIView>();

            if (uiView == null) return;
            if (UIView.Find("BulldozerBar") == null) return;

            /////////////////////////////////////////////////////

            GameObject demButton = new GameObject();
            GameObject demButton2 = new GameObject();
            GameObject demButton3 = new GameObject();

            demButton.transform.parent = UIView.Find("BulldozerBar").transform;
            demButton2.transform.parent = UIView.Find("BulldozerBar").transform;
            demButton3.transform.parent = UIView.Find("BulldozerBar").transform;
            
            demolishAbandonedButton = demButton.AddComponent<UIButton>();
            demolishAbandonedButton.relativePosition = new Vector3(10.0f, -20.0f);
            demolishAbandonedButton.text = "Demolish Abandoned";
            demolishAbandonedButton.width = 200;
            demolishAbandonedButton.height = 50;
            demolishAbandonedButton.name = "DemolishAbandonedButton";
            initButton(demolishAbandonedButton, true);

            demolishBurnedButton = demButton2.AddComponent<UIButton>();
            demolishBurnedButton.relativePosition = new Vector3(220.0f, -20.0f);
            demolishBurnedButton.text = "Demolish Burned";
            demolishBurnedButton.width = 200;
            demolishBurnedButton.height = 50;
            demolishBurnedButton.name = "DemolishBurnedButton";
            initButton(demolishBurnedButton, true);

            demolishNowButton = demButton3.AddComponent<UIButton>();
            demolishNowButton.relativePosition = new Vector3(430.0f, -20.0f);
            demolishNowButton.text = "Demolish All";
            demolishNowButton.width = 120;
            demolishNowButton.height = 50;
            demolishNowButton.name = "DemolishNowButton";
            demolishNowButton.tooltip = "Demolishes all demolishable buildings";
            initButton(demolishNowButton, false);

            demolishAbandonedButton.eventClick += demolishAbandonedClick;
            demolishBurnedButton.eventClick += demolishBurnedClick;
            demolishNowButton.eventClick += demolishAllClick;


            updateCheckButton(demolishAbandonedButton, b_demolishAbandoned.value);
            updateCheckButton(demolishBurnedButton, b_demolishBurned.value);


            initialized = true;
        }
        private static void demolishBurnedClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            b_demolishBurned.value = !(b_demolishBurned.value);
            updateCheckButton(demolishBurnedButton, b_demolishBurned.value);
        }

        private static void demolishAbandonedClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            b_demolishAbandoned.value = !(b_demolishAbandoned.value);
            updateCheckButton(demolishAbandonedButton, b_demolishAbandoned.value);
        }
        private static void demolishAllClick(UIComponent component, UIMouseEventParameter Event)
        {
            try
            {
                (new ThreadingTestMod()).ManualDemolishAll();
            } catch (ModException e)
            {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, e.StackTrace);
                DebugOutputPanel.Show();
            }
        }
    }
}
