// Services/DatabaseService.cs
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Backend.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<object> ExecuteScalarAsync(string query, params (string, object?)[] parameters)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = query;
            
            foreach (var (name, value) in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            return await command.ExecuteScalarAsync() ?? DBNull.Value;
        }

        public async Task<int> ExecuteNonQueryAsync(string query, params (string, object?)[] parameters)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = query;
            
            foreach (var (name, value) in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            return await command.ExecuteNonQueryAsync();
        }

        public async Task<T?> QuerySingleOrDefaultAsync<T>(string query, params (string, object?)[] parameters) where T : class, new()
        {
            var results = await ExecuteQueryAsync<T>(query, parameters);
            return results.FirstOrDefault();
        }

        public async Task<List<T>> ExecuteQueryAsync<T>(string query, params (string, object?)[] parameters) where T : class, new()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = query;
            
            foreach (var (name, value) in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            var results = new List<T>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var item = new T();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var property = typeof(T).GetProperty(reader.GetName(i));
                    if (property != null && !reader.IsDBNull(i))
                    {
                        var value = reader.GetValue(i);
                        if (value != DBNull.Value)
                        {
                            property.SetValue(item, Convert.ChangeType(value, 
                                Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType));
                        }
                    }
                }
                results.Add(item);
            }
            
            return results;
        }

        public async Task<List<T>> QueryAsync<T>(string query, params (string, object?)[] parameters) where T : class, new()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = query;
            
            foreach (var (name, value) in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            using var reader = await command.ExecuteReaderAsync();
            var results = new List<T>();
            var properties = typeof(T).GetProperties();

            while (await reader.ReadAsync())
            {
                var item = new T();
                foreach (var property in properties)
                {
                    var value = reader[property.Name];
                    if (value != DBNull.Value)
                    {
                        if (property.PropertyType == typeof(DateTime) && value is string dateStr)
                        {
                            property.SetValue(item, DateTime.Parse(dateStr));
                        }
                        else if (property.PropertyType == typeof(DateTime?) && value is string nullableDateStr)
                        {
                            property.SetValue(item, DateTime.Parse(nullableDateStr));
                        }
                        else
                        {
                            property.SetValue(item, Convert.ChangeType(value, property.PropertyType));
                        }
                    }
                }
                results.Add(item);
            }

            return results;
        }

        public async Task InitializeDatabaseAsync()
        {
            // Create Users table
            await ExecuteNonQueryAsync(@"
                CREATE TABLE IF NOT EXISTS Users (
                    Id TEXT PRIMARY KEY,
                    Email TEXT UNIQUE NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    FirstName TEXT,
                    LastName TEXT,
                    Role TEXT NOT NULL DEFAULT 'User',
                    CreatedAt TEXT NOT NULL,
                    LastLoginAt TEXT
                );");
        }
    }
}
