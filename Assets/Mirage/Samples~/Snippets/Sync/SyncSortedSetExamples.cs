using UnityEngine;
using Mirage;
using Mirage.Collections;

namespace Mirage.Snippets.Sync.SortedSets.Basic
{
    // CodeEmbed-Start: SyncSortedSetBasicExample
    class Player : NetworkBehaviour 
    {
        class SyncSkillSet : SyncSortedSet<string> {}

        readonly SyncSkillSet skills = new SyncSkillSet();

        int skillPoints = 10;

        [ServerRpc]
        public void LearnSkill(string skillName)
        {
            if (skillPoints > 1)
            {
                skillPoints--;

                skills.Add(skillName);
            }
        }
    }
    // CodeEmbed-End: SyncSortedSetBasicExample
}

namespace Mirage.Snippets.Sync.SortedSets.Callbacks
{
    // CodeEmbed-Start: SyncSortedSetCallbackExample
    public class Player : NetworkBehaviour
    {
        private class SyncSetBuffs : SyncSortedSet<string> {}

        private readonly SyncSetBuffs buffs = new SyncSetBuffs();

        // This will add the delegate on the client.
        // Use OnStartServer instead if you want it on the server
        private void Awake()
        {
            Identity.OnStartClient.AddListener(OnStartClient);
        }

        private void OnStartClient()
        {
            buffs.OnAdd += OnBuffAdded;
            buffs.OnRemove += OnBuffRemoved;
            buffs.OnClear += OnBuffsCleared;
        }

        private void OnBuffAdded(string buff)
        {
            // we added a buff, draw an icon on the character
        }

        private void OnBuffRemoved(string buff)
        {
            // We removed a buff from the character
        }

        private void OnBuffsCleared()
        {
            // clear all buffs from the character
        }
    }
    // CodeEmbed-End: SyncSortedSetCallbackExample
}
