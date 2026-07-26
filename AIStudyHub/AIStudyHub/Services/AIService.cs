using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AIStudyHub.Data;
using AIStudyHub.Models;

namespace AIStudyHub.Services
{
    public class AIService
    {
        private static readonly string[] SummarizeKeywords =
            { "tóm tắt", "summarize", "summary", "tóm lược", "tổng hợp", "khái quát" };

        /// <summary>
        /// Answers a question using local RAG chunks, or uses Gemini File API for summarization requests.
        /// </summary>
        public async Task<string> AskQuestionAsync(Guid documentId, string question, List<ChatMessage> chatHistory)
        {
            using var db = new AppDbContext();

            var apiKey  = db.AppSettings.Find("ApiKey")?.Value;
            var endpoint  = db.AppSettings.Find("ApiEndpoint")?.Value ?? "https://generativelanguage.googleapis.com/v1beta/models/";
            var modelName = db.AppSettings.Find("AiModel")?.Value ?? "gemma-4-31b-it";

            if (string.IsNullOrWhiteSpace(apiKey))
                return "Lỗi: Chưa thiết lập API Key. Vui lòng vào màn hình Cài đặt (Settings) để nhập API Key.";

            bool isSummarizeRequest = SummarizeKeywords.Any(k =>
                question.Contains(k, StringComparison.OrdinalIgnoreCase));

            // ── SUMMARIZE PATH: Upload whole file to Gemini File API ─────────────
            if (isSummarizeRequest && documentId != Guid.Empty)
            {
                var doc = db.Documents.Find(documentId);
                if (doc == null || !File.Exists(doc.FilePath))
                    return "Không tìm thấy file tài liệu trên máy. Vui lòng upload lại.";

                try
                {
                    // Convert Office → PDF offline (MiniPdf), PDF/TXT/MD returned as-is
                    var pdfPath = await Task.Run(() =>
                        OfficeConversionService.ConvertToPdfForUpload(doc.FilePath));

                    var mimeType = OfficeConversionService.GetMimeType(pdfPath);

                    // Upload to Gemini File API
                    var fileUri = await UploadFileToGeminiAsync(pdfPath, mimeType, apiKey);

                    // Build generation request using the fileUri
                    var contents = BuildChatHistory(chatHistory, question,
                        fileDataUri: fileUri, mimeType: mimeType);

                    string systemPrompt =
                        "Bạn là một gia sư AI thân thiện và chuyên nghiệp. " +
                        "Người dùng đã cung cấp một tài liệu học tập. Hãy tóm tắt nội dung chính của tài liệu một cách rõ ràng, có cấu trúc, dễ hiểu.";

                    return await CallGeminiAsync(contents, systemPrompt, endpoint, modelName, apiKey);
                }
                catch (Exception ex)
                {
                    return $"Lỗi khi tải file lên Gemini: {ex.Message}";
                }
            }

            // ── SUMMARIZE with no document open ──────────────────────────────────
            if (isSummarizeRequest && documentId == Guid.Empty)
                return "Vui lòng mở một tài liệu trước khi yêu cầu tóm tắt.";

            // ── NORMAL RAG PATH ───────────────────────────────────────────────────
            var keywords = question
                .Split(new[] { ' ', ',', '.', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(k => k.Length > 2)
                .ToList();

            string contextText = string.Empty;
            var chunksQuery = db.DocumentChunks.Where(c => c.DocumentId == documentId);

            if (keywords.Any())
            {
                var matchingChunks = chunksQuery.AsEnumerable()
                    .Where(c => keywords.Any(k => c.Content.Contains(k, StringComparison.OrdinalIgnoreCase)))
                    .Take(5)
                    .Select(c => c.Content);
                contextText = string.Join("\n\n", matchingChunks);
            }

            if (string.IsNullOrWhiteSpace(contextText))
            {
                // Fallback: lấy 3 đoạn đầu
                contextText = string.Join("\n\n", chunksQuery.Take(3).Select(c => c.Content));
            }

            string ragSystemPrompt = $"Bạn là một gia sư AI thân thiện. Dưới đây là nội dung tài liệu trích xuất:\n{contextText}\n\nHãy trả lời câu hỏi của người dùng. Nếu câu hỏi liên quan tới tài liệu, hãy dựa vào tài liệu. Nếu câu hỏi là kiến thức chung, hãy dùng kiến thức của bạn.";

            var ragContents = BuildChatHistory(chatHistory, question);
            return await CallGeminiAsync(ragContents, ragSystemPrompt, endpoint, modelName, apiKey);
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Uploads a local file to the Gemini File API and returns the fileUri.
        /// </summary>
        private static async Task<string> UploadFileToGeminiAsync(string filePath, string mimeType, string apiKey)
        {
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileName  = Path.GetFileName(filePath);

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(3);

            // Step 1: Initiate resumable upload
            var initUrl = $"https://generativelanguage.googleapis.com/upload/v1beta/files?uploadType=resumable&key={apiKey}";
            var initBody = JsonSerializer.Serialize(new
            {
                file = new { display_name = fileName }
            });

            var initRequest = new HttpRequestMessage(HttpMethod.Post, initUrl);
            initRequest.Content = new StringContent(initBody, Encoding.UTF8, "application/json");
            initRequest.Headers.Add("X-Goog-Upload-Protocol", "resumable");
            initRequest.Headers.Add("X-Goog-Upload-Command", "start");
            initRequest.Headers.Add("X-Goog-Upload-Header-Content-Length", fileBytes.Length.ToString());
            initRequest.Headers.Add("X-Goog-Upload-Header-Content-Type", mimeType);

            var initResponse = await httpClient.SendAsync(initRequest);
            if (!initResponse.IsSuccessStatusCode)
            {
                var errBody = await initResponse.Content.ReadAsStringAsync();
                throw new Exception($"Initiate upload failed ({initResponse.StatusCode}): {errBody}");
            }

            // Step 2: Upload the actual bytes
            var uploadUrl = initResponse.Headers.GetValues("X-Goog-Upload-URL").FirstOrDefault()
                         ?? throw new Exception("Upload URL not returned by Gemini File API.");

            var uploadRequest = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            uploadRequest.Content = new ByteArrayContent(fileBytes);
            uploadRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            uploadRequest.Headers.Add("X-Goog-Upload-Command", "upload, finalize");
            uploadRequest.Headers.Add("X-Goog-Upload-Offset", "0");

            var uploadResponse = await httpClient.SendAsync(uploadRequest);
            var uploadBody = await uploadResponse.Content.ReadAsStringAsync();

            if (!uploadResponse.IsSuccessStatusCode)
                throw new Exception($"Upload failed ({uploadResponse.StatusCode}): {uploadBody}");

            using var jsonDoc = JsonDocument.Parse(uploadBody);
            var fileUri = jsonDoc.RootElement
                .GetProperty("file")
                .GetProperty("uri")
                .GetString()
                ?? throw new Exception("fileUri not found in upload response.");

            return fileUri;
        }

        /// <summary>
        /// Builds the 'contents' array for a Gemini API request.
        /// If fileDataUri is provided, it is prepended as a file_data part.
        /// </summary>
        private static List<object> BuildChatHistory(List<ChatMessage> history, string currentQuestion,
            string? fileDataUri = null, string? mimeType = null)
        {
            var contents = new List<object>();

            var recentHistory = history.Skip(Math.Max(0, history.Count - 10)).ToList();

            foreach (var msg in recentHistory)
            {
                contents.Add(new
                {
                    role  = msg.Role == "user" ? "user" : "model",
                    parts = new[] { new { text = msg.Content } }
                });
            }

            // Current user turn — optionally prepend file reference
            if (!string.IsNullOrEmpty(fileDataUri) && !string.IsNullOrEmpty(mimeType))
            {
                contents.Add(new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { file_data = new { mime_type = mimeType, file_uri = fileDataUri } },
                        new { text = currentQuestion }
                    }
                });
            }
            else
            {
                contents.Add(new
                {
                    role  = "user",
                    parts = new[] { new { text = currentQuestion } }
                });
            }

            return contents;
        }

        /// <summary>
        /// Calls the Gemini generateContent endpoint and extracts the answer text.
        /// </summary>
        private static async Task<string> CallGeminiAsync(
            List<object> contents, string systemPrompt,
            string endpoint, string modelName, string apiKey)
        {
            var requestBody = new
            {
                contents,
                systemInstruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            string requestUrl = $"{endpoint.TrimEnd('/')}/{modelName}:generateContent?key={apiKey}";

            using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };
            try
            {
                var response = await httpClient.PostAsync(requestUrl, jsonContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && responseString.Contains("exceeds the maximum number of tokens allowed"))
                    {
                        return "Tài liệu quá lớn so với giới hạn của mô hình AI hiện tại. Vui lòng đặt các câu hỏi cụ thể hơn để AI tìm kiếm, hoặc thử đổi sang mô hình có giới hạn lớn hơn trong phần Cài đặt.";
                    }
                    return $"Lỗi API: {response.StatusCode}\n{responseString}";
                }

                using var jsonDoc = JsonDocument.Parse(responseString);
                var parts = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts");

                var answerBuilder = new StringBuilder();
                foreach (var part in parts.EnumerateArray())
                {
                    bool isThought = part.TryGetProperty("thought", out var thoughtProp) && thoughtProp.GetBoolean();
                    if (!isThought && part.TryGetProperty("text", out var textProp))
                        answerBuilder.Append(textProp.GetString());
                }

                var textResult = answerBuilder.ToString();
                return string.IsNullOrWhiteSpace(textResult)
                    ? "Không có câu trả lời từ AI."
                    : textResult.Trim();
            }
            catch (Exception ex)
            {
                return $"Lỗi kết nối: {ex.Message}";
            }
        }
    }
}
