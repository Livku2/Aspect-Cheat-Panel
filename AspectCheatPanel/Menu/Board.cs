using UnityEngine;
using UnityEngine.UI;

namespace Aspect.MenuLib
{
    /// <summary>
    /// This class is used to change the board colors and text.
    /// </summary>
    public static class Board
    {
        public static void SetBoardText(string title, string content)
        {
            // dont set text if the board doesn't exist
            if (GameObject.Find("COC Text") == null) return;

            //set text
            GameObject boardTitle = GameObject.Find("CodeOfConduct");
            boardTitle.GetComponent<Text>().text = "<<color=yellow>" + title + "</color>>";

            GameObject board = GameObject.Find("COC Text");
            board.GetComponent<Text>().text = content;

            RectTransform component = board.GetComponent<RectTransform>();
            component.sizeDelta = new Vector2(68.8744f, 177.5f);
            component.localPosition = new Vector3(-56.2008f, -63f, 0.0002f);
        }

        public static void SetBoardColor(Color color1, Color color2)
        {
            // include all screen in the game asap - done (mostly)
            string[] screens = {
                "wallmonitorcanyon",
                "wallmonitorcosmetics",
                "wallmonitorcave",
                "wallmonitorforest",
                "wallmonitorskyjungle"
            };
            for (int i = 0; i < screens.Length; i++)
            {
                // skip if screen doesnt exist
                if (!GameObject.Find(screens[i])) continue;
                // set custom material
                Material material = new Material(Shader.Find("Standard"));
                GameObject.Find(screens[i]).GetComponent<Renderer>().material = material;

                //initialize colorchanger
                Menu.ColorChanger colorChanger = GameObject.Find(screens[i]).AddComponent<Menu.ColorChanger>();
            }

            foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                if (obj.name.Contains("forestatlas") && obj.transform.parent.name == "TreeRoom" && obj.GetInstanceID() == 94332)
                {
                    // set custom material
                    Material material = new Material(Shader.Find("Standard"));
                    obj.GetComponent<Renderer>().material = material;

                    //initialize colorchanger
                    Menu.ColorChanger colorChanger = obj.AddComponent<Menu.ColorChanger>();
                }
            }
        }
    }
}