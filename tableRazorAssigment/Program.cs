using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using tableRazorAssigment.Configuration;
using tableRazorAssigment.Data;
using tableRazorAssigment.Middleware;
using tableRazorAssigment.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 1;          
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 0;      
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Configuration.AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services.AddSingleton(ConfigureEmail(builder.Configuration));
builder.Services.AddScoped<SignInManager<User>, CustomSignInManager>();
builder.Services.AddScoped<CustomSignInManager, CustomSignInManager>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddRazorPages();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<UserStatusMiddleware>();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

EmailSettings ConfigureEmail(ConfigurationManager configuration)
{
    return new EmailSettings
    {
        Host = configuration["EMAIL_HOST"] ?? "smtp.gmail.com",
        Port = int.Parse(configuration["EMAIL_PORT"] ?? "587"),
        Username = configuration["EMAIL_USERNAME"] ?? "",
        Password = configuration["EMAIL_PASSWORD"] ?? "",
        From = configuration["EMAIL_FROM"] ?? "",
        EnableSsl = bool.Parse(configuration["EMAIL_ENABLE_SSL"] ?? "true")
    };
}