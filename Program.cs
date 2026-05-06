using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StajSistemi.data;
using StajSistemi.Repositories.Abstract;
using StajSistemi.Repositories.Concrete;
using StajSistemi.Mapping;
using StajSistemi.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using StajSistemi.Services;
using StajSistemi.Helpers;
using StajSistemi.Hubs;
using StajSistemi.Filters;
using Serilog;
using Microsoft.AspNetCore.Authorization;

// --- 🚀 SİBER GÜNLÜK (LOGGING) YAPILANDIRMASI ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/staj_sistemi_gunluk.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// --- Standart Servisler ---
builder.Services.AddControllersWithViews(options =>
{
    // 👈 MÜHÜR: Profil kontrolü aktif!
    options.Filters.Add<ProfileCompletionFilter>();
});

builder.Services.AddSignalR();
builder.Services.AddMemoryCache();

// ✅ Session (Oturum) Motoru Kaydı
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// --- Veri Tabanı Bağlantısı ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- REPOSITORY VE UNIT OF WORK KAYDI ---
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

// --- 🛡️ COOKIE VE AJAX YETKİ AYARLARI (KRİTİK DÜZELTME) ---
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);

    // 🚀 SİBER MÜHÜR: AJAX isteklerinde HTML (Login sayfası) dönmesini engelleyen blok
    options.Events.OnRedirectToLogin = context =>
    {
        // İstek AJAX mı yoksa Department API'sine mi gidiyor?
        bool isAjax = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        bool isDepartmentApi = context.Request.Path.StartsWithSegments("/Department");

        if (isAjax || isDepartmentApi)
        {
            // Eğer AJAX ise sakın yönlendirme yapma! Sadece "Yetki Yok" (401) de.
            // Bu sayede JavaScript login sayfasını değil, boş listeyi anlar.
            context.Response.StatusCode = 401;
        }
        else
        {
            // Normal sayfa isteklerini giriş sayfasına yönlendir.
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// --- Pipeline Ayarları (Siber Kalkan) ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Session özelliğini aktifleştiriyoruz
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// 🚀 CANLI YAYIN KANALI
app.MapHub<ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --- ROL SEEDING ---
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