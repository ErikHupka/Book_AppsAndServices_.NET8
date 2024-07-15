﻿using Microsoft.Data.SqlClient; // To use SqlConnectionStringBuilder.
using Microsoft.EntityFrameworkCore; // ToQueryString, GetConnectionString
using Northwind.Models; // To use NorthwindDb.
using System.Diagnostics;
using static System.Environment;

#region Set UserName and Passwords
//WriteLine($"{GetEnvironmentVariable("MY_SQL_USD")} + {GetEnvironmentVariable("MY_SQL_PWD")}");

//string username_key = "MY_SQL_USD";
//string password_key = "MY_SQL_PWD";

//Write("Enter a server username: ");
//string? username = ReadLine();

//WriteLine();

//Write("Enter a server password: ");
//string? pswd = ReadLine();

//SetEnvironmentVariable(username_key, username);
//SetEnvironmentVariable(password_key, pswd);

//WriteLine($"{GetEnvironmentVariable("MY_SQL_USD")} + {GetEnvironmentVariable("MY_SQL_PWD")}");

#endregion

#region Querying the Northwind model

SqlConnectionStringBuilder builder = new()
{
    InitialCatalog = "Northwind",
    MultipleActiveResultSets = true,
    Encrypt = true,
    TrustServerCertificate = true,
    ConnectTimeout = 10
};

WriteLine("Connect to:");
WriteLine("  1 - SQL Server on local machine");
WriteLine("  2 - Azure SQL Database");
WriteLine("  3 - Azure SQL Edge");
WriteLine();
Write("Press a key: ");
ConsoleKey key = ReadKey().Key;
WriteLine(); WriteLine();

if (key is ConsoleKey.D1 or ConsoleKey.NumPad1)
{
    builder.DataSource = "."; // Local SQL Server
                              // @".\apps-services-book"; // Local SQL Server with an instance name
}
else if (key is ConsoleKey.D2 or ConsoleKey.NumPad2)
{
    builder.DataSource = // Azure SQL Database
      "tcp:apps-services-book-dvlpr.database.windows.net,1433";
}
else if (key is ConsoleKey.D3 or ConsoleKey.NumPad3)
{
    builder.DataSource = "tcp:127.0.0.1,1433"; // Azure SQL Edge
}
else
{
    WriteLine("No data source selected.");
    return;
}

WriteLine("Authenticate using:");
WriteLine("  1 - Windows Integrated Security");
WriteLine("  2 - SQL Login, for example, sa");
WriteLine();
Write("Press a key: ");

key = ReadKey().Key;
WriteLine(); WriteLine();
if (key is ConsoleKey.D1 or ConsoleKey.NumPad1)
{
    builder.IntegratedSecurity = true;
}

else if (key is ConsoleKey.D2 or ConsoleKey.NumPad2)
{
    Write("Enter your SQL Server user ID: ");
    string? userId = ReadLine();

    if (string.IsNullOrWhiteSpace(userId))
    {
        WriteLine("User ID cannot be empty or null.");
        return;
    }

    builder.UserID = userId;
    Write("Enter your SQL Server password: ");
    string? password = ReadLine();

    if (string.IsNullOrWhiteSpace(password))
    {
        WriteLine("Password cannot be empty or null.");
        return;
    }

    builder.Password = password;
    builder.PersistSecurityInfo = false;
}

else
{
    WriteLine("No authentication selected.");
    return;
}

DbContextOptionsBuilder<NorthwindDb> options = new();
options.UseSqlServer(builder.ConnectionString);
using (NorthwindDb db = new(options.Options))
{
    Write("Enter a unit price: ");
    string? priceText = ReadLine();
    if (!decimal.TryParse(priceText, out decimal price))
    {
        WriteLine("You must enter a valid unit price.");
        return;
    }
    // We have to use var because we are projecting into an anonymous type.
    var products = db.Products
      .Where(p => p.UnitPrice > price)
      .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice });
    WriteLine("----------------------------------------------------------");
    WriteLine("| {0,5} | {1,-35} | {2,8} |", "Id", "Name", "Price");
    WriteLine("----------------------------------------------------------");
    foreach (var p in products)
    {
        WriteLine("| {0,5} | {1,-35} | {2,8:C} |",
          p.ProductId, p.ProductName, p.UnitPrice);
    }
    WriteLine("----------------------------------------------------------");
    WriteLine(products.ToQueryString());
    WriteLine();
    WriteLine($"Provider:   {db.Database.ProviderName}");
    WriteLine($"Connection: {db.Database.GetConnectionString()}");
}

#endregion