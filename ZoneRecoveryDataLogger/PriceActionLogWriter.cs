using System;
using System.Data.SQLite;

namespace ZoneRecoveryDataLogger
{
    public class PriceActionLogWriter
    {
        private SQLiteConnection _connection;

        public PriceActionLogWriter(SQLiteConnection connection)
        {
            _connection = connection;                     
        }

        public void PriceAction(long timestamp, double bid, double ask)
        {
            using (var transaction = _connection.BeginTransaction())
            {
                var insertCommand = _connection.CreateCommand();
                insertCommand.Transaction = transaction;
                insertCommand.CommandText = "INSERT INTO PriceAction(timestamp, bid, ask) VALUES ($timestamp, $bid, $ask)";
                insertCommand.Parameters.AddWithValue("$timestamp", timestamp);
                insertCommand.Parameters.AddWithValue("$bid", bid);
                insertCommand.Parameters.AddWithValue("$ask", ask);
                insertCommand.ExecuteNonQuery();                

                transaction.Commit();
            }
        }
    }
}
