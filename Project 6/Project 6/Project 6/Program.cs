using Microsoft.EntityFrameworkCore;
using Project_6.Models;
using Rotativa.AspNetCore;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnectionString")));


builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});



// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthorization();
app.UseSession();


RotativaConfiguration.Setup("wwwroot", "Rotativa");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=HomePages}/{action=Index}/{id?}");

app.Run();
