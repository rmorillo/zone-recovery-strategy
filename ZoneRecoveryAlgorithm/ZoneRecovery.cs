using System;
using System.Collections.Generic;
using System.Text;

namespace ZoneRecoveryAlgorithm
{
    public class ZoneRecovery : IZoneRecoverySettings
    {
        public double InitLotSize { get; private set; }
        public double PipFactor { get; private set; }
        public double CommissionRate { get; private set; }
        public double ProfitMarginRate { get; private set; }
        public double Slippage { get; private set; }

        public ZoneRecovery(double initLotSize, double pipFactor, double commission, double profitMarginRate, double slippage)
        {
            InitLotSize = initLotSize;
            PipFactor = pipFactor;
            CommissionRate =commission;
            ProfitMarginRate = profitMarginRate;
            Slippage = slippage;
        }

        public Session CreateSession(MarketPosition initPosition, double entryBidPrice, double entryAskPrice, double tradeZoneSize, double zoneRecoverySize)
        {
            return new Session(initPosition, entryBidPrice, entryAskPrice, tradeZoneSize, zoneRecoverySize, this );
        }
    }
}
