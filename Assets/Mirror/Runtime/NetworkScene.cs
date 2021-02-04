using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{

    [DisallowMultipleComponent]
    public class NetworkScene : MonoBehaviour
    {
        public List<NetworkIdentity> SceneObjects;
    }
}
