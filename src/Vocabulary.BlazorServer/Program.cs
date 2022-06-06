using MediatR;
using MudBlazor;
using MudBlazor.Services;
using p1eXu5.AutoProfile;
using Quartz;
using System.Text;
using Vocabulary.Adapters;
using Vocabulary.Adapters.Persistance;
using Vocabulary.Adapters.Persistance.Models;
using Vocabulary.BlazorServer;
using Vocabulary.Descriptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<AppState>();

builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();

builder.Services.AddMediatR(typeof(CheckTermsCommand));

builder.Services.AddAutoMapper((serviceProvider, cfg) => {
    var logger = serviceProvider.GetRequiredService<ILogger<AutoProfile>>();
    var profile = new AutoProfile(typeof(Term), logger);
    cfg.AddProfile(profile.Configure());
}, Array.Empty<Type>());

builder.Services.AddAdapters(builder.Configuration);

builder.Services.AddMemoryCache();

/*
builder.Services.AddQuartz(q =>
{
    // TODO: schedule job for temp files removing
    // 1. Register Job service
    // 2. Add Job
    // 3. 

});
*/

var app = builder.Build();

LogConfiguration(app);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

try {
    app.Services.GetRequiredService<IDbContextFactory<VocabularyDbContext>>().CreateDbContext().Database.Migrate();
    app.Run();
}
catch (Exception ex) {
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Host could not run!");
}



void LogConfiguration(WebApplication app)
{
    if (app.Configuration is null) {
        // when in test
        return;
    }


    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var sb = new StringBuilder();

    foreach (string key in app.Configuration.AsEnumerable().Select(kvp => kvp.Key).OrderBy(key => key)) {
        sb.Append(key).Append(": ").Append(app.Configuration[key]).AppendLine();
    }

    logger.LogTrace("{configs}", sb.ToString());
}


public partial class Program { }