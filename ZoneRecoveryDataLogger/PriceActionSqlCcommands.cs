using System;
using System.Collections.Generic;
using System.Text;

namespace ZoneRecoveryDataLogger
{
    public class PriceActionSqlCcommands
    {
        public const string CreateTableIfNotExisting = "CREATE TABLE IF NOT EXISTS PriceAction(timestamp INTEGER, bid DOUBLE, ask DOUBLE)";
    }
}
