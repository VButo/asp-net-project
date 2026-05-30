using Microsoft.EntityFrameworkCore;
using API_tester.Models;
namespace API_tester.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApiWorkspace> ApiWorkspaces { get; set; }
    public DbSet<ApiCollection> ApiCollections { get; set; }
    public DbSet<ApiRequest> ApiRequests { get; set; }
    public DbSet<ApiHeader> ApiHeaders { get; set; }
    public DbSet<ApiResponse> ApiResponses { get; set; }
    public DbSet<ApiEnvironment> ApiEnvironments { get; set; }
    public DbSet<EnvironmentVariable> EnvironmentVariables { get; set; }
    public DbSet<RequestTag> RequestTags { get; set; }
    public DbSet<User> Users { get; set; }

    public DbSet<RequestTagMap> RequestTagMaps { get; set; }
    public DbSet<RequestEnvironmentLink> RequestEnvironmentLinks { get; set; }
    public DbSet<WorkspaceMembership> WorkspaceMemberships { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApiWorkspace>().ToTable("ApiWorkspace");
        modelBuilder.Entity<ApiCollection>().ToTable("ApiCollection");
        modelBuilder.Entity<ApiRequest>().ToTable("ApiRequest");
        modelBuilder.Entity<ApiHeader>().ToTable("ApiHeader");
        modelBuilder.Entity<ApiResponse>().ToTable("ApiResponse");
        modelBuilder.Entity<ApiEnvironment>().ToTable("ApiEnvironment");
        modelBuilder.Entity<EnvironmentVariable>().ToTable("EnvironmentVariable");
        modelBuilder.Entity<RequestTag>().ToTable("RequestTag");
        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<RequestTagMap>().ToTable("RequestTagMap");
        modelBuilder.Entity<RequestEnvironmentLink>().ToTable("RequestEnvironmentLink");
        modelBuilder.Entity<WorkspaceMembership>().ToTable("WorkspaceMembership");

        // Composite Keys
        modelBuilder.Entity<RequestTagMap>().HasKey(x => new { x.RequestId, x.TagId });
        modelBuilder.Entity<RequestEnvironmentLink>().HasKey(x => new { x.RequestId, x.EnvironmentId });
        modelBuilder.Entity<WorkspaceMembership>().HasKey(x => new { x.UserId, x.WorkspaceId });

        // ApiWorkspace -> ApiCollection (1-N)
        modelBuilder.Entity<ApiCollection>()
            .HasOne(c => c.Workspace)
            .WithMany(w => w.Collections)
            .HasForeignKey(c => c.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApiWorkspace -> ApiEnvironment (1-N)
        modelBuilder.Entity<ApiEnvironment>()
            .HasOne(e => e.Workspace)
            .WithMany(w => w.Environments)
            .HasForeignKey(e => e.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApiWorkspace -> WorkspaceMembership (1-N)
        modelBuilder.Entity<WorkspaceMembership>()
            .HasOne(m => m.Workspace)
            .WithMany(w => w.Members)
            .HasForeignKey(m => m.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> WorkspaceMembership (1-N)
        modelBuilder.Entity<WorkspaceMembership>()
            .HasOne(m => m.User)
            .WithMany(u => u.Memberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> ApiWorkspace Owner (1-N)
        modelBuilder.Entity<ApiWorkspace>()
            .HasOne(w => w.OwnerUser)
            .WithMany()
            .HasForeignKey(w => w.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ApiCollection -> ApiRequest (1-N)
        modelBuilder.Entity<ApiRequest>()
            .HasOne(r => r.Collection)
            .WithMany(c => c.Requests)
            .HasForeignKey(r => r.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApiRequest -> ApiHeader (1-N)
        modelBuilder.Entity<ApiHeader>()
            .HasOne(h => h.Request)
            .WithMany(r => r.Headers)
            .HasForeignKey(h => h.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApiRequest -> ApiResponse (1-N)
        modelBuilder.Entity<ApiResponse>()
            .HasOne(resp => resp.Request)
            .WithMany(r => r.Responses)
            .HasForeignKey(resp => resp.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApiEnvironment -> EnvironmentVariable (1-N)
        modelBuilder.Entity<EnvironmentVariable>()
            .HasOne(ev => ev.Environment)
            .WithMany(e => e.Variables)
            .HasForeignKey(ev => ev.EnvironmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // RequestTag -> RequestTagMap (1-N)
        modelBuilder.Entity<RequestTagMap>()
            .HasOne(rtm => rtm.Tag)
            .WithMany(t => t.RequestLinks)
            .HasForeignKey(rtm => rtm.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApiRequest -> RequestTagMap (1-N)
        modelBuilder.Entity<RequestTagMap>()
            .HasOne(rtm => rtm.Request)
            .WithMany(r => r.TagLinks)
            .HasForeignKey(rtm => rtm.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApiEnvironment -> RequestEnvironmentLink (1-N)
        modelBuilder.Entity<RequestEnvironmentLink>()
            .HasOne(rel => rel.Environment)
            .WithMany(e => e.RequestLinks)
            .HasForeignKey(rel => rel.EnvironmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApiRequest -> RequestEnvironmentLink (1-N)
        modelBuilder.Entity<RequestEnvironmentLink>()
            .HasOne(rel => rel.Request)
            .WithMany(r => r.EnvironmentLinks)
            .HasForeignKey(rel => rel.RequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}