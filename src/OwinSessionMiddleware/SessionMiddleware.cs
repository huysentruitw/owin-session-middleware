using System.Threading.Tasks;
using Microsoft.Owin;

namespace OwinSessionMiddleware
{
    public class SessionMiddleware : OwinMiddleware
    {
        public SessionMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {


            await Next.Invoke(context);
        }
    }
}
