using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneRecovery
{
    public enum TradePosition
    {
        None,
        Long,
        Short
    }

    public enum PriceActionResult
    {
        Nothing,
        RecoveryLevelHit,
        TakeProfitLevelHit,
        StopLossLevelHit,
        MaxSlippageLevelHit
    }

    public interface IActiveTurn
    {
        void Update(ZoneRecoveryPosition activeTurn);
    }

    public class ZoneRecoveryCalculator: IActiveTurn
    {               

        public ZoneRecoveryPosition ActivePosition { get; private set; }
        public ZoneLevels ZoneLevels { get; }

        public double UnrealizedGrossProfit { get { return CalculateUnrealizedGrossProfit(); } }
        public double UnrealizedNetProfit { get { return CalculateUnrealizedNetProfit(); } }

        public ZoneRecoveryCalculator(TradePosition initPosition, double entryBidPrice, double entryAskPrice, double initLotSize, double spread, double commission, double slippage, int tradeZoneSize, int zoneRecoverySize)
        {
            ZoneLevels = new ZoneLevels((entryBidPrice + entryAskPrice)/2d, tradeZoneSize, zoneRecoverySize);

            ActivePosition = new ZoneRecoveryPosition(this, null, ZoneLevels, initPosition, initPosition, entryBidPrice, entryAskPrice, initLotSize, spread, commission, slippage);

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

        public void Update(ZoneRecoveryPosition activeTurn)
        {
            ActivePosition = activeTurn;
        }
    }

    public class ZoneRecoveryPosition
    {
        private ZoneLevels _zoneLevels;
        private double _previousPrice;
        private double _currentPrice;
        private TradePosition _entryPosition;        
        public ZoneRecoveryPosition PreviousTurn;
        private double _commissionRate;
        private IActiveTurn _activeTurn;

        public TradePosition Position { get; }
        public double EntryPrice { get; }
        public double LotSize { get; }
        public double Spread { get; }
        public double Commission { get; }        
        public double MaxSlippageRate { get; }
        public bool IsActive { get; private set; }

        private double _unrealizedNetProfit;
        public double UnrealizedNetProfit { get { return _unrealizedNetProfit; } }

        private double _unrealizedGrossProfit;
        public double UnrealizedGrossProfit { get { return _unrealizedGrossProfit; } }

        public ZoneRecoveryPosition(IActiveTurn activeTurn, ZoneRecoveryPosition previousTurn, ZoneLevels zoneLevel, TradePosition entryPosition, TradePosition turnPosition, double entryBidPrice, double entryAskPrice, double lotSize, double spread, double commissionRate, double slippage)
        {
            _activeTurn = activeTurn;

            _activeTurn.Update(this);
            
            _zoneLevels = zoneLevel;
            _entryPosition = entryPosition;            
            PreviousTurn = previousTurn;

            IsActive = true;
            Position = turnPosition;
            if (Position == TradePosition.Long)
            {
                EntryPrice = entryAskPrice;
            }
            if (Position == TradePosition.Short)
            {
                EntryPrice = entryBidPrice;
            }

            _currentPrice = EntryPrice;
            _commissionRate = commissionRate;

            LotSize = lotSize;
            Commission = lotSize * commissionRate;
            Spread = spread;            
            MaxSlippageRate = slippage;
        }

        public Tuple<PriceActionResult, ZoneRecoveryPosition> PriceAction(double bid, double ask)
        {
            PreviousTurn?.PriceAction(bid, ask);

            _previousPrice = _currentPrice;
            _currentPrice = bid;

            if (_entryPosition == TradePosition.Long)
            {
                if (Position == TradePosition.Long)
                {                       
                    _currentPrice = ask;

                    _unrealizedNetProfit = ((_currentPrice - EntryPrice) * LotSize) - Commission;
                    _unrealizedGrossProfit = (_currentPrice - EntryPrice) * LotSize;

                    if (IsActive && _currentPrice >= _zoneLevels.UpperTradingZone)
                    {
                        return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.TakeProfitLevelHit, null);
                    }
                    else if (_currentPrice <= _zoneLevels.LowerRecoveryZone)
                    {
                        if (IsMaximumSlippageHit(_currentPrice))
                        {
                            return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.MaxSlippageLevelHit, null);
                        }
                        else
                        {
                            double commissionRate = 0.34;
                            var newPosition = Position.Reverse();
                            double previousTurnTargetNetReturns = CalculatePreviousTurnNetReturns(newPosition, PreviousTurn);
                            double lotSize = GetLossRecoveryLotSize(_zoneLevels, previousTurnTargetNetReturns, commissionRate);
                            double entryPrice = bid;
                            double spread = bid - ask;

                            IsActive = false;

                            return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.RecoveryLevelHit, new ZoneRecoveryPosition(_activeTurn, this, _zoneLevels, _entryPosition, newPosition, bid, ask, lotSize, spread, commissionRate, MaxSlippageRate));
                        }
                    }
                    else
                    {
                        return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.Nothing, null);
                    }
                }
                else if (Position == TradePosition.Short)
                {
                    _unrealizedNetProfit = ((EntryPrice - _currentPrice) * LotSize) - Commission;
                    _unrealizedGrossProfit = (EntryPrice - _currentPrice) * LotSize;

                    if (_currentPrice<=_zoneLevels.LowerTradingZone)
                    {
                        return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.TakeProfitLevelHit, null);
                    }
                    else if (IsActive && _previousPrice < _zoneLevels.UpperRecoveryZone && _currentPrice >= _zoneLevels.UpperRecoveryZone)
                    {
                        if (IsMaximumSlippageHit(_currentPrice))
                        {
                            return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.MaxSlippageLevelHit, null);
                        }
                        else
                        {
                            double commissionRate = 0.34;
                            var newPosition = Position.Reverse();
                            double previousTurnTargetNetReturns = CalculatePreviousTurnNetReturns(newPosition, PreviousTurn);
                            double lotSize = GetLossRecoveryLotSize(_zoneLevels, previousTurnTargetNetReturns, commissionRate);
                            double entryPrice = ask;
                            double spread = bid - ask;

                            IsActive = false;

                            return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.RecoveryLevelHit, new ZoneRecoveryPosition(_activeTurn, this, _zoneLevels, _entryPosition, newPosition, bid, ask, lotSize, spread, commissionRate, MaxSlippageRate));
                        }
                    }
                    else
                    {
                        return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.Nothing, null);
                    }
                }
            }
            else if (_entryPosition == TradePosition.Short)
            {
                if (Position == TradePosition.Long)
                {
                    _currentPrice = ask;

                    _unrealizedNetProfit = ((_currentPrice - EntryPrice) * LotSize) - Commission;
                    _unrealizedGrossProfit = (_currentPrice - EntryPrice) * LotSize;

                    if (_currentPrice >= _zoneLevels.UpperTradingZone)
                    {
                        return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.StopLossLevelHit, null);
                    }
                    else if (IsActive &&_currentPrice <= _zoneLevels.LowerRecoveryZone)
                    {
                        if (IsMaximumSlippageHit(_currentPrice))
                        {
                            return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.MaxSlippageLevelHit, null);
                        }
                        else
                        {
                            double commissionRate = 0.34;
                            var newPosition = Position.Reverse();
                            double previousTurnTargetNetReturns = CalculatePreviousTurnNetReturns(newPosition, PreviousTurn);
                            double lotSize = GetLossRecoveryLotSize(_zoneLevels, previousTurnTargetNetReturns, commissionRate);
                            double entryPrice = bid;
                            double spread = bid - ask;

                            IsActive = false;

                            return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.RecoveryLevelHit, new ZoneRecoveryPosition(_activeTurn, this, _zoneLevels, _entryPosition, newPosition,  bid, ask, lotSize, spread, commissionRate, MaxSlippageRate));
                        }
                    }
                    else
                    {
                        return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.Nothing, null);
                    }
                }
                else if (Position == TradePosition.Short)
                {
                    _unrealizedNetProfit = ((EntryPrice - _currentPrice) * LotSize) - Commission;
                    _unrealizedGrossProfit = (EntryPrice - _currentPrice) * LotSize;

                    if (_currentPrice <= _zoneLevels.LowerTradingZone)
                    {
                        return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.TakeProfitLevelHit, null);
                    }
                    else if (IsActive && _previousPrice < _zoneLevels.UpperRecoveryZone && _currentPrice >= _zoneLevels.UpperRecoveryZone)
                    {
                        if (IsMaximumSlippageHit(_currentPrice))
                        {
                            return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.MaxSlippageLevelHit, null);
                        }
                        else
                        {
                            double commissionRate = 0.34;
                            var newPosition = Position.Reverse();
                            double previousTurnTargetNetReturns = CalculatePreviousTurnNetReturns(newPosition, PreviousTurn);
                            double lotSize = GetLossRecoveryLotSize(_zoneLevels, previousTurnTargetNetReturns, commissionRate);
                            double entryPrice = ask;
                            double spread = bid - ask;

                            IsActive = false;

                            return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.RecoveryLevelHit, new ZoneRecoveryPosition(_activeTurn, this, _zoneLevels, _entryPosition, newPosition, bid, ask, lotSize, spread, commissionRate, MaxSlippageRate));
                        }
                    }
                    else
                    {
                        return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.Nothing, null);
                    }
                }
            }

            
            return new Tuple<PriceActionResult, ZoneRecoveryPosition>(PriceActionResult.Nothing, null);
        }

        private bool IsMaximumSlippageHit(double currentPrice)
        {
            if (Position == TradePosition.Long)
            {
                return (_zoneLevels.LowerRecoveryZone - currentPrice) / (_zoneLevels.UpperRecoveryZone - _zoneLevels.LowerRecoveryZone) > MaxSlippageRate;
            }
            else if (Position == TradePosition.Short)
            {
                return (currentPrice - _zoneLevels.UpperRecoveryZone) / (_zoneLevels.UpperRecoveryZone - _zoneLevels.LowerRecoveryZone) > MaxSlippageRate;
            }
            else
            {
                return false;
            }
        }

        private double GetLossRecoveryLotSize(ZoneLevels zoneLevels, double previousTurnTargetNetReturns, double commissionRate)
        {
            // Total Gain Potential (TPG) - Total Loss Potential (TPL) = 0
            // Commission (CO) = Recovery Size (RS) * Commission Rate (CR)
            // Total Gain Potential (TPG) = Recovery Size (RS) * (Recovery Level (RL) - Stop Loss Level (SL)) - Commission (CO)
            // Total Loss Potential (TPL) = Previous Recovery Size (PRS) * ABS(Previous Entry Levell (PEL)- Previous Stop Loss Level (PSL)) + Previous Turn Commission (PTC)


            //TPG - TPL = 0
            //CO = RS * CR
            //TPG = (RS * (RL - SL)) - CO
            //TPL = PRS * ABS(PEL - PSL) + PTC
            //(RS * (RL - SL)) - (RS * CR) =  PRS * ABS(PEL - PSL) + PTC
            //RS (RL-SL) - CR =  PRS * ABS(PEL - PSL) + PTC
            //RS = (PRS * ABS(PEL - PSL) + PTC) / (RL -SL - CR)                                                

            if (Position == TradePosition.Long)
            {
                return (LotSize * (EntryPrice - zoneLevels.LowerTradingZone) + Commission - previousTurnTargetNetReturns) / (zoneLevels.LowerRecoveryZone - zoneLevels.LowerTradingZone - _commissionRate);
            }
            else if (Position == TradePosition.Short)
            {
                return (LotSize * (zoneLevels.UpperTradingZone - EntryPrice) + Commission - previousTurnTargetNetReturns) / (zoneLevels.UpperTradingZone - zoneLevels.UpperRecoveryZone - _commissionRate);
            }
            else
            {
                return 0; //Invalid position
            }
        }

        private double CalculatePreviousTurnNetReturns(TradePosition entryPosition, ZoneRecoveryPosition previousTurn)
        {
            var turn = previousTurn;
            double totalNetReturns = 0;
            while(turn != null)
            {
                if (_entryPosition == TradePosition.Long)
                {
                    if (entryPosition == TradePosition.Short)
                    {
                        if (turn.Position == TradePosition.Long)
                        {
                            totalNetReturns -= turn.LotSize * (_zoneLevels.UpperRecoveryZone - _zoneLevels.LowerTradingZone) + turn.Commission;
                        }
                        else if (turn.Position == TradePosition.Short)
                        {
                            totalNetReturns += turn.LotSize * (_zoneLevels.LowerRecoveryZone - _zoneLevels.LowerTradingZone) - turn.Commission;
                        }
                    }
                    else if (entryPosition == TradePosition.Long)
                    {
                        if (turn.Position == TradePosition.Long)
                        {
                            totalNetReturns += turn.LotSize * (_zoneLevels.UpperTradingZone - _zoneLevels.UpperRecoveryZone) - turn.Commission;
                        }
                        else if (turn.Position == TradePosition.Short)
                        {
                            totalNetReturns -= turn.LotSize * (_zoneLevels.UpperTradingZone - _zoneLevels.UpperRecoveryZone) + turn.Commission;
                        }
                    }
                }
                else if (_entryPosition == TradePosition.Short)
                {
                    if (entryPosition == TradePosition.Long)
                    {
                        totalNetReturns += turn.LotSize * (_zoneLevels.UpperRecoveryZone - _zoneLevels.LowerTradingZone) - turn.Commission;
                    }
                    else if (entryPosition == TradePosition.Short)
                    {
                        totalNetReturns += turn.LotSize * (_zoneLevels.LowerRecoveryZone - _zoneLevels.LowerTradingZone) - turn.Commission;
                    }
                }
                turn = turn.PreviousTurn;
            }

            return totalNetReturns;
        }
            
    }

    public class ZoneLevels
    {
        public double UpperTradingZone { get; }
        public double LowerTradingZone { get; }
        public double UpperRecoveryZone { get; }
        public double LowerRecoveryZone { get; }

        public ZoneLevels(double entryPrice, int tradeZoneSize, int zoneRecoverySize)
        {
            UpperTradingZone = entryPrice + tradeZoneSize;
            LowerTradingZone = entryPrice - zoneRecoverySize - tradeZoneSize;
            UpperRecoveryZone = entryPrice;
            LowerRecoveryZone = entryPrice - zoneRecoverySize;
        }
    }

    public static class Extensions
    {
        public static TradePosition Reverse(this TradePosition position)
        {
            if (position == TradePosition.Long)
                return TradePosition.Short;
            else if (position == TradePosition.Short)
                return TradePosition.Long;
            else
                return position;
        }
    }
}
