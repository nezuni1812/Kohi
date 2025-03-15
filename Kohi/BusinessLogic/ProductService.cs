using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kohi.Models;
using Microsoft.EntityFrameworkCore;

namespace Kohi.BusinessLogic
{
    class ProductService
    {
        private readonly AppDbContext _context;

        public ProductService()
        {
            _context = new AppDbContext();
        }

        public async Task<List<ProductModel>> GetProductAsync()
        {
            return await _context.products.Include(e => e.Category).ToListAsync();
        }

        //This function could change to take in a name and id instead of a Product object to naming and stuff
        public async Task AddProductAsync(ProductModel product)
        {
            _context.products.Add(product);
            await _context.SaveChangesAsync();
        }
    }
}
