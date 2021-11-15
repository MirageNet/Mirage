using System.Collections;
using Mirage;
using Mirage.SocketLayer;
using UnityEditor;
using UnityEngine;

public class MultiClients : MonoBehaviour
{
    public int ClientCount = 50;
    private NetworkServer _server;

    const string MonsterPath = "Assets/Tests/Performance/Runtime/InterestManagement/InterestManagement/Prefabs/Enemy.prefab";
    const string PlayerPath = "Assets/Tests/Performance/Runtime/InterestManagement/InterestManagement/Prefabs/Player.prefab";
    private NetworkIdentity PlayerPrefab;
    private NetworkIdentity MonsterPrefab;
    [SerializeField] private Transform _plane;
    private float _planeX, _planeZ;

    private void Awake()
    {
        MonsterPrefab = AssetDatabase.LoadAssetAtPath<NetworkIdentity>(MonsterPath);
        PlayerPrefab = AssetDatabase.LoadAssetAtPath<NetworkIdentity>(PlayerPath);

        Mesh mesh = _plane.GetComponent<MeshFilter>().mesh;

        _planeX = (mesh.bounds.size.x / 2) * _plane.localScale.x;
        _planeZ = (mesh.bounds.size.z / 2) * _plane.localScale.z;

        _server = FindObjectOfType<NetworkServer>();
        _server.Started.AddListener(OnServerStarted);
        _server.MaxConnections = ClientCount + 1;
        _server.StartServer();
    }

    private void OnServerStarted()
    {
        // connect from a bunch of clients
        for (int i = 0; i < ClientCount; i++)
            StartClient(i, _server.GetComponent<SocketFactory>());
    }

    private void StartClient(int i, SocketFactory socketFactory)
    {
        float xRand = Random.Range(-_planeX, _planeX);
        float zRand = Random.Range(-_planeZ, _planeZ);

        var clientGo = new GameObject($"Client {i}", typeof(NetworkClient), typeof(ClientObjectManager));

        clientGo.transform.position = new Vector3(xRand, 1, zRand);
        clientGo.SetActive(false);
        NetworkClient client = clientGo.GetComponent<NetworkClient>();
        ClientObjectManager objectManager = clientGo.GetComponent<ClientObjectManager>();
        objectManager.Client = client;
        objectManager.Start();

        client.SocketFactory = socketFactory;

        CharacterSpawner spawner = clientGo.AddComponent<CharacterSpawner>();
        spawner.Client = client;
        spawner.ClientObjectManager = objectManager;
        spawner.PlayerPrefab = PlayerPrefab;

        objectManager.RegisterPrefab(MonsterPrefab);
        objectManager.RegisterPrefab(PlayerPrefab);

        clientGo.SetActive(true);
        client.Connect("localhost");
    }
}
