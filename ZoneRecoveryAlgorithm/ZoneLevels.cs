using System;
using System.Collections.Generic;
using System.Text;

namespace ZoneRecoveryAlgorithm
{
    public class ZoneLevels
    {
        public double UpperTradingZone { get; } = 0;
        public double LowerTradingZone { get; } = 0;
        public double UpperRecoveryZone { get; } = 0;
        public double LowerRecoveryZone { get; } = 0;

        public ZoneLevels(MarketPosition position, double entryPrice, double tradeZoneSize, double zoneRecoverySize)
        {                        
            if (position == MarketPosition.Long)
            {
                UpperTradingZone = entryPrice + tradeZoneSize;
                LowerTradingZone = entryPrice - zoneRecoverySize - tradeZoneSize;
                UpperRecoveryZone = entryPrice;
                LowerRecoveryZone = entryPrice - zoneRecoverySize;
            }
            else if (position == MarketPosition.Short)
            {
                UpperTradingZone = entryPrice + zoneRecoverySize + tradeZoneSize;
                LowerTradingZone = entryPrice - tradeZoneSize;
                UpperRecoveryZone = entryPrice + zoneRecoverySize; 
                LowerRecoveryZone = entryPrice;
            }
        }
    }
}
