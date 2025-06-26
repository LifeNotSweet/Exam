using System.Text.Json;
using System.Text;
using OllamaSharp;
using System.Data;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace LibraryService.Services
{
    public class ShortDescription
    {
        public static async Task<string> ShortenTextWithAI(string title)
        {
            var uri = new Uri("http://localhost:11434");
            using var ollama = new OllamaApiClient(uri);

            ollama.SelectedModel = "gemma3:1b";

            var prompt = "Представь, что ты писатель. Тебе нужно сократить текст в несколько раз, сохранив основную суть.Не нужно здоровоться и предлагать варианты. Постарайся сделать так, чтобы текст казался полным:\n" + title;

            var fullResponse = new StringBuilder();

            // Явное указание пространства имён OllamaSharp
            await foreach (var chunk in OllamaSharp.OllamaApiClientExtensions.GenerateAsync(
                ollama,
                prompt,
                null,
                CancellationToken.None))
            {
                fullResponse.Append(chunk.Response);
            }

            return fullResponse.ToString();
        }
    }
}