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
        private Button lastClickedTab;
        private StyleColor defaultButtonBackgroundColor;
        private StyleColor defaultButtonBorderColor;

        #region Setup

        #region Urls

        private const string WelcomePageUrl = "https://miragenet.github.io/Mirage/index.html";
        private const string QuickStartUrl = "https://miragenet.github.io/Mirage/Articles/Guides/CommunityGuides/MirageQuickStartGuide/index.html";
        private const string ChangelogUrl = "https://github.com/MirageNet/Mirage/blob/master/Assets/Mirage/CHANGELOG.md";
        private const string BestPracticesUrl = "https://miragenet.github.io/Mirage/Articles/Guides/BestPractices.html";
        private const string FaqUrl = "https://miragenet.github.io/Mirage/Articles/Guides/FAQ.html";
        private const string SponsorUrl = "";
        private const string DiscordInviteUrl = "https://discord.gg/DTBPBYvexy";

        private readonly List<Package> Packages = new List<Package>()
        {
            new Package { displayName = "LAN Discovery", packageName = "com.miragenet.discovery", gitUrl = "https://github.com/MirageNet/Discovery.git?path=/Assets/Discovery"},
            new Package { displayName = "Steam (Facepunch)", packageName = "com.miragenet.steamyface", gitUrl = "https://github.com/MirageNet/SteamyFaceNG.git?path=/Assets/Mirage/Runtime/Transport/SteamyFaceMirror" },
            new Package { displayName = "Steam (Steamworks.NET)", packageName = "com.miragenet.steamy", gitUrl = "https://github.com/MirageNet/FizzySteamyMirror.git?path=/Assets/Mirage/Runtime/Transport/FizzySteamyMirror" },
        };

        #endregion

        //request for the module install
        private static AddRequest installRequest;
        private static RemoveRequest uninstallRequest;
        private static ListRequest listRequest;

        private static WelcomeWindow currentWindow;

        //window size of the welcome screen
        private static Vector2 windowSize = new Vector2(500, 415);
        private static string screenToOpenKey = "MirageScreenToOpen";

        //editorprefs keys
        private static string firstTimeVersionKey = string.Empty;
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
                if (!EditorPrefs.GetBool(firstTimeMirageKey, false) && !EditorPrefs.GetBool(firstTimeVersionKey, false) && firstTimeVersionKey != "MirageUnknown")
                {
                    return false;
                }
                else if (EditorPrefs.GetBool(firstTimeMirageKey, false) && !EditorPrefs.GetBool(firstTimeVersionKey, false) && firstTimeVersionKey != "MirageUnknown")
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
            firstTimeVersionKey = GetVersion();

            if ((!EditorPrefs.GetBool(firstTimeMirageKey, false) || !EditorPrefs.GetBool(firstTimeVersionKey, false)) && firstTimeVersionKey != "MirageUnknown")
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
            currentWindow = GetWindow<WelcomeWindow>("Mirage Welcome Page");
            //set the position and size
            currentWindow.position = new Rect(new Vector2(100, 100), windowSize);
            //set min and max sizes so we cant readjust window size
            currentWindow.maxSize = windowSize;
            currentWindow.minSize = windowSize;
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

            //set button default colors (used in page nav)
            IStyle sampleStyle = rootVisualElement.Q<Button>("WelcomeButton").style;
            defaultButtonBackgroundColor = sampleStyle.backgroundColor;
            defaultButtonBorderColor = sampleStyle.borderTopColor;

            #region Page buttons

            ConfigureTab("WelcomeButton", "Welcome", WelcomePageUrl);
            ConfigureTab("ChangeLogButton", "ChangeLog", ChangelogUrl);
            ConfigureTab("QuickStartButton", "QuickStart", QuickStartUrl);
            ConfigureTab("BestPracticesButton", "BestPractices", BestPracticesUrl);
            ConfigureTab("FaqButton", "Faq", FaqUrl);
            ConfigureTab("SponsorButton", "Sponsor", SponsorUrl);
            ConfigureTab("DiscordButton", "Discord", DiscordInviteUrl);
            ConfigurePackagesTab();

            ShowTab(EditorPrefs.GetString(screenToOpenKey, "Welcome"));

            //set the screen's button to be tinted when welcome window is opened
            float color = EditorPrefs.GetFloat("buttonClickedColor");
            float borderColor = EditorPrefs.GetFloat("buttonClickedBorderColor");
            Button openedButton = rootVisualElement.Q<Button>(EditorPrefs.GetString(screenToOpenKey, "Welcome") + "Button");
            openedButton.style.backgroundColor = new StyleColor(new Color(color, color, color));
            openedButton.style.borderBottomColor = openedButton.style.borderTopColor = openedButton.style.borderLeftColor = openedButton.style.borderRightColor = new StyleColor(new Color(borderColor, borderColor, borderColor));
            lastClickedTab = openedButton;

            #endregion
        }

        private void OnDisable()
        {
            //now that we have seen the welcome window, 
            //set this this to true so we don't load the window every time we recompile (for the current version)
            EditorPrefs.SetBool(firstTimeVersionKey, true);
            EditorPrefs.SetBool(firstTimeMirageKey, true);
        }

        private void ConfigureTab(string tabButtonName, string tab, string url)
        {
            Button tabButton = rootVisualElement.Q<Button>(tabButtonName);
            tabButton.clicked += () =>
            {
                ToggleMenuButtonColor(tabButton, true);
                ToggleMenuButtonColor(lastClickedTab, false);
                ShowTab(tab);

                lastClickedTab = tabButton;
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

        private void ToggleMenuButtonColor(Button button, bool toggle)
        {
            if (button == null) { return; }

            if (toggle)
            {
                button.style.backgroundColor = button.resolvedStyle.backgroundColor;
                button.style.borderBottomColor = button.style.borderTopColor = button.style.borderLeftColor = button.style.borderRightColor = button.resolvedStyle.borderBottomColor;
                
                EditorPrefs.SetFloat("buttonClickedColor", button.resolvedStyle.backgroundColor.r);
                EditorPrefs.SetFloat("buttonClickedBorderColor", button.resolvedStyle.borderBottomColor.r);
            }
            else
            {
                button.style.backgroundColor = defaultButtonBackgroundColor;
                button.style.borderBottomColor = button.style.borderTopColor = button.style.borderLeftColor = button.style.borderRightColor = defaultButtonBorderColor;
            }
        }

        #region Packages

        //configure the package tab when the tab button is pressed
        private void ConfigurePackagesTab()
        {
            Button tabButton = rootVisualElement.Q<Button>("PackagesButton");
            tabButton.clicked += () =>
            {
                ToggleMenuButtonColor(tabButton, true);
                ToggleMenuButtonColor(lastClickedTab, false);
                ShowTab("Packages");

                lastClickedTab = tabButton;
                EditorPrefs.SetString(screenToOpenKey, "Packages");
            };

            listRequest = UnityEditor.PackageManager.Client.List(true, false);
            EditorApplication.update += ListPackageProgress;
        }

        //install the package
        private void InstallPackage(string packageName)
        {
            installRequest = UnityEditor.PackageManager.Client.Add(Packages.Find((x) => x.displayName == packageName).gitUrl);

            //subscribe to InstallPackageProgress
            EditorApplication.update += InstallPackageProgress;
        }

        //uninstall the package
        private void UninstallPackage(string packageName)
        {
            uninstallRequest = UnityEditor.PackageManager.Client.Remove(Packages.Find((x) => x.displayName == packageName).packageName);

            //subscribe to UninstallPackageProgress
            EditorApplication.update += UninstallPackageProgress;
        }

        //keeps track of the package install progress
        private void InstallPackageProgress()
        {
            if (installRequest.IsCompleted)
            {
                if (installRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Package install successful.");
                }
                else if (installRequest.Status == StatusCode.Failure)
                {
                    Debug.LogError("Package install was unsuccessful. \n Error Code: " + installRequest.Error.errorCode + "\n Error Message: " + installRequest.Error.message);
                }

                EditorApplication.update -= InstallPackageProgress;

                //refresh the package tab
                currentWindow?.Close();
                OpenWindow();
            }
        }

        private void UninstallPackageProgress()
        {
            if (uninstallRequest.IsCompleted)
            {
                EditorApplication.update -= UninstallPackageProgress;

                if (uninstallRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Package uninstall successful.");
                }
                else if (uninstallRequest.Status == StatusCode.Failure)
                {
                    Debug.LogError("Package uninstall was unsuccessful. \n Error Code: " + uninstallRequest.Error.errorCode + "\n Error Message: " + uninstallRequest.Error.message);
                }

                //refresh the package tab
                currentWindow?.Close();
                OpenWindow();
            }
        }

        private void ListPackageProgress()
        {
            if (listRequest.IsCompleted)
            {
                EditorApplication.update -= ListPackageProgress;

                if (listRequest.Status == StatusCode.Success)
                {
                    List<string> installedPackages = new List<string>();

                    foreach (var package in listRequest.Result)
                    {
                        Package? miragePackage = Packages.Find((x) => x.packageName == package.name);
                        if (miragePackage != null)
                        {
                            installedPackages.Add(miragePackage.Value.displayName);
                        }
                    }

                    ConfigureInstallButtons(installedPackages);
                }
                else if (listRequest.Status == StatusCode.Failure)
                {
                    Debug.LogError("There was an issue finding packages. \n Error Code: " + listRequest.Error.errorCode + "\n Error Message: " + listRequest.Error.message);
                }
            }
        }

        private void ConfigureInstallButtons(List<string> installedPackages)
        {
            foreach (VisualElement package in rootVisualElement.Q<VisualElement>("ModulesList").Children())
            {
                Button installButton = package.Q<Button>("InstallButton");
                string packageName = package.Q<Label>("Name").text;
                bool foundInInstalledPackages = installedPackages.Contains(packageName);

                installButton.text = !foundInInstalledPackages ? "Install" : "Uninstall";
                if (!foundInInstalledPackages)
                {
                    installButton.clicked += () => 
                    { 
                        InstallPackage(packageName);
                        installButton.text = "Installing";
                    };
                }
                else
                {
                    installButton.clicked += () => 
                    { 
                        UninstallPackage(packageName);
                        installButton.text = "Uninstalling";
                    };
                }
            }
        }

        #endregion

        #endregion
    }

    //module data type
    public struct Package
    {
        public string displayName;
        public string packageName;
        public string gitUrl;
    }
}
