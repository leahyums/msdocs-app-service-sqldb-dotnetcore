using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Data;
var builder = WebApplication.CreateBuilder(args);

// Add database context and cache
if(builder.Environment.IsDevelopment())
{
    // use same prod setup for local dev since it's for testing on always encrypted which need to connect Azure Key Vault either way...
    // builder.Services.AddDbContext<MyDatabaseContext>(options =>
    //     options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection")));
    // builder.Services.AddDistributedMemoryCache();

    builder.Services.AddDbContext<MyDatabaseContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING") + ";Column Encryption Setting=Enabled"));
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
