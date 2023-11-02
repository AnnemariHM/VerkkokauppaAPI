using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var database = new Databaselogics();

app.MapGet("/", () => "Tervetuloa verkkokauppaan!");

// JOS HERJAA NIIN RUNNAA: dotnet add package Microsoft.Data.Sqlite

// Nimeä polku SAMALLA NIMELLÄ KUIN METODI jota kutsutaan
// Tee POSTEIHIN recordi Databaselogicsiin
// Muuta Databaselogicsiin metodeihin connection niin, että ei ota sitä parametrina, vaan metodin sisällä luo ja open ja close

app.MapGet("/getproductid/{name}", (string name) => database.GetProductId(name));

app.MapPost("/addproduct", (Tuote tuote) => database.AddProduct(tuote.name, tuote.productCtgory, tuote.productCtgory2, tuote.price, tuote.amount, tuote.img, tuote.description));

app.Run();
