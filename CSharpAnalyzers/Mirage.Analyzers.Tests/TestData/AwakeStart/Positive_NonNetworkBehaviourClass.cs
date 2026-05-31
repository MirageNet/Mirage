public class NonNetworkClass
{
    public bool IsServer { get; set; }
    public int Health { get; set; }

    private void Awake()
    {
        if (IsServer)
        {
            Health = 100;
        }
    }
}
