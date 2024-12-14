using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using DotNetCoreSqlDb.Data;
var builder = WebApplication.CreateBuilder(args);


// Add database context and cache
if(builder.Environment.IsDevelopment())
{
    // use same prod setup for local dev since it's for testing on always encrypted which need to connect Azure Key Vault either way...
    // builder.Services.AddDbContext<MyDatabaseContext>(options =>
    //     options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection")));
    // builder.Services.AddDistributedMemoryCache();

    if (Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") != null)
    {
        /
         Console.WriteLine("// Running in Azure App Service, use managed identity");
        var azureSqlConnection = new SqlConnection(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING"));
        azureSqlConnection.AccessToken = new DefaultAzureCredential().GetToken(
            new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" })).Token;
        builder.Services.AddDbContext<MyDatabaseContext>(options =>
            options.UseSqlServer(azureSqlConnection));
    }
    else
    {
        Console.WriteLine("// Not running in Azure App Service, use connection string without managed identity");
        // Not running in Azure App Service, use connection string without managed identity
        builder.Services.AddDbContext<MyDatabaseContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection")));
        builder.Services.AddDistributedMemoryCache();
    }

    // debugging
    Console.WriteLine("AZURE_SQL_CONNECTIONSTRING: " + builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING"));
    // WEBSITE_INSTANCE_ID
    Console.WriteLine("WEBSITE_INSTANCE_ID: " + Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));


    // builder.Services.AddDbContext<MyDatabaseContext>(options =>
    //     // options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING") + ";Column Encryption Setting=Enabled"));
    //     options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")));
    
    builder.Services.AddStackExchangeRedisCache(options =>
    {
    options.Configuration = builder.Configuration["AZURE_REDIS_CONNECTIONSTRING"];
    options.InstanceName = "SampleInstance";
    });
}
else
{
    builder.Services.AddDbContext<MyDatabaseContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING") + ";Column Encryption Setting=Enabled"));
    builder.Services.AddStackExchangeRedisCache(options =>
    {
    options.Configuration = builder.Configuration["AZURE_REDIS_CONNECTIONSTRING"];
    options.InstanceName = "SampleInstance";
    });
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add App Service logging
builder.Logging.AddAzureWebAppDiagnostics();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Todos}/{action=Index}/{id?}");

app.Run();
