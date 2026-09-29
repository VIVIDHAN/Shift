#r "nuget: MySqlConnector, 2.3.5"
using MySqlConnector;

string connStr = "Server=192.168.31.175;Database=shunmugarai_erp;User=root;Password=Root@12345;";
using var conn = new MySqlConnection(connStr);
conn.Open();
using var cmd = new MySqlCommand("SELECT PhoneNumber, PasswordHash FROM Drivers;", conn);
using var reader = cmd.ExecuteReader();
while(reader.Read()) {
    Console.WriteLine($"Phone: {reader.GetString(0)}, Hash: {(reader.IsDBNull(1) ? "NULL" : reader.GetString(1))}");
}
