using Npgsql;

namespace BagsOn.Data
{
    public static class DatabaseManager
    {
        // Рядок підключення до бази даних PostgreSQL
        private const string ConnectionString =
            "Host=localhost;Port=5432;Database=BagsOn;Username=postgres;Password=ggeess2006i";

        // Метод створює нове підключення до бази даних
        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(ConnectionString);
        }
    }
}