using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ZoneRecovery;

namespace ZoneRecoveryTests
{
    [TestClass]
    public class ZoneRecoveryCalculatorTests
    {
        [TestMethod]
        public void BasicTest()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0, 0, 3, 1);

            Assert.AreEqual(7, zrCalc.ZoneLevels.UpperTradingZone);
            Assert.AreEqual(0, zrCalc.ZoneLevels.LowerTradingZone);
            Assert.AreEqual(4, zrCalc.ZoneLevels.UpperRecoveryZone);
            Assert.AreEqual(3, zrCalc.ZoneLevels.LowerRecoveryZone);
        }

        [TestMethod]
        public void InitialPositionTest()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0, 0, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            Assert.AreEqual(4, initPosition.EntryPrice);
        }

        [TestMethod]
        public void InitLongPositionSidewayPriceActionWReturnsNoResult()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0, 0, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            var result = initPosition.PriceAction(4, 4);

            Assert.AreEqual(PriceActionResult.Nothing, result.Item1);
        }

        [TestMethod]
        public void InitLongPositionUpwardPriceActionHitsTakeProfitLevel()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            var result = initPosition.PriceAction(7, 7);

            Assert.AreEqual(PriceActionResult.TakeProfitLevelHit, result.Item1);
            Assert.IsTrue(Math.Round(zrCalc.UnrealizedNetProfit, 5) > 0);
        }

        [TestMethod]
        public void InitLongPositionDownwardPriceActionSpikeHitsZoneRecoveryTurn()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            var result = initPosition.PriceAction(3, 3);

            Assert.AreEqual(PriceActionResult.RecoveryLevelHit, result.Item1);
            Assert.AreEqual(1.63, Math.Round(result.Item2.LotSize,2));
        }

        [TestMethod]
        public void InitLongPositionDownwardPriceActionSpikeHitsMaximumSlippage()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            var result = initPosition.PriceAction(2.5, 2.5);

            Assert.AreEqual(PriceActionResult.MaxSlippageLevelHit, result.Item1);          
        }

        [TestMethod]
        public void FirstTurnShortPositionDownwardPriceActionHitsTakeProfit()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            var initResult = initPosition.PriceAction(3, 3);

            var firstTurnResult = initResult.Item2.PriceAction(0, 0);

            Assert.AreEqual(PriceActionResult.TakeProfitLevelHit, firstTurnResult.Item1);
            Assert.IsTrue(Math.Round(zrCalc.UnrealizedNetProfit, 5) == 0);
        }

        [TestMethod]
        public void FirstTurnShortPositionUpwardPriceActionHitsZoneRecoveryLevel()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            var initResult = initPosition.PriceAction(3, 3);

            var firstTurnResult = initResult.Item2.PriceAction(4, 4);

            Assert.AreEqual(PriceActionResult.RecoveryLevelHit, firstTurnResult.Item1);            
        }

        [TestMethod]
        public void SecondTurnLongPositionUpwardPriceActionHitsTakeProfitLevel()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            var initResult = initPosition.PriceAction(3, 3);

            var firstTurnResult = initResult.Item2.PriceAction(4, 4);

            var secondTurnResult = firstTurnResult.Item2.PriceAction(7, 7);

            Assert.AreEqual(PriceActionResult.TakeProfitLevelHit, secondTurnResult.Item1);
            Assert.IsTrue(Math.Round(zrCalc.UnrealizedNetProfit,5) == 0);
        }

        [TestMethod]
        public void SecondTurnLongPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            var initResult = initPosition.PriceAction(3, 3);

            var firstTurnResult = initResult.Item2.PriceAction(4, 4);

            var secondTurnResult = firstTurnResult.Item2.PriceAction(3, 3);

            Assert.AreEqual(PriceActionResult.RecoveryLevelHit, secondTurnResult.Item1);            
        }

        [TestMethod]
        public void ThirdTurnShortPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            var initResult = initPosition.PriceAction(3, 3);

            var firstTurnResult = initResult.Item2.PriceAction(4, 4);

            var secondTurnResult = firstTurnResult.Item2.PriceAction(3, 3);

            var thirdTurnResult = secondTurnResult.Item2.PriceAction(4, 4);

            Assert.AreEqual(PriceActionResult.RecoveryLevelHit, thirdTurnResult.Item1);            
        }

        [TestMethod]
        public void ThirdTurnShortPositionDownwardPriceActionHitsTakeProfitLevel()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            var initResult = initPosition.PriceAction(3, 3);

            var firstTurnResult = initResult.Item2.PriceAction(4, 4);

            var secondTurnResult = firstTurnResult.Item2.PriceAction(3, 3);

            var thirdTurnResult = secondTurnResult.Item2.PriceAction(0, 0);

            Assert.AreEqual(PriceActionResult.TakeProfitLevelHit, thirdTurnResult.Item1);
            Assert.IsTrue(Math.Round(zrCalc.UnrealizedNetProfit, 5) == 0);
        }

        [TestMethod]
        public void FourthTurnLongPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            var initResult = initPosition.PriceAction(3, 3);

            var firstTurnResult = initResult.Item2.PriceAction(4, 4);

            var secondTurnResult = firstTurnResult.Item2.PriceAction(3, 3);

            var thirdTurnResult = secondTurnResult.Item2.PriceAction(4, 4);

            var fourthTurnResult = thirdTurnResult.Item2.PriceAction(3, 3);

            Assert.AreEqual(PriceActionResult.RecoveryLevelHit, fourthTurnResult.Item1);            
        }

        [TestMethod]
        public void FourthTurnLongPositionUpwardPriceActionHitsTakeProfitLevel()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            var initResult = initPosition.PriceAction(3, 3);

            var firstTurnResult = initResult.Item2.PriceAction(4, 4);

            var secondTurnResult = firstTurnResult.Item2.PriceAction(3, 3);

            var thirdTurnResult = secondTurnResult.Item2.PriceAction(4, 4);

            var fourthTurnResult = thirdTurnResult.Item2.PriceAction(7, 7);

            Assert.AreEqual(PriceActionResult.TakeProfitLevelHit, fourthTurnResult.Item1);
            Assert.IsTrue(Math.Round(zrCalc.UnrealizedNetProfit, 5) == 0);
        }

        [TestMethod]
        public void FifthTurnShortPositionDownwardPriceActionHitsZoneRecoveryLevel()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            var initResult = initPosition.PriceAction(3, 3);

            var firstTurnResult = initResult.Item2.PriceAction(4, 4);

            var secondTurnResult = firstTurnResult.Item2.PriceAction(3, 3);

            var thirdTurnResult = secondTurnResult.Item2.PriceAction(4, 4);

            var fourthTurnResult = thirdTurnResult.Item2.PriceAction(3, 3);

            var fifthTurnResult = fourthTurnResult.Item2.PriceAction(4, 4);

            Assert.AreEqual(PriceActionResult.RecoveryLevelHit, fifthTurnResult.Item1);            
        }

        [TestMethod]
        public void FifthTurnShortPositionDownwardPriceActionHitsTakeProfitLevel()
        {
            var zrCalc = new ZoneRecoveryCalculator(TradePosition.Long, 4, 4, 1, 0, 0.34, 0.4, 3, 1);

            var initPosition = zrCalc.ActivePosition;

            var initResult = initPosition.PriceAction(3, 3);

            var firstTurnResult = initResult.Item2.PriceAction(4, 4);

            var secondTurnResult = firstTurnResult.Item2.PriceAction(3, 3);

            var thirdTurnResult = secondTurnResult.Item2.PriceAction(4, 4);

            var fourthTurnResult = thirdTurnResult.Item2.PriceAction(3, 3);

            var fifthTurnResult = fourthTurnResult.Item2.PriceAction(0, 0);

            Assert.AreEqual(PriceActionResult.TakeProfitLevelHit, fifthTurnResult.Item1);
            Assert.IsTrue(Math.Round(zrCalc.UnrealizedNetProfit, 5) == 0);
        }
    }
}
