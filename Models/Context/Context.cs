using Microsoft.EntityFrameworkCore;

public class Context : DbContext
{
    public DbSet<Atteched> Attecheds_tbl { get; set; }
    public DbSet<Messages> Messages_tbl { get; set; }
    public DbSet<Recivers> Recivers_tbl { get; set; }
    public DbSet<Users> Users_tbl { get; set; }
    public DbSet<smsUser> sms_tbl { get; set; }
    public DbSet<MessageLog> msgLog_tbl { get; set; }
    public DbSet<UserLog> userLogs_tbl { get; set; }
    public DbSet<smsToken> smsTokens { get; set; }    
    public DbSet<Permission> Permission_tbl { get; set; }
    public DbSet<Role> Role_tbl { get; set; }
    public DbSet<RolePermission> RolePermissions_tbl { get; set; }
    public DbSet<UserRole> UserRoles_tbl { get; set; }
    public DbSet<Reply> Reply_tbl { get; set; }
    public DbSet<AttechedReply> AttechedReplies_tbl { get; set; }
    public DbSet<Category> Categories_tbl { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // optionsBuilder.UseSqlServer("server=.\\SQL2019;database=IliaDabirkhane;trusted_connection=true;MultipleActiveResultSets=True;TrustServerCertificate=True");
        // optionsBuilder.UseSqlServer("server=.\\SQL2019;database=IliaDabirkhaneTesti;trusted_connection=true;MultipleActiveResultSets=True;TrustServerCertificate=True");
        // optionsBuilder.UseSqlServer("server=DEVELOPER2;database=IliaDabirkhaneMVC;user ID=sa;password=12345@Iran;MultipleActiveResultSets=True;TrustServerCertificate=True");
        // optionsBuilder.UseSqlServer("data source=.;initial catalog = OmidApp;integrated security=true;MultipleActiveResultSets=True;TrustServerCertificate=True");
        optionsBuilder.UseSqlServer("data source=.;initial catalog = IliaDabir22;integrated security=true;MultipleActiveResultSets=True;TrustServerCertificate=True");
    }
}