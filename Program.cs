using AWS.PM.Entity;
using AWS.PM.Interfaces;
using AWS.PM.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NewTicketApi.Models;
using NewTicketApi.Utils;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle



builder.Services.AddHttpClient();
builder.Services.AddSingleton<Cachelper>();
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("AppOptions"));
//对称加密
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            //Audience,Issuer,clientSecret的值要和sso的一致

            //JWT有一些默认的属性，就是给鉴权时就可以筛选了
            ValidateIssuer = true,//是否验证Issuer
            ValidateAudience = true,//是否验证Audience
            ValidateLifetime = true,//是否验证失效时间
            ValidateIssuerSigningKey = true,//是否验证client secret
            ValidIssuer = builder.Configuration["SSOSetting:issuer"],//
            ValidAudience = builder.Configuration["SSOSetting:audience"],//Issuer，这两项和前面签发jwt的设置一致
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["SSOSetting:clientSecret"]))//client secret
        };
    });

#region 非对称加密-鉴权
//var rsa = RSA.Create();
//byte[] publickey = Convert.FromBase64String(AppSetting.PublicKey); //公钥，去掉begin...  end ...
////rsa.ImportPkcs8PublicKey 是一个扩展方法，来源于RSAExtensions包
//rsa.ImportPkcs8PublicKey(publickey);
//var key = new RsaSecurityKey(rsa);
//var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaPKCS1);//私钥才有pkcs1和pkcs8之分，公钥没有

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            //Audience,Issuer,clientSecret的值要和sso的一致

//            //JWT有一些默认的属性，就是给鉴权时就可以筛选了
//            ValidateIssuer = true,//是否验证Issuer
//            ValidateAudience = true,//是否验证Audience
//            ValidateLifetime = true,//是否验证失效时间
//            ValidateIssuerSigningKey = true,//是否验证client secret
//            ValidIssuer = builder.Configuration["SSOSetting:issuer"],//
//            ValidAudience = builder.Configuration["SSOSetting:audience"],//Issuer，这两项和前面签发jwt的设置一致
//            IssuerSigningKey = signingCredentials.Key
//        };
//    });

#endregion

builder.Services.AddTransient<ITicketService, TicketService>();
builder.Services.AddTransient<TicketEntity>();

builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddTransient<FileEntity>();

//builder.Services.AddSqlServer<MainDbContext>(builder.Configuration.GetConnectionString("MainDbContext"));

//builder.Services.AddDbContext<MainDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("MainDbContext")));

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
builder.Services.AddNpgsql<MainDbContext>(builder.Configuration.GetConnectionString("MainDbContext"));

builder.Services.AddTransient<DbContext, MainDbContext>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
ServiceLocator.Instance = app.Services;// ServiceLocator.Instance = app.Services; //用于手动获取DI对象
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();//need
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.MapGet("/", () => "Hello World!");

app.MapGet("/ticket", [MyAuthorize] ([FromServices] ITicketService ticketService) => ticketService.Query<TicketEntity>(o => !string.IsNullOrWhiteSpace(o.Title)).OrderByDescending(o => o.CreatedOn).ToList()).RequireAuthorization().WithName("list").WithTags("ticket");

app.MapGet("/ticket/{Id:int}", [MyAuthorize] ([FromServices] ITicketService ticketService, int Id) => ticketService.Find<TicketEntity>(Id)).WithName("get").WithTags("ticket");

app.MapPut("/ticket", [MyAuthorize] ([FromServices] ITicketService ticketService, [FromBody] TicketEntity ticket) => ticketService.Update<TicketEntity>(ticket)).WithName("put").WithTags("ticket");

app.MapPost("/ticket", [MyAuthorize] ([FromServices] ITicketService ticketService, [FromBody] TicketEntity ticket) => ticketService.Insert<TicketEntity>(ticket)).WithName("post").WithTags("ticket");

app.MapDelete("/ticket/{Id:int}", [MyAuthorize] ([FromServices] ITicketService ticketService, int Id) => ticketService.Delete<TicketEntity>(Id)).WithName("delete").WithTags("ticket");



//app.MapGet("/file/{TickeId:int}", [MyAuthorize] ([FromServices] IFileService fileService, int TickeId) => fileService.Query<FileEntity>(o => o.IsDelete == false && o.TicketID == TickeId).OrderBy(o => o.CreatedOn).ToList()).WithName("getfile").WithTags("file");

//app.MapPost("/file", [MyAuthorize] ([FromServices] IFileService fileService, [FromBody] FileEntity file) => fileService.Insert<FileEntity>(file)).WithName("postfile").WithTags("file");

app.Run();
