using System.Text.Json;
using System.Text;

namespace LibraryService.Services
{
    public class ShortDescription
    {
        public static async Task<string> ShortenTextWithAI(string title)
        {
            //Настройка
            string apiKey = "sk-391de3fb5e8848149ec053af2d11b3ec";
            string apiUrl = "https://api.deepseek.com/chat/completions";
            //Получение пути
            string path = AppDomain.CurrentDomain.BaseDirectory + "BooksDescription\\" + title + ".txt";
            string text = File.ReadAllText(path);
            if (text == "")
                throw new Exception("There's no book with that title");
            //Запрос
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // 3. Формирование запроса
            var requestData = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                new { role = "system", content = "Сократи текст в 2-3 раза, сохраняя суть" },
                new { role = "user", content = text }
            },
                max_tokens = 2000,
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 4. Отправка запроса и обработка ответа
            using var response = await client.PostAsync(apiUrl, content);
            string responseString = await response.Content.ReadAsStringAsync();

            // Проверка статуса
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"API error: {response.StatusCode}\n{responseString}");
            }

            // 5. Парсинг ответа с проверкой структуры
            using JsonDocument doc = JsonDocument.Parse(responseString);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("choices", out JsonElement choices) && choices.GetArrayLength() > 0)
            {
                if (choices[0].TryGetProperty("message", out JsonElement message) &&
                message.TryGetProperty("content", out JsonElement contentElement))
                {
                    return contentElement.GetString();
                }
            }
            // Если не удалось распарсить
            throw new InvalidOperationException($"Invalid API response:\n{responseString}");
        }
    }
}