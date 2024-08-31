using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Mirage.Logging;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_2021_3_OR_NEWER
using System.Linq;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#endif

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
        private static readonly ILogger logger = LogFactory.GetLogger<WelcomeWindow>();

        private Button lastClickedTab;

        #region Setup

        #region Urls

        private const string WelcomePageUrl = "https://miragenet.github.io/Mirage/";
        private const string QuickStartUrl = "https://miragenet.github.io/Mirage/docs/guides/community-guides/mirage-quick-start-guide";
        private const string ChangelogUrl = "https://github.com/MirageNet/Mirage/blob/main/Assets/Mirage/CHANGELOG.md";
        private const string BestPracticesUrl = "https://miragenet.github.io/Mirage/docs/guides/best-practices";
        private const string FaqUrl = "https://miragenet.github.io/Mirage/docs/guides/faq";
        private const string DiscordInviteUrl = "https://discord.gg/DTBPBYvexy";

#if UNITY_2021_2_OR_NEWER
        private const string BoldReplace = "<b>$1</b>";
#else
        private const string BoldReplace = "$1";
#endif

        private readonly List<Package> packages = new List<Package>();

        #endregion

        //request for the module install
        private AddRequest installRequest;
        private RemoveRequest uninstallRequest;
        private SearchRequest searchRequest;
        private ListRequest listRequest;

        private static WelcomeWindow currentWindow;
        private static VisualTreeAsset _changeLogTemplate;

        //window size of the welcome screen
        private static Vector2 windowSize = new Vector2(500, 415);
        private static string screenToOpenKey = "MirageScreenToOpen";

        //editorprefs keys
        private static string firstStartUpKey = string.Empty;
        private const string firstTimeMirageKey = "MirageWelcome";
        private const string miragePackageName = "com.miragenet.mirage";
        private const int _numberOfChangeLogs = 3;

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

            var dontShow = EditorPrefs.GetBool("DontShowToggle", false);
            if (dontShow)
                return;

            if ((!EditorPrefs.GetBool(firstTimeMirageKey, false) || !EditorPrefs.GetBool(firstStartUpKey, false)) && firstStartUpKey != "MirageUnknown")
            {
                EditorPrefs.SetString(screenToOpenKey, ShowChangeLog ? "ChangeLog" : "Welcome");

#if UNITY_2019_1_OR_NEWER
                OpenWindow();
#else
                if (logger.LogEnabled()) logger.Log($"WelcomeWindow not supported in {Application.unityVersion}, it is only supported in Unity 2020.1 or newer");
#endif
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
            var root = rootVisualElement;
            var uxml = Resources.Load<VisualTreeAsset>("WelcomeWindow");
            var uss = Resources.Load<StyleSheet>("WelcomeWindow");

            _changeLogTemplate = Resources.Load<VisualTreeAsset>("Changelog");

            root.styleSheets.Add(uss);
            uxml.CloneTree(root);

            //set the version text
            var versionText = root.Q<Label>("VersionText");
            versionText.text = "v" + GetVersion();

            DrawChangeLog(ParseChangeLog());

            var dontShowToggle = root.Q<Toggle>("DontShowToggle");
            dontShowToggle.value = EditorPrefs.GetBool("DontShowToggle", false);
            dontShowToggle.tooltip = "Dont show welcome window after there is an update to Mirage";
            dontShowToggle.RegisterValueChangedCallback(b => EditorPrefs.SetBool("DontShowToggle", b.newValue));

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
            var openedButton = rootVisualElement.Q<Button>(EditorPrefs.GetString(screenToOpenKey, "Welcome") + "Button");
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
            var tabButton = rootVisualElement.Q<Button>(tabButtonName);

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

            var redirectButton = rootVisualElement.Q<VisualElement>(tab).Q<Button>("Redirect");
            if (redirectButton != null)
            {
                redirectButton.clicked += () => Application.OpenURL(url);
            }
        }

        //switch between content
        private void ShowTab(string screen)
        {
            var rightColumn = rootVisualElement.Q<VisualElement>("RightColumnBox");

            foreach (var tab in rightColumn.Children())
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
            var content = new List<string>();

            var currentChangeLogs = 0;

            var changeLogPath = FindChangeLog();
            using (var reader = new StreamReader(changeLogPath))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    //we dont need to parse an empty string
                    if (line == string.Empty)
                        continue;

                    //always add the first line
                    if (content.Count == 0)
                    {
                        content.Add(line);
                        continue;
                    }

                    //if we havent reached the next version yet
                    if (line.Contains("https://github.com/MirageNet/Mirage/compare/"))
                    {
                        if (currentChangeLogs == _numberOfChangeLogs)
                            break;

                        currentChangeLogs++;
                    }

                    content.Add(line);
                }
            }

            return content;
        }

        private string FindChangeLog()
        {
#if UNITY_2021_3_OR_NEWER
            var miragePackage = PackageInfo.GetAllRegisteredPackages()
                .Where(x => x.name == miragePackageName)
                .FirstOrDefault();

            // if we are installed via package, then use that
            if (miragePackage != null)
            {
                return miragePackage.assetPath + "/CHANGELOG.md";
            }
            // otherwise gets the asset path of this file, and then find changelog relative to that
            else
#endif
            // note: for unity 2020 or earlier
            //       we just return this path, because GetAllRegisteredPackages doesn't exist
            //       make sure that the "else" is inside the #if so this is valid c#
            {
                return AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)) + "/../../../CHANGELOG.md";
            }
        }

        //draw the parsed information
        private void DrawChangeLog(List<string> content)
        {
            var currentVersionCount = -1;

            var builder = new StringBuilder();
            var firstTitle = true;
            var regexLine = new Regex("^\\* (.*)$");
            var regexBold = new Regex("\\*\\*(.*)\\*\\*");

            for (var i = 0; i < content.Count; i++)
            {
                var item = content[i];

                //if the item is a version
                if (item.Contains("# [") || item.Contains("## ["))
                {
                    currentVersionCount++;

                    var newLog = _changeLogTemplate.CloneTree();

                    newLog.Q<Label>("ChangeLogVersion").text =
                        $"Version {item.Split(new[] { "[" }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new[] { "]" }, StringSplitOptions.RemoveEmptyEntries)[0]}";

                    rootVisualElement.Q<VisualElement>("ChangelogData").Add(newLog);

                    builder = new StringBuilder();
                    firstTitle = true;
                }
                //if the item is a change title
                else if (item.Contains("###"))
                {
                    //only add a space above the title if it isn't the first title
                    if (!firstTitle) { builder.Append("\n"); }

#if UNITY_2021_2_OR_NEWER
                    var subTitle = $"<b>{item.Substring(4)}</b>";
#else
                    var subTitle = item.Substring(4);
#endif
                    builder.Append(subTitle);
                    builder.Append("\n");
                    firstTitle = false;
                }
                //if the item is a change
                else
                {
                    var change = item.Split(new string[] { "([" }, StringSplitOptions.None)[0];
                    change = regexLine.Replace(change, "- $1\n");
                    change = regexBold.Replace(change, BoldReplace);

                    builder.Append(change);
                }

                rootVisualElement.Q<VisualElement>("ChangelogData").ElementAt(currentVersionCount).Q<Label>("ChangeLogText").text = builder.ToString();
            }
        }

        #region Packages

        //configure the package tab when the tab button is pressed
        private void ConfigurePackagesTab()
        {
            var tabButton = rootVisualElement.Q<Button>("PackagesButton");

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

            listRequest = Client.List(true, false);
            searchRequest = Client.SearchAll(false);

            //subscribe to ListPackageProgress for updates
            EditorApplication.update += ListPackageProgress;
        }

        /// <summary>
        ///     Install or uninstall package.
        /// </summary>
        /// <param name="packageName">The package to install or uninstall.</param>
        private void ModuleButtonClicked(string packageName)
        {
            var packageElement = rootVisualElement.Q<VisualElement>("ModulesList").Q<VisualElement>(packageName);

            var packageButton = packageElement.Q<Button>();

            switch (packageButton.text)
            {
                case "Install":
                    InstallPackage(packageName);
                    packageButton.text = "Uninstall";
                    break;
                case "Uninstall":
                    UninstallPackage(packageName);
                    packageButton.text = "Install";
                    break;
            }
        }

        //install the package
        private void InstallPackage(string packageName)
        {
            installRequest = Client.Add(packages.Find((x) => x.displayName == packageName).gitUrl);

            //subscribe to InstallPackageProgress for updates
            EditorApplication.update += InstallPackageProgress;
        }

        //uninstall the package
        private void UninstallPackage(string packageName)
        {
            uninstallRequest = Client.Remove(packages.Find((x) => x.displayName == packageName).packageName);

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
                    if (logger.LogEnabled()) logger.Log("Package install successful.");
                }
                else if (installRequest.Status == StatusCode.Failure)
                {
                    logger.LogError($"Package install was unsuccessful. \n Error Code: {installRequest.Error.errorCode}\n Error Message: {installRequest.Error.message}");

                }

                EditorApplication.update -= InstallPackageProgress;

                //refresh the package tab
                currentWindow.Repaint();
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
                    if (logger.LogEnabled()) logger.Log("Package uninstall successful.");
                }
                else if (uninstallRequest.Status == StatusCode.Failure)
                {
                    logger.LogError($"Package uninstall was unsuccessful. \n Error Code: {uninstallRequest.Error.errorCode}\n Error Message: {uninstallRequest.Error.message}");
                }

                //refresh the package tab
                currentWindow.Repaint();
            }
        }

        private void ListPackageProgress()
        {
            if (!searchRequest.IsCompleted | !listRequest.IsCompleted)
            {
                return;
            }

            EditorApplication.update -= ListPackageProgress;

            switch (searchRequest.Status)
            {
                case StatusCode.Success:

                    packages.Clear();

                    foreach (var package in searchRequest.Result)
                    {
                        if (!package.name.Contains("com.miragenet") || package.name.Equals(miragePackageName)) continue;

                        var packageInstalled = false;

                        foreach (var installedPackages in listRequest.Result)
                        {
                            if (!package.name.Equals(installedPackages.name)) continue;

                            packageInstalled = true;
                        }

                        packages.Add(new Package
                        {
                            displayName = package.displayName,
                            gitUrl = package.name,
                            packageName = package.name,
                            tooltip = package.description,
                            installed = packageInstalled
                        });
                    }

                    ConfigureInstallButtons();

                    break;
                case StatusCode.Failure:
                    if (logger.ErrorEnabled()) logger.LogError($"There was an issue finding packages. \n Error Code: {searchRequest.Error.errorCode}\n Error Message: {searchRequest.Error.message}");
                    break;
            }
        }

        //configures the install button
        //changes text and functionality after button press
        private void ConfigureInstallButtons()
        {
            var moduleVisualElement = rootVisualElement.Q<VisualElement>("ModulesList");
            moduleVisualElement.Clear();

            foreach (var module in packages)
            {
                //set the button and name of the package
                var moduleButton = new Button(() => ModuleButtonClicked(module.displayName))
                {
                    style = { height = new Length(20, LengthUnit.Pixel), position = Position.Relative, left = 40 },
                    text = module.installed ? "Uninstall" : "Install"
                };

                var containerElement = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, alignItems = Align.Center },
                    name = module.displayName
                };

                var label = new Label(module.displayName) { style = { width = new StyleLength(200) }, tooltip = module.tooltip };

                containerElement.Add(label);
                containerElement.Add(moduleButton);

                moduleVisualElement.Add(containerElement);
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
        public string tooltip;
        public bool installed;
    }
}
