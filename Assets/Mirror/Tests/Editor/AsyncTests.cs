using System;
using System.Collections;
using System.Threading.Tasks;

namespace Mirror.Tests
{
    public class AsyncTests
    {
        // Unity's nunit does not support async tests
        // so we do this boilerplate to run our async methods
        public static IEnumerator RunAsync(Func<Task> block)
        {
            var task = Task.Run(block);

            while (!task.IsCompleted) { yield return null; }
            if (task.IsFaulted) { throw task.Exception; }
        }

    }
}