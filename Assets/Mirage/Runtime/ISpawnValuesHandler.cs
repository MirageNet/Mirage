using UnityEngine;

namespace Mirage
{
    public interface ISpawnValuesHandler
    {
        /// <summary>
        /// Server-side: Generates the <see cref="SpawnValues"/> to be sent across the network for this identity.
        /// </summary>
        SpawnValues CreateSpawnValues(NetworkIdentity identity);

        /// <summary>
        /// Client-side: Returns the position and rotation used when instantiating a prefab.
        /// </summary>
        (Vector3 pos, Quaternion rot) GetPrefabPosition(NetworkIdentity prefab, SpawnValues values);

        /// <summary>
        /// Client-side: Applies received spawn values (position, rotation, scale, name, active) to the identity.
        /// </summary>
        void ApplySpawnValues(NetworkIdentity identity, SpawnValues values);
    }

    public sealed class DefaultSpawnValuesHandler : ISpawnValuesHandler
    {
        public static readonly DefaultSpawnValuesHandler Instance = new DefaultSpawnValuesHandler();

        public SpawnValues CreateSpawnValues(NetworkIdentity identity)
        {
            var settings = identity.SpawnSettings;
            SpawnValues values = default;

            // values in msg are nullable, so only set those enabled by the identity's settings
            if (settings.SendPosition) 
                values.Position = identity.transform.localPosition;

            if (settings.SendRotation) 
                values.Rotation = identity.transform.localRotation;

            if (settings.SendScale) 
                values.Scale = identity.transform.localScale;

            if (settings.SendName) 
                values.Name = identity.name;

            switch (settings.SendActive)
            {
                case SyncActiveOption.SyncWithServer:
                    values.SelfActive = identity.gameObject.activeSelf;
                    break;
                case SyncActiveOption.ForceEnable:
                    values.SelfActive = true;
                    break;
            }

            return values;
        }

        public (Vector3 pos, Quaternion rot) GetPrefabPosition(NetworkIdentity prefab, SpawnValues values)
        {
            var pos = values.Position ?? prefab.transform.position;
            var rot = values.Rotation ?? prefab.transform.rotation;
            return (pos, rot);
        }

        public void ApplySpawnValues(NetworkIdentity identity, SpawnValues values)
        {
            var transform = identity.transform;
            if (values.Position.HasValue) 
                transform.localPosition = values.Position.Value;

            if (values.Rotation.HasValue) 
                transform.localRotation = values.Rotation.Value;

            if (values.Scale.HasValue) 
                transform.localScale = values.Scale.Value;

            if (!string.IsNullOrEmpty(values.Name)) 
                identity.gameObject.name = values.Name;

            if (values.SelfActive.HasValue) 
                identity.gameObject.SetActive(values.SelfActive.Value);
        }
    }
}
