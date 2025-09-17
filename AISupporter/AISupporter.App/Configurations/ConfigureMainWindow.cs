using AISupporter.ExternalService.AI.Interfaces.Model;
using AISupporter.ExternalService.AI.Interfaces;
using AISupporter.ExternalService.AI.OpenAI;
using AISupporter.ExternalService.AI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISupporter.App.Configurations
{
    public static class ConfigureMainWindow
    {
        public static IServiceCollection AddMainWindow(this IServiceCollection services)
        {
            services.AddTransient<MainWindow>();

            return services;
        }
    }
}
