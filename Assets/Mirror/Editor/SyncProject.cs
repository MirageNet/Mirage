using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace Mirror
{
    public static class SyncSolution
    {
        // generates csproj and .sln files for the current project
        public static void Sync()
        {
            var editor = Type.GetType("UnityEditor.SyncVS, UnityEditor");
            var SyncSolution = editor.GetMethod("SyncSolution", BindingFlags.Public | BindingFlags.Static);
            SyncSolution.Invoke(null, null);
            Debug.Log("Solution synced!");
        }
    }
}
