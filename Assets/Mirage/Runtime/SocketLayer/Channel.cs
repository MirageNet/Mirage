namespace Mirage
{
    public static class Channel
    {
        // 2 well known channels
        // transports can implement other channels
        // to expose their features
        public const int Reliable = 0;
        public const int Unreliable = 1;
    }
}
