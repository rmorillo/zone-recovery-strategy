using System;
using System.Collections.Generic;
using System.Text;

namespace ZoneRecoveryAlgorithm
{
    public enum PriceActionResult
    {
        Nothing,
        RecoveryLevelHit,
        TakeProfitLevelHit,
        StopLossLevelHit,
        MaxSlippageLevelHit
    }
}
