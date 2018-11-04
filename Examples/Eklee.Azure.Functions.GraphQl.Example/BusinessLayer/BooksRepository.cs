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

            _books.Add(new Book { Id = "5", Name = "How To Draw Anything", Category = "Art" });
            _books.Add(new Book { Id = "6", Name = "Anatomy and Drawing", Category = "Art" });
            _books.Add(new Book { Id = "7", Name = "Space Drawings", Category = "Art" });
            _books.Add(new Book { Id = "8", Name = "Art for Beginners", Category = "Art" });
            _books.Add(new Book { Id = "9", Name = "Chinese Art", Category = "Art" });
            _books.Add(new Book { Id = "10", Name = "Art in 2018", Category = "Art" });
            _books.Add(new Book { Id = "11", Name = "Makers of design", Category = "Art" });
            _books.Add(new Book { Id = "12", Name = "Art Forms in Nature", Category = "Art" });
            _books.Add(new Book { Id = "13", Name = "The Art of Instruction", Category = "Art" });
            _books.Add(new Book { Id = "14", Name = "The Contemporary Art Book", Category = "Art" });
            _books.Add(new Book { Id = "15", Name = "Sketchbook Fairy Tale", Category = "Art" });
            _books.Add(new Book { Id = "16", Name = "Art History", Category = "Art" });
            _books.Add(new Book { Id = "17", Name = "Roman Art", Category = "Art" });
            _books.Add(new Book { Id = "18", Name = "The Metropolitan Museum of Art", Category = "Art" });
            _books.Add(new Book { Id = "19", Name = "Texas Artworks", Category = "Art" });
            _books.Add(new Book { Id = "20", Name = "Simple arts", Category = "Art" });
            _books.Add(new Book { Id = "21", Name = "Art for kids", Category = "Art" });
            _books.Add(new Book { Id = "22", Name = "Advanced art for kids", Category = "Art" });
            _books.Add(new Book { Id = "23", Name = "Art and music", Category = "Art" });
            _books.Add(new Book { Id = "24", Name = "Art Apps", Category = "Art" });
            _books.Add(new Book { Id = "25", Name = "Art in homes", Category = "Art" });
            _books.Add(new Book { Id = "26", Name = "Art ABC", Category = "Art" });
            _books.Add(new Book { Id = "27", Name = "Touching Art", Category = "Art" });
            _books.Add(new Book { Id = "28", Name = "Woodwork Art", Category = "Art" });
            _books.Add(new Book { Id = "29", Name = "Plastics Art work", Category = "Art" });
            _books.Add(new Book { Id = "30", Name = "Boring Art", Category = "Art" });

            _authors.Add(new Author { Id = "1", Name = "James Wood" });
            _authors.Add(new Author { Id = "2", Name = "Andy Liu" });
            _authors.Add(new Author { Id = "3", Name = "Derick North" });
            _authors.Add(new Author { Id = "4", Name = "Mary Jane" });
            _authors.Add(new Author { Id = "5", Name = "Sir Richard Ice" });
            _authors.Add(new Author { Id = "6", Name = "Ken Poh" });

            _authors.Add(new Author { Id = "7", Name = "Zac Mer" });
            _authors.Add(new Author { Id = "8", Name = "Kathy Zhang" });
            _authors.Add(new Author { Id = "9", Name = "Joe Cook" });
            _authors.Add(new Author { Id = "10", Name = "Michael Han" });
            _authors.Add(new Author { Id = "11", Name = "Fu Wen" });
            _authors.Add(new Author { Id = "12", Name = "Li Wei" });

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

            _booksAuthors.Add(new BookAuthors
            {
                Book = _books.Single(x => x.Id == "5"),
                Authors = new List<Author>
                {
                    _authors.Single(x => x.Id == "7")
                }
            });

            _booksAuthors.Add(new BookAuthors
            {
                Book = _books.Single(x => x.Id == "6"),
                Authors = new List<Author>
                {
                    _authors.Single(x => x.Id == "8")
                }
            });

            _booksAuthors.Add(new BookAuthors
            {
                Book = _books.Single(x => x.Id == "7"),
                Authors = new List<Author>
                {
                    _authors.Single(x => x.Id == "9")
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

        public IEnumerable<Book> GetBooks()
        {
            return _books;
        }

        public Book AddBook(Book book)
        {
            _books.Add(book);

            return book;
        }
    }
}
