using Microsoft.EntityFrameworkCore;

public class BankingDBContext : DbContext
{
    public BankingDBContext(DbContextOptions<BankingDBContext> options) : base(options)
    {

    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.CustomerId); // Primary key

            entity.Property(c => c.FirstName)
                    .IsRequired()
                    .HasMaxLength(15);

            entity.Property(c => c.LastName)
                    .IsRequired()
                    .HasMaxLength(15);

            entity.Property(c => c.Email)
                    .IsRequired()
                    .HasMaxLength(75);

            entity.HasIndex(c => c.Email)
                    .IsUnique(); // Email should be unique

            entity.Property(c => c.NationalId)
                    .IsRequired()
                    .HasMaxLength(30);

            entity.HasIndex(c => c.NationalId)
                    .IsUnique();

            entity.Property(c => c.ContactNumber)
                      .HasMaxLength(20);

            entity.Property(c => c.Address)
                    .HasMaxLength(250);

            // Default values for new customers
            entity.Property(c => c.RegistrationDate)
                    .HasDefaultValueSql("NOW()") // sets default to current utc time in postgresql
                    .ValueGeneratedOnAdd(); // EF Core generates this value on add

            entity.Property(c => c.IsActive)
                    .HasDefaultValue(true); // Default to true for soft delete

            // one-to-many relationship with Account
            entity.HasMany(c => c.Accounts) // Customer has many Accounts
                    .WithOne(a => a.Customer)
                    .HasForeignKey(a => a.CustomerId) // Foreign key in Account table
                    .OnDelete(DeleteBehavior.Restrict);
        });

        // Account
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.AccountId); // primary key

            entity.Property(a => a.AccountNumber)
                    .IsRequired()
                    .HasMaxLength(20);

            entity.HasIndex(a => a.AccountNumber)
                    .IsUnique(); // unique constraint

            entity.Property(a => a.Balance)
                    .IsRequired()
                    .HasColumnType("numeric(18, 2)") // precision and scale
                    .HasDefaultValue(0m); // initial balance to 0

            entity.Property(a => a.OpeningDate)
                    .HasDefaultValueSql("NOW()") // sets current UTC time in postgresql
                    .ValueGeneratedOnAdd(); // generates this value on add

            // stores enum as string in database for readability
            entity.Property(a => a.AccountType)
                    .HasConversion<string>()
                    .IsRequired();

            entity.Property(a => a.Status)
                    .HasConversion<string>()
                    .HasDefaultValue(AccountStatus.Active) // default status
                    .IsRequired();

            entity.Property(a => a.IsActive)
                    .HasDefaultValue(true); // Default to true for soft delete


            //Already in customer but good to be explicit :)
            entity.HasOne(a => a.Customer) // account has one customer
                    .WithMany(c => c.Accounts) // customers have many Accounts
                    .HasForeignKey(a => a.CustomerId) // Foreign key in Account table
                    .IsRequired();
        });
        
        // Transactions
        modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.TransactionId); // Primary Key

                entity.Property(t => t.Amount)
                      .IsRequired() 
                      .HasColumnType("numeric(18, 2)");

                entity.Property(t => t.IsDebit)
                      .IsRequired();

                entity.Property(t => t.TransactionDate)
                      .HasDefaultValueSql("NOW()") // sets current UTC time in PostgreSQL
                      .ValueGeneratedOnAdd(); // generates this value on add

                entity.Property(t => t.Description)
                      .HasMaxLength(150); 

                entity.Property(t => t.ReferenceNumber)
                      .HasMaxLength(50); 

                entity.Property(t => t.BalanceAfterTransaction)
                      .IsRequired() 
                      .HasColumnType("numeric(18, 2)");

                // storing enum as string in database for readability
                entity.Property(t => t.TransactionType)
                      .HasConversion<string>()
                      .IsRequired();

                entity.Property(t => t.Status)
                      .HasConversion<string>()
                      .HasDefaultValue(TransactionStatus.Completed) // default
                      .IsRequired();

                // many to one with account
                entity.HasOne(t => t.Account) // transaction belongs to one Account
                      .WithMany(a => a.Transactions) // account has many Transactions
                      .HasForeignKey(t => t.AccountId) // Foreign key
                      .IsRequired() // AccountId is required
                      .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes
            });
    }
}