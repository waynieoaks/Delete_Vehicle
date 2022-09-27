using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
        public static Keys ConfirmKey { get; set; }
        public static Keys ConfirmModifierKey { get; set; }
        public static string ConfirmString { get; set; }
        public static Keys DeclineKey { get; set; }
        public static Keys DeclineModifierKey { get; set; }
        public static string DeclineString { get; set; }

        // public static ControllerButtons DeleteButton { get; set; }
        // public static ControllerButtons DeleteModifierButton { get; set; }
       // public static Boolean ProtectCurrentVehicle { get; set; }
       // public static Boolean ProtectLastVehicle { get; set; }
       // public static Boolean ProtectEmergencyVehicles { get; set; }
        public static Boolean ShowDebug { get; set; }
        public static Vehicle GetVehicle { get; set; }
        public static Boolean AwaitingInput { get; set; }
        public static UInt32 MsgID { get; set; }

        //Initialization of the plugin.
        public static void Main()
        {
            Game.LogTrivial("Loading settings...");
            LoadValuesFromIniFile();

            Game.LogTrivial(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " has been initialised.");

            // Keybind control fiber
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

                        ChkDeleteVehicle();
                        GameFiber.Sleep(1000);
                    }
                    if (Game.IsKeyDown(ConfirmKey) &&
                            (
                                (Game.IsKeyDownRightNow(ConfirmModifierKey) || ConfirmModifierKey == Keys.None)
                            )
                                && (AwaitingInput == true)
                        )
                    {
                        // Delete confirmed
                        Game.RemoveNotification(MsgID);
                        DeleteVehicle();
                        AwaitingInput = false; // Cancel the timer
                        
                        GameFiber.Sleep(500);
                    }
                    if (Game.IsKeyDown(DeclineKey) &&
                            (
                                (Game.IsKeyDownRightNow(DeclineModifierKey) || DeclineModifierKey == Keys.None)
                            )
                                && (AwaitingInput == true)
                        )
                    {
                        // Confirm cancellation
                        Game.RemoveNotification(MsgID);

                        Command_Debug("Delete Cancelled");

                        AwaitingInput = false; // Cancel the timer
                        GameFiber.Sleep(500);
                    }
                }
            });
        }

        private static void ChkDeleteVehicle()
        {
            try
            {
                // Get some local variables
                Ped playerPed = Game.LocalPlayer.Character;
 
                // Get the closest vehicle
                GetVehicle = (Vehicle)World.GetClosestEntity(playerPed.Position, 6.0f, GetEntitiesFlags.ConsiderAllVehicles); 
                // Search 6m around player for vehicle

                if (GetVehicle != null)
                {
                    // Vehicle GetVehicle = (Vehicle)GetEntity;
                    Command_Debug("Found a " + GetVehicle.Model.Name + ", Registration: " + GetVehicle.LicensePlate);

                    if (playerPed.IsInAnyVehicle(true))
                    {
                        Game.DisplayNotification("Sorry, but you cannot delete the vehicle you are sitting in.");
                        //  MsgID = Game.DisplayNotification("<b>~y~Delete Vehicle:</b>~s~~n~ Are you sure you want to delete the vehicle you are sitting in?~n~    [<b>~r~" + ConfirmString + "~s~</b>]    [<b>~b~" + DeclineString + "~s~</b>]");
                        //   System.Threading.Tasks.Task AwaitInput = AwaitConfirmation();

                        return;

                    }
                    else if (GetVehicle == playerPed.LastVehicle)
                    {
                        //If vehcile is players last vehicle, prompt are you sure? 
                        MsgID = Game.DisplayNotification("<b>~y~Delete Vehicle:</b>~s~~n~ Are you sure you want to delete the vehicle you were last sitting in?~n~    [<b>~r~" + ConfirmString + "~s~</b>]    [<b>~b~" + DeclineString + "~s~</b>]");
                        System.Threading.Tasks.Task AwaitInput = AwaitConfirmation();

                        return;

                    }
                    else if (GetVehicle.Class == VehicleClass.Emergency)
                    {
                        // If ProtectEmergencyVehicles is true check if emergency vehicle and prompt are you sure?
                        MsgID = Game.DisplayNotification("<b>~y~Delete Vehicle:</b>~s~~n~ Are you sure you want to delete an emergency vehicle?~n~    [<b>~r~"+ ConfirmString + "~s~</b>]    [<b>~b~"+ DeclineString + "~s~</b>]");
                        System.Threading.Tasks.Task AwaitInput = AwaitConfirmation();

                        return;
                    }

                    // Else we can delete the vehicle
                    DeleteVehicle();

                } else
                {
                    // There was no vehicle nearby...
                    Game.DisplayNotification("Could not find vehicle to delete! Try getting a little closer.");
                }

            }
            catch (Exception e)
            {
                ErrorLogger(e, "Activation", "Error during vehicle check execution");
            }
        }

        public static async System.Threading.Tasks.Task AwaitConfirmation()
        {
            try
            {
                Command_Debug("AwaitConfirmation Task started");
                int SecsToWait = 10; // Wait 10 seconds
                int i = 0;
                AwaitingInput = true;
                while (AwaitingInput == true)
                {
                    if (i == SecsToWait*2) // Wait 10 seconds 
                    {
                        AwaitingInput = false;
                    } else
                    {
                        await System.Threading.Tasks.Task.Delay(500);
                        i++;
                    }
                }
                Command_Debug("AwaitConfirmation Task completed"); 
            }
            catch (Exception e)
            {
                ErrorLogger(e, "AwaitConfirmation", "Error waiting for confirmation");
            }
        } 

        private static void DeleteVehicle()
        {

            try
            {
                // Check if there is a driver and delete them ( https://github.com/waynieoaks/RPH_Delete_Vehicle/issues/1 ) //
                Ped GetDriver = GetVehicle.Driver;
                if ( (GetDriver != null) && (GetDriver != Game.LocalPlayer.Character) )
                {
                    GetDriver.Delete();
                }

                // Now delete vehicle
                GetVehicle.Delete();
            }
            catch (Exception e)
            {
                ErrorLogger(e, "Activation", "Error during vehicle delete execution");
            }
        }

            private static void LoadValuesFromIniFile()
        {
            InitializationFile ini = new InitializationFile(INIpath);
            ini.Create();

            try
            {
                //Keyboard ini

                //  Delete vehicle keys
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

                // Confirm keys
                if (ini.DoesKeyExist("Keyboard", "ConfirmKey")) { 
                    ConfirmKey = ini.ReadEnum<Keys>("Keyboard", "ConfirmKey", Keys.Y);
                    ConfirmString = ini.Read("Keyboard", "ConfirmKey", "Y");
                }
                else
                {
                    ini.Write("Keyboard", "ConfirmKey", "Y");
                    ConfirmKey = Keys.Y;
                    ConfirmString = "Y";
                }


                if (ini.DoesKeyExist("Keyboard", "ConfirmModifierKey")) { 
                    ConfirmModifierKey = ini.ReadEnum<Keys>("Keyboard", "ConfirmModifierKey", Keys.None);
                    if (ConfirmModifierKey != Keys.None)
                    {
                        ConfirmString = ini.Read("Keyboard", "ConfirmModifierKey", "") + "+" + ConfirmString;
                    }
                }
                else
                {
                    ini.Write("Keyboard", "ConfirmModifierKey", "None");
                    ConfirmModifierKey = Keys.None;
                    // Don't need to add modifier to ConfirmString as default is none
                }

                // Decline keys
                if (ini.DoesKeyExist("Keyboard", "DeclineKey")) { 
                    DeclineKey = ini.ReadEnum<Keys>("Keyboard", "DeclineKey", Keys.N);
                    DeclineString = ini.Read("Keyboard", "DeclineKey", "N");
                }
                else
                {
                    ini.Write("Keyboard", "DeclineKey", "N");
                    DeclineKey = Keys.N;
                    DeclineString = "N";
                }

                if (ini.DoesKeyExist("Keyboard", "DeclineModifierKey")) { 
                    DeclineModifierKey = ini.ReadEnum<Keys>("Keyboard", "DeclineModifierKey", Keys.None);
                    DeclineString = ini.Read("Keyboard", "DeclineModifierKey", "") + "+" + DeclineString;
                }
                else
                {
                    ini.Write("Keyboard", "DeclineModifierKey", "None");
                    DeclineModifierKey = Keys.None;
                    // Don't need to add modifier to DeclineString as default is none
                }

                // We should probably check if confirm and decline are the same keybindings here. 
                if (ConfirmString == DeclineString)
                {
                    //Oops, that isn't going to work.
                    Game.LogTrivial("Delete Vehicle: ERROR: Confirm key and decline key cannot be the same, switching to default. Please check your .ini");
                    Game.DisplayNotification("<b>~y~Delete Vehicle: ~r~ERROR:</b>~s~~n~ Confirm key and decline key cannot be the same, switching to default.~n~~b~Please check your .ini~s~");
                    ConfirmKey = Keys.Y;
                    ConfirmModifierKey = Keys.None;
                    ConfirmString = "Y";
                    DeclineKey = Keys.N;
                    DeclineString = "N";
                    DeclineModifierKey = Keys.None;
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

                //if (ini.DoesKeyExist("Other", "ProtectCurrentVehicle")) { ProtectCurrentVehicle = ini.ReadBoolean("Other", "ProtectCurrentVehicle", true); }
                //else
                //{
                //    ini.Write("Other", "ProtectCurrentVehicle", "true");
                //    ProtectCurrentVehicle = true;
                //}

                //if (ini.DoesKeyExist("Other", "ProtectlastVehicle")) { ProtectLastVehicle = ini.ReadBoolean("Other", "ProtectlastVehicle", true); }
                //else
                //{
                //    ini.Write("Other", "ProtectlastVehicle", "true");
                //    ProtectLastVehicle = true;
                //}

                //if (ini.DoesKeyExist("Other", "ProtectEmergencyVehicles")) { ProtectEmergencyVehicles = ini.ReadBoolean("Other", "ProtectEmergencyVehicles", true); }
                //else
                //{
                //    ini.Write("Other", "ProtectEmergencyVehicles", "true");
                //    ProtectEmergencyVehicles = true;
                //}

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
