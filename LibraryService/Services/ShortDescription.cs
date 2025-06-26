using System.Text.Json;
using System.Text;

namespace LibraryService.Services
{
    public class ShortDescription
    {
        static async Task<string> ShortenTextWithAI(string title)
        {
            //Настройка
            string apiKey = "sk-391de3fb5e8848149ec053af2d11b3ec";
            string apiUrl = "https://api.deepseek.com/chat/completions";
            //Получение пути
            string path = AppDomain.CurrentDomain.BaseDirectory + "BooksDescription\"" + title + ".txt";
            string text = File.ReadAllText(path);
            if (text == "")
                throw new Exception("There's no book with that title");
            //Запрос
            using (HttpClient client = new HttpClient())
            {
                var request = new
                {
                    model = "deepseek-chat",
                    messages = new[]
                    {
                    new
                    {
                        role = "system",
                        content = "Ты профессиональный редактор. Сократи текст, сохранив главную мысль. Результат должен быть в 2-3 раза короче оригинала."
                    },
                    new
                    {
                        role = "user",
                        content = text
                    }
                },
                    max_tokens = 2000,
                    temperature = 0.3
                };// Добавляем заголовки
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                // Отправляем запрос
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                // Парсим ответ
                using (JsonDocument doc = JsonDocument.Parse(responseString))
                {
                    var root = doc.RootElement;
                    return root.GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();
                }
            }
        }
    }
}
