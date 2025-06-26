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
        RepeatedField<string> Items { get; set; }
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;

            ListOfBooks = new BookLibrary();
            string path = AppDomain.CurrentDomain.BaseDirectory + "Books.txt";
            string text = File.ReadAllText(path);
            while(text != null)
            {
                string line = text.Substring(0,text.IndexOf("\n"));
                Items.Add(line);
                text.Remove(text.IndexOf("\n"));
            }
        }
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
        public override async Task<HelloReply> GetListOfBooks(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<BooksTitles> streamTitles, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                await streamTitles.WriteAsync(new BooksTitles()
                {

                    books = Items;
            });
        }
    }
}
}
