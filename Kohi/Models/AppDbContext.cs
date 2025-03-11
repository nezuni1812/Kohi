using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using Windows.Storage;

namespace Kohi.Models
{
    class AppDbContext : DbContext
    {
        public DbSet<Product> products { get; set; }
        public DbSet<Category> categories { get; set; }

        public AppDbContext()
        {
            Debug.WriteLine("AppDbContext initialized");
            Batteries.Init();
            Database.EnsureCreated(); // ✅ Automatically creates the database and tables
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string SQLiteFileName = "data.db";
            string dbPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SQLiteFileName);
            if (!File.Exists(dbPath))
            {
                using (File.Create(dbPath)) { }
            }

            options.UseSqlite($"Data Source={dbPath}");
            Debug.WriteLine($"Database in file: {dbPath}");
        }
    }
}
