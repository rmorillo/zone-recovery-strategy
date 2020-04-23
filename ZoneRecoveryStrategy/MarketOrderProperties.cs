using System;
using System.Collections.Generic;
using System.Text;

namespace ZoneRecoveryStrategy
{
    public class MarketOrderProperties
    {
        public double MarketOrderPrice { get; set; }
        public double StopLossLevel { get; set; }
        public double TakeProfitLevel { get; set; }        

        public MarketOrderProperties()
        {
            MarketOrderPrice = 0;
            StopLossLevel = 0;
            TakeProfitLevel = 0;            
        }

        public MarketOrderProperties(double marketOrderPrice, double stopLossLevel, double takeProfitLevel)
        {
            MarketOrderPrice = marketOrderPrice;
            StopLossLevel = stopLossLevel;
            TakeProfitLevel = takeProfitLevel;        
        }

    }
}
