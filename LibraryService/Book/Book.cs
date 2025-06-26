namespace LibraryService.Book
{
    public enum BookStatus
    {
        Available,   // Доступна
        Reserved,    // Забронирована
        CheckedOut   // Взята читателем
    }

    public class Book
    {
        public int ShelfNumber { get; }
        public int RoomNumber { get; }
        public string Title { get; }
        public BookStatus Status { get; private set; }
        public string CurrentHolder { get; private set; }
        public string DescriptionFilePath { get; }


        public Book(int shelf, int room, string title)
        {
            ShelfNumber = shelf;
            RoomNumber = room;
            Title = title;
            Status = BookStatus.Available;
            CurrentHolder = string.Empty;
        }
    }

    public class BookLibrary
    {
        private List<Book> _books = new List<Book>();

        public Book Add(int shelf, int room, string title)
        {
            var book = new Book(shelf, room, title);
            _books.Add(book);
            return book;
        }

        public Book Find(int shelf, int room, string title)
        {
            return _books.FirstOrDefault(b => b.ShelfNumber == shelf && b.RoomNumber == room && b.Title == title);
        }

        public bool Remove(int shelf, int room, string title)
        {
            var book = Find(shelf, room, title);
            return book != null && _books.Remove(book);
        }

        public List<Book> GetAll() => _books.ToList();
    }
}
