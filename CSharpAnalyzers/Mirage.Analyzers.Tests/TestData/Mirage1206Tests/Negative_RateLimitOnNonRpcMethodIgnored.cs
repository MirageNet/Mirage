using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [RateLimit(Interval = 0f, Refill = 0, MaxTokens = 0)]
    public void LocalFire()
    {
    }
}
