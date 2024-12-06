# Data Access

## **Understanding ORM and LINQ in the System**

This section demonstrates how the introduction of an ORM (Object-Relational Mapper) like Entity Framework Core transforms data management and simplifies database interactions. It also explains how LINQ differs from the traditional SQL approach, with examples based on the system's source code.

### **How the Introduction of an ORM Changes How You Work with Data**

Entity Framework Core (EF Core) abstracts database interactions by allowing developers to define entities, relationships, and schema configurations in C# code. This eliminates the need for raw SQL queries and manual schema management.

### **ApplicationDbContext**
```csharp
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public required DbSet<User> Users { get; set; }
        public required DbSet<Role> Roles { get; set; }
        public required DbSet<UserRole> UserRoles { get; set; }
        public required DbSet<EmissionRecord> EmissionRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configure UserRole relationships
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        }
    }
}
```

#### **Explanation**
1. **DbSet Properties**:
   - `DbSet<User>`, `DbSet<Role>`, etc., define entity classes (`User`, `Role`) that map to database tables.
   - They allow CRUD operations on tables without writing SQL.

2. **OnModelCreating**:
   - Configures entity relationships, constraints, and indexes:
     - `UserRole` uses a composite key (`UserId`, `RoleId`) for many-to-many relationships.
     - `User.Email` is set to be unique using `HasIndex`.

3. **Automated Schema Management**:
   - Schema changes made in the entity classes or configuration are applied using migrations, eliminating the need for manual SQL scripts.

---

#### **Migration Snapshot**
```csharp
#nullable disable
namespace Backend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity("Backend.Models.EmissionRecord", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<string>("UserId").IsRequired().HasColumnType("TEXT");
                b.HasIndex("UserId");
                b.ToTable("EmissionRecords");
            });

            modelBuilder.Entity("Backend.Models.Role", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.HasData(
                    new { Id = 1, Name = "Admin", Description = "Administrator" },
                    new { Id = 2, Name = "Customer", Description = "Regular User" });
                b.ToTable("Roles");
            });

            // Relationships
            modelBuilder.Entity("Backend.Models.UserRole", b =>
            {
                b.HasOne("Backend.Models.User", "User")
                  .WithMany("UserRoles")
                  .HasForeignKey("UserId")
                  .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
```

#### **Explanation**
1. **Database Schema Representation**:
   - The snapshot reflects the current database schema, generated from the `ApplicationDbContext` and entity configurations.
2. **Seed Data**:
   - Roles (`Admin`, `Customer`) are preloaded, avoiding manual SQL inserts.
3. **Relationships**:
   - Many-to-many relationships are captured programmatically, ensuring data integrity.

---

### **How Using LINQ Differs from the Traditional SQL Approach**

LINQ (Language-Integrated Query) offers a declarative, strongly-typed syntax for database queries, eliminating the need for raw SQL strings. 

### **Example: Traditional SQL vs LINQ**

#### **Traditional SQL**
```sql
SELECT * 
FROM Users 
WHERE Email = 'example@example.com';
```
- Requires raw SQL strings.
- Lacks compile-time safety, with errors appearing only at runtime.
- Manual joins and parameter handling are required.

#### **LINQ**
```csharp
var user = await context.Users
    .FirstOrDefaultAsync(u => u.Email == "example@example.com");
```
- **Type-Safe**: Queries are validated at compile time.
- **Readability**: Integrates with the language, improving code clarity.
- **Relationship Navigation**: Supports object-oriented navigation (e.g., `.Include()` for related data).

---

## **Key Differences**
1. **Syntax**:
   - LINQ uses a declarative, C#-integrated syntax.
   - SQL relies on raw strings, which are prone to errors.

2. **Object-Oriented**:
   - LINQ directly maps entities to objects, enabling seamless navigation of relationships.
   - SQL requires explicit joins and table handling.

3. **Compile-Time Safety**:
   - LINQ catches errors at compile time, while SQL errors occur at runtime.

---

