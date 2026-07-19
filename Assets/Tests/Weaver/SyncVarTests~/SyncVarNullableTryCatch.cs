using System;
using Mirage;

namespace SyncVarTests.SyncVarNullableTryCatch
{
    class SyncVarNullableTryCatch : NetworkBehaviour
    {
        [SyncVar]
        int? nullableInt;

        [SyncVar]
        float? nullableFloat;

        // Tests try-catch-finally blocks where nullable SyncVar assignments are placed in
        // the try, catch, and finally blocks, verifying that exception handler boundaries
        // targeting these instructions remain valid when mutated.
        public void TryCatchFinallyMethod()
        {
            try
            {
                // Assigned to null inside the try block.
                nullableInt = null;
            }
            catch (Exception)
            {
                // Assigned to default inside the catch block.
                nullableInt = default;
            }
            finally
            {
                // Assigned to null inside the finally block.
                nullableInt = null;
            }
        }

        // Tests exception handling when conditional branching is used inside try-catch.
        // The catch block starts with the nullable assignment.
        public void TryCatchFloatMethod(bool throwException)
        {
            try
            {
                if (throwException)
                {
                    throw new Exception("Test Exception");
                }
            }
            catch (Exception)
            {
                // Assigned inside catch block targeting the exception handler start.
                nullableFloat = null;
            }
        }
    }
}
