using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using OwinSessionMiddleware;

namespace OwinSessionMiddlewareTests
{
    [TestFixture]
    public class InMemorySessionStoreTests
    {
        private InMemorySessionStore _store;

        [SetUp]
        public void SetUp()
        {
            _store = new InMemorySessionStore();
        }

        [Test]
        public async Task Find_UnknownProperty_ShouldReturnNull()
        {
            var sessionId = Guid.NewGuid().ToString();
            Assert.That(await _store.FindById(sessionId), Is.Null);
        }

        [Test]
        public async Task Add_Session_ShouldPersistSession()
        {
            var sessionId = Guid.NewGuid().ToString();
            var session = new[] { Kvp("A", 1) };
            await _store.Add(sessionId, session);
            Assert.That(await _store.FindById(sessionId), Is.EqualTo(session));
        }

        [Test]
        public async Task Add_TwoSessionsWithSameId_ShouldThrowArgumentException()
        {
            var sessionId = Guid.NewGuid().ToString();
            var session1 = new[] { Kvp("A", 1) };
            var session2 = new[] { Kvp("B", 2) };
            await _store.Add(sessionId, session1);
            Assert.ThrowsAsync<ArgumentException>(() => _store.Add(sessionId, session2));
        }

        [Test]
        public void Update_UnknownSession_ShouldThrowException()
        {
            var sessionId = Guid.NewGuid().ToString();
            var session = new[] { Kvp("A", 1) };
            Assert.ThrowsAsync<KeyNotFoundException>(() => _store.Update(sessionId, session));
        }

        [Test]
        public async Task Update_ExistingSession_ShouldSaveUpdatedSession()
        {
            var sessionId = Guid.NewGuid().ToString();
            await _store.Add(sessionId, new[] { Kvp("A", 1) });
            Assert.That((await _store.FindById(sessionId)).Count(), Is.EqualTo(1));
            Assert.That((await _store.FindById(sessionId)).First().Key, Is.EqualTo("A"));
            Assert.That((await _store.FindById(sessionId)).First().Value, Is.EqualTo(1));

            await _store.Update(sessionId, new[] { Kvp("B", 2) });
            Assert.That((await _store.FindById(sessionId)).Count(), Is.EqualTo(1));
            Assert.That((await _store.FindById(sessionId)).First().Key, Is.EqualTo("B"));
            Assert.That((await _store.FindById(sessionId)).First().Value, Is.EqualTo(2));
        }

        [Test]
        public async Task Update_ExistingSession_ShouldLeaveOtherSessionsIntact()
        {
            var sessionId = Guid.NewGuid().ToString();
            var otherSessionId = Guid.NewGuid().ToString();

            await _store.Add(sessionId, new[] { Kvp("A", 1) });
            await _store.Add(otherSessionId, new[] { Kvp("B", 2) });

            await _store.Update(sessionId, new[] { Kvp("C", 3) });
            Assert.That((await _store.FindById(sessionId)).Count(), Is.EqualTo(1));
            Assert.That((await _store.FindById(sessionId)).First().Key, Is.EqualTo("C"));
            Assert.That((await _store.FindById(sessionId)).First().Value, Is.EqualTo(3));
            Assert.That((await _store.FindById(otherSessionId)).Count(), Is.EqualTo(1));
            Assert.That((await _store.FindById(otherSessionId)).First().Key, Is.EqualTo("B"));
            Assert.That((await _store.FindById(otherSessionId)).First().Value, Is.EqualTo(2));
        }

        [Test]
        public async Task Delete_UnknownSession_ShouldNotThrowException()
        {
            var sessionId = Guid.NewGuid().ToString();
            await _store.Delete(sessionId);
        }

        [Test]
        public async Task Delete_ExistingSession_ShouldDeleteSession()
        {
            var sessionId = Guid.NewGuid().ToString();
            await _store.Add(sessionId, new[] { Kvp("A", 1) });
            Assert.That(await _store.FindById(sessionId), Is.Not.Null);
            await _store.Delete(sessionId);
            Assert.That(await _store.FindById(sessionId), Is.Null);
        }

        [Test]
        public async Task Delete_ExistingSession_ShouldKeepOtherSessions()
        {
            var sessionId = Guid.NewGuid().ToString();
            var otherSessionId = Guid.NewGuid().ToString();
            await _store.Add(sessionId, new[] { Kvp("A", 1) });
            await _store.Add(otherSessionId, new[] { Kvp("B", 2) });
            Assert.That(await _store.FindById(sessionId), Is.Not.Null);
            Assert.That(await _store.FindById(otherSessionId), Is.Not.Null);
            await _store.Delete(sessionId);
            Assert.That(await _store.FindById(sessionId), Is.Null);
            Assert.That(await _store.FindById(otherSessionId), Is.Not.Null);
        }

        private static KeyValuePair<string, object> Kvp(string key, object value)
            => new KeyValuePair<string, object>(key, value);
    }
}
