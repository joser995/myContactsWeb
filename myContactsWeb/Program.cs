using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using myContactsWeb.Components;
using myContactsWeb.Components.Account;
using myContactsWeb.Data;
using myContactsWeb.Services.Implementations;
using myContactsWeb.Services.Interfaces;
using Polly;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("UserAuthDbConnection") ?? throw new InvalidOperationException("Connection string 'UserAuthDbConnection' not found.");
builder.Services.AddDbContext<AuthenticationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
    .AddEntityFrameworkStores<AuthenticationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<User>, IdentityNoOpEmailSender>();

#region HttpClients
builder.Services.AddHttpClient<IContactsApiService, ContactsApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ContactsApiUrl"] ?? throw new Exception("ContactsApiUrl not found"));
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "myContactsWeb");
    client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
    client.DefaultRequestHeaders.Add("Keep-Alive", "timeout=600, max=100");
    client.DefaultRequestHeaders.Remove("X-Powered-By");
    client.DefaultRequestHeaders.Remove("X-AspNetVersion");
    client.DefaultRequestHeaders.Remove("X-AspNetMvcVersion");
    client.DefaultRequestHeaders.Remove("Server");
})
    .ConfigurePrimaryHttpMessageHandler(
        MessageHandler =>
        {
            if (bool.Parse(builder.Configuration["NeedsProxy"] ?? throw new Exception("NeedsProxy not found")))
            {
                var proxy = new WebProxy(builder.Configuration["ProxyUrl"], true);
                proxy.Credentials = new NetworkCredential(builder.Configuration["ProxyUsername"], builder.Configuration["ProxyPassword"]);
                return new HttpClientHandler { Proxy = proxy, UseProxy = true };
            }
            else
            {
                return new HttpClientHandler();
            }

        })
        .AddTransientHttpErrorPolicy(policyBuilder =>
            policyBuilder.WaitAndRetryAsync(3, retryNumber => TimeSpan.FromMilliseconds(600))
        );
#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
