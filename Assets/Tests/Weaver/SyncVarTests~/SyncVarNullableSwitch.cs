using Mirage;

namespace SyncVarTests.SyncVarNullableSwitch
{
    class SyncVarNullableSwitch : NetworkBehaviour
    {
        [SyncVar]
        int? nullableInt;

        [SyncVar]
        bool? nullableBool;

        // Tests switch statements branching directly into nullable SyncVar assignment to null.
        // This ensures the switch jump targets to the ldflda/initobj instructions are not broken.
        public void SetNullableIntSwitch(int value)
        {
            switch (value)
            {
                case 1:
                    // Assigned to null inside a switch case.
                    nullableInt = null;
                    break;
                case 2:
                    nullableInt = 10;
                    break;
                default:
                    // Assigned to default inside a switch case.
                    nullableInt = default;
                    break;
            }
        }

        // Tests string switch statements branching into nullable SyncVar assignment.
        // String switches compile to dictionary lookups or nested ifs, which test different branch IL.
        public void SetNullableBoolSwitch(string value)
        {
            switch (value)
            {
                case "null":
                    // Assigned to null inside a string switch case.
                    nullableBool = null;
                    break;
                case "true":
                    nullableBool = true;
                    break;
                default:
                    // Assigned to default inside a string switch case.
                    nullableBool = default;
                    break;
            }
        }

        // Tests loop structures where the loop body starts with a nullable SyncVar assignment to null.
        // The loop back-branch (or conditional jump) will target the start of the loop body.
        public void LoopAssign(int count)
        {
            for (int i = 0; i < count; i++)
            {
                nullableInt = null;
            }
        }

        // Tests while loops targeting nullable SyncVar assignment to default.
        // The branch conditional jump targets the loop entry/body.
        public void WhileLoopAssign(bool condition)
        {
            while (condition)
            {
                nullableBool = default;
            }
        }
    }
}
