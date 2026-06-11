using Mirage;

public class AddChild : NetworkBehaviour
{
    public NetworkIdentity prefab;
    [SyncVar]
    private NetworkIdentity clone { get; set; }

    private void Awake()
    {
        Identity.OnStartServer.AddListener(ServerStart);
        Identity.OnStartClient.AddListener(ClientStart);
    }

    private void ClientStart()
    {
        clone.transform.parent = transform;
    }

    private void ServerStart()
    {
        var c = Instantiate(prefab, transform);
        ServerObjectManager.Spawn(c);
        clone = c;
    }
}
