using System;
using System.Collections.Generic;
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
            
            Delegates.LimitOrder limitOrder = delegate (double lotSize, double price, sbyte position, double stopLossLevel, double takeProfitLevel)
            {                
                return (true, "Success");
            };            

            //Act
            strategy.StartSession(MarketPosition.Long, 1.1234, 1.1234, 0.0003, 0.0001, marketOrder, limitOrder);
            
            //Assert
            Assert.Equal(1.1234, lastMarketOrder.MarketOrderPrice);
            Assert.Equal(1.1230, lastMarketOrder.StopLossLevel);
            Assert.Equal(1.1237, lastMarketOrder.TakeProfitLevel);            
        }

        [Fact]
        public void GenesisPosition_PlacesLimitOrder()
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
            double limitOrderPrice = double.NaN;
            double limitOrderStopLossLevel = double.NaN;
            double limitOrderTakeProfitLevel = double.NaN;

            sbyte limitOrderPosition = 0;
            Delegates.LimitOrder limitOrder = delegate (double lotSize, double price, sbyte position, double stopLossLevel, double takeProfitLevel)
            {
                limitOrderLotSize = lotSize;
                limitOrderPosition = position;
                limitOrderPrice = price;
                limitOrderStopLossLevel = stopLossLevel;
                limitOrderTakeProfitLevel = takeProfitLevel;
                return (true, "Success");
            };

            //Act
            strategy.StartSession(MarketPosition.Long, 1.1234, 1.1234, 0.0003, 0.0001, marketOrder, limitOrder);

            //Assert
            Assert.True(limitOrderLotSize > 2);
            Assert.Equal(MarketPosition.Short.GetValue(), limitOrderPosition);
            Assert.Equal(1.1233, limitOrderPrice);
            Assert.Equal(1.1237, limitOrderStopLossLevel);
            Assert.Equal(1.1230, limitOrderTakeProfitLevel);
        }

        [Fact]
        public void GenesisPosition_LossRecoveryLevelHit_TriggersLimitOrder()
        {
            //Arrange
            var strategy = new Strategy();
            strategy.Initialize(1, 0.0001, 0.67, 0, 1);

            var marketOrders = new List<MarketOrderProperties>();

            int marketOrderCounter = 0;

            Delegates.MarketOrder marketOrder = delegate (double lotSize, double entryPrice, sbyte position, double stopLossLevel, double takeProfitLevel)
            {
                marketOrderCounter++;
                marketOrders.Add(new MarketOrderProperties(entryPrice, stopLossLevel, takeProfitLevel));

                return (true, "Success");
            };

            double limitOrderLotSize = double.NaN;
            double limitOrderPrice = double.NaN;
            double limitOrderStopLossLevel = double.NaN;
            double limitOrderTakeProfitLevel = double.NaN;

            sbyte limitOrderPosition = 0;
            Delegates.LimitOrder limitOrder = delegate (double lotSize, double price, sbyte position, double stopLossLevel, double takeProfitLevel)
            {
                limitOrderLotSize = lotSize;
                limitOrderPosition = position;
                limitOrderPrice = price;
                limitOrderStopLossLevel = stopLossLevel;
                limitOrderTakeProfitLevel = takeProfitLevel;
                return (true, "Success");
            };
            
            strategy.StartSession(MarketPosition.Long, 1.1234, 1.1234, 0.0003, 0.0001, marketOrder, limitOrder);

            //Act
            var result = strategy.PriceTick(DateTime.Now.Ticks, 1.1233, 1.1233);

            //Assert
            Assert.Equal(2, marketOrderCounter);
            Assert.Equal(MarketPosition.Long.GetValue(), limitOrderPosition);
            Assert.Equal(marketOrders[0].MarketOrderPrice, limitOrderPrice);
            Assert.Equal(marketOrders[0].StopLossLevel, limitOrderStopLossLevel);
            Assert.Equal(marketOrders[0].TakeProfitLevel, limitOrderTakeProfitLevel);
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
