using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Data;
using LogisControlAPI.Services;
using LogisControlAPI.Interfaces;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LogisControlAPI.Auxiliar;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

//Configurar a conexão ao SQL Server
builder.Services.AddDbContext<LogisControlContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Ativar controladores para API REST
builder.Services.AddControllers();
builder.Services.AddScoped<UtilizadorService>();
builder.Services.AddScoped<ComprasService>();

//Configurar o serviço de email
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<NotificationService>();

//Producao
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<VerificacaoStockEncomendaService>();
builder.Services.AddScoped<ProducaoService>();

//Produto Service
builder.Services.AddScoped<ProdutoService>();

//Configurar o serviço de Telegram
// Configura o binding da secção "Telegram" do appsettings.json
builder.Services.Configure<TelegramConfig>(builder.Configuration.GetSection("TelegramConfig"));
// Regista o serviço TelegramService com HttpClient e configuração injetada
builder.Services.AddSingleton<ITelegramService>(sp =>
{
    var config = sp.GetRequiredService<IOptions<TelegramConfig>>().Value;
    var httpClient = new HttpClient();
    return new TelegramService(httpClient, config);
});


//Configurar o serviço Pedidos Manutenção
builder.Services.AddScoped<ManutencaoService>();


//Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AuthSettings.PrivateKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

//mais simples
//builder.Services.AddAuthorization(); // Add default authorization services
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("Operador", policy => policy.RequireRole("Operador"));
    options.AddPolicy("Gestor", policy => policy.RequireRole("Gestor"));
    options.AddPolicy("Manutencao", policy => policy.RequireRole("Manutencao"));
    options.AddPolicy("Compras", policy => policy.RequireRole("Compras"));
});

builder.Services.AddTransient<AuthService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LogisControl API", Version = "v1" });

    // Adiciona suporte para comentários XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter your JWT token in this field",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
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
            new string[] {}
        }
            };

    c.AddSecurityRequirement(securityRequirement);
});

var app = builder.Build();

app.UseCors("CorsPolicy");

// **Importante**: primeiro autenticação, depois autorização
app.UseAuthentication();
app.UseAuthorization();

//Ativar Swagger
app.UseSwagger();
app.UseSwaggerUI();

//Ativar autorização
app.UseAuthorization();

//Mapear controladores da API
app.MapControllers();
app.Run();
