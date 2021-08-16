using System;

namespace Mirage.Serialization
{
    /// <summary>
    /// Tells Weaver to ignore an Extension method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WeaverIgnoreAttribute : Attribute { }

    /// <summary>
    /// Weaver will pick the method with highest Priority if 2 methods are found with same type
    /// <para>
    /// Methods without this attribute are given priority 0.
    /// If 1 method has no attribute, and another has Priority=-1 then the method with no attribute will be picked
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SerializeExtensionAttribute : Attribute
    {
        public int Priority;
    }
}
