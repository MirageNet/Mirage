using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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

        #endregion

        //window size of the welcome screen
        private static Vector2 windowSize = new Vector2(500, 415);
        private static string screenToOpenKey = "MirageScreenToOpen";

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
                EditorPrefs.SetString(screenToOpenKey, ShowChangeLog ? "ChangeLog" : "Welcome");

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

            ShowTab(EditorPrefs.GetString(screenToOpenKey, "Welcome"));

            //set the screen's button to be tinted when welcome window is opened
            float color = EditorPrefs.GetFloat("buttonClickedColor");
            float borderColor = EditorPrefs.GetFloat("buttonClickedBorderColor");
            Button openedButton = rootVisualElement.Q<Button>(EditorPrefs.GetString(screenToOpenKey, "Welcome") + "Button");
            openedButton.style.backgroundColor = new StyleColor(new Color(color, color, color));
            openedButton.style.borderBottomColor = openedButton.style.borderTopColor = openedButton.style.borderLeftColor = openedButton.style.borderRightColor = new StyleColor(new Color(borderColor, borderColor, borderColor));

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
            tabButton.clicked += () =>
            {
                ShowTab(tab);
                EditorPrefs.SetString(screenToOpenKey, tab);
            };

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

        #endregion
    }

}
