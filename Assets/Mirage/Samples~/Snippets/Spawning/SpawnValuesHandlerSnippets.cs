using UnityEngine;

namespace Mirage.Snippets.Spawning
{
    // CodeEmbed-Start: floating-origin-spawn-values-handler
    /// <summary>
    /// Custom spawn values handler that translates positions between large world global coordinates
    /// and local Unity coordinates (floating origin).
    /// </summary>
    public class FloatingOriginSpawnValuesHandler : ISpawnValuesHandler
    {
        // Reference to your floating origin system
        public Vector3 CurrentOriginOffset;

        public SpawnValues CreateSpawnValues(NetworkIdentity identity)
        {
            // Start with default settings (position, rotation, scale, name, active state)
            var values = DefaultSpawnValuesHandler.Instance.CreateSpawnValues(identity);

            if (values.Position.HasValue)
            {
                // Convert Unity local position to global world coordinates for network transmission
                values.Position = values.Position.Value + CurrentOriginOffset;
            }

            return values;
        }

        public (Vector3 pos, Quaternion rot) GetPrefabPosition(NetworkIdentity prefab, SpawnValues values)
        {
            // Convert network global coordinates back to client's local Unity position relative to its current floating origin
            var pos = values.Position.HasValue ? values.Position.Value - CurrentOriginOffset : prefab.transform.position;
            var rot = values.Rotation ?? prefab.transform.rotation;
            return (pos, rot);
        }

        public void ApplySpawnValues(NetworkIdentity identity, SpawnValues values)
        {
            // Adjust position before applying
            var adjustedValues = values;
            if (adjustedValues.Position.HasValue)
            {
                adjustedValues.Position = adjustedValues.Position.Value - CurrentOriginOffset;
            }

            DefaultSpawnValuesHandler.Instance.ApplySpawnValues(identity, adjustedValues);
        }
    }
    // CodeEmbed-End: floating-origin-spawn-values-handler

    // CodeEmbed-Start: assign-spawn-values-handler
    public class SetupFloatingOriginSpawning : MonoBehaviour
    {
        public ServerObjectManager ServerObjectManager;
        public ClientObjectManager ClientObjectManager;

        public FloatingOriginSpawnValuesHandler OriginHandler = new FloatingOriginSpawnValuesHandler();

        private void Awake()
        {
            // Assign handler on server and client
            ServerObjectManager.SpawnValuesHandler = OriginHandler;
            ClientObjectManager.SpawnValuesHandler = OriginHandler;
        }
    }
    // CodeEmbed-End: assign-spawn-values-handler
}
