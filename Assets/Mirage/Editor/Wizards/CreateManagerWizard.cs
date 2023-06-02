using System;
using System.Collections.Generic;
using Mirage.SocketLayer;
using UnityEditor;
using UnityEngine;

namespace Mirage
{
    public class CreateManagerWizard : ScriptableWizard
    {
        public bool addNetworkManager = true;
        public bool addNetworkServer = true;
        public bool addNetworkClient = true;
        public bool addServerObjectManager = true;
        public bool addClientObjectManager = true;
        public bool addCharacterSpawner = false;
        public bool addNetworkSceneManager = false;
        public int networkManagerHudOrGui = 0;

        public MonoScript socketFactory;
        private MonoScript[] socketFactoryScripts;
        private string[] socketFactoryNames;
        private int selectedSocketFactoryIndex;

        private int currentPage = 1;

        [MenuItem("GameObject/Network Manager Wizard")]
        private static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard<CreateManagerWizard>("Create Network Manager Wizard", "Create", "Next");
        }

        private void OnWizardCreate()
        {
            if (currentPage == 4)
            {
                var go = new GameObject("Network Manager");
                if (addNetworkManager) go.AddComponent<NetworkManager>();
                if (addNetworkServer) go.AddComponent<NetworkServer>();
                if (addNetworkClient) go.AddComponent<NetworkClient>();
                if (addServerObjectManager) go.AddComponent<ServerObjectManager>();
                if (addClientObjectManager) go.AddComponent<ClientObjectManager>();
                if (addCharacterSpawner) go.AddComponent<CharacterSpawner>();
                if (addNetworkSceneManager) go.AddComponent<NetworkSceneManager>();

                switch (networkManagerHudOrGui)
                {
                    case 1:
                        go.AddComponent<NetworkManagerHud>();
                        break;
                    case 2:
                        go.AddComponent<NetworkManagerGUI>();
                        break;
                }

                if (socketFactory != null)
                {
                    var scriptType = socketFactory.GetClass();
                    if (scriptType != null)
                    {
                        go.AddComponent(scriptType);
                    }
                }

                // close after creating
                Close();
            }
            else
            {
                currentPage++;
            }
        }

        private void OnWizardOtherButton()
        {
            if (currentPage > 1)
            {
                currentPage--;
            }
        }

        public override IEnumerable<Type> GetExtraPaneTypes()
        {
            return base.GetExtraPaneTypes();
        }
        protected override bool DrawWizardGUI()
        {
            EditorGUI.BeginChangeCheck();
            if (currentPage == 1)
            {
                addNetworkManager = EditorGUILayout.Toggle("Add Network Manager", addNetworkManager);
                addNetworkServer = EditorGUILayout.Toggle("Add Network Server", addNetworkServer);
                addNetworkClient = EditorGUILayout.Toggle("Add Network Client", addNetworkClient);
                addServerObjectManager = EditorGUILayout.Toggle("Add Server Object Manager", addServerObjectManager);
                addClientObjectManager = EditorGUILayout.Toggle("Add Client Object Manager", addClientObjectManager);
            }
            else if (currentPage == 2)
            {
                addCharacterSpawner = EditorGUILayout.Toggle("Add Character Spawner", addCharacterSpawner);
                addNetworkSceneManager = EditorGUILayout.Toggle("Add Network Scene Manager", addNetworkSceneManager);
            }
            else if (currentPage == 3)
            {
                networkManagerHudOrGui = EditorGUILayout.Popup("Select Network Manager HUD or GUI", networkManagerHudOrGui, new string[] {
                    "none",
                    "Network Manager HUD",
                    "Network Manager GUI"
                });
            }
            else if (currentPage == 4)
            {
                selectedSocketFactoryIndex = EditorGUILayout.Popup("Select Socket Factory", selectedSocketFactoryIndex, socketFactoryNames);
                socketFactory = socketFactoryScripts[selectedSocketFactoryIndex];
            }
            return EditorGUI.EndChangeCheck();
        }

        private void OnWizardUpdate()
        {

        }

        private void OnGUI()
        {
            DrawWizardGUI();
            OnWizardUpdate();

            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(currentPage == 1);
            if (GUILayout.Button("Back"))
            {
                OnWizardOtherButton();
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button(currentPage == 4 ? "Create" : "Next"))
            {
                OnWizardCreate();
            }
            GUILayout.EndHorizontal();
        }

        private void OnEnable()
        {
            FindSocketFactoryScripts();
        }

        private void FindSocketFactoryScripts()
        {
            var guids = AssetDatabase.FindAssets("t:MonoScript");
            var scripts = new List<MonoScript>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script.GetClass() != null && script.GetClass().IsSubclassOf(typeof(SocketFactory)))
                {
                    scripts.Add(script);
                }
            }
            socketFactoryScripts = scripts.ToArray();
            socketFactoryNames = new string[socketFactoryScripts.Length];
            for (var i = 0; i < socketFactoryScripts.Length; i++)
            {
                socketFactoryNames[i] = socketFactoryScripts[i].name;
            }
        }
    }
}
