using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
    public class Book
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
    }

    public class Author
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class BookAuthors
    {
        public Book Book { get; set; }
        public List<Author> Authors { get; set; }
    }
}
