using AISupporter.ExternalService.AI.Interfaces.Model;
using AISupporter.ExternalService.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISupporter.ExternalService.AI
{
    public class AIModelService : IAIModelService
    {
        public async Task<List<AIModel>> GetAllEnableModelsAsync()
        {
            var models = await GetAllModelsAsync();

            return models
                .Where(i => i.Enable == true)
                .ToList();
        }

        public Task<List<AIModel>> GetAllModelsAsync()
        {
            var models = new List<AIModel>()
            {
                new AIModel(AIConst.OpenAI, "gpt-4.1", "GPT-4.1", true, true),
                new AIModel(AIConst.OpenAI, "gpt-5", "GPT-5", true, true),

                new AIModel(AIConst.OpenAI, "gpt-5-nano", "GPT-5-nano", false, true),
                new AIModel(AIConst.OpenAI, "gpt-4.1-nano", "GPT-4.1-nano", false, true)
            };

            return Task.FromResult(models);
        }

        public async Task<AIModel?> GetDefaultModel(string? modelCode)
        {
            var models = await GetAllEnableModelsAsync();
            var model = models.FirstOrDefault(i => i.Code == modelCode);

            if (model != null)
            {
                return model;
            }

            return models.FirstOrDefault();
        }
    }
}
