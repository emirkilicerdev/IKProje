using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models; // Swagger için gerekli

var builder = WebApplication.CreateBuilder(args);

// Uygulamanýza servisleri ekleyin (Dependency Injection Container'a).

// 1. Controller'larý API endpoint'leri olarak kaydet
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// 2. API Explorer ve Swagger/OpenAPI dokümantasyonu için servisleri ekle
// Swagger, API'nizi test etmek ve dokümante etmek için harika bir araçtýr.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Management API", Version = "v1" });

    // Swagger UI'da JWT yetkilendirme seçeneði eklemek için SecurityDefinition tanýmla
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", // HTTP baþlýðýnýn adý
        Type = SecuritySchemeType.ApiKey, // Yetkilendirme þemasý tipi
        Scheme = "Bearer", // Yetkilendirme þemasý (JWT için "Bearer")
        BearerFormat = "JWT", // Bearer formatý
        In = ParameterLocation.Header, // Token'ýn HTTP baþlýðýnda olacaðýný belirtir
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
    });

    // Tanýmlanan "Bearer" güvenlik þemasýný tüm API endpoint'leri için zorunlu kýl
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {} // Kapsamlar (scopes) boþ býrakýldý
        }
    });
});

// 3. Veritabaný Baðlamýný (DbContext) Dependency Injection'a ekle
// appsettings.json dosyasýndaki "DefaultConnection" stringini kullanýr.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. JWT Kimlik Doðrulama Ayarlarý
// JWT Bearer þemasýný kullanarak kimlik doðrulamayý yapýlandýrýr.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Jwt:Key deðeri null ise hata fýrlat veya varsayýlan bir deðer kullan
        var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Token'ýn yayýncýsýný (Issuer) doðrula
            ValidateIssuer = true,
            // Token'ýn hedef kitlesini (Audience) doðrula
            ValidateAudience = true,
            // Token'ýn geçerlilik süresini doðrula
            ValidateLifetime = true,
            // Token'ýn imzalama anahtarýný doðrula
            ValidateIssuerSigningKey = true,

            // appsettings.json'dan alýnan geçerli yayýncý
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            // appsettings.json'dan alýnan geçerli hedef kitle
            ValidAudience = builder.Configuration["Jwt:Audience"],
            // appsettings.json'dan alýnan gizli anahtarla imzalama anahtarýný oluþtur
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)) // Null kontrolü yapýldý
        };
    });

// 5. CORS (Cross-Origin Resource Sharing) Ayarlarý
// Frontend uygulamanýzýn (örn. Angular) backend API'nize eriþmesine izin verir.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", // Politika adý
        builder =>
        {
            // Angular uygulamanýzýn URL'sini buraya ekleyin.
            builder.WithOrigins("http://localhost:4200")
                   .AllowAnyHeader() // Tüm HTTP baþlýklarýna izin ver
                   .AllowAnyMethod(); // Tüm HTTP metotlarýna (GET, POST, PUT, DELETE vb.) izin ver
        });
});

// Uygulamayý oluþtur
var app = builder.Build();

// HTTP Ýstek Ýþlem Hattýný (Request Pipeline) yapýlandýrýn.

// Geliþtirme ortamýnda Swagger UI'ý etkinleþtir
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Swagger JSON endpoint'ini etkinleþtirir
    app.UseSwaggerUI(); // Swagger UI'ý etkinleþtirir
}

// HTTP'den HTTPS'ye yönlendirme (güvenlik için önerilir)
app.UseHttpsRedirection();

// Yönlendirme middleware'i: Ýsteklerin hangi endpoint'e gideceðini belirler
app.UseRouting(); // CORS'tan önce olmalý

// CORS politikasýný uygula
app.UseCors("AllowAngularApp"); // Tanýmladýðýnýz CORS politikasýný kullan

// Kimlik doðrulama middleware'i: Kullanýcýnýn kimliðini doðrular
app.UseAuthentication();

// Yetkilendirme middleware'i: Kimliði doðrulanmýþ kullanýcýnýn belirli kaynaklara eriþim yetkisini kontrol eder
app.UseAuthorization();

// Controller'larý HTTP isteklerine eþler
app.MapControllers();

// Uygulamayý çalýþtýr
app.Run();
