namespace Mirage
{
    public static class Version
    {
        public static readonly string Current = typeof(NetworkIdentity).Assembly.GetName().Version.ToString();
    }

}
