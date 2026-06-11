using Mirage;
using Mirage.Collections;

namespace Mirage.Snippets.General
{
    // CodeEmbed-Start: troubleshooting-no-writer-triggering
    public struct MyCustomType
    {
        public int id;
        public string name;
    }

    public class MyBehaviour : NetworkBehaviour
    {
        private readonly SyncList<MyCustomType> myList = new SyncList<MyCustomType>();
    }
    // CodeEmbed-End: troubleshooting-no-writer-triggering
}

namespace Mirage.Snippets.General.Resolved
{
    // CodeEmbed-Start: troubleshooting-no-writer-resolved
    [NetworkMessage]
    public struct MyCustomType
    {
        public int id;
        public string name;
    }
    // CodeEmbed-End: troubleshooting-no-writer-resolved
}
