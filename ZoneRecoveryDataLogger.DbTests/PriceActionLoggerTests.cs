using System;
using System.Linq;
using Xunit;

namespace ZoneRecoveryDataLogger.DbTests
{
    public class PriceActionLoggerTests
    {
        [Fact]
        public void PriceAction_AddsNewRow()
        {
            //Arrange
            var manager = new LogSession();
            manager.Open();

            var writer = manager.CreatePriceActionLogWriter();
            var reader = manager.CreatePriceActionLogReader();
            var timestamp = DateTime.Now.Ticks;

            //Act
            writer.PriceAction(timestamp, 1, 2);

            //Asssert
            var result = reader.GetPriceAction(timestamp, timestamp);
            Assert.Single(result);

            manager.Close();
        }
    }
}
