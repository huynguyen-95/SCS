using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SCS.Api.Domain;

public class User
{
    public string EmpNo { get; private set; }

    public string Username { get; private set; }

    public bool IsAdmin { get; private set; } = false;

    public User(string empNo, string username, bool isAdmin = false)
    {
        EmpNo = empNo;
        Username = username;
        IsAdmin = isAdmin;
    }
}

public sealed class UserDomainConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.EmpNo);
        builder.Property(u => u.EmpNo).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Username).IsRequired().HasMaxLength(100);
        builder.Property(u => u.IsAdmin).IsRequired();
    }
}
