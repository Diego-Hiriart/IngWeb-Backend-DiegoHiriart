//Diego Hiriart

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;//To allow Cross Origin Resource Sharing
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using Microsoft.IdentityModel.Logging;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.SQS;

var frontEndOrigins = "frontEndOrigins";

var builder = WebApplication.CreateBuilder(args);

//For auth0 redirections
IdentityModelEventSource.ShowPII = true;

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: frontEndOrigins,
        policy =>
        {
            policy.WithOrigins(builder.Configuration.GetValue<string>("AllowedHosts").Split(";"))//Only the front end address can use it
            .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization)//Allow content type (to use jsons) and authorization (for login) to be in the header
            .WithMethods("POST", "GET", "PUT", "DELETE");//Allow all methods

        });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//Added options to have a login, just for this UI, it creates a login that sends a token in the header, this is replicated in the front end React
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Authorization header with the bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();//A filter
});
//Create the authentication scheme
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Token"))),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

//Set up AWS
//Get profile info adn credentials
var awsOptions = builder.Configuration.GetAWSOptions();
//Configure AWS clients to use obtainded credentials
IAmazonS3 client = awsOptions.CreateServiceClient<IAmazonS3>();
builder.Services.AddDefaultAWSOptions(awsOptions);
//Add AWS services
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddAWSService<IAmazonSQS>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//app.UseRouting();//Wasnt here before, added in case CORS needs it, might not be the case

app.UseCors(frontEndOrigins);//Use the CORS policy, after UseRouting, before UseAuthorization

app.UseAuthentication();//Authentication middleware, must be above UseAuthorization

app.UseAuthorization();

app.MapControllers();

app.Run();