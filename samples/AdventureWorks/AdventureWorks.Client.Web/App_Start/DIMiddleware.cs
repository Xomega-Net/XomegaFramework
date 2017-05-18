using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xomega.Framework;
using Xomega.Framework.Web;

namespace AdventureWorks.Client.Web
{
    using Environment = IDictionary<string, object>;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class DIMiddleware
    {
        private readonly AppFunc next;

        public DIMiddleware(AppFunc next)
        {
            this.next = next;
        }

        public async Task Invoke(Environment context)
        {
            // set current scope before request
            WebDI.CurrentServiceScope = DI.DefaultServiceProvider.CreateScope();

            // invoke request handler
            await this.next.Invoke(context);

            // clean up scope after request
            var scope = WebDI.CurrentServiceScope;
            if (scope != null) scope.Dispose();
        }
    }
}