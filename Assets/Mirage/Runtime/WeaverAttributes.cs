using System;

namespace Mirage
{
    /// <summary>
    /// Tells Weaver to ignore an Extension method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WeaverIgnoreAttribute : Attribute { }

    /// <summary>
    /// Weaver will pick the method with highest Priority if 2 methods are found with same type
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SerializeExtensionAttribute : Attribute
    {
        public int Priority { get; set; }
    }
}
