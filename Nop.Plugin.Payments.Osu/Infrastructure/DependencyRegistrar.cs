﻿using Autofac;
using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="appSettings">App settings</param>
        public virtual void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<IPaymentInfoFactory, DefaultPaymentInfoFactory>();
            services.AddScoped<IWidgetScriptFactory, DefaultWidgetScriptFactory>();
            services.AddScoped<IOsuPaymentService, DefaultOsuPaymentService>();
            services.AddScoped<IWebHookPaymentProcessor, DefaultWebHookPaymentProcessor>();
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 1;
    }
}
