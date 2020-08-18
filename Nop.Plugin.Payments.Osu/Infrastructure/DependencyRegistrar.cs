﻿using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Payments.Osu.Services;

namespace Nop.Plugin.Payments.Osu.Infrastructure
{
    /// <summary>
    /// Represents a plugin dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<DefaultPaymentInfoFactory>().As<IPaymentInfoFactory>().InstancePerLifetimeScope();
            builder.RegisterType<DefaultWidgetScriptFactory>().As<IWidgetScriptFactory>().InstancePerLifetimeScope();
            builder.RegisterType<DefaultOsuPaymentService>().As<IOsuPaymentService>().InstancePerLifetimeScope();
            builder.RegisterType<DefaultWebHookPaymentProcessor>().As<IWebHookPaymentProcessor>().InstancePerLifetimeScope();
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 1;
    }
}
