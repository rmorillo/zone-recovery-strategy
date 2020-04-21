using System;
using ZoneRecoveryAlgorithm;

namespace ZoneRecoveryStrategy
{
    public class Strategy
    {
        private ZoneRecovery _zoneRecovery;
        private Session _session;
        private double _initLotSize;

        public void Initialize(double initLotSize, double pipFactor, double commissionRate, double profitMarginRate, double slippage)
        {
            _initLotSize = initLotSize;
            _zoneRecovery = new ZoneRecovery(initLotSize, pipFactor, commissionRate, profitMarginRate, slippage);
        }

        public bool StartSession(MarketPosition position, double entryBidPrice, double entryAskPrice, double tradeZoneSize, double zoneRecoverySize, Delegates.MarketOrder firstMarketOrder)
        {
            double entryPrice = double.NaN;

            if (position == MarketPosition.Long)
            {
                entryPrice = entryAskPrice;
            }
            else if (position == MarketPosition.Short)
            {
                entryPrice = entryBidPrice;
            }
            else
            {
                throw new Exception("Invalid position.");
            }

            var (isSuccessful, message, (orderLotSize, orderEntryPrice, orderSpread)) = firstMarketOrder(_initLotSize, entryPrice, position.GetValue(), 0, 0);
            if (isSuccessful)
            {
                _session = _zoneRecovery.CreateSession(position, entryBidPrice, entryAskPrice, tradeZoneSize, zoneRecoverySize);
            }

            return isSuccessful;
        }

        public PriceActionResult PriceTick(long timestamp, double bid, double ask, Delegates.MarketOrder recoveryTurnMarketOrder)
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

                    var (isSuccessful, message, (orderLotSize, orderEntryPrice, orderSpread)) = recoveryTurnMarketOrder(recoveryTurn.LotSize, entryPrice, recoveryTurn.Position.GetValue(), 0, 0);

                    if (isSuccessful)
                    {
                        var (lotSizeSlippageRate, entryPriceSlippageRage) = recoveryTurn.CalculateMarketOrderSlippageRate(orderLotSize, orderEntryPrice);

                        recoveryTurn.SyncPosition(orderLotSize, orderEntryPrice);
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
