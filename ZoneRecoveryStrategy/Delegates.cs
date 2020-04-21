using System;
using System.Collections.Generic;
using System.Text;

namespace ZoneRecoveryStrategy
{
    public static class Delegates
    {
        public delegate (bool isSuccessful, string message, (double orderLotSize, double orderPrice, double slippage)) MarketOrder(double lotSize, double price, sbyte position, double stopLossLevel, double takeProfitLevel);
    }
}
