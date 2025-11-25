using CSDLPT.Web.Repositories;
using Microsoft.AspNetCore.Builder;
using CSDLPT.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IDoiBongRepo, DoiBongRepo>();
builder.Services.AddScoped<ICauThuRepo, CauThuRepo>();
builder.Services.AddScoped<ITranDauRepo, TranDauRepo>();
builder.Services.AddScoped<ISanRepo, SanRepo>();
builder.Services.AddScoped<IThamGiaRepo, ThamGiaRepo>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "CSDLPT.AuthCookie";
        options.LoginPath = "/Account/Login"; 
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });


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
app.UseAuthentication();


app.MapControllerRoute(
    name: "default",
   pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
