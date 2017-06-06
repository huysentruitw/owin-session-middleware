using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwinSessionMiddleware
{
    /// <summary>
    /// An in-memory implementation of <see cref="ISessionStore{TSessionProperty}"/>.
    /// </summary>
    /// <typeparam name="TSessionProperty"></typeparam>
    public class InMemorySessionStore<TSessionProperty> : ISessionStore<TSessionProperty>
    {
        private readonly Dictionary<string, IEnumerable<KeyValuePair<string, TSessionProperty>>> _store = new Dictionary<string, IEnumerable<KeyValuePair<string, TSessionProperty>>>();

        /// <summary>
        /// Finds a session by its id.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <returns>The session properties or null when the session was not found.</returns>
        public Task<IEnumerable<KeyValuePair<string, TSessionProperty>>> FindById(string sessionId)
            => Task.FromResult(_store.ContainsKey(sessionId) ? _store[sessionId] : null);

        /// <summary>
        /// Add a session to the store.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="properties">The session properties.</param>
        /// <returns>A <see cref="Task"/> for async execution.</returns>
        public Task Add(string sessionId, IEnumerable<KeyValuePair<string, TSessionProperty>> properties)
        {
            _store.Add(sessionId, properties.ToList());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates an existing session in the store.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="properties">The session properties.</param>
        /// <returns>A <see cref="Task"/> for async execution.</returns>
        public Task Update(string sessionId, IEnumerable<KeyValuePair<string, TSessionProperty>> properties)
        {
            _store[sessionId] = properties.ToList();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes an existing session from the store.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <returns>A <see cref="Task"/> for async execution.</returns>
        public Task Delete(string sessionId)
        {
            if (_store.ContainsKey(sessionId)) _store.Remove(sessionId);
            return Task.CompletedTask;
        }
    }
}
