using UnityEngine;
using Mirage;
using Mirage.Collections;

namespace Mirage.Snippets.Sync.HashSets.Basic
{
    // CodeEmbed-Start: SyncHashSetBasicExample
    [System.Serializable]
    public class SyncSkillSet : SyncHashSet<string> {}

    public class Player : NetworkBehaviour
    {
        [SerializeField]
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
    // CodeEmbed-End: SyncHashSetBasicExample
}

namespace Mirage.Snippets.Sync.HashSets.Callbacks
{
    // CodeEmbed-Start: SyncHashSetCallbackExample
    [System.Serializable]
    public class SyncSetBuffs : SyncHashSet<string> {}

    public class Player : NetworkBehaviour
    {
        [SerializeField]
        public readonly SyncSetBuffs buffs = new SyncSetBuffs();

        // this will add the delegate on the client.
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
    // CodeEmbed-End: SyncHashSetCallbackExample
}
