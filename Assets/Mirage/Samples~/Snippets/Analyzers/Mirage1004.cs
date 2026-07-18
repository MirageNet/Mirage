using System;
using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1004.Triggering
    {
        // CodeEmbed-Start: mirage1004-triggering
        public class Player : NetworkBehaviour
        {
            // Case 1: Hook method does not exist
            [SyncVar(hook = "OnHealthChanged")]
            public int health;

            // Case 2: Parameter types do not match the SyncVar type (double vs float)
            [SyncVar(hook = nameof(OnMatchStartTimeChanged))]
            public double matchStartTime;

            public static void OnMatchStartTimeChanged(float oldTime, float newTime)
            {
            }

            // Case 3: Event hook is not a System.Action delegate
            [SyncVar(hook = nameof(OnScoreChanged))]
            public int score;

            public delegate void ScoreChangedDelegate(int oldScore, int newScore);
            public static event ScoreChangedDelegate OnScoreChanged;

            // Case 4: Multiple matching overloads exist with automatic resolution
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
            // Case 1: Valid instance method with matching parameters
            [SyncVar(hook = nameof(OnHealthChanged))]
            public int health;

            public void OnHealthChanged(int oldHealth, int newHealth)
            {
            }

            // Case 2: Parameters match the SyncVar type exactly
            [SyncVar(hook = nameof(OnMatchStartTimeChanged))]
            public double matchStartTime;

            public static void OnMatchStartTimeChanged(double oldTime, double newTime)
            {
            }

            // Case 3: Event uses a System.Action delegate
            [SyncVar(hook = nameof(OnScoreChanged))]
            public int score;

            public static event Action<int, int> OnScoreChanged;

            // Case 4: Explicitly specify hookType to resolve ambiguity
            [SyncVar(hook = nameof(OnGoldChanged), hookType = SyncHookType.MethodWith2Arg)]
            public int gold;

            public void OnGoldChanged() { }
            public void OnGoldChanged(int oldGold, int newGold) { }
        }
        // CodeEmbed-End: mirage1004-resolved
    }
}
