using Mirage;
using UnityEngine;
using UnityEngine.UI;

namespace Mirage.Snippets.CommunityGuides
{
    namespace GettingStarted
    {
        // CodeEmbed-Start: quickstart-playerscript-base
        public class PlayerScript : NetworkBehaviour
        {
            private void Awake()
            {
                Identity.OnStartLocalPlayer.AddListener(OnStartLocalPlayer);
            }

            private void OnStartLocalPlayer()
            {
                Camera.main.transform.SetParent(transform);
                Camera.main.transform.localPosition = new Vector3(0, 0, 0);
            }

            private void Update()
            {
                if (!IsLocalPlayer)
                    return;

                float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110.0f;
                float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 4f;

                transform.Rotate(0, moveX, 0);
                transform.Translate(0, 0, moveZ);
            }
        }
        // CodeEmbed-End: quickstart-playerscript-base

        // CodeEmbed-Start: quickstart-startserver
        public class StartServer : MonoBehaviour
        {
            [SerializeField] private NetworkManager networkManager;

            private void Start() 
            {
                if (!networkManager)
                    return;
                
                networkManager.Server.StartServer(networkManager.Client);
            }
        }
        // CodeEmbed-End: quickstart-startserver
    }

    namespace QuickStart
    {
        public class SceneScript : NetworkBehaviour
        {
            public Text canvasStatusText;
            public PlayerScript playerScript;

            [SyncVar(hook = nameof(OnStatusTextChanged))]
            public string statusText { get; set; }

            void OnStatusTextChanged(string _Old, string _New)
            {
                //called from sync var hook, to update info on screen for all players
                canvasStatusText.text = statusText;
            }

            public void ButtonSendMessage()
            {
                if (playerScript != null)  
                {
                    playerScript.CmdSendPlayerMessage();
                }
            }
        }

        public class PlayerScript : NetworkBehaviour
        {
            // CodeEmbed-Start: quickstart-playerscript-names
            public TextMesh playerNameText;
            public GameObject floatingInfo;

            private Material playerMaterialClone;

            [SyncVar(hook = nameof(OnNameChanged))]
            public string playerName { get; set; }

            [SyncVar(hook = nameof(OnColorChanged))]
            public Color playerColor { get; set; } = Color.white;

            [ServerRpc]
            public void CmdSetupPlayer(string _name, Color _col)
            {
                // player info sent to server, then server updates sync vars which handles it on all clients
                playerName = _name;
                playerColor = _col;
            }

            private void Awake()
            {
                Identity.OnStartLocalPlayer.AddListener(OnStartLocalPlayer);
            }

            private void OnStartLocalPlayer()
            {
                Camera.main.transform.SetParent(transform);
                Camera.main.transform.localPosition = new Vector3(0, 0, 0);
                
                floatingInfo.transform.localPosition = new Vector3(0, -0.3f, 0.6f);
                floatingInfo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                string name = "Player" + Random.Range(100, 999);
                Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                CmdSetupPlayer(name, color);
            }

            private void OnNameChanged(string _Old, string _New)
            {
                playerNameText.text = playerName;
            }

            private void OnColorChanged(Color _Old, Color _New)
            {
                playerNameText.color = _New;
                playerMaterialClone = new Material(GetComponent<Renderer>().material);
                playerMaterialClone.color = _New;
                GetComponent<Renderer>().material = playerMaterialClone;
            }

            private void Update()
            {
                if (!IsLocalPlayer)
                {
                    // make non-local players run this
                    floatingInfo.transform.LookAt(Camera.main.transform);
                    return;
                }

                float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110.0f;
                float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 4f;

                transform.Rotate(0, moveX, 0);
                transform.Translate(0, 0, moveZ);
            }
            // CodeEmbed-End: quickstart-playerscript-names

            public void CmdSendPlayerMessage() {}
        }

        public class PlayerScriptPart11 : NetworkBehaviour
        {
            public string playerName { get; set; }
            public Color playerColor { get; set; }

            // CodeEmbed-Start: quickstart-playerscript-part11
            private SceneScript sceneScript;

            void Awake()
            {
                //allow all players to run this
                sceneScript = GameObject.FindObjectOfType<SceneScript>();
                Identity.OnStartLocalPlayer.AddListener(OnStartLocalPlayer);
            }
            [ServerRpc]
            public void CmdSendPlayerMessage()
            {
                if (sceneScript) 
                { 
                    sceneScript.statusText = $"{playerName} says hello {Random.Range(10, 99)}";
                }
            }
            [ServerRpc]
            public void CmdSetupPlayer(string _name, Color _col)
            {
                //player info sent to server, then server updates sync vars which handles it on all clients
                playerName = _name;
                playerColor = _col;
                sceneScript.statusText = $"{playerName} joined.";
            }
            public void OnStartLocalPlayer()
            {
                sceneScript.playerScript = this;
                //. . . . ^ new line to add here
            // CodeEmbed-End: quickstart-playerscript-part11
            }
        }

        // CodeEmbed-Start: quickstart-scenescript
        public class SceneScriptSnippet : NetworkBehaviour
        {
            public Text canvasStatusText;
            public PlayerScript playerScript;

            [SyncVar(hook = nameof(OnStatusTextChanged))]
            public string statusText { get; set; }

            void OnStatusTextChanged(string _Old, string _New)
            {
                //called from sync var hook, to update info on screen for all players
                canvasStatusText.text = statusText;
            }

            public void ButtonSendMessage()
            {
                if (playerScript != null)  
                {
                    playerScript.CmdSendPlayerMessage();
                }
            }
        }
        // CodeEmbed-End: quickstart-scenescript

        public class PlayerScriptWeaponSwitch : NetworkBehaviour
        {
            public GameObject floatingInfo;

            // CodeEmbed-Start: quickstart-playerscript-weaponswitch
            private int selectedWeaponLocal = 1;
            public GameObject[] weaponArray;

            [SyncVar(hook = nameof(OnWeaponChanged))]
            public int activeWeaponSynced { get; set; }

            void OnWeaponChanged(int _Old, int _New)
            {
                // disable old weapon
                // in range and not null
                if (0 < _Old && _Old < weaponArray.Length && weaponArray[_Old] != null)
                {
                    weaponArray[_Old].SetActive(false);
                }
                
                // enable new weapon
                // in range and not null
                if (0 < _New && _New < weaponArray.Length && weaponArray[_New] != null)
                {
                    weaponArray[_New].SetActive(true);
                }
            }

            [ServerRpc]
            public void CmdChangeActiveWeapon(int newIndex)
            {
                activeWeaponSynced = newIndex;
            }

            void Awake() 
            {
                // disable all weapons
                foreach (var item in weaponArray)
                {
                    if (item != null)
                    { 
                        item.SetActive(false); 
                    }
                }
            }
            // CodeEmbed-End: quickstart-playerscript-weaponswitch

            // CodeEmbed-Start: quickstart-playerscript-weaponswitch-update
            void Update()
            {
                if (!IsLocalPlayer)
                {
                    // make non-local players run this
                    floatingInfo.transform.LookAt(Camera.main.transform);
                    return;
                }

                float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110.0f;
                float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 4f;

                transform.Rotate(0, moveX, 0);
                transform.Translate(0, 0, moveZ);

                if (Input.GetButtonDown("Fire2")) //Fire2 is mouse 2nd click and left alt
                {
                    selectedWeaponLocal += 1;

                    if (selectedWeaponLocal > weaponArray.Length) 
                    {
                        selectedWeaponLocal = 1; 
                    }

                    CmdChangeActiveWeapon(selectedWeaponLocal);
                }
            }
            // CodeEmbed-End: quickstart-playerscript-weaponswitch-update
        }
    }
}
