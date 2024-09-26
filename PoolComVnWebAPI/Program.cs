using BusinessObject.Models;
using DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PoolComVnWebAPI.Authorization;
using PoolComVnWebAPI.Common;
using PoolComVnWebAPI.DTO;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var builder2 = new ConfigurationBuilder()
.SetBasePath(Directory.GetCurrentDirectory())
.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
IConfigurationRoot configuration = builder2.Build();
builder.Services.AddDbContext<poolcomvnContext>(options => options.UseSqlServer(configuration.GetConnectionString("PoolCom")));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<NewsDAO>();
builder.Services.AddScoped<AccountDAO>();
builder.Services.AddScoped<ClubDAO>();
builder.Services.AddScoped<ClubPostDAO>();
builder.Services.AddScoped<PlayerDAO>();
builder.Services.AddScoped<TournamentDAO>();
builder.Services.AddScoped<TableDAO>();
builder.Services.AddScoped<MatchDAO>();
builder.Services.AddScoped<AddressDAO>();
builder.Services.AddScoped<SoloMatchDAO>();
builder.Services.AddScoped<UserDAO>();
builder.Services.AddScoped<ContactDAO>();

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddCors();
// Add services to the container
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option =>
    {
        option.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    }
    );

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("CustomPolicy", policy => policy.Requirements.Add(new CustomAuthorizationRequirement(1)));
//});

builder.Services.AddSingleton<IAuthorizationHandler, CustomAuthorizationHandler>();
builder.Services.AddAutoMapper(typeof(MapperConfig));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(builder =>
{
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});
app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
