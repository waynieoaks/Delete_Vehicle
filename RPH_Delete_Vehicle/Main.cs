using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace RPH_Delete_Vehicle
{
    public static class EntryPoint

    {
        public static string INIpath = "Plugins\\RPH_Delete_Vehicle.ini";
        public static Keys DeleteKey { get; set; }
        public static Keys DeleteModifierKey { get; set; }
        // public static ControllerButtons DeleteButton { get; set; }
        // public static ControllerButtons DeleteModifierButton { get; set; }
        public static Boolean ProtectCurrentVehicle { get; set; }
        public static Boolean ProtectLastVehicle { get; set; }
        public static Boolean ProtectEmergencyVehicles { get; set; }
        public static Boolean ShowDebug { get; set; }

        //Initialization of the plugin.
        public static void Main()
        {
            Game.LogTrivial("Loading settings...");
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

                        DoDeleteVehicle();
                        GameFiber.Sleep(1000);
                    }
                }
            });
        }

        private static void DoDeleteVehicle()
        {
            try
            {
                // Get some local variables
                Ped playerPed = Game.LocalPlayer.Character;
 
                // Get the closest vehicle
                Vehicle GetVehicle = (Vehicle)World.GetClosestEntity(playerPed.Position, 5.0f, GetEntitiesFlags.ConsiderAllVehicles); // Search 5m around player for vehicle

                if (GetVehicle != null)
                {
                    // Vehicle GetVehicle = (Vehicle)GetEntity;
                    Command_Debug("Found a " + GetVehicle.Model.Name + ", Registration: " + GetVehicle.LicensePlate);

                    if ((ProtectCurrentVehicle == true) && (playerPed.IsInAnyVehicle(true)))
                    {
                        Game.DisplayNotification("You cannot delete the vehicle you are sitting in!");

                        return;

                    }
                    else if ((ProtectCurrentVehicle == true) && (GetVehicle == playerPed.LastVehicle))
                    {
                        //If vehcile is players last vehicle, prompt are you sure? 
                        Game.DisplayNotification("You cannot delete the last vehicle you were sitting in!");

                        return;

                    }
                    else if ((ProtectEmergencyVehicles == true) && (GetVehicle.Class == VehicleClass.Emergency)) 
                    {
                        // If ProtectEmergencyVehicles is true check if emergency vehicle and prompt are you sure?
                         Game.DisplayNotification("You cannot delete emergency vehicles!");

                        return;
                    }

                    // Else delete it
                    GetVehicle.Delete();

                } else
                {
                    // There was no vehicle nearby...
                    Game.DisplayNotification("Could not find vehicle to delete! Try getting a little closer.");
                }





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

                if (ini.DoesKeyExist("Keyboard", "DeleteKey")) { DeleteKey = ini.ReadEnum<Keys>("Keyboard", "DeleteKey", Keys.L); }
                else
                {
                    ini.Write("Keyboard", "DeleteKey", "L");
                    DeleteKey = Keys.L;
                }

                if (ini.DoesKeyExist("Keyboard", "DeleteModifierKey")) { DeleteModifierKey = ini.ReadEnum<Keys>("Keyboard", "DeleteModifierKey", Keys.ShiftKey); }
                else
                {
                    ini.Write("Keyboard", "DeleteModifierKey", "ShiftKey");
                    DeleteModifierKey = Keys.ShiftKey;
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

                if (ini.DoesKeyExist("Other", "ProtectCurrentVehicle")) { ProtectCurrentVehicle = ini.ReadBoolean("Other", "ProtectCurrentVehicle", true); }
                else
                {
                    ini.Write("Other", "ProtectCurrentVehicle", "true");
                    ProtectCurrentVehicle = true;
                }
                
                if (ini.DoesKeyExist("Other", "ProtectlastVehicle")) { ProtectLastVehicle = ini.ReadBoolean("Other", "ProtectlastVehicle", true); }
                else
                {
                    ini.Write("Other", "ProtectlastVehicle", "true");
                    ProtectLastVehicle = true;
                }
                
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
                Game.DisplayNotification("~r~~h~Debug:~h~~s~ " + text);
                Game.LogTrivial("Debug: " + text);
            }
        }

        public static void ErrorLogger(Exception Err, String ErrMod, String ErrDesc)
        {
            Game.LogTrivial("--------------------------------------");
            Game.LogTrivial("Error during " + ErrMod);
            Game.LogTrivial("Decription: " + ErrDesc);
            Game.LogTrivial(Err.ToString());
            Game.DisplayNotification("~r~~h~Delete Vehicle:~h~~s~ Error during " + ErrMod + ". Please send logs.");
        }
    }
}
