using System;
using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1005.Triggering
    {
        // CodeEmbed-Start: mirage1005-triggering
        public class Player : NetworkBehaviour
        {
            // Case 1: Hook method 'OnHealthChanged' does not exist in the class
            [SyncVar(hook = "OnHealthChanged")]
            public int health { get; set; }

            // Case 2: Hook method parameter types do not match the SyncVar's type (int vs float)
            [SyncVar(hook = nameof(OnManaChanged))]
            public int mana { get; set; }

            public void OnManaChanged(float oldMana, float newMana)
            {
                // Wrong parameter types
            }

            // Case 3: A static event hook is declared (unsupported, causes invalid IL in weaver)
            [SyncVar(hook = nameof(OnScoreChanged))]
            public int score { get; set; }

            public static event Action<int, int> OnScoreChanged;

            // Case 4: Multiple matching overloads exist under automatic hook type resolving
            [SyncVar(hook = nameof(OnGoldChanged))]
            public int gold { get; set; }

            public void OnGoldChanged() { }
            public void OnGoldChanged(int oldGold, int newGold) { }
        }
        // CodeEmbed-End: mirage1005-triggering
    }

    namespace M1005.Resolved
    {
        // CodeEmbed-Start: mirage1005-resolved
        public class Player : NetworkBehaviour
        {
            // Case 1: Hook method is an instance method with 2 parameters matching the type exactly
            [SyncVar(hook = nameof(OnHealthChanged))]
            public int health { get; set; }

            public void OnHealthChanged(int oldHealth, int newHealth)
            {
                // Correct instance method hook
            }

            // Case 2: Hook method is a static method (fully supported by Mirage Weaver)
            [SyncVar(hook = nameof(OnManaChanged))]
            public int mana { get; set; }

            public static void OnManaChanged(int oldMana, int newMana)
            {
                // Correct static method hook
            }

            // Case 3: Hook can be an instance event of type System.Action
            [SyncVar(hook = nameof(OnScoreChanged))]
            public int score { get; set; }

            public event Action<int, int> OnScoreChanged;

            // Case 4: Multiple overloads resolved by explicitly specifying hookType
            [SyncVar(hook = nameof(OnGoldChanged), hookType = SyncHookType.MethodWith2Arg)]
            public int gold { get; set; }

            public void OnGoldChanged() { }
            public void OnGoldChanged(int oldGold, int newGold) { }
        }
        // CodeEmbed-End: mirage1005-resolved
    }
}
