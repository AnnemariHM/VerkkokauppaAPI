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

// Customer
app.MapPost("/addcustomer", (Asiakas asiakas) => database.AddCustomer(asiakas.name, asiakas.email, asiakas.address, asiakas.phonenumber));
app.MapGet("/getcustomerinfo/{column}/{email}", (string column, string email) => database.GetCustomerInfo(column, email)); // http://localhost:{PORT}/getcustomerinfo/nimi/anssipeltola@hotmail.com
//

app.MapGet("/getproductid/{name}", (string name) => database.GetProductId(name));

app.MapPost("/addproduct", (Tuote tuote) => database.AddProduct(tuote.name, tuote.productCtgory, tuote.productCtgory2, tuote.price, tuote.amount, tuote.img, tuote.description));

#region ReviewsMapping

app.MapGet("/getreview/{name}", (string name) => database.GetReview(name));

app.MapPost("/addreview", (AddReview review) => database.AddReview(review.ProductId, review.CustomerId, review.Review, review.NumReview));

#endregion

app.Run();
