using Aspect.MenuLib;
using BepInEx;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using System.Reflection;
using UnityEngine;
using Utilla;
using UnityEngine.InputSystem;
using System.Linq;
using Aspect.Utilities;
using System;

namespace Aspect.Plugin
{
    /// <summary>
    /// This class handles applying harmony patches to the game, and handles the monke UI.
    /// </summary>
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")] // Make sure to add Utilla 1.5.0 as a dependency!
    [ModdedGamemode]
    public class Plugin : BaseUnityPlugin
    {
        // Plugin data
        private const string modGUID = "aspect.cheat.panel";
        public const string modVersion = "6.2.3";
        private const string modName = "Aspect - Cheat Panel v6.2.3";

        private static Harmony harmony;
        public static bool Patched { get; private set; } = false;

        // Load harmony
        private void OnEnable()
        {
            if (!Patched)
            {
                if (harmony == null)
                {
                    harmony = new Harmony(modGUID);
                }
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Patched = true;
            }
        }

        // Unload harmony
        private void OnDisable()
        {
            if (harmony != null && Patched)
            {
                harmony.UnpatchSelf();
                Patched = false;
            }
        }

        // Run menu-update
        private void LateUpdate()
        {
            if (Patched && GorillaLocomotion.Player.Instance)
            {
                MenuLib.Update.Run_OnUpdate(GorillaLocomotion.Player.Instance);
            }
        }

        // These are the gui variables, should not be touched, except for special equations 
        private Rect windowRect = new Rect(20, 20, 350, 500);
        private bool isDragging = false;
        private Vector2 dragOffset;
        private Vector2 scrollPosition = Vector2.zero;
        private string LobbyToJoin = "enter lobby code";
        private float MovementSpeedWASD = 5f;
        private bool DoKeyboardMovement = false;
        private int GUICooldown;
        private bool IsOpen = false;
        private string Name = "name to change to";
        private string[] Categorys = { "Info", "Room", "Buttons" };
        private int CurrentCategory = 0;
        private float R = 1f;
        private float G = 1f;
        private float B = 1f;
        private bool clickStuffWithMouse = false;

        // Handles and runs GUI
        private void OnGUI()
        {
            if (Patched)
            {
                // Get input to open GUI
                if (Keyboard.current.insertKey.isPressed && GUICooldown + 30 < Time.frameCount)
                {
                    IsOpen = !IsOpen;
                    GUICooldown = Time.frameCount;
                }

                if (IsOpen) // Check if gui is open
                {
                    // Set fontstyles
                    GUI.skin.window.fontStyle = FontStyle.BoldAndItalic;
                    GUI.skin.label.fontStyle = FontStyle.Italic;
                    GUI.skin.button.fontStyle = FontStyle.Italic;
                    GUI.skin.toggle.fontStyle = FontStyle.Italic;

                    // Create a draggable window
                    windowRect = GUI.Window(0, windowRect, WindowFunction, $"Aspect Cheat Panel {modVersion} | {(int)(1f / Time.deltaTime)}");
                }

                GUIStyle style = new GUIStyle(GUI.skin.label); // Create a new GUIStyle, copying the default label style
                style.normal.textColor = Color.magenta; // Set the text color to magenta
                style.fontSize = 20; // Set the font size

                // Starting position for the first label
                float startY = 7.5f;

                // Loop through each string in the list and create a label for it
                foreach (Menu.ButtonTemplate button in MenuLib.Update.menu.Buttons)
                {
                    // Continue the next button if the buttons isnt turned on
                    if (!button.ButtonState) continue;

                    Rect labelRect = new Rect(10, startY, 1000, 35); // Define the position and size of the label
                    GUI.Label(labelRect, button.Text, style); // Create the label with the current string

                    // Update the starting Y position for the next label
                    startY += 20f;
                }
            }
        }

        void WindowFunction(int _)
        {
            // Add Categorys
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < Categorys.Length; i++)
            {
                if (GUILayout.Button(Categorys[i]))
                {
                    CurrentCategory = i;
                    scrollPosition = Vector2.zero;
                }
            }
            GUILayout.EndHorizontal();

            switch (CurrentCategory)
            {
                // INFO
                case 0:
                    GUILayout.Label("Welcome to the Aspect Cheat Panel GUI, here you can use (almost) every mod on pc. There might be some bugs since this is a new feature, please report them in my server if you find any, and tell if you have any problems using this GUI.");
                    break;

                // ROOM
                case 1:
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition); // Begin the scroll view

                    GUI.backgroundColor = Color.red; // Set gui color

                    if (GUILayout.Button("Quit")) Environment.FailFast("Exited GTAG"); // Disconnect button

                    if (GUILayout.Button("Disconnect")) PhotonNetwork.Disconnect(); // Disconnect button

                    LobbyToJoin = GUILayout.TextField(LobbyToJoin.ToUpper()); // Join specific lobby
                    if (GUILayout.Button("Join Lobby")) PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(LobbyToJoin);

                    Name = GUILayout.TextField(Name.ToUpper()); // Change name
                    if (GUILayout.Button("Change Name")) Util.ChangeName(Name);

                    // Change color
                    GUILayout.Label($"(R:{(int)(R * 9)} G:{(int)(G * 9)} B:{(int)(B * 9)})");
                    R = GUILayout.HorizontalSlider(R, 0, 1);
                    G = GUILayout.HorizontalSlider(G, 0, 1);
                    B = GUILayout.HorizontalSlider(B, 0, 1);
                    if (GUILayout.Button("Color")) Util.ChangeColor(R, G, B);

                    GUILayout.EndScrollView(); // End scrollview
                    break;

                // BUTTONS
                case 2:
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition); // Begin the scroll view

                    GUI.backgroundColor = Color.red; // Set gui color

                    // Keyboard movement
                    GUILayout.Label($"Keyboard Movement Speed: {(int)MovementSpeedWASD}");
                    MovementSpeedWASD = GUILayout.HorizontalSlider(MovementSpeedWASD, 1, 20);
                    DoKeyboardMovement = GUILayout.Toggle(DoKeyboardMovement, "Keyboard Movement");
                    clickStuffWithMouse = GUILayout.Toggle(clickStuffWithMouse, "Click Ingame Buttons");

                    // Add menubuttons to gui
                    Menu.ButtonTemplate[] Buttons = MenuLib.Update.menu.FavoritedMods.Concat(MenuLib.Update.menu.Buttons).ToArray();
                    for (int i = 0; i < MenuLib.Update.menu.Buttons.Count; i++)
                    {
                        Menu.ButtonTemplate button = Buttons[i]; // Get button

                        if (!button.IsFavorited) GUI.backgroundColor = button.ButtonState ? Color.green : Color.red; // Handle button colors
                        else if (button.IsFavorited) GUI.backgroundColor = Color.yellow;

                        if (GUILayout.Button(button.Text)) // Create GUI button
                        {
                            MenuLib.Update.AllGunsOnPC = true; // Turn on PC guns for gun-mods

                            if (button.Toggle) button.ButtonState = !button.ButtonState; // turn on/off button
                            if (Aspect.MenuLib.Input.instance.CheckButton(MenuLib.Input.ButtonType.grip)) Menu.FavoriteButton(button, Aspect.MenuLib.Update.menu, !button.IsFavorited); // Favorite mod

                            // Check and run mods
                            try
                            {
                                if (button.ButtonState || !button.Toggle)
                                {
                                    if (button.OnEnable != null)
                                    {
                                        button.OnEnable.Invoke();
                                    }
                                    if (button.OnUpdate != null)
                                    {
                                        button.OnUpdate.Invoke();
                                    }
                                }
                                else
                                {
                                    if (button.OnDisable != null)
                                    {
                                        button.OnDisable.Invoke();
                                    }
                                }

                                // Put mod description on board and send notification
                                if (!button.Text.Contains(">") && !button.Text.Contains("<") && button.Text != "Disconnect" && button.Description != "")
                                {
                                    string color = !button.ButtonState ? "green" : "red";
                                    if (!button.Toggle) color = "green";
                                    string text = $"[<color={color}>{button.Text}</color>] {button.Description}";
                                    Board.SetBoardText($"Aspect Cheat Panel {modVersion}", MenuLib.Update.GetBoardText(MenuLib.Update.menu, $"\n\nCurrent Mod:\n{text}"));
                                    NotifiLib.SendNotification(text);
                                }
                            }
                            catch { }
                            Menu.RefreshMenu(MenuLib.Update.menu); // Refresh to update the menu
                        }
                        GUI.backgroundColor = Color.white; // Reset gui color
                    }

                    // End the scroll view
                    GUILayout.EndScrollView();
                    break;

                // ERROR CATCH
                default:
                    Debug.LogError("Current Category is invalid");
                    break;
            }

            // Make the window draggable
            GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));
        }


        void Update()
        {
            // Do keyboard movement
            if (DoKeyboardMovement) GorillaMods.DoKeyboardMovement(DoKeyboardMovement, MovementSpeedWASD);
            else GorillaMods.DoKeyboardMovement(false, 0);

            // Do click button stuff
            if (clickStuffWithMouse)
            {
                GorillaMods.ClickButtonsGun();
            }

            // Check for mouse events to handle dragging
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && windowRect.Contains(Event.current.mousePosition))
            {
                isDragging = true;
                dragOffset = Event.current.mousePosition - windowRect.position;
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                isDragging = false;
            }

            if (isDragging)
            {
                // Update the window position while dragging
                windowRect.position = Event.current.mousePosition - dragOffset;
            }
        }

        // Utilla Stuff
        public static bool inAllowedRoom = false;

        [ModdedGamemodeJoin]
        private void RoomJoined(string gamemode)
        {
            // The room is modded. Enable mod stuff.
            inAllowedRoom = true;
        }

        [ModdedGamemodeLeave]
        private void RoomLeft(string gamemode)
        {
            // The room was left. Disable mod stuff.
            inAllowedRoom = false;
        }
    }
}
