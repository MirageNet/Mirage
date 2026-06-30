using Mirage;
using UnityEngine;

public class MyBehaviour : MonoBehaviour
{
    public NetworkServer Server;
    public NetworkClient Client;
    public NetworkIdentity Identity;

    public void Update()
    {
        if (Server.Active) {}
        if (Client.Active) {}
        if (Identity.IsSpawned) {}
    }
}
