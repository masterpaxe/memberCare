using MemberCare.Api.Services;
using MemberCare.Api.Middleware;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddSingleton<SqlConnectionFactory>();
builder.Services.AddScoped<BranchContext>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<VisitorService>();
builder.Services.AddScoped<NewConvertService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<FollowUpService>();
builder.Services.AddScoped<ReportService>();

// Configure JWT authentication
var jwtSecret = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT:SecretKey is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "membercare-api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "membercare-client";

var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = securityKey,
        RoleClaimType = "role",
        NameClaimType = ClaimTypes.Name
    };
});

builder.Services.AddAuthorization(options =>
{
    // Super Admin can do everything
    options.AddPolicy("SuperAdmin", policy => policy.RequireClaim("role", "super_admin"));
    
    // Church Admin - everything except admin user management
    options.AddPolicy("ChurchAdmin", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("role", "super_admin") ||
            context.User.HasClaim("role", "church_admin")));
    
    // Member Management - super_admin and church_admin only
    options.AddPolicy("MemberManagement", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("role", "super_admin") ||
            context.User.HasClaim("role", "church_admin")));
    
    // Visitor/Convert Management - super_admin, church_admin, pastor, follow_up_officer
    options.AddPolicy("VisitorManagement", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("role", "super_admin") ||
            context.User.HasClaim("role", "church_admin") ||
            context.User.HasClaim("role", "pastor") ||
            context.User.HasClaim("role", "follow_up_officer")));
    
    // Attendance Management - super_admin, church_admin, attendance_officer
    options.AddPolicy("AttendanceManagement", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("role", "super_admin") ||
            context.User.HasClaim("role", "church_admin") ||
            context.User.HasClaim("role", "attendance_officer")));
    
    // Follow-Up Management - super_admin, church_admin, pastor, follow_up_officer
    options.AddPolicy("FollowUpManagement", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("role", "super_admin") ||
            context.User.HasClaim("role", "church_admin") ||
            context.User.HasClaim("role", "pastor") ||
            context.User.HasClaim("role", "follow_up_officer")));
    
    // Reports - super_admin, church_admin, pastor, finance_officer, report_viewer
    options.AddPolicy("Reports", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("role", "super_admin") ||
            context.User.HasClaim("role", "church_admin") ||
            context.User.HasClaim("role", "pastor") ||
            context.User.HasClaim("role", "finance_officer") ||
            context.User.HasClaim("role", "report_viewer")));
});

DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("DevCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
