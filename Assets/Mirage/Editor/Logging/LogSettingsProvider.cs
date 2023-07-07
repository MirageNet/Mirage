using System.Collections.Generic;
using Mirage.Logging;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mirage.EditorScripts.Logging
{
    public class LogSettingsProvider : SettingsProvider
    {
        private LogSettingsSO settings;

        public LogSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords) { }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new LogSettingsProvider("Mirage/Logging", SettingsScope.Project) { label = "Logging" };
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // look for existing settings first
            if (settings == null)
            {
                settings = EditorLogSettingsLoader.FindLogSettings();
            }

            // then draw field
            var objectField = new ObjectField();
            objectField.objectType = typeof(LogSettingsSO);
            objectField.value = settings;
            objectField.RegisterValueChangedCallback(e =>
            {
                settings = (LogSettingsSO)e.newValue;
                UpdateUI(rootElement);
            });

            rootElement.Add(objectField);

            var inner = new VisualElement();
            rootElement.Add(inner);
            UpdateUI(inner);
        }

        private void UpdateUI(VisualElement rootElement)
        {
            rootElement.Clear();

            if (settings != null)
            {
                var element = LogLevelsGUI_UIElements.Create(settings);
                rootElement.Add(element);
            }
            else
            {
                var createNewButton = DrawCreateNewButton();
                rootElement.Add(createNewButton);
            }
        }

        public static VisualElement DrawCreateNewButton()
        {
            var container = new VisualElement();

            var button = new Button(() =>
            {
                var newSettings = ScriptableObjectUtility.CreateAsset<LogSettingsSO>(nameof(LogSettingsSO), "Assets");
                newSettings.SaveFromLogFactory();
            });

            button.text = "Create New Settings";
            container.Add(button);

            return container;
        }
    }
}
