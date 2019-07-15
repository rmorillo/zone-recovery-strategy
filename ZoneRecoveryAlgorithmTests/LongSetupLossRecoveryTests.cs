using System;
using Xunit;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryTests
{
    public class LongSetupLossRecoveryTests
    {        
        [Fact]
        public void GenesisLongPositionSidewayPriceActionReturnsNoResult()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0, 0, 0, 3, 1);

            //Act
            (var result, _) = session.PriceAction(4, 4);

            //Assert
            Assert.Equal(PriceActionResult.Nothing, result);
        }

        [Fact]
        public void GenesisLongPositionUpwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);

            //Act
            (var result, _) = session.PriceAction(7, 7);

            //Assert
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) > 0);
        }

        [Fact]
        public void GenesisLongPositionDownwardPriceActionSpikeHitsZoneRecoveryTurn()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);

            //Act
            (var result, _) = session.PriceAction(3, 3);

            //Assert
            Assert.Equal(1, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);            
        }

        [Fact]
        public void GenesisLongPositionDownwardPriceActionSpikeHitsMaximumSlippage()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);

            //Act            
            (var result, _) = session.PriceAction(2.5, 2.5);

            //Assert
            Assert.Equal(PriceActionResult.MaxSlippageLevelHit, result);
        }

        [Fact]
        public void FirstTurnShortPositionDownwardPriceActionHitsTakeProfit()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);

            session.PriceAction(3, 3);  //First turn

            //Act
            (var result, _) = session.PriceAction(0, 0);  //Take profit level hit

            //Assert
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void FirstTurnShortPositionUpwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);

            session.PriceAction(3, 3); //First turn

            //Act
            (var result, _) = session.PriceAction(4, 4);

            //Assert
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }

        [Fact]
        public void SecondTurnLongPositionUpwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);

            session.PriceAction(3, 3); //First turn

            session.PriceAction(4, 4); //Second turn

            //Act
            (var result, _) = session.PriceAction(7, 7);

            //Assert
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void SecondTurnLongPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);

            session.PriceAction(3, 3); //First turn
            session.PriceAction(4, 4); //Second turn

            //Act
            (var result, _) = session.PriceAction(3, 3);

            //Assert
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }

        [Fact]
        public void ThirdTurnShortPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange            
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);

            session.PriceAction(3, 3); //First recovery turn to the downside
            session.PriceAction(4, 4); //Second recovery turn to the upside
            session.PriceAction(3, 3); //Third recovery turn to the downside            

            //Act
            (var result, _) = session.PriceAction(4, 4);  //Creates fourth recovery turn to the upside

            //Assert
            Assert.Equal(4, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }

        [Fact]
        public void ThirdTurnShortPositionDownwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);

            session.PriceAction(3, 3); //First recovery turn to the downside
            session.PriceAction(4, 4); //Second recovery turn to the upside
            session.PriceAction(3, 3); //Third recovery turn to the downside
            session.PriceAction(0.5, 0.5);
            //Act
            (var result, _) = session.PriceAction(0, 0); //Hits take profit level

            //Assert
            Assert.Equal(3, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void FourthTurnLongPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);

            session.PriceAction(3, 3); //First recovery turn to the downside
            session.PriceAction(4, 4); //Second recovery turn to the upside
            session.PriceAction(3, 3); //Third recovery turn to the downside
            session.PriceAction(4, 4); //Fourth recovery turn to the upside

            //Act
            (var result, _) = session.PriceAction(3, 3); //Creates fifth recovery turn to the downside

            //Assert
            Assert.Equal(5, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }

        [Fact]
        public void FourthTurnLongPositionUpwardPriceActionHitsTakeProfitLevel()
        {
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);

            session.PriceAction(3, 3); //First recovery turn to the downside
            session.PriceAction(4, 4); //Second recovery turn to the upside
            session.PriceAction(3, 3); //Third recovery turn to the downside
            session.PriceAction(4, 4); //Fourth recovery turn to the upside

            //Act
            (var result, _) = session.PriceAction(7, 7); //Hits take profit level

            //Assert
            Assert.Equal(4, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void FifthTurnShortPositionUpwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);

            session.PriceAction(3, 3); //First recovery turn to the downside
            session.PriceAction(4, 4); //Second recovery turn to the upside
            session.PriceAction(3, 3); //Third recovery turn to the downside
            session.PriceAction(4, 4); //Fourth recovery turn to the upside
            session.PriceAction(3, 3); //Fifth recovery turn to the downside

            //Act
            (var result, _) = session.PriceAction(4, 4); //Creates sixth recovery turn to the upside

            //Assert
            Assert.Equal(6, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }

        [Fact]
        public void FifthTurnShortPositionDownwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0, 0.4, 3, 1);

            session.PriceAction(3, 3); //First recovery turn to the downside
            session.PriceAction(4, 4); //Second recovery turn to the upside
            session.PriceAction(3, 3); //Third recovery turn to the downside
            session.PriceAction(4, 4); //Fourth recovery turn to the upside
            session.PriceAction(3, 3); //Fifth recovery turn to the downside

            //Act 
            (var result, _) = session.PriceAction(0, 0); //Hits take profit level

            //Assert
            Assert.Equal(5, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }
    }
}
