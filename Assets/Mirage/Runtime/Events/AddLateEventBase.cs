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
        /// Resets invoked flag, meaning new handles wont be invoked untill invoke is called again
        /// <para>Reset does not remove listeners</para>
        /// </summary>
        public void Reset()
        {
            hasInvoked = false;
        }

        /// <summary>
        /// Remove all non-persisent (ie created from script) listeners from the event.
        /// </summary>
        public void RemoveAllListeners()
        {
            baseEvent.RemoveAllListeners();
        }
    }
}
