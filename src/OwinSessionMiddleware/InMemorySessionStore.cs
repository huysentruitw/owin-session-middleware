using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwinSessionMiddleware
{
    public class InMemorySessionStore : ISessionStore
    {
        private readonly Dictionary<string, IEnumerable<KeyValuePair<string, string>>> _store = new Dictionary<string, IEnumerable<KeyValuePair<string, string>>>();

        public Task Add(string sessionId, IEnumerable<KeyValuePair<string, string>> properties)
        {
            _store.Add(sessionId, properties.ToList());
            return Task.CompletedTask;
        }

        public Task<IEnumerable<KeyValuePair<string, string>>> FindById(string sessionId)
            => Task.FromResult(_store.ContainsKey(sessionId) ? _store[sessionId] : null);

        public Task Update(string sessionId, IEnumerable<KeyValuePair<string, string>> properties)
        {
            _store[sessionId] = properties.ToList();
            return Task.CompletedTask;
        }
    }
}
