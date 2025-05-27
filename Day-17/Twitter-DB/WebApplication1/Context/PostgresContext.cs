using FirstTwitterApp.Models;
using Microsoft.EntityFrameworkCore;
public class Postgres : DbContext
{
    public Postgres(DbContextOptions options) : base(options)
    {

    }

    public DbSet<User> Users { get; set; }

    public DbSet<Tweet> Tweets { get; set; }

    public DbSet<Like> Likes { get; set; }

    public DbSet<Follow> Follows { get; set; }

    public DbSet<Hashtag> Hashtags { get; set; } 
    
    public DbSet<TweetHashtag> TweetHashtags { get; set;  }
}

// dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
// dotnet add package Microsoft.EntityFrameworkCore

// dotnet ef migrations add init
// dotnet ef database update
