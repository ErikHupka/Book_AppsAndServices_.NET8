using Microsoft.Data.SqlClient; // To use SqlConnection and so on.
using System.Data; // To use CommandType

ConfigureConsole();

#region Set up the connection string builder
SqlConnectionStringBuilder builder = new()
{
    InitialCatalog = "Northwind",
    MultipleActiveResultSets = true,
    Encrypt = true,
    TrustServerCertificate = true,
    ConnectTimeout = 10 // Default is 30 seconds
};

WriteLine("Connect to:");
WriteLine(" 1 - Sql Server on local machine");
WriteLine(" 2 - Azure SQL Database");
WriteLine(" 3 - Azure SQL Edge");
WriteLine();
Write("Press a key: ");
ConsoleKey key = ReadKey().Key;
WriteLine(); WriteLine();

switch (key)
{
    case ConsoleKey.D1 or ConsoleKey.NumPad1:
        builder.DataSource = ".";
        break;
    case ConsoleKey.D2 or ConsoleKey.NumPad2:
        builder.DataSource =
        "tcp:apps-services-book-dvlpr.database.windows.net,1433";
        break;
    case ConsoleKey.D3 or ConsoleKey.NumPad3:
        builder.DataSource = "tcp:127.0.0.1,1443";
        break;
    default:
        WriteLine("No data source selected.");
        return;
}

WriteLine("Authenticate using: ");
WriteLine(" 1 - Windows Integrated Security");
WriteLine(" 2 - SQL Login, for example, sa");
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
        WriteLine("User ID cannot be empty or null");
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

#endregion

#region Create and open the connection

SqlConnection connection = new(builder.ConnectionString);
WriteLine(connection.ConnectionString);
WriteLine();
connection.StateChange += Connection_StateChange;
connection.InfoMessage += Connection_InfoMessage;

try
{
    WriteLine($"Opening connection. Please wait up to {builder.ConnectTimeout} seconds...");
    WriteLine();
    connection.Open();
    WriteLine($"SQL Server version: {connection.ServerVersion}");
}
catch (SqlException ex)
{
    WriteLineInColor($"SQL exception: {ex.Message}", ConsoleColor.Red);
    return;
}

#endregion

Write("Enter a unit price: ");
string? priceText = ReadLine();
if (!decimal.TryParse(priceText, out decimal price))
{
    WriteLine("You must enter a valid unit price.");
    return;
}

SqlCommand command = connection.CreateCommand();
command.CommandType = CommandType.Text;
command.CommandText = "SELECT ProductId, ProductName, UnitPrice FROM Products"
    + "WHERE UnitPrice >= @minimumPrice";
command.Parameters.AddWithValue("minimumPrice", price);

SqlDataReader r = command.ExecuteReader();
string horizontalLine = new('-', 60);
WriteLine(horizontalLine);
WriteLine("| {0,5} | {1,-35} | {2,10} |",
  arg0: "Id", arg1: "Name", arg2: "Price");
WriteLine(horizontalLine);
while (r.Read())
{
    WriteLine("| {0,5} | {1,-35} | {2,10:C} |",
      r.GetInt32("ProductId"),
      r.GetString("ProductName"),
      r.GetDecimal("UnitPrice"));
}
WriteLine(horizontalLine);
r.Close();

connection.Close();