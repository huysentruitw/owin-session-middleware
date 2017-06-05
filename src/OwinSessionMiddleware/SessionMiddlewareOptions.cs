namespace OwinSessionMiddleware
{
    public class SessionMiddlewareOptions
    {
        public static class Defaults
        {
            public const string CookieName = "sid";
        }

        public string CookieName { get; set; } = Defaults.CookieName;
    }
}
