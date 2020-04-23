using System;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryStrategy
{
    public class Strategy
    {
        private ZoneRecovery _zoneRecovery;
        private Session _session;
        private double _initLotSize;
        private Delegates.MarketOrder _marketOrder;
        private Delegates.LimitOrder _limitOrder;

        public void Initialize(double initLotSize, double pipFactor, double commissionRate, double profitMarginRate, double slippage)
        {
            _initLotSize = initLotSize;
            _zoneRecovery = new ZoneRecovery(initLotSize, pipFactor, commissionRate, profitMarginRate, slippage);
        }

        public bool StartSession(MarketPosition position, double entryBidPrice, double entryAskPrice, double tradeZoneSize, double zoneRecoverySize, Delegates.MarketOrder marketOrder, Delegates.LimitOrder limitOrder)
        {
            _marketOrder = marketOrder;
            _limitOrder = limitOrder;

            if (position == MarketPosition.None)
            {
                throw new Exception("Invalid position.");                
            }

            _session = _zoneRecovery.CreateSession(position, entryBidPrice, entryAskPrice, tradeZoneSize, zoneRecoverySize);

            double entryPrice = double.NaN;
            double stopLossLevel = double.NaN;
            double takeProfitLevel = double.NaN;

            if (position == MarketPosition.Long)
            {
                entryPrice = entryAskPrice;
                stopLossLevel = _session.ZoneLevels.StopLossLevel;
                takeProfitLevel = _session.ZoneLevels.TakeProfitLevel;
            }
            else if (position == MarketPosition.Short)
            {
                entryPrice = entryBidPrice;
                stopLossLevel = _session.ZoneLevels.TakeProfitLevel;
                takeProfitLevel = _session.ZoneLevels.StopLossLevel;
            }
          
            var (isSuccessful, message) = _marketOrder(_initLotSize, entryPrice, position.GetValue(), stopLossLevel, takeProfitLevel);

            if (isSuccessful)
            {
                
            }

            double limitOrderLotSize = _session.ActivePosition.GetNextTurnLotSize();

            var limitOrderZoneLevels = _session.ZoneLevels.Reverse();

            var (isLimitSuccessful, limitOrderMessage) = _limitOrder(limitOrderLotSize, limitOrderZoneLevels.EntryPrice, limitOrderZoneLevels.Position.GetValue(), limitOrderZoneLevels.StopLossLevel, limitOrderZoneLevels.TakeProfitLevel);

            if (isLimitSuccessful)
            {

            }

            return isSuccessful;
        }

        public PriceActionResult PriceTick(long timestamp, double bid, double ask)
        {
            if (_session == null)
            {
                return PriceActionResult.Nothing;
            }
            else
            {
                var (result, recoveryTurn) = _session.PriceAction(bid, ask);

                double entryPrice = double.NaN;


                if (result == PriceActionResult.RecoveryLevelHit)
                {
                    var position = _session.ActivePosition.Position.Reverse();

                    if (position == MarketPosition.Long)
                    {
                        entryPrice = ask;
                    }
                    else if (position == MarketPosition.Short)
                    {
                        entryPrice = bid;
                    }
                    else
                    {
                        throw new Exception("Invalid position.");
                    }

                    var (isSuccessful, message) = _marketOrder(recoveryTurn.LotSize, entryPrice, recoveryTurn.Position.GetValue(), 0, 0);

                    if (isSuccessful)
                    {
                        var (lotSizeSlippageRate, entryPriceSlippageRage) = recoveryTurn.CalculateMarketOrderSlippageRate(recoveryTurn.LotSize, entryPrice);

                        recoveryTurn.SyncPosition(recoveryTurn.LotSize, entryPrice);
                    }

                }

                return result;
            }
        }

        public void EndSession()
        {

        }
    }
}
