using AISupporter.ExternalService.AI.Converter;
using AISupporter.ExternalService.AI.Interfaces;
using AISupporter.ExternalService.AI.Interfaces.Model;
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

        public OpenAIService(AIServiceContext context)
        {
            _context = context;
            _chatMessageConverter = new OpenAIChatMessageConverter();
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
                if (!File.Exists(imagePath))
                {
                    Console.WriteLine($"Image file not found: {imagePath}");
                    return;
                }

                byte[] imageBytes = File.ReadAllBytes(imagePath);
                string mimeType = Path.GetExtension(imagePath).ToLower() switch
                {
                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    _ => "image/png"
                };

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

            var openAIMessges = _chatMessageConverter.ConvertToModelFormat(messages);
            ChatCompletion completion = await chatClient.CompleteChatAsync(openAIMessges);

            var newMessage = completion.Content[0].Text;
            messages.Add(AIChatMessage.CreateAssistantMessage(newMessage));

        }

    }
}
