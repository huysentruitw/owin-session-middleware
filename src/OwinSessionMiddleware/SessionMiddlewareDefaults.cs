﻿using System;
using System.Security.Cryptography;

namespace OwinSessionMiddleware
{
    /// <summary>
    /// Default values used by <see cref="SessionMiddlewareOptions"/>.
    /// </summary>
    public static class SessionMiddlewareDefaults
    {
        /// <summary>
        /// The default cookie name.
        /// </summary>
        public const string CookieName = "osm.sid";

        /// <summary>
        /// The default unique session id generator based on a unique <see cref="Guid"/> combined with a random part generated by <see cref="RNGCryptoServiceProvider"/>.
        /// </summary>
        /// <returns>A unique session id. Maximum length of the resulting string is 45 characters.</returns>
        public static string UniqueSessionIdGenerator()
        {
            var random = new byte[8];
            using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(random);
            return $"{Guid.NewGuid():N}.{Convert.ToBase64String(random)}";
        }
    }
}
