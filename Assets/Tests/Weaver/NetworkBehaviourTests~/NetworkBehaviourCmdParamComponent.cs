using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourCmdParamComponent
{
    class NetworkBehaviourCmdParamComponent : NetworkBehaviour
    {
        public class ComponentClass : UnityEngine.Component
        {
            int monkeys = 12;
        }

        [ServerRpc]
        public void CmdCantHaveParamComponent(ComponentClass monkeyComp) { }
    }
}
