using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace OwinSessionMiddleware
{
    /// <summary>
    /// The session middleware class.
    /// </summary>
    /// <typeparam name="TSessionProperty">The type of the session properties.</typeparam>
    public class SessionMiddleware<TSessionProperty> : OwinMiddleware
    {
        private readonly SessionMiddlewareOptions<TSessionProperty> _options;

        /// <summary>
        /// Constructs a new <see cref="SessionMiddleware{TValue}"/> instance.
        /// </summary>
        /// <param name="next">The next middleware in the chain.</param>
        /// <param name="options">The middleware specific options.</param>
        public SessionMiddleware(OwinMiddleware next, SessionMiddlewareOptions<TSessionProperty> options) : base(next)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _options = options;
        }

        /// <summary>
        /// Process an individual request.
        /// </summary>
        /// <param name="context">The current OWIN context.</param>
        /// <returns>An async task.</returns>
        public override async Task Invoke(IOwinContext context)
        {
            var sessionContext = await GetOrCreateSessionContext(context.Request, context.Response);
            context.Set(Constants.SessionContextOwinEnvironmentKey, sessionContext);

            await Next.Invoke(context);

            await UpdateSessionStore(sessionContext);
        }

        /// <summary>
        /// Reads the session id from the cookie in the request.
        /// </summary>
        /// <param name="request">The current request.</param>
        /// <returns>The session id or null in case the cookie was not available.</returns>
        protected virtual string ReadSessionIdFromRequest(IOwinRequest request)
            => request.Cookies[_options.CookieName];

        /// <summary>
        /// Writes the session id to a cookie in the response.
        /// </summary>
        /// <param name="response">The current response.</param>
        /// <param name="sessionId">The session id.</param>
        protected virtual void WriteSessionIdToResponse(IOwinResponse response, string sessionId)
        {
            var expires = _options.CookieLifetime.HasValue
                ? DateTime.UtcNow.Add(_options.CookieLifetime.Value)
                : (DateTime?)null;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = _options.UseSecureCookie,
                Domain = _options.CookieDomain,
                Expires = expires
            };

            response.Cookies.Append(_options.CookieName, sessionId);
        }

        /// <summary>
        /// Gets or creates a <see cref="SessionContext{TSessionProperty}"/> for the current request.
        /// If a new session needs to be created, a session cookie will be written to the response.
        /// </summary>
        /// <param name="request">The current request.</param>
        /// <param name="response">The current response.</param>
        /// <returns>The existing or newly created session context.</returns>
        protected virtual async Task<SessionContext<TSessionProperty>> GetOrCreateSessionContext(IOwinRequest request, IOwinResponse response)
        {
            var sessionId = ReadSessionIdFromRequest(request);

            if (sessionId != null)
            {
                var properties = await _options.Store.FindById(sessionId).ConfigureAwait(false)
                    ?? Enumerable.Empty<KeyValuePair<string, TSessionProperty>>();
                
                return SessionContext<TSessionProperty>.ForExistingSession(sessionId, properties);
            }

            sessionId = _options.UniqueSessionIdGenerator();
            WriteSessionIdToResponse(response, sessionId);
            return SessionContext<TSessionProperty>.ForNewSession(sessionId);
        }

        /// <summary>
        /// Update a <see cref="SessionContext{TSessionProperty}"/> in the store for the current request.
        /// </summary>
        /// <param name="sessionContext">The session context to update in the store.</param>
        /// <returns>A <see cref="Task"/> for async execution.</returns>
        protected virtual async Task UpdateSessionStore(SessionContext<TSessionProperty> sessionContext)
        {
            if (sessionContext.IsNew && !sessionContext.IsEmpty)
                await _options.Store.Add(sessionContext.SessionId, sessionContext.Properties).ConfigureAwait(false);

            if (sessionContext.IsModified)
            {
                if (!sessionContext.IsEmpty)
                    await _options.Store.Update(sessionContext.SessionId, sessionContext.Properties).ConfigureAwait(false);
                else
                    await _options.Store.Delete(sessionContext.SessionId).ConfigureAwait(false);
            }
        }
    }
}
