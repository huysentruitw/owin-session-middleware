using System;
using System.Security.Cryptography;

namespace OwinSessionMiddleware
{
    public class SessionMiddlewareOptions
    {
        public static class Defaults
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

        public string CookieName { get; set; } = Defaults.CookieName;

        public string CookieDomain { get; set; } = null;

        public TimeSpan? CookieLifetime { get; set; } = null;

        public bool UseSecureCookie { get; set; } = true;

        public string SessionContextOwinEnvironmentKey { get; set; } = Defaults.SessionContextOwinEnvironmentKey;

        public ISessionStore Store { get; set; } = new InMemorySessionStore();

        public Func<string> UniqueSessionIdGenerator { get; set; } = Defaults.UniqueSessionIdGenerator;
    }
}
