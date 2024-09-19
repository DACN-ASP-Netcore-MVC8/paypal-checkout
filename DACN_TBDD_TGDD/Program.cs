using DACN_TBDD_TGDD.Areas.Admin.Repository;
using DACN_TBDD_TGDD.Models;
using DACN_TBDD_TGDD.Models.Services;
using DACN_TBDD_TGDD.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;


var builder = WebApplication.CreateBuilder(args);

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
// Connect to the database
builder.Services.AddDbContext<DataContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
//Add Email sender
builder.Services.AddTransient<IEmailSender,EmailSender>();



builder.Services.AddSingleton(x=> new PaypalClient(
	builder.Configuration["PayPal: ClientId"],
    builder.Configuration["PayPal: ClientSecret"],
    builder.Configuration["PayPal: Mode"]
   ) );

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.IsEssential = true;
});

builder.Services.AddIdentity<AppUserModel, IdentityRole>()
	.AddEntityFrameworkStores<DataContext>()  
	.AddDefaultTokenProviders();



builder.Services.Configure<IdentityOptions>(options =>
{
	// Password settings.
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
	options.Password.RequiredLength = 4;
	
	options.User.RequireUniqueEmail = true;
});

builder.Services.AddSignalR();
var app = builder.Build();



app.UseStatusCodePagesWithRedirects("Home/Error?statuscode={0}");

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())   
{
	app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();// dang nhap truoc 
app.UseAuthorization();//kiem tra quyen


app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Product}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "category",
    pattern: "category/{slug?}", // Adjusted pattern
    defaults: new { controller = "Category", action = "Index" });
app.MapControllerRoute(
    name: "brand",
    pattern: "brand/{slug?}", // Adjusted pattern
    defaults: new { controller = "Brand", action = "Index" });

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<ChatHub>("/chathub");

var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
SeeData.SeedingData(context);

app.Run();
