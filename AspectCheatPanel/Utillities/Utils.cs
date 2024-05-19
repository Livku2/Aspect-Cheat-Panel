using Aspect.MenuLib;
using ExitGames.Client.Photon;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace Aspect.Utilities
{
    /// <summary>
    /// Extra tools.
    /// </summary>
    public static class Util
    {
        // Send Projectile - taken straight from RoomSystem
        static int projectileCount = 0;
        public static void SendLaunchProjectile(Vector3 position, Vector3 velocity, bool randomColour, float r, float g, float b, float a)
        {
            projectileCount = (int)AccessTools.Method("ProjectileTracker:IncrementLocalPlayerProjectileCount").Invoke(null, null);
            object[] sendData = new object[]
            {
                position,
                velocity,
                0,
                projectileCount,
                randomColour,
                r, g, b, a
            };
            RaiseEventOptions reoOthers = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others
            };
            SendOptions soUnreliable = SendOptions.SendUnreliable;
            SendEvent(0, sendData, reoOthers, soUnreliable);
        }

        // Send Event - also taken straight from RoomSystem
        internal static void SendEvent(in byte code, in object evData, in RaiseEventOptions reo, in SendOptions so)
        {
            object[] DataToSend = new object[] { PhotonNetwork.ServerTimestamp, code, evData };
            PhotonNetwork.RaiseEvent(3, DataToSend, reo, so);
        }

        // Change color
        public static void ChangeColor(float R, float G, float B, bool local = false)
        {
            if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(RigManager.VRRigToPhotonView(GorillaTagger.Instance.offlineVRRig).Owner.UserId))
            {
                if (!local && PhotonNetwork.InRoom) RigManager.VRRigToPhotonView(GorillaTagger.Instance.offlineVRRig).RPC("InitializeNoobMaterial", RpcTarget.All, new object[] { R, G, B });
                else GorillaTagger.Instance.offlineVRRig.InitializeNoobMaterialLocal(R, G, B);
                PlayerPrefs.SetFloat("redValue", R);
                PlayerPrefs.SetFloat("greenValue", G);
                PlayerPrefs.SetFloat("blueValue", B);
                PlayerPrefs.Save();
            }
        }

        // Change name
        public static void ChangeName(string name)
        {
            GorillaComputer.instance.currentName = name;
            PhotonNetwork.LocalPlayer.NickName = name;
            GorillaComputer.instance.offlineVRRigNametagText.text = name;
            GorillaComputer.instance.savedName = name;
            PlayerPrefs.SetString("playerName", name);
            PlayerPrefs.Save();
        }

        // Function to create a random string
        public static string GenRandomString(int length)
        {
            string finalString = "";
            string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";

            for (int i = 0; i < length; i++)
            {
                char caracterAtRandomIndex = letters[Random.Range(1, letters.Length)];
                finalString += caracterAtRandomIndex;
            }

            return finalString;
        }

        // Function to add players to the script except for specific players
        public static List<Photon.Realtime.Player> AddPlayersExcept(List<Photon.Realtime.Player> excludedPlayers)
        {
            // Create list
            List<Photon.Realtime.Player> correctPlayerlist = new List<Photon.Realtime.Player>
            {
                PhotonNetwork.LocalPlayer
            };

            // Make list
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                if (player != PhotonNetwork.LocalPlayer && !excludedPlayers.Contains(player))
                {
                    // Add player to the script, and remove local player if list contains
                    correctPlayerlist.Add(player);
                    if (correctPlayerlist.Contains(PhotonNetwork.LocalPlayer)) correctPlayerlist.Remove(PhotonNetwork.LocalPlayer);
                }
            }

            // Return correct playerlist
            return correctPlayerlist;
        }
        public static List<VRRig> AddVRRigsExcept(List<Photon.Realtime.Player> excludedPlayers)
        {
            // Create list
            List<VRRig> correctPlayerlist = new List<VRRig>
            {
                GorillaTagger.Instance.offlineVRRig
            };

            // Make list
            foreach (VRRig player in GorillaParent.instance.vrrigs)
            {
                if (player == GorillaTagger.Instance.offlineVRRig) continue;
                if (player != GorillaTagger.Instance.offlineVRRig && !excludedPlayers.Contains(RigManager.VRRigToPhotonView(player).Owner))
                {
                    // Add player to the script, and remove local player if list contains
                    correctPlayerlist.Add(player);
                    if (correctPlayerlist.Contains(GorillaTagger.Instance.offlineVRRig)) correctPlayerlist.Remove(GorillaTagger.Instance.offlineVRRig);
                }
            }

            // Return correct playerlist
            return correctPlayerlist;
        }

        // Get VRRig list from playerlist
        public static List<VRRig> GetVRRigsFromPlayerlist(List<Photon.Realtime.Player> playerList)
        {
            List<VRRig> vrrigs = new List<VRRig>();
            foreach (Photon.Realtime.Player player in playerList)
            {
                if (player == PhotonNetwork.LocalPlayer) continue;
                vrrigs.Add(RigManager.PlayerToVRRig(player));
            }
            return vrrigs;
        }

        // Gets a multiplied deltaTime for velocity mods
        public static float GetFixedDeltaTime()
        {
            return Time.deltaTime * 80f;
        }

        // Get pos between
        public static Vector3 PosBetween(Vector3 pos1, Vector3 pos2)
        {
            return pos1 + (pos1 - pos2);
        }
    }

    /// <summary>
    /// This class is used as a library for all rig/gamemode functions
    /// </summary>
    public static class RigManager
    {
        // shaders
        public static Shader textShader = Shader.Find("GUI/Text Shader");
        public static Shader uberShader = Shader.Find("GorillaTag/UberShader");

        // material names
        public static string it = "It";
        public static string infected = "infected";
        public static string casual = "gorilla_body(Clone)";
        public static string hunted = "ice";
        public static string bluealive = "bluealive";
        public static string bluestunned = "bluestunned";
        public static string bluehit = "bluehit";
        public static string paintsplatterblue = "paintsplattersmallblue";
        public static string orangealive = "orangealive";
        public static string orangestunned = "orangestunned";
        public static string orangehit = "orangehit";
        public static string paintsplatterorange = "paintsplattersmallorange";
        public static string sodainfected = "SodaInfected";

        // material colors
        public static Color taggedColor = new Color(255f / 255f, 0f / 255f, 0f / 255f, 0.3f);
        public static Color casualColor = new Color(0f / 255f, 255f / 255f, 0f / 255f, 0.3f);
        public static Color huntedColor = new Color(0f / 255f, 128f / 255f, 255f / 255f, 0.3f);
        public static Color sodaInfected = new Color(0f / 255f, 255f / 255, 128f / 255, 0.3f);
        public static Color blueAlive = new Color(0f / 255f, 128f / 255f, 255f / 255f, 0.3f);
        public static Color blueHit = new Color(0f / 255f, 64f / 255f, 255f / 255f, 0.3f);
        public static Color bluepaintsplatter = new Color(0f / 255f, 0f / 255f, 255f / 255f, 0.3f);
        public static Color orangeAlive = new Color(255f / 255f, 128f / 255f, 0f / 255f, 0.3f);
        public static Color orangeHit = new Color(255f / 255f, 64f / 255f, 0f / 255f, 0.3f);
        public static Color orangepaintsplatter = new Color(255f / 255f, 0f / 255f, 0f / 255f, 0.3f);

        public static PhotonView VRRigToPhotonView(VRRig rig)
        {
            return (PhotonView)Traverse.Create(rig).Field("photonView").GetValue();
        }

        public static VRRig PlayerToVRRig(Photon.Realtime.Player player)
        {
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (VRRigToPhotonView(rig).Owner.UserId == player.UserId) return rig;
            }
            return null;
        }

        public static bool IsTagged(VRRig rig)
        {
            return rig.mainSkin.material.name.Contains(it) || rig.mainSkin.material.name.Contains(infected);
        }

        public static bool IsHunted(VRRig rig)
        {
            return rig.mainSkin.material.name.Contains(hunted);
        }

        public static string CurrentGameMode()
        {
            if (GorillaGameManager.instance.GetComponent<GorillaTagManager>()) return "INFECTION";
            else if (GorillaGameManager.instance.GetComponent<GorillaHuntManager>()) return "HUNT";
            else if (GorillaGameManager.instance.GetComponent<GorillaBattleManager>()) return "BATTLE";
            else return "CASUAL";
        }

        public static Photon.Realtime.Player GetPlayerFromID(string id)
        {
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                if (player.UserId == id)
                {
                    return player;
                }
            }
            return null;
        }

        public static VRRig GetClosestTagged()
        {
            VRRig myRig = GorillaTagger.Instance.offlineVRRig;
            VRRig closest = myRig;
            float closestYet = float.MaxValue;

            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig == myRig || CurrentGameMode() != "INFECTION" || !IsTagged(rig)) continue;

                if (Vector3.SqrMagnitude(myRig.transform.position - rig.transform.position) < closestYet)
                {
                    closest = rig;
                    closestYet = Vector3.SqrMagnitude(myRig.transform.position - rig.transform.position);
                }
            }

            return closest;
        }

        public static VRRig GetClosest()
        {
            VRRig myRig = GorillaTagger.Instance.offlineVRRig;
            VRRig closest = myRig;
            float closestYet = float.MaxValue;

            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (Vector3.SqrMagnitude(myRig.transform.position - rig.transform.position) < closestYet)
                {
                    closest = rig;
                    closestYet = Vector3.SqrMagnitude(myRig.transform.position - rig.transform.position);
                }
            }

            return closest;
        }
    }
}
