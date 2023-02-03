using Mirage;
using UnityEngine;

public class NetworkPrefabGuiTest : MonoBehaviour
{
    [NetworkedPrefab] public NetworkIdentity Prefab1;
    [NetworkedPrefab] public GameObject Prefab2;
    [NetworkedPrefab] public NetworkAnimator Prefab3;
    [NetworkedPrefab] public GameObject NoIdentity;
    [NetworkedPrefab] public GameObject SceneObject;
    [NetworkedPrefab] public GameObject NullObject;

    [NetworkedPrefab] public int NotAReference;
    [NetworkedPrefab] public ScriptableObject NotAPrefab;
}
