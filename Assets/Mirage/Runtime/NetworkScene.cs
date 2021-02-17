using System.Collections.Generic;
using UnityEngine;

namespace Mirage
{

    [DisallowMultipleComponent]
    public class NetworkScene : MonoBehaviour
    {
        public List<NetworkIdentity> SceneObjects = new List<NetworkIdentity>();
    }
}
