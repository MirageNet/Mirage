using Mirage;
using UnityEngine;

namespace Example.CustomCharacter
{
    [NetworkMessage]
    public struct CreateMMOCharacterMessage
    {
        public Race race;
        public string name;
        public Color hairColor;
        public Color eyeColor;
    }

    public enum Race
    {
        Human,
        Elvish,
        Dwarvish,
    }
}
