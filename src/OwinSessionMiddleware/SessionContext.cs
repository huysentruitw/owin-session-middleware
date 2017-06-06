using System;
using System.Collections.Generic;
using System.Linq;

namespace OwinSessionMiddleware
{
    /// <summary>
    /// The session context.
    /// </summary>
    public class SessionContext
    {
        private readonly string _sessionId;
        private readonly IDictionary<string, object> _properties;
        private readonly bool _isNew;
        private bool _isModified = false;

        internal string SessionId => _sessionId;
        internal bool IsNew => _isNew;
        internal bool IsModified => _isModified;
        internal bool IsEmpty => !_properties.Any();
        internal IEnumerable<KeyValuePair<string, object>> Properties => _properties.Select(x => x);

        internal static SessionContext ForExistingSession(string sessionId, IEnumerable<KeyValuePair<string, object>> properties)
            => new SessionContext(sessionId, properties?.ToDictionary(x => x.Key, x => x.Value), false);

        internal static SessionContext ForNewSession(string sessionId)
            => new SessionContext(sessionId, new Dictionary<string, object>(), true);

        private SessionContext(string sessionId, IDictionary<string, object> properties, bool isNew)
        {
            if (string.IsNullOrEmpty(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            _sessionId = sessionId;
            _properties = properties;
            _isNew = isNew;
        }

        /// <summary>
        /// Gets a property for the current session.
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <returns>The value of the property or null in case the property was not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the key is null.</exception>
        public object Get(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (_properties.ContainsKey(key)) return _properties[key];
            return null;
        }

        /// <summary>
        /// Gets a property for the current session.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="key">The key of the property.</param>
        /// <returns>The value of the property or default(T) in case the property was not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the key is null.</exception>
        /// <exception cref="InvalidCastException">Thrown when an existing property is of a different type.</exception>
        public T Get<T>(string key) => (T)(Get(key) ?? default(T));

        /// <summary>
        /// Add or update a property for the current session.
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <exception cref="ArgumentNullException">Thrown when the key is null.</exception>
        public void AddOrUpdate(string key, object value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (_properties.ContainsKey(key)) _properties[key] = value;
            else _properties.Add(key, value);
            if (!_isNew) _isModified = true;
        }

        /// <summary>
        /// Delete a property for the current session.
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <exception cref="ArgumentNullException">Thrown when the key is null.</exception>
        public void Delete(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _properties.Remove(key);
            if (!_isNew) _isModified = true;
        }

        /// <summary>
        /// Clears the current session.
        /// </summary>
        public void Clear()
        {
            if (!_properties.Any()) return;
            _properties.Clear();
            if (!_isNew) _isModified = true;
        }
    }
}
