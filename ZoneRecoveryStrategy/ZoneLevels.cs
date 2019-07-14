using System;
using System.Collections.Generic;
using System.Text;

namespace ZoneRecoveryAlgorithm
{
    public class ZoneLevels
    {
        public double UpperTradingZone { get; }
        public double LowerTradingZone { get; }
        public double UpperRecoveryZone { get; }
        public double LowerRecoveryZone { get; }

        public ZoneLevels(double entryPrice, int tradeZoneSize, int zoneRecoverySize)
        {
            UpperTradingZone = entryPrice + tradeZoneSize;
            LowerTradingZone = entryPrice - zoneRecoverySize - tradeZoneSize;
            UpperRecoveryZone = entryPrice;
            LowerRecoveryZone = entryPrice - zoneRecoverySize;
        }
    }
}
