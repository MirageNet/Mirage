using System.Collections.Generic;
using Mirage;
using UnityEngine;

namespace GeneratedReaderWriter.GivesErrorForInvalidListType
{
    public class GivesErrorForInvalidListType : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcDoSomething(List<MonoBehaviour> data)
        {
            // empty
        }
    }
}
