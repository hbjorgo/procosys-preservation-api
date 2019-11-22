﻿using Equinor.Procosys.Preservation.Command;
using Equinor.Procosys.Preservation.Command.EventHandlers;
using Equinor.Procosys.Preservation.Command.IntegrationEventHandlers;
using Equinor.Procosys.Preservation.Domain;
using Equinor.Procosys.Preservation.Domain.Events;
using Equinor.Procosys.Preservation.Domain.IntegrationEvents;
using Equinor.Procosys.Preservation.Infrastructure;
using Equinor.Procosys.Preservation.Messaging;
using Equinor.Procosys.Preservation.WebApi.Middleware;
using Equinor.Procosys.Preservation.WebApi.Misc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.Procosys.Preservation.WebApi.DIModules
{
    public static class ApplicationModule
    {
        public static void AddApplicationModules(this IServiceCollection services, string dbConnectionString)
        {
            services.AddDbContext<PreservationContext>(options =>
            {
                options.UseSqlServer(dbConnectionString);
            });

            // Transient - Created each time it is requested from the service container
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IPlantProvider, PlantProvider>();

            // Scoped - Created once per client request (connection)
            services.AddScoped<IReadOnlyContext, PreservationContext>();
            services.AddScoped<IUnitOfWork, PreservationContext>();
            services.AddScoped<IEventDispatcher, EventDispatcher>();

            // Singleton - Created the first time they are requested
            services.AddSingleton<ITimeService, TimeService>();
            services.AddSingleton<IMessageSender, AzureServiceBusSender>();
            services.AddSingleton<IMessageReceiver, AzureServiceBusReceiver>();
            services.AddSingleton<IMessageHandler, MessageHandler>();
        }
    }
}
