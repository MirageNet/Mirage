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

        #region Page variables

        //type of page
        private enum EScreens { welcome, changelog, quickstart, bestpractices, faq, sponsor, discord }

        //the current page
        private EScreens currentScreen = EScreens.welcome;

        //data type that we want to retrieve when we are using this enum
        private enum EPageDataType { header, description, redirectButtonTitle, redirectButtonUrl }

        //headers of the different pages
        private const string welcomePageHeader = "Welcome";
        private const string changelogHeader = "Change Log";
        private const string quickStartHeader = "Quick Start Guide";
        private const string bestPracticesHeader = "Best Practices";
        private const string faqHeader = "FAQ";
        private const string sponsorHeader = "Sponsor Us";

        //descriptions of the different pages
        private const string welcomePageDescription = "Hello! Thank you for installing MirrorNG. Please visit all the pages on this window. Clicking the button at the bottom of the pages will redirect you to a webpage. Additionally, there are example projects in the Mirror folder that you can look at. \n\nHave fun using Mirror!";
        private const string changelogDescription = "The Change Log is a list of changes made to MirrorNG. Sometimes these changes can cause your game to break.";
        private const string quickStartDescription = "The Quick Start Guide is meant for people who just started using MirrorNG. The Quick Start Guide will help new users learn how to accomplish important tasks. It is highly recommended that you complete the guide.";
        private const string bestPracticesDescription = "This page describes the best practices that you should use during development. Currently a work in progress.";
        private const string faqDescription = "The FAQ page holds commonly asked questions. Currently, the FAQ page contains answers to: \n\n   1. Syncing custom data types \n   2. How to connect \n   3. Host migration \n   4. Server lists and matchmaking";
        private const string sponsorDescription = "Sponsoring will give you access to Mirror PRO which gives you special access to tools and priority support.";

        //titles of the redirect buttons
        private const string welcomePageButtonTitle = "Visit API Reference";
        private const string changelogPageButtonTitle = "Visit Change Log";
        private const string quickStartPageButtonTitle = "Visit Quick Start Guide";
        private const string bestPracticesPageButtonTitle = "Visit Best Practices Page";
        private const string faqPageButtonTitle = "Visit FAQ";
        private const string sponsorPageButtonTitle = "Sponsor Us";

        #endregion

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
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MirrorPath + "/Editor/WelcomeWindow/WelcomeWindow.uxml");
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(MirrorPath + "/Editor/WelcomeWindow/WelcomeWindow.uss");

            //Load the descriptions

            uxml.CloneTree(root);
            root.styleSheets.Add(uss);

            //set the icon image
            var icon = root.Q<Image>("Icon");
            icon.image = MirrorIcon;

            //set the version text
            var versionText = root.Q<Label>("VersionText");
            versionText.text = "v" + GetVersion().Substring(6);

            //get the header
            var header = root.Q<Label>("Header");
            //get the description
            var description = root.Q<Label>("Description");
            //get the redirect button
            var redirectButton = root.Q<Button>("Redirect");

            //init the content in the right column
            header.text = GetPageData(EPageDataType.header).ToString();
            description.text = GetPageData(EPageDataType.description).ToString();
            redirectButton.text = GetPageData(EPageDataType.redirectButtonTitle).ToString();
            redirectButton.clicked += () => { new ButtonClicked(GetPageData(EPageDataType.redirectButtonUrl).ToString()); };

            #region Page buttons

            string[] buttonHeaders = new[] { welcomePageHeader, changelogHeader, quickStartHeader, bestPracticesHeader, faqHeader, sponsorHeader, "Discord" };

            var button0 = root.Q<Button>("WelcomeButton");
            button0.text = buttonHeaders[0];
            button0.clicked += () => PageButtonClicked(header, description, redirectButton, 0);

            var button1 = root.Q<Button>("ChangeLogButton");
            button1.text = buttonHeaders[1];
            button1.clicked += () => PageButtonClicked(header, description, redirectButton, 1);

            var button2 = root.Q<Button>("QuickStartButton");
            button2.text = buttonHeaders[2];
            button2.clicked += () => PageButtonClicked(header, description, redirectButton, 2);

            var button3 = root.Q<Button>("BestPracticesButton");
            button3.text = buttonHeaders[3];
            button3.clicked += () => PageButtonClicked(header, description, redirectButton, 3);

            var button4 = root.Q<Button>("FaqButton");
            button4.text = buttonHeaders[4];
            button4.clicked += () => PageButtonClicked(header, description, redirectButton, 4);

            var button5 = root.Q<Button>("SponsorButton");
            button5.text = buttonHeaders[5];
            button5.clicked += () => PageButtonClicked(header, description, redirectButton, 5);

            var button6 = root.Q<Button>("DiscordButton");
            button6.text = buttonHeaders[6];
            button6.clicked += () => PageButtonClicked(header, description, redirectButton, 6);

            #endregion

        }

        private void PageButtonClicked(Label header, Label description, Button redirectButton, int newScreen)
        {
            EScreens screen = (EScreens)newScreen;

            if (screen != EScreens.discord)
            {
                currentScreen = screen;
                header.text = GetPageData(EPageDataType.header).ToString();
                description.text = GetPageData(EPageDataType.description).ToString();
                redirectButton.text = GetPageData(EPageDataType.redirectButtonTitle).ToString();
                redirectButton.clicked += () => { new ButtonClicked(GetPageData(EPageDataType.redirectButtonUrl).ToString()); };
            }
            else
            {
                Application.OpenURL(discordInviteUrl);
            }
        }

        //get the page data based on the page and the type needed
        private object GetPageData(EPageDataType type)
        {
            string[] returnTypes = new string[8];

            //check the data type, set return types based on data type
            if (type == EPageDataType.header)
            {
                returnTypes = new[] { welcomePageHeader, quickStartHeader, bestPracticesHeader, faqHeader, sponsorHeader, changelogHeader };
            }
            else if (type == EPageDataType.description)
            {
                returnTypes = new[] { welcomePageDescription, quickStartDescription, bestPracticesDescription, faqDescription, sponsorDescription, changelogDescription };
            }
            else if (type == EPageDataType.redirectButtonTitle)
            {
                returnTypes = new[] { welcomePageButtonTitle, quickStartPageButtonTitle, bestPracticesPageButtonTitle, faqPageButtonTitle, sponsorPageButtonTitle, changelogPageButtonTitle };
            }
            else if (type == EPageDataType.redirectButtonUrl)
            {
                returnTypes = new[] { welcomePageUrl, quickStartUrl, bestPracticesUrl, faqUrl, sponsorUrl, changelogUrl };
            }

            //return results based on the current page
            if (currentScreen == EScreens.welcome) { return returnTypes[0]; }
            else if (currentScreen == EScreens.quickstart) { return returnTypes[1]; }
            else if (currentScreen == EScreens.bestpractices) { return returnTypes[2]; }
            else if (currentScreen == EScreens.faq) { return returnTypes[3]; }
            else if (currentScreen == EScreens.sponsor) { return returnTypes[4]; }
            else if (currentScreen == EScreens.changelog) { return returnTypes[5]; }

            return "You forgot to update GetPageData()";
        }

        #endregion
    }

    //used to prevent mulitple tabs from opening. only opens one
    public class ButtonClicked
    {
        public ButtonClicked(string url)
        {
            Application.OpenURL(url);
        }
    }
}
