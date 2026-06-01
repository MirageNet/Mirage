using System;
using System.Collections;
using UnityEngine;
using Mirage.Serialization;

namespace Mirage.Snippets.Serialization.GameObjectLookup
{
    public class GameObjectLookup : NetworkBehaviour
    {
        // CodeEmbed-Start: game-object-lookup
        public GameObject target;

        [SyncVar(hook = nameof(OnTargetChanged))]
        public uint targetID { get; set; }

        void OnTargetChanged(uint _, uint newValue)
        {
            if (NetworkIdentity.World.Spawned.TryGetValue(targetID, out NetworkIdentity identity))
                target = identity.gameObject;
            else
                StartCoroutine(SetTarget());
        }

        IEnumerator SetTarget()
        {
            while (target == null)
            {
                yield return null;
                if (NetworkIdentity.World.SpawnedObjects.TryGetValue(targetID, out NetworkIdentity identity))
                    target = identity.gameObject;
            }
        }
        // CodeEmbed-End: game-object-lookup
    }
}

namespace Mirage.Snippets.Serialization.DateTimeSerializer
{
    // CodeEmbed-Start: datetime-serializer
    public static class DateTimeReaderWriter
    {
        public static void WriteDateTime(this NetworkWriter writer, DateTime dateTime)
        {
            writer.WriteInt64(dateTime.Ticks);
        }
       
        public static DateTime ReadDateTime(this NetworkReader reader)
        {
            return new DateTime(reader.ReadInt64());
        }
    }
    // CodeEmbed-End: datetime-serializer
}

namespace Mirage.Snippets.Serialization.PolymorphicEquip
{
    // CodeEmbed-Start: polymorphic-equip
    class Item 
    {
        public string name;
    }

    class Weapon : Item
    {
        public int hitPoints;
    }

    class Armor : Item
    {
        public int hitPoints;
        public int level;
    }

    class Player : NetworkBehaviour
    {
        [ServerRpc]
        void ServerRpcEquip(Item item)
        {
            // IMPORTANT: this does not work. Mirage will pass you an object of type item
            // even if you pass a weapon or an armor.
            if (item is Weapon weapon)
            {
                // The item is a weapon, 
                // maybe you need to equip it in the hand
            }
            else if (item is Armor armor)
            {
                // you might want to equip armor in the body
            }
        }

        [ServerRpc]
        void ServerEquipArmor(Armor armor)
        {
            // IMPORTANT: this does not work either, you will receive an armor, but 
            // the armor will not have a valid Item.name, even if you passed an armor with name
        }
    }
    // CodeEmbed-End: polymorphic-equip
}

namespace Mirage.Snippets.Serialization.PolymorphicSerializer
{
    using PolymorphicEquip;

    // CodeEmbed-Start: polymorphic-serializer
    public static class ItemSerializer 
    {
        const byte WEAPON = 1;
        const byte ARMOR = 2;

        public static void WriteItem(this NetworkWriter writer, Item item)
        {
            if (item is Weapon weapon)
            {
                writer.WriteByte(WEAPON);
                writer.WriteString(weapon.name);
                writer.WritePackedInt32(weapon.hitPoints);
            }
            else if (item is Armor armor)
            {
                writer.WriteByte(ARMOR);
                writer.WriteString(armor.name);
                writer.WritePackedInt32(armor.hitPoints);
                writer.WritePackedInt32(armor.level);
            }
        }

        public static Item ReadItem(this NetworkReader reader)
        {
            byte type = reader.ReadByte();
            switch(type)
            {
                case WEAPON:
                    return new Weapon
                    {
                        name = reader.ReadString(),
                        hitPoints = reader.ReadPackedInt32()
                    };
                case ARMOR:
                    return new Armor
                    {
                        name = reader.ReadString(),
                        hitPoints = reader.ReadPackedInt32(),
                        level = reader.ReadPackedInt32()
                    };
                default:
                    throw new Exception($"Invalid weapon type {type}");
            }
        }
    }
    // CodeEmbed-End: polymorphic-serializer
}

namespace Mirage.Snippets.Serialization.ScriptableObjectSerializer
{
    // CodeEmbed-Start: scriptable-object-serializer
    [CreateAssetMenu(fileName = "New Armor", menuName = "Armor Data")]
    class Armor : ScriptableObject
    {
        public int Hitpoints;
        public int Weight;
        public string Description;
        public Texture2D Icon;
        // ...
    }

    public static class ArmorSerializer 
    {
        public static void WriteArmor(this NetworkWriter writer, Armor armor)
        {
           // No need to serialize the data, just the name of the armor.
           writer.WriteString(armor.name);
        }

        public static Armor ReadArmor(this NetworkReader reader)
        {
            // Load the same armor by name. The data will come from the asset in Resources folder.
            return Resources.Load<Armor>(reader.ReadString());
        }
    }
    // CodeEmbed-End: scriptable-object-serializer
}
