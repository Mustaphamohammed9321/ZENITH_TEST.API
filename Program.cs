using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Configuration;
using System.Text;
using ZENITH_TEST.CORE.Helpers;
using ZENITH_TEST.CORE.Interface;
using ZENITH_TEST.CORE.IRepositories;
using ZENITH_TEST.CORE.Repositories;
using ZENITH_TEST.CORE.Repository;
using ZENITH_TEST.INFRASTRUCTURE.Services.User.Implementations;
using ZENITH_TEST.INFRASTRUCTURE.Services.User.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var key = Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("JWT:SecretKey")).ToString();
var connection = builder.Configuration.GetValue<string>("ConnectionStrings:ConStr"); //local server

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddSingleton<IUserService>(gt => new UserService(gt.GetRequiredService<IUserRepository>()));
builder.Services.AddSingleton<IUserRepository>(new UserRepository(connection));
builder.Services.AddSingleton<IJWTAuthManager>(new JWTAuthManager(key, builder.Configuration));

builder.Services.AddSingleton<IAppSettingsFactory>(new AppSettingsFactory(builder.Configuration));
builder.Services.AddSingleton<IRoleClass>(new RoleClass(builder.Configuration.GetValue<string>("ConnectionStrings:ConStr")));
builder.Services.AddSingleton<ICrypt>(new Crypt(builder.Configuration.GetValue<string>("ConnectionStrings:CryptKey")));
builder.Services.AddSingleton<IJWTAuthManager>(new JWTAuthManager(key: key.ToString(), configuration: builder.Configuration));
builder.Services.AddSingleton<IRoleRepository>(new RoleRepository(connection, config: builder.Configuration));


builder.Services.AddControllers();


#region JWT ON SWAGGER
//add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = builder.Configuration["JWT:Issuer"],
         ValidAudience = builder.Configuration["JWT:Issuer"],
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
     };
 });


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "ZENITH_TEST.API",
            Version = "v1"
        });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
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
                        new string[] {}
                    }
                });

});
#endregion



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
