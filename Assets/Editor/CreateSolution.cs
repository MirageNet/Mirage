#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;
using VSCodeEditor;

namespace GitTools
{
    public static class Solution
    {
        public static void Sync()
        {
            var projectGeneration = new ProjectGeneration(Directory.GetParent(Application.dataPath).FullName);
            AssetDatabase.Refresh();
            projectGeneration.GenerateAndWriteSolutionAndProjects();
        }
    }
}
#endif
