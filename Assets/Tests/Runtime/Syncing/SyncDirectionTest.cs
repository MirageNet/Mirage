using NUnit.Framework;

namespace Mirage.Tests.Runtime.Syncing
{
    public class SyncDirectionTest : TestBase
    {
        [Test]
        public void FromToNoneIsValid()
        {
            var valid = SyncSettings.IsValidDirection(SyncFrom.None, SyncTo.None);
            Assert.IsTrue(valid);
        }

        [Test]
        public void FromNoneToAnyIsInvalid()
        {
            for (var i = 1; i < 8; i++)
            {
                var valid = SyncSettings.IsValidDirection(SyncFrom.None, (SyncTo)i);
                Assert.IsFalse(valid);
            }
        }
        public void FromAnyToNoneIsInvalid()
        {
            for (var i = 1; i < 4; i++)
            {
                var valid = SyncSettings.IsValidDirection((SyncFrom)i, SyncTo.None);
                Assert.IsFalse(valid);
            }
        }

        [Test]
        // cases are in order, 1 to 7, (bit mask)
        [TestCase(SyncTo.Owner, ExpectedResult = false)]
        [TestCase(SyncTo.ObserversOnly, ExpectedResult = false)]
        [TestCase(SyncTo.Owner | SyncTo.ObserversOnly, ExpectedResult = false)]
        [TestCase(SyncTo.Server, ExpectedResult = true)]
        [TestCase(SyncTo.Server | SyncTo.Owner, ExpectedResult = false)]
        [TestCase(SyncTo.Server | SyncTo.ObserversOnly, ExpectedResult = true)]
        [TestCase(SyncTo.Server | SyncTo.Owner | SyncTo.ObserversOnly, ExpectedResult = false)]
        public bool FromOwner(SyncTo to)
        {
            return SyncSettings.IsValidDirection(SyncFrom.Owner, to);
        }

        [Test]
        // cases are in order, 1 to 7, (bit mask)
        [TestCase(SyncTo.Owner, ExpectedResult = true)]
        [TestCase(SyncTo.ObserversOnly, ExpectedResult = true)]
        [TestCase(SyncTo.Owner | SyncTo.ObserversOnly, ExpectedResult = true)]
        [TestCase(SyncTo.Server, ExpectedResult = false)]
        [TestCase(SyncTo.Server | SyncTo.Owner, ExpectedResult = false)]
        [TestCase(SyncTo.Server | SyncTo.ObserversOnly, ExpectedResult = false)]
        [TestCase(SyncTo.Server | SyncTo.Owner | SyncTo.ObserversOnly, ExpectedResult = false)]
        public bool FromServer(SyncTo to)
        {
            return SyncSettings.IsValidDirection(SyncFrom.Server, to);
        }


        [Test]
        // cases are in order, 1 to 7, (bit mask)
        [TestCase(SyncTo.Owner, ExpectedResult = false)] // can't go just to owner, (would mean from.owner is going nowhere)
        [TestCase(SyncTo.ObserversOnly, ExpectedResult = false)] // same as above, owner needs to send to server
        [TestCase(SyncTo.Owner | SyncTo.ObserversOnly, ExpectedResult = false)] // same
        [TestCase(SyncTo.Server, ExpectedResult = false)] // same, but for server instead
        [TestCase(SyncTo.Server | SyncTo.Owner, ExpectedResult = true)]
        [TestCase(SyncTo.Server | SyncTo.ObserversOnly, ExpectedResult = true)] // server changes only go to observers, but owner changes will go to server. 
        [TestCase(SyncTo.Server | SyncTo.Owner | SyncTo.ObserversOnly, ExpectedResult = true)]
        public bool FromOwnerAndServer(SyncTo to)
        {
            return SyncSettings.IsValidDirection(SyncFrom.Owner | SyncFrom.Server, to);
        }
    }
}
