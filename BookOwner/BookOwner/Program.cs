using Grpc.Core;
using Grpc.Net.Client;
using LibraryService;

internal class Program
{
    static async Task Main(string[] args)
    {
        using var channel = GrpcChannel.ForAddress("https://localhost:7001");
        var client = new Greeter.GreeterClient(channel);

        Console.WriteLine("Выберите действие:");
        Console.WriteLine("1 - Получить список всех книг");
        Console.WriteLine("2 - Получить информацию о конкретной книге");
        Console.WriteLine("0 - Выход");

        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                await GetBooksList(client);
                break;
            case "2":
                await GetBookInfo(client);
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Неверный выбор");
                break;
        }
    }

    static async Task GetBooksList(Greeter.GreeterClient client)
    {
        // Создаем потоковый вызов
        using var call = client.GetListOfBooks();

        // Задача для чтения ответов от сервера
        var readTask = Task.Run(async () =>
        {
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine($"Книга: {response.Books}");
            }
        });

        // Отправляем один пустой запрос для инициации потока
        await call.RequestStream.WriteAsync(new HelloRequest { Name = "" });
        await call.RequestStream.CompleteAsync();

        await readTask;
    }

    static async Task GetBookInfo(Greeter.GreeterClient client)
    {
        Console.WriteLine("Введите название книги:");
        var bookTitle = Console.ReadLine();

        // Создаем потоковый вызов
        using var call = client.GetBookInfo();

        // Задача для чтения ответов от сервера
        var readTask = Task.Run(async () =>
        {
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine($"Название: {response.Name}");
                Console.WriteLine($"Автор: {response.Author}");
                Console.WriteLine($"Описание: {response.Description}");
                Console.WriteLine();
            }
        });

        // Отправляем запрос с названием книги
        await call.RequestStream.WriteAsync(new HelloRequest { Name = bookTitle });
        await call.RequestStream.CompleteAsync();

        await readTask;
    }
}

