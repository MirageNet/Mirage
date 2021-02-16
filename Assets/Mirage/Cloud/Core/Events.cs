using System;
using Mirage.Cloud.ListServerService;
using UnityEngine.Events;

namespace Mirage.Cloud
{
    [System.Serializable]
    public class ServerListEvent : UnityEvent<ServerCollectionJson> { }

    [System.Serializable]
    public class MatchFoundEvent : UnityEvent<ServerJson> { }
}
