using System;
using System.Collections.Generic;
using System.Text;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryStrategy
{
    public static class Delegates
    {
        public delegate (bool isSuccessful, string message) MarketOrder(double lotSize, double price, sbyte position, double stopLossLevel, double takeProfitLevel);
        public delegate (bool isSuccessful, string message) LimitOrder(double lotSize, double price, sbyte position, double stopLossLevel, double takeProfitLevel);
        public delegate (bool isSuccessful, string message) TakeProfit(long marketOrderId, double exitPrice);
        public delegate (bool isSuccessful, string message) StopLoss(long marketOrderId, double exitPrice);
        public delegate (bool isSuccessful, string message) TrailStop(long marketOrderId, double exitPrice);

    }
}
