using DicomDataGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

// Local-only tool: serve on a fixed localhost port unless overridden.
builder.WebHost.UseUrls(builder.Configuration["Urls"] ?? "http://localhost:5300");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Resolve the seed-data folder (repo root /seeddata), overridable via config "SeedDataPath".
var seedRoot = ResolveSeedRoot(builder.Environment.ContentRootPath, builder.Configuration["SeedDataPath"]);

var seed = new SeedDataLoader(seedRoot);
seed.Load();
var tags = new TagCatalog();
tags.Load(seedRoot);

builder.Services.AddSingleton(seed);
builder.Services.AddSingleton(tags);
builder.Services.AddSingleton<NameProvider>();
builder.Services.AddSingleton<ModalityCatalog>();
builder.Services.AddSingleton<DicomFileBuilder>();
builder.Services.AddSingleton<PacsSender>();
builder.Services.AddSingleton<GenerationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();   // serve wwwroot/index.html at /
app.UseStaticFiles();
app.MapControllers();

app.Run();

static string ResolveSeedRoot(string contentRoot, string? configured)
{
    if (!string.IsNullOrWhiteSpace(configured) && Directory.Exists(configured))
    {
        return configured;
    }
    // Walk up from the content root looking for a "seeddata" folder (repo root).
    var dir = new DirectoryInfo(contentRoot);
    while (dir != null)
    {
        var candidate = Path.Combine(dir.FullName, "seeddata");
        if (Directory.Exists(candidate))
        {
            return candidate;
        }
        dir = dir.Parent;
    }
    return Path.Combine(contentRoot, "seeddata");
}

// Exposed for integration tests.
public partial class Program { }
