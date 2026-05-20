using Mirage.Collections;

namespace Mirage.Tests.GUiTests
{
    public class SyncListGuiTest : NetworkBehaviour
    {
        public readonly SyncList<int> SyncList = new SyncList<int>(100);
        [ShowInInspector] private readonly SyncList<string> _strings = new SyncList<string>(100);

        public SyncListGuiTest()
        {
            for (var i = 0; i < 5; i++)
            {
                SyncList.Add(i * i);
                _strings.Add("Item " + i.ToString());
            }

            _strings.Add("abc");
            _strings.Add("xyz");
        }
    }
}
