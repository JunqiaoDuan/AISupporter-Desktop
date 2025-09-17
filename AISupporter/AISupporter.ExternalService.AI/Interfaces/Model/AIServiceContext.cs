using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISupporter.ExternalService.AI.Interfaces.Model
{
    public class AIServiceContext
    {
        public string EndPoint { get; set; } = "";
        public string SecretKey { get; set; } = "";
    }
}
