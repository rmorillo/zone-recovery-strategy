using System;
using Xunit;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryTests
{    
    public class AdvanceScenarioTests
    {
        [Fact]
        public void IncreaseInBidAskSpread_IncreasesLotSize()
        {
            //Arrange
            var zero_spread_session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);
            var one_spread_session = new Session(MarketPosition.Long, 4.5, 3.5, 1, 0, 0.34, 0.4, 3, 1);

            //Act
            zero_spread_session.PriceAction(3, 3);  //Creates first recovery turn to the downside
            one_spread_session.PriceAction(3.5, 2.5);  //Creates first recovery turn to the downside

            //Assert
            Assert.True(zero_spread_session.ActivePosition.LotSize < one_spread_session.ActivePosition.LotSize);
        }       

    }
}
