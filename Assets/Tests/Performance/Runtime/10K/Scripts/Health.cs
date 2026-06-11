namespace Mirage.Examples
{
    public class Health : NetworkBehaviour
    {
        [SyncVar] public int health { get; set; } = 10;

        [Server(error = false)]
        public void Update()
        {
            health = (health + 1) % 10;
        }
    }
}
