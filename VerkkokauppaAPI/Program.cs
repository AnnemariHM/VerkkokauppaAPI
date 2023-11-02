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

#region CustomerMapping
// Add Customer - http://localhost:{PORT}/addcustomer - Body JSON: {"name":"Anssi Peltola","email": "anssipeltola@hotmail.com", "address": "Itsenäisyydenkatu 18", "phonenumber": "0400244925"}
app.MapPost("/addcustomer", (Asiakas asiakas) => 
{
    database.AddCustomer(asiakas.name, asiakas.email, asiakas.address, asiakas.phonenumber);
    return Results.Ok(new { message = "Customer added successfully" });
});

// Get Customer Info - http://localhost:{PORT}/getcustomerinfo/nimi/anssipeltola@hotmail.com
app.MapGet("/getcustomerinfo/{column}/{email}", (string column, string email) => database.GetCustomerInfo(column, email)); 

// Update Customer Info - http://localhost:{PORT}/updatecustomer/nimi/Anssi%20Peltola/anssipeltola%40hotmail.com %20 = välilyönti %40 = @
app.MapPut("/updatecustomer/{column}/{newInfo}/{email}", (string column, string newInfo, string email) => database.UpdateCustomer(column, newInfo, email));

// Delete Customer - http://localhost:{PORT}/deletecustomer/anssipeltola%40hotmail.com
app.MapDelete("/deletecustomer/{email}", (string email) => database.DeleteCustomer(email));
#endregion

#region TilausriviMapping
// Add orderline - http://localhost:{PORT}/addorderline - Body JSON: {"tilaus_id": 1, "tuote_id": 1, "maara": 1, "hinta": 1}
// En saanut vielä testattua, koska Tilaukset table on tyhjä ja tilaus_id on foreign key
app.MapPost("/addorderline", (Tilasrivi tilausrivi) => 
{
    database.AddOrderLine(tilausrivi.tilaus_id, tilausrivi.tuote_id, tilausrivi.maara, tilausrivi.hinta);
    return Results.Ok();
});

// Delete orderline - http://localhost:{PORT}/deleteorderline/1 - Ei testattu!
app.MapDelete("/deleteorderline/{id}", (int id) => database.DeleteOrderLine(id));

// Get orderline - http://localhost:{PORT}/getorderline/1 - Ei testattu!
app.MapGet("/getorderline/{id}", (int id) => database.GetOrderLine(id));
#endregion

#region ReviewsMapping

app.MapGet("/getreview/{name}", (string name) => database.GetReview(name));

app.MapPost("/addreview", (AddReview review) => database.AddReview(review.ProductId, review.CustomerId, review.Review, review.NumReview));

#endregion

app.Run();
