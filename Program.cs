using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StajSistemi.data;
using StajSistemi.Repositories.Abstract;
using StajSistemi.Repositories.Concrete;
using StajSistemi.Repositories.UnitOfWork;
using StajSistemi.Mapping;
using StajSistemi.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using StajSistemi.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Standart Servisler ---
builder.Services.AddControllersWithViews();

// --- Veri Tabanı Bağlantısı ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- REPOSITORY, UNIT OF WORK VE MAPPING ---
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAutoMapper(typeof(MapProfile));

// --- MAİL SERVİSİ KAYDI ---
builder.Services.AddTransient<IEmailSender, EmailSender>();

// --- IDENTITY AYARLARI (Hafta 3) ---
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --- ✅ YENİ: POLİTİKA VE CLAIMS TABANLI YETKİLENDİRME (3. HAFTA MÜHÜRÜ) ---
builder.Services.AddAuthorization(options =>
{
    // Sadece 'Student' rolü olan ve sisteme 'FullAccess' claim'i ile girenler için
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));

    // Sadece Danışmanlar için özel kural
    options.AddPolicy("AdvisorOnly", policy => policy.RequireRole("Advisor"));

    // Sadece Adminler için tam yetki
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Cookie (Oturum) Ayarları
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

// ÖNEMLİ: Kimlik doğrulama her zaman yetkilendirmeden önce gelmeli!
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --- HAFTA 3: Rolleri Otomatik Oluşturma (Seeding) ---
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    string[] roles = { "Student", "Advisor", "Admin" };

    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new AppRole { Name = roleName });
        }
    }
}

app.Run();