namespace Mirage
{
    internal class Incrementer
    {
        uint next;

        public Incrementer(uint initial = 1)
        {
            next = initial;
        }

        public uint GetNext() => next++;

        public void Reset(uint initial = 1)
        {
            next = initial;
        }
    }
}
