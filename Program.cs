using _16Nov_task.DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(opt=>opt.UseSqlServer(
    "server=DESKTOP-NK9HMU9\\MSSQLSERVER01;database=ProniaTask;trusted_connection=true;integrated security=true;encrypt=false"));
var app = builder.Build();


app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    "default",
    "{controller=home}/{action=index}/{id?}"
    );

app.Run();
