using System;
using Xunit;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryAlgorithm.UnitTests
{
    public class AdvanceScenarioTests
    {
        private ZoneRecovery _zoneRecovery;

        public AdvanceScenarioTests()
        {
            _zoneRecovery = new ZoneRecovery(1, 0.0001, 0.64, 0, 0);
        }

        [Fact]
        public void IncreaseInBidAskSpread_IncreasesLossRecoveryLotSize()
        {
            //Arrange
            var zero_spread_session = _zoneRecovery.CreateSession(MarketPosition.Long, 4, 4, 3, 1);
            var one_spread_session = _zoneRecovery.CreateSession(MarketPosition.Long, 4.5, 3.5, 3, 1);

            //Act
            zero_spread_session.PriceAction(3, 3);  //Creates first recovery turn to the downside
            one_spread_session.PriceAction(3.5, 2.5);  //Creates first recovery turn to the downside

            //Assert
            Assert.True(zero_spread_session.ActivePosition.LotSize < one_spread_session.ActivePosition.LotSize);
        }

        [Fact]
        public void IncreaseInZoneRecoverySize_DecreasesLossRecoveryLotSize()
        {
            
            var smallZoneSizeResult = Utility.GenerateLotSizes(5, MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0, 0.0003, 0.0001);
            var smallZoneSizeTotalLotSize = SumOfLotSizes(smallZoneSizeResult);

            var mediumZoneSizeResult = Utility.GenerateLotSizes(5, MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0, 0.0006, 0.0002);
            var mediumZoneSizeTotalLotSize = SumOfLotSizes(mediumZoneSizeResult);

            var highZoneSizeResult = Utility.GenerateLotSizes(5, MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0, 0.0009, 0.0003);
            //var highZoneSizeResult = Utility.GenerateLotSizes(10, MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0, 0.015, 0.005);
            //var highZoneSizeResult = Utility.GenerateLotSizes(10, MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0, 0.0021, 0.0007);
            var highZoneSizeTotalLotSize = SumOfLotSizes(highZoneSizeResult);
            var highZoneSizeNetTotalListSize = NetSumOfLotSizes(highZoneSizeResult);

            Assert.True(smallZoneSizeTotalLotSize > mediumZoneSizeTotalLotSize && mediumZoneSizeTotalLotSize > highZoneSizeTotalLotSize);
        }

        private double SumOfLotSizes((MarketPosition, double)[] lotSizes)
        {
            double sum = 0;
            foreach((_, var lotSize) in lotSizes)
            {
                sum += lotSize;
            }

            return sum;
        }

        private double NetSumOfLotSizes((MarketPosition, double)[] lotSizes)
        {
            var initPosition = lotSizes[0].Item1;
            double sum = 0;
            foreach ((var position, var lotSize) in lotSizes)
            {
                if (initPosition == position)
                {
                    sum += lotSize;
                }
                else
                {
                    sum -= lotSize;
                }
            }

            return sum;
        }
    }
}
