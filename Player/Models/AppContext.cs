using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Player.Models
{
    public sealed class AppContext : DbContext
    {
        public DbSet<Audio> Audio { get; set; }
        public AppContext(DbContextOptions<AppContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

    }
}
