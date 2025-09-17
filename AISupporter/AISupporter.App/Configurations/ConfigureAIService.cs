using AISupporter.ExternalService.AI;
using AISupporter.ExternalService.AI.Interfaces;
using AISupporter.ExternalService.AI.Interfaces.Model;
using AISupporter.ExternalService.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISupporter.App.Configurations
{
    public static class ConfigureAIService
    {
        public static IServiceCollection AddAIService(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            #region Model service

            services.AddSingleton<IAIModelService, AIModelService>();

            #endregion

            #region Open AI

            var openAIendpoint = configuration["OpenAI:EndPoint"];
            var openAISecretKey = configuration["OpenAI:SecretKey"];

            services.AddSingleton<OpenAIService>(sp =>
                new OpenAIService(new AIServiceContext()
                {
                    EndPoint = openAIendpoint,
                    SecretKey = openAISecretKey
                })
            );

            #endregion

            #region Gemini

            #endregion

            // Register keyed services for IAIService
            services.AddKeyedSingleton<IAIService, OpenAIService>(AIConst.OpenAI, (sp, _) => sp.GetRequiredService<OpenAIService>());

            // Register the AIServiceFactory
            services.AddSingleton<IAIServiceFactory, AIServiceFactory>();

            return services;
        }
    }
}
