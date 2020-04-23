using System;
using Xunit;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryStrategy.UnitTests
{
    public class BasicTests
    {
        [Fact]
        public void GenesisPosition_TriggersMarketOrder()
        {
            //Arrange
            var strategy = new Strategy();
            strategy.Initialize(1, 0.0001, 0.67, 0, 1);
            MarketOrderProperties lastMarketOrder = null;

            Delegates.MarketOrder marketOrder = delegate (double lotSize, double entryPrice, sbyte position, double stopLossLevel, double takeProfitLevel)
            {
                lastMarketOrder = new MarketOrderProperties(entryPrice, stopLossLevel, takeProfitLevel);

                return (true, "Success");
            };

            double limitOrderLotSize = double.NaN;
            sbyte limitOrderPosition = 0;
            Delegates.LimitOrder limitOrder = delegate (double lotSize, double price, sbyte position, double stopLossLevel, double takeProfitLevel)
            {
                limitOrderLotSize = lotSize;
                limitOrderPosition = position;
                return (true, "Success");
            };            

            //Act
            strategy.StartSession(MarketPosition.Long, 1.1234, 1.1234, 0.0003, 0.0001, marketOrder, limitOrder);
            
            //Assert
            Assert.Equal(1.1234, lastMarketOrder.MarketOrderPrice);
            Assert.Equal(1.1230, lastMarketOrder.StopLossLevel);
            Assert.Equal(1.1237, lastMarketOrder.TakeProfitLevel);
            Assert.True(limitOrderLotSize > 2);
            Assert.Equal(MarketPosition.Short.GetValue(), limitOrderPosition);
        }

        [Fact]
        public void GenesisPositionTakesProfit_ClosesPosition()
        {
            //Arrange
            var strategy = new Strategy();
            strategy.Initialize(1, 0.0001, 0.67, 0, 1);
            MarketOrderProperties lastMarketOrder = null;

            Delegates.MarketOrder marketOrder = delegate (double lotSize, double entryPrice, sbyte position, double stopLossLevel, double takeProfitLevel)
            {                
                lastMarketOrder = new MarketOrderProperties(entryPrice, stopLossLevel, takeProfitLevel);
                return (true, "Success");
            };

            double limitOrderLotSize = double.NaN;
            Delegates.LimitOrder limitOrder = delegate (double lotSize, double price, sbyte position, double stopLossLevel, double takeProfitLevel)
            {
                limitOrderLotSize = lotSize;
                return (true, "Success");
            };

            strategy.StartSession(MarketPosition.Long, 1.1234, 1.1234, 0.0003, 0.0001, marketOrder, limitOrder);            

            //Act
            var result = strategy.PriceTick(DateTime.Now.Ticks, 1.1237, 1.1237);

            //Assert
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.Equal(1.1234, lastMarketOrder.MarketOrderPrice);
        }

        [Fact]
        public void FirstRecoveryTurn_ExecutesMarketOrderSuccessfully()
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

                return (true, "Success");
            };

            double limitOrderLotSize = double.NaN;
            Delegates.LimitOrder limitOrder = delegate (double lotSize, double price, sbyte position, double stopLossLevel, double takeProfitLevel)
            {
                limitOrderLotSize = lotSize;
                return (true, "Success");
            };

            strategy.StartSession(MarketPosition.Long, 1.1234, 1.1234, 0.0003, 0.0001, marketOrder, limitOrder);

            //Act
            var result = strategy.PriceTick(DateTime.Now.Ticks, 1.1233, 1.1233);

            //Assert
            Assert.Equal(MarketPosition.Short, lastMarketOrderPosition);
            Assert.Equal(1.1233, lastMarketOrderPrice);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }
    }
}
