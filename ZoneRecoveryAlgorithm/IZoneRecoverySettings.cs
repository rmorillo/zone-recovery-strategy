using System;
using System.Collections.Generic;
using System.Text;

namespace ZoneRecoveryAlgorithm
{
    public interface IZoneRecoverySettings
    {
        double InitLotSize { get; }
        double PipFactor { get; }
        double CommissionRate { get; }
        double ProfitMarginRate { get; }
        double Slippage { get; }
    }
}
