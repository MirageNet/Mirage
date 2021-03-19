namespace Mirage
{
    /// <summary>
    /// Incremnts Uint each time <see cref="Next"/> is called.
    /// <para>
    /// <see cref="Reset(uint)"/> can be used to set the next value
    /// </para>
    /// </summary>
    /// <remarks>
    /// Incrementer will not wrap back to 0, instead it will throw <see cref="System.OverflowException"/>
    /// </remarks>
    internal class Incrementer
    {
        uint next;

        public Incrementer(uint initial = 1)
        {
            next = initial;
        }

        public uint Next()
        {
            checked
            {
                return next++;
            }
        }

        public void Reset(uint initial = 1)
        {
            next = initial;
        }
    }
}
