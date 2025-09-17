using AISupporter.ExternalService.AI.Interfaces.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISupporter.ExternalService.AI.Interfaces
{
    public interface IAIService
    {
        void Test();
        Task ChatAsync(List<AIChatMessage> messages, string modelCode);
    }
}
