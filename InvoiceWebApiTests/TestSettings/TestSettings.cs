namespace InvoiceWebApiTests
{
    internal class TestSettings
    {
        //Connection String to connect with the SQL server
        public const string ConnectionString = "Server=tcp:invoice-sql-server.database.windows.net,1433;Initial Catalog=invoise-sql-db;Persist Security Info=False;User ID=invoice_admin;Password=Password123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
                
        public const string CurrencyApiUrl = "https://v6.exchangerate-api.com/v6";

        public const string CurrencyApiKey = "5b92b19cdf26d2cad6050ca8";

    }
}
