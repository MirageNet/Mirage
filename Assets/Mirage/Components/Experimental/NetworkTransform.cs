using UnityEngine;

namespace Mirage.Experimental
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/Experimental/NetworkTransformExperimental")]
    [HelpURL("https://miragenet.github.io/Mirage/docs/components/network-transform")]
    public class NetworkTransform : NetworkTransformBase
    {
        protected override Transform TargetTransform => transform;
    }
}
