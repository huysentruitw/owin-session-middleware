# OWIN Session middleware

[![Build status](https://ci.appveyor.com/api/projects/status/r94rbm7d2dksmi0d/branch/master?svg=true)](https://ci.appveyor.com/project/huysentruitw/owin-session-middleware/branch/master)

OWIN middleware for working with session-cookies and session properties.

A session is a way to store information (properties) to be used across multiple requests.

While it does use a cookie for remembering the session id inside the users browser, the information/properties itself is stored in a back-end store.

The project comes with an in-memory session store, but can easily be replaced by a custom implementation.

**REMARK** This library is only for those who don't want to depend on `System.Web` or `HttpContext.Current` and want a clean OWIN-only solution.

# Get it on NuGet

    PM> Install-Package Install-Package OwinSessionMiddleware

or, if you want WebAPI integration

    PM> Install-Package Install-Package OwinSessionMiddleware.WebApi

# Register the middleware

In its simplest form, no extra parameters are required as the defaults will fit many simple projects:

```C#
public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        app.UseSessionMiddleware();

        // other middleware registrations...
        app.UseWebApi();
    }
}
```

# Options

Options are set by passing an instance of [SessionMiddlewareOptions](https://github.com/huysentruitw/owin-session-middleware/blob/master/src/OwinSessionMiddleware/SessionMiddlewareOptions.cs) to the [`UseSessionMiddleware`](https://github.com/huysentruitw/owin-session-middleware/blob/master/src/OwinSessionMiddleware/Extensions/SessionMiddlewareAppBuilderExtensions.cs) extension method.

```C#
var options = new SessionMiddlewareOptions
{
    CookieName = "MyFancyCookieName"
};
app.UseSessionMiddleware(options);
```

## CookieName

Changes the name of the cookie that will be returned by the middleware. This option defaults to `osm.sid`.

## CookieDomain

Adds a `domain` value to the cookie that will be returned by the middleware. This option defaults to `null` which will not add a domain to the cookie.

## CookieLifetime

Adds an `expires` value to the cookie that will be returned by the middleware. This option defaults to `null` which will not add an expires value to the cookie, so it will be valid for the current browser session only.

Please note that browsers configured to remember open tabs, often store session cookies and recall them when the user re-opens the browser.

## UseSecureCookie

Adds the `secure` flag to the cookie that will be returned by the middleware. This option defaults to `true` which means that the browser will only sent the cookie for secure URLs (thus, for https:// and not for http://).

## UseHttpOnlyCookie

While this would be a possible option candidate, I decided not to include it because making the cookie available to JavaScript makes it vulnarable for XSS attacks. Therefore the `HttpOnly` flag is always set for the cookie.

## Store

Changes the session store that will be used to store sessions and their properties. This option defauls to an instance of [`InMemorySessionStore`](https://github.com/huysentruitw/owin-session-middleware/blob/master/src/OwinSessionMiddleware/InMemorySessionStore.cs).

Any class that implements [`ISessionStore`](https://github.com/huysentruitw/owin-session-middleware/blob/master/src/OwinSessionMiddleware/ISessionStore.cs) interface can be used.

## UniqueSessionIdGenerator

A delegate for generating unique session id's. The default generator combines a `Guid` for uniqueness with a random byte sequence for randomness which should be good for most applications.

# Reading/writing session properties

The base library adds an extension method to `IOwinContext` for getting the current session.

Once you have an instance of `IOwinContext`, you can get access to the session context.

From OWIN middleware, you can access the current session like this:

```C#
app.Use(async (ctx, next) =>
{
    var sessionContext = ctx.GetSessionContext();

    var requestCount = sessionContext.Get<int>("RequestCount");

    sessionContext.AddOrUpdate("RequestCount", ++requestCount);

    await next();
});
```

From a controller, you could use `HttpContext.Current.GetOwinContext().GetSessionContext()` to get the context.

If you're using this inside an `ApiController`, consider using the OwinSessionMiddleware.WebApi package which has some convenient extension methods you can use inside your controller actions:

```C#
public IHttpActionResult MyAction()
{
    var requestCount = Request.GetSessionProperty<int>("RequestCount");

    Request.SetSessionProperty("RequestCount", ++requestCount);
}
```
