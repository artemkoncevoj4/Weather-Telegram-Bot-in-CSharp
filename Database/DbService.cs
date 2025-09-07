using Microsoft.Data.Sqlite;
namespace Database
{
    public class WeatherDB
    {
        internal SqliteConnection _connection;
        public WeatherDB(string connectionString = "Data Source=user_weather.db")
        {
            _connection = new SqliteConnection(connectionString);
        }
        public async Task InitializeAsync()
        {
            await ConnectAsync();
            await CreateTableAsync();
        }
        private async Task ConnectAsync()
        {
            try
            {
                await _connection.OpenAsync();
                Console.WriteLine("Database connected successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection error: {ex}");
                throw;
            }
        }
        private async Task CreateTableAsync()
        {
            try
            {
                string CreateSql = @"CREATE TABLE IF NOT EXISTS Users (
                    LocalID INTEGER PRIMARY KEY AUTOINCREMENT,
                    FIRSTNAME TEXT,
                    LASTNAME TEXT,
                    NICKNAME TEXT,
                    CITY TEXT
                )";
                using var command = new SqliteCommand(CreateSql, _connection);
                await command.ExecuteNonQueryAsync();

                Console.WriteLine("Table created or already exists");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Table creation error: {ex}");
                throw;
            }
        }
        public async Task AddOrUpdateUser(string FirstName, string LastName, string NickName, string City)
        {
            await ConnectAsync();

            string first_name = FirstName ?? "";
            string last_name = LastName ?? "";
            string nickname = NickName ?? "";
            string city = City;


            string updateSQL = $@"
            UPDATE Users
            SET CITY = '{City}'
            WHERE FIRSTNAME = '{FirstName}'
            AND LASTNAME = '{LastName}'
            AND NICKNAME = '{NickName}'";

            using var updateCommand = new SqliteCommand(updateSQL, _connection);
            int updateRows = await updateCommand.ExecuteNonQueryAsync();

            if (updateRows == 0)
            {
                string insertSQL = $@"
                INSERT INTO Users (FIRSTNAME, LASTNAME, NICKNAME, CITY)
                VALUES ('{first_name}', '{last_name}', '{nickname}', '{city}')";

                using var insertCommand = new SqliteCommand(insertSQL, _connection);
                await insertCommand.ExecuteNonQueryAsync();
            }
        }
        public async Task<string> GetCity(string firstName, string lastName, string nickName)
        {
            await ConnectAsync();

            await using var command = _connection.CreateCommand();
            command.CommandText = $@"
            SELECT CITY
            FROM Users
            WHERE FIRSTNAME = '{firstName}' 
                AND LASTNAME = '{lastName}' 
                AND NICKNAME = '{nickName}'";

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader.IsDBNull(0) ? null : reader.GetString(0);
            }
            return null;
        }
    }
}