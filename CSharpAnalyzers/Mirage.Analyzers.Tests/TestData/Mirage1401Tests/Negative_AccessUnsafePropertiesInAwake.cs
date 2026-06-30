using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    private void MyServerRpc() {}

    [ClientRpc]
    private void MyClientRpc() {}

    [Server]
    private void MyServerMethod() {}

    [Client]
    private void MyClientMethod() {}

    [HasAuthority]
    private void MyHasAuthorityMethod() {}

    [LocalPlayer]
    private void MyLocalPlayerMethod() {}

    [NetworkMethod(NetworkFlags.Active)]
    private void MyNetworkFlagsMethod() {}

    private void Awake()
    {
        var server = {|#0:Server|};
        var client = {|#1:Client|};
        var world = {|#2:World|};
        var som = {|#3:ServerObjectManager|};
        var com = {|#4:ClientObjectManager|};
        var visibility = Identity.{|#5:Visibility|};
        var svs = Identity.{|#6:SyncVarSender|};
        var owner = {|#14:Owner|};
        var isHost = {|#15:IsHost|};
        var isLocalClient = {|#16:IsLocalClient|};
        var isServerOnly = {|#17:IsServerOnly|};
        var isClientOnly = {|#18:IsClientOnly|};
        var hasAuthority = {|#19:HasAuthority|};

        {|#7:MyServerRpc|}();
        {|#8:MyClientRpc|}();
        {|#9:MyServerMethod|}();
        {|#10:MyClientMethod|}();
        {|#11:MyHasAuthorityMethod|}();
        {|#12:MyLocalPlayerMethod|}();
        {|#13:MyNetworkFlagsMethod|}();
    }
}
