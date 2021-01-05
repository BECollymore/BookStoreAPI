﻿using System;
namespace BookStore_API.DTOs
{
    public class BookDTO
    {
        public BookDTO()
        {
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public int? Year { get; set; }
        public string ISBN { get; set; }
        public string Summary { get; set; }
        public string Image { get; set; }
        public double? Price { get; set; }

        public int? AuthorId { get; set; }
        public virtual AuthorDTO Author { get; set; }
    }
}
