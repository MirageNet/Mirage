using UnityEngine;

namespace Mirage.Experimental
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/Experimental/NetworkTransformExperimental")]
    [HelpURL("https://mirror-networking.com/docs/Components/NetworkTransform.html")]
    public class NetworkTransform : NetworkTransformBase
    {
        protected override Transform TargetTransform => transform;
    }
}
