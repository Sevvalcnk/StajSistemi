using Microsoft.EntityFrameworkCore;
using StajSistemi.data;
// --- YENÝ EKLENEN USÝNG SATIRLARI ---
using StajSistemi.Repositories.Abstract;
using StajSistemi.Repositories.Concrete;
using StajSistemi.Repositories.UnitOfWork;
using StajSistemi.Mapping;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- Veri Tabaný Baðlantýsý ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- ?? YENÝ MÝMARÝ KAYITLARI BURAYA GELÝYOR ---

// 1. Repository Kaydý: Her tablo için tek tek yazmak yerine Generic yapýyý tanýtýyoruz.
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// 2. Unit of Work Kaydý: Tüm iþlemleri yöneten "Müdürü" tanýtýyoruz.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 3. AutoMapper Kaydý: DTO dönüþümlerini yapacak motoru tanýtýyoruz.
builder.Services.AddAutoMapper(typeof(MapProfile));

// ----------------------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();