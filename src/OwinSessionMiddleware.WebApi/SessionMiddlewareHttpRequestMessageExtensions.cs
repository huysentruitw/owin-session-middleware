using System;
using System.Net.Http;

namespace OwinSessionMiddleware.WebApi
{
    /// <summary>
    /// Extension methods for <see cref="HttpRequestMessage"/>.
    /// </summary>
    public static class SessionMiddlewareHttpRequestMessageExtensions
    {
        /// <summary>
        /// Get a property for the current session.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> instance.</param>
        /// <param name="key">The key of the property.</param>
        /// <returns>The value of the property or null in case the property was not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the key is null.</exception>
        public static object GetSessionProperty(this HttpRequestMessage request, string key)
            => request.GetSessionContext().Get(key);

        /// <summary>
        /// Get a property for the current session.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="request">The <see cref="HttpRequestMessage"/> instance.</param>
        /// <param name="key">The key of the property.</param>
        /// <returns>The value of the property or default(T) in case the property was not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the key is null.</exception>
        /// <exception cref="InvalidCastException">Thrown when an existing property is of a different type.</exception>
        public static T GetSessionProperty<T>(this HttpRequestMessage request, string key)
            => request.GetSessionContext().Get<T>(key);

        /// <summary>
        /// Sets a property for the current session.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> instance.</param>
        /// <param name="key">The key of the property.</param>
        /// <param name="value">The new value of the property.</param>
        /// <exception cref="ArgumentNullException">Thrown when the key is null.</exception>
        public static void SetSessionProperty(this HttpRequestMessage request, string key, object value)
            => request.GetSessionContext().AddOrUpdate(key, value);

        /// <summary>
        /// Deletes a property from the current session.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> instance.</param>
        /// <param name="key">The key of the property.</param>
        /// <exception cref="ArgumentNullException">Thrown when the key is null.</exception>
        public static void DeleteSessionProperty(this HttpRequestMessage request, string key)
            => request.GetSessionContext().Delete(key);

        /// <summary>
        /// Clears all properties from the current session.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> instance.</param>
        public static void ClearSession(this HttpRequestMessage request)
            => request.GetSessionContext().Clear();

        private static SessionContext GetSessionContext(this HttpRequestMessage request)
            => request.GetOwinContext().GetSessionContext();
    }
}
