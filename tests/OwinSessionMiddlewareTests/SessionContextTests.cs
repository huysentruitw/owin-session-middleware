using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OwinSessionMiddleware;

namespace OwinSessionMiddlewareTests
{
    [TestFixture]
    public class SessionContextTests
    {
        [Test]
        public void ForNewSession_ShouldCreateEmptySessionContext()
        {
            var context = SessionContext.ForNewSession(Guid.NewGuid().ToString());
            Assert.That(context.IsNew, Is.True);
            Assert.That(context.IsModified, Is.False);
            Assert.That(context.IsEmpty, Is.True);
            Assert.That(context.Properties, Is.Empty);
        }

        [Test]
        public void ForNewSession_ShouldCopySessionId()
        {
            var sessionId = Guid.NewGuid().ToString();
            var context = SessionContext.ForNewSession(sessionId);
            Assert.That(context.SessionId, Is.EqualTo(sessionId));
        }

        [Test]
        public void ForNewSession_NullSessionId_ShouldThrowException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SessionContext.ForNewSession(null));
            Assert.That(ex.ParamName, Is.EqualTo("sessionId"));
        }

        [Test]
        public void ForNewSession_EmptySessionId_ShouldThrowException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SessionContext.ForNewSession(string.Empty));
            Assert.That(ex.ParamName, Is.EqualTo("sessionId"));
        }

        [Test]
        public void ForExistingSession_IsNewShouldBeFalse()
        {
            var context = SessionContext.ForExistingSession(Guid.NewGuid().ToString(), Enumerable.Empty<KeyValuePair<string, object>>());
            Assert.That(context.IsNew, Is.False);
        }

        [Test]
        public void ForExistingSession_PassingEmptyPropertyList_IsEmptyShouldBeTrue()
        {
            var context = SessionContext.ForExistingSession(Guid.NewGuid().ToString(), Enumerable.Empty<KeyValuePair<string, object>>());
            Assert.That(context.IsEmpty, Is.True);
        }

        [Test]
        public void ForExistingSession_PassingPropertyList_IsEmptyShouldBeFalse()
        {
            var context = SessionContext.ForExistingSession(Guid.NewGuid().ToString(), new[] { Kvp("A", 1) });
            Assert.That(context.IsEmpty, Is.False);
        }

        [Test]
        public void ForExistingSession_ShouldCopySessionId()
        {
            var sessionId = Guid.NewGuid().ToString();
            var context = SessionContext.ForExistingSession(sessionId, Enumerable.Empty<KeyValuePair<string, object>>());
            Assert.That(context.SessionId, Is.EqualTo(sessionId));
        }

        [Test]
        public void ForExistingSession_PassingPropertyList_ShouldReturnProperties()
        {
            var context = SessionContext.ForExistingSession(Guid.NewGuid().ToString(), new[] { Kvp("A", 1), Kvp("B", 2) });
            Assert.That(context.Properties, Contains.Item(Kvp("A", 1)));
            Assert.That(context.Properties, Contains.Item(Kvp("B", 2)));
        }

        [Test]
        public void ForExistingSession_NullSessionId_ShouldThrowException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SessionContext.ForExistingSession(null, Enumerable.Empty<KeyValuePair<string, object>>()));
            Assert.That(ex.ParamName, Is.EqualTo("sessionId"));
        }

        [Test]
        public void ForExistingSession_EmptySessionId_ShouldThrowException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SessionContext.ForExistingSession(string.Empty, Enumerable.Empty<KeyValuePair<string, object>>()));
            Assert.That(ex.ParamName, Is.EqualTo("sessionId"));
        }

        [Test]
        public void ForExistingSession_NullProperties_ShouldThrowException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SessionContext.ForExistingSession(Guid.NewGuid().ToString(), null));
            Assert.That(ex.ParamName, Is.EqualTo("properties"));
        }

        [Test]
        public void Get_UnknownProperty_ShouldReturnNull()
        {
            var context = SessionContext.ForExistingSession("abc", Enumerable.Empty<KeyValuePair<string, object>>());
            Assert.That(context.Get("B"), Is.Null);
        }

        [Test]
        public void Get_ExistingProperty_ShouldReturnPropertyValue()
        {
            var context = SessionContext.ForExistingSession(Guid.NewGuid().ToString(), new[] { Kvp("A", 1), Kvp("B", 2) });
            Assert.That(context.Get("B"), Is.EqualTo(2));
        }

        [Test]
        public void Get_NullKey_ShouldThrowException()
        {
            var context = SessionContext.ForExistingSession(Guid.NewGuid().ToString(), Enumerable.Empty<KeyValuePair<string, object>>());
            var ex = Assert.Throws<ArgumentNullException>(() => context.Get(null));
            Assert.That(ex.ParamName, Is.EqualTo("key"));
        }

        [Test]
        public void GetGeneric_UnknownProperty_ShouldReturnDefaultT()
        {
            var context = SessionContext.ForNewSession("abc");
            Assert.That(context.Get<int>("B"), Is.Zero);
            Assert.That(context.Get<bool>("C"), Is.False);
        }

        [Test]
        public void GetGeneric_ExistingPropertyOfCorrectType_ShouldReturnPropertyValue()
        {
            var context = SessionContext.ForNewSession("abc");
            context.AddOrUpdate("B", 5);
            context.AddOrUpdate("C", true);
            Assert.That(context.Get<int>("B"), Is.EqualTo(5));
            Assert.That(context.Get<bool>("C"), Is.True);
        }

        [Test]
        public void GetGeneric_ExistingPropertyOfIncorrectType_ShouldThrowException()
        {
            var context = SessionContext.ForNewSession("abc");
            context.AddOrUpdate("B", true);
            Assert.Throws<InvalidCastException>(() => context.Get<int>("B"));
        }

        [Test]
        public void AddOrUpdate_AddNewPropertyToExistingSession_ShouldRememberPropertyAndSetIsModified()
        {
            var context = SessionContext.ForExistingSession(Guid.NewGuid().ToString(), new[] { Kvp("A", 1), Kvp("B", 2) });
            Assert.That(context.IsModified, Is.False);
            context.AddOrUpdate("C", 3);
            Assert.That(context.Properties.Count(), Is.EqualTo(3));
            Assert.That(context.Properties, Contains.Item(Kvp("A", 1)));
            Assert.That(context.Properties, Contains.Item(Kvp("B", 2)));
            Assert.That(context.Properties, Contains.Item(Kvp("C", 3)));
            Assert.That(context.IsNew, Is.False);
            Assert.That(context.IsModified, Is.True);
        }

        [Test]
        public void AddOrUpdate_UpdatePropertyInExistingSession_ShouldRememberUpdatedPropertyAndSetIsModified()
        {
            var context = SessionContext.ForExistingSession(Guid.NewGuid().ToString(), new[] { Kvp("A", 1), Kvp("B", 2) });
            Assert.That(context.IsModified, Is.False);
            context.AddOrUpdate("A", 4);
            Assert.That(context.Properties.Count(), Is.EqualTo(2));
            Assert.That(context.Properties, Contains.Item(Kvp("A", 4)));
            Assert.That(context.Properties, Contains.Item(Kvp("B", 2)));
            Assert.That(context.IsNew, Is.False);
            Assert.That(context.IsModified, Is.True);
        }

        [Test]
        public void AddOrUpdate_AddNewPropertyToNewSession_ShouldRememberPropertyButNotSetIsModified()
        {
            var context = SessionContext.ForNewSession(Guid.NewGuid().ToString());
            context.AddOrUpdate("A", 1);
            context.AddOrUpdate("B", 2);
            context.AddOrUpdate("C", 3);
            Assert.That(context.Properties.Count(), Is.EqualTo(3));
            Assert.That(context.Properties, Contains.Item(Kvp("A", 1)));
            Assert.That(context.Properties, Contains.Item(Kvp("B", 2)));
            Assert.That(context.Properties, Contains.Item(Kvp("C", 3)));
            Assert.That(context.IsNew, Is.True);
            Assert.That(context.IsModified, Is.False);
        }

        [Test]
        public void AddOrUpdate_UpdatePropertyInNewSession_ShouldRememberUpdatedPropertyButNotSetIsModified()
        {
            var context = SessionContext.ForNewSession(Guid.NewGuid().ToString());
            context.AddOrUpdate("A", 3);
            context.AddOrUpdate("B", 2);

            context.AddOrUpdate("A", 4);
            Assert.That(context.Properties.Count(), Is.EqualTo(2));
            Assert.That(context.Properties, Contains.Item(Kvp("A", 4)));
            Assert.That(context.Properties, Contains.Item(Kvp("B", 2)));
            Assert.That(context.IsNew, Is.True);
            Assert.That(context.IsModified, Is.False);
        }

        [Test]
        public void AddOrUpdate_NullKey_ShouldThrowException()
        {
            var context = SessionContext.ForNewSession(Guid.NewGuid().ToString());
            var ex = Assert.Throws<ArgumentNullException>(() => context.AddOrUpdate(null, 2));
            Assert.That(ex.ParamName, Is.EqualTo("key"));
        }

        [Test]
        public void Delete_UnknownProperty_ShouldNotThrowException()
        {
            var context = SessionContext.ForNewSession(Guid.NewGuid().ToString());
            context.Delete("A");
        }

        [Test]
        public void Delete_ExistingPropertyFromExistingSession_ShouldDeletePropertyAndSetIsModified()
        {
            var context = SessionContext.ForExistingSession(Guid.NewGuid().ToString(), new[] { Kvp("A", 1), Kvp("B", 2), Kvp("C", 3) });
            context.Delete("B");
            Assert.That(context.Properties.Count(), Is.EqualTo(2));
            Assert.That(context.Properties, Contains.Item(Kvp("A", 1)));
            Assert.That(context.Properties, Contains.Item(Kvp("C", 3)));
            Assert.That(context.IsNew, Is.False);
            Assert.That(context.IsModified, Is.True);
        }

        [Test]
        public void Delete_ExistingPropertyFromNewSession_ShouldDeletePropertyButNotSetIsModified()
        {
            var context = SessionContext.ForNewSession(Guid.NewGuid().ToString());
            context.AddOrUpdate("A", 1);
            context.AddOrUpdate("B", 2);
            context.AddOrUpdate("C", 3);
            context.Delete("B");
            Assert.That(context.Properties.Count(), Is.EqualTo(2));
            Assert.That(context.Properties, Contains.Item(Kvp("A", 1)));
            Assert.That(context.Properties, Contains.Item(Kvp("C", 3)));
            Assert.That(context.IsNew, Is.True);
            Assert.That(context.IsModified, Is.False);
        }

        [Test]
        public void Delete_NullKey_ShouldThrowException()
        {
            var context = SessionContext.ForNewSession(Guid.NewGuid().ToString());
            var ex = Assert.Throws<ArgumentNullException>(() => context.Delete(null));
            Assert.That(ex.ParamName, Is.EqualTo("key"));
        }

        [Test]
        public void Clear_ExistingSession_ShouldRemoveAllPropertiesAndSetIsModified()
        {
            var context = SessionContext.ForExistingSession(Guid.NewGuid().ToString(), new[] { Kvp("A", 1), Kvp("B", 2) });
            context.Clear();
            Assert.That(context.Properties.Count(), Is.Zero);
            Assert.That(context.IsNew, Is.False);
            Assert.That(context.IsModified, Is.True);
        }

        [Test]
        public void Clear_ExistingEmptySession_ShouldStillBeEmptyButNotSetIsModified()
        {
            var context = SessionContext.ForExistingSession(Guid.NewGuid().ToString(), Enumerable.Empty<KeyValuePair<string, object>>());
            Assert.That(context.Properties.Count(), Is.Zero);
            context.Clear();
            Assert.That(context.Properties.Count(), Is.Zero);
            Assert.That(context.IsNew, Is.False);
            Assert.That(context.IsModified, Is.False);
        }

        [Test]
        public void Clear_NewSession_ShouldRemoveAllPropertiesButNotSetIsModified()
        {
            var context = SessionContext.ForNewSession(Guid.NewGuid().ToString());
            context.AddOrUpdate("A", 1);
            context.AddOrUpdate("B", 2);
            context.Clear();
            Assert.That(context.Properties.Count(), Is.Zero);
            Assert.That(context.IsNew, Is.True);
            Assert.That(context.IsModified, Is.False);
        }

        private static KeyValuePair<string, object> Kvp(string key, object value)
            => new KeyValuePair<string, object>(key, value);
    }
}
