using Grpc.Core;
using Grpc.Net.Client;
using LibraryService;

internal class Program
{

    static async Task Main(string[] args)
    {
        using var channel = GrpcChannel.ForAddress("https://localhost:7001");
        var client = new Greeter.GreeterClient(channel);
        var callListOfBooks = client.GetListOfBooks();
        string choice = "";
        while (choice != "0")
        {
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1 - Получить список всех книг");
            Console.WriteLine("2 - Получить информацию о конкретной книге");
            Console.WriteLine("0 - Выход");

            choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await GetBooksList(client);
                    break;
                case "2":
                    await GetBookInfo(client);
                    break;
                default:
                    Console.WriteLine("Неверный выбор");
                    break;
            }
        }
    }

    static async Task GetBooksList(Greeter.GreeterClient client)
    {
        // Создаем потоковый вызов
        using var callListOfBooks = client.GetListOfBooks();
        await callListOfBooks.RequestStream.WriteAsync(new HelloRequest() { Name = "None" });
        await callListOfBooks.RequestStream.CompleteAsync();
        // Задача для чтения ответов от сервера
        var readTask = Task.Run(async () =>
        {
            await foreach (var response in callListOfBooks.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine("Книги:\n"+response.Books);

            }
        });

        // Отправляем один пустой запрос для инициации поток

        await readTask;
        readTask.Dispose();
        
    }

    static async Task GetBookInfo(Greeter.GreeterClient client)
    {
        Console.WriteLine("Введите название книги:");
        var bookTitle = Console.ReadLine();

        // Создаем потоковый вызов
        using var call = client.GetBookInfo();
        await call.RequestStream.WriteAsync(new HelloRequest { Name = bookTitle });
        await call.RequestStream.CompleteAsync();
        // Задача для чтения ответов от сервера
        var readTask = Task.Run(async () =>
        {
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                if (response.Name == "Unknown")
                {
                    Console.WriteLine("Книга не найдена");
                    break;
                }
                Console.WriteLine($"Название: {response.Name}");
                Console.WriteLine($"Автор: {response.Author}");
                Console.WriteLine($"Описание: {response.Description}");
                Console.WriteLine("Краткое описание: "+response.Shortdesc);
            }
        });

        // Отправляем запрос с названием книги
        

        await readTask;
        readTask.Dispose();
    }
}

