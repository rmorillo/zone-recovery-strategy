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

        [Fact]
        public void InitLongPositionSidewayPriceActionReturnsNoResult()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0, 0, 3, 1);            

            //Act
            var result = session.PriceAction(4, 4);

            //Assert
            Assert.Equal(PriceActionResult.Nothing, result.Item1);
        }

        [Fact]
        public void InitLongPositionUpwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            //Act
            var result = session.PriceAction(7, 7);            

            //Assert
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result.Item1);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) > 0);
        }

        [Fact]
        public void InitLongPositionDownwardPriceActionSpikeHitsZoneRecoveryTurn()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);
            
            //Act
            var result = session.PriceAction(3, 3);

            //Assert
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result.Item1);
            Assert.Equal(1.63, Math.Round(result.Item2.LotSize,2));
        }

        [Fact]
        public void InitLongPositionDownwardPriceActionSpikeHitsMaximumSlippage()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            //Act            
            var result = session.PriceAction(2.5, 2.5);

            //Assert
            Assert.Equal(PriceActionResult.MaxSlippageLevelHit, result.Item1);          
        }

        [Fact]
        public void FirstTurnShortPositionDownwardPriceActionHitsTakeProfit()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);            

            session.PriceAction(3, 3);  //First turn

            //Act
            var result = session.PriceAction(0, 0);  //Take profit level hit

            //Assert
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result.Item1);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void FirstTurnShortPositionUpwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);            

            session.PriceAction(3, 3); //First turn

            //Act
            var result = session.PriceAction(4, 4);

            //Assert
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result.Item1);            
        }

        [Fact]
        public void SecondTurnLongPositionUpwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);
            
            session.PriceAction(3, 3); //First turn

            session.PriceAction(4, 4); //Second turn

            //Act
            var result = session.PriceAction(7, 7);

            //Assert
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result.Item1);
            Assert.True(Math.Round(session.UnrealizedNetProfit,5) == 0);
        }

        [Fact]
        public void SecondTurnLongPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);
            
            session.PriceAction(3, 3); //First turn
            session.PriceAction(4, 4); //Second turn

            //Act
            var result = session.PriceAction(3, 3);

            //Assert
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result.Item1);            
        }

        [Fact]
        public void ThirdTurnShortPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange            
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);
            
            session.PriceAction(3, 3); //First recovery turn to the downside
            session.PriceAction(4, 4); //Second recovery turn to the upside
            session.PriceAction(3, 3); //Third recovery turn to the downside

            //Act
            var result = session.PriceAction(4, 4);  //Creates fourth recovery turn to the upside

            //Assert
            Assert.Equal(4, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result.Item1);            
        }

        [Fact]
        public void ThirdTurnShortPositionDownwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            session.PriceAction(3, 3); //First recovery turn to the downside
            session.PriceAction(4, 4); //Second recovery turn to the upside
            session.PriceAction(3, 3); //Third recovery turn to the downside

            //Act
            var result = session.PriceAction(0, 0); //Hits take profit level

            //Assert
            Assert.Equal(3, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result.Item1);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void FourthTurnLongPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            session.PriceAction(3, 3); //First recovery turn to the downside
            session.PriceAction(4, 4); //Second recovery turn to the upside
            session.PriceAction(3, 3); //Third recovery turn to the downside
            session.PriceAction(4, 4); //Fourth recovery turn to the upside

            //Act
            var result = session.PriceAction(3, 3); //Creates fifth recovery turn to the downside

            //Assert
            Assert.Equal(5, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result.Item1);            
        }

        [Fact]
        public void FourthTurnLongPositionUpwardPriceActionHitsTakeProfitLevel()
        {
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            session.PriceAction(3, 3); //First recovery turn to the downside
            session.PriceAction(4, 4); //Second recovery turn to the upside
            session.PriceAction(3, 3); //Third recovery turn to the downside
            session.PriceAction(4, 4); //Fourth recovery turn to the upside

            //Act
            var result = session.PriceAction(7, 7); //Hits take profit level

            //Assert
            Assert.Equal(4, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result.Item1);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }

        [Fact]
        public void FifthTurnShortPositionUpwardPriceActionHitsZoneRecoveryLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            session.PriceAction(3, 3); //First recovery turn to the downside
            session.PriceAction(4, 4); //Second recovery turn to the upside
            session.PriceAction(3, 3); //Third recovery turn to the downside
            session.PriceAction(4, 4); //Fourth recovery turn to the upside
            session.PriceAction(3, 3); //Fifth recovery turn to the downside

            //Act
            var result = session.PriceAction(4, 4); //Creates sixth recovery turn to the upside

            //Assert
            Assert.Equal(6, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.RecoveryLevelHit, result.Item1);            
        }

        [Fact]
        public void FifthTurnShortPositionDownwardPriceActionHitsTakeProfitLevel()
        {
            //Arrange
            var session = new Session(MarketPosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            session.PriceAction(3, 3); //First recovery turn to the downside
            session.PriceAction(4, 4); //Second recovery turn to the upside
            session.PriceAction(3, 3); //Third recovery turn to the downside
            session.PriceAction(4, 4); //Fourth recovery turn to the upside
            session.PriceAction(3, 3); //Fifth recovery turn to the downside

            //Act 
            var result = session.PriceAction(0, 0); //Hits take profit level

            //Assert
            Assert.Equal(5, session.RecoveryTurns);
            Assert.Equal(PriceActionResult.TakeProfitLevelHit, result.Item1);
            Assert.True(Math.Round(session.UnrealizedNetProfit, 5) == 0);
        }
    }
}
