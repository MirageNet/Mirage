using System;
using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1004.Triggering
    {
        // CodeEmbed-Start: mirage1004-triggering
        public class Player : NetworkBehaviour
        {
            // Case 1: Hook method 'OnHealthChanged' does not exist in the class
            [SyncVar(hook = "OnHealthChanged")]
            public int health;

            // Case 2: Hook method parameter types do not match the SyncVar's type (int vs float)
            [SyncVar(hook = nameof(OnManaChanged))]
            public int mana;

            public void OnManaChanged(float oldMana, float newMana)
            {
                // Wrong parameter types
            }

            // Case 3: A static event hook is declared (unsupported, causes invalid IL in weaver)
            [SyncVar(hook = nameof(OnScoreChanged))]
            public int score;

            public static event Action<int, int> OnScoreChanged;

            // Case 4: Multiple matching overloads exist under automatic hook type resolving
            [SyncVar(hook = nameof(OnGoldChanged))]
            public int gold;

            public void OnGoldChanged() { }
            public void OnGoldChanged(int oldGold, int newGold) { }
        }
        // CodeEmbed-End: mirage1004-triggering
    }

    namespace M1004.Resolved
    {
        // CodeEmbed-Start: mirage1004-resolved
        public class Player : NetworkBehaviour
        {
            // Case 1: Hook method is an instance method with 2 parameters matching the type exactly
            [SyncVar(hook = nameof(OnHealthChanged))]
            public int health;

            public void OnHealthChanged(int oldHealth, int newHealth)
            {
                // Correct instance method hook
            }

            // Case 2: Hook method is a static method (fully supported by Mirage Weaver)
            [SyncVar(hook = nameof(OnManaChanged))]
            public int mana;

            public static void OnManaChanged(int oldMana, int newMana)
            {
                // Correct static method hook
            }

            // Case 3: Hook can be an instance event of type System.Action
            [SyncVar(hook = nameof(OnScoreChanged))]
            public int score;

            public event Action<int, int> OnScoreChanged;

            // Case 4: Multiple overloads resolved by explicitly specifying hookType
            [SyncVar(hook = nameof(OnGoldChanged), hookType = SyncHookType.MethodWith2Arg)]
            public int gold;

            public void OnGoldChanged() { }
            public void OnGoldChanged(int oldGold, int newGold) { }
        }
        // CodeEmbed-End: mirage1004-resolved
    }
}
