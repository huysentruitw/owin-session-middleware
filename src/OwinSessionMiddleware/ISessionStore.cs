using System.Collections.Generic;
using System.Threading.Tasks;

namespace OwinSessionMiddleware
{
    public interface ISessionStore
    {
        Task<IEnumerable<KeyValuePair<string, string>>> FindById(string sessionId);
        Task Add(string sessionId, IEnumerable<KeyValuePair<string, string>> properties);
        Task Update(string sessionId, IEnumerable<KeyValuePair<string, string>> properties);
    }
}
