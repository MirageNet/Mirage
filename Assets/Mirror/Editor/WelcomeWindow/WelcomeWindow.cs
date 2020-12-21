using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Collections.Generic;

/**
 * Docs used:
 * UXML: https://docs.unity3d.com/Manual/UIE-UXML.html
 * USS: https://docs.unity3d.com/Manual/UIE-USS-SupportedProperties.html
 * Unity Guide: https://learn.unity.com/tutorial/uielements-first-steps/?tab=overview#5cd19ef9edbc2a1156524842
 * External Guide: https://www.raywenderlich.com/6452218-uielements-tutorial-for-unity-getting-started#toc-anchor-010
 */

//this script handles the functionality of the UI

[InitializeOnLoad]
public class WelcomeWindow : EditorWindow
{
    #region Setup

    #region Page variables

    //type of page
    private enum EScreens { welcome, changelog, quickstart, bestpractices, templates, faq, sponsor, discord }

    //the current page
    private EScreens currentScreen = EScreens.welcome;

    //data type that we want to retrieve when we are using this enum
    private enum EPageDataType { header, description, redirectButtonTitle, redirectButtonUrl }

    //scroll position of the changelog
    private Vector2 scrollPos;

    //headers of the different pages
    private static string welcomePageHeader = "Welcome";
    private static string changelogHeader = "Change Log";
    private static string quickStartHeader = "Quick Start Guide";
    private static string bestPracticesHeader = "Best Practices";
    private static string templatesHeader = "Script Templates";
    private static string faqHeader = "FAQ";
    private static string sponsorHeader = "Sponsor Us";

    //descriptions of the different pages
    private static string welcomePageDescription = "Hello! Thank you for installing Mirror. Please visit all the pages on this window. Clicking the button at the bottom of the pages will redirect you to a webpage. Additionally, there are example projects in the Mirror folder that you can look at. \n\nHave fun using Mirror!";
    private static string changelogDescription = "The Change Log is a list of changes made to Mirror. Sometimes these changes can cause your game to break.";
    private static string quickStartDescription = "The Quick Start Guide is meant for people who just started using Mirror. The Quick Start Guide will help new users learn how to accomplish important tasks. It is highly recommended that you complete the guide.";
    private static string bestPracticesDescription = "This page describes the best practices that you should use during development. Currently a work in progress.";
    private static string templatesDescription = "Script templates make it easier to create derived class scripts that inherit from our base classes. The templates have all the possible overrides made for you and organized with comments describing functionality.";
    private static string faqDescription = "The FAQ page holds commonly asked questions. Currently, the FAQ page contains answers to: \n\n   1. Syncing custom data types \n   2. How to connect \n   3. Host migration \n   4. Server lists and matchmaking";
    private static string sponsorDescription = "Sponsoring will give you access to Mirror PRO which gives you special access to tools and priority support.";

    //titles of the redirect buttons
    private static string welcomePageButtonTitle = "Visit API Reference";
    private static string changelogPageButtonTitle = "Visit Change Log";
    private static string quickStartPageButtonTitle = "Visit Quick Start Guide";
    private static string bestPracticesPageButtonTitle = "Visit Best Practices Page";
    private static string templatesPageButtonTitle = "Download Script Templates";
    private static string faqPageButtonTitle = "Visit FAQ";
    private static string sponsorPageButtonTitle = "Sponsor Us";

    #endregion    

    #region Urls

    private static string welcomePageUrl = "https://mirror-networking.com/docs/api/Mirror.html";
    private static string quickStartUrl = "https://mirror-networking.com/docs/Articles/CommunityGuides/MirrorQuickStartGuide/index.html";
    private static string changelogUrl = "https://mirror-networking.com/docs/Articles/General/ChangeLog.html";
    private static string bestPracticesUrl = "https://mirror-networking.com/docs/Articles/Guides/BestPractices.html";
    private static string templatesUrl = "https://mirror-networking.com/docs/Articles/General/ScriptTemplates.html";
    private static string faqUrl = "https://mirror-networking.com/docs/Articles/FAQ.html";
    private static string sponsorUrl = "https://github.com/sponsors/vis2k";
    private static string discordInviteUrl = "https://discord.gg/N9QVxbM";

    #endregion

    //window size of the welcome screen
    private static Vector2 windowSize = new Vector2(500, 600);

    //returns the path to the Mirror folder (ex. Assets/Mirror)
    private static string mirrorPath
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
    private static Texture2D mirrorIcon = null;

    #region Getters for icon and key

    //called only once
    private static string GetStartUpKey()
    {
        //if the file doesnt exist, return unknown mirror version
        if (!File.Exists(mirrorPath + "/package.json"))
        {
            return "MirrorUnknown";
        }

        //read the Version.txt file
        foreach (string line in File.ReadAllLines(mirrorPath + "/package.json"))
        {
            if (line.Contains("version"))
                return line.Substring(7).Replace("\"", "").Replace(",", "");
        }

        return "MirrorUnknown";
    }

    private static Texture2D GetMirrorIcon()
    {
        return (Texture2D)AssetDatabase.LoadAssetAtPath(mirrorPath + "/Icon/MirrorIcon.png", typeof(Texture2D));
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
        firstStartUpKey = GetStartUpKey();
        if (EditorPrefs.GetBool(firstStartUpKey, false) == false && firstStartUpKey != "MirrorUnknown")
        {
            OpenWindow();
            //now that we have seen the welcome window, set this this to true so we don't load the window every time we recompile (for the current version)
            EditorPrefs.SetBool(firstStartUpKey, true);
        }

        EditorApplication.update -= ShowWindowOnFirstStart;
    }

    //open the window (also openable through the path below)
    [MenuItem("Mirror/Welcome")]
    public static void OpenWindow()
    {
        mirrorIcon = GetMirrorIcon();
        //create the window
        WelcomeWindow window = GetWindow<WelcomeWindow>("Mirror Welcome Page");
        //set the position and size
        window.position = new Rect(new Vector2(100, 100), windowSize);
        //set min and max sizes so we cant readjust window size
        window.maxSize = windowSize;
        window.minSize = windowSize;
    }

    #endregion

    #endregion

    #region Displaying UI

    //prevent the image from being cleared on recompile
    private void OnValidate() { mirrorIcon = GetMirrorIcon(); }

    //the code to handle display and button clicking
    private void OnEnable()
    {
        //Load the UI
        //Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(mirrorPath + "/Editor/WelcomeWindow/WelcomeWindow.uxml");
        var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(mirrorPath + "/Editor/WelcomeWindow/WelcomeWindow.uss");

        //Load the descriptions

        uxml.CloneTree(root);
        root.styleSheets.Add(uss);

        //set the icon image
        var icon = root.Q<Image>("Icon");
        icon.image = mirrorIcon;

        //set the version text
        var versionText = root.Q<Label>("VersionText");
        versionText.text = "v" + GetStartUpKey().Substring(6);

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
        redirectButton.clicked += () => { ButtonClicked bc = new ButtonClicked(GetPageData(EPageDataType.redirectButtonUrl).ToString()); };

        #region Page buttons

        List<Button> buttons = new List<Button>();
        string[] buttonHeaders = new string[] { welcomePageHeader, changelogHeader, quickStartHeader, bestPracticesHeader, templatesHeader, faqHeader, sponsorHeader, "Discord" };

        var button0 = root.Q<Button>("Button0");
        button0.text = buttonHeaders[0];
        button0.clicked += () => pageButtonClicked(header, description, redirectButton, 0);

        var button1 = root.Q<Button>("Button1");
        button1.text = buttonHeaders[1];
        button1.clicked += () => pageButtonClicked(header, description, redirectButton, 1);

        var button2 = root.Q<Button>("Button2");
        button2.text = buttonHeaders[2];
        button2.clicked += () => pageButtonClicked(header, description, redirectButton, 2);

        var button3 = root.Q<Button>("Button3");
        button3.text = buttonHeaders[3];
        button3.clicked += () => pageButtonClicked(header, description, redirectButton, 3);

        var button4 = root.Q<Button>("Button4");
        button4.text = buttonHeaders[4];
        button4.clicked += () => pageButtonClicked(header, description, redirectButton, 4);

        var button5 = root.Q<Button>("Button5");
        button5.text = buttonHeaders[5];
        button5.clicked += () => pageButtonClicked(header, description, redirectButton, 5);

        var button6 = root.Q<Button>("Button6");
        button6.text = buttonHeaders[6];
        button6.clicked += () => pageButtonClicked(header, description, redirectButton, 6);

        var button7 = root.Q<Button>("Button7");
        button7.text = buttonHeaders[7];
        button7.clicked += () => pageButtonClicked(header, description, redirectButton, 7);

        #endregion

    }

    private void pageButtonClicked(Label header, Label description, Button redirectButton, int newScreen)
    {
        EScreens screen = (EScreens)newScreen;

        if (screen != EScreens.discord)
        {
            currentScreen = screen;
            header.text = GetPageData(EPageDataType.header).ToString();
            description.text = GetPageData(EPageDataType.description).ToString();
            redirectButton.text = GetPageData(EPageDataType.redirectButtonTitle).ToString();
            redirectButton.clicked += () => { ButtonClicked bc = new ButtonClicked(GetPageData(EPageDataType.redirectButtonUrl).ToString()); };
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
            returnTypes = new string[] { welcomePageHeader, quickStartHeader, bestPracticesHeader, templatesHeader, faqHeader, sponsorHeader, changelogHeader };
        }
        else if (type == EPageDataType.description)
        {
            returnTypes = new string[] { welcomePageDescription, quickStartDescription, bestPracticesDescription, templatesDescription, faqDescription, sponsorDescription, changelogDescription };
        }
        else if (type == EPageDataType.redirectButtonTitle)
        {
            returnTypes = new string[] { welcomePageButtonTitle, quickStartPageButtonTitle, bestPracticesPageButtonTitle, templatesPageButtonTitle, faqPageButtonTitle, sponsorPageButtonTitle, changelogPageButtonTitle };
        }
        else if (type == EPageDataType.redirectButtonUrl)
        {
            returnTypes = new string[] { welcomePageUrl, quickStartUrl, bestPracticesUrl, templatesUrl, faqUrl, sponsorUrl, changelogUrl };
        }

        //return results based on the current page
        if (currentScreen == EScreens.welcome) { return returnTypes[0]; }
        else if (currentScreen == EScreens.quickstart) { return returnTypes[1]; }
        else if (currentScreen == EScreens.bestpractices) { return returnTypes[2]; }
        else if (currentScreen == EScreens.templates) { return returnTypes[3]; }
        else if (currentScreen == EScreens.faq) { return returnTypes[4]; }
        else if (currentScreen == EScreens.sponsor) { return returnTypes[5]; }
        else if (currentScreen == EScreens.changelog) { return returnTypes[6]; }

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
