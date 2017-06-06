using Owin;

namespace OwinSessionMiddleware
{
    /// <summary>
    /// Extension methods for <see cref="IAppBuilder"/>.
    /// </summary>
    public static class SessionMiddlewareAppBuilderExtensions
    {
        /// <summary>
        /// Use the session middleware.
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> instance.</param>
        /// <param name="options">The optional session middleware options.</param>
        /// <returns>The <see cref="IAppBuilder"/> instance for method chaining.</returns>
        public static IAppBuilder UseSessionMiddleware(this IAppBuilder app, SessionMiddlewareOptions options = null)
            => app.Use<SessionMiddleware>(options ?? new SessionMiddlewareOptions());
    }
}
