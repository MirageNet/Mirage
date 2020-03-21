using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mirror.Tests
{
    public static class AsyncUtil
    {
        // Unity's nunit does not support async tests
        // so we do this boilerplate to run our async methods
        public static IEnumerator RunAsync(Func<Task> block)
        {
            var task = block();

            while (!task.IsCompleted) { yield return 0; }
            if (task.IsFaulted) { throw task.Exception; }
        }
    }
}
