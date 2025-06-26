using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Net.Client.Balancer;
using LibraryService;
using LibraryService.Book;


namespace LibraryService.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        private BookLibrary ListOfBooks;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;

            ListOfBooks = new BookLibrary();
            string text=ReadBooks();
            while(text != "")
            {
                string line = text.Substring(0,text.IndexOf("\n"));
                if (text.IndexOf("\n") + 1 != text.Length)
                    text = text.Remove(0, text.IndexOf("\n") + 1);
                else
                    text = "";
                while(line != "")
                {
                    int first = line.IndexOf("{") + 1;
                    int second = line.IndexOf(",") - line.IndexOf("{")-1;
                    int third = line.IndexOf(",") + 1;
                    int fourth = line.IndexOf("}") - line.IndexOf(",")-1;
                    int fifth = line.LastIndexOf(";")-1-line.IndexOf("\"");
                    ListOfBooks.Add(Convert.ToInt32(line.Substring(first,second)), Convert.ToInt32(line.Substring(third, fourth)), line.Substring(line.IndexOf("\""), fifth));
                    var bb = ListOfBooks.GetAll();
                    if (!CheckBorrowing(line))
                    {
                        int tt = line.IndexOf("}") + 1;
                        if (line.Substring(tt, line.IndexOf(":")-tt-1) == "CheckedOut")
                            bb[bb.Count - 1].Status = BookStatus.CheckedOut;
                        else
                            bb[bb.Count - 1].Status = BookStatus.Reserved;
                        bb[bb.Count - 1].CurrentHolder = line.Substring(line.IndexOf(":")+1, line.Length - line.IndexOf(":")-1-1);
                    }
                    else
                        bb[bb.Count - 1].Status = BookStatus.Available;
                    break;
                }
            }
        }
        private bool CheckBorrowing(string text)
        {
            if (text.IndexOf("}") + 2 == text.Length)
                return true;
            else {
                return false;
                 }
        }
        private string ReadBooks()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Books.txt";
            string text = File.ReadAllText(path);
            return text;
        }
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
        public override async Task GetListOfBooks(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<BooksTitles> streamTitles, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                await streamTitles.WriteAsync(new BooksTitles()
                {
                    
                    Books = ReadBooks()
                });
            }
        }
        private string GetDescription(string title)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "BooksDescription\\"+title+".txt";
            string text = File.ReadAllText(path);
            return text;
        }
        public override async Task GetBookInfo(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<BookInfo> streamTitles, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                var books=ListOfBooks.GetAll();
                int temp = -1;
                string author = "";
                string name = "";
                string desc = "";
                string shorten = "";
                for(int i = 0; i < books.Count; i++)
                {
                    if (books[i].Title.IndexOf(request.Name) > -1)
                    {
                        temp = i;
                        break;
                    }
                }
                if (temp != -1) {
                    string title = books[temp].Title;
                    author = title.Substring(title.IndexOf("-") + 1, title.Length - title.IndexOf("-") - 1);
                    name = title.Substring(1, title.LastIndexOf("\"")- 1);
                    desc = GetDescription(request.Name);
                    shorten = await ShortDescription.ShortenTextWithAI(request.Name);
                }
                else {
                    name = "Unfound";
                    author = "";
                    desc = "";
                    shorten = "";
                }
                await streamTitles.WriteAsync(new BookInfo()
                {
                    Author = author,
                    Name = name,
                    Description = desc,
                    Shortdesc = shorten
                }
                ) ;
            }
        }
    }
}
