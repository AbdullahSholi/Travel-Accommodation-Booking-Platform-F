var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:80");

app.MapGet("/", () => "Hello from Travel Accommodation API! - Activate CI/CD Pipeline");

app.Run();