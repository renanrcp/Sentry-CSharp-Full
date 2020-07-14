using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Sentry_Csharp_Full
{
    public class SampleContext : DbContext
    {
        public SampleContext([NotNull] DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }

    [Table("user")]
    public class User
    {
        [Key]
        [Column("use_id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Column("use_name")]
        public string Name { get; set; }

        [Column("use_userid")]
        public ulong OtherId { get; set; }
    }
}