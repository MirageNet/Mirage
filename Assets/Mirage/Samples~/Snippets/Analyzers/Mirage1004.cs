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

            // Case 2: Hook method parameter types do not match the SyncVar's type (double vs float)
            // Note: Hook method can be static or instance
            [SyncVar(hook = nameof(OnMatchStartTimeChanged))]
            public double matchStartTime;

            public static void OnMatchStartTimeChanged(float oldTime, float newTime)
            {
                // Wrong parameter types
            }

            // Case 3: An event hook is declared with an invalid delegate type (not System.Action)
            // Note: Hook event can be static or instance
            [SyncVar(hook = nameof(OnScoreChanged))]
            public int score;

            public delegate void ScoreChangedDelegate(int oldScore, int newScore);
            public static event ScoreChangedDelegate OnScoreChanged;

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

            // Case 2: Hook method parameter types match the SyncVar's type exactly
            // Note: Hook method can be static or instance
            [SyncVar(hook = nameof(OnMatchStartTimeChanged))]
            public double matchStartTime;

            public static void OnMatchStartTimeChanged(double oldTime, double newTime)
            {
                // Correct static method hook
            }

            // Case 3: Hook event is declared using a System.Action delegate
            // Note: Hook event can be static or instance
            [SyncVar(hook = nameof(OnScoreChanged))]
            public int score;

            public static event Action<int, int> OnScoreChanged;

            // Case 4: Multiple overloads resolved by explicitly specifying hookType
            [SyncVar(hook = nameof(OnGoldChanged), hookType = SyncHookType.MethodWith2Arg)]
            public int gold;

            public void OnGoldChanged() { }
            public void OnGoldChanged(int oldGold, int newGold) { }
        }
        // CodeEmbed-End: mirage1004-resolved
    }
}
