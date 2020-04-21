using System;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryCTraderCAlgo
{
    public class ZoneRecoveryAdapter
    {
        private ZoneRecovery _zoneRecovery;
        private Session _session;

        public void OnStart(double initLotSize, double pipFactor, double commissionRate, double profitMarginRate, double slippage)
        {
            _zoneRecovery = new ZoneRecovery(initLotSize, pipFactor, commissionRate, profitMarginRate, slippage);
        }

        public void StartSession(MarketPosition position, double entryBidPrice, double entryAskPrice, double tradeZoneSize, double zoneRecoverySize)
        {
            _session = _zoneRecovery.CreateSession(position, entryBidPrice, entryAskPrice, tradeZoneSize, zoneRecoverySize);
        }

        public PriceActionResult OnTick(long timestamp, double bid, double ask, Func<double, MarketPosition, (bool, string, CAlgoPositionInfo)> recoveryTurnMarketOrder)
        {
            if (_session == null)
            {
                return PriceActionResult.Nothing;
            }
            else
            {
                var (result, recoveryTurn) = _session.PriceAction(bid, ask);

                if (result == PriceActionResult.RecoveryLevelHit)
                {
                    var (isSuccessful, message, cAlgoPosition)  = recoveryTurnMarketOrder(recoveryTurn.LotSize, recoveryTurn.Position);
                    
                    if (isSuccessful)
                    {
                        var (lotSizeSlippageRate, entryPriceSlippageRage) = recoveryTurn.CalculateMarketOrderSlippageRate(cAlgoPosition.LotSize, cAlgoPosition.EntryPrice);

                        recoveryTurn.SyncPosition(cAlgoPosition.LotSize, cAlgoPosition.EntryPrice);
                    }
                    
                }

                return result;
            }
        }
    }
}
