#if UNITY_EDITOR
using System;
using System.IO;
using Mirage.SocketLayer;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Mirage.Examples.CoinGrabArena.EditorScripts
{
    /// <summary>
    /// Editor utility that generates all Materials, Prefabs, and the Scene for the Coin Grab Arena sample.
    /// Organized into individual menu steps plus a single "Create All" command so users can inspect or rebuild any part.
    /// </summary>
    public static class CoinGrabArenaBuilder
    {
        private const string MenuRoot = "Tools/Mirage Samples/Coin Grab Arena/";

        // Use built-in Ignore Raycast layer (2) so CoinSpawner.CheckSphere avoids walls and obstacles
        // without requiring users to edit ProjectSettings/TagManager.asset when importing the sample.
        private const int ObstacleLayerIndex = 2;

        private const string PaletteTextureName = "CoinGrabPalette.png";
        private const string PaletteMaterialName = "Palette.mat";
        private const string PlayerBodyMaterialName = "PlayerBody.mat";
        private const string CoinNormalMaterialName = "CoinNormal.mat";
        private const string CoinGoldMaterialName = "CoinGold.mat";
        private const string ArenaFloorMaterialName = "ArenaFloor.mat";
        private const string OuterGroundMaterialName = "OuterGround.mat";

        [MenuItem(MenuRoot + "Create All (Materials, Prefabs & Scene)", priority = 1)]
        public static void CreateAll()
        {
            string sampleRoot = GetSampleRootPath();
            EnsureDirectories(sampleRoot);

            ConfigureModelAndTextureImporters(sampleRoot);
            CreateMaterialsInternal(sampleRoot);
            CreatePrefabsInternal(sampleRoot);
            CreateSceneInternal(sampleRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CoinGrabArenaBuilder] Created all Materials, Prefabs, and Scene in '{sampleRoot}'.");
        }

        [MenuItem(MenuRoot + "Create Materials", priority = 20)]
        public static void CreateMaterials()
        {
            string sampleRoot = GetSampleRootPath();
            EnsureDirectories(sampleRoot);

            ConfigureModelAndTextureImporters(sampleRoot);
            CreateMaterialsInternal(sampleRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CoinGrabArenaBuilder] Created Materials in '{sampleRoot}/Materials'.");
        }

        [MenuItem(MenuRoot + "Create Prefabs", priority = 21)]
        public static void CreatePrefabs()
        {
            string sampleRoot = GetSampleRootPath();
            EnsureDirectories(sampleRoot);

            CreateMaterialsInternal(sampleRoot);
            CreatePrefabsInternal(sampleRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CoinGrabArenaBuilder] Created Prefabs in '{sampleRoot}/Prefabs'.");
        }

        [MenuItem(MenuRoot + "Create Scene", priority = 22)]
        public static void CreateScene()
        {
            string sampleRoot = GetSampleRootPath();
            EnsureDirectories(sampleRoot);

            CreateMaterialsInternal(sampleRoot);
            CreatePrefabsInternal(sampleRoot);
            CreateSceneInternal(sampleRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CoinGrabArenaBuilder] Created Scene at '{sampleRoot}/Scenes/CoinGrabArena.unity'.");
        }

        private static string GetSampleRootPath()
        {
            // Resolve the sample root relative to this script so it works both in the Mirage repo (Assets/Examples/CoinGrabArena)
            // and when imported by a user via Package Manager (Assets/Samples/Mirage/<version>/CoinGrabArena).
            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {nameof(CoinGrabArenaBuilder)}");
            foreach (string guid in guids)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(guid);
                if (scriptPath.EndsWith($"{nameof(CoinGrabArenaBuilder)}.cs"))
                {
                    string scriptsDir = Path.GetDirectoryName(scriptPath)?.Replace('\\', '/');
                    string sampleDir = Path.GetDirectoryName(scriptsDir)?.Replace('\\', '/');
                    if (!string.IsNullOrEmpty(sampleDir))
                        return sampleDir;
                }
            }

            return "Assets/Examples/CoinGrabArena";
        }

        private static void EnsureDirectories(string sampleRoot)
        {
            EnsureFolder(sampleRoot, "Materials");
            EnsureFolder(sampleRoot, "Prefabs");
            EnsureFolder($"{sampleRoot}/Prefabs", "UI");
            EnsureFolder($"{sampleRoot}/Prefabs", "Environment");
            EnsureFolder(sampleRoot, "Scenes");
        }

        private static void EnsureFolder(string parentFolder, string newFolderName)
        {
            string fullPath = $"{parentFolder}/{newFolderName}";
            if (!AssetDatabase.IsValidFolder(fullPath))
                AssetDatabase.CreateFolder(parentFolder, newFolderName);
        }

        private static void ConfigureModelAndTextureImporters(string sampleRoot)
        {
            string palettePath = $"{sampleRoot}/Models/{PaletteTextureName}";
            var textureImporter = AssetImporter.GetAtPath(palettePath) as TextureImporter;
            if (textureImporter != null)
            {
                // Point filtering keeps the 4x4 color swatch grid crisp with no bleeding between UV cells
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.sRGBTexture = true;
                textureImporter.filterMode = FilterMode.Point;
                textureImporter.wrapMode = TextureWrapMode.Clamp;
                textureImporter.mipmapEnabled = false;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                textureImporter.npotScale = TextureImporterNPOTScale.None;
                textureImporter.SaveAndReimport();
            }

            string[] fbxFiles =
            {
                "Player.fbx",
                "Coin.fbx",
                "GoldCoin.fbx",
                "WallBlock.fbx",
                "ObstacleCube.fbx",
                "ObstacleStump.fbx",
                "Tree.fbx",
                "Rock.fbx",
                "PickupStar.fbx"
            };

            foreach (string fbxFile in fbxFiles)
            {
                string fbxPath = $"{sampleRoot}/Models/{fbxFile}";
                var modelImporter = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
                if (modelImporter != null)
                {
                    // Bake axis conversion into mesh vertices so MeshFilter objects stand upright at Quaternion.identity
                    modelImporter.bakeAxisConversion = true;
                    modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
                    modelImporter.importCameras = false;
                    modelImporter.importLights = false;
                    modelImporter.importAnimation = false;
                    modelImporter.animationType = ModelImporterAnimationType.None;
                    modelImporter.SaveAndReimport();
                }
            }
        }

        private static void CreateMaterialsInternal(string sampleRoot)
        {
            var paletteTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{sampleRoot}/Models/{PaletteTextureName}");
            string materialsDir = $"{sampleRoot}/Materials";

            // Shared palette material used by environment props and player eyes
            CreateOrUpdateMaterial(
                $"{materialsDir}/{PaletteMaterialName}",
                Color.white,
                paletteTex,
                smoothness: 0.2f,
                metallic: 0f);

            // Untextured white base material for the player capsule so MaterialPropertyBlock tinting produces pure player colors
            CreateOrUpdateMaterial(
                $"{materialsDir}/{PlayerBodyMaterialName}",
                Color.white,
                mainTex: null,
                smoothness: 0.35f,
                metallic: 0.05f);

            // Coin materials use the palette texture with higher shine so coins stand out on the arena floor
            CreateOrUpdateMaterial(
                $"{materialsDir}/{CoinNormalMaterialName}",
                Color.white,
                paletteTex,
                smoothness: 0.55f,
                metallic: 0.25f);

            CreateOrUpdateMaterial(
                $"{materialsDir}/{CoinGoldMaterialName}",
                Color.white,
                paletteTex,
                smoothness: 0.7f,
                metallic: 0.4f);

            // Ground materials toned down so directional light does not blow out the floor to pure white
            CreateOrUpdateMaterial(
                $"{materialsDir}/{ArenaFloorMaterialName}",
                new Color(0.68f, 0.65f, 0.58f, 1f),
                mainTex: null,
                smoothness: 0.12f,
                metallic: 0f);

            CreateOrUpdateMaterial(
                $"{materialsDir}/{OuterGroundMaterialName}",
                new Color(0.26f, 0.48f, 0.20f, 1f),
                mainTex: null,
                smoothness: 0.08f,
                metallic: 0f);
        }

        private static Material CreateOrUpdateMaterial(string assetPath, Color color, Texture2D mainTex, float smoothness, float metallic)
        {
            Shader shader = GetLitShader();
            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
            }
            else
            {
                material.shader = shader;
            }

            material.color = color;
            material.mainTexture = mainTex;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", mainTex);
            if (material.HasProperty("_Glossiness"))
                material.SetFloat("_Glossiness", smoothness);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", metallic);

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Shader GetLitShader()
        {
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                if (urpShader != null)
                    return urpShader;
            }

            return Shader.Find("Standard");
        }

        private static void CreatePrefabsInternal(string sampleRoot)
        {
            string materialsDir = $"{sampleRoot}/Materials";
            var paletteMat = AssetDatabase.LoadAssetAtPath<Material>($"{materialsDir}/{PaletteMaterialName}");
            var playerBodyMat = AssetDatabase.LoadAssetAtPath<Material>($"{materialsDir}/{PlayerBodyMaterialName}");
            var coinNormalMat = AssetDatabase.LoadAssetAtPath<Material>($"{materialsDir}/{CoinNormalMaterialName}");
            var coinGoldMat = AssetDatabase.LoadAssetAtPath<Material>($"{materialsDir}/{CoinGoldMaterialName}");

            // Core gameplay prefabs in Prefabs/ root so they are easy to find and copy
            ParticleSystem pickupEffectPrefab = CreatePickupEffectPrefab(sampleRoot, coinGoldMat);
            CreateCoinPrefab(sampleRoot, coinNormalMat, coinGoldMat, pickupEffectPrefab);
            CreatePlayerPrefab(sampleRoot, playerBodyMat, paletteMat);

            // Reusable UI prefabs in Prefabs/UI/
            CreateNetworkManagerHudPrefab(sampleRoot);
            CreateCoinGrabHudPrefab(sampleRoot);

            // Environment and obstacle prefabs in Prefabs/Environment/
            CreateWallBlockPrefab(sampleRoot, paletteMat);
            CreateObstacleCubePrefab(sampleRoot, paletteMat);
            CreateObstacleStumpPrefab(sampleRoot, paletteMat);
            CreateTreePrefab(sampleRoot, paletteMat);
            CreateRockPrefab(sampleRoot, paletteMat);
        }

        private static ParticleSystem CreatePickupEffectPrefab(string sampleRoot, Material starMaterial)
        {
            string prefabPath = $"{sampleRoot}/Prefabs/PickupEffect.prefab";
            var root = new GameObject("PickupEffect");

            var ps = root.AddComponent<ParticleSystem>();
            var psRenderer = root.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.duration = 0.6f;
            main.loop = false;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 5.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.55f);
            main.gravityModifier = 1.4f;
            main.maxParticles = 24;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 35f;
            shape.radius = 0.25f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            Mesh starMesh = LoadMeshFromFbx(sampleRoot, "PickupStar.fbx");
            if (starMesh != null)
            {
                psRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                psRenderer.mesh = starMesh;
            }
            psRenderer.sharedMaterial = starMaterial;

            GameObject savedPrefab = SavePrefab(root, prefabPath);
            return savedPrefab.GetComponent<ParticleSystem>();
        }

        private static GameObject CreateCoinPrefab(string sampleRoot, Material normalMat, Material goldMat, ParticleSystem pickupEffectPrefab)
        {
            string prefabPath = $"{sampleRoot}/Prefabs/Coin.prefab";
            var root = new GameObject("Coin");

            root.AddComponent<NetworkIdentity>();

            // Kinematic Rigidbody + trigger Collider allow the server to receive OnTriggerEnter when players walk over the coin
            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Disabled by default on the prefab; Coin.OnStartServer enables it on the server only
            var trigger = root.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.75f;
            trigger.center = Vector3.zero;
            trigger.enabled = false;

            // Child Visual object holds the MeshFilter and MeshRenderer so the model can be swapped cleanly
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            var meshFilter = visual.AddComponent<MeshFilter>();
            var meshRenderer = visual.AddComponent<MeshRenderer>();

            Mesh normalMesh = LoadMeshFromFbx(sampleRoot, "Coin.fbx");
            Mesh goldMesh = LoadMeshFromFbx(sampleRoot, "GoldCoin.fbx");

            AssignMeshAndMaterials(meshFilter, meshRenderer, normalMesh, normalMat);

            var coin = root.AddComponent<Coin>();
            coin.CoinMeshFilter = meshFilter;
            coin.CoinRenderer = meshRenderer;
            coin.TriggerCollider = trigger;
            coin.NormalMesh = normalMesh;
            coin.GoldMesh = goldMesh;
            coin.NormalMaterial = normalMat;
            coin.GoldMaterial = goldMat;
            coin.PickupEffectPrefab = pickupEffectPrefab;

            return SavePrefab(root, prefabPath);
        }

        private static GameObject CreatePlayerPrefab(string sampleRoot, Material playerBodyMat, Material paletteMat)
        {
            string prefabPath = $"{sampleRoot}/Prefabs/CoinGrabPlayer.prefab";
            var root = new GameObject("CoinGrabPlayer");

            root.AddComponent<NetworkIdentity>();

            // Owner-authoritative movement so the local player feels responsive without waiting for a server round-trip
            #pragma warning disable CS0618
            var netTransform = root.AddComponent<NetworkTransform>();
            netTransform.ClientAuthority = true;
            #pragma warning restore CS0618

            // CharacterController starts disabled on the prefab; CoinGrabPlayer.OnStartLocalPlayer enables it for the owner only
            var controller = root.AddComponent<CharacterController>();
            controller.height = 1.6f;
            controller.radius = 0.42f;
            controller.center = new Vector3(0f, 0.8f, 0f);
            controller.skinWidth = 0.04f;
            controller.minMoveDistance = 0f;
            controller.enabled = false;

            // Non-trigger collider enabled on the server by CoinGrabPlayer.OnStartServer so remote client players trigger Coin pickups
            var serverCollider = root.AddComponent<CapsuleCollider>();
            serverCollider.isTrigger = false;
            serverCollider.height = 1.6f;
            serverCollider.radius = 0.42f;
            serverCollider.center = new Vector3(0f, 0.8f, 0f);
            serverCollider.enabled = false;

            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Visual hierarchy separated from root physics/network components so users can swap meshes easily.
            // Rotated 180 degrees on Y because Blender's -Y forward maps to -Z after axis conversion.
            var visuals = new GameObject("Visuals");
            visuals.transform.SetParent(root.transform, false);
            visuals.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            var bodyObj = new GameObject("Body");
            bodyObj.transform.SetParent(visuals.transform, false);
            var bodyFilter = bodyObj.AddComponent<MeshFilter>();
            var bodyRenderer = bodyObj.AddComponent<MeshRenderer>();
            AssignMeshAndMaterials(bodyFilter, bodyRenderer, LoadMeshFromFbx(sampleRoot, "Player.fbx", "Player"), playerBodyMat);

            var eyesObj = new GameObject("Eyes");
            eyesObj.transform.SetParent(visuals.transform, false);
            var eyesFilter = eyesObj.AddComponent<MeshFilter>();
            var eyesRenderer = eyesObj.AddComponent<MeshRenderer>();
            AssignMeshAndMaterials(eyesFilter, eyesRenderer, LoadMeshFromFbx(sampleRoot, "Player.fbx", "Eyes"), paletteMat);

            // Floating 3D name plate tilted toward the top-down camera
            var namePlateObj = new GameObject("NamePlate");
            namePlateObj.transform.SetParent(root.transform, false);
            namePlateObj.transform.localPosition = new Vector3(0f, 2.05f, 0f);
            namePlateObj.transform.localRotation = Quaternion.Euler(55f, 0f, 0f);

            var textMesh = namePlateObj.AddComponent<TextMesh>();
            textMesh.text = "Player";
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = 0.11f;
            textMesh.fontSize = 48;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.color = Color.white;

            var player = root.AddComponent<CoinGrabPlayer>();
            player.CharacterController = controller;
            player.ServerCollider = serverCollider;
            player.BodyRenderer = bodyRenderer;
            player.NameLabel = textMesh;

            return SavePrefab(root, prefabPath);
        }

        private static GameObject CreateNetworkManagerHudPrefab(string sampleRoot)
        {
            string prefabPath = $"{sampleRoot}/Prefabs/UI/NetworkManagerHud.prefab";
            Font font = GetDefaultFont();

            var canvasRoot = CreateCanvasRoot("NetworkManagerHud", sortingOrder: 10);
            var hud = canvasRoot.AddComponent<NetworkManagerHud>();
            hud.NetworkAddress = "localhost";
            hud.DontDestroy = false;

            // Top-left card for Host / Client / Server Only connection controls
            var card = CreatePanel("ConnectionPanel", canvasRoot.transform, new Color(0.11f, 0.13f, 0.18f, 0.92f));
            SetTopLeftRect(card.GetComponent<RectTransform>(), new Vector2(12f, -12f), new Vector2(220f, 188f));

            var statusText = CreateText("StatusLabel", card.transform, font, "Mirage Coin Grab Arena", 14, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.95f, 0.82f, 0.32f));
            SetTopLeftRect(statusText.GetComponent<RectTransform>(), new Vector2(10f, -8f), new Vector2(200f, 22f));

            // Offline container (Host, Client + Address, Server Only)
            var offlineGo = new GameObject("OfflinePanel", typeof(RectTransform));
            offlineGo.transform.SetParent(card.transform, false);
            SetStretchRect(offlineGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

            Button hostButton = CreateButton("HostButton", offlineGo.transform, font, "Host (Server + Client)", new Color(0.22f, 0.58f, 0.34f, 1f));
            SetTopLeftRect(hostButton.GetComponent<RectTransform>(), new Vector2(10f, -36f), new Vector2(200f, 32f));

            Button clientButton = CreateButton("ClientButton", offlineGo.transform, font, "Client", new Color(0.24f, 0.48f, 0.78f, 1f));
            SetTopLeftRect(clientButton.GetComponent<RectTransform>(), new Vector2(10f, -74f), new Vector2(76f, 32f));

            InputField addressInput = CreateInputField("AddressInput", offlineGo.transform, font, "localhost", "Host IP...");
            SetTopLeftRect(addressInput.GetComponent<RectTransform>(), new Vector2(92f, -74f), new Vector2(118f, 32f));

            Button serverButton = CreateButton("ServerOnlyButton", offlineGo.transform, font, "Server Only", new Color(0.32f, 0.35f, 0.42f, 1f));
            SetTopLeftRect(serverButton.GetComponent<RectTransform>(), new Vector2(10f, -112f), new Vector2(200f, 30f));

            var controlsHint = CreateText("ControlsHint", offlineGo.transform, font, "Move: WASD | Dash: Space", 12, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.78f, 0.82f, 0.88f));
            SetTopLeftRect(controlsHint.GetComponent<RectTransform>(), new Vector2(10f, -150f), new Vector2(200f, 28f));

            // Online container (Stop button shown while connected/hosting)
            var onlineGo = new GameObject("OnlinePanel", typeof(RectTransform));
            onlineGo.transform.SetParent(card.transform, false);
            SetStretchRect(onlineGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

            Button stopButton = CreateButton("StopButton", onlineGo.transform, font, "Stop / Disconnect", new Color(0.78f, 0.26f, 0.26f, 1f));
            SetTopLeftRect(stopButton.GetComponent<RectTransform>(), new Vector2(10f, -40f), new Vector2(200f, 34f));

            onlineGo.SetActive(false);

            hud.NetworkAddressInput = addressInput;
            hud.OfflineGO = offlineGo;
            hud.OnlineGO = onlineGo;
            hud.StatusLabel = statusText;

            // Persist UI button listeners on the prefab so they work out of the box when instantiated
            UnityEventTools.AddPersistentListener(hostButton.onClick, hud.StartHostButtonHandler);
            UnityEventTools.AddPersistentListener(clientButton.onClick, hud.StartClientButtonHandler);
            UnityEventTools.AddPersistentListener(serverButton.onClick, hud.StartServerOnlyButtonHandler);
            UnityEventTools.AddPersistentListener(stopButton.onClick, hud.StopButtonHandler);
            UnityEventTools.AddVoidPersistentListener(addressInput.onValueChanged, hud.OnNetworkAddressInputUpdate);

            return SavePrefab(canvasRoot, prefabPath);
        }

        private static GameObject CreateCoinGrabHudPrefab(string sampleRoot)
        {
            string prefabPath = $"{sampleRoot}/Prefabs/UI/CoinGrabHud.prefab";
            Font font = GetDefaultFont();

            var canvasRoot = CreateCanvasRoot("CoinGrabHud", sortingOrder: 0);
            var coinGrabHud = canvasRoot.AddComponent<CoinGrabHud>();

            var hudRoot = new GameObject("HudRoot", typeof(RectTransform));
            hudRoot.transform.SetParent(canvasRoot.transform, false);
            SetStretchRect(hudRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

            // Top-center banner showing round state, countdown timer, and player name input
            var topBar = CreatePanel("TopBanner", hudRoot.transform, new Color(0.11f, 0.13f, 0.18f, 0.90f));
            var topBarRect = topBar.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0.5f, 1f);
            topBarRect.anchorMax = new Vector2(0.5f, 1f);
            topBarRect.pivot = new Vector2(0.5f, 1f);
            topBarRect.anchoredPosition = new Vector2(0f, -12f);
            topBarRect.sizeDelta = new Vector2(340f, 114f);

            Text stateText = CreateText("StateText", topBar.transform, font, "Waiting for players (0/2)", 16, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            SetTopLeftRect(stateText.GetComponent<RectTransform>(), new Vector2(12f, -8f), new Vector2(316f, 24f));

            Text timerText = CreateText("TimerText", topBar.transform, font, "", 20, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.25f));
            SetTopLeftRect(timerText.GetComponent<RectTransform>(), new Vector2(12f, -34f), new Vector2(316f, 28f));

            Button startAloneButton = CreateButton("StartAloneButton", topBar.transform, font, "Start Alone", new Color(0.22f, 0.58f, 0.34f, 1f));
            SetTopLeftRect(startAloneButton.GetComponent<RectTransform>(), new Vector2(95f, -35f), new Vector2(150f, 28f));

            InputField nameInput = CreateInputField("NameInputField", topBar.transform, font, "", "Player Name...");
            nameInput.characterLimit = 20;
            SetTopLeftRect(nameInput.GetComponent<RectTransform>(), new Vector2(16f, -70f), new Vector2(204f, 32f));

            Button setNameButton = CreateButton("SetNameButton", topBar.transform, font, "Set Name", new Color(0.24f, 0.52f, 0.78f, 1f));
            SetTopLeftRect(setNameButton.GetComponent<RectTransform>(), new Vector2(228f, -70f), new Vector2(96f, 32f));

            // Top-right scoreboard panel (CoinGrabHud.RebuildScoreboard writes "<b>SCOREBOARD</b>" + player rows)
            var scoreboardPanel = CreatePanel("ScoreboardPanel", hudRoot.transform, new Color(0.11f, 0.13f, 0.18f, 0.90f));
            var scoreboardRect = scoreboardPanel.GetComponent<RectTransform>();
            scoreboardRect.anchorMin = new Vector2(1f, 1f);
            scoreboardRect.anchorMax = new Vector2(1f, 1f);
            scoreboardRect.pivot = new Vector2(1f, 1f);
            scoreboardRect.anchoredPosition = new Vector2(-12f, -12f);
            scoreboardRect.sizeDelta = new Vector2(210f, 200f);

            Text scoreboardText = CreateText("ScoreboardText", scoreboardPanel.transform, font, "<b>SCOREBOARD</b>\nPlayer 1 (You): 0", 14, FontStyle.Normal, TextAnchor.UpperLeft, Color.white);
            scoreboardText.supportRichText = true;
            scoreboardText.lineSpacing = 1.15f;
            SetTopLeftRect(scoreboardText.GetComponent<RectTransform>(), new Vector2(14f, -12f), new Vector2(182f, 176f));

            // Center-screen results overlay panel shown at the end of a round
            var resultsPanel = CreatePanel("ResultsPanel", hudRoot.transform, new Color(0.08f, 0.10f, 0.14f, 0.95f));
            var resultsRect = resultsPanel.GetComponent<RectTransform>();
            resultsRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultsRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultsRect.pivot = new Vector2(0.5f, 0.5f);
            resultsRect.anchoredPosition = Vector2.zero;
            resultsRect.sizeDelta = new Vector2(400f, 130f);

            Text resultsHeader = CreateText("ResultsHeader", resultsPanel.transform, font, "ROUND COMPLETE", 18, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.95f, 0.82f, 0.32f));
            SetTopLeftRect(resultsHeader.GetComponent<RectTransform>(), new Vector2(16f, -14f), new Vector2(368f, 26f));

            Text winnerText = CreateText("WinnerText", resultsPanel.transform, font, "Player 1 wins!", 22, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            SetTopLeftRect(winnerText.GetComponent<RectTransform>(), new Vector2(16f, -46f), new Vector2(368f, 68f));

            resultsPanel.SetActive(false);

            coinGrabHud.HudRoot = hudRoot;
            coinGrabHud.StateText = stateText;
            coinGrabHud.TimerText = timerText;
            coinGrabHud.StartAloneButton = startAloneButton;
            coinGrabHud.ScoreboardText = scoreboardText;
            coinGrabHud.ResultsPanel = resultsPanel;
            coinGrabHud.WinnerText = winnerText;
            coinGrabHud.NameInputField = nameInput;
            coinGrabHud.SetNameButton = setNameButton;

            return SavePrefab(canvasRoot, prefabPath);
        }

        private static GameObject CreateWallBlockPrefab(string sampleRoot, Material paletteMat)
        {
            string prefabPath = $"{sampleRoot}/Prefabs/Environment/WallBlock.prefab";
            var root = CreateMeshPrefabRoot("WallBlock", sampleRoot, "WallBlock.fbx", paletteMat);
            root.layer = ObstacleLayerIndex;

            var collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(3f, 1.2f, 1f);
            collider.center = new Vector3(0f, 0.6f, 0f);

            return SavePrefab(root, prefabPath);
        }

        private static GameObject CreateObstacleCubePrefab(string sampleRoot, Material paletteMat)
        {
            string prefabPath = $"{sampleRoot}/Prefabs/Environment/ObstacleCube.prefab";
            var root = CreateMeshPrefabRoot("ObstacleCube", sampleRoot, "ObstacleCube.fbx", paletteMat);
            root.layer = ObstacleLayerIndex;

            var collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(2f, 2f, 2f);
            collider.center = new Vector3(0f, 1f, 0f);

            return SavePrefab(root, prefabPath);
        }

        private static GameObject CreateObstacleStumpPrefab(string sampleRoot, Material paletteMat)
        {
            string prefabPath = $"{sampleRoot}/Prefabs/Environment/ObstacleStump.prefab";
            var root = CreateMeshPrefabRoot("ObstacleStump", sampleRoot, "ObstacleStump.fbx", paletteMat);
            root.layer = ObstacleLayerIndex;

            var collider = root.AddComponent<CapsuleCollider>();
            collider.radius = 0.72f;
            collider.height = 1.6f;
            collider.center = new Vector3(0f, 0.8f, 0f);

            return SavePrefab(root, prefabPath);
        }

        private static GameObject CreateTreePrefab(string sampleRoot, Material paletteMat)
        {
            string prefabPath = $"{sampleRoot}/Prefabs/Environment/Tree.prefab";
            var root = CreateMeshPrefabRoot("Tree", sampleRoot, "Tree.fbx", paletteMat);
            return SavePrefab(root, prefabPath);
        }

        private static GameObject CreateRockPrefab(string sampleRoot, Material paletteMat)
        {
            string prefabPath = $"{sampleRoot}/Prefabs/Environment/Rock.prefab";
            var root = CreateMeshPrefabRoot("Rock", sampleRoot, "Rock.fbx", paletteMat);
            return SavePrefab(root, prefabPath);
        }

        private static GameObject CreateMeshPrefabRoot(string name, string sampleRoot, string fbxFileName, Material material)
        {
            var root = new GameObject(name);
            var meshFilter = root.AddComponent<MeshFilter>();
            var meshRenderer = root.AddComponent<MeshRenderer>();

            Mesh mesh = LoadMeshFromFbx(sampleRoot, fbxFileName);
            AssignMeshAndMaterials(meshFilter, meshRenderer, mesh, material);

            return root;
        }

        private static void AssignMeshAndMaterials(MeshFilter meshFilter, MeshRenderer meshRenderer, Mesh mesh, Material material)
        {
            meshFilter.sharedMesh = mesh;

            // Blender exports one submesh per material slot; assign the shared palette material to every submesh
            // so foliage cones on Tree, wood rings on ObstacleStump, and faces/stars on Coins all render properly
            int subMeshCount = mesh != null ? Mathf.Max(1, mesh.subMeshCount) : 1;
            var materials = new Material[subMeshCount];
            for (int i = 0; i < subMeshCount; i++)
                materials[i] = material;

            meshRenderer.sharedMaterials = materials;
        }

        private static void CreateSceneInternal(string sampleRoot)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Load generated materials and prefabs
            var arenaFloorMat = AssetDatabase.LoadAssetAtPath<Material>($"{sampleRoot}/Materials/{ArenaFloorMaterialName}");
            var outerGroundMat = AssetDatabase.LoadAssetAtPath<Material>($"{sampleRoot}/Materials/{OuterGroundMaterialName}");

            var playerPrefab = AssetDatabase.LoadAssetAtPath<CoinGrabPlayer>($"{sampleRoot}/Prefabs/CoinGrabPlayer.prefab");
            var coinPrefab = AssetDatabase.LoadAssetAtPath<Coin>($"{sampleRoot}/Prefabs/Coin.prefab");
            var netHudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{sampleRoot}/Prefabs/UI/NetworkManagerHud.prefab");
            var gameHudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{sampleRoot}/Prefabs/UI/CoinGrabHud.prefab");

            var wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{sampleRoot}/Prefabs/Environment/WallBlock.prefab");
            var cubePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{sampleRoot}/Prefabs/Environment/ObstacleCube.prefab");
            var stumpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{sampleRoot}/Prefabs/Environment/ObstacleStump.prefab");
            var treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{sampleRoot}/Prefabs/Environment/Tree.prefab");
            var rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{sampleRoot}/Prefabs/Environment/Rock.prefab");

            // Camera positioned high enough to frame all 4 walls and surrounding trees/rocks below the TopBanner UI
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(0f, 34f, -21f);
            cameraGo.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.18f, 0.32f, 0.15f, 1f);
            camera.fieldOfView = 45f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 120f;
            cameraGo.AddComponent<AudioListener>();

            // Warm directional sunlight (moderate intensity so the stone floor is not blown out to pure white)
            var lightGo = new GameObject("Directional Light");
            lightGo.transform.position = new Vector3(0f, 15f, 0f);
            lightGo.transform.rotation = Quaternion.Euler(52f, -35f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.97f, 0.90f, 1f);
            light.intensity = 0.85f;
            light.shadows = LightShadows.Soft;

            // Arena environment hierarchy
            Transform[] spawnPoints = BuildArenaEnvironment(
                arenaFloorMat,
                outerGroundMat,
                wallPrefab,
                cubePrefab,
                stumpPrefab,
                treePrefab,
                rockPrefab);

            // NetworkManager + Spawner setup
            var networkManagerGo = new GameObject("NetworkManager");
            Type udpSocketType = Type.GetType("Mirage.Sockets.Udp.UdpSocketFactory, Mirage.Sockets.Udp");
            SocketFactory socketFactory = udpSocketType != null
                ? (SocketFactory)networkManagerGo.AddComponent(udpSocketType)
                : null;
            var server = networkManagerGo.AddComponent<NetworkServer>();
            var client = networkManagerGo.AddComponent<NetworkClient>();
            var serverObjectManager = networkManagerGo.AddComponent<ServerObjectManager>();
            var clientObjectManager = networkManagerGo.AddComponent<ClientObjectManager>();
            var networkManager = networkManagerGo.AddComponent<NetworkManager>();
            var playerSpawner = networkManagerGo.AddComponent<CoinGrabSpawner>();

            server.SocketFactory = socketFactory;
            server.ObjectManager = serverObjectManager;

            client.SocketFactory = socketFactory;
            client.ObjectManager = clientObjectManager;

            networkManager.Server = server;
            networkManager.Client = client;
            networkManager.ServerObjectManager = serverObjectManager;
            networkManager.ClientObjectManager = clientObjectManager;

            // Register all dynamic networked prefabs on ClientObjectManager so clients can spawn them
            clientObjectManager.spawnPrefabs.Clear();
            clientObjectManager.spawnPrefabs.Add(playerPrefab.Identity);
            clientObjectManager.spawnPrefabs.Add(coinPrefab.Identity);

            playerSpawner.Server = server;
            playerSpawner.ServerObjectManager = serverObjectManager;
            playerSpawner.PlayerPrefab = playerPrefab;
            playerSpawner.SpawnPoints.Clear();
            playerSpawner.SpawnPoints.AddRange(spawnPoints);

            // Scene NetworkBehaviour GameManager + CoinSpawner
            var gameManagerGo = new GameObject("GameManager");
            gameManagerGo.AddComponent<NetworkIdentity>();
            var coinSpawner = gameManagerGo.AddComponent<CoinSpawner>();
            var gameManager = gameManagerGo.AddComponent<CoinGrabGameManager>();

            coinSpawner.Server = server;
            coinSpawner.ServerObjectManager = serverObjectManager;
            coinSpawner.GameManager = gameManager;
            coinSpawner.CoinPrefab = coinPrefab;
            coinSpawner.ObstacleLayer = 1 << ObstacleLayerIndex;
            coinSpawner.ArenaHalfExtent = 10.5f;

            gameManager.CoinSpawner = coinSpawner;
            gameManager.PlayerSpawner = playerSpawner;

            // Instantiate UI prefabs so users can inspect or copy them directly from the Prefabs/UI folder
            var netHudInstance = (GameObject)PrefabUtility.InstantiatePrefab(netHudPrefab);
            var netHud = netHudInstance.GetComponent<NetworkManagerHud>();
            netHud.NetworkManager = networkManager;
            EditorUtility.SetDirty(netHud);

            var gameHudInstance = (GameObject)PrefabUtility.InstantiatePrefab(gameHudPrefab);
            var gameHud = gameHudInstance.GetComponent<CoinGrabHud>();
            gameHud.Server = server;
            gameHud.Client = client;
            gameHud.GameManager = gameManager;
            EditorUtility.SetDirty(gameHud);

            // EventSystem required for Unity UI buttons and input fields
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();

            string scenePath = $"{sampleRoot}/Scenes/CoinGrabArena.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static Transform[] BuildArenaEnvironment(
            Material arenaFloorMat,
            Material outerGroundMat,
            GameObject wallPrefab,
            GameObject cubePrefab,
            GameObject stumpPrefab,
            GameObject treePrefab,
            GameObject rockPrefab)
        {
            var arenaRoot = new GameObject("Arena");

            // Ground surfaces: ArenaFloor sized to 26x26 so its outer edges tuck cleanly inside the perimeter walls
            var groundGroup = new GameObject("Ground");
            groundGroup.transform.SetParent(arenaRoot.transform, false);

            var arenaFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arenaFloor.name = "ArenaFloor";
            arenaFloor.transform.SetParent(groundGroup.transform, false);
            arenaFloor.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            arenaFloor.transform.localScale = new Vector3(26f, 0.5f, 26f);
            arenaFloor.GetComponent<MeshRenderer>().sharedMaterial = arenaFloorMat;

            var outerGround = GameObject.CreatePrimitive(PrimitiveType.Cube);
            outerGround.name = "OuterGround";
            outerGround.transform.SetParent(groundGroup.transform, false);
            outerGround.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            outerGround.transform.localScale = new Vector3(90f, 0.5f, 90f);
            outerGround.GetComponent<MeshRenderer>().sharedMaterial = outerGroundMat;

            // 8 player spawn points evenly spaced on a ring of radius 9.2m
            var spawnPointsGroup = new GameObject("SpawnPoints");
            spawnPointsGroup.transform.SetParent(arenaRoot.transform, false);
            var spawnPoints = new Transform[8];
            const float spawnRingRadius = 9.2f;

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                float angleRad = i * Mathf.PI * 2f / spawnPoints.Length;
                var sp = new GameObject($"SpawnPoint_{i + 1}");
                sp.transform.SetParent(spawnPointsGroup.transform, false);
                sp.transform.localPosition = new Vector3(Mathf.Cos(angleRad) * spawnRingRadius, 0f, Mathf.Sin(angleRad) * spawnRingRadius);
                sp.transform.LookAt(Vector3.zero);
                spawnPoints[i] = sp.transform;
            }

            // Perimeter walls: 8 segments of 3m per side spanning [-12, +12], capped by 2x2x2 corner pillars at (±13, 0, ±13)
            var wallsGroup = new GameObject("PerimeterWalls");
            wallsGroup.transform.SetParent(arenaRoot.transform, false);
            const int blocksPerSide = 8;
            const float blockLength = 3f;
            const float halfSpan = 13f;

            for (int i = 0; i < blocksPerSide; i++)
            {
                float offset = (i - (blocksPerSide - 1) * 0.5f) * blockLength;
                PlacePrefabInstance(wallPrefab, wallsGroup.transform, new Vector3(offset, 0f, halfSpan), Quaternion.identity, Vector3.one);
                PlacePrefabInstance(wallPrefab, wallsGroup.transform, new Vector3(offset, 0f, -halfSpan), Quaternion.identity, Vector3.one);
                PlacePrefabInstance(wallPrefab, wallsGroup.transform, new Vector3(halfSpan, 0f, offset), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
                PlacePrefabInstance(wallPrefab, wallsGroup.transform, new Vector3(-halfSpan, 0f, offset), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
            }

            Vector3[] cornerPillarPositions =
            {
                new Vector3(-halfSpan, 0f, halfSpan),
                new Vector3(halfSpan, 0f, halfSpan),
                new Vector3(-halfSpan, 0f, -halfSpan),
                new Vector3(halfSpan, 0f, -halfSpan)
            };

            foreach (Vector3 cornerPos in cornerPillarPositions)
                PlacePrefabInstance(cubePrefab, wallsGroup.transform, cornerPos, Quaternion.identity, Vector3.one);

            // Interior obstacles (cubes and tree stumps on the ObstacleLayer so coins never spawn inside them)
            var obstaclesGroup = new GameObject("Obstacles");
            obstaclesGroup.transform.SetParent(arenaRoot.transform, false);

            Vector3[] cubePositions =
            {
                new Vector3(-5.5f, 0f, 5.5f),
                new Vector3(5.5f, 0f, 5.5f),
                new Vector3(-5.5f, 0f, -5.5f),
                new Vector3(5.5f, 0f, -5.5f)
            };

            foreach (Vector3 pos in cubePositions)
                PlacePrefabInstance(cubePrefab, obstaclesGroup.transform, pos, Quaternion.identity, Vector3.one);

            Vector3[] stumpPositions =
            {
                new Vector3(0f, 0f, 6.5f),
                new Vector3(0f, 0f, -6.5f),
                new Vector3(-6.5f, 0f, 0f),
                new Vector3(6.5f, 0f, 0f)
            };

            foreach (Vector3 pos in stumpPositions)
                PlacePrefabInstance(stumpPrefab, obstaclesGroup.transform, pos, Quaternion.identity, Vector3.one);

            // Outer decorative trees and rocks around the border of the arena
            var sceneryGroup = new GameObject("Scenery");
            sceneryGroup.transform.SetParent(arenaRoot.transform, false);

            (Vector3 pos, float rotY, float scale)[] trees =
            {
                (new Vector3(-17.5f, -0.35f, 15.5f), 15f, 1.1f),
                (new Vector3(-11.0f, -0.35f, 17.0f), 80f, 0.95f),
                (new Vector3(-2.5f, -0.35f, 16.5f), 140f, 1.15f),
                (new Vector3(6.5f, -0.35f, 17.2f), 210f, 1.0f),
                (new Vector3(15.5f, -0.35f, 16.0f), 45f, 1.2f),
                (new Vector3(17.5f, -0.35f, 8.5f), 110f, 1.05f),
                (new Vector3(18.0f, -0.35f, -1.5f), 260f, 0.9f),
                (new Vector3(17.0f, -0.35f, -10.5f), 310f, 1.15f),
                (new Vector3(15.0f, -0.35f, -16.5f), 25f, 1.0f),
                (new Vector3(5.0f, -0.35f, -16.8f), 175f, 0.95f),
                (new Vector3(-6.0f, -0.35f, -16.5f), 290f, 1.1f),
                (new Vector3(-15.5f, -0.35f, -16.0f), 65f, 1.05f),
                (new Vector3(-17.5f, -0.35f, -8.5f), 195f, 1.2f),
                (new Vector3(-18.0f, -0.35f, 2.0f), 330f, 0.95f),
                (new Vector3(-17.0f, -0.35f, 10.0f), 120f, 1.0f)
            };

            foreach (var (pos, rotY, scale) in trees)
                PlacePrefabInstance(treePrefab, sceneryGroup.transform, pos, Quaternion.Euler(0f, rotY, 0f), Vector3.one * scale);

            (Vector3 pos, float rotY, float scale)[] rocks =
            {
                (new Vector3(-15.8f, -0.35f, 13.5f), 40f, 1.0f),
                (new Vector3(-7.0f, -0.35f, 16.0f), 125f, 0.85f),
                (new Vector3(11.5f, -0.35f, 16.2f), 230f, 1.15f),
                (new Vector3(16.5f, -0.35f, 4.0f), 15f, 0.9f),
                (new Vector3(16.2f, -0.35f, -6.0f), 160f, 1.1f),
                (new Vector3(10.5f, -0.35f, -16.0f), 85f, 0.95f),
                (new Vector3(-11.5f, -0.35f, -15.8f), 275f, 1.05f),
                (new Vector3(-16.5f, -0.35f, -3.5f), 190f, 0.9f)
            };

            foreach (var (pos, rotY, scale) in rocks)
                PlacePrefabInstance(rockPrefab, sceneryGroup.transform, pos, Quaternion.Euler(0f, rotY, 0f), Vector3.one * scale);

            return spawnPoints;
        }

        private static GameObject PlacePrefabInstance(GameObject prefab, Transform parent, Vector3 localPos, Quaternion localRot, Vector3 localScale)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.localPosition = localPos;
            instance.transform.localRotation = localRot;
            instance.transform.localScale = localScale;
            return instance;
        }

        private static Mesh LoadMeshFromFbx(string sampleRoot, string fbxFileName, string subMeshName = null)
        {
            string fbxPath = $"{sampleRoot}/Models/{fbxFileName}";
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            Mesh fallback = null;

            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Mesh mesh)
                {
                    if (string.IsNullOrEmpty(subMeshName) || mesh.name == subMeshName)
                        return mesh;

                    if (fallback == null)
                        fallback = mesh;
                }
            }

            return fallback;
        }

        private static GameObject SavePrefab(GameObject root, string prefabPath)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateCanvasRoot(string name, int sortingOrder)
        {
            var canvasGo = new GameObject(name);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            // Fixed pixel size keeps UI crisp and readable when testing multiple small windowed instances side-by-side
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            canvasGo.AddComponent<GraphicRaycaster>();
            return canvasGo;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var panelGo = new GameObject(name, typeof(RectTransform));
            panelGo.transform.SetParent(parent, false);
            var image = panelGo.AddComponent<Image>();
            image.color = color;
            return panelGo;
        }

        private static Text CreateText(string name, Transform parent, Font font, string content, int fontSize, FontStyle fontStyle, TextAnchor alignment, Color color)
        {
            var textGo = new GameObject(name, typeof(RectTransform));
            textGo.transform.SetParent(parent, false);
            var text = textGo.AddComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, Font font, string label, Color bgColor)
        {
            var buttonGo = new GameObject(name, typeof(RectTransform));
            buttonGo.transform.SetParent(parent, false);

            var image = buttonGo.AddComponent<Image>();
            image.color = bgColor;

            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = image;

            Text labelText = CreateText("Label", buttonGo.transform, font, label, 15, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            SetStretchRect(labelText.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

            return button;
        }

        private static InputField CreateInputField(string name, Transform parent, Font font, string initialText, string placeholderText)
        {
            var inputGo = new GameObject(name, typeof(RectTransform));
            inputGo.transform.SetParent(parent, false);

            var bgImage = inputGo.AddComponent<Image>();
            bgImage.color = new Color(0.18f, 0.21f, 0.28f, 1f);

            var inputField = inputGo.AddComponent<InputField>();
            inputField.targetGraphic = bgImage;

            Text placeholder = CreateText("Placeholder", inputGo.transform, font, placeholderText, 14, FontStyle.Italic, TextAnchor.MiddleLeft, new Color(0.62f, 0.66f, 0.72f, 0.85f));
            SetStretchRect(placeholder.GetComponent<RectTransform>(), new Vector2(10f, 2f), new Vector2(-10f, -2f));

            Text textComponent = CreateText("Text", inputGo.transform, font, initialText, 14, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);
            textComponent.supportRichText = false;
            SetStretchRect(textComponent.GetComponent<RectTransform>(), new Vector2(10f, 2f), new Vector2(-10f, -2f));

            inputField.placeholder = placeholder;
            inputField.textComponent = textComponent;
            inputField.text = initialText;

            return inputField;
        }

        private static void SetTopLeftRect(RectTransform rect, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static void SetStretchRect(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }
    }
}
#endif
