using Microsoft.Owin;

namespace OwinSessionMiddleware
{
    /// <summary>
    /// Extension methods for <see cref="IOwinContext"/>.
    /// </summary>
    public static class SessionMiddlewareOwinContextExtensions
    {
        /// <summary>
        /// Get the session context from the OWIN context.
        /// </summary>
        /// <typeparam name="TSessionProperty">The type of the session properties.</typeparam>
        /// <param name="context">The <see cref="IOwinContext"/> instance.</param>
        /// <returns>The session context or null in case no session context is available.</returns>
        /// <remarks>In case no session context is availble, register the session middleware before the middleware where this method gets called.</remarks>
        public static SessionContext<TSessionProperty> GetSessionContext<TSessionProperty>(this IOwinContext context)
            => context.Get<SessionContext<TSessionProperty>>(Constants.SessionContextOwinEnvironmentKey);
    }
}
