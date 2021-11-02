using UnityEngine;

namespace Mirage
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkTransform")]
    [HelpURL("https://miragenet.github.io/Mirage/Articles/Components/NetworkTransform.html")]
    public class NetworkTransform : NetworkTransformBase
    {
        protected override Transform TargetComponent => transform;
    }
}
