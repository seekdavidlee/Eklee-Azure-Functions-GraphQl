using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
    public class BooksRepository
    {
        private readonly ILogger _logger;
        private readonly List<Book> _books = new List<Book>();
        private readonly List<Author> _authors = new List<Author>();
        private readonly List<BookAuthors> _booksAuthors = new List<BookAuthors>();

        public BooksRepository(ILogger logger)
        {
            _logger = logger;
            _books.Add(new Book { Id = "1", Name = "Nancy in the Wonderland", Category = "Children" });
            _books.Add(new Book { Id = "2", Name = "App in the Cloud", Category = "Technology" });
            _books.Add(new Book { Id = "3", Name = "History of China", Category = "History" });
            _books.Add(new Book { Id = "4", Name = "C/C++ for Beginners", Category = "Technology" });

            _authors.Add(new Author { Id = "1", Name = "James Wood" });
            _authors.Add(new Author { Id = "2", Name = "Andy Liu" });
            _authors.Add(new Author { Id = "3", Name = "Derick North" });
            _authors.Add(new Author { Id = "4", Name = "Mary Jane" });
            _authors.Add(new Author { Id = "5", Name = "Sir Richard Ice" });
            _authors.Add(new Author { Id = "6", Name = "Ken Poh" });

            _booksAuthors.Add(new BookAuthors
            {
                Book = _books.Single(x => x.Id == "1"),
                Authors = new List<Author>
                {
                    _authors.Single(x => x.Id == "1")
                }
            });

            _booksAuthors.Add(new BookAuthors
            {
                Book = _books.Single(x => x.Id == "2"),
                Authors = new List<Author>
                {
                    _authors.Single(x => x.Id == "2"),
                    _authors.Single(x => x.Id == "3"),
                    _authors.Single(x => x.Id == "4")
                }
            });

            _booksAuthors.Add(new BookAuthors
            {
                Book = _books.Single(x => x.Id == "3"),
                Authors = new List<Author>
                {
                    _authors.Single(x => x.Id == "5")
                }
            });

            _booksAuthors.Add(new BookAuthors
            {
                Book = _books.Single(x => x.Id == "4"),
                Authors = new List<Author>
                {
                    _authors.Single(x => x.Id == "2")
                }
            });
        }

        public Book GetBook(string id)
        {
            _logger.LogDebug($"Executing query to get Book with id {id}.");
            return _books.Single(x => x.Id == id);
        }

        public IEnumerable<Book> GetBooks(string category)
        {
            return _books.Where(x => x.Category == category).ToList();
        }

        public Book AddBook(Book book)
        {
            _books.Add(book);

            return book;
        }
    }
}
