using System;

namespace Mirage.Collections
{
    public static class SyncObjectUtils
    {
        public static void ThrowIfReadOnly(bool isReadOnly)
        {
            if (isReadOnly)
            {
                throw new InvalidOperationException("SyncObject is marked as ReadOnly. Check SyncDirection and make sure you can set values on this instance. By default you can only add items on server.");
            }
        }
    }
}
