using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeliveryApi.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly string _connectionString;

        public AdminController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        [HttpGet("database")]
        public async Task<IActionResult> GetFullDatabase()
        {
            var databaseStructure = new List<object>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // 1. Get all tables in the database
            var tables = new List<string>();
            using (var command = new MySqlCommand("SHOW TABLES;", connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }

            // 2. For every table, get its columns and rows dynamically
            foreach (var tableName in tables)
            {
                var columns = new List<string>();
                var rows = new List<Dictionary<string, object>>();

                using (var command = new MySqlCommand($"SELECT * FROM `{tableName}`;", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    // Get columns
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        columns.Add(reader.GetName(i));
                    }

                    // Get rows
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[columns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        }
                        rows.Add(row);
                    }
                }

                databaseStructure.Add(new
                {
                    TableName = tableName,
                    Columns = columns,
                    Rows = rows
                });
            }

            return Ok(databaseStructure);
        }

        public class SqlQueryRequest
        {
            public string Sql { get; set; } = string.Empty;
        }

        [HttpPost("query")]
        public async Task<IActionResult> ExecuteQuery([FromBody] SqlQueryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Sql)) return BadRequest("SQL query cannot be empty");

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new MySqlCommand(request.Sql, connection);
                
                int rowsAffected = await command.ExecuteNonQueryAsync();
                return Ok(new { success = true, rowsAffected });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }
    }
}
