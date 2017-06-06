using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OwinSessionMiddleware;

namespace OwinSessionMiddlewareTests
{
    [TestFixture]
    public class SessionMiddlewareDefaultsTests
    {
        [Test]
        public void UniqueSessionIdGenerator_CallThousandTimes_ValuesShouldBeUnique()
        {
            var set = new HashSet<string>(Enumerable.Range(0, 1000).Select(_ => SessionMiddlewareDefaults.UniqueSessionIdGenerator()));
            Assert.That(set.Count, Is.EqualTo(1000));
        }

        [Test]
        public void UniqueSessionIdGenerator_CallThousandTimes_ResultShouldNotExceed45Characters()
        {
            var maxLength = Enumerable.Range(0, 1000).Max(_ => SessionMiddlewareDefaults.UniqueSessionIdGenerator().Length);
            Assert.That(maxLength, Is.LessThanOrEqualTo(45));
            Assert.That(maxLength, Is.GreaterThan(40), "Max length too small, consider re-running the test");
        }
    }
}
