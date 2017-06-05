using System;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace OwinSessionMiddleware
{
    public class SessionMiddleware : OwinMiddleware
    {
        private readonly SessionMiddlewareOptions _options;

        public SessionMiddleware(OwinMiddleware next, SessionMiddlewareOptions options) : base(next)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _options = options;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var sessionContext = await GetOrCreateSessionContext(context.Request);
            context.Set(_options.SessionContextOwinEnvironmentKey, sessionContext);

            await Next.Invoke(context);

            await SaveSessionContextIfNeeded(sessionContext);
        }

        private async Task<SessionContext> GetOrCreateSessionContext(IOwinRequest request)
        {
            var sessionId = request.Cookies[_options.CookieName];

            if (sessionId != null)
            {
                var properties = await _options.Store.FindById(sessionId).ConfigureAwait(false);
                if (properties == null) throw new InvalidOperationException($"SessionId {sessionId} not found in store");
                return SessionContext.ForExistingSession(sessionId, properties);
            }

            return SessionContext.ForNewSession(_options.UniqueSessionIdGenerator());
        }

        private async Task SaveSessionContextIfNeeded(SessionContext sessionContext)
        {
            if (sessionContext.IsNew) await _options.Store.Add(sessionContext.SessionId, sessionContext.Properties).ConfigureAwait(false);
            await _options.Store.Update(sessionContext.SessionId, sessionContext.Properties).ConfigureAwait(false);
        }
    }
}
