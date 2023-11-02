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

// Customer
app.MapPost("/addcustomer", (Asiakas asiakas) => database.AddCustomer(asiakas.name, asiakas.email, asiakas.address, asiakas.phonenumber));
app.MapGet("/getcustomerinfo/{column}/{email}", (string column, string email) => database.GetCustomerInfo(column, email)); // http://localhost:{PORT}/getcustomerinfo/nimi/anssipeltola@hotmail.com
//

#region ReviewsMapping

app.MapGet("/getreview/{name}", (string name) => database.GetReview(name));

app.MapPost("/addreview", (AddReview review) => database.AddReview(review.ProductId, review.CustomerId, review.Review, review.NumReview));

#endregion

app.Run();
