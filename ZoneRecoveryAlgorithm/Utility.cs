using System.Collections.Generic;

namespace ZoneRecoveryAlgorithm
{
    public class Utility
    {
        public static (MarketPosition, double)[] GenerateLotSizes(double maxTurns, MarketPosition initPosition, double entryBidPrice, double entryAskPrice, double initLotSize, double spread, double pipFactor, double commission, double profitMargin, double slippage, double tradeZoneSize, double zoneRecoverySize)
        {
            var lotSizes = new List<(MarketPosition, double)>() { (initPosition, initLotSize) };

            var session = new Session(initPosition, entryBidPrice, entryAskPrice, initLotSize, spread, pipFactor, commission, profitMargin, slippage, tradeZoneSize, zoneRecoverySize);

            var position = initPosition;

            for(int index=0; index<maxTurns; index++)
            {
                double bid = double.NaN;
                double ask = double.NaN;
                  
                if (position == MarketPosition.Long)
                {
                    bid = session.ZoneLevels.LowerRecoveryZone;                                        
                }
                else if (position == MarketPosition.Short)
                {
                    bid = session.ZoneLevels.UpperRecoveryZone;                    
                }

                ask = bid;

                var (result, turn)  = session.PriceAction(bid, ask);

                if (result == PriceActionResult.RecoveryLevelHit)
                {
                    lotSizes.Add((turn.Position, turn.LotSize));
                }
                else
                {
                    throw new System.Exception("Unexpected price action result!");
                }

                position = position.Reverse();
            }

            return lotSizes.ToArray();
        }
    }
}
