using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;

public record Tuote(int id, string name, string productCtgory, string productCtgory2, int price, int amount, string img, string description);

public record AddReview(int Id, int ProductId, int CustomerId, string Review,int NumReview);

    internal class Databaselogics
    {
        private static string _connectionString = "Data Source = verkkokauppa.db";
        public Databaselogics()
        {
            // Nää jokaiseen metodiin alkuun ja loppuun:
            // var connection = new SqliteConnection(_connectionString);
            // connection.Open();
            // Write your code here :´D
            // connection.Close();
        }

        #region CreateTables
        public void CreateTables()
        {  
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Creates a table for 'Tuotteet' ('kuva' could be BLOB-type, but we are using TEXT for now)
            var crProductTable = connection.CreateCommand();
            crProductTable.CommandText = 
            @"CREATE TABLE IF NOT EXISTS Tuotteet (
            id INTEGER PRIMARY KEY,
            nimi TEXT,
            kategoria TEXT,
            kategoria_kaksi TEXT,
            hinta INTEGER,
            kappalemaara INTEGER,
            kuva TEXT, 
            kuvaus TEXT
            )";
            crProductTable.ExecuteNonQuery();
            
            // Luodaan taulu Asiakkaat
            var crtTblAsiakkaat = connection.CreateCommand();
            crtTblAsiakkaat.CommandText = 
                @"CREATE TABLE IF NOT EXISTS Asiakkaat (
                id INTEGER PRIMARY KEY,
                nimi TEXT NOT NULL,
                email TEXT NOT NULL,
                osoite TEXT NOT NULL,
                puhelinnumero TEXT NOT NULL
                )";
            crtTblAsiakkaat.ExecuteNonQuery();
          
            // Creates table Arvostelut
            var createReviewCmd = connection.CreateCommand();
            createReviewCmd.CommandText = @"CREATE TABLE IF NOT EXISTS Arvostelut
            (id INTEGER PRIMARY KEY, tuote_id INTEGER, asiakas_id INTEGER, arvostelu TEXT, numeerinen_arvio INTEGER)";
            createReviewCmd.ExecuteNonQuery();

            // Create table Tilaukset
            var createTilaukset = connection.CreateCommand();
            createTilaukset.CommandText = 
                @"CREATE TABLE IF NOT EXISTS Tilaukset(
                id INTEGER PRIMARY KEY,
                asiakas_id INTEGER,
                tilauspaiva TEXT,
                toimitusosoite TEXT,
                tilauksen_hinta INTEGER,
                tilauksen_tila TEXT,
                lisatiedot TEXT
                )";
            createTilaukset.ExecuteNonQuery();

            // Create table Tilausrivit
            var crtTilausrivit = connection.CreateCommand();
            crtTilausrivit.CommandText = 
                @"CREATE TABLE IF NOT EXISTS Tilausrivit(
                id INTEGER PRIMARY KEY,
                tilaus_id INTEGER,
                tuote_id INTEGER,
                maara INTEGER,
                hinta INTEGER,
                FOREIGN KEY (tilaus_id) REFERENCES Tilaukset(id),
                FOREIGN KEY (tuote_id) REFERENCES Tuotteet(id)
                )";
            crtTilausrivit.ExecuteNonQuery();

            // Creates table Maksut
            var createPaymentsCmd = connection.CreateCommand();
            createPaymentsCmd.CommandText = @"CREATE TABLE IF NOT EXISTS Maksut
            (id INTEGER PRIMARY KEY, tilaus_id INTEGER, maksutapa TEXT, summa INTEGER)";
            createPaymentsCmd.ExecuteNonQuery();

            // Create a table for Logins
            var crLoginTable = connection.CreateCommand();
            crLoginTable.CommandText = 
            @"CREATE TABLE IF NOT EXISTS Kirjautumistiedot (
            id INTEGER PRIMARY KEY,
            asiakas_id INTEGER,
            salasana_hash TEXT,
            salasana_salt TEXT,
            FOREIGN KEY (asiakas_id) REFERENCES Asiakkaat(id)
            )";
            crLoginTable.ExecuteNonQuery();

            connection.Close();
        }
        #endregion

        #region Reviews
        // Inserts new product review
        public void AddReview(int productId, int customerId, string review,int numReview)
        { 
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO Arvostelut (tuote_id, asiakas_id, arvostelu, numeerinen_arvio)
            VALUES ($tuote_id, $asiakas_id, $arvostelu, $numeerinen_arvio)";
            insertCmd.Parameters.AddWithValue("$tuote_id", productId);
            insertCmd.Parameters.AddWithValue("$asiakas_id", customerId);
            insertCmd.Parameters.AddWithValue("$arvostelu", review);
            insertCmd.Parameters.AddWithValue("$numeerinen_arvio", numReview);
            insertCmd.ExecuteNonQuery();

            connection.Close();
        }

        // Gets (select) product review (text) by product name
        public List<string> GetReview(string productName)
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            List <string> returning = new List<string>();
            List<string> noReviews = new List<string>();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT arvostelu FROM Arvostelut
            JOIN Tuotteet ON Tuotteet.id = Arvostelut.tuote_id
            WHERE Tuotteet.nimi = $productName";
            selectCmd.Parameters.AddWithValue("$productName", productName);
            var result = selectCmd.ExecuteReader();

            while(result.Read())
            {
                returning.Add(result.GetString(0));
            }

            if(returning.Count==0)
            {
                noReviews.Add("");
                return noReviews;
            }

            connection.Close();
            return returning;
        }

        // Gets (select) numeric product review by product name
        public List<int> GetNumericReview(string productName) //Tästä eteenpäin ei oo vielä
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            List<int> returning = new List<int>();
            List<int> noReviews = new List<int>();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT numeerinen_arvio FROM Arvostelut
            JOIN Tuotteet ON Tuotteet.id = Arvostelut.tuote_id
            WHERE Tuotteet.nimi = $productName";
            selectCmd.Parameters.AddWithValue("$productName", productName);
            var result = selectCmd.ExecuteReader();

            while(result.Read())
            {
                returning.Add(result.GetInt32(0));
            }

            if(returning.Count==0)
            {
                noReviews.Add(404);
                return noReviews;
            }

            connection.Close();
            return returning;
        }

        //Gets customer's id searched by review
        public List<int> GetCustomerIdFromReview(string review)
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            List<int> customerId = new List<int>();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT asiakas_id FROM Arvostelut
            WHERE arvostelu = $review";
            selectCmd.Parameters.AddWithValue("$review", review);
            var result = selectCmd.ExecuteReader();

            while(result.Read())
            {
                customerId.Add(result.GetInt32(0));
            }

            connection.Close();
            return customerId;
        }

        // Deletes the review searched by string
        public void DeleteReview(SqliteConnection connection, string toBeDeleted)
        {
            var delCmd = connection.CreateCommand();
            delCmd.CommandText = @"DELETE FROM Arvostelut WHERE arvostelu = $arvostelu";
            delCmd.Parameters.AddWithValue("$arvostelu", toBeDeleted);
            delCmd.ExecuteNonQuery();
        }
        #endregion
      
        #region Customers
        // Lisää asiakas tauluun Asiakkaat
        public void AddCustomer(SqliteConnection connection, string name, string email, string address, string phonenumber)
        {
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = 
            @"INSERT INTO Asiakkaat (nimi, email, osoite, puhelinnumero)
            VALUES ($name, $email, $address, $phonenumber)";
            insertCmd.Parameters.AddWithValue("$name", name);
            insertCmd.Parameters.AddWithValue("$email", email);
            insertCmd.Parameters.AddWithValue("$address", address);
            insertCmd.Parameters.AddWithValue("$phonenumber", phonenumber);
            insertCmd.ExecuteNonQuery();
        }

        // UPDATE asiakkaan tietoja taulusta Asiakkaat. Valitse parametrillä email kenen tietoja päivitetään, colum mitä tietoja ja newInfo uusi tieto.
        public void UpdateCustomer(SqliteConnection connection, string column, string newInfo, string email)
        {
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText =
            $"UPDATE Asiakkaat SET {column} = $newInfo WHERE email = $email";
            updateCmd.Parameters.AddWithValue("$newInfo", newInfo);
            updateCmd.Parameters.AddWithValue("$email", email);
            updateCmd.ExecuteNonQuery();
        }

        // Hakee tablesta Asiakkaat columnin tiedot siltä, missä email mätsää.
        public string GetCustomerInfo(SqliteConnection connection, string column, string email)
        {
            string returnResult = "";

            var getCmd = connection.CreateCommand();
            getCmd.CommandText = $"SELECT {column} FROM Asiakkaat WHERE email = $email";
            getCmd.Parameters.AddWithValue("$email", email);
            var result = getCmd.ExecuteReader();

            if (result.Read())
            {
                returnResult = result.GetString(0);
            }

            return returnResult;
        }

        // Poistaa tablesta Asiakkaat asiakkaan tiedot emailin perusteella
        public void DeleteCustomer(SqliteConnection connection, string email)
        {
            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = @"DELETE FROM Asiakkaat WHERE email = $email";
            deleteCmd.Parameters.AddWithValue("$email", email);
            deleteCmd.ExecuteNonQuery();
        }

        // Printtaa consoleen tarjolla olevat sarakkeet haluamasta tablesta parametrillä tableName. En halunnut, että se näyttää "id" saraketta joten se skippaa ne!
        public void PrintColumnNames(SqliteConnection connection, string tableName)
        {
            Console.Write("Saatavilla olevat sarakkeet: ");
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info({tableName})";

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Get.Ordinal("name") Määrittää mitä tieto columnista haetaan. GetString(1)); tekisi saman asian!
                    string columnName = reader.GetString(reader.GetOrdinal("name"));

                    if (columnName != "id")
                    {
                        Console.Write(columnName + " ");
                    }
                }
            }
        }

        #endregion
      
        #region Products
        // Adds a product to the table
        public void AddProduct(string productName, string productCtgory, string productCtgory2, int productPrice, int productAmount, string productImg, string productDescription)
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = 
            @"INSERT INTO Tuotteet (nimi, kategoria, kategoria_kaksi, hinta, kappalemaara, kuva, kuvaus) 
            VALUES ($nimi, $kategoria, $kategoria_kaksi, $hinta, $kappalemaara, $kuva, $kuvaus)";
            insertCmd.Parameters.AddWithValue("$nimi", productName);
            insertCmd.Parameters.AddWithValue("$kategoria", productCtgory);
            insertCmd.Parameters.AddWithValue("$kategoria_kaksi", productCtgory2);
            insertCmd.Parameters.AddWithValue("$hinta", productPrice);
            insertCmd.Parameters.AddWithValue("$kappalemaara", productAmount);
            insertCmd.Parameters.AddWithValue("$kuva", productImg);
            insertCmd.Parameters.AddWithValue("$kuvaus", productDescription);
            insertCmd.ExecuteNonQuery();

            connection.Close();
        }

        // Returns named product's id
        public int GetProductId(string productName)
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            int id = 0;
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = 
            @"SELECT id
            FROM Tuotteet
            WHERE nimi = $nimi";
            selectCmd.Parameters.AddWithValue("$nimi", productName);
            var result = selectCmd.ExecuteReader();
            if (result.Read())
            {
                id = result.GetInt32(0);
            }

            connection.Close();
            return id;
        }

        // Returns a list of all the product names in database
        public List<string> GetAllProductNames(SqliteConnection connection)
        {
            List<string> names = new List<string>();
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = 
            @"SELECT nimi
            FROM Tuotteet";
            var result = selectCmd.ExecuteReader();
            while (result.Read())
            {
                names.Add(result.GetString(0));
            }
            return names;
        }

        // Returns named product's category
        public string GetProductCategory(SqliteConnection connection, string productName)
        {
            string ctgory = "";
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = 
            @"SELECT kategoria
            FROM Tuotteet
            WHERE nimi = $nimi";
            selectCmd.Parameters.AddWithValue("$nimi", productName);
            var result = selectCmd.ExecuteReader();
            if (result.Read())
            {
                ctgory = result.GetString(0);
            }
            return ctgory;
        }

        // Returns named product's second category
        public string GetProductCategory2(SqliteConnection connection, string productName)
        {
            string ctgory = "";
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = 
            @"SELECT kategoria_kaksi
            FROM Tuotteet
            WHERE nimi = $nimi";
            selectCmd.Parameters.AddWithValue("$nimi", productName);
            var result = selectCmd.ExecuteReader();
            if (result.Read())
            {
                ctgory = result.GetString(0);
            }
            return ctgory;        
        }

        // Returns named product's price
        public int GetProductPrice(SqliteConnection connection, string productName)
        {
            int price = 0;
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = 
            @"SELECT hinta
            FROM Tuotteet
            WHERE nimi = $nimi";
            selectCmd.Parameters.AddWithValue("$nimi", productName);
            var result = selectCmd.ExecuteReader();
            if (result.Read())
            {
                price = result.GetInt32(0);
            }
            return price;
        }

        // Returns named product's amount
        public int GetProductAmount(SqliteConnection connection, string productName)
        {
            int amount = 0;
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = 
            @"SELECT kappalemaara
            FROM Tuotteet
            WHERE nimi = $nimi";
            selectCmd.Parameters.AddWithValue("$nimi", productName);
            var result = selectCmd.ExecuteReader();
            if (result.Read())
            {
                amount = result.GetInt32(0);
            }
            return amount;
        }

        // Returns named product's img
        public string GetProductImg(SqliteConnection connection, string productName)
        {
            string img = "";
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = 
            @"SELECT kuva
            FROM Tuotteet
            WHERE nimi = $nimi";
            selectCmd.Parameters.AddWithValue("$nimi", productName);
            var result = selectCmd.ExecuteReader();
            if (result.Read())
            {
                img = result.GetString(0);
            }
            return img;
        }

        // Returns named product's description
        public string GetProductDescription(SqliteConnection connection, string productName)
        {
            string description = "";
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = 
            @"SELECT kuvaus
            FROM Tuotteet
            WHERE nimi = $nimi";
            selectCmd.Parameters.AddWithValue("$nimi", productName);
            var result = selectCmd.ExecuteReader();
            if (result.Read())
            {
                description = result.GetString(0);
            }
            return description;
        }

        // Updates the product's name to the database
        public void UpdateProductName(SqliteConnection connection, string productName, string newName)
        {
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = 
            @"UPDATE Tuotteet
            SET nimi = $newName
            WHERE nimi = $productName";
            updateCmd.Parameters.AddWithValue("$newName", newName);
            updateCmd.Parameters.AddWithValue("$productName", productName);
            updateCmd.ExecuteNonQuery();
        }

        // Updates the product's category to the database
        public void UpdateProductCategory(SqliteConnection connection, string productName, string newCategory)
        {
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText =
            @"UPDATE Tuotteet 
            SET kategoria = $newCategory
            WHERE nimi = $productName";
            updateCmd.Parameters.AddWithValue("$newCategory", newCategory);
            updateCmd.Parameters.AddWithValue("$productName", productName);
            updateCmd.ExecuteNonQuery();
        }

        // Updates the product's second category to the database
        public void UpdateProductCategory2(SqliteConnection connection, string productName, string newCategory)
        {
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText =
            @"UPDATE Tuotteet 
            SET kategoria_kaksi = $newCategory
            WHERE nimi = $productName";
            updateCmd.Parameters.AddWithValue("$newCategory", newCategory);
            updateCmd.Parameters.AddWithValue("$productName", productName);
            updateCmd.ExecuteNonQuery();
        }

        // Updates the product's price to the database
        public void UpdateProductPrice(SqliteConnection connection, string productName, int newPrice)
        {
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText =
            @"UPDATE Tuotteet 
            SET hinta = $newPrice
            WHERE nimi = $productName";
            updateCmd.Parameters.AddWithValue("$newPrice", newPrice);
            updateCmd.Parameters.AddWithValue("$productName", productName);
            updateCmd.ExecuteNonQuery();
        }

        // Updates the product's amount to the database
        public void UpdateProductAmount(SqliteConnection connection, string productName, int newAmount)
        {
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText =
            @"UPDATE Tuotteet 
            SET kappalemaara = $newAmount
            WHERE nimi = $productName";
            updateCmd.Parameters.AddWithValue("$newAmount", newAmount);
            updateCmd.Parameters.AddWithValue("$productName", productName);
            updateCmd.ExecuteNonQuery();
        }

        // Updates the product's img to the database
        public void UpdateProductImg(SqliteConnection connection, string productName, string newImg)
        {
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText =
            @"UPDATE Tuotteet 
            SET kuva = $newImg
            WHERE nimi = $productName";
            updateCmd.Parameters.AddWithValue("$newImg", newImg);
            updateCmd.Parameters.AddWithValue("$productName", productName);
            updateCmd.ExecuteNonQuery();
        }

        // Updates the product's description to the database
        public void UpdateProductDescription(SqliteConnection connection, string productName, string newDescription)
        {
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText =
            @"UPDATE Tuotteet 
            SET kuvaus = $newDescription
            WHERE nimi = $productName";
            updateCmd.Parameters.AddWithValue("$newDescription", newDescription);
            updateCmd.Parameters.AddWithValue("$productName", productName);
            updateCmd.ExecuteNonQuery();
        }

        // Deletes a product from the table
        public void DeleteProduct(SqliteConnection connection, string name)
        {
            var delCmd = connection.CreateCommand();
            delCmd.CommandText = 
            @"DELETE FROM Tuotteet
            WHERE nimi = $nimi";
            delCmd.Parameters.AddWithValue("$nimi", name);
            delCmd.ExecuteNonQuery();
        }

        // Prints all products to console (for testing purposes)
        public void PrintAllProducts(SqliteConnection connection)
        {
            Console.WriteLine("Tuotteet:");
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Tuotteet";
            var product = selectCmd.ExecuteReader();
            
            while (product.Read())
            {
                Console.WriteLine($"{product["id"]} {product["nimi"]} {product["kategoria"]} {product["kategoria_kaksi"]} {product["hinta"]} {product["kappalemaara"]} {product["kuva"]} {product["kuvaus"]}");
            }
        }
        #endregion

        #region Purchase
        // Lisää ostoksia TILAUKSET-tauluun
        public void AddPurchase(SqliteConnection connection, int asiakas_id, string tilauspaiva, string toimitusosoite, int tilauksen_hinta, string tilauksen_tila, string lisatiedot)
        {
            var insertCmd2 = connection.CreateCommand();
            insertCmd2.CommandText = @"INSERT INTO Tilaukset (
            asiakas_id, tilauspaiva, toimitusosoite, tilauksen_hinta, tilauksen_tila, lisatiedot) 
            VALUES ($asiakas_id, $tilauspaiva, $toimitusosoite, $tilauksen_hinta, $tilauksen_tila, $lisatiedot)";
            insertCmd2.Parameters.AddWithValue($"asiakas_id", asiakas_id);
            insertCmd2.Parameters.AddWithValue($"tilauspaiva", tilauspaiva);
            insertCmd2.Parameters.AddWithValue($"toimitusosoite", toimitusosoite);
            insertCmd2.Parameters.AddWithValue($"tilauksen_hinta", tilauksen_hinta);
            insertCmd2.Parameters.AddWithValue($"tilauksen_tila", tilauksen_tila);
            insertCmd2.Parameters.AddWithValue($"lisatiedot", lisatiedot);
            insertCmd2.ExecuteNonQuery();
        }
        // hae TILAUS tilaus_id:n perusteella (asiakasnimi, asiakasId, tilauspaiva)
        public void FindPurchaseId(SqliteConnection connection, int findOstos_id)
        {
            // hae ostos_id:n perusteella
            // tilauksen hinta tulee toisesta  ??
            var selectPurchase = connection.CreateCommand();
            selectPurchase.CommandText = @"SELECT Tilaukset.id, Tilaukset.asiakas_id, Tilaukset.tilauspaiva, Tilaukset.toimitusosoite, Tilaukset.tilauksen_hinta, Tilaukset.tilauksen_tila, Tilaukset.lisatiedot, Asiakkaat.nimi FROM Tilaukset
            LEFT JOIN Asiakkaat ON Tilaukset.asiakas_id = Asiakkaat.id
            WHERE Tilaukset.id = $findOstos_id";
            selectPurchase.Parameters.AddWithValue("$findOstos_id", findOstos_id );
            var purchases = selectPurchase.ExecuteReader();

            while(purchases.Read())
            {
                Console.WriteLine($"TilausID: {purchases["id"]} | AsiakasID: {purchases["asiakas_id"]} | Asiakasnimi: {purchases["nimi"]} | Tilauspäivä: {purchases["tilauspaiva"]} | Toimitusosoite: {purchases["toimitusosoite"]} | Tilauksen hinta €: {purchases["tilauksen_hinta"]} | Tilauksen tila: {purchases["tilauksen_tila"]} | Lisätiedot: {purchases["lisatiedot"]}");
            }
        }
        // select hae TILAUS asiakas-id perusteella
        public void FindPurchaseCustomerId(SqliteConnection connection, int findTilaus_tilaajanId)
        {
            // tilauksen hinta tulee toisesta taulusta, SE PUUTTUU ??
            var selectPurchase = connection.CreateCommand();
            selectPurchase.CommandText = @"SELECT Tilaukset.id, Tilaukset.asiakas_id, Tilaukset.tilauspaiva, Tilaukset.toimitusosoite, Tilaukset.tilauksen_hinta, Tilaukset.tilauksen_tila, Tilaukset.lisatiedot, Asiakkaat.nimi FROM Tilaukset
            LEFT JOIN Asiakkaat ON Tilaukset.asiakas_id = Asiakkaat.id
            WHERE Tilaukset.asiakas_id = $findTilaus_tilaajanId";
            selectPurchase.Parameters.AddWithValue("$findTilaus_tilaajanId", findTilaus_tilaajanId);
            var purchases = selectPurchase.ExecuteReader();

            while(purchases.Read())
            {
                Console.WriteLine($"----------------------------\nTilausID: {purchases["id"]} | AsiakasID: {purchases["asiakas_id"]} | Asiakasnimi: {purchases["nimi"]} \nTilauspäivä: {purchases["tilauspaiva"]} | Toimitusosoite: {purchases["toimitusosoite"]} | Tilauksen hinta €: {purchases["tilauksen_hinta"]} | Tilauksen tila: {purchases["tilauksen_tila"]} | Lisätiedot: {purchases["lisatiedot"]}\n----------------------------");
            }
        }
        // select hae TILAUS päivämäärän perusteella
        public void FindTilaus_paivamaara(SqliteConnection connection, string findTilaus_paivamaara)
        {
            var selectPurchaseD = connection.CreateCommand();
            selectPurchaseD.CommandText = @"SELECT Tilaukset.id, Tilaukset.asiakas_id, Tilaukset.tilauspaiva, Tilaukset.toimitusosoite, Tilaukset.tilauksen_hinta, Tilaukset.tilauksen_tila, Tilaukset.lisatiedot, Asiakkaat.nimi FROM Tilaukset
            LEFT JOIN Asiakkaat ON Tilaukset.asiakas_id = Asiakkaat.id
            WHERE Tilaukset.tilauspaiva = $findTilaus_paivamaara";
            selectPurchaseD.Parameters.AddWithValue("$findTilaus_paivamaara", findTilaus_paivamaara);
            var purchaseD = selectPurchaseD.ExecuteReader();

            while(purchaseD.Read())
            {
                Console.WriteLine($"----------------------------\nTilausID: {purchaseD["id"]} | AsiakasID: {purchaseD["asiakas_id"]} | Asiakasnimi: {purchaseD["nimi"]} \nTilauspäivä: {purchaseD["tilauspaiva"]} | Toimitusosoite: {purchaseD["toimitusosoite"]} | Tilauksen hinta €: {purchaseD["tilauksen_hinta"]} | Tilauksen tila: {purchaseD["tilauksen_tila"]} | Lisätiedot: {purchaseD["lisatiedot"]}\n----------------------------");
            }
        }

        //
        public void DeletePurchaseById (SqliteConnection connection, int delTilaus)
        {
            var deletePurchaseCmd = connection.CreateCommand();
            deletePurchaseCmd.CommandText = "DELETE FROM Tilaukset WHERE Tilaukset.id = $delTilaus";
            deletePurchaseCmd.Parameters.AddWithValue("$delTilaus", delTilaus);
            deletePurchaseCmd.ExecuteNonQuery();
        }

        public void PrintAllPurchases(SqliteConnection connection)
        {
            var printPurchases = connection.CreateCommand();
            printPurchases.CommandText = "SELECT * FROM Tilaukset LEFT JOIN Asiakkaat ON Tilaukset.asiakas_id = Asiakkaat.id";
            var purchases = printPurchases.ExecuteReader();
            
            // Tulosta tuotteet
            while (purchases.Read())
            {
                Console.WriteLine($"----------------------------\nTilausID: {purchases["id"]} | AsiakasID: {purchases["asiakas_id"]} | Asiakasnimi: {purchases["nimi"]} \nTilauspäivä: {purchases["tilauspaiva"]} | Toimitusosoite: {purchases["toimitusosoite"]} | Tilauksen hinta €: {purchases["tilauksen_hinta"]} | Tilauksen tila: {purchases["tilauksen_tila"]} | Lisätiedot: {purchases["lisatiedot"]}\n");
            }
        }

        #endregion

        #region Tilausrivit

        public void AddOrderLine(SqliteConnection connection, int tilaus_id, int tuote_id, int maara, int hinta)
        {
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = 
                @"INSERT INTO Tilausrivit (tilaus_id, tuote_id, maara, hinta)
                VALUES ($tilaus_id, $tuote_id, $maara, $hinta)";
            insertCmd.Parameters.AddWithValue("$tilaus_id", tilaus_id);
            insertCmd.Parameters.AddWithValue("$tuote_id", tuote_id);
            insertCmd.Parameters.AddWithValue("$maara", maara);
            insertCmd.Parameters.AddWithValue("$hinta", hinta);
            insertCmd.ExecuteNonQuery();
        }

        public void DeleteOrderLine(SqliteConnection connection, int deleteID)
        {
            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = @"DELETE FROM Tilausrivit WHERE id = $deleteID";
            deleteCmd.Parameters.AddWithValue("$deleteID", deleteID);
            deleteCmd.ExecuteNonQuery();
        }

        #endregion
        
        #region Logins
        // Adds log in info to the table
        public void AddLogin(SqliteConnection connection, string customerEmail, string password)
        {
            // Check if the given email exists in the table for customers
            if(DoesEmailExist(connection, customerEmail))
            {
                // Find the customer id
                int customerId = Convert.ToInt32(GetCustomerInfo(connection, "id", customerEmail));

                // Create salt by hashing current dateTime
                string hashedSalt = Convert.ToString(DateTime.Now.ToString().GetHashCode());
                // Add salt to password and hash them
                string hashedPassword = Convert.ToString((password + hashedSalt).GetHashCode());
                // Adds the log in info to table
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = 
                @"INSERT INTO Kirjautumistiedot (asiakas_id, salasana_hash, salasana_salt) 
                VALUES ($asiakas_id, $salasana_hash, $salasana_salt)";
                insertCmd.Parameters.AddWithValue("$asiakas_id", customerId);
                insertCmd.Parameters.AddWithValue("$salasana_hash", hashedPassword);
                insertCmd.Parameters.AddWithValue("$salasana_salt", hashedSalt);
                insertCmd.ExecuteNonQuery();
            }
        }

        // Checks if the given email and password match in the table
        public bool CheckPassword(SqliteConnection connection, string customerEmail, string password)
        {
            // Check if the given email exists in the table for customers
            if (DoesEmailExist(connection, customerEmail))
            {
                // Finds the salt used by email
                string salt = "";
                var selectSaltCmd = connection.CreateCommand();
                selectSaltCmd.CommandText =
                @"SELECT Kirjautumistiedot.salasana_salt
                FROM Kirjautumistiedot JOIN Asiakkaat ON Kirjautumistiedot.asiakas_id = Asiakkaat.id
                WHERE Asiakkaat.email = $customer_email";
                selectSaltCmd.Parameters.AddWithValue("$customer_email", customerEmail);
                var result = selectSaltCmd.ExecuteReader();
                if (result.Read())
                {
                    salt = result.GetString(0);
                }
                // Add the correct salt to the given password and hash them
                string inputHashed = Convert.ToString((password + salt).GetHashCode());

                // Find the correct hashed password by email
                string customerPassword = "";
                var selectPasswordCmd = connection.CreateCommand();
                selectPasswordCmd.CommandText =
                @"SELECT Kirjautumistiedot.salasana_hash
                FROM Kirjautumistiedot JOIN Asiakkaat ON Kirjautumistiedot.asiakas_id = Asiakkaat.id
                WHERE Asiakkaat.email = $customer_email";
                selectPasswordCmd.Parameters.AddWithValue("$customer_email", customerEmail);
                var result2 = selectPasswordCmd.ExecuteReader();
                if (result2.Read())
                {
                    customerPassword = result2.GetString(0);
                }

                // Compare the hashed given password to the correct hashed password
                return inputHashed == customerPassword;
            }
            else
                return false;
        }

        // Delete Login from table
        public void DeleteLogin(SqliteConnection connection, int customerId)
        {
            var delCmd = connection.CreateCommand();
            delCmd.CommandText = 
            @"DELETE FROM Kirjautumistiedot
            WHERE asiakas_id = $id";
            delCmd.Parameters.AddWithValue("$id", customerId);
            delCmd.ExecuteNonQuery();
        }

        // Prints all Logins for testing
        public void PrintAllLogins(SqliteConnection connection)
        {
            Console.WriteLine("Loginit:");
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Kirjautumistiedot";
            var login = selectCmd.ExecuteReader();
            
            while (login.Read())
            {
                Console.WriteLine($"{login["id"]} {login["asiakas_id"]} {login["salasana_hash"]} {login["salasana_salt"]}");
            }
        }

        #endregion

        #region UsefulFunctions

        public bool DoesEmailExist(SqliteConnection connection, string email)
        {
            var cmd = connection.CreateCommand();
            // Laskee monta emailia mätsää parametrin emailiin Tablesta 'Asiakkaat'
            cmd.CommandText = "SELECT COUNT(*) FROM Asiakkaat WHERE email = $email";
            cmd.Parameters.AddWithValue("$email", email);

            // ExecuteScalar palauttaa yksittäisen arvon kyselystä (määrän)

            // ExecuteScalar palauttaa kyselyn yksittäisen arvon (määrän) objektina
            // (long) varmistaa, että käsittelemme tulosta jonkinlaisena lukuna, normaalisti ExecuteScaler() palauttaa objektin
            var count = (long)cmd.ExecuteScalar();

            // Tarkista sähköpostin olemassaolon tarkistaminen, onko määrä suurempi kuin 0
            return count > 0;
        }

        // Palauttaa listan tablen columneista, jonka parametri annetaan tableName:ille
        public List<string> GetColumnNames(SqliteConnection connection, string tableName)
        {
            List <string> columns = new List<string>();

            var getCmd = connection.CreateCommand();
            getCmd.CommandText = $"PRAGMA table_info({tableName})";
            var result = getCmd.ExecuteReader();

            while (result.Read())
            {
                
                columns.Add(result.GetString(1)); // 1 koska, muuten se tulostaa vaan 1,2,3,4. Indexistä 1 tulostaa columnin nimen.
            }
            return columns;
        }

        #endregion

        #region Maksut
        public void AddPayment(SqliteConnection connection, int orderId, string paymentMethod, int sum)
        {
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO Maksut (tilaus_id, maksutapa, summa)
            VALUES ($tilaus_id, $maksutapa, $summa)";
            insertCmd.Parameters.AddWithValue("$tilaus_id", orderId);
            insertCmd.Parameters.AddWithValue("$maksutapa", paymentMethod);
            insertCmd.Parameters.AddWithValue("$summa", sum);
            insertCmd.ExecuteNonQuery();
        }

        //For testing, get info
        /*public void TestPayment(SqliteConnection connection, int orderId)
        {
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT maksutapa, summa FROM Maksut
            WHERE tilaus_id = $orderId";
            selectCmd.Parameters.AddWithValue("$orderId", orderId);
            var payments = selectCmd.ExecuteReader();

            while(payments.Read())
            {
                Console.WriteLine($"Maksutapa: {payments["maksutapa"]}, summa: {payments["summa"]}");
            }
        }*/

        #endregion

    }
