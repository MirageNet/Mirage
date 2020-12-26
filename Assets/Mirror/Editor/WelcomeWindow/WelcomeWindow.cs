using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

/**
 * Docs used:
 * UXML: https://docs.unity3d.com/Manual/UIE-UXML.html
 * USS: https://docs.unity3d.com/Manual/UIE-USS-SupportedProperties.html
 * Unity Guide: https://learn.unity.com/tutorial/uielements-first-steps/?tab=overview#5cd19ef9edbc2a1156524842
 * External Guide: https://www.raywenderlich.com/6452218-uielements-tutorial-for-unity-getting-started#toc-anchor-010
 */

namespace Mirror
{

    //this script handles the functionality of the UI

    [InitializeOnLoad]
    public class WelcomeWindow : EditorWindow
    {
        #region Setup

        #region Urls

        private const string welcomePageUrl = "https://mirrorng.github.io/MirrorNG/";
        private const string quickStartUrl = "https://mirrorng.github.io/MirrorNG/Articles/Guides/CommunityGuides/MirrorQuickStartGuide/index.html";
        private const string changelogUrl = "https://github.com/MirrorNG/MirrorNG/commits/master";
        private const string bestPracticesUrl = "https://mirrorng.github.io/MirrorNG/Articles/Guides/BestPractices.html";
        private const string faqUrl = "https://mirrorng.github.io/MirrorNG/Articles/Guides/FAQ.html";
        private const string sponsorUrl = "";
        private const string discordInviteUrl = "https://discord.gg/N9QVxbM";

        #endregion

        //window size of the welcome screen
        private static Vector2 windowSize = new Vector2(500, 600);

        //returns the path to the Mirror folder (ex. Assets/Mirror)
        private static string MirrorPath
        {
            get
            {
                //get an array of results based on the search
                string[] results = AssetDatabase.FindAssets("", new[] { "Assets" });

                //loop through every result
                foreach (string guid in results)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    //if the path contains Mirror/Version.txt, then we have found the Mirror folder
                    if (path.Contains("Mirror/package.json"))
                    {
                        return path.Remove(path.IndexOf("/package.json"));
                    }
                }
                //return nothing if path wasn't found
                return "";
            }
        }

        //get the start up key
        private static string firstStartUpKey = string.Empty;

        //get the icon texture
        private static Texture2D MirrorIcon;

        #region Getters for icon and key

        //called only once
        private static string GetVersion()
        {
            //if the file doesnt exist, return unknown mirror version
            if (!File.Exists(MirrorPath + "/package.json"))
            {
                return "MirrorUnknown";
            }

            //read the Version.txt file
            foreach (string line in File.ReadAllLines(MirrorPath + "/package.json"))
            {
                if (line.Contains("version"))
                    return line.Substring(7).Replace("\"", "").Replace(",", "");
            }

            return "MirrorUnknown";
        }

        #endregion

        #region Handle visibility

        //constructor (called by InitializeOnLoad)
        static WelcomeWindow()
        {
            EditorApplication.update += ShowWindowOnFirstStart;
        }

        //decide if we should open the window on recompile
        private static void ShowWindowOnFirstStart()
        {
            //if we haven't seen the welcome page on the current mirror version, show it
            //if there is no version, skip this
            firstStartUpKey = GetVersion();
            if (!EditorPrefs.GetBool(firstStartUpKey, false) && firstStartUpKey != "MirrorUnknown")
            {
                OpenWindow();
                //now that we have seen the welcome window, set this this to true so we don't load the window every time we recompile (for the current version)
                EditorPrefs.SetBool(firstStartUpKey, true);
            }

            EditorApplication.update -= ShowWindowOnFirstStart;
        }

        //open the window (also openable through the path below)
        [MenuItem("Window/MirrorNG/Welcome")]
        public static void OpenWindow()
        {
            //create the window
            WelcomeWindow window = GetWindow<WelcomeWindow>("MirrorNG Welcome Page");
            //set the position and size
            window.position = new Rect(new Vector2(100, 100), windowSize);
            //set min and max sizes so we cant readjust window size
            window.maxSize = windowSize;
            window.minSize = windowSize;
        }

        #endregion

        #endregion

        #region Displaying UI

        //the code to handle display and button clicking
        private void OnEnable()
        {
            //Load the UI
            //Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MirrorPath + "/Editor/WelcomeWindow/WelcomeWindow.uxml");
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(MirrorPath + "/Editor/WelcomeWindow/WelcomeWindow.uss");

            //Load the descriptions

            uxml.CloneTree(root);
            root.styleSheets.Add(uss);

            //set the version text
            var versionText = root.Q<Label>("VersionText");
            versionText.text = "v" + GetVersion().Substring(6);

            #region Page buttons

            Button WelcomeButton = root.Q<Button>("WelcomeButton");
            WelcomeButton.clicked += () => ShowTab("Welcome");

            Button ChangeLogButton = root.Q<Button>("ChangeLogButton");
            ChangeLogButton.clicked += () => ShowTab("ChangeLog");

            Button QuickStartButton = root.Q<Button>("QuickStartButton");
            QuickStartButton.clicked += () => ShowTab("QuickStart");

            Button BestPracticesButton = root.Q<Button>("BestPracticesButton");
            BestPracticesButton.clicked += () => ShowTab("BestPractices");

            Button FaqButton = root.Q<Button>("FaqButton");
            FaqButton.clicked += () => ShowTab("Faq");

            Button SponsorButton = root.Q<Button>("SponsorButton");
            SponsorButton.clicked += () => ShowTab("Sponsor");

            Button DiscordButton = root.Q<Button>("DiscordButton");
            DiscordButton.clicked += () => Application.OpenURL(discordInviteUrl);

            ShowTab("Welcome");
            #endregion
        }

        private void ShowTab(string screen)
        {
            VisualElement rightColumn = rootVisualElement.Q<VisualElement>("RightColumnBox");
            foreach (VisualElement tab in rightColumn.Children())
            {
                tab.style.display = tab.name == screen ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        #endregion
    }

}
