using System;
using System.Linq;
using Marten;
using Npgsql;
using Shouldly;
using Xunit;

namespace MartenIdleTransaction
{
    public class User
    {
        public Guid Id { get; set; }
    }
    public class TransactionTest
    {
        // docker run -it --name marten-defect-test --rm  -p 5455:5432 -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=docker -e POSTGRES_DB=postgres  postgres
        [Fact]
        public void Test1()
        {
            var connString =
                "User ID=postgres;Password=docker;Host=localhost;Port=5455;Database=postgres;Pooling=true;";
            var store = DocumentStore.For(opts =>
            {
                opts.Connection(connString);
                opts.Schema.For<User>();
            });

            var lightweightSession = store.LightweightSession();
            lightweightSession.Query<User>().ToList();

            var sql = @"select * from pg_stat_activity where state = 'idle in transaction'";
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            var command = conn.CreateCommand();
            command.CommandText = sql;
            command.ExecuteReader().HasRows.ShouldBeFalse();
        }
    }
}