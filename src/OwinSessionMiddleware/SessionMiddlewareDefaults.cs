using System;
using System.Security.Cryptography;

namespace OwinSessionMiddleware
{
    public static class SessionMiddlewareDefaults
    {
        public const string CookieName = "osm.sid";

        public const string SessionContextOwinEnvironmentKey = "OSM.SessionContext";

        public static string UniqueSessionIdGenerator()
        {
            var random = new byte[8];
            using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(random);
            return $"{Guid.NewGuid():N}.{Convert.ToBase64String(random)}";
        }
    }
}
