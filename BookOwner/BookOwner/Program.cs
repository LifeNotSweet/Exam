using Grpc.Core;
using Grpc.Net.Client;
using LibraryService;

internal class Program
{
    static private string password = "root";
    static async Task Main(string[] args)
    {
        using var channel = GrpcChannel.ForAddress("https://localhost:7001");
        var client = new Greeter.GreeterClient(channel);
        var callListOfBooks = client.GetListOfBooks();
        string choice = "";
        bool Authorized = false;
        bool Admin = false;
        while (!Authorized)
        {
            Console.WriteLine("Выберите права:");
            Console.WriteLine("1 - Пользователь");
            Console.WriteLine("2 - Администратор");
            Console.WriteLine("0 - Выход");
            choice = Console.ReadLine();
            if (choice == "1")
                Authorized = true;
            if(choice == "2")
            {
                string temp = "";
                Console.WriteLine("Пожалуйста, введите пароль для администратора:");
                temp=Console.ReadLine();
                if (temp == password)
                {
                    Authorized = true;
                    Admin = true;
                    break;
                }
                else
                {
                    Console.WriteLine("Ошибка: неверный пароль\n");
                    continue;
                }
            }
            if (choice == "0")
                break;
            else
                Console.WriteLine("Неизвестная команда.\n");

        }
        while (choice != "0" && Admin==false)
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
        while (choice != "0" && Admin == true)
        {
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1 - Получить список всех книг");
            Console.WriteLine("2 - Получить информацию о конкретной книге");
            Console.WriteLine("3 - Добавить книгу");
            Console.WriteLine("4 - Удалить книгу");
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
                case "3":

                    break;
                case "4":

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
                Console.WriteLine("Краткое описание: "+response.Shortdesc+"\n");
            }
        });

        // Отправляем запрос с названием книги
        

        await readTask;
        readTask.Dispose();
    }
}

