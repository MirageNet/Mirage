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

        [Command]
        public void CmdLearnSkill(string skillName)
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
        public override void OnStartClient()
        {
            buffs.Callback += OnBuffsChanged;
        }

        private void OnBuffsChanged(SyncSetBuffs.Operation op, string buff)
        {
            switch (op) 
            {
                case SyncSetBuffs.Operation.OP_ADD:
                    // we added a buff, draw an icon on the character
                    break;
                case SyncSetBuffs.Operation.OP_CLEAR:
                    // clear all buffs from the character
                    break;
                case SyncSetBuffs.Operation.OP_REMOVE:
                    // We removed a buff from the character
                    break;
            }
        }
    }
    // CodeEmbed-End: SyncSortedSetCallbackExample
}
