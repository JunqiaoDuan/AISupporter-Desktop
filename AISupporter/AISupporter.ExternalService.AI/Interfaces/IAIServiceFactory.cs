using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISupporter.ExternalService.AI.Interfaces
{
    public interface IAIServiceFactory
    {
        IAIService GetService(string providerName);
        IAIService GetDefaultService();
    }
}
