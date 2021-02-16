using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Collections.Generic;

/**
 * Docs used:
 * UXML: https://docs.unity3d.com/Manual/UIE-UXML.html
 * USS: https://docs.unity3d.com/Manual/UIE-USS-SupportedProperties.html
 * Unity Guide: https://learn.unity.com/tutorial/uielements-first-steps/?tab=overview#5cd19ef9edbc2a1156524842
 * External Guide: https://www.raywenderlich.com/6452218-uielements-tutorial-for-unity-getting-started#toc-anchor-010
 */

namespace Mirage
{
    //this script handles the functionality of the UI
    [InitializeOnLoad]
    public class WelcomeWindow : EditorWindow
    {
        #region Setup

        #region Urls

        private const string WelcomePageUrl = "https://miragenet.github.io/Mirage/index.html";
        private const string QuickStartUrl = "https://miragenet.github.io/Mirage/Articles/Guides/CommunityGuides/MirageQuickStartGuide/index.html";
        private const string ChangelogUrl = "https://github.com/MirageNet/Mirage/blob/master/Assets/Mirage/CHANGELOG.md";
        private const string BestPracticesUrl = "https://miragenet.github.io/Mirage/Articles/Guides/BestPractices.html";
        private const string FaqUrl = "https://miragenet.github.io/Mirage/Articles/Guides/FAQ.html";
        private const string SponsorUrl = "";
        private const string DiscordInviteUrl = "https://discord.gg/rp6Fv3JjEz";

        private List<Module> Modules = new List<Module>()
        {
            new Module { name = "Momentum", gitUrl = "https://github.com/MirrorNG/Momentum.git?path=/Assets/Momentum" },
        };

        #endregion

        //request for the module install
        private static AddRequest request;

        //window size of the welcome screen
        private static Vector2 windowSize = new Vector2(500, 415);

        //editorprefs keys
        private static string firstStartUpKey = string.Empty;
        private const string firstTimeMirageKey = "MirageWelcome";

        private static string GetVersion()
        {
            return typeof(NetworkIdentity).Assembly.GetName().Version.ToString();
        }

        #region Handle visibility

        private static bool ShowChangeLog
        {
            get
            {
                if (!EditorPrefs.GetBool(firstTimeMirageKey, false) && !EditorPrefs.GetBool(firstStartUpKey, false) && firstStartUpKey != "MirageUnknown")
                {
                    return false;
                }
                else if (EditorPrefs.GetBool(firstTimeMirageKey, false) && !EditorPrefs.GetBool(firstStartUpKey, false) && firstStartUpKey != "MirageUnknown")
                {
                    return true;
                }

                return false;
            }
        }

        //constructor (called by InitializeOnLoad)
        static WelcomeWindow()
        {
            EditorApplication.update += ShowWindowOnFirstStart;
        }

        private static void ShowWindowOnFirstStart()
        {
            EditorApplication.update -= ShowWindowOnFirstStart;
            firstStartUpKey = GetVersion();

            if ((!EditorPrefs.GetBool(firstTimeMirageKey, false) || !EditorPrefs.GetBool(firstStartUpKey, false)) && firstStartUpKey != "MirageUnknown")
            {
                OpenWindow();
            }
        }

        //open the window (also openable through the path below)
        [MenuItem("Window/Mirage/Welcome")]
        public static void OpenWindow()
        {
            //create the window
            WelcomeWindow window = GetWindow<WelcomeWindow>("Mirage Welcome Page");
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
            VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>("WelcomeWindow");
            StyleSheet uss = Resources.Load<StyleSheet>("WelcomeWindow");

            uxml.CloneTree(root);
            root.styleSheets.Add(uss);

            //set the version text
            Label versionText = root.Q<Label>("VersionText");
            versionText.text = "v" + GetVersion();

            #region Page buttons

            ConfigureTab("WelcomeButton", "Welcome", WelcomePageUrl);
            ConfigureTab("ChangeLogButton", "ChangeLog", ChangelogUrl);
            ConfigureTab("QuickStartButton", "QuickStart", QuickStartUrl);
            ConfigureTab("BestPracticesButton", "BestPractices", BestPracticesUrl);
            ConfigureTab("FaqButton", "Faq", FaqUrl);
            ConfigureTab("SponsorButton", "Sponsor", SponsorUrl);
            ConfigureTab("DiscordButton", "Discord", DiscordInviteUrl);
            ConfigureModulesTab();

            ShowTab(ShowChangeLog ? "ChangeLog" : "Welcome");
            #endregion
        }

        private void OnDisable()
        {
            //now that we have seen the welcome window, 
            //set this this to true so we don't load the window every time we recompile (for the current version)
            EditorPrefs.SetBool(firstStartUpKey, true);
            EditorPrefs.SetBool(firstTimeMirageKey, true);
        }

        private void ConfigureTab(string tabButtonName, string tab, string url)
        {
            Button tabButton = rootVisualElement.Q<Button>(tabButtonName);
            tabButton.clicked += () => ShowTab(tab);
            Button redirectButton = rootVisualElement.Q<VisualElement>(tab).Q<Button>("Redirect");
            redirectButton.clicked += () => Application.OpenURL(url);
        }

        private void ShowTab(string screen)
        {
            VisualElement rightColumn = rootVisualElement.Q<VisualElement>("RightColumnBox");

            foreach (VisualElement tab in rightColumn.Children())
            {
                if (tab.name == screen)
                {
                    if (tab.name == "ChangeLog" && ShowChangeLog)
                    {
                        tab.Q<Label>("Header").text = "Change Log (updated)";
                    }

                    tab.style.display = DisplayStyle.Flex;
                }
                else
                {
                    tab.style.display = DisplayStyle.None;
                }
            }
        }

        #region Modules

        //configure the module tab when the tab button is pressed
        private void ConfigureModulesTab()
        {
            Button tabButton = rootVisualElement.Q<Button>("ModulesButton");
            tabButton.clicked += () => ShowTab("Modules");

            Button install = rootVisualElement.Q<VisualElement>("Modules").Q<Button>("InstallModules");
            install.clicked += () => InstallModules(rootVisualElement.Q<VisualElement>("Modules").Q<ScrollView>("ModulesScrollView"));

            Button uninstall = rootVisualElement.Q<VisualElement>("Modules").Q<Button>("UninstallModules");
            uninstall.clicked += () => UninstallModules(rootVisualElement.Q<VisualElement>("Modules").Q<ScrollView>("ModulesScrollView"));
        }

        //install the module
        private void InstallModules(VisualElement scrollView)
        {
            //subscribe to InstallModuleProgress
            EditorApplication.update += InstallModuleProgress;

            //find the modules that were selected and install them
            foreach (Toggle toggle in scrollView.Children())
            {
                if(toggle.value)
                {
                    request = UnityEditor.PackageManager.Client.Add(Modules.Find((x) => x.name == toggle.label).gitUrl);
                }
            }
        }

        //uninstall the module
        private void UninstallModules(VisualElement scrollView)
        {
            foreach (Toggle toggle in scrollView.Children())
            {
                if (toggle.value)
                {
                    //request = UnityEditor.PackageManager.Client.Remove();
                }
            }
        }

        //keeps track of the module install progress
        private void InstallModuleProgress()
        {
            Label waitLabel = rootVisualElement.Q<Label>("PleaseWaitLabel");

            if(request.IsCompleted)
            {
                if(request.Status == StatusCode.Success)
                {
                    Debug.Log("Module installation successful");
                }
                else if(request.Status == StatusCode.Failure)
                {
                    Debug.LogError("Module installation was unsuccessful. \n Error Code: " + request.Error.errorCode + "\n Error Message: " + request.Error.message);
                }

                rootVisualElement.Q<Label>("PleaseWaitLabel").style.visibility = Visibility.Hidden;
                EditorApplication.update -= InstallModuleProgress;
            }
            else
            {
                waitLabel.style.visibility = Visibility.Visible;
            }
        }

        #endregion

        #endregion
    }

    //module data type
    public struct Module
    {
        public string name;
        public string gitUrl;
    }

}
