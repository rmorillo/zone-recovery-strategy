using System;
using Xunit;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryAlgorithm.UnitTests
{
    public class ShortSetupLossRecoveryTests
    {
        private ZoneRecovery _zoneRecovery;

        public ShortSetupLossRecoveryTests()
        {
            _zoneRecovery = new ZoneRecovery(1, 1, 0.64, 0, 0.1);
        }

        [Fact]
        public void GenesisShortPositionSidewayPriceActionReturnsNoResult()
        {
            //Arrange
            var session = _zoneRecovery.CreateSession(MarketPosition.Short, 3, 3, 3, 1);

            //Act
            (var result, _) = session.PriceAction(3, 3);  //No price change

            //Assert
            Assert.Equal(PriceActionResult.Nothing, result);
        }

        [Fact]
        public void GenesisShortPositionDownwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = _zoneRecovery.CreateSession(MarketPosition.Short, 3, 3, 3, 1);

            //Act
            (var result, _) = session.PriceAction(0, 0); //Hits take profit level

            //Assert
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) > 0);
        }

        [Fact]
        public void GenesisShortPositionUpwardPriceActionSpikeHitsZoneRecoveryTurn()
        {
            //Arrange
            var session = _zoneRecovery.CreateSession(MarketPosition.Short, 3, 3, 3, 1);

            //Act
            (var result, _) = session.PriceAction(4, 4); //First recovery turn to the upside

            //Assert
            Assert.Equal(1, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);            
        }

        [Fact]
        public void GenesisShortPositionUpwardPriceActionSpikeHitsMaximumSlippage()
        {
            //Arrange
            var session = _zoneRecovery.CreateSession(MarketPosition.Short, 3, 3, 3, 1);

            //Act            
            (var result, _) = session.PriceAction(4.5, 4.5); //Exceeded recovery turn level by 50%

            //Assert
            Assert.Equal(PriceActionResult.MaxSlippageLevelHit, result);
        }

        [Fact]
        public void FirstTurnShortPositionUpwardPriceActionHitsTakeProfit()
        {
            //Arrange
            var session = _zoneRecovery.CreateSession(MarketPosition.Short, 3, 3, 3, 1);

            session.PriceAction(4, 4);  //First recovery turn to the upside

            //Act
            (var result, _) = session.PriceAction(7, 7);  //Take profit level hit

            //Assert
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void FirstTurnLongPositionDownwarddPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = _zoneRecovery.CreateSession(MarketPosition.Short, 3, 3, 3, 1);

            session.PriceAction(4, 4); //First recovery turn to the upside

            //Act
            (var result, _) = session.PriceAction(3, 3); //Creates second recovery turn to the downside

            //Assert
            Assert.Equal(2, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);            
        }

        [Fact]
        public void SecondTurnShortPositionDownwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = _zoneRecovery.CreateSession(MarketPosition.Short, 3, 3, 3, 1);

            session.PriceAction(4, 4); //First recovery turn to the upside
            session.PriceAction(3, 3); //Second recovery turn to the downside

            //Act
            (var result, _) = session.PriceAction(0, 0); //Take profit level hit

            //Assert
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void SecondTurnShortPositionUpwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = _zoneRecovery.CreateSession(MarketPosition.Short, 3, 3, 3, 1);

            session.PriceAction(4, 4); //First recovery turn to the upside
            session.PriceAction(3, 3); //Second recovery turn to the downside

            //Act
            (var result, _) = session.PriceAction(4, 4); //Creates third recovery turn to the upside

            //Assert
            Assert.Equal(3, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }

        [Fact]
        public void ThirdTurnLongPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange            
            var session = _zoneRecovery.CreateSession(MarketPosition.Short, 3, 3, 3, 1);

            session.PriceAction(4, 4); //First recovery turn to the upside
            session.PriceAction(3, 3); //Second recovery turn to the downside
            session.PriceAction(4, 4); //Third recovery turn to the upside

            //Act
            (var result, _) = session.PriceAction(3, 3);  //Creates fourth recovery turn to the downside

            //Assert
            Assert.Equal(4, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }

        [Fact]
        public void ThirdTurnLongPositionUpwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = _zoneRecovery.CreateSession(MarketPosition.Short, 3, 3, 3, 1);

            session.PriceAction(4, 4); //First recovery turn to the upside
            session.PriceAction(3, 3); //Second recovery turn to the downside
            session.PriceAction(4, 4); //Third recovery turn to the upside

            //Act
            (var result, _) = session.PriceAction(7, 7); //Hits take profit level

            //Assert
            Assert.Equal(3, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void FourthTurnShortPositionUpwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = _zoneRecovery.CreateSession(MarketPosition.Short, 3, 3, 3, 1);

            session.PriceAction(4, 4); //First recovery turn to the upside
            session.PriceAction(3, 3); //Second recovery turn to the downside
            session.PriceAction(4, 4); //Third recovery turn to the upside
            session.PriceAction(3, 3); //Fourth recovery turn to the downside

            //Act
            (var result, _) = session.PriceAction(4, 4); //Creates fifth recovery turn to the downside

            //Assert
            Assert.Equal(5, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }

        [Fact]
        public void FourthTurnShortPositionDownwardPriceActionHitsTakeProfitLevel()
        {
            var session = _zoneRecovery.CreateSession(MarketPosition.Short, 3, 3, 3, 1);

            session.PriceAction(4, 4); //First recovery turn to the upside
            session.PriceAction(3, 3); //Second recovery turn to the downside
            session.PriceAction(4, 4); //Third recovery turn to the upside
            session.PriceAction(3, 3); //Fourth recovery turn to the downside

            //Act
            (var result, _) = session.PriceAction(0, 0); //Hits take profit level

            //Assert
            Assert.Equal(4, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void FifthTurnLongPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = _zoneRecovery.CreateSession(MarketPosition.Short, 3, 3, 3, 1);

            session.PriceAction(4, 4); //First recovery turn to the upside
            session.PriceAction(3, 3); //Second recovery turn to the downside
            session.PriceAction(4, 4); //Third recovery turn to the upside
            session.PriceAction(3, 3); //Fourth recovery turn to the downside
            session.PriceAction(4, 4); //Fifth recovery turn to the upside

            //Act
            (var result, _) = session.PriceAction(3, 3); //Creates sixth recovery turn to the downside

            //Assert
            Assert.Equal(6, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }

        [Fact]
        public void FifthTurnShortPositionDownwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = _zoneRecovery.CreateSession(MarketPosition.Short, 3, 3, 3, 1);

            session.PriceAction(4, 4); //First recovery turn to the upside
            session.PriceAction(3, 3); //Second recovery turn to the downside
            session.PriceAction(4, 4); //Third recovery turn to the upside
            session.PriceAction(3, 3); //Fourth recovery turn to the downside
            session.PriceAction(4, 4); //Fifth recovery turn to the upside

            //Act 
            (var result, _) = session.PriceAction(7, 7); //Hits take profit level

            //Assert
            Assert.Equal(5, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }
    }
}
