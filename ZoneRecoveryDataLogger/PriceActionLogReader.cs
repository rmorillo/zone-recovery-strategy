using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace ZoneRecoveryDataLogger
{
    public class PriceActionLogReader
    {
        private SQLiteConnection _connection;

        public PriceActionLogReader(SQLiteConnection connection)
        {
            _connection = connection;
        }

        public IEnumerable<(long timestamp, double bid, double ask)> GetPriceAction(long fromTimestamp, long toTimestamp)
        {
            var selectCommand = _connection.CreateCommand();
            
            selectCommand.CommandText = "SELECT timestamp, bid, ask FROM PriceAction WHERE timestamp >= $fromTimestamp AND timestamp <= $toTimestamp";
            selectCommand.Parameters.AddWithValue("$fromTimestamp", fromTimestamp);
            selectCommand.Parameters.AddWithValue("$toTimestamp", toTimestamp);

            using (var reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    yield return (reader.GetInt64(0), reader.GetDouble(1), reader.GetDouble(2));
                }
            }
        }
    }
}
