using System;
using System.Collections.Generic;
using System.Text;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryCTraderCAlgo
{
    public class CAlgoPositionInfo
    {
        public double LotSize { get; }

        public double EntryPrice { get; }

        public MarketPosition Position { get; }

        public double Spread { get;  }
    }
}
