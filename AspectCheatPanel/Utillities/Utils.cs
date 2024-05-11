using GorillaExtensions;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspect.Utilities
{
    /// <summary>
    /// Extra tools.
    /// </summary>
    public static class Util
    {
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
    }

    /// <summary>
    /// This class is used as a library for all rig/gamemode functions
    /// </summary>
    public static class RigManager
    {
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
    }
}
