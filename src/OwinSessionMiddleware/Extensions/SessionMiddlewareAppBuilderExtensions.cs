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
        /// <typeparam name="TSessionProperty">The type of the session properties.</typeparam>
        /// <param name="app">The <see cref="IAppBuilder"/> instance.</param>
        /// <param name="options">The optional session middleware options.</param>
        /// <returns>The <see cref="IAppBuilder"/> instance for method chaining.</returns>
        public static IAppBuilder UseSessionMiddleware<TSessionProperty>(this IAppBuilder app, SessionMiddlewareOptions<TSessionProperty> options = null)
            => app.Use<SessionMiddleware<TSessionProperty>>(options ?? new SessionMiddlewareOptions<TSessionProperty>());

        /// <summary>
        /// Use the session middleware with string as session property type.
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> instance.</param>
        /// <param name="options">The optional session middleware options.</param>
        /// <returns>The <see cref="IAppBuilder"/> instance for method chaining.</returns>
        public static IAppBuilder UseSessionMiddleware(this IAppBuilder app, SessionMiddlewareOptions options = null)
            => app.Use<SessionMiddleware<string>>(options ?? new SessionMiddlewareOptions<string>());
    }
}
