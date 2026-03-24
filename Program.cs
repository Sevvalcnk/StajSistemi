using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StajSistemi.data;
using StajSistemi.Repositories.Abstract;   // ✅ SADECE BU KALMALI
using StajSistemi.Repositories.Concrete;   // ✅ SADECE BU KALMALI
using StajSistemi.Mapping;
using StajSistemi.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using StajSistemi.Services;
using StajSistemi.Helpers;

// ❌ 'using StajSistemi.Repositories.UnitOfWork;' SATIRINI SİLDİK! 
// Çünkü artık dosyalarımız Abstract ve Concrete içinde.

var builder = WebApplication.CreateBuilder(args);

// --- Standart Servisler ---
builder.Services.AddControllersWithViews();

// ✅ IP Loglama için Gerekli
builder.Services.AddHttpContextAccessor();

// --- Veri Tabanı Bağlantısı ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- REPOSITORY VE UNIT OF WORK KAYDI ---
// Burada 'IUnitOfWork' Abstract'tan, 'UnitOfWork' Concrete'ten gelir.
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(typeof(MapProfile));

// --- MAİL SERVİSİ KAYDI ---
builder.Services.AddTransient<IEmailSender, EmailSender>();

// --- IDENTITY AYARLARI ---
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<TurkishIdentityErrorDescriber>();

// --- YETKİLENDİRME POLİTİKALARI ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
    options.AddPolicy("AdvisorOnly", policy => policy.RequireRole("Advisor"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Cookie Ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

var app = builder.Build();

// --- Pipeline Ayarları ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --- ROL SEEDING (Sistem İlk Açıldığında Rolleri Oluşturur) ---
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    string[] roles = { "Student", "Advisor", "Admin" };

    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new AppRole { Name = roleName, NormalizedName = roleName.ToUpper() });
        }
    }
}

app.Run();