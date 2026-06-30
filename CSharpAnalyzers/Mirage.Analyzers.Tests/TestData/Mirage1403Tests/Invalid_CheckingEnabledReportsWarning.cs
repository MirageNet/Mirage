using Mirage;
using UnityEngine;

public class MyBehaviour : MonoBehaviour
{
    public NetworkServer Server;
    public NetworkClient Client;
    public NetworkIdentity Identity;

    public void Update()
    {
        if (Server.{|#0:enabled|}) {}
        if (Client.{|#1:enabled|}) {}
        if (Identity.{|#2:enabled|}) {}
    }
}
