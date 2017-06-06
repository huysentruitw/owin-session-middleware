using System.Collections.Generic;
using System.Threading.Tasks;

namespace OwinSessionMiddleware
{
    /// <summary>
    /// Interface that describes the session store.
    /// </summary>
    public interface ISessionStore
    {
        /// <summary>
        /// Finds a session by its id.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <returns>The session properties or null when the session was not found.</returns>
        Task<IEnumerable<KeyValuePair<string, object>>> FindById(string sessionId);

        /// <summary>
        /// Add a session to the store.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="properties">The session properties.</param>
        /// <returns>A <see cref="Task"/> for async execution.</returns>
        Task Add(string sessionId, IEnumerable<KeyValuePair<string, object>> properties);

        /// <summary>
        /// Updates an existing session in the store.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="properties">The session properties.</param>
        /// <returns>A <see cref="Task"/> for async execution.</returns>
        Task Update(string sessionId, IEnumerable<KeyValuePair<string, object>> properties);

        /// <summary>
        /// Deletes an existing session from the store.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <returns>A <see cref="Task"/> for async execution.</returns>
        Task Delete(string sessionId);
    }
}
