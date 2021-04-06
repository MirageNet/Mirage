using System;
using UnityEngine.Events;

namespace Mirage.Events
{
    [Serializable]
    public class BoolUnityEvent : UnityEvent<bool> { }

    [Serializable]
    public class BoolAddLateEvent : AddLateEvent<bool, BoolUnityEvent> { }
}
