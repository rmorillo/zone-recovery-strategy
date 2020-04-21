using MarketProbe;
using System;
using System.Threading;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryBacktester
{
    class Program
    {
        static void Main(string[] args)
        {
            double equity = 10000;
            double lotSize = 1;
            double commissionRate = 0.67;
            double tradeZone = 15;
            double spread = 0;
            double maximumTurns = 100;
            double recoveryZone = tradeZone / 3;
            var marketPosition = MarketPosition.Long;
            double profitMargin = 0;
            double pipFactor = 0.0001;
            double slippage = 1;

            var _zoneRecovery = new ZoneRecovery(lotSize, pipFactor, commissionRate, profitMargin, slippage);

            var random = new Random(unchecked((int)DateTime.Now.Ticks));

            var priceGenerator = new PriceGenerator(DateTime.Now.Millisecond, 4, 10, random.Next(800, 1300), 0.004, 0.002, spread);

            var initQuote = priceGenerator.NextQuote;

            double midPrice = (initQuote.Bid + initQuote.Ask) / 2d;

            var session = _zoneRecovery.CreateSession(marketPosition, initQuote.Bid, initQuote.Ask, tradeZone, recoveryZone);

            int ticks = 0;

            int positionCount = 1;
            while(true)
            {
                var nextQuote = priceGenerator.NextQuote;
                midPrice = (nextQuote.Bid + nextQuote.Ask) / 2d;
                //Console.WriteLine($"Bid: {nextQuote.Bid}, Ask: {nextQuote.Ask}");
                (var result, _) = session.PriceAction(nextQuote.Bid, nextQuote.Ask);
                ticks++;
                if (result==PriceActionResult.TakeProfitLevelHit || session.RecoveryTurns > maximumTurns)
                {
                    equity += (session.UnrealizedNetProfit * lotSize);
                    Console.WriteLine($"TP Hit in {session.RecoveryTurns} turns, {session.TotalLotSize} lots, {ticks} ticks, {session.UnrealizedNetProfit} returns, {equity} equity balance");
                    Thread.Sleep(500);
                    if (positionCount > 300)
                    {                        
                        return;
                    }                                        
                    session = _zoneRecovery.CreateSession(marketPosition, nextQuote.Bid, nextQuote.Ask, tradeZone, recoveryZone);
                    ticks = 0;
                    positionCount++;
                    
                }                
            }
        }
    }
}
