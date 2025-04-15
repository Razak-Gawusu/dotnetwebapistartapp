using Innout.DependencyInjection;
using Innout.RequestPipeline;

var builder = WebApplication.CreateBuilder(args);
{
    // Configure services DI.
    builder.Services
        .AddGlobalErrorHandling()
        .AddServices()
        .AddJwtAuthentication(builder.Configuration)
        .AddPersistence(builder.Configuration)
        .AddRedis(builder.Configuration)
        .AddSwagger()
        .AddControllers();
}

var app = builder.Build();
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseHttpsRedirection();
    app.UseGlobalErrorHandling();
    app.MapControllers();
    app.InitializeDatabase();
    app.Run();
}
