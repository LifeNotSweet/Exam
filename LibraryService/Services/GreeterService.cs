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
            while(text != null)
            {
                string line = text.Substring(0,text.IndexOf("\n"));
                text.Remove(text.IndexOf("\n"));
                while(line != null)
                {
                    ListOfBooks.Add(Convert.ToInt32(line.Substring(line.IndexOf("{"),line.IndexOf(","))), Convert.ToInt32(line.Substring(line.IndexOf(","), line.IndexOf("}"))), line.Substring(0, line.LastIndexOf(";")));
                    var bb = ListOfBooks.GetAll();
                    if (!CheckBorrowing(line))
                    {
                        if (line.Substring(line.IndexOf("}"), line.IndexOf(":")) == "CheckedOut")
                            bb[bb.Count - 1].Status = BookStatus.CheckedOut;
                        else
                            bb[bb.Count - 1].Status = BookStatus.Reserved;
                        bb[bb.Count - 1].CurrentHolder = line.Substring(line.IndexOf(":"), line.Length - line.IndexOf(":"));
                    }
                    else
                        bb[bb.Count - 1].Status = BookStatus.Available;
                }
            }
        }
        private bool CheckBorrowing(string text)
        {
            if (text.IndexOf("}") + 1 == text.Length)
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
    }
}
