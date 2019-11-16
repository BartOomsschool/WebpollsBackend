
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpdrachtAPI.Models;

namespace OpdrachtAPI.Models
{
    public class WebpollContext : DbContext
    {
        public WebpollContext(DbContextOptions<WebpollContext> options) : base(options) { }

        public DbSet<Antwoord> Antwoord { get; set; }
        public DbSet<Poll> Poll { get; set; }
        public DbSet<PollUser> PollUser { get; set; }
        public DbSet<Stem> Stem { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Vriend> Vriend { get; set; }

        public DbSet<VriendUser> VriendUser { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Antwoord>().ToTable("Antwoord");
            modelBuilder.Entity<Poll>().ToTable("Poll");
            modelBuilder.Entity<PollUser>().ToTable("PollUser");
            modelBuilder.Entity<Stem>().ToTable("Stem");
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Vriend>().ToTable("Vriend");
            modelBuilder.Entity<VriendUser>().ToTable("VriendUser");
        }


    }
}
