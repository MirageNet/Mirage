using System;
using System.Net;

namespace Mirage.Snippets.Components
{
    // Mock class to make the snippet compile/be structurally valid
    public class NetworkDiscoveryBase<TRequest, TResponse>
    {
        protected virtual void ProcessClientRequest(TRequest request, IPEndPoint endpoint) { }
        protected virtual TResponse ProcessRequest(TRequest request, IPEndPoint endpoint) => default;
        protected virtual TRequest GetRequest() => default;
        protected virtual void ProcessResponse(TResponse response, IPEndPoint endpoint) { }
    }

    // CodeEmbed-Start: discovery-messages
    public class DiscoveryRequest
    {
        public string language = "en";

        // Add properties for whatever information you want sent by clients
        // in their broadcast messages that servers will consume.
    }

    public enum GameMode { PvP, PvE }

    public class DiscoveryResponse
    {

        // you probably want uri so clients know how to connect to the server
        public Uri uri;

        public GameMode GameMode;
        public int TotalPlayers;
        public int HostPlayerName;

        // Add properties for whatever information you want the server to return to
        // clients for them to display or consume for establishing a connection.
    }
    // CodeEmbed-End: discovery-messages

    // CodeEmbed-Start: discovery-custom
    public class NewNetworkDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
    {
        #region Server

        protected override void ProcessClientRequest(DiscoveryRequest request, IPEndPoint endpoint)
        {
            base.ProcessClientRequest(request, endpoint);
        }

        protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint)
        {
            // TODO: Create your response and return it   
            return new DiscoveryResponse();
        }

        #endregion

        #region Client

        protected override DiscoveryRequest GetRequest()
        {
            return new DiscoveryRequest();
        }

        protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint)
        {
            // TODO: a server replied,  do something with the response such as invoking a unityevent
        }

        #endregion
    }
    // CodeEmbed-End: discovery-custom
}
