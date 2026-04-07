using MemberCare.Api.Services;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<SqlConnectionFactory>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<VisitorService>();
builder.Services.AddScoped<NewConvertService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<FollowUpService>();
builder.Services.AddScoped<ReportService>();

DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
