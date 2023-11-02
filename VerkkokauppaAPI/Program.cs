using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
// JOS HERJAA NIIN RUNNAA: dotnet add package Microsoft.Data.Sqlite

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var database = new Databaselogics();

app.MapGet("/", () => "Tervetuloa verkkokauppaan!");

// Nimeä polku SAMALLA NIMELLÄ KUIN METODI jota kutsutaan
// Tee POSTEIHIN recordi Databaselogicsiin
// Muuta Databaselogicsiin metodeihin connection niin, että ei ota sitä parametrina, vaan metodin sisällä luo ja open ja close

#region ProductMapping
    app.MapPost("/addproduct", (Product product) => database.AddProduct(product.name, product.productCtgory, product.productCtgory2, product.price, product.amount, product.img, product.description));
    app.MapGet("/getproductid/{name}", (string name) => database.GetProductId(name));
    app.MapGet("/getallproductnames", () => database.GetAllProductNames());
#endregion


app.Run();
