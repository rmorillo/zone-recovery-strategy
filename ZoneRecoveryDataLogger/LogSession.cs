using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace ZoneRecoveryDataLogger
{
    public class LogSession
    {
        private SQLiteConnection _connection;

        private bool _isPriceActionTableExisting = false;

        public LogSession()
        {
            var builder = new SQLiteConnectionStringBuilder() { DataSource = "hello.db" };
            
            _connection = new SQLiteConnection(builder.ConnectionString);                            
        }

        public void Open()
        {
            _connection.Open();
        }

        public void Close()
        {
            _connection.Close();
        }

        public PriceActionLogWriter CreatePriceActionLogWriter()
        {
            CreateTableIfNotExisting(PriceActionSqlCcommands.CreateTableIfNotExisting);

            return new PriceActionLogWriter(_connection);
        }

        public PriceActionLogReader CreatePriceActionLogReader()
        {
            CreateTableIfNotExisting(PriceActionSqlCcommands.CreateTableIfNotExisting);

            return new PriceActionLogReader(_connection);
        }

        private void CreateTableIfNotExisting(string createTableSqlCommand)
        {
            if (! _isPriceActionTableExisting)
            {
                var command = _connection.CreateCommand();
                command.CommandText = createTableSqlCommand;
                command.ExecuteNonQuery();

                _isPriceActionTableExisting = true;
            }
        }
    }
}
