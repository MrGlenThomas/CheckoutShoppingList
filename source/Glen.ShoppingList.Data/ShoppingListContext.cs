namespace Glen.ShoppingList.Data
{
    using System;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Model;

    public class ShoppingListContext : DbContext
    {
        public ShoppingListContext(DbContextOptions<ShoppingListContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Drink>().HasKey("Id");
        }

        public DbSet<Drink> Drinks { get; set; }

        public T Find<T>(Guid id) where T : class
        {
            return Set<T>().Find(id);
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return Set<T>();
        }

        public void Save<T>(T entity) where T : class
        {
            var entry = Entry(entity);

            if (entry.State == EntityState.Detached)
                Set<T>().Add(entity);

            SaveChanges();
        }
    }
}
