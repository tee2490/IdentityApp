global using IdentityApp.Models;
global using IdentityApp.Data;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.EntityFrameworkCore;
global using IdentityApp.Setvices.IService;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using IdentityApp.Setvices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddDbContext<DataContext>();

#region Identityสร้างเซอร์วิส User,Role (ระวังการเรียงลำดับ)
builder.Services.AddIdentityCore<ApplicationUser>(opt =>
{
    opt.User.RequireUniqueEmail = true;
}).AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DataContext>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
#endregion

//ยืนยัน Token ที่ได้รับว่าถูกต้องหรือไม่บนเซิฟเวอร์
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                       .AddJwtBearer(opt =>
                       {
                           opt.TokenValidationParameters = new TokenValidationParameters
                           {
                               ValidateIssuer = false,
                               ValidateAudience = false,
                               ValidateLifetime = true,
                               ValidateIssuerSigningKey = true,
                               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                                   .GetBytes(builder.Configuration["JWTSettings:TokenKey"]))
                           };
                       });

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IAccountService,AccountService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

#region  //สร้างฐานข้อมูลอัตโนมัติ
using var scope = app.Services.CreateScope(); //using หลังทำงานเสร็จจะถูกทำลายจากMemory
var context = scope.ServiceProvider.GetRequiredService<DataContext>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

try
{
    await context.Database.MigrateAsync();   //สร้าง DB ให้อัตโนมัติถ้ายังไม่มี
}
catch (Exception ex)
{
    logger.LogError(ex, "Problem migrating data");
}
#endregion

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
