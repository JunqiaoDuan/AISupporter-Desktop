using AISupporter.ExternalService.AI.Interfaces.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISupporter.ExternalService.AI.Interfaces
{
    public interface IAIModelService
    {
        Task<List<AIModel>> GetAllModelsAsync();
        Task<List<AIModel>> GetAllEnableModelsAsync();
        Task<AIModel?> GetDefaultModel(string? modelCode);
    }
}
