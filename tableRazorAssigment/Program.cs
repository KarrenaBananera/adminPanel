using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using tableRazorAssigment.Configuration;
using tableRazorAssigment.Data;
using tableRazorAssigment.Middleware;
using tableRazorAssigment.Services;
using tableRazorAssigment.Services.Implenetation;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddHttpContextAccessor();
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

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
});

builder.Configuration.AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services.AddSingleton(ConfigureEmail(builder.Configuration));
builder.Services.AddScoped<SignInManager<User>, CustomSignInManager>();
builder.Services.AddScoped<CustomSignInManager, CustomSignInManager>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserRegisterService, UserRegisterService>();
builder.Services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();
builder.Services.AddScoped<IUserFetcher, UserFetcher>();
builder.Services.AddScoped<IUserManagerService, UserManagerService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();


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
app.MapStaticAssets();
app.UseMiddleware<UserLastSeenMiddleware>();
app.UseAuthorization();
app.UseMiddleware<LogOutInvalidUserMiddleware>();

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