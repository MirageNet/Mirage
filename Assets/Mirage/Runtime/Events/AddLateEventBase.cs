namespace Mirage.Events
{
    public abstract class AddLateEventBase
    {
        protected bool hasInvoked { get; private set; }

        protected void MarkInvoked()
        {
            hasInvoked = true;
        }

        /// <summary>
        /// Resets invoked flag, meaning new handles wont be invoked until invoke is called again
        /// <para>Reset does not remove listeners</para>
        /// </summary>
        public void Reset()
        {
            hasInvoked = false;
        }
    }
}
