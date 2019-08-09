using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Quill.WebApi.Service;

namespace Quill.WebApi.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes());
            ServiceContainer.Initialize(types, (builder) =>
            {
                builder.RegisterType<DBAccess.ChannelApiContext>()
                       .As<DBAccess.ChannelApiContext>()
                       .InstancePerLifetimeScope();

                // 注册主项目的Controllers
                builder.RegisterControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();
            });
            DependencyResolver.SetResolver(new AutofacDependencyResolver(ServiceContainer.Container));
        }
    }
}
