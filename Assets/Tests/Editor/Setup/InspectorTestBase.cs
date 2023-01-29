using UnityEditor;
using UnityEngine;

namespace Mirage.Tests
{
    public abstract class InspectorTestBase : TestBase
    {
        /// <summary>
        /// Creates an editor for the target object, then destroys it in teardown.
        /// </summary>
        protected T CreateEditor<T>(Object target) where T : Editor
        {
            var editor = Editor.CreateEditor(target);
            toDestroy.Add(editor);
            return (T) editor;
        }
    }
}
