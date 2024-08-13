
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QLSVVV.Data;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình dịch vụ DbContext
builder.Services.AddDbContext<QLSVVVContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QLSVVVContext") ?? throw new InvalidOperationException("Connection string 'QLSVVVContext' not found.")));
builder.Services.AddDbContext<ICourseContext, QLSVVVContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QLSVVVContext") ?? throw new InvalidOperationException("Connection string 'QLSVVVContext' not found.")));
builder.Services.AddDbContext<IStudentContext, QLSVVVContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QLSVVVContext") ?? throw new InvalidOperationException("Connection string 'QLSVVVContext' not found.")));
// Cấu hình dịch vụ Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cấu hình xác thực với cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Authentication/Login";
        options.AccessDeniedPath = "/Authentication/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Teacher", policy => policy.RequireRole("Teacher"));
    options.AddPolicy("Student", policy => policy.RequireRole("Student"));
});

builder.Services.AddScoped<IQLSVVVContext, QLSVVVContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Sử dụng Session

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
