using AISupporter.ExternalService.AI.Interfaces.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISupporter.ExternalService.AI.Interfaces.Model
{
    public class AIChatMessage
    {
        public AIChatMessageRole Role { get; private set; }
        public string Content { get; private set; }
        public string? Name { get; private set; }
        public Dictionary<string, object>? Metadata { get; private set; }
        public DateTime ChatTime { get; private set; }
        public string? Model { get; private set; }
        public string? ImagePath { get; private set; }

        public AIChatMessage(AIChatMessageRole role, string content, string? name = null, string? imagePath = null)
        {
            Role = role;
            Content = content;
            Name = name;
            ImagePath = imagePath;
            ChatTime = DateTime.UtcNow;
            Metadata = new Dictionary<string, object>();
        }

        public static AIChatMessage CreateSystemMessage(string content)
        {
            return new AIChatMessage(AIChatMessageRole.System, content);
        }

        public static AIChatMessage CreateUserMessage(string content, string? imagePath = null)
        {
            return new AIChatMessage(AIChatMessageRole.User, content, imagePath: imagePath);
        }

        public static AIChatMessage CreateAssistantMessage(string content, string? model = null)
        {
            return new AIChatMessage(AIChatMessageRole.Assistant, content)
            {
                Model = model
            };
        }

        public bool HasImage => !string.IsNullOrEmpty(ImagePath);
    }
}
