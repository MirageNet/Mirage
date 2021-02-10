using UnityEngine;

namespace Mirage
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkTransform")]
    [HelpURL("https://mirror-networking.com/docs/Components/NetworkTransform.html")]
    public class NetworkTransform : NetworkTransformBase
    {
        protected override Transform TargetComponent => transform;
    }
}
