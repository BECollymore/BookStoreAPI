using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookStore_API.Contracts;
using BookStore_API.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore_API.Services
{
    public class BookRepository:IBookRepository
    {
        private readonly ApplicationDbContext _db;
        public BookRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Create(Book entity)
        {
            await _db.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(Book entity)
        {
            _db.Remove(entity);
            return await Save();
        }

        public async Task<IList<Book>> FindAll()
        {
            return await _db.Books.ToListAsync();
        }

        public async Task<Book> FindById(int id)
        {
            var book = await _db.Books.FindAsync(id);
            return book;
        }

        public async Task<bool> isExists(int id)
        {
            return await _db.Books.AnyAsync(b => b.Id == id);
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Update(Book entity)
        {
            var isexist = await isExists(entity.Id);
            if(isexist)
            {
                _db.Update(entity);

            }
            return await Save();

        }
    }
}
