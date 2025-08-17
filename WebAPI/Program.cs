using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models; // Swagger i�in gerekli

var builder = WebApplication.CreateBuilder(args);

// Uygulaman�za servisleri ekleyin (Dependency Injection Container'a).

// 1. Controller'lar� API endpoint'leri olarak kaydet
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// 2. API Explorer ve Swagger/OpenAPI dok�mantasyonu i�in servisleri ekle
// Swagger, API'nizi test etmek ve dok�mante etmek i�in harika bir ara�t�r.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Management API", Version = "v1" });

    // Swagger UI'da JWT yetkilendirme se�ene�i eklemek i�in SecurityDefinition tan�mla
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", // HTTP ba�l���n�n ad�
        Type = SecuritySchemeType.ApiKey, // Yetkilendirme �emas� tipi
        Scheme = "Bearer", // Yetkilendirme �emas� (JWT i�in "Bearer")
        BearerFormat = "JWT", // Bearer format�
        In = ParameterLocation.Header, // Token'�n HTTP ba�l���nda olaca��n� belirtir
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
    });

    // Tan�mlanan "Bearer" g�venlik �emas�n� t�m API endpoint'leri i�in zorunlu k�l
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
            new string[] {} // Kapsamlar (scopes) bo� b�rak�ld�
        }
    });
});

// 3. Veritaban� Ba�lam�n� (DbContext) Dependency Injection'a ekle
// appsettings.json dosyas�ndaki "DefaultConnection" stringini kullan�r.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. JWT Kimlik Do�rulama Ayarlar�
// JWT Bearer �emas�n� kullanarak kimlik do�rulamay� yap�land�r�r.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Jwt:Key de�eri null ise hata f�rlat veya varsay�lan bir de�er kullan
        var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Token'�n yay�nc�s�n� (Issuer) do�rula
            ValidateIssuer = true,
            // Token'�n hedef kitlesini (Audience) do�rula
            ValidateAudience = true,
            // Token'�n ge�erlilik s�resini do�rula
            ValidateLifetime = true,
            // Token'�n imzalama anahtar�n� do�rula
            ValidateIssuerSigningKey = true,

            // appsettings.json'dan al�nan ge�erli yay�nc�
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            // appsettings.json'dan al�nan ge�erli hedef kitle
            ValidAudience = builder.Configuration["Jwt:Audience"],
            // appsettings.json'dan al�nan gizli anahtarla imzalama anahtar�n� olu�tur
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)) // Null kontrol� yap�ld�
        };
    });

// 5. CORS (Cross-Origin Resource Sharing) Ayarlar�
// Frontend uygulaman�z�n (�rn. Angular) backend API'nize eri�mesine izin verir.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", // Politika ad�
        builder =>
        {
            // Angular uygulaman�z�n URL'sini buraya ekleyin.
            builder.WithOrigins("http://localhost:4200")
                   .AllowAnyHeader() // T�m HTTP ba�l�klar�na izin ver
                   .AllowAnyMethod(); // T�m HTTP metotlar�na (GET, POST, PUT, DELETE vb.) izin ver
        });
});

// Uygulamay� olu�tur
var app = builder.Build();

// HTTP �stek ��lem Hatt�n� (Request Pipeline) yap�land�r�n.

// Geli�tirme ortam�nda Swagger UI'� etkinle�tir
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Swagger JSON endpoint'ini etkinle�tirir
    app.UseSwaggerUI(); // Swagger UI'� etkinle�tirir
}

// HTTP'den HTTPS'ye y�nlendirme (g�venlik i�in �nerilir)
app.UseHttpsRedirection();

// Y�nlendirme middleware'i: �steklerin hangi endpoint'e gidece�ini belirler
app.UseRouting(); // CORS'tan �nce olmal�

// CORS politikas�n� uygula
app.UseCors("AllowAngularApp"); // Tan�mlad���n�z CORS politikas�n� kullan

// Kimlik do�rulama middleware'i: Kullan�c�n�n kimli�ini do�rular
app.UseAuthentication();

// Yetkilendirme middleware'i: Kimli�i do�rulanm�� kullan�c�n�n belirli kaynaklara eri�im yetkisini kontrol eder
app.UseAuthorization();

// Controller'lar� HTTP isteklerine e�ler
app.MapControllers();

// Uygulamay� �al��t�r
app.Run();
