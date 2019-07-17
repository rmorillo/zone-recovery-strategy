using System;

namespace ZoneRecoveryAlgorithm
{           
    public class Session: IActiveTurn
    {               

        public RecoveryTurn ActivePosition { get; private set; }
        public ZoneLevels ZoneLevels { get; }

        public double UnrealizedGrossProfit { get { return CalculateUnrealizedGrossProfit(); } }
        public double UnrealizedNetProfit { get { return CalculateUnrealizedNetProfit(); } }

        public double TotalLotSize { get { return CalculateTotalLotSize(); } }

        public double RecoveryTurns {  get { return ActivePosition.TurnIndex; } }

        public Session(MarketPosition initPosition, double entryBidPrice, double entryAskPrice, double initLotSize, double spread, double pipFactor, double commission, double profitMarginRate, double slippage, double tradeZoneSize, double zoneRecoverySize)
        {
            var midPrice = (entryBidPrice + entryAskPrice) / 2d;            
            ZoneLevels = new ZoneLevels(initPosition, midPrice, tradeZoneSize, zoneRecoverySize);
            double profitMargin = tradeZoneSize * profitMarginRate;

            ActivePosition = new RecoveryTurn(this, null, ZoneLevels, initPosition, initPosition, entryBidPrice, entryAskPrice, initLotSize, pipFactor, spread, commission, profitMargin, slippage);
        }

        public (PriceActionResult, RecoveryTurn) PriceAction(double bid, double ask)
        {
            return ActivePosition.PriceAction(bid, ask);
        }

        private double CalculateUnrealizedNetProfit()
        {
            var turn = ActivePosition;
            double totalNetReturns = 0;
            while (turn != null)
            {
                totalNetReturns += turn.UnrealizedNetProfit;
                turn = turn.PreviousTurn;
            }

            return totalNetReturns;
        }

        private double CalculateUnrealizedGrossProfit()
        {
            var turn = ActivePosition;
            double totalNetReturns = 0;
            while (turn != null)
            {
                totalNetReturns += turn.UnrealizedGrossProfit;
                turn = turn.PreviousTurn;
            }

            return totalNetReturns;
        }

        private double CalculateTotalLotSize()
        {
            var turn = ActivePosition;
            double totalLotSize = 0;
            while (turn != null)
            {
                totalLotSize += turn.LotSize;
                turn = turn.PreviousTurn;
            }

            return totalLotSize;
        }

        public void Update(RecoveryTurn activeTurn)
        {
            ActivePosition = activeTurn;
        }
    }    
}
