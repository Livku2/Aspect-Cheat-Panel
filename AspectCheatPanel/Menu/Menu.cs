using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using GorillaLocomotion;
using Aspect.Utilities;
using Photon.Pun;
using GorillaNetworking;
using static Aspect.MenuLib.Menu;
using Aspect.Plugin;
using UnityEngine.Animations.Rigging;

namespace Aspect.MenuLib
{
    /// <summary>
    /// This class handles updates, menu-setup, mod-setup, and board-setup.
    /// 
    /// To create a Button there's alot of different settings, use this as a start:
    /// new Menu.ButtonTemplate { Text = "", Description = "" }
    /// 
    /// For a setting button yyou can use this as a start instead, makse sure Text and Menu.GetButtonFromName("", menu) is the same value:
    /// new Menu.ButtonTemplate { Text = "", OnUpdate = () => { bool = !bool; menu.ExtraNameValues[Menu.GetButtonFromName("", menu)] = bool; }, ExtraValueText = bool, Description = "" }
    /// 
    /// If you want further instructions on what the different button-variables do,
    /// go into Menu.ButtonTemplates.
    /// </summary>
    internal static class Update
    {
        // Returns appropriate board text
        public static string GetBoardText(Menu.MenuTemplate menu, string text = "", bool buttonToggle = false)
        {
            // Get daytime text
            string[] time = DateTime.Now.ToString("hh.tt").Split('.');
            string daytimeText;
            if (time[1] == "PM")
            {
                if (int.Parse(time[0]) >= 6)
                {
                    daytimeText = "Good evening!";
                }
                else
                {
                    daytimeText = "Good afternoon!";
                }
            }
            else
            {
                if (int.Parse(time[0]) >= 6)
                {
                    daytimeText = "Good morning!";
                }
                else
                {
                    daytimeText = "Good night!";
                }
            }

            // Make board text
            List<string> boardTextArray = new List<string>();
            if (!buttonToggle)
            {
                boardTextArray.Add($"{daytimeText} ");
                boardTextArray.Add("Thanks for choosing <color=yellow>aspects cheat panel</color>, join my discord at discord.gg/aspects-crib.\n");
                boardTextArray.Add($"There is currently {menu.Buttons.Count + menu.SettingButtons.Count} mods on this mod menu.\n\n");
                boardTextArray.Add("Controls:\n");
                boardTextArray.Add("<color=grey>    <press secondary to open the mod menu></color>\n");
                boardTextArray.Add("<color=grey>    <press the buttons with your index finger to use them></color>\n\n");
                boardTextArray.Add("<color=red>This text will vanish when clicking your first button.</color>");
            }
            else
            {
                boardTextArray.Add("discord.gg/aspects-crib\n\n");
                boardTextArray.Add(text);
            }

            return string.Join("", boardTextArray).ToUpper();
        }

        // Font(s)
        public static Font menuTitleFont { get; private set; }
        public static Font menuButtonFont { get; private set; }

        // Menu variables
        public static Menu.MenuTemplate menu;
        public static bool isSetup = false;

        // Gun variables
        public static bool AllGunsOnPC = false;
        public static bool GunLock = true;

        // Report stuff
        public static bool DisconnectOnReport = false;
        public static float reportCount = 0;

        // Variables collected from start
        public static Vector3 leftPosOffset = Vector3.zero;
        public static Vector3 rightPosOffset = Vector3.zero;
        public static GameObject thirdPersonCameraGO { get; private set; }

        // Notification
        public static bool tooltipNotification = true;

        // Rig
        public static Vector3 defaultHeadRot;
        public static Vector3 rightHandOffset;
        public static Vector3 leftHandOffset;

        public static void Run_OnUpdate(Player __instance)
        {
            // If this is true, the menu won't run
            bool extraStateDepender = true;

            if (extraStateDepender)
            {
                // Setup
                if (!isSetup)
                {
                    // Get current armlength if you don't have it already
                    if (leftPosOffset == Vector3.zero)
                    {
                        leftPosOffset = __instance.leftHandOffset;
                    }
                    if (rightPosOffset == Vector3.zero)
                    {
                        rightPosOffset = __instance.rightHandOffset;
                    }

                    // Get third person camera
                    if (GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera"))
                    {
                        thirdPersonCameraGO = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
                    }

                    // Get default rig vars
                    defaultHeadRot = GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset;
                    rightHandOffset = GorillaTagger.Instance.offlineVRRig.rightHand.trackingRotationOffset;
                    leftHandOffset = GorillaTagger.Instance.offlineVRRig.leftHand.trackingRotationOffset;

                    // Initialize ESP colorways
                    GorillaMods.SetupColorways();

                    // Initialize fonts
                    menuTitleFont = Font.CreateDynamicFontFromOSFont("Agency FB", 24);
                    menuButtonFont = Font.CreateDynamicFontFromOSFont("Agency FB", 20);

                    // Create menu
                    menu = Menu.MenuTemplate.CreateMenu(
                        $"{Plugin.Plugin.modVersion}",
                        Color.white,
                        new Vector3(0.1f, 1f, 1f),
                        __instance.leftControllerTransform.gameObject,
                        true
                    );
                    menu.SetupMenuThemes();

                    // CREATE CATEGORIES
                    List<CategoryTemplate> Settings = new List<CategoryTemplate>() // Create SETTINGS category
                    {
                        new CategoryTemplate
                        {
                            Name = "Menu - Settings",
                            ID = "MENU_SETTING_1",
                            ButtonList = new List<ButtonTemplate>
                            {
                                new Menu.ButtonTemplate { Text = "Change Menu Theme", OnUpdate = () => { menu.ExtraNameValues[Menu.GetButtonFromName("Change Menu Theme", menu)] = menu.ChangeMenuTheme(); }, ExtraValueText = (string)menu.menuThemes[menu.menuTheme][1], Toggle = false, Description = "Change the theme of the mod menu." },
                                new Menu.ButtonTemplate { Text = "Tooltip Notifications", OnUpdate = () => { tooltipNotification = !tooltipNotification; menu.ExtraNameValues[Menu.GetButtonFromName("Tooltip Notifications", menu)] = tooltipNotification.ToString(); }, Toggle = false, ExtraValueText = tooltipNotification.ToString(), Description = "Turns off the tooltip notifications that's getting send every time you click a button." },
                                new Menu.ButtonTemplate { Text = "Unlock Framerate", OnEnable = () => GorillaMods.UnlockFramrate(), OnDisable = () => GorillaMods.UnlockFramrate(true), Description = "Unlocks framerate" },
                                new Menu.ButtonTemplate { Text = "Turn Off NotifiLib", OnUpdate = () => { menu.ExtraNameValues[Menu.GetButtonFromName("Turn Off NotifiLib", menu)] = !NotifiLib.TurnOffNotifications(); }, ExtraValueText = (!NotifiLib.IsEnabled).ToString(), Toggle = false, Description = "Disables incoming notifications." },
                                new Menu.ButtonTemplate { Text = "Gun Lock", OnUpdate = () => { GunLock = !GunLock; menu.ExtraNameValues[Menu.GetButtonFromName("Gun Lock", menu)] = GunLock; }, ExtraValueText = GunLock.ToString(), Toggle = false, Description = "Makes the pointer on guns locking onto people." }
                            }
                        },
                        new CategoryTemplate
                        {
                            Name = "Visual - Settings",
                            ID = "VISUAL_SETTING_1",
                            ButtonList = new List<ButtonTemplate>
                            {
                                new Menu.ButtonTemplate { Text = "ESP Colorway", OnUpdate = () => { menu.ExtraNameValues[Menu.GetButtonFromName("ESP Colorway", menu)] = GorillaMods.ChangeESPTheme(); }, ExtraValueText = GorillaMods.colorways[GorillaMods.ColorwayESP], Toggle = false, Description = "Change the colorway of the esp." }
                            }
                        },
                        new CategoryTemplate
                        {
                            Name = "Rig Mods - Settings",
                            ID = "RIGMODS_SETTING_1",
                            ButtonList = new List<ButtonTemplate>
                            {
                                new Menu.ButtonTemplate { Text = "Create Rig When Disabling", OnUpdate = () => { menu.ExtraNameValues[Menu.GetButtonFromName("Create Rig When Disabling", menu)] = GorillaPatches.OnDisablePatch.DisableCreateRig(); }, ExtraValueText = GorillaPatches.createRig.ToString(), Toggle = false, Description = "Disables creating a rig when ." }
                            }
                        }
                    };
                    menu.SettingCategorys = Settings;
                    List<ButtonTemplate> buttons = new List<ButtonTemplate>();
                    List<List<ButtonTemplate>> buttonLists = new List<List<ButtonTemplate>>();
                    foreach (CategoryTemplate category in Settings)
                    {
                        buttonLists.Add(category.ButtonList);
                    }
                    foreach (List<ButtonTemplate> buttonList in buttonLists)
                    {
                        foreach (ButtonTemplate button in buttonList)
                        {
                            buttons.Add(button);
                        }
                    }
                    menu.SettingButtons = buttons;
                    CategoryTemplate SettingsMain = new CategoryTemplate // Create Settings category for main menu page
                    {
                        Name = "Settings",
                        ID = "SETTING_1", // DO NOT EDIT THE ID OF THIS CATEGORY
                    };
                    menu.Categorys.Add(SettingsMain);

                    CategoryTemplate Room = new CategoryTemplate // Create ROOM_MISC category
                    {
                        Name = "Room",
                        ID = "ROOM",
                        ButtonList = new List<ButtonTemplate>()
                        {
                            new Menu.ButtonTemplate { Text = "Quit Game", OnUpdate = () => Environment.FailFast("Exited GTAG"), Toggle = false },
                            new Menu.ButtonTemplate { Text = "Join Random Room", OnUpdate = () => PhotonNetwork.JoinRandomRoom(), Toggle = false},
                            new Menu.ButtonTemplate { Text = "Accept TOS", OnUpdate = () => GorillaMods.AcceptTOS(), Description = "Accepts gorillas terms of service." },
                            new Menu.ButtonTemplate { Text = "Switch Hand", OnUpdate = () => menu.SwitchHands(), Toggle = false, Description = "Switches current mainhand." },
                            new Menu.ButtonTemplate { Text = "Disable Quitbox", OnUpdate = () => GorillaMods.DisableQuitbox(true), Toggle = false, Description = "Disables quitbox." }
                        }
                    };
                    menu.Categorys.Add(Room);
                    menu.Buttons = menu.Buttons.Concat(Room.ButtonList).ToList();

                    CategoryTemplate Safety = new CategoryTemplate // Create SAFETY category
                    {
                        Name = "Safety",
                        ID = "SAFETY",
                        ButtonList = new List<ButtonTemplate>()
                        {
                            new Menu.ButtonTemplate { Text = "Disconnect On Report", Working = false, OnUpdate = () => { DisconnectOnReport = true; }, OnDisable = () => { DisconnectOnReport = false; }, Description = "Disconnects you when other player reports you." },
                            new Menu.ButtonTemplate { Text = "AntiCheat Disconnect", OnUpdate = () => GorillaMods.AntiCheatDisconnect(), OnDisable = () => GorillaMods.AntiCheatDisconnect(true), ButtonState = ConfigManager.SAFEMODE.Value, Description = "Disconnects you when the anticheat is activated." }
                        }
                    };
                    menu.Categorys.Add(Safety);
                    menu.Buttons = menu.Buttons.Concat(Safety.ButtonList).ToList();

                    CategoryTemplate Visuals = new Menu.CategoryTemplate // Create VISUALS category
                    {
                        Name = "Visuals",
                        ID = "VISUALS",
                        ButtonList = new List<ButtonTemplate>()
                        {
                            new Menu.ButtonTemplate { Text = "Performance Mode", OnEnable = () => GorillaMods.SetPerformanceMode(), OnDisable = () => GorillaMods.SetPerformanceMode(true), Description = "Boosts your fps by turning down the visuals." },
                            new Menu.ButtonTemplate { Text = "Simple Camera Mod", Working = false, OnUpdate = () => GorillaMods.SimpleCameraMod(thirdPersonCameraGO), Description = "Gives you a good first person view from your computer." },
                            new Menu.ButtonTemplate { Text = "Chams", OnUpdate = () => GorillaMods.Chams(), OnDisable = () => GorillaMods.Chams(true), Description = "Makes players glow through the wall." },
                            new Menu.ButtonTemplate { Text = "Tracers", OnUpdate = () => GorillaMods.Tracers(Input.instance.CheckButton(Input.ButtonType.grip, true), true), OnDisable = () => GorillaMods.Tracers(false, true, true), Description = "Hold left grip to draw tracers." },
                            new Menu.ButtonTemplate { Text = "Box ESP", OnUpdate = () => GorillaMods.BoxESP(), OnDisable = () => GorillaMods.BoxESP(true), Description = "Draws a box around players." },
                            new Menu.ButtonTemplate { Text = "Change Time Of Day", OnUpdate = () => GorillaMods.ChangeTimeOfDay(), Toggle = false, Description = "Changes time of day to the next time of day." },
                            new Menu.ButtonTemplate { Text = "Change Time To Day", OnUpdate = () => GorillaMods.ChangeTimeOfDay(false, 4), Toggle = false, Description = "Changes time to day." },
                            new Menu.ButtonTemplate { Text = "Change Time To Night", OnUpdate = () => GorillaMods.ChangeTimeOfDay(false, 0), Toggle = false, Description = "Changes time to night." }
                        }
                    };
                    menu.Categorys.Add(Visuals);
                    menu.Buttons = menu.Buttons.Concat(Visuals.ButtonList).ToList();

                    CategoryTemplate Movement = new Menu.CategoryTemplate // Create MOVEMENT category
                    {
                        Name = "Movement",
                        ID = "MOVEMENT",
                        ButtonList = new List<ButtonTemplate>()
                        {
                            new Menu.ButtonTemplate { Text = "Platforms", OnUpdate = () => GorillaMods.Platforms(), OnDisable = () => GorillaMods.Platforms(true), Description = "Use grips to activate platforms." },
                            new Menu.ButtonTemplate { Text = "Sticky Platforms", OnUpdate = () => GorillaMods.Platforms(false, true), OnDisable = () => GorillaMods.Platforms(true), Description = "Use grips to activate platforms." },
                            new Menu.ButtonTemplate { Text = "Silent Platforms", OnUpdate = () => GorillaMods.Platforms(false, true, 0, true), OnDisable = () => GorillaMods.Platforms(true), Description = "Use grips to activate silent platforms." },
                            new Menu.ButtonTemplate { Text = "Frozone", OnUpdate = () => GorillaMods.Frozone(), Description = "Press your grips to spawn slippery platforms." },
                            new Menu.ButtonTemplate { Text = "No-Clip", OnUpdate = () => GorillaMods.NoClip(), OnDisable = () => GorillaMods.NoClip(false), Description = "Press trigger to turn off colliders." },
                            new Menu.ButtonTemplate { Text = "Climb Anywhere", OnUpdate = () => GorillaMods.ClimbAnywhere(), OnDisable = () => GorillaMods.ClimbAnywhere(true), Description = "Use grips while touching a surface to activate climbing." },
                            new Menu.ButtonTemplate { Text = "Iron Monkey", OnUpdate = () => GorillaMods.IronMonkey(ConfigManager.IRONMONKEYSPEED.Value), Description = "Use Primary-buttons to fly like you have a jetpack." },
                            new Menu.ButtonTemplate { Text = "Super Monkey", OnUpdate = () => GorillaMods.SuperMonkey(ConfigManager.SUPERMONKEYSPEED.Value), OnDisable = () => GorillaMods.SuperMonkey(0f, false), Description = "Seconday to fly, Primary to activate low gravity." },
                            new Menu.ButtonTemplate { Text = "Fast Super Monkey", OnUpdate = () => GorillaMods.SuperMonkey(ConfigManager.SUPERMONKEYSPEED.Value * 0.571f), OnDisable = () => GorillaMods.SuperMonkey(0f, false), Description = "Seconday to fly, Primary to activate low gravity." },
                            new Menu.ButtonTemplate { Text = "Low Gravity", OnUpdate = () => GorillaMods.ChangeGravity(false), OnDisable = () => GorillaMods.ChangeGravity(true), Description = "Sets low gravity." },
                            new Menu.ButtonTemplate { Text = "High Gravity", OnUpdate = () => GorillaMods.ChangeGravity(false, 20f), OnDisable = () => GorillaMods.ChangeGravity(true), Description = "Sets high gravity." },
                            new Menu.ButtonTemplate { Text = "Silent Handtaps", OnUpdate = () => GorillaMods.SetHandtapVolume(0f), OnDisable = () => GorillaMods.SetHandtapVolume(), Description = "Makes your steps not make a single sound!" },
                            new Menu.ButtonTemplate { Text = "Loud Handtaps", OnUpdate = () => GorillaMods.SetHandtapVolume(1f), OnDisable = () => GorillaMods.SetHandtapVolume(), Description = "Makes your steps very, very loud!" },
                            new Menu.ButtonTemplate { Text = "Speedboost (7.2)", OnUpdate = () => GorillaMods.Speedboost(7.2f, 1.1f), OnDisable = () => GorillaMods.Speedboost(0, 0, true), Description = "Makes you go faster unnoticeable." },
                            new Menu.ButtonTemplate { Text = "Speedboost (7.6)", OnUpdate = () => GorillaMods.Speedboost(7.6f, 1.1f), OnDisable = () => GorillaMods.Speedboost(0, 0, true), Description = "Makes you go faster unnoticeable." },
                            new Menu.ButtonTemplate { Text = "Speedboost (8)", OnUpdate = () => GorillaMods.Speedboost(8, 1.1f), OnDisable = () => GorillaMods.Speedboost(0, 0, true), Description = "Makes you go faster unnoticeable." },
                            new Menu.ButtonTemplate { Text = "Speedboost (12)", OnUpdate = () => GorillaMods.Speedboost(12, 1.3f), OnDisable = () => GorillaMods.Speedboost(0, 0, true), Description = "Makes you go faster." },
                            new Menu.ButtonTemplate { Text = "Up And Down", OnUpdate = () => GorillaMods.UpAndDown(), Description = "Left-trigger to accelerate upwards, and right to accelerate downwards." },
                            new Menu.ButtonTemplate { Text = "Teleport Gun", OnUpdate = () => GorillaMods.TeleportGun(), OnDisable = () => GorillaExtensions.GunTemplate(true), Description = "Grip to aim, trigger to teleport to the pointer." },
                            new Menu.ButtonTemplate { Text = "Teleport To Random Player", OnUpdate = () => GorillaMods.TeleportToRandomPlayer(), Toggle = false, Description = "Teleports, you to a random player." },
                            new Menu.ButtonTemplate { Text = "Teleport To Random Tagged Player", OnUpdate = () => GorillaMods.TeleportToRandomPlayer("TAGGED"), Toggle = false, Description = "Teleports, you to a random tagged player." },
                            new Menu.ButtonTemplate { Text = "Teleport To Random Untagged Player", OnUpdate = () => GorillaMods.TeleportToRandomPlayer("UNTAGGED"), Toggle = false, Description = "Teleports, you to a random untagged player." },
                            new Menu.ButtonTemplate { Text = "Projectile Teleport", OnUpdate = () => GorillaMods.ProjectileTeleport(false), Description = "Throw/shoot a projectile to teleport." },
                            new Menu.ButtonTemplate { Text = "Ride Projectile", OnUpdate = () => GorillaMods.ProjectileTeleport(true), Description = "Throw/shoot a projectile to ride it." },
                            new Menu.ButtonTemplate { Text = "Strong Wall Walk [GRIP]", OnUpdate = () => GorillaMods.WallWalk(10, Input.instance.CheckButton(Input.ButtonType.grip, false)), Description = "Pulls you towards whatever you're touching fast." },
                            new Menu.ButtonTemplate { Text = "Legit Wall Walk [GRIP]", OnUpdate = () => GorillaMods.WallWalk(2, Input.instance.CheckButton(Input.ButtonType.grip, false)), Description = "Pulls you towards whatever you're touching slowly." },
                            new Menu.ButtonTemplate { Text = "Longarms", OnUpdate = () => GorillaMods.LongArms(Vector3.forward / 8, Vector3.forward / 8), OnDisable = () => GorillaMods.LongArms(Vector3.zero, Vector3.zero, true), Description = "Gives you longarms, like your controllers are on sticks." },
                            new Menu.ButtonTemplate { Text = "Legit Longarms", OnUpdate = () => GorillaMods.LongArms(Vector3.forward / 40, Vector3.forward / 40), OnDisable = () => GorillaMods.LongArms(Vector3.zero, Vector3.zero, true), Description = "Gives you unnoticeable longarms, like your controllers are on sticks." },
                            new Menu.ButtonTemplate { Text = "C4", OnUpdate = () => GorillaMods.C4(), OnDisable = () => GorillaMods.C4(0, true), Description = "Left grip to plant, and right grip to detonate." },
                            new Menu.ButtonTemplate { Text = "Spider Monkey [OLD AND BUGGY]", OnUpdate = () => GorillaMods.SpiderMonkey(), OnDisable = () => GorillaMods.SpiderMonkey(true), Description = "Makes you swing from brach to branch, with high velocitys." },
                            new Menu.ButtonTemplate { Text = "New Spider Monkey", OnUpdate = () => GorillaMods.NewSpiderMonkey(true, true), OnDisable = () => GorillaMods.NewSpiderMonkey(false, false, true), Description = "Makes you able to shoot webs, and swing around." },
                            new Menu.ButtonTemplate { Text = "Left Web Shooter", OnUpdate = () => GorillaMods.NewSpiderMonkey(true, false), OnDisable = () => GorillaMods.NewSpiderMonkey(false, false, true), Description = "Makes you able to shoot webs and swing with left hand only." },
                            new Menu.ButtonTemplate { Text = "Right Web Shooter", OnUpdate = () => GorillaMods.NewSpiderMonkey(false, true), OnDisable = () => GorillaMods.NewSpiderMonkey(false, false, true), Description = "Makes you able to shoot webs and swing with right hand only." }
                        }
                    };
                    menu.Categorys.Add(Movement);
                    menu.Buttons = menu.Buttons.Concat(Movement.ButtonList).ToList();

                    // Creates buttons that needs top have their own buttons referenced in .OnUpdate() for the Advantage category
                    Menu.ButtonTemplate TagSelfButton = new Menu.ButtonTemplate { Text = "Tag Self", Description = "Tags yourself, even without master (It works better with master though)." }; TagSelfButton.OnUpdate = () => GorillaMods.TagSelf(TagSelfButton); TagSelfButton.OnDisable = () => GorillaMods.TagSelf(TagSelfButton);
                    Menu.ButtonTemplate TagAllButton = new Menu.ButtonTemplate { Text = "Tag All", Description = "Tags every player in your current lobby." }; TagAllButton.OnUpdate = () => GorillaMods.TagAll(TagAllButton);
                    
                    CategoryTemplate Advantage = new Menu.CategoryTemplate // Create ADVANTAGE category
                    {
                        Name = "Advantage",
                        ID = "ADVANTAGE",
                        ButtonList = new List<ButtonTemplate>()
                        {
                            new Menu.ButtonTemplate { Text = "Tag Gun", OnUpdate = () => GorillaMods.TagGun(), OnDisable = () => GorillaExtensions.GunTemplate(true), Description = "Use grip to aim and trigger to shoot. It's way better when you have master!" },
                            TagAllButton,
                            new Menu.ButtonTemplate { Text = "Tag Aura", OnUpdate = () => GorillaMods.TagAura(), Description = "When you go close to others, you tag them." },
                            new Menu.ButtonTemplate { Text = "Flick Tag", OnUpdate = () => GorillaMods.FlickTag(), Description = "Hold right grip to flick tag." },
                            new Menu.ButtonTemplate { Text = "Anti Tag Freeze", OnUpdate = () => GorillaMods.TagFreeze(true), Description = "Disable the freeze after you get tagged." },
                            new Menu.ButtonTemplate { Text = "Force Tag Freeze", OnUpdate = () => GorillaMods.TagFreeze(false), Description = "Forced tagfreeze." },
                            new Menu.ButtonTemplate { Text = "No Tag On Join", OnUpdate = () => GorillaMods.NoTagOnJoin(), OnDisable = () => GorillaMods.NoTagOnJoin(true), Description = "When you join a public infection lobby you dont get tagged." },
                            TagSelfButton
                        }
                    };
                    menu.Categorys.Add(Advantage);
                    menu.Buttons = menu.Buttons.Concat(Advantage.ButtonList).ToList();

                    CategoryTemplate Rig = new Menu.CategoryTemplate // Create RIG category
                    {
                        Name = "Rig",
                        ID = "RIG",
                        ButtonList = new List<ButtonTemplate>()
                        {
                            new Menu.ButtonTemplate { Text = "Slow RGB", OnUpdate = () => GorillaMods.RGB(), Extensions = "STUMP", Description = "Changes your color slowly." },
                            new Menu.ButtonTemplate { Text = "Fast RGB", OnUpdate = () => GorillaMods.RGB(0.1f), Extensions = "STUMP", Description = "Changes your color smoothly and quickly." },
                            new Menu.ButtonTemplate { Text = "Slow Strobe", OnUpdate = () => GorillaMods.RGB(1f, true), Extensions = "STUMP", Description = "Changes your color to a random one once every second." },
                            new Menu.ButtonTemplate { Text = "Fast Strobe", OnUpdate = () => GorillaMods.RGB(0.1f, true), Extensions = "STUMP", Description = "Changes your color to a random one once every 0.1 seconds." },
                            new Menu.ButtonTemplate { Text = "Silent Handtaps", OnUpdate = () => GorillaMods.SetHandtapVolume(0f), OnDisable = () => GorillaMods.SetHandtapVolume(), Description = "Makes your steps not make a single sound!" },
                            new Menu.ButtonTemplate { Text = "Loud Handtaps", OnUpdate = () => GorillaMods.SetHandtapVolume(1f), OnDisable = () => GorillaMods.SetHandtapVolume(), Description = "Makes your steps very, very loud!" },
                            new Menu.ButtonTemplate { Text = "Ghost Monkey", OnUpdate = () => GorillaMods.GhostMonkey(), OnDisable = () => GorillaMods.GhostMonkey(false), Description = "Secondary to go out of your rig." },
                            new Menu.ButtonTemplate { Text = "Invisibility", OnUpdate = () => GorillaMods.Invisibility(), OnDisable = () => GorillaMods.Invisibility(false), Description = "Secondary to go invisible." },
                            new Menu.ButtonTemplate { Text = "Spin Head (X)", OnUpdate = () => GorillaMods.HeadSpin("x"), OnDisable = () => GorillaMods.HeadSpin("x", true), Description = "Spins head on X axis." },
                            new Menu.ButtonTemplate { Text = "Spin Head (Y)", OnUpdate = () => GorillaMods.HeadSpin("y"), OnDisable = () => GorillaMods.HeadSpin("y", true), Description = "Spins head on Y axis." },
                            new Menu.ButtonTemplate { Text = "Spin Head (Z)", OnUpdate = () => GorillaMods.HeadSpin("z"), OnDisable = () => GorillaMods.HeadSpin("z", true), Description = "Spins head on Z axis." },
                            new Menu.ButtonTemplate { Text = "Crazy Head", OnEnable = () => GorillaMods.CrazyHead(), OnUpdate = () => GorillaMods.CrazyHead(), OnDisable = () => GorillaMods.CrazyHead(true), Description = "Sets your head to a random rotation." },
                            new Menu.ButtonTemplate { Text = "Go Crazy", OnUpdate = () => GorillaMods.GoCrazy(), OnDisable = () => GorillaMods.GoCrazy(true), Description = "Spins head and hands at the same time." },
                            new Menu.ButtonTemplate { Text = "Helicopter", OnUpdate = () => GorillaMods.Helicopter(), OnDisable = () => { GorillaTagger.Instance.offlineVRRig.enabled = true; }, Description = "If you hold grip you start spinning and flying upwards." },
                            new Menu.ButtonTemplate { Text = "Freeze [GRIP]", OnUpdate = () => GorillaMods.FreezeRig(), OnDisable = () => { GorillaTagger.Instance.offlineVRRig.enabled = true; }, Description = "Makes your rig freeze, but still follow your player position." },
                            new Menu.ButtonTemplate { Text = "Freeze + Spin [GRIP]", OnUpdate = () => GorillaMods.FreezeAndSpin(), OnDisable = () => { GorillaTagger.Instance.offlineVRRig.enabled = true; }, Description = "Makes your rig freeze and spin, but still follow your player position." },
                            new Menu.ButtonTemplate { Text = "Solid Monkeys", OnUpdate = () => GorillaMods.SolidMonkeys(), OnDisable = () => GorillaMods.SolidMonkeys(true), Description = "Makes every other player solid, so you can stand on them." },
                            new Menu.ButtonTemplate { Text = "Projectile Spammer", OnUpdate = () => GorillaMods.ProjectileSpammer(), Extensions = "DETECTED", Description = "Spams projectile when holding grip while holding a slingshot." }
                        }
                    };
                    menu.Categorys.Add(Rig);
                    menu.Buttons = menu.Buttons.Concat(Rig.ButtonList).ToList();

                    CategoryTemplate Fun = new Menu.CategoryTemplate // Create RIG category
                    {
                        Name = "Fun & Random",
                        ID = "FUN",
                        ButtonList = new List<ButtonTemplate>()
                        {
                            new Menu.ButtonTemplate { Text = "Snipe Bug", OnUpdate = () => GorillaMods.SnipeBug(), Description = "Hold grip to get the bug." },
                            new Menu.ButtonTemplate { Text = "Snipe Bat", OnUpdate = () => GorillaMods.SnipeBat(), Description = "Hold grip to get the bat." },
                            new Menu.ButtonTemplate { Text = "Control Bug", OnUpdate = () => GorillaMods.ControlBug(), OnDisable = () => GorillaExtensions.GunTemplate(true), Description = "A gun-mod that makes you able to control the bug." },
                            new Menu.ButtonTemplate { Text = "Allow Stealing Doug", OnUpdate = () => GorillaMods.AllowStealingDoug(), Description = "Makes other people able to grab doug out of others hands." },
                            new Menu.ButtonTemplate { Text = "Allow Stealing Bat", OnUpdate = () => GorillaMods.AllowStealingBat(), Description = "Makes other people able to grab bat out of others hands." },
                            new Menu.ButtonTemplate { Text = "Platform Gun", OnUpdate = () => GorillaMods.PlatformGun(), OnDisable = () => GorillaExtensions.GunTemplate(true), Description = "Spam spawning networked platforms." },
                            new Menu.ButtonTemplate { Text = "Click Buttons Gun", OnUpdate = () => GorillaMods.ClickButtonsGun(), Description = "A gun to click any type of buttons." },
                            new Menu.ButtonTemplate { Text = "Random Sound Spam", OnUpdate = () => GorillaMods.SoundSpam(), Description = "Plays random tap-sounds." },
                            new Menu.ButtonTemplate { Text = "Punch Mod", OnUpdate = () => GorillaMods.PunchMod(), OnDisable = () => GorillaMods.PunchMod(true), Description = "Makes other people able to punch you, to give you knockback." },
                            new Menu.ButtonTemplate { Text = "Snow On Ground", OnUpdate = () => GorillaMods.SnowOnGround(), Description = "Makes you able to pick up snowballs from all surfaces." }
                        }
                    };
                    menu.Categorys.Add(Fun);
                    menu.Buttons = menu.Buttons.Concat(Fun.ButtonList).ToList();

                    CategoryTemplate Master = new Menu.CategoryTemplate // Create MASTER category
                    {
                        Name = "Master",
                        ID = "MASTER",
                        ButtonList = new List<ButtonTemplate>()
                        {
                            new Menu.ButtonTemplate { Text = "Freeze Gun", OnUpdate = () => GorillaMods.FreezeGun(), Extensions = "MASTER", OnDisable = () => GorillaExtensions.GunTemplate(true), Description = "Gives players tag freeze." },
                            new Menu.ButtonTemplate { Text = "Vibrate Gun", OnUpdate = () => GorillaMods.VibrateGun(), Extensions = "MASTER", OnDisable = () => GorillaExtensions.GunTemplate(true), Description = "Vibrates players controllers." },
                            new Menu.ButtonTemplate { Text = "Material Gun", OnUpdate = () => GorillaMods.MaterialGun(), Extensions = "MASTER", OnDisable = () => GorillaExtensions.GunTemplate(true), Description = "Switches material rapidly on the player the gun is pointed at." },
                            new Menu.ButtonTemplate { Text = "Material All", OnUpdate = () => GorillaMods.MaterialAll(), Extensions = "MASTER", Description = "Switches material rapidly on all players in the lobby." },
                            new Menu.ButtonTemplate { Text = "Material Self", OnUpdate = () => GorillaMods.MaterialSelf(), Extensions = "MASTER", Description = "Switches material rapidly when in a lobby." }
                        }
                    };
                    menu.Categorys.Add(Master);
                    menu.Buttons = menu.Buttons.Concat(Master.ButtonList).ToList();

                    // Setup custom board
                    Board.SetBoardText($"ASPECT CHEAT PANEL {Plugin.Plugin.modVersion}", GetBoardText(menu));
                    Board.SetBoardColor(new Color32(85, 15, 150, 1), new Color32(125, 15, 200, 1));

                    // add extra value to 
                    foreach (ButtonTemplate btn in menu.Buttons.Concat(menu.SettingButtons).ToArray())
                    {
                        if (!menu.ExtraNameValues.ContainsKey(btn))
                        {
                            menu.ExtraNameValues.Add(btn, btn.ExtraValueText);
                        }
                    }

                    // Close menu setup
                    isSetup = true;
                }

                // Update
                Menu.CallUpdate(Input.instance.CheckButton(Input.ButtonType.secondary, menu.LeftHand), menu);
                Menu.UpdateToggledMods(menu);

                // Event stuff - CAUSES SMALL MEMORY LEAK
                //PhotonNetwork.NetworkingClient.EventReceived += GorillaExtensions.ReportNetwork;
            }
        }
    }

    /// <summary>
    /// This is the Base Menu Library, it's in this class where the menu gets drawn, and buttons get updated.
    /// You should not need to modify this class.
    /// </summary>
    public class Menu
    {
        public static void CallUpdate(bool StateDepender, MenuTemplate Menu)
        {
            if (!StateDepender && Menu.MenuRoot != null)
            {
                // Destroy menu reference
                Menu.Reference.transform.parent = null;
                GameObject.Destroy(Menu.Reference);
                Menu.Reference = null;

                // Create menuroot rigidbody
                bool loadOnce = false;
                try
                {
                    if (!loadOnce)
                    {
                        Rigidbody menuRB = Menu.MenuRoot.AddComponent<Rigidbody>();
                        foreach (Collider collider in Menu.MenuRoot.GetComponentsInChildren<Collider>())
                        {
                            GameObject.Destroy(collider);
                        }
                        if (Menu.LeftHand)
                        {
                            menuRB.velocity = Player.Instance.leftHandCenterVelocityTracker.GetAverageVelocity(true, 0);
                            menuRB.angularVelocity = GameObject.Find("TurnParent/LeftHand Controller").GetComponent<GorillaVelocityEstimator>().angularVelocity;
                        }
                        else
                        {
                            menuRB.velocity = Player.Instance.rightHandCenterVelocityTracker.GetAverageVelocity(true, 0);
                            menuRB.angularVelocity = GameObject.Find("TurnParent/RightHand Controller").GetComponent<GorillaVelocityEstimator>().angularVelocity;
                        }
                        loadOnce = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }

                // Destroy Menu
                GameObject.Destroy(Menu.MenuRoot, 1);
                Menu.MenuRoot = null;

                return;
            }

            if (Menu.MenuRoot == null && StateDepender)
            {
                Draw(Menu);

                Menu.Reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GorillaLocomotion.Player instance = GorillaLocomotion.Player.Instance;
                VRRig rig = GorillaTagger.Instance.offlineVRRig;
                if (instance.leftHandOffset == Update.leftPosOffset && instance.rightHandOffset == Update.rightPosOffset && rig.enabled && rig.rightHand.trackingRotationOffset == Update.rightHandOffset && rig.leftHand.trackingRotationOffset == Update.leftHandOffset)
                {
                    Menu.Reference.transform.parent = Menu.ReferenceParent;
                    GameObject.Destroy(Menu.Reference.GetComponent<Renderer>());
                    Menu.Reference.transform.localPosition = Vector3.zero;
                    Menu.Reference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                }
                else
                {
                    Transform parent = Menu.LeftHand ? GorillaLocomotion.Player.Instance.rightControllerTransform : GorillaLocomotion.Player.Instance.leftControllerTransform;
                    Menu.Reference.transform.parent = parent;
                    Menu.Reference.AddComponent<ColorChanger>();
                    Menu.Reference.transform.localPosition = new Vector3(0, -0.1f, 0f);
                    Menu.Reference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f) * instance.scale;
                }

                Menu.ReferenceName = Util.GenRandomString(100);
                Menu.Reference.GetComponent<SphereCollider>().gameObject.name = Menu.ReferenceName;
            }

            else if (Menu.MenuRoot != null)
            {
                Menu.MenuRoot.transform.position = Menu.Pivot.transform.position;
                Menu.MenuRoot.transform.rotation = Menu.Pivot.transform.rotation;

                if (!Menu.LeftHand) Menu.MenuRoot.transform.RotateAround(Menu.MenuRoot.transform.position, Menu.MenuRoot.transform.forward, 180f);
            }
        }

        // This needs to be only updating selected mods
        public static void UpdateToggledMods(MenuTemplate Menu)
        {
            foreach (ButtonTemplate btn in Menu.EnabledButtons)
            {
                //if (!Menu.ExtraNameValues.ContainsKey(btn))
                //{
                //    Menu.ExtraNameValues.Add(btn, btn.ExtraValueText);
                //}
                // make sure extra name values is in sync
                //else if (btn.ExtraValueText != Menu.ExtraNameValues[btn].ToString())
                //{
                //    btn.ExtraValueText = Menu.ExtraNameValues[btn].ToString();
                //}
                if (btn.ButtonState && btn.OnUpdate != null)
                {
                    try
                    {
                        btn.OnUpdate.Invoke();
                    }
                    catch (Exception e)
                    {
                        SendError(e);
                    }
                }
            }
        }

        private static void Draw(MenuTemplate Menu)
        {
            // menu root
            Menu.MenuRoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(Menu.MenuRoot.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(Menu.MenuRoot.GetComponent<BoxCollider>());
            UnityEngine.Object.Destroy(Menu.MenuRoot.GetComponent<Renderer>());
            Menu.MenuRoot.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f) * GorillaLocomotion.Player.Instance.scale;

            // menu background
            GameObject bgObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(bgObject.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(bgObject.GetComponent<BoxCollider>());
            bgObject.transform.SetParent(Menu.MenuRoot.transform, false);
            bgObject.transform.localScale = Menu.Scale;
            bgObject.transform.position = new Vector3(0.05f, 0f, 0f) * GorillaLocomotion.Player.Instance.scale;
            ColorChanger colorChanger = bgObject.AddComponent<ColorChanger>();

            // canvas
            Menu.Canvas = new GameObject();
            Menu.Canvas.transform.parent = Menu.MenuRoot.transform;
            Canvas canvas = Menu.Canvas.AddComponent<Canvas>();
            CanvasScaler canvasScaler = Menu.Canvas.AddComponent<CanvasScaler>();
            Menu.Canvas.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScaler.dynamicPixelsPerUnit = 3000f / GorillaLocomotion.Player.Instance.scale;

            // text
            GameObject textObj = new GameObject();
            textObj.transform.parent = Menu.Canvas.transform;
            Text text = textObj.AddComponent<Text>();
            text.font = Update.menuTitleFont;
            text.text = Menu.Title + " [" + Menu.currentPage.ToString() + "]";
            text.color = Menu.TitleColor;
            text.fontSize = 1;
            text.fontStyle = FontStyle.BoldAndItalic;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;

            // text rect transform
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(0.28f, 0.05f) * GorillaLocomotion.Player.Instance.scale;
            text.GetComponent<RectTransform>().position = new Vector3(0.06f, 0f, 0.175f) * GorillaLocomotion.Player.Instance.scale;
            text.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            AddPageButtons(Menu); // Add the change page buttons
            ButtonTemplate[] DisconnectButton = { new ButtonTemplate { Text = "Disconnect", OnUpdate = () => PhotonNetwork.Disconnect(), Toggle = false }, new ButtonTemplate { Empty = true }, new ButtonTemplate { Empty = true } };
            
            // This code sorts out categorys ... don't even begin to ask me how it works, because i already forgot
            if (Menu.InCategory && Menu.CurrentCategory.ID != "SETTING_1") // im a category
            {
                List<ButtonTemplate> returnButton = new List<ButtonTemplate>() // Create "Return To Main-Menu" button
                {
                    new ButtonTemplate { Text = "Return To Main-Menu", Toggle = false, OnUpdate = () => { Menu.InCategory = false; }, CategoryButton = true }
                };

                // Add all buttons from the current category
                ButtonTemplate[] array = DisconnectButton.ToList().Concat(returnButton.Concat(Menu.CurrentCategory.ButtonList).ToArray().Skip(Menu.currentPage * Menu.ButtonsPerPage).Take(Menu.ButtonsPerPage).ToList()).ToArray();
                for (int i = 0; i < array.Length; i++)
                {
                    AddButton(Menu.ButtonSpace * i, array[i], Menu);
                }
            }
            else if (Menu.InCategory && Menu.CurrentCategory.ID == "SETTING_1") // in settings category
            {
                if (!Menu.InSettings)
                {
                    List<ButtonTemplate> buttons = new List<ButtonTemplate>() // Create category buttons list with all the buttons from the current category + the return button in one list
                    {
                        new ButtonTemplate { Text = "Return To Main-Menu", Toggle = false, OnUpdate = () => { Menu.InCategory = false; }, CategoryButton = true }
                    };
                    foreach (CategoryTemplate category in Menu.SettingCategorys)
                    {
                        buttons.Add(new Menu.ButtonTemplate { Text = category.Name, Toggle = false, OnUpdate = () => { Menu.CurrentSettingsPage = category; Menu.InSettings = true; Menu.currentPage = 0; }, CategoryButton = true });
                    }

                    // Add categoryButtons
                    ButtonTemplate[] array = DisconnectButton.ToList().Concat(buttons.ToArray().Skip(Menu.currentPage * Menu.ButtonsPerPage).Take(Menu.ButtonsPerPage).ToList()).ToArray();
                    for (int i = 0; i < array.Length; i++)
                    {
                        AddButton(Menu.ButtonSpace * i, array[i], Menu);
                    }
                }
                else // in an under category inside of settings
                {
                    List<ButtonTemplate> returnButton = new List<ButtonTemplate>() // Create "Return To Main-Menu" button
                    {
                        new ButtonTemplate { Text = "Return To Setting Categories", Toggle = false, OnUpdate = () => { Menu.InSettings = false; }, CategoryButton = true }
                    };

                    // Load all setting buttons from the current setting category
                    ButtonTemplate[] array = DisconnectButton.ToList().Concat(returnButton.Concat(Menu.CurrentSettingsPage.ButtonList).ToArray().Skip(Menu.currentPage * Menu.ButtonsPerPage).Take(Menu.ButtonsPerPage).ToList()).ToArray();
                    for (int i = 0; i < array.Length; i++)
                    {
                        AddButton(Menu.ButtonSpace * i, array[i], Menu);
                    }
                }
            }
            else // outside a category
            {
                // Add categorys as buttons to a list and add them to the current page
                List<ButtonTemplate> categoryButtons = new List<ButtonTemplate>();
                foreach (CategoryTemplate category in Menu.Categorys)
                {
                    categoryButtons.Add(new Menu.ButtonTemplate { Text = category.Name, Toggle = false, OnUpdate = () => { Menu.CurrentCategory = category; Menu.InCategory = true; Menu.currentPage = 0; }, CategoryButton = true });
                }
                if (Menu.ButtonsOnMainMenu.Count > 0) categoryButtons = categoryButtons.Concat(Menu.ButtonsOnMainMenu).ToList();
                ButtonTemplate[] array = DisconnectButton.ToList().Concat(categoryButtons.ToArray().Skip(Menu.currentPage * Menu.ButtonsPerPage).Take(Menu.ButtonsPerPage).ToList()).ToArray();
                for (int i = 0; i < array.Length; i++)
                {
                    AddButton(Menu.ButtonSpace * i, array[i], Menu);
                }
            }
        }

        private static void AddButton(float Offset, ButtonTemplate Button, MenuTemplate Menu)
        {
            if (Button.Empty == true) return;

            // creates the button object
            GameObject buttonGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(buttonGO.GetComponent<Rigidbody>());
            buttonGO.GetComponent<BoxCollider>().isTrigger = true;
            buttonGO.transform.SetParent(Menu.MenuRoot.transform, false);
            buttonGO.transform.localScale = new Vector3(0.09f, Menu.Scale.y - 0.1f, 0.08f);
            buttonGO.transform.localPosition = new Vector3(0.56f, 0f, 0.67f - Offset);
            buttonGO.AddComponent<ButtonCollider>().button = Button;
            buttonGO.GetComponent<ButtonCollider>().menu = Menu;

            // Manages the button extensions - made this a switch statement instead of a bunch of if's
            string[] ButtonExtensions = Button.Working ? Button.Extensions.Split('.') : new string[] { "NOTWORKING" }.Concat(Button.Extensions.Split('.')).ToArray();
            string Extentions = "";
            for (int i = 0; i < ButtonExtensions.Length; i++)
            {
                switch (ButtonExtensions[i])
                {
                    case "DETECTED":
                        Button.IsLabel = ConfigManager.SAFEMODE.Value;
                        Extentions += $" [<color=red>DETECTED</color>]";
                        break;

                    case "MASTER":
                        string m_color = PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient ? "green" : "red";
                        Extentions += $" [<color={m_color}>MASTER</color>]";
                        break;

                    case "STUMP":
                        string s_color = PhotonNetwork.InRoom && GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(RigManager.VRRigToPhotonView(GorillaTagger.Instance.offlineVRRig).Owner.UserId) ? "green" : "red";
                        Extentions += $" [<color={s_color}>STUMP</color>]";
                        break;

                    case "MODDED":
                        string m_color2 = Plugin.Plugin.inAllowedRoom ? "green" : "red";
                        Extentions += $" [<color={m_color2}>MODDED</color>]";
                        break;

                    case "NOTWORKING":
                        Extentions += $" [<color=red>NOT WORKING</color>]";
                        break;
                }
            }
            string ExtraValue = Menu.ExtraNameValues.ContainsKey(Button) ? GetExtraValueText(Menu.ExtraNameValues[Button]) : "";

            // Manages the button colors
            if (ConfigManager.SAFEMODE.Value && ButtonExtensions.Contains("DETECTED")) Button.IsLabel = true;
            Color targetColor;
            if (!Button.IsFavorited)
            {
                targetColor = Button.IsLabel ? Menu.LabelColor : Button.ButtonState ? Menu.OnColor : Menu.OffColor;
            }
            else
            {
                targetColor = Button.ButtonState ? Menu.FavOnColor : Menu.FavOffColor;
            } 
            buttonGO.GetComponent<Renderer>().material.SetColor("_Color", targetColor);

            // creates the text objects
            GameObject textObj = new GameObject();
            textObj.transform.parent = Menu.Canvas.transform;
            Text text = textObj.AddComponent<Text>();
            text.font = Update.menuButtonFont;
            text.text = Button.Text + ExtraValue + Extentions;
            text.fontSize = 1;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Italic;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;

            // initialize the text rect transform
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(0.2f, 0.03f) * GorillaLocomotion.Player.Instance.scale;
            text.GetComponent<RectTransform>().localPosition = new Vector3(0.064f, 0f, 0.269f - Offset / 2.522522522522523f) * GorillaLocomotion.Player.Instance.scale; // 2.55f is wrong - changed to 2.522522522522523f
            text.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        private static void AddPageButtons(MenuTemplate Menu)
        {
            // button variables
            float space = -Menu.ButtonSpace;
            float calculatedSpace = Menu.ButtonSpace * Menu.ButtonsPerPage;
            string ButtonText = "<<<";

            for (int i = 0; i < 2; i++)
            {
                space += Menu.ButtonSpace;

                // creates the button object
                GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GameObject.Destroy(button.GetComponent<Rigidbody>());
                button.GetComponent<BoxCollider>().isTrigger = true;
                button.transform.SetParent(Menu.MenuRoot.transform, false);
                button.transform.localScale = new Vector3(0.09f, Menu.Scale.y - 0.1f, 0.08f);
                button.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - calculatedSpace);
                button.GetComponent<Renderer>().material.SetColor("_Color", Menu.PagebuttonColor);
                button.AddComponent<ButtonCollider>().button = new ButtonTemplate { Text = ButtonText, Toggle = false };
                button.GetComponent<ButtonCollider>().menu = Menu;

                // creates the text objects
                GameObject textObj = new GameObject();
                textObj.transform.parent = Menu.Canvas.transform;
                Text text = textObj.AddComponent<Text>();
                text.font = Update.menuButtonFont;
                text.text = ButtonText;
                text.fontSize = 1;
                text.alignment = TextAnchor.MiddleCenter;
                text.fontStyle = FontStyle.Italic;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 0;

                // initialize the text rect transform
                text.GetComponent<RectTransform>().sizeDelta = new Vector2(0.2f, 0.03f) * GorillaLocomotion.Player.Instance.scale;
                text.GetComponent<RectTransform>().localPosition = new Vector3(0.064f, 0f, 0.111f - calculatedSpace / 2.522522522522523f) * GorillaLocomotion.Player.Instance.scale;
                text.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

                ButtonText = ">>>";
                calculatedSpace = Menu.ButtonSpace * (Menu.ButtonsPerPage + 1);
            }
        }

        public static void RefreshMenu(MenuTemplate Menu)
        {
            if (Menu.MenuRoot && Menu.Reference)
            {
                UnityEngine.Object.Destroy(Menu.MenuRoot);
                Menu.MenuRoot = null;
                Menu.Reference.transform.parent = null;
                UnityEngine.Object.Destroy(Menu.Reference);
                Menu.Reference = null;
            }
        }

        public static void SendError(Exception error)
        {
            NotifiLib.SendNotification($"<color=red>ERROR</color>: {error.Message}");
            Debug.LogError(error.Message);
        }

        public static ButtonTemplate GetButtonFromName(string ButtonName, MenuTemplate menu)
        {
            foreach (ButtonTemplate btn in menu.Buttons.Concat(menu.SettingButtons))
            {
                if (btn.Text == ButtonName)
                {
                    return btn;
                }
            }
            return new ButtonTemplate();
        }

        public static CategoryTemplate GetCategoryFromID(string ID, MenuTemplate menu)
        {
            CategoryTemplate category = new CategoryTemplate();
            foreach (CategoryTemplate cat in menu.Categorys)
            {
                if (cat.ID == ID)
                {
                    category = cat;
                    break;
                }
            }
            return category;
        }

        // this is brokent rn, it needs to be adding these to a "favorited mods" category
        public static int FavoriteButton(ButtonTemplate button, MenuTemplate menu, bool favorite)
        {
            if (button.IsFavorited == favorite && false == true) return Time.frameCount;
            
            button.IsFavorited = favorite;
            switch (button.IsFavorited)
            {
                // Remove Favorited Button
                case false:
                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(menu.TapFavButtonSound, menu.LeftHand, menu.TapSoundStrength);
                    menu.FavoritedMods.Remove(button);
                    List<ButtonTemplate> fixedButtons1 = new List<ButtonTemplate>();
                    foreach (ButtonTemplate btn in menu.Buttons)
                    {
                        if (!menu.FavoritedMods.Contains(btn))
                        {
                            fixedButtons1.Add(button);
                        }
                    }
                    menu.Buttons = menu.FavoritedMods.Concat(fixedButtons1).ToList();
                    Menu.RefreshMenu(menu);
                    return Time.frameCount;

                // Add Favorited Button
                case true:
                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(menu.TapFavButtonSound, menu.LeftHand, menu.TapSoundStrength);
                    menu.FavoritedMods.Add(button);
                    List<ButtonTemplate> fixedButtons2 = new List<ButtonTemplate>();
                    foreach (ButtonTemplate btn in menu.Buttons)
                    {
                        if (!menu.FavoritedMods.Contains(btn))
                        {
                            fixedButtons2.Add(btn);
                        }
                    }
                    menu.Buttons = menu.FavoritedMods.Concat(fixedButtons2).ToList();
                    Menu.RefreshMenu(menu);
                    return Time.frameCount;

                default:
                    return Time.frameCount;
            }
        }

        public static string GetExtraValueText(object value)
        {
            if (value.ToString() == "") return "";
            string color = value.ToString() == "False" ? "red" : value.ToString() == "True" ? "green" : "white";
            return $" [<color={color}>{value}</color>]";
        }

        public class CategoryTemplate
        {
            // Choose a unique category ID for each category   
            public string ID { get; set; } = "DEFAULT_ID";

            // Choose a unique name for this category
            public string Name { get; set; } = "CATEGORY1";

            // Add ButtonList
            public List<ButtonTemplate> ButtonList { get; set; }
        }

        public class ButtonTemplate
        {
            // Text that shows on the button
            public string Text { get; set; } = "";

            // Text that shows after the button text, mainly for setting buttons, but can be used in other cases too
            public string ExtraValueText { get; set; } = "";

            // Put mod Description here
            public string Description { get; set; } = "";

            // If this is turned on the menu skips the button so it becomes an empty slot
            public bool Empty { get; set; } = false;

            /*
                OnEnable runs one time when the button gets enabled.
                OnUpdate runs each frame when the button is enabled.
                OnDisable runs one time when you turn off the button.
            */
            public Action OnEnable { get; set; } = null;
            public Action OnUpdate { get; set; } = null;
            public Action OnDisable { get; set; } = null;


            // Turn this off if you want your button to run once and not toggle.
            public bool Toggle { get; set; } = true;

            // Make the button into a label
            public bool IsLabel { get; set; } = false;

            // If this is on, the button is favorited and pinned to the start
            public bool IsFavorited { get; set; } = false;

            // Handles if the buttonArray is enabled or not, may not be customized.
            internal bool ButtonState { get; set; } = false;

            // Current extentions { MASTER, MODDED, STUMP, DETECTED }
            public string Extensions { get; set; } = "";

            // If this is turned on, The Button is disabled
            public bool Working { get; set; } = true;

            // If this is on, the button is for going inside a category
            public bool CategoryButton { get; set; } = false;
        }

        public class MenuTemplate
        {
            // Button variables
            internal int ButtonsPerPage = 4;
            public List<ButtonTemplate> Buttons = new List<ButtonTemplate>();
            public List<ButtonTemplate> FavoritedMods = new List<ButtonTemplate>();
            public List<ButtonTemplate> EnabledButtons = new List<ButtonTemplate>();

            // Public variables
            public string Title;
            public Color TitleColor;
            public Vector3 Scale;
            public GameObject Pivot;
            public bool LeftHand;
            public int TapFavButtonSound = 84;
            public float TapSoundStrength = 0.25f;

            // Menu core variables
            internal int currentPage = 0;
            internal GameObject MenuRoot = null;
            internal GameObject Canvas = null;
            internal GameObject Reference = null;
            internal string ReferenceName;
            internal Transform ReferenceParent;
            internal float ButtonSpace = 0.13f;

            // Colors
            public Color ThemeColor1 = new Color32(85, 15, 150, 1);
            public Color ThemeColor2 = new Color32(125, 15, 200, 1);
            public Color OffColor = new Color32(115, 0, 160, 1);
            public Color OnColor = new Color32(70, 0, 100, 1);
            public Color FavOffColor = new Color32(240, 170, 50, 1);
            public Color FavOnColor = new Color32(175, 130, 30, 1);
            public Color LabelColor = Color.grey;
            public Color PagebuttonColor = new Color32(115, 0, 160, 1);

            // Category Variables
            public List<CategoryTemplate> SettingCategorys = new List<CategoryTemplate>();
            public List<ButtonTemplate> ButtonsOnSettingsPage = new List<ButtonTemplate>();
            public List<ButtonTemplate> SettingButtons = new List<ButtonTemplate>();
            public bool InSettings = false;
            public int settingButtonsCount = 0;
            public CategoryTemplate CurrentSettingsPage;
            public List<CategoryTemplate> Categorys = new List<CategoryTemplate>();
            public List<ButtonTemplate> ButtonsOnMainMenu = new List<ButtonTemplate>();
            public bool InCategory = false;
            public CategoryTemplate CurrentCategory;
            public Dictionary<ButtonTemplate, object> ExtraNameValues = new Dictionary<ButtonTemplate, object>();

            // Returns a new menu instance
            public static MenuTemplate CreateMenu(string title, Color titleColor, Vector3 scale, GameObject pivot, bool leftHand)
            {
                MenuTemplate template = new MenuTemplate();

                template.Title = title;
                template.TitleColor = titleColor;
                template.Scale = scale;
                template.Pivot = pivot;
                template.LeftHand = leftHand;

                if (leftHand) template.ReferenceParent = GorillaTagger.Instance.rightHandTriggerCollider.transform;

                else template.ReferenceParent = GorillaTagger.Instance.leftHandTriggerCollider.transform;

                return template;
            }

            // Switch hands
            public void SwitchHands()
            {
                if (LeftHand)
                {
                    Pivot = Player.Instance.rightControllerTransform.gameObject;
                    this.ReferenceParent = GorillaTagger.Instance.leftHandTriggerCollider.transform;
                    LeftHand = false;
                    RefreshMenu(this);
                }
                else
                {
                    Pivot = Player.Instance.leftControllerTransform.gameObject;
                    this.ReferenceParent = GorillaTagger.Instance.rightHandTriggerCollider.transform;
                    LeftHand = true;
                    RefreshMenu(this);
                }
            }

            public int menuTheme = 1;
            public Dictionary<int, object[]> menuThemes = new Dictionary<int, object[]>();
            public void SetupMenuThemes()
            {
                menuThemes.Add(1, new object[] { new Color[] { new Color32(85, 15, 150, 1), new Color32(125, 15, 200, 1), new Color32(115, 0, 160, 1), new Color32(70, 0, 100, 1) }, "Original" });
                menuThemes.Add(2, new object[] { new Color[] { new Color32(36, 120, 220, 1), new Color32(45, 135, 250, 1), new Color32(0, 110, 255, 1), new Color32(0, 95, 215, 1) }, "Blue" });
                menuThemes.Add(3, new object[] { new Color[] { new Color32(210, 55, 35, 1), new Color32(235, 65, 45, 1), new Color32(255, 25, 0, 1), new Color32(190, 20, 0, 1) }, "Red" });
            }

            public string ChangeMenuTheme()
            {
                // Change theme
                if (menuTheme + 1 > menuThemes.Count)
                {
                    menuTheme = 1;
                }
                else
                {
                    menuTheme++;
                }

                // Change colors
                Color[] colors = (Color[])menuThemes[menuTheme][0];
                ThemeColor1 = colors[0];
                ThemeColor2 = colors[1];
                OffColor = colors[2];
                OnColor = colors[3];
                PagebuttonColor = colors[2];

                // Update board color
                Board.SetBoardColor(ThemeColor1, ThemeColor2);

                // Return theme name
                return (string)menuThemes[menuTheme][1];
            }
        }

        public class ColorChanger : MonoBehaviour
        {
            public Color Color1 = Aspect.MenuLib.Update.menu.ThemeColor1;
            public Color Color2 = Aspect.MenuLib.Update.menu.ThemeColor2;
            public Shader shader = null;

            Gradient gradient = new Gradient();
            Color32 color;

            public void Start()
            {
                // set color and alphakeys 
                var colors = new GradientColorKey[3];
                colors[0] = new GradientColorKey(Color1, 0.0f);
                colors[1] = new GradientColorKey(Color2, 0.5f);
                colors[2] = new GradientColorKey(Color1, 1.0f);

                var alphas = new GradientAlphaKey[3];
                alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
                alphas[1] = new GradientAlphaKey(0.5f, 0.5f);
                alphas[2] = new GradientAlphaKey(0.0f, 1.0f);

                gradient.SetKeys(colors, alphas);
            }

            public void Update()
            {
                color = gradient.Evaluate((Time.time / 4) % 1);
                if (base.GetComponent<Renderer>().material.color != color)
                {
                    Material material = new Material(RigManager.uberShader);
                    base.GetComponent<Renderer>().material = material;
                    base.GetComponent<Renderer>().material.SetColor("_Color", color);
                    if (shader != null) base.GetComponent<Renderer>().material.shader = shader;
                }
                else
                {
                    base.GetComponent<Renderer>().material.SetColor("_Color", color);
                    if (shader != null) base.GetComponent<Renderer>().material.shader = shader;
                }
            }
        }

        internal class ButtonCollider : MonoBehaviour
        {
            public ButtonTemplate button;
            public MenuTemplate menu;
            static float PressCooldown = 0;

            public void OnTriggerEnter(Collider collider)
            {
                if (collider.gameObject.name == menu.ReferenceName && !button.IsLabel && Time.frameCount >= PressCooldown + 30f)
                {
                    MenuLib.Update.AllGunsOnPC = false;
                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, false, 0.5f);
                    if (!button.Working)
                    {
                        NotifiLib.SendNotification("[<color=magenta>BUTTON</color> This button is not working, it'll be fixed soon");
                        RefreshMenu(menu);
                        PressCooldown = Time.frameCount;
                        return;
                    }
                    if (button.Text.Contains(">"))
                    {
                        if (menu.InCategory)
                        {
                            if (menu.currentPage < (menu.CurrentCategory.ButtonList.ToArray().Length + menu.ButtonsPerPage) / menu.ButtonsPerPage - 1)
                            {
                                menu.currentPage++;
                            }
                            else
                            {
                                menu.currentPage = 0;
                            }
                        }
                        else
                        {
                            if (menu.currentPage < (menu.Categorys.ToArray().Length + menu.ButtonsPerPage) / menu.ButtonsPerPage - 1)
                            {
                                menu.currentPage++;
                            }
                            else
                            {
                                menu.currentPage = 0;
                            }
                        }
                    }
                    else if (button.Text.Contains("<"))
                    {
                        if (menu.InCategory)
                        {
                            if (menu.currentPage > 0)
                            {
                                menu.currentPage--;
                            }
                            else
                            {
                                menu.currentPage = (menu.CurrentCategory.ButtonList.ToArray().Length + menu.ButtonsPerPage) / menu.ButtonsPerPage - 1;
                            }
                        }
                        else
                        {
                            if (menu.currentPage > 0)
                            {
                                menu.currentPage--;
                            }
                            else
                            {
                                menu.currentPage = (menu.Categorys.ToArray().Length + menu.ButtonsPerPage) / menu.ButtonsPerPage - 1;
                            }
                        }
                    }

                    // something is wrong pls fix - fixed - it needs new fix for the category update
                    /*if (Input.instance.CheckButton(Input.ButtonType.grip, true) && !button.Text.Contains(">") && !button.Text.Contains("<") && button.Text != "Disconnect")
                    {
                        FavoriteButton(button, menu, !button.IsFavorited);
                    }*/

                    if (button.ButtonState && button.OnDisable != null && button.Toggle)
                    {
                        button.ButtonState = !button.ButtonState;

                        try
                        {
                            if (button.OnDisable != null) button.OnDisable.Invoke();
                            if (button.ExtraValueText != menu.ExtraNameValues[button].ToString())
                            {
                                button.ExtraValueText = menu.ExtraNameValues[button].ToString();
                            }
                            menu.EnabledButtons.Remove(button);
                        }
                        catch (Exception ex)
                        {
                            SendError(ex);
                        }
                    }

                    else if (button.Toggle)
                    {
                        button.ButtonState = !button.ButtonState;

                        if (button.ButtonState)
                        {
                            try
                            {
                                if (button.OnEnable != null) button.OnEnable.Invoke();
                                if (button.OnUpdate != null) button.OnUpdate.Invoke();
                                if (button.ExtraValueText != menu.ExtraNameValues[button].ToString())
                                {
                                    button.ExtraValueText = menu.ExtraNameValues[button].ToString();
                                }
                                menu.EnabledButtons.Add(button);
                            }
                            catch (Exception ex)
                            {
                                button.ButtonState = !button.ButtonState;
                                SendError(ex);
                            }
                        }
                    }

                    else if (!button.Toggle)
                    {
                        try
                        {
                            if (button.OnUpdate != null) button.OnUpdate.Invoke();
                            if (!button.CategoryButton && button.Text != "Disconnect" && !button.Text.Contains("<") && !button.Text.Contains(">"))
                            {
                                if (button.ExtraValueText != menu.ExtraNameValues[button].ToString())
                                {
                                    button.ExtraValueText = menu.ExtraNameValues[button].ToString();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SendError(ex);
                        }
                    }

                    // Put mod description on board and send notification
                    if (!button.Text.Contains(">") && !button.Text.Contains("<") && button.Text != "Disconnect" && button.Description != "")
                    {
                        string color = button.ButtonState ? "green" : "red";
                        if (!button.Toggle) color = "green";
                        string text = $"[<color={color}>{button.Text}</color>] {button.Description}";
                        Board.SetBoardText($"Aspect Cheat Panel {Plugin.Plugin.modVersion}", Update.GetBoardText(Update.menu, $"\n\nCurrent Mod:\n{text}", true));
                        if (Update.tooltipNotification) NotifiLib.SendNotification(text);
                    }

                    Menu.RefreshMenu(menu);
                    PressCooldown = Time.frameCount;
                }
            }
        }
    }
}
