using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISupporter.ExternalService.AI.Interfaces.Model
{
    public class AIModel
    {
        public AIModel(string aiProvider, string code, string dispalyName, bool needAuth, bool enable)
        {
            Provider = aiProvider;
            Code = code;
            DisplayName = dispalyName;
            NeedAuth = needAuth;
            Enable = enable;
        }

        public string Provider { get; set; }
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public bool NeedAuth { get; set; }
        public bool Enable { get; set; }
    }
}
