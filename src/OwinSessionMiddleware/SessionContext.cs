using System;
using System.Collections.Generic;
using System.Linq;

namespace OwinSessionMiddleware
{
    public class SessionContext
    {
        private readonly string _sessionId;
        private readonly IDictionary<string, string> _properties;
        private readonly bool _isNew;
        private bool _isModified = false;

        internal string SessionId => _sessionId;
        internal bool IsNew => _isNew;
        internal bool IsModified => _isModified;
        internal IEnumerable<KeyValuePair<string, string>> Properties => _properties.Select(x => x);

        internal static SessionContext ForExistingSession(string sessionId, IEnumerable<KeyValuePair<string, string>> properties)
            => new SessionContext(sessionId, properties.ToDictionary(x => x.Key, x => x.Value), false);

        internal static SessionContext ForNewSession(string sessionId)
            => new SessionContext(sessionId, new Dictionary<string, string>(), true);

        private SessionContext(string sessionId, IDictionary<string, string> properties, bool isNew)
        {
            if (string.IsNullOrEmpty(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            _sessionId = sessionId;
            _properties = properties;
            _isNew = isNew;
        }

        public string Find(string key)
        {
            if (_properties.ContainsKey(key)) return _properties[key];
            return null;
        }

        public void AddOrUpdate(string key, string value)
        {
            if (_properties.ContainsKey(key)) _properties[key] = value;
            else _properties.Add(key, value);
            if (!_isNew) _isModified = true;
        }

        public void Delete(string key)
        {
            _properties.Remove(key);
            if (!_isNew) _isModified = true;
        }
    }
}
