using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [SyncVar]
    public static int {|#0:MySyncVar|} { get; set; }
}
