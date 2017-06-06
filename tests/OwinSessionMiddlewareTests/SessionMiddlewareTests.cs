using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Moq;
using NUnit.Framework;
using Owin;
using OwinSessionMiddleware;

namespace OwinSessionMiddlewareTests
{
    [TestFixture]
    public class SessionMiddlewareTests
    {
        private const string CookieRegex = @"^(?<name>[^=]+)=(?<value>[^;]+)(;\s*domain=(?<domain>[^;]+))?(;\s*path=(?<path>[^;]+))?(;\s*expires=(?<expires>[^;]+))?(;\s*(?<secure>secure))?(;\s*(?<httponly>HttpOnly))?$";
        private Mock<ISessionStore> _storeMock;

        [SetUp]
        public void SetUp()
        {
            _storeMock = new Mock<ISessionStore>();
        }

        [Test]
        public void SessionMiddlewareConstructor_NullOptions_ShouldThrowException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new SessionMiddleware(null, null));
            Assert.That(ex.ParamName, Is.EqualTo("options"));
        }

        [Test]
        public async Task SessionMiddleware_RequestWithoutSessionCookie_ShouldSetSessionCookieWithDefaultSettings()
        {
            using (var server = TestServer.Create(Startup()))
            {
                var response = await server.CreateRequest("/").GetAsync();
                var cookie = response.Headers.GetValues("Set-Cookie").First();

                var match = Regex.Match(cookie, CookieRegex);
                Assert.That(match.Success, Is.True, "Cookie didn't match regex");
                Assert.That(match.Groups["name"].Value, Is.EqualTo(SessionMiddlewareDefaults.CookieName));
                Assert.That(match.Groups["value"].Success, Is.True, "Value could not be extracted");
                Assert.That(match.Groups["domain"].Success, Is.False, "Domain should not be set");
                Assert.That(match.Groups["path"].Value, Is.EqualTo("/"), "Invalid path value");
                Assert.That(match.Groups["expires"].Success, Is.False, "Expires should not be set");
                Assert.That(match.Groups["secure"].Success, Is.True, "Secure flag not found");
                Assert.That(match.Groups["httponly"].Success, Is.True, "HttpOnly flag not found");

                var idParts = match.Groups["value"].Value.Split('.');
                Guid id;
                Assert.That(Guid.TryParse(idParts[0], out id), Is.True);
                var bytes = Convert.FromBase64String(WebUtility.UrlDecode(idParts[1]));
                Assert.That(bytes.Count, Is.EqualTo(8));
            }
        }

        [Test]
        public async Task SessionMiddleware_RequestWithSessionCookie_ShouldNotSetSessionCookie()
        {
            using (var server = TestServer.Create(Startup()))
            {
                var cookie = $"{SessionMiddlewareDefaults.CookieName}=09e160b22b2d4ab5b2d09d43ddf5e39d.y62hp8MafL0%3D";
                var response = await server.CreateRequest("/").AddHeader("Cookie", cookie).GetAsync();
                Assert.That(response.Headers.Contains("Set-Cookie"), Is.False);
            }
        }

        [Test]
        public async Task SessionMiddleware_ConfigureDifferentCookieDomain_ShouldSetCookieDomain()
        {
            var options = new SessionMiddlewareOptions();
            options.CookieDomain = ".google.com";
            using (var server = TestServer.Create(Startup(options)))
            {
                var response = await server.CreateRequest("/").GetAsync();
                var cookie = response.Headers.GetValues("Set-Cookie").First();

                var match = Regex.Match(cookie, CookieRegex);
                Assert.That(match.Success, Is.True, "Cookie didn't match regex");
                Assert.That(match.Groups["domain"].Value, Is.EqualTo(".google.com"));
                Assert.That(match.Groups["path"].Value, Is.EqualTo("/"), "Invalid path value");
                Assert.That(match.Groups["expires"].Success, Is.False, "Expires should not be set");
                Assert.That(match.Groups["secure"].Success, Is.True, "Secure flag not found");
                Assert.That(match.Groups["httponly"].Success, Is.True, "HttpOnly flag not found");
            }
        }

        [Test]
        public async Task SessionMiddleware_ConfigureInsecureCookie_ShouldSetCookieWithoutSecureFlag()
        {
            var options = new SessionMiddlewareOptions();
            options.UseSecureCookie = false;
            using (var server = TestServer.Create(Startup(options)))
            {
                var response = await server.CreateRequest("/").GetAsync();
                var cookie = response.Headers.GetValues("Set-Cookie").First();

                var match = Regex.Match(cookie, CookieRegex);
                Assert.That(match.Success, Is.True, "Cookie didn't match regex");
                Assert.That(match.Groups["domain"].Success, Is.False, "Domain should not be set");
                Assert.That(match.Groups["path"].Value, Is.EqualTo("/"), "Invalid path value");
                Assert.That(match.Groups["expires"].Success, Is.False, "Expires should not be set");
                Assert.That(match.Groups["secure"].Success, Is.False, "Secure flag was set");
                Assert.That(match.Groups["httponly"].Success, Is.True, "HttpOnly flag not found");
            }
        }

        [Test]
        public async Task SessionMiddleware_WonfigureCookieLifetime_ShouldSetCookieExpiration()
        {
            var options = new SessionMiddlewareOptions();
            options.CookieLifetime = TimeSpan.FromHours(1);
            using (var server = TestServer.Create(Startup(options)))
            {
                var response = await server.CreateRequest("/").GetAsync();
                var cookie = response.Headers.GetValues("Set-Cookie").First();

                var match = Regex.Match(cookie, CookieRegex);
                Assert.That(match.Success, Is.True, "Cookie didn't match regex");
                Assert.That(match.Groups["domain"].Success, Is.False, "Domain should not be set");
                Assert.That(match.Groups["path"].Value, Is.EqualTo("/"), "Invalid path value");
                Assert.That(match.Groups["expires"].Success, Is.True, "Expires not found");
                Assert.That(match.Groups["secure"].Success, Is.True, "Secure flag not found");
                Assert.That(match.Groups["httponly"].Success, Is.True, "HttpOnly flag not found");

                DateTimeOffset expiresAt;
                Assert.That(DateTimeOffset.TryParse(match.Groups["expires"].Value, out expiresAt), Is.True, "Failed to parse expires field");

                var difference = expiresAt - DateTime.UtcNow - TimeSpan.FromHours(1);
                Assert.That(difference.TotalSeconds, Is.GreaterThan(-1).And.LessThanOrEqualTo(0));
            }
        }

        [Test]
        public async Task SessionMiddleware_RequestWithoutSessionCookie_ShouldNotLookupSessionInStore()
        {
            var options = new SessionMiddlewareOptions();
            options.Store = _storeMock.Object;

            using (var server = TestServer.Create(Startup(options)))
                await server.CreateRequest("/").GetAsync();

            _storeMock.Verify(x => x.FindById(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SessionMiddleware_RequestWithSessionCookie_ShouldLookupSessionForGivenSessionIdInStore()
        {
            var options = new SessionMiddlewareOptions();
            options.Store = _storeMock.Object;

            using (var server = TestServer.Create(Startup(options)))
            {
                var cookie = $"{SessionMiddlewareDefaults.CookieName}=09e160b22b2d4ab5b2d09d43ddf5e39d.y62hp8MafL0%3D";
                await server.CreateRequest("/").AddHeader("Cookie", cookie).GetAsync();
            }

            _storeMock.Verify(x => x.FindById("09e160b22b2d4ab5b2d09d43ddf5e39d.y62hp8MafL0="), Times.Once);
        }

        [Test]
        public async Task SessionMiddleware_RequestWithoutSessionCookie_NotAddingProperties_ShouldNotAddSessionToStore()
        {
            var options = new SessionMiddlewareOptions();
            options.Store = _storeMock.Object;

            using (var server = TestServer.Create(Startup(options)))
                await server.CreateRequest("/").GetAsync();

            _storeMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>()), Times.Never);
            _storeMock.Verify(x => x.Update(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>()), Times.Never);
            _storeMock.Verify(x => x.Delete(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SessionMiddleware_RequestWithSessionCookie_NotAddingProperties_ShouldNotUpdateSessionInStore()
        {
            var options = new SessionMiddlewareOptions();
            options.Store = _storeMock.Object;

            using (var server = TestServer.Create(Startup(options)))
            {
                var cookie = $"{SessionMiddlewareDefaults.CookieName}=09e160b22b2d4ab5b2d09d43ddf5e39d.y62hp8MafL0%3D";
                await server.CreateRequest("/").AddHeader("Cookie", cookie).GetAsync();
            }

            _storeMock.Verify(x => x.Update(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>()), Times.Never);
            _storeMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>()), Times.Never);
            _storeMock.Verify(x => x.Delete(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SessionMiddleware_RequestWithoutSessionCookie_AddPropertyToSession_ShouldAddSessionToStore()
        {
            var options = new SessionMiddlewareOptions();
            options.Store = _storeMock.Object;
            options.UniqueSessionIdGenerator = () => "abc123";

            using (var server = TestServer.Create(Startup(options, ctx => ctx.GetSessionContext().AddOrUpdate("A", 1))))
                await server.CreateRequest("/").GetAsync();

            _storeMock.Verify(x => x.Add("abc123", new[] { Kvp("A", 1) }), Times.Once);
            _storeMock.Verify(x => x.Update(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>()), Times.Never);
            _storeMock.Verify(x => x.Delete(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SessionMiddleware_RequestWithSessionCookie_AddPropertyToSession_ShouldUpdateSessionInStore()
        {
            var options = new SessionMiddlewareOptions();
            options.Store = _storeMock.Object;

            using (var server = TestServer.Create(Startup(options, ctx => ctx.GetSessionContext().AddOrUpdate("B", 2))))
            {
                var cookie = $"{SessionMiddlewareDefaults.CookieName}=09e160b22b2d4ab5b2d09d43ddf5e39d.y62hp8MafL0%3D";
                await server.CreateRequest("/").AddHeader("Cookie", cookie).GetAsync();
            }

            _storeMock.Verify(x => x.Update("09e160b22b2d4ab5b2d09d43ddf5e39d.y62hp8MafL0=", new[] { Kvp("B", 2) }), Times.Once);
            _storeMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>()), Times.Never);
            _storeMock.Verify(x => x.Delete(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SessionMiddleware_RequestWithSessionCookie_RemoveLastProperty_ShouldDeleteSessionFromStore()
        {
            var options = new SessionMiddlewareOptions();
            options.Store = _storeMock.Object;

            using (var server = TestServer.Create(Startup(options, ctx =>
            {
                ctx.GetSessionContext().AddOrUpdate("B", 2);
                ctx.GetSessionContext().Delete("B");
            })))
            {
                var cookie = $"{SessionMiddlewareDefaults.CookieName}=09e160b22b2d4ab5b2d09d43ddf5e39d.y62hp8MafL0%3D";
                await server.CreateRequest("/").AddHeader("Cookie", cookie).GetAsync();
            }

            _storeMock.Verify(x => x.Update(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>()), Times.Never);
            _storeMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>()), Times.Never);
            _storeMock.Verify(x => x.Delete("09e160b22b2d4ab5b2d09d43ddf5e39d.y62hp8MafL0="), Times.Once);
        }

        private static KeyValuePair<string, object> Kvp(string key, object value)
            => new KeyValuePair<string, object>(key, value);

        private static Action<IAppBuilder> Startup(SessionMiddlewareOptions options = null, Action<IOwinContext> handler = null) =>
            app =>
            {
                app.UseSessionMiddleware(options);

                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.Path.Equals(new PathString("/")))
                    {
                        handler?.Invoke(ctx);
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        return;
                    }

                    await next();
                });
            };
    }
}
