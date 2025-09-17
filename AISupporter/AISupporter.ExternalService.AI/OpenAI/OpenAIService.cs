using AISupporter.ExternalService.AI.Converter;
using AISupporter.ExternalService.AI.Interfaces;
using AISupporter.ExternalService.AI.Interfaces.Enum;
using AISupporter.ExternalService.AI.Interfaces.Model;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISupporter.ExternalService.AI.OpenAI
{
    public class OpenAIService : IAIService
    {
        private AIServiceContext _context;
        private OpenAIChatMessageConverter _chatMessageConverter;
        private readonly string _systemPrompt;

        public OpenAIService(AIServiceContext context, IConfiguration configuration)
        {
            _context = context;
            _chatMessageConverter = new OpenAIChatMessageConverter();
            _systemPrompt = configuration["OpenAI:AISystemPrompt"] ?? string.Empty;
        }

        public void Test()
        {
            var _secretKey = _context.SecretKey;
            var _endpoint = _context.EndPoint;

            var chatClient = new ChatClient(
              "gpt-4.1",
              new ApiKeyCredential(_secretKey),
              new OpenAIClientOptions
              {
                  Endpoint = new Uri(_endpoint),
              }
            );

            TestWithLocalImage(chatClient, @"E:\Repos\Release\AISupporter-Desktop\AISupporter\AISupporter.App\bin\Debug\net8.0-windows\ScreenCapture\screen_capture_20250916_170207_1759.png");
        }

        private void TestWithLocalImage(ChatClient chatClient, string imagePath)
        {
            try
            {
                var imageResult = ProcessImage(imagePath);
                if (imageResult == null)
                {
                    Console.WriteLine($"Image file not found: {imagePath}");
                    return;
                }

                var (imageBytes, mimeType) = imageResult.Value;

                var messages = new List<ChatMessage>
        {
            ChatMessage.CreateUserMessage(
                ChatMessageContentPart.CreateTextPart("Describe this image in detail."),
                ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(imageBytes), mimeType)
            )
        };

                ChatCompletion completion = chatClient.CompleteChat(messages);
                Console.WriteLine($"[ASSISTANT]: {completion.Content[0].Text}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task ChatAsync(List<AIChatMessage> messages, string modelCode)
        {
            // Prepend the system prompt as the first message
            if (!messages.Any() || messages.First().Role != AIChatMessageRole.System)
            {
                messages.Insert(0, AIChatMessage.CreateSystemMessage(_systemPrompt));
            }

            var _secretKey = _context.SecretKey;
            var _endpoint = _context.EndPoint;

            var chatClient = new ChatClient(
              modelCode,
              new ApiKeyCredential(_secretKey),
              new OpenAIClientOptions
              {
                  Endpoint = new Uri(_endpoint),
              }
            );

            var openAIMessages = _chatMessageConverter.ConvertToModelFormat(messages);

            var lastMessage = messages.LastOrDefault();
            if (lastMessage?.HasImage == true)
            {
                var imageResult = ProcessImage(lastMessage.ImagePath);
                if (imageResult != null)
                {
                    var (imageBytes, mimeType) = imageResult.Value;
                    var lastOpenAIMessage = openAIMessages.Last();
                    openAIMessages.RemoveAt(openAIMessages.Count - 1);

                    var messageWithImage = ChatMessage.CreateUserMessage(
                        ChatMessageContentPart.CreateTextPart(lastMessage.Content),
                        ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(imageBytes), mimeType)
                    );

                    openAIMessages.Add(messageWithImage);
                }
            }

            ChatCompletion completion = await chatClient.CompleteChatAsync(openAIMessages);
            var newMessage = completion.Content[0].Text;
            messages.Add(AIChatMessage.CreateAssistantMessage(newMessage));
        }

        private (byte[] ImageBytes, string MimeType)? ProcessImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return null;

            byte[] imageBytes = File.ReadAllBytes(imagePath);
            string mimeType = Path.GetExtension(imagePath).ToLower() switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/png"
            };
            return (imageBytes, mimeType);
        }
    }
}
