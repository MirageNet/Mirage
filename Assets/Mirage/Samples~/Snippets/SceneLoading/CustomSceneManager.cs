namespace Mirage.Snippets.SceneLoading.OverrideOnServerAuthenticated
{
#pragma warning disable CS0618 // Type or member is obsolete
    // CodeEmbed-Start: custom-scene-manager-on-server-authenticated
    public class MySceneManager : NetworkSceneManager
    {
        protected internal override void OnServerAuthenticated(INetworkPlayer player)
        {
            // just load server's active scene instead of all additive scenes as well
            player.Send(new SceneMessage { MainActivateScene = ActiveScenePath });
            player.Send(new SceneReadyMessage());
        }
    }
    // CodeEmbed-End: custom-scene-manager-on-server-authenticated
#pragma warning restore CS0618
}

namespace Mirage.Snippets.SceneLoading.OverrideStart
{
#pragma warning disable CS0618 // Type or member is obsolete
    // CodeEmbed-Start: custom-scene-manager-start
    public class MySceneManager : NetworkSceneManager
    {
        public override void Start()
        {
            // add your stuff before.

            base.Start();

            // add your stuff after.
        }
    }
    // CodeEmbed-End: custom-scene-manager-start
#pragma warning restore CS0618
}

namespace Mirage.Snippets.SceneLoading.OverrideOnDestroy
{
#pragma warning disable CS0618 // Type or member is obsolete
    // CodeEmbed-Start: custom-scene-manager-on-destroy
    public class MySceneManager : NetworkSceneManager
    {
        public override void OnDestroy()
        {
            // add your stuff before.

            base.OnDestroy();

            // add your stuff after.
        }
    }
    // CodeEmbed-End: custom-scene-manager-on-destroy
#pragma warning restore CS0618
}
