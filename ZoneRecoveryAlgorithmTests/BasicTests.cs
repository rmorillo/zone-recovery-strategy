using System;
using Xunit;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryTests
{    
    public class ZoneRecoveryAlgorithmBasicTests
    {
        [Fact]
        public void ZoneLevelsAreGeneratedCorrectly()
        {
            //Arrange and act
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0, 0, 3, 1);

            //Assert
            Assert.Equal(7, session.ZoneLevels.UpperTradingZone);
            Assert.Equal(0, session.ZoneLevels.LowerTradingZone);
            Assert.Equal(4, session.ZoneLevels.UpperRecoveryZone);
            Assert.Equal(3, session.ZoneLevels.LowerRecoveryZone);
        }

        [Fact]
        public void SessionInitializesSucccessfully()
        {
            //Arrange and act
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0, 0, 3, 1);            

            //Assert
            Assert.Equal(4, session.ActivePosition.EntryPrice);
        }

    }
}
