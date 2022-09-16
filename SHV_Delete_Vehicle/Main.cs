using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SHV_Delete_Vehicle
{
    public class Main : Script

    {
        public static string INIpath = "scripts\\SHV_Delete_Vehicle.ini";
        public static ScriptSettings IniSettings;
        public static Keys DeleteKey { get; set; }
        public static Keys DeleteModifierKey { get; set; }
        public static Keys ConfirmKey { get; set; }
        public static Keys ConfirmModifierKey { get; set; }
        public static string ConfirmString { get; set; }
        public static Keys DeclineKey { get; set; }
        public static Keys DeclineModifierKey { get; set; }
        public static string DeclineString { get; set; }

        // public static ControllerButtons DeleteButton { get; set; }
        // public static ControllerButtons DeleteModifierButton { get; set; }
        public static Boolean ProtectCurrentVehicle { get; set; }
        public static Boolean ProtectLastVehicle { get; set; }
        public static Boolean ProtectEmergencyVehicles { get; set; }
        public static Boolean ShowDebug { get; set; }
        public static Vehicle GetVehicle { get; set; }
        public static Boolean AwaitingInput { get; set; }
        public static UInt32 MsgID { get; set; }

        //Initialization of the plugin.

        public Main()
        {
            LoadValuesFromIniFile();

            KeyDown += OnKeyDown;
           // Tick += OnControllerDown;

            Interval = 0;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == DeleteKey && e.Modifiers == DeleteModifierKey)
            {
                Command_Debug("Debug: ExampleKey just pressed");
                // Call the thing to do here
            }
        }

        //private void OnControllerDown(object sender, EventArgs e)
        //{
        //    if (ExampleButton != ControllerKeybinds.None)
        //    {
        //        if (ExampleModifierButton != ControllerKeybinds.None)
        //        {
        //            if (Game.IsControlPressed((GTA.Control)ExampleModifierButton) && Game.IsControlJustReleased((GTA.Control)ExampleButton))
        //            {
        //                // Call the thing to do here
        //            }
        //        }
        //        else if (ExampleModifierButton == ControllerKeybinds.None && Game.IsControlJustReleased((GTA.Control)ExampleButton))
        //        {
        //            // Call the thing to do here
        //        }
        //    }
        //}

        private static void ChkDeleteVehicle()
        {

        }

        public static async System.Threading.Tasks.Task AwaitConfirmation()
        {

        }

        private static void DeleteVehicle()
        {

        }

            private void LoadValuesFromIniFile()
            {
            ScriptSettings scriptSettings = ScriptSettings.Load(INIpath);
            DeleteKey = (Keys)scriptSettings.GetValue<Keys>("Keyboard", "DeleteKey", Keys.L);
            DeleteModifierKey = (Keys)scriptSettings.GetValue<Keys>("Keyboard", "DeleteModifierKey", Keys.None);

            //Confrim
            ConfirmKey = (Keys)scriptSettings.GetValue<Keys>("Keyboard", "ConfirmKey", Keys.Y);
            ConfirmModifierKey = (Keys)scriptSettings.GetValue<Keys>("Keyboard", "ConfirmModifierKey", Keys.None);
            ConfirmString = "You forgot to set this, dummy!";

            //Modify
            DeclineKey = (Keys)scriptSettings.GetValue<Keys>("Keyboard", "DeclineKey", Keys.N);
            DeclineModifierKey = (Keys)scriptSettings.GetValue<Keys>("Keyboard", "DeclineModifierKey", Keys.None);
            DeclineString = "You forgot to set this, dummy!";

            //ExampleButton = (ControllerKeybinds)scriptSettings.GetValue<ControllerKeybinds>("Controller", "ExampleButton", ControllerKeybinds.None);
            //ExampleModifierButton = (ControllerKeybinds)scriptSettings.GetValue<ControllerKeybinds>("Controller", "ExampleModifierButton", ControllerKeybinds.None);

            ShowDebug = (bool)scriptSettings.GetValue<bool>("Other", "ShowDebug", false);
            Command_Debug("Debug: ELS Cinematic INI loaded");
        }

        public enum ControllerKeybinds
        {
            None = -1, // 0xFFFFFFFF
            A = 201, // 0x000000C9
            B = 202, // 0x000000CA
            X = 203, // 0x000000CB
            Y = 204, // 0x000000CC
            LB = 226, // 0x000000E2
            RB = 227, // 0x000000E3
            LT = 228, // 0x000000E4
            RT = 229, // 0x000000E5
            LS = 230, // 0x000000E6
            RS = 231, // 0x000000E7
            DPadUp = 232, // 0x000000E8
            DPadDown = 233, // 0x000000E9
            DPadLeft = 234, // 0x000000EA
            DPadRight = 235, // 0x000000EB
        }

        public static void Command_Debug(string text)
        {
            if (ShowDebug == true)
            {
                GTA.UI.Notification.Show(text);
            }
        }

        public static void ErrorLogger(Exception Err, String ErrMod, String ErrDesc)
        {
            //    Game.LogTrivial("--------------------------------------");
            //    Game.LogTrivial("Error during " + ErrMod);
            //    Game.LogTrivial("Decription: " + ErrDesc);
            //    Game.LogTrivial(Err.ToString());

            GTA.UI.Notification.Show("~r~~h~Delete Vehicle:~h~~s~ Error during " + ErrMod + ". Please send logs.");
        }
    }
}
