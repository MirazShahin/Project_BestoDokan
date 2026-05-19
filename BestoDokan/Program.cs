using BestoDokan.Application.Interfaces;
using BestoDokan.Application.Validators;
using BestoDokan.Exceptions;
using BestoDokan.Infrastructure.Data;
using BestoDokan.Infrastructure.Repositories;
using BestoDokan.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args); /* এটি অ্যাপের মূল ভিত্তি। (Host Builder)
                                           * এটি এনভায়রনমেন্ট,  কনফিগারেশন (appsettings.json) 
                                           * এবং অন্যান্য ডিফল্ট সেটিংস লোড করে।*/

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); /*এটি appsettings.json ফাইল
                                                                            * থেকে ডাটাবেসের 
                                                                            * ঠিকানা (Address) খুঁজে বের 
                                                                            * করে আনে।*/

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));       /*এটি অ্যাপকে জানায় যে আমরা Entity Framework Core 
                                                     * ব্যবহার করছি এবং ডাটাবেস হিসেবে SQL Server 
                                                     * ব্যবহার হবে। এটি ApplicationDbContext-কে 
                                                     * একটি সার্ভিস হিসেবে রেজিস্টার করে।*/

builder.Services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IProductRepository, ProductRepository>(); /*একে বলা হয় Dependency Injection। এর 
                                                                      * মানে হলো, যখনই তোমার প্রজেক্টে 
                                                                      * কেউ IProductRepository ইন্টারফেসটি 
                                                                      * খুঁজবে, অ্যাপ তাকে অটোমেটিক 
                                                                      * ProductRepository ক্লাসের 
                                                                      * একটি কপি (Instance) দিয়ে দিবে।*/

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// টোকেন সার্ভিসের ডিপেন্ডেন্সি ইনজেকশন রেজিস্ট্রেশন
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// Add services to the container.

builder.Services.AddControllers(); /*এটি তোমার অ্যাপকে ক্ষমতা দেয় যেন 
                                    * সে Controllers (যেখানে এপিআই-এর 
                                    * মেইন লজিক থাকে) চিনতে পারে এবং 
                                    * ব্যবহার করতে পারে।*/

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // যেকোনো ডোমেইন থেকে রিকোয়েস্ট এলাউ করবে
              .AllowAnyMethod()   // GET, POST, PUT, DELETE সব মেথড এলাউ করবে
              .AllowAnyHeader();  // সব ধরণের হেডার (যেমন Authorization Header) এলাউ করবে
    });
});
// ASP.NET Core Identity কনফিগারেশন (কাস্টম ApplicationUser সহ)
builder.Services.AddIdentity<BestoDokan.Domain.Entities.ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddRoles<IdentityRole>()
.AddDefaultTokenProviders();

// কুকি-বেসড JWT Authentication কনফিগারেশন
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretKeyDefault1234567890";

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // ব্রাউজার থেকে পাঠানো কুকি থেকে টোকেন রিড করার কাস্টম ইভেন্ট
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // রিকোয়েস্টের কুকি চেক করে যদি "jwt_token" পাওয়া যায়, তবে সেটাকে অথেনটিকেশনের জন্য পাস করা হবে
            if (context.Request.Cookies.ContainsKey("jwt_token"))
            {
                context.Token = context.Request.Cookies["jwt_token"];
            }
            return Task.CompletedTask;
        }
    };
});


var app = builder.Build(); /*এটি একটি দেয়ালের মতো। এর উপরের সব কাজ হলো 
                            * রেজিস্ট্রেশন (কী কী থাকবে তা ঠিক করা)। 
                            * এই লাইনের পর অ্যাপটি তৈরি 
                            * (Build) হয়ে যায় এবং এরপর 
                            * আমরা ঠিক করি অ্যাপটি কীভাবে চলবে।*/

// Configure the HTTP request pipeline.

/*Middleware হলো আপনার অ্যাপ্লিকেশনের এমন কিছু ছোট ছোট কোড 
 * বা কম্পোনেন্ট যা Request এবং Response-এর মাঝখানে দাঁড়িয়ে থাকে।

যখন একজন ইউজার আপনার ই-কমার্স সাইটে কোনো রিকোয়েস্ট পাঠায়, 
সেটি সরাসরি কন্ট্রোলারে যায় না; বরং মাঝপথে থাকা কতগুলো 
"গেট" বা "চেকপোস্ট" পার হয়ে যায়। এই চেকপোস্টগুলোই হলো মিডলওয়্যার।*/

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseCors("AllowAll");
// সিকিউরিটি চেকপোস্ট মিডেলওয়্যারসমূহ (অবশ্যই সিরিয়াল ঠিক রাখতে হবে)
app.UseAuthentication(); // আগে চেক করবে ইউজার আসলে কে? (কুকি থেকে টোকেন চেক করবে)
app.UseAuthorization();  // তারপর চেক করবে ইউজারের এই এপিআই অ্যাক্সেস করার পারমিশন আছে কি না।

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Admin", "Customer" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

app.Run();