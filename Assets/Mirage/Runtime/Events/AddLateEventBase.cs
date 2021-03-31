using UnityEngine.Events;

namespace Mirage.Events
{
    public abstract class AddLateEventBase
    {
        protected abstract UnityEventBase baseEvent { get; }
        protected bool hasInvoked { get; private set; }

        protected void MarkInvoked()
        {
            hasInvoked = true;
        }

        /// <summary>
        /// Resets event, removing all listens and allowing it to be invoked again
        /// </summary>
        public void Reset()
        {
            hasInvoked = false;
            baseEvent.RemoveAllListeners();
        }
    }
}
