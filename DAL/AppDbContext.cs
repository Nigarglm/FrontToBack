﻿using _16Nov_task.Models;
using Microsoft.EntityFrameworkCore;

namespace _16Nov_task.DAL
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
            
        }

        public DbSet<Slide> Slides { get; set; }
        public DbSet<Product> Products { get;set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
    }
}
