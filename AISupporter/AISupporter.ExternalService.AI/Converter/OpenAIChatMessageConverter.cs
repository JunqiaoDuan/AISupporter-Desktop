using AISupporter.ExternalService.AI.Interfaces.Enum;
using AISupporter.ExternalService.AI.Interfaces.Model;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISupporter.ExternalService.AI.Converter
{
    public class OpenAIChatMessageConverter
    {
        public ChatMessage ConvertToModelFormat(AIChatMessage message)
        {
            return message.Role switch
            {
                AIChatMessageRole.System => ChatMessage.CreateSystemMessage(message.Content),
                AIChatMessageRole.User => ChatMessage.CreateUserMessage(message.Content),
                AIChatMessageRole.Assistant => ChatMessage.CreateAssistantMessage(message.Content),
                _ => throw new ArgumentException($"Unsupported role: {message.Role}")
            };
        }

        public List<ChatMessage> ConvertToModelFormat(List<AIChatMessage> messages)
        {
            return messages.Select(ConvertToModelFormat).ToList();
        }

    }
}
