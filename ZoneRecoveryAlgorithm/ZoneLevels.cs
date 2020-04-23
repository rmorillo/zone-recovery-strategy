using System;
using System.Collections.Generic;
using System.Text;

namespace ZoneRecoveryAlgorithm
{
    public class ZoneLevels
    {
        public MarketPosition Position { get; }
        public double EntryPrice { get; }
        public double TradeZoneSize { get; }
        public double ZoneRecoverySize { get; }
        public double TakeProfitLevel
        {
            get
            {
                if (Position == MarketPosition.Long)
                {
                    return EntryPrice + TradeZoneSize;                    
                }
                else if (Position == MarketPosition.Short)
                {                    
                    return EntryPrice - TradeZoneSize;                    
                }
                else
                {
                    return double.NaN;
                }
            }
        } 
        public double StopLossLevel
        {
            get
            {
                if (Position == MarketPosition.Long)
                {
                    return EntryPrice - ZoneRecoverySize - TradeZoneSize;
                }
                else if (Position == MarketPosition.Short)
                {
                    return EntryPrice + ZoneRecoverySize + TradeZoneSize;
                }
                else
                {
                    return double.NaN;
                }
            }
        }

        public double EntryLevel
        {
            get
            {
                return EntryPrice;
            }
        }
        public double LossRecoveryLevel
        {
            get
            {
                if (Position == MarketPosition.Long)
                {
                    return EntryPrice - ZoneRecoverySize;
                }
                else if (Position == MarketPosition.Short)
                {
                    return EntryPrice + ZoneRecoverySize;
                }
                else
                {
                    return double.NaN;
                }
            }
        }

        public ZoneLevels(MarketPosition position, double entryPrice, double tradeZoneSize, double zoneRecoverySize)
        {
            Position = position;
            EntryPrice = entryPrice;
            TradeZoneSize = tradeZoneSize;
            ZoneRecoverySize = zoneRecoverySize;            
        }

        public ZoneLevels Reverse()
        {
            if (Position == MarketPosition.Long)
            {
                return new ZoneLevels(MarketPosition.Short, LossRecoveryLevel, TradeZoneSize, ZoneRecoverySize);
            }
            else if (Position == MarketPosition.Short)
            {
                return new ZoneLevels(MarketPosition.Long, LossRecoveryLevel, TradeZoneSize, ZoneRecoverySize);
            }
            else
            {
                return null;
            }

        }
    }
}
