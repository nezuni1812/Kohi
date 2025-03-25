//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Data.Sqlite;

//using Microsoft.EntityFrameworkCore;
//using SQLitePCL;
//using Windows.Storage;

//namespace Kohi.Models
//{
//    class AppDbContext : DbContext
//    {
//        public DbSet<ProductModel> products { get; set; }
//        public DbSet<CategoryModel> categories { get; set; }

//        public AppDbContext()
//        {
//            Debug.WriteLine("AppDbContext initialized");
//            Batteries.Init();
//            Database.EnsureCreated(); // ✅ Automatically creates the database and tables
//        }

//        protected override void OnConfiguring(DbContextOptionsBuilder options)
//        {
//            string SQLiteFileName = "data.db";
//            string dbPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SQLiteFileName);
//            if (!File.Exists(dbPath))
//            {
//                using (File.Create(dbPath)) { }
//            }

//            options.UseSqlite($"Data Source={dbPath}");
//            Debug.WriteLine($"Database in file: {dbPath}");
//        }
//    }
//}

//using Microsoft.EntityFrameworkCore;
//using Microsoft.Data.Sqlite;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;

//namespace Kohi.Models
//{
//    public class AppDbContext : DbContext
//    {
//        // DbSets cho tất cả 19 class
//        public DbSet<CategoryModel> Categories { get; set; }
//        public DbSet<CustomerModel> Customers { get; set; }
//        public DbSet<ExpenseCategoryModel> ExpenseCategories { get; set; }
//        public DbSet<ExpenseModel> Expenses { get; set; }
//        public DbSet<InboundModel> Inbounds { get; set; }
//        public DbSet<InvoiceModel> Invoices { get; set; }
//        public DbSet<InventoryModel> Inventories { get; set; }
//        public DbSet<IngredientModel> Ingredients { get; set; }
//        public DbSet<InvoiceDetailModel> InvoiceDetails { get; set; }
//        public DbSet<ProductVariantModel> RecipeDetails { get; set; }
//        public DbSet<PaymentModel> Payments { get; set; }
//        public DbSet<OrderToppingModel> OrderToppings { get; set; }
//        public DbSet<SupplierModel> Suppliers { get; set; }
//        public DbSet<ProductModel> Products { get; set; }
//        public DbSet<OutboundModel> Outbounds { get; set; }
//        public DbSet<ProductVariantModel> Recipes { get; set; }
//        public DbSet<TaxModel> Taxes { get; set; }
//        public DbSet<InvoiceTaxModel> InvoiceTaxes { get; set; }

//        // Constructor hỗ trợ Dependency Injection
//        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
//        {
//            Debug.WriteLine("AppDbContext initialized with options");
//            Database.EnsureCreated();
//        }

//        // Constructor mặc định
//        public AppDbContext()
//        {
//            Debug.WriteLine("AppDbContext initialized");
//            Database.EnsureCreated();
//        }

//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        => optionsBuilder.UseNpgsql("Host=localhost;Username=kohi;Password=1234;Database=kohi");


//        //protected override void OnConfiguring(DbContextOptionsBuilder options)
//        //{
//        //    if (!options.IsConfigured) // Chỉ cấu hình nếu chưa được cấu hình qua DI
//        //    {
//        //        string SQLiteFileName = "data.db";
//        //        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SQLiteFileName);
//        //        if (!File.Exists(dbPath))
//        //        {
//        //            File.Create(dbPath).Dispose();
//        //            Debug.WriteLine($"Created new database file at: {dbPath}");
//        //        }
//        //        options.UseSqlite($"Data Source={dbPath}");
//        //        Debug.WriteLine($"Database path: {dbPath}");
//        //    }
//        //}

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            // CategoryModel
//            modelBuilder.Entity<CategoryModel>()
//                .HasMany(c => c.Products)
//                .WithOne(p => p.Category)
//                .HasForeignKey(p => p.CategoryId);

//            // CustomerModel
//            modelBuilder.Entity<CustomerModel>()
//                .HasMany(c => c.Invoices)
//                .WithOne(i => i.Customer)
//                .HasForeignKey(i => i.CustomerId);


//            modelBuilder.Entity<ExpenseCategoryModel>()
//                .HasMany(ec => ec.Expenses)
//                .WithOne(e => e.ExpenseCategory)
//                .HasForeignKey(e => e.ExpenseCategoryId);

//            // ExpenseModel
//            modelBuilder.Entity<ExpenseModel>()
//                .HasOne(e => e.ExpenseCategory)
//                .WithMany(ec => ec.Expenses)
//                .HasForeignKey(e => e.ExpenseCategoryId);

//            // InboundModel
//            modelBuilder.Entity<InboundModel>()
//                .HasOne(ib => ib.Ingredient)
//                .WithMany()
//                .HasForeignKey(ib => ib.IngredientId);

//            modelBuilder.Entity<InboundModel>()
//                .HasOne(ib => ib.Supplier)
//                .WithMany()
//                .HasForeignKey(ib => ib.SupplierId);


//            // InvoiceModel
//            modelBuilder.Entity<InvoiceModel>()
//                .HasOne(i => i.Customer)
//                .WithMany(c => c.Invoices)
//                .HasForeignKey(i => i.CustomerId);

//            modelBuilder.Entity<InvoiceModel>()
//                .HasMany(i => i.InvoiceDetails)
//                .WithOne(id => id.Invoice)
//                .HasForeignKey(id => id.InvoiceId);

//            // InventoryModel
//            modelBuilder.Entity<InventoryModel>()
//                .HasOne(iv => iv.Inbound)
//                .WithMany()
//                .HasForeignKey(iv => iv.InboundId);


//            // InvoiceDetailModel
//            modelBuilder.Entity<InvoiceDetailModel>()
//                .HasOne(id => id.Invoice)
//                .WithMany(i => i.InvoiceDetails)
//                .HasForeignKey(id => id.InvoiceId);

//            modelBuilder.Entity<InvoiceDetailModel>()
//                .HasOne(id => id.Product)
//                .WithMany(p => p.InvoiceDetails)
//                .HasForeignKey(id => id.ProductId);

//            modelBuilder.Entity<InvoiceDetailModel>()
//                .HasMany(id => id.Toppings)
//                .WithOne(ot => ot.InvoiceDetail)
//                .HasForeignKey(ot => ot.InvoiceDetailId);

//            // RecipeDetailModel
//            modelBuilder.Entity<ProductVariantModel>()
//                .HasKey(rd => new { rd.RecipeId, rd.IngredientId }); // Composite key

//            modelBuilder.Entity<ProductVariantModel>()
//                .HasOne(rd => rd.Recipe)
//                .WithMany(r => r.Ingredients)
//                .HasForeignKey(rd => rd.RecipeId);

//            modelBuilder.Entity<ProductVariantModel>()
//                .HasOne(rd => rd.Ingredient)
//                .WithMany()
//                .HasForeignKey(rd => rd.IngredientId);

//            // PaymentModel
//            modelBuilder.Entity<PaymentModel>()
//                .HasOne(p => p.Invoice)
//                .WithMany()
//                .HasForeignKey(p => p.InvoiceId);

//            // OrderToppingModel
//            modelBuilder.Entity<OrderToppingModel>()
//                .HasOne(ot => ot.InvoiceDetail)
//                .WithMany(id => id.Toppings)
//                .HasForeignKey(ot => ot.InvoiceDetailId);

//            modelBuilder.Entity<OrderToppingModel>()
//                .HasOne(ot => ot.Product)
//                .WithMany()
//                .HasForeignKey(ot => ot.ProductId);

//            // SupplierModel (không có Navigation Property ngược)

//            // ProductModel
//            modelBuilder.Entity<ProductModel>()
//                .HasOne(p => p.Category)
//                .WithMany(c => c.Products)
//                .HasForeignKey(p => p.CategoryId);

//            modelBuilder.Entity<ProductModel>()
//                .HasMany(p => p.InvoiceDetails)
//                .WithOne(id => id.Product)
//                .HasForeignKey(id => id.ProductId);

//            // OutboundModel
//            modelBuilder.Entity<OutboundModel>()
//                .HasOne(o => o.Inventory)
//                .WithMany()
//                .HasForeignKey(o => o.InventoryId);

//            // RecipeModel
//            modelBuilder.Entity<ProductVariantModel>()
//                .HasOne(r => r.Product)
//                .WithMany()
//                .HasForeignKey(r => r.ProductId);

//            modelBuilder.Entity<ProductVariantModel>()
//                .HasMany(r => r.Ingredients)
//                .WithOne(rd => rd.Recipe)
//                .HasForeignKey(rd => rd.RecipeId);

//            // TaxModel
//            modelBuilder.Entity<TaxModel>()
//                .HasMany(t => t.InvoiceTaxes)
//                .WithOne(it => it.Tax)
//                .HasForeignKey(it => it.TaxId);

//            // InvoiceTaxModel
//            modelBuilder.Entity<InvoiceTaxModel>()
//                .HasOne(it => it.Invoice)
//                .WithMany()
//                .HasForeignKey(it => it.InvoiceId);

//            modelBuilder.Entity<InvoiceTaxModel>()
//                .HasOne(it => it.Tax)
//                .WithMany(t => t.InvoiceTaxes)
//                .HasForeignKey(it => it.TaxId);
//        }
//    }
//}