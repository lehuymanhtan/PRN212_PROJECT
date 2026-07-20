using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AIStudyHub.Data;
using AIStudyHub.Models;

namespace AIStudyHub.Services
{
    public class AIService
    {
        public async Task<string> AskQuestionAsync(Guid documentId, string question, List<ChatMessage> chatHistory)
        {
            using var db = new AppDbContext();
            
            var apiKey = db.AppSettings.Find("ApiKey")?.Value;
            var endpoint = db.AppSettings.Find("ApiEndpoint")?.Value ?? "https://generativelanguage.googleapis.com/v1beta/models/";
            var modelName = db.AppSettings.Find("AiModel")?.Value ?? "gemma-4-31b-it";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "Lỗi: Chưa thiết lập API Key. Vui lòng vào màn hình Cài đặt (Settings) để nhập API Key.";
            }

            // Keyword Search (RAG Step 1)
            var keywords = question.Split(new[] { ' ', ',', '.', '?' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Where(k => k.Length > 2)
                                   .ToList();

            string contextText = "";
            var chunksQuery = db.DocumentChunks.Where(c => c.DocumentId == documentId);
            
            if (keywords.Any())
            {
                // Lấy 5 chunk có chứa nhiều từ khóa nhất
                var matchingChunks = chunksQuery.AsEnumerable()
                                          .Where(c => keywords.Any(k => c.Content.Contains(k, StringComparison.OrdinalIgnoreCase)))
                                          .Take(5)
                                          .Select(c => c.Content);
                contextText = string.Join("\n\n", matchingChunks);
            }

            if (string.IsNullOrWhiteSpace(contextText))
            {
                // Fallback nếu không có từ khóa nào khớp, lấy 3 đoạn đầu tiên
                contextText = string.Join("\n\n", chunksQuery.Take(3).Select(c => c.Content));
            }

            string systemPrompt = $"Bạn là một gia sư AI thân thiện. Dưới đây là nội dung tài liệu trích xuất:\n{contextText}\n\nHãy trả lời câu hỏi của người dùng. Nếu câu hỏi liên quan tới tài liệu, hãy dựa vào tài liệu. Nếu câu hỏi là kiến thức chung, hãy dùng kiến thức của bạn.";

            // Xây dựng JSON Request cho Gemini API (Gemini format)
            var contents = new List<object>();
            
            // Lịch sử Chat (nếu quá dài, chỉ lấy 10 tin nhắn gần nhất để khỏi tràn Token)
            var recentHistory = chatHistory.Skip(Math.Max(0, chatHistory.Count - 10)).ToList();
            
            foreach(var msg in recentHistory)
            {
                contents.Add(new {
                    role = msg.Role == "user" ? "user" : "model",
                    parts = new[] { new { text = msg.Content } }
                });
            }
            
            // Câu hỏi hiện tại
            contents.Add(new {
                role = "user",
                parts = new[] { new { text = question } }
            });

            var requestBody = new {
                contents = contents,
                systemInstruction = new {
                    parts = new[] { new { text = systemPrompt } }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            string requestUrl = $"{endpoint.TrimEnd('/')}/{modelName}:generateContent?key={apiKey}";

            using var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.PostAsync(requestUrl, jsonContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"Lỗi API: {response.StatusCode}\n{responseString}";
                }

                using var jsonDoc = JsonDocument.Parse(responseString);
                var parts = jsonDoc.RootElement
                                .GetProperty("candidates")[0]
                                .GetProperty("content")
                                .GetProperty("parts");

                // Gemma 4 thinking models return multiple parts:
                // parts with "thought": true are internal reasoning — skip them.
                // Only collect parts without the "thought" field (or where it is false).
                var answerBuilder = new StringBuilder();
                foreach (var part in parts.EnumerateArray())
                {
                    bool isThought = part.TryGetProperty("thought", out var thoughtProp) && thoughtProp.GetBoolean();
                    if (!isThought && part.TryGetProperty("text", out var textProp))
                    {
                        answerBuilder.Append(textProp.GetString());
                    }
                }

                var textResult = answerBuilder.ToString();
                if (string.IsNullOrWhiteSpace(textResult))
                    return "Không có câu trả lời từ AI.";

                return textResult.Trim();
            }
            catch (Exception ex)
            {
                return $"Lỗi kết nối: {ex.Message}";
            }
        }
    }
}
