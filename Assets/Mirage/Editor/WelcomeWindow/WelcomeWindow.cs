using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Collections.Generic;
using System.IO;
using System;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

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
        private readonly List<Button> installButtons = new List<Button>();

        #region Setup

        #region Urls

        private const string WelcomePageUrl = "https://miragenet.github.io/Mirage/index.html";
        private const string QuickStartUrl = "https://miragenet.github.io/Mirage/Articles/Guides/CommunityGuides/MirageQuickStartGuide/index.html";
        private const string ChangelogUrl = "https://github.com/MirageNet/Mirage/blob/master/Assets/Mirage/CHANGELOG.md";
        private const string BestPracticesUrl = "https://miragenet.github.io/Mirage/Articles/Guides/BestPractices.html";
        private const string FaqUrl = "https://miragenet.github.io/Mirage/Articles/Guides/FAQ.html";
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
        private static string firstStartUpKey = string.Empty;
        private const string firstTimeMirageKey = "MirageWelcome";

        /// <summary>
        ///     Hard coded for source code version. If package version is found, this will
        ///     be set later on to package version. through checking for packages anyways.
        /// </summary>
        private static string changeLogPath = "Assets/Mirage/CHANGELOG.md";

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

            DrawChangeLog(ParseChangeLog());

            #region Page buttons

            ConfigureTab("WelcomeButton", "Welcome", WelcomePageUrl);
            ConfigureTab("ChangeLogButton", "ChangeLog", ChangelogUrl);
            ConfigureTab("QuickStartButton", "QuickStart", QuickStartUrl);
            ConfigureTab("BestPracticesButton", "BestPractices", BestPracticesUrl);
            ConfigureTab("FaqButton", "Faq", FaqUrl);
            ConfigureTab("DiscordButton", "Discord", DiscordInviteUrl);
            ConfigurePackagesTab();

            ShowTab(EditorPrefs.GetString(screenToOpenKey, "Welcome"));

            //set the screen's button to be tinted when welcome window is opened
            Button openedButton = rootVisualElement.Q<Button>(EditorPrefs.GetString(screenToOpenKey, "Welcome") + "Button");
            ToggleMenuButtonColor(openedButton, true);
            lastClickedTab = openedButton;

            #endregion
        }

        private void OnDisable()
        {
            //now that we have seen the welcome window, 
            //set this this to true so we don't load the window every time we recompile (for the current version)
            EditorPrefs.SetBool(firstStartUpKey, true);
            EditorPrefs.SetBool(firstTimeMirageKey, true);
        }

        //menu button setup
        private void ConfigureTab(string tabButtonName, string tab, string url)
        {
            Button tabButton = rootVisualElement.Q<Button>(tabButtonName);

            tabButton.EnableInClassList("dark-selected-tab", false);
            tabButton.EnableInClassList("light-selected-tab", false);

            tabButton.clicked += () => 
            {
                ToggleMenuButtonColor(tabButton, true);
                ToggleMenuButtonColor(lastClickedTab, false);
                ShowTab(tab);

                lastClickedTab = tabButton;
                EditorPrefs.SetString(screenToOpenKey, tab);
            };

            Button redirectButton = rootVisualElement.Q<VisualElement>(tab).Q<Button>("Redirect");
            if(redirectButton != null)
            {
                redirectButton.clicked += () => Application.OpenURL(url);
            }
        }

        //switch between content
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

        //changes the background and border color of the button
        //if toggle == true, keep the button tinted
        //otherwise, return the button to the default, not active, colors
        private void ToggleMenuButtonColor(Button button, bool toggle)
        {
            if (button == null) { return; }

            //dark mode
            if (EditorGUIUtility.isProSkin)
            {
                button.EnableInClassList("dark-selected-tab", toggle);
            }
            //light mode
            else
            {
                button.EnableInClassList("light-selected-tab", toggle);
            }
        }

        //parse the change log file
        private List<string> ParseChangeLog()
        {
            List<string> content = new List<string>();

            using(StreamReader reader = new StreamReader(changeLogPath))
            {
                string line;

                while((line = reader.ReadLine()) != null)
                {
                    //we dont need to parse an empty string
                    if(line == string.Empty) 
                        continue;

                    //always add the first line
                    if(content.Count == 0) 
                    {
                        content.Add(line); 
                        continue;
                    }

                    //if we havent reached the next version yet
                    if(line.Contains("https://github.com/MirageNet/Mirage/compare/"))
                        break;

                    content.Add(line);
                }
            }

            return content;
        }

        //draw the parsed information
        private void DrawChangeLog(List<string> content)
        {
            Label changeLogText = rootVisualElement.Q<Label>("ChangeLogText");

            for (int i = 0; i < content.Count; i++)
            {
                string item = content[i];

                //if the item is a version
                if (item.Contains("# [") || item.Contains("## ["))
                {
                    string version = GetVersion();
                    rootVisualElement.Q<Label>("ChangeLogVersion").text = "Version " + version.Substring(0, version.Length - 2);
                }
                //if the item is a change title
                else if (item.Contains("###"))
                {
                    //only add a space above the title if it isn't the first title
                    if (i > 2) { changeLogText.text += "\n"; }

                    changeLogText.text += item.Substring(4) + "\n";
                }
                //if the item is a change
                else
                {
                    string change = item.Split(new string[] { "([" }, StringSplitOptions.None)[0];
                    change = change.Replace("*", "-");
                    changeLogText.text += change + "\n";
                }
            }
        }

        #region Packages

        //configure the package tab when the tab button is pressed
        private void ConfigurePackagesTab()
        {
            Button tabButton = rootVisualElement.Q<Button>("PackagesButton");

            tabButton.EnableInClassList("dark-selected-tab", false);
            tabButton.EnableInClassList("light-selected-tab", false);

            tabButton.clicked += () =>
            {
                ToggleMenuButtonColor(tabButton, true);
                ToggleMenuButtonColor(lastClickedTab, false);
                ShowTab("Packages");

                lastClickedTab = tabButton;
                EditorPrefs.SetString(screenToOpenKey, "Packages");
            };

            listRequest = UnityEditor.PackageManager.Client.List(true, false);

            //subscribe to ListPackageProgress for updates
            EditorApplication.update += ListPackageProgress;
        }

        //install the package
        private void InstallPackage(string packageName)
        {
            installRequest = UnityEditor.PackageManager.Client.Add(Packages.Find((x) => x.displayName == packageName).gitUrl);

            //subscribe to InstallPackageProgress for updates
            EditorApplication.update += InstallPackageProgress;
        }

        //uninstall the package
        private void UninstallPackage(string packageName)
        {
            uninstallRequest = UnityEditor.PackageManager.Client.Remove(Packages.Find((x) => x.displayName == packageName).packageName);

            //subscribe to UninstallPackageProgress for updates
            EditorApplication.update += UninstallPackageProgress;
        }

        //keeps track of the package install progress
        private void InstallPackageProgress()
        {
            if (installRequest.IsCompleted)
            {
                //log results
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
                //log results
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

                    //populate installedPackages
                    foreach (PackageInfo package in listRequest.Result)
                    {
                        Package? miragePackage = Packages.Find((x) => x.packageName == package.name);

                        if (miragePackage?.packageName != null)
                        {
                            if (miragePackage.Value.packageName != null && miragePackage.Value.packageName.Equals("Mirage"))
                            {
                                // Found mirage package let's set up our change log path.
                                changeLogPath = "Packages/com.miragenet.mirage/CHANGELOG.md";
                            }

                            installedPackages.Add(miragePackage.Value.displayName);
                        }
                    }

                    ConfigureInstallButtons(installedPackages);
                }
                //log error
                else if (listRequest.Status == StatusCode.Failure)
                {
                    Debug.LogError("There was an issue finding packages. \n Error Code: " + listRequest.Error.errorCode + "\n Error Message: " + listRequest.Error.message);
                }
            }
        }

        //configures the install button
        //changes text and functionality after button press
        private void ConfigureInstallButtons(List<string> installedPackages)
        {
            //get all the packages
            foreach (VisualElement package in rootVisualElement.Q<VisualElement>("ModulesList").Children())
            {
                //get the button and name of the package
                Button installButton = package.Q<Button>("InstallButton");
                string packageName = package.Q<Label>("Name").text;
                bool foundInInstalledPackages = installedPackages.Contains(packageName);

                //set text
                installButton.text = !foundInInstalledPackages ? "Install" : "Uninstall";
                
                //set functionality
                if (!foundInInstalledPackages)
                {
                    installButton.clicked += () => 
                    { 
                        InstallPackage(packageName);
                        installButton.text = "Installing";
                        DisableInstallButtons();
                    };
                }
                else
                {
                    installButton.clicked += () => 
                    { 
                        UninstallPackage(packageName);
                        installButton.text = "Uninstalling";
                        DisableInstallButtons();
                    };
                }

                installButtons.Add(installButton);
            }
        }

        //prevents user from spamming install button
        //spamming the button while installing/uninstalling throws errors
        //buttons enabled again after window refreshes
        private void DisableInstallButtons()
        {
            foreach (Button button in installButtons)
            {
                button.SetEnabled(false);
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
