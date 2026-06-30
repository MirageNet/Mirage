using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    public int NonSyncField;
    public int NonSyncProp { get; set; }

    private void Awake()
    {
        NonSyncField = 5;
        NonSyncProp = 10;
    }
}
