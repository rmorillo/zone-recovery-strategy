using System;
using Xunit;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryTests
{
    public class AdvanceScenarioTests
    {
        [Fact]
        public void IncreaseInBidAskSpread_IncreasesLossRecoveryLotSize()
        {
            //Arrange
            var zero_spread_session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);
            var one_spread_session = new Session(MarketPosition.Long, 4.5, 3.5, 1, 0, 0.34, 0, 0.4, 3, 1);

            //Act
            zero_spread_session.PriceAction(3, 3);  //Creates first recovery turn to the downside
            one_spread_session.PriceAction(3.5, 2.5);  //Creates first recovery turn to the downside

            //Assert
            Assert.True(zero_spread_session.ActivePosition.LotSize < one_spread_session.ActivePosition.LotSize);
        }

        [Fact]
        public void IncreaseInZoneRecoverySize_DecreasesLossRecoveryLotSize()
        {
            var smallZoneSizeResult = Utility.GenerateLotSizes(5, MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);
            var smallZoneSizeTotalLotSize = SumOfLotSizes(smallZoneSizeResult);

            var mediumZoneSizeResult = Utility.GenerateLotSizes(5, MarketPosition.Long, 8, 8, 1, 0, 0.34, 0, 0.4, 6, 2);
            var mediumZoneSizeTotalLotSize = SumOfLotSizes(mediumZoneSizeResult);

            var highZoneSizeResult = Utility.GenerateLotSizes(5, MarketPosition.Long, 12, 12, 1, 0, 0.34, 0, 0.4, 9, 3);
            var highZoneSizeTotalLotSize = SumOfLotSizes(highZoneSizeResult);

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
    }
}
