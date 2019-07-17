using System;
using Xunit;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryAlgorithm.UnitTests
{
    public class LongSetupLossRecoveryTests
    {        
        [Fact]
        public void GenesisLongPositionSidewayPriceActionReturnsNoResult()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 1.1234, 1.1234, 1, 0.0001, 0, 0.34, 0, 0.1, 0.0003, 0.0001);

            //Act
            (var result, _) = session.PriceAction(1.1234, 1.1234);

            //Assert
            Assert.Equal(PriceActionResult.Nothing, result);
        }

        [Fact]
        public void GenesisLongPositionUpwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0.1, 0.0006, 0.0003);

            //Act
            (var result, _) = session.PriceAction(1.1240, 1.1240);

            //Assert
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) > 0);
        }

        [Fact]
        public void GenesisLongPositionDownwardPriceActionSpikeHitsZoneRecoveryTurn()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 1.12096, 1.12096, 1, 0, 0.0001, 0.67, 0, 1, 0.00009, 0.00003);

            //Act
            (var result, _) = session.PriceAction(1.12093, 1.12093);

            //Assert
            Assert.Equal(1, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);            
        }

        [Fact]
        public void GenesisLongPositionDownwardPriceActionSpikeHitsMaximumSlippage()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0.1, 0.0006, 0.0003);

            //Act            
            (var result, _) = session.PriceAction(1.1229, 1.1229);

            //Assert
            Assert.Equal(PriceActionResult.MaxSlippageLevelHit, result);
        }

        [Fact]
        public void FirstTurnShortPositionDownwardPriceActionHitsTakeProfit()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0.1, 0.0006, 0.0003);

            session.PriceAction(1.1231, 1.1231); //First turn

            //Act
            (var result, _) = session.PriceAction(1.1225, 1.1225);  //Take profit level hit

            //Assert
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void FirstTurnShortPositionUpwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0.1, 0.0006, 0.0003);

            session.PriceAction(1.1231, 1.1231); //First turn

            //Act
            (var result, _) = session.PriceAction(1.1234, 1.1234);

            //Assert
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }

        [Fact]
        public void SecondTurnLongPositionUpwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0.1, 0.0006, 0.0003);

            session.PriceAction(1.1231, 1.1231); //First turn

            session.PriceAction(1.1234, 1.1234); //Second turn

            //Act
            (var result, _) = session.PriceAction(1.124, 1.124);

            //Assert
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void SecondTurnLongPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0.1, 0.0006, 0.0003);

            session.PriceAction(1.1231, 1.1231); //First turn
            session.PriceAction(1.1234, 1.1234); //Second turn

            //Act
            (var result, _) = session.PriceAction(1.1231, 1.1231);

            //Assert
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }

        [Fact]
        public void ThirdTurnShortPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange            
            var session = new Session(MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0.1, 0.0006, 0.0003);

            session.PriceAction(1.1231, 1.1231); //First recovery turn to the downside
            session.PriceAction(1.1234, 1.1234); //Second recovery turn to the upside
            session.PriceAction(1.1231, 1.1231); //Third recovery turn to the downside            

            //Act
            (var result, _) = session.PriceAction(1.1234, 1.1234);  //Creates fourth recovery turn to the upside

            //Assert
            Assert.Equal(4, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }

        [Fact]
        public void ThirdTurnShortPositionDownwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0.1, 0.0006, 0.0003);

            session.PriceAction(1.1231, 1.1231); //First recovery turn to the downside
            session.PriceAction(1.1234, 1.1234); //Second recovery turn to the upside
            session.PriceAction(1.1231, 1.1231); //Third recovery turn to the downside
            session.PriceAction(0.5, 0.5);
            //Act
            (var result, _) = session.PriceAction(1.1225, 1.1225); //Hits take profit level

            //Assert
            Assert.Equal(3, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void FourthTurnLongPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0.1, 0.0006, 0.0003);

            session.PriceAction(1.1231, 1.1231); //First recovery turn to the downside
            session.PriceAction(1.1234, 1.1234); //Second recovery turn to the upside
            session.PriceAction(1.1231, 1.1231); //Third recovery turn to the downside
            session.PriceAction(1.1234, 1.1234); //Fourth recovery turn to the upside

            //Act
            (var result, _) = session.PriceAction(1.1231, 1.1231); //Creates fifth recovery turn to the downside

            //Assert
            Assert.Equal(5, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }

        [Fact]
        public void FourthTurnLongPositionUpwardPriceActionHitsTakeProfitLevel()
        {
            var session = new Session(MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0.1, 0.0006, 0.0003);

            session.PriceAction(1.1231, 1.1231); //First recovery turn to the downside
            session.PriceAction(1.1234, 1.1234); //Second recovery turn to the upside
            session.PriceAction(1.1231, 1.1231); //Third recovery turn to the downside
            session.PriceAction(1.1234, 1.1234); //Fourth recovery turn to the upside

            //Act
            (var result, _) = session.PriceAction(1.124, 1.124); //Hits take profit level

            //Assert
            Assert.Equal(4, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void FifthTurnShortPositionUpwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0.1, 0.0006, 0.0003);

            session.PriceAction(1.1231, 1.1231); //First recovery turn to the downside
            session.PriceAction(1.1234, 1.1234); //Second recovery turn to the upside
            session.PriceAction(1.1231, 1.1231); //Third recovery turn to the downside
            session.PriceAction(1.1234, 1.1234); //Fourth recovery turn to the upside
            session.PriceAction(1.1231, 1.1231); //Fifth recovery turn to the downside

            //Act
            (var result, _) = session.PriceAction(1.1234, 1.1234); //Creates sixth recovery turn to the upside

            //Assert
            Assert.Equal(6, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result);
        }

        [Fact]
        public void FifthTurnShortPositionDownwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 1.1234, 1.1234, 1, 0, 0.0001, 0.67, 0, 0.1, 0.0006, 0.0003);

            session.PriceAction(1.1231, 1.1231); //First recovery turn to the downside
            session.PriceAction(1.1234, 1.1234); //Second recovery turn to the upside
            session.PriceAction(1.1231, 1.1231); //Third recovery turn to the downside
            session.PriceAction(1.1234, 1.1234); //Fourth recovery turn to the upside
            session.PriceAction(1.1231, 1.1231); //Fifth recovery turn to the downside

            //Act 
            (var result, _) = session.PriceAction(1.1225, 1.1225); //Hits take profit level

            //Assert
            Assert.Equal(5, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }
    }
}
