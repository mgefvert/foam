using System;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;

namespace Foam.API.Memory
{
    public class MySqlMemory : IMemory
    {
        private MySqlConnection _connection;

        private const string ProviderSchema = @"
            create table provider_storage
            (
                facility varchar(50) not null,
                name varchar(50) not null,
                value text not null,
                updated datetime not null,
                primary key (facility, name),
                key (updated)
            );
        ";

        public MySqlMemory(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
            _connection.Open();

            var tables = _connection.Query<string>("show tables").ToList();
            if (!tables.Contains("provider_storage"))
                _connection.Execute(ProviderSchema);
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _connection = null;
        }

        public void Delete(string facility, string name)
        {
            _connection.Execute("delete from provider_storage where facility=@facility and name=@name",
                new { facility, name });
        }

        public bool Exists(string facility, string name)
        {
            return _connection.Query<int>("select count(*) from provider_storage where facility=@facility and name=@name",
                new { facility, name }).Single() > 0;
        }

        public string Get(string facility, string name)
        {
            return _connection.Query<string>("select value from provider_storage where facility=@facility and name=@name",
                new { facility, name }).FirstOrDefault();
        }

        public void Purge(string facility, int ageSeconds)
        {
            var date = DateTime.UtcNow.AddSeconds(-ageSeconds);
            _connection.Execute("delete from provider_storage where facility=@facility and updated<@date",
                new { facility, date });
        }

        public void Set(string facility, string name, string value)
        {
            _connection.Execute("replace into provider_storage (facility, name, value, updated) values (@facility, @name, @value, @updated)",
                new { facility, name, value, updated = DateTime.UtcNow });
        }
    }
}
