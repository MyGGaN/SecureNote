using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SecureNote.Models;

var builder = WebApplication.CreateBuilder(args);

// Setup Sqlite
var conn = new SqliteConnection("Filename=:memory:");
conn.Open();
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(conn));

// Add services to the container.
builder.Services.AddControllersWithViews();
var app = builder.Build();
AddCustomerData(app);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();


static void AddCustomerData(WebApplication app)
{
    var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetService<AppDbContext>();

    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();

    db.SaveChanges();
}


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<PwdResetCode> PwdResetCodes { get; set; }
}
