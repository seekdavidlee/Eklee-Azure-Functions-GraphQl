using System.Threading.Tasks;
using Autofac;
using Eklee.Azure.Functions.GraphQl.Example.Models;

namespace Eklee.Azure.Functions.GraphQl.Example
{
    public class MyStartup : IStartable
    {
        private readonly IGraphQlRepository _graphQlRepository;

        public MyStartup(IGraphQlRepository graphQlRepository)
        {
            _graphQlRepository = graphQlRepository;
        }
        public void Start()
        {
            Task.Run(async () =>
            {
                await _graphQlRepository.AddAsync(new Book { Id = "1", Name = "Nancy in the Wonderland", Category = "Children" });
                await _graphQlRepository.AddAsync(new Book { Id = "2", Name = "App in the Cloud", Category = "Technology" });
                await _graphQlRepository.AddAsync(new Book { Id = "3", Name = "History of China", Category = "History" });
                await _graphQlRepository.AddAsync(new Book { Id = "4", Name = "C/C++ for Beginners", Category = "Technology" });

                await _graphQlRepository.AddAsync(new Book { Id = "5", Name = "How To Draw Anything", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "6", Name = "Anatomy and Drawing", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "7", Name = "Space Drawings", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "8", Name = "Art for Beginners", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "9", Name = "Chinese Art", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "10", Name = "Art in 2018", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "11", Name = "Makers of design", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "12", Name = "Art Forms in Nature", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "13", Name = "The Art of Instruction", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "14", Name = "The Contemporary Art Book", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "15", Name = "Sketchbook Fairy Tale", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "16", Name = "Art History", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "17", Name = "Roman Art", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "18", Name = "The Metropolitan Museum of Art", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "19", Name = "Texas Artworks", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "20", Name = "Simple arts", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "21", Name = "Art for kids", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "22", Name = "Advanced art for kids", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "23", Name = "Art and music", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "24", Name = "Art Apps", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "25", Name = "Art in homes", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "26", Name = "Art ABC", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "27", Name = "Touching Art", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "28", Name = "Woodwork Art", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "29", Name = "Plastics Art work", Category = "Art" });
                await _graphQlRepository.AddAsync(new Book { Id = "30", Name = "Boring Art", Category = "Art" });

                await _graphQlRepository.AddAsync(new Author { Id = "1", Name = "James Wood" });
                await _graphQlRepository.AddAsync(new Author { Id = "2", Name = "Andy Liu" });
                await _graphQlRepository.AddAsync(new Author { Id = "3", Name = "Derick North" });
                await _graphQlRepository.AddAsync(new Author { Id = "4", Name = "Mary Jane" });
                await _graphQlRepository.AddAsync(new Author { Id = "5", Name = "Sir Richard Ice" });
                await _graphQlRepository.AddAsync(new Author { Id = "6", Name = "Ken Poh" });

                await _graphQlRepository.AddAsync(new Author { Id = "7", Name = "Zac Mer" });
                await _graphQlRepository.AddAsync(new Author { Id = "8", Name = "Kathy Zhang" });
                await _graphQlRepository.AddAsync(new Author { Id = "9", Name = "Joe Cook" });
                await _graphQlRepository.AddAsync(new Author { Id = "10", Name = "Michael Han" });
                await _graphQlRepository.AddAsync(new Author { Id = "11", Name = "Fu Wen" });
                await _graphQlRepository.AddAsync(new Author { Id = "12", Name = "Li Wei" });
                /*
                await _graphQlRepository.AddAsync(new BookAuthors
                {
                    Book = _books.Single(x => x.Id == "1"),
                    Authors = new List<Author>
                {
                    _authors.Single(x => x.Id == "1")
                }
                });

                await _graphQlRepository.AddAsync(new BookAuthors
                {
                    Book = _books.Single(x => x.Id == "2"),
                    Authors = new List<Author>
                {
                    _authors.Single(x => x.Id == "2"),
                    _authors.Single(x => x.Id == "3"),
                    _authors.Single(x => x.Id == "4")
                }
                });

                await _graphQlRepository.AddAsync(new BookAuthors
                {
                    Book = _books.Single(x => x.Id == "3"),
                    Authors = new List<Author>
                {
                    _authors.Single(x => x.Id == "5")
                }
                });

                await _graphQlRepository.AddAsync(new BookAuthors
                {
                    Book = _books.Single(x => x.Id == "4"),
                    Authors = new List<Author>
                {
                    _authors.Single(x => x.Id == "2")
                }
                });

                await _graphQlRepository.AddAsync(new BookAuthors
                {
                    Book = _books.Single(x => x.Id == "5"),
                    Authors = new List<Author>
                {
                    _authors.Single(x => x.Id == "7")
                }
                });

                await _graphQlRepository.AddAsync(new BookAuthors
                {
                    Book = _books.Single(x => x.Id == "6"),
                    Authors = new List<Author>
                {
                    _authors.Single(x => x.Id == "8")
                }
                });

                await _graphQlRepository.AddAsync(new BookAuthors
                {
                    Book = _books.Single(x => x.Id == "7"),
                    Authors = new List<Author>
                {
                    _authors.Single(x => x.Id == "9")
                }
                });
                */
            });
        }
    }
}
