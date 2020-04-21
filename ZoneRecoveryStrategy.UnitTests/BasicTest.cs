using System;
using Xunit;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryStrategy.UnitTests
{
    public class BasicTests
    {
        [Fact]
        public void GenesisPositionTakesProfit_ClosesPosition()
        {
            //Arrange
            var strategy = new Strategy();
            strategy.Initialize(1, 0.0001, 0.67, 0, 1);
            double lastMarketOrderPrice = 0;

            Delegates.MarketOrder marketOrder = delegate (double lotSize, double entryPrice, sbyte position, double stopLossLevel, double takeProfitLevel)
            {
                lastMarketOrderPrice = entryPrice;
                return (true, "Success", (entryPrice, lotSize, 0));
            };

            strategy.StartSession(MarketPosition.Long, 1.1234, 1.1234, 0.0003, 0.0001, marketOrder);            

            //Act
            var result = strategy.PriceTick(DateTime.Now.Ticks, 1.1237, 1.1237, marketOrder);

            //Assert
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.Equal(1.1234, lastMarketOrderPrice);            
        }

        [Fact]
        public void FirstRecoveryTurn_ExecutesMarketOoderSuccessfully()
        {
            //Arrange
            var strategy = new Strategy();
            strategy.Initialize(1, 0.0001, 0.67, 0, 1);
            double lastMarketOrderPrice = 0;
            MarketPosition lastMarketOrderPosition = MarketPosition.None;

            Delegates.MarketOrder marketOrder = delegate (double lotSize, double entryPrice, sbyte position, double stopLossLevel, double takeProfitLevel)
            {
                lastMarketOrderPrice = entryPrice;
                lastMarketOrderPosition = (MarketPosition)position;

                return (true, "Success", (entryPrice, lotSize, 0));
            };

            strategy.StartSession(MarketPosition.Long, 1.1234, 1.1234, 0.0003, 0.0001, marketOrder);            

            //Act
            var result = strategy.PriceTick(DateTime.Now.Ticks, 1.1233, 1.1233, marketOrder);

            //Assert
            Assert.Equal(MarketPosition.Short, lastMarketOrderPosition);
            Assert.Equal(1.1233, lastMarketOrderPrice);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }
    }
}
