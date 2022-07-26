using UnityEngine;

namespace Mirage
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkTransform")]
    [HelpURL("https://miragenet.github.io/Mirage/docs/components/network-transform")]
    public class NetworkTransform : NetworkTransformBase
    {
        protected override Transform TargetComponent => transform;
    }
}
