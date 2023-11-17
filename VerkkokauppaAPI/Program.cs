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
    app.MapGet("/getproductinfo/{name}", (string name) => database.GetProductInfo(name));
    app.MapGet("/getproductid/{name}", (string name) => database.GetProductId(name));
    app.MapGet("/getallproductnames", () => database.GetAllProductNames());
    app.MapPut("updateproductimg/{name}/{img}", (string name, string img) => database.UpdateProductImg(name, img));
#endregion

#region CustomerMapping
// Add Customer - http://localhost:{PORT}/addcustomer - Body JSON: {"name":"Anssi Peltola","email": "anssipeltola@hotmail.com", "address": "Itsenäisyydenkatu 18", "phonenumber": "0400244925"}
app.MapPost("/addcustomer", (Asiakas asiakas) => 
{
    database.AddCustomer(asiakas.name, asiakas.email, asiakas.address, asiakas.phonenumber);
    return Results.Ok(new { message = "Customer added successfully" });
});

// Get Customer Info from wanted column by email- http://localhost:{PORT}/getcustomerinfo/nimi/anssipeltola@hotmail.com
app.MapGet("/getcustomerinfo/{column}/{email}", (string column, string email) => database.GetCustomerInfo(column, email)); 

// Update Customer Info - http://localhost:{PORT}/updatecustomer/nimi/Anssi%20Peltola/anssipeltola%40hotmail.com %20 = välilyönti %40 = @
app.MapPut("/updatecustomer/{column}/{newInfo}/{email}", (string column, string newInfo, string email) => database.UpdateCustomer(column, newInfo, email));

// Delete Customer - http://localhost:{PORT}/deletecustomer/anssipeltola%40hotmail.com
app.MapDelete("/deletecustomer/{email}", (string email) => database.DeleteCustomer(email));

// Get Customer By Email - http://localhost:{PORT}/getcustomerbyemail/anssipeltola%40hotmail.com
app.MapGet("/getcustomerbyemail/{email}", (string email) => database.GetCustomerByEmail(email));
#endregion

#region TilausriviMapping
// Add orderline - http://localhost:{PORT}/addorderline - Body JSON: {"tilaus_id": 1, "tuote_id": 1, "maara": 1}
app.MapPost("/addorderline", (Tilasrivi tilausrivi) => 
{
    database.AddOrderLine(tilausrivi.tilaus_id, tilausrivi.tuote_id, tilausrivi.maara);
    return Results.Ok();
});

// Delete orderline - http://localhost:{PORT}/deleteorderline/1
app.MapDelete("/deleteorderline/{id}", (int id) => database.DeleteOrderLine(id));

// Get orderline - http://localhost:{PORT}/getorderline/1 
app.MapGet("/getorderline/{id}", (int id) => database.GetOrderLine(id));

// UPDATE orderline - http://localhost:{PORT}/updateorderline/1/1/1/1/1
app.MapPut("/updateorderline/{id}/{tilaus_id}/{tuote_id}/{maara}/{hinta}", (int id, int tilaus_id, int tuote_id, int maara, int hinta) => database.UpdateOrderLine(id, tilaus_id, tuote_id, maara, hinta));
#endregion

#region ReviewsMapping

//Pitäiskö tehdä vielä metodi, että string ja numeric review palautuis samassa molemmat
app.MapGet("/getreview/{name}", (string name) => database.GetReview(name));
app.MapGet("/getnumericreview/{name}", (string name) => database.GetNumericReview(name));
app.MapGet("/getcustomeridfromreview/{review}", (string review) => database.GetCustomerIdFromReview(review));

app.MapPost("/addreview", (AddReview review) => database.AddReview(review.ProductId, review.CustomerId, review.Review, review.NumReview));

app.MapDelete("/deletereview/{review}", (string review) => database.DeleteReview(review));

#endregion

#region PurchasesMapping

// Add Purchase - http://localhost:{PORT}/addpurchase 
app.MapPost("/addpurchase", (Tilaukset tilaus) => database.AddPurchase(tilaus.asiakas_id, tilaus.tilauspaiva, tilaus.toimitusosoite, tilaus.tilauksen_hinta, tilaus.tilauksen_tila, tilaus.lisatiedot));

// Find Purchase by Id - http://localhost:{PORT}/findpurchaseid/id
app.MapGet("/findpurchaseid/{id}", (int id) => database.FindPurchaseId(id));

// Find Purchase by CustomerId - http://localhost:{PORT}/findpurchasecustomerid/{customer_id}
app.MapGet("/findpurchasecustomerid/{customer_id}", (int customer_id) => database.FindPurchaseCustomerId(customer_id));

// Find Purchase by date - http://localhost:{PORT}/findpurchasebydate/{date} - example 03112023
app.MapGet("/findpurchasebydate/{date}", (string date) => database.FindPurchase_bydate(date));

// Delete Purchase by Id - http://localhost:{PORT}/deletePurchaseById/{id}
app.MapDelete("/deletePurchaseById/{id}", (int id) => database.DeletePurchaseById(id));

// Print all purchases - http://localhost:{PORT}/printallpurchases
//app.MapGet("/printallpurchases", () => database.PrintAllPurchases());

#endregion

app.Run();
