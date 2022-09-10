using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPH_Delete_Vehicle
{
    public static class EntryPoint

    {
        public static string INIpath = "Plugins\\RPH_Delete_Vehicle.ini";
        public static Keys DeleteKey { get; set; }
        public static Keys DeleteModifierKey { get; set; }
        // public static ControllerButtons DeleteButton { get; set; }
        // public static ControllerButtons DeleteModifierButton { get; set; }
        public static Boolean ProtectEmergencyVehicles { get; set; }
        public static Boolean ShowDebug { get; set; }

        //Initialization of the plugin.
        public static void Main()
        {
            Game.LogTrivial("Loading " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " settings...");
            LoadValuesFromIniFile();

            Game.LogTrivial(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " has been initialised.");

            // Cinematic control fiber
            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Yield();

                    //Check for key press
                    if (
                            (Game.IsKeyDown(DeleteKey)
                                && (Game.IsKeyDownRightNow(DeleteModifierKey)
                                || DeleteModifierKey == Keys.None)
                            )
                         //   ||
                         //   (Game.IsControllerButtonDown(ExampleButton)
                         //       && (Game.IsControllerButtonDownRightNow(ExampleModifierButton)
                         //       || ExampleModifierButton == ControllerButtons.None)
                         //   )
                        )
                    {
                        // Call the thing to do here!

                        GameFiber.Sleep(1000);
                    }
                }
            });
        }

        private static void DoDeleteVehicle()
        {
            try
            {
                // Do your stuff here!
                Game.DisplayNotification("Delete vehicle key pressed!");
            }
            catch (Exception e)
            {
                ErrorLogger(e, "Activation", "Error during execution");
            }
        }

        private static void LoadValuesFromIniFile()
        {
            InitializationFile ini = new InitializationFile(INIpath);
            ini.Create();

            try
            {
                //Keyboard ini

                if (ini.DoesKeyExist("Keyboard", "DeleteKey")) { DeleteKey = ini.ReadEnum<Keys>("Keyboard", "DeleteKey", Keys.R); }
                else
                {
                    ini.Write("Keyboard", "DeleteKey", "R");
                    DeleteKey = Keys.R;
                }

                if (ini.DoesKeyExist("Keyboard", "DeleteModifierKey")) { DeleteModifierKey = ini.ReadEnum<Keys>("Keyboard", "DeleteModifierKey", Keys.ControlKey); }
                else
                {
                    ini.Write("Keyboard", "DeleteModifierKey", "ControlKey");
                    DeleteModifierKey = Keys.ControlKey;
                }

                // Controller ini

                // if (ini.DoesKeyExist("Controller", "ExampleButton")) { ExampleButton = ini.ReadEnum<ControllerButtons>("Controller", "ExampleButton", ControllerButtons.None); }
                // else
                // {
                //     ini.Write("Controller", "ExampleButton", "None");
                //     ExampleButton = ControllerButtons.None;
                // }
                //
                // if (ini.DoesKeyExist("Controller", "ExampleModifierButton")) { ExampleModifierButton = ini.ReadEnum<ControllerButtons>("Controller", "ExampleModifierButton", ControllerButtons.None); }
                // else
                // {
                //     ini.Write("Controller", "ExampleModifierButton", "None");
                //     ExampleModifierButton = ControllerButtons.None;
                // }

                // Other ini

                if (ini.DoesKeyExist("Other", "ProtectEmergencyVehicles")) { ProtectEmergencyVehicles = ini.ReadBoolean("Other", "ProtectEmergencyVehicles", true); }
                else
                {
                    ini.Write("Other", "ProtectEmergencyVehicles", "true");
                    ProtectEmergencyVehicles = true;
                }

                if (ini.DoesKeyExist("Other", "ShowDebug")) { ShowDebug = ini.ReadBoolean("Other", "ShowDebug", false); }
                else
                {
                    ini.Write("Other", "ShowDebug", "false");
                    ShowDebug = false;
                }

                Game.LogTrivial("Settings initialisation complete.");
            }
            catch (Exception e)
            {
                ErrorLogger(e, "Initialisation", "Unable to read INI file.");
            }
        }

        public static void Command_Debug(string text)
        {
            if (ShowDebug == true)
            {
                Game.DisplayNotification(text);
            }
        }

        public static void ErrorLogger(Exception Err, String ErrMod, String ErrDesc)
        {
            Game.LogTrivial("--------------------------------------");
            Game.LogTrivial("Error during " + ErrMod);
            Game.LogTrivial("Decription: " + ErrDesc);
            Game.LogTrivial(Err.ToString());
            Game.DisplayNotification("~r~~h~" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + ":~h~~s~ Error during " + ErrMod + ". Please send logs.");
        }
    }
}
