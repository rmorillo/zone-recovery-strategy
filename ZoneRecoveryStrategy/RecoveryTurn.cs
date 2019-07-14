using System;
using System.Collections.Generic;
using System.Text;

namespace ZoneRecoveryAlgorithm
{
    public class RecoveryTurn
    {
        private ZoneLevels _zoneLevels;
        private double _previousPrice;
        private double _currentPrice;
        private MarketPosition _entryPosition;
        public RecoveryTurn PreviousTurn;
        private double _commissionRate;
        private IActiveTurn _activeTurn;

        public MarketPosition Position { get; }
        public double EntryPrice { get; }
        public double LotSize { get; }
        public double Spread { get; }
        public double Commission { get; }
        public double MaxSlippageRate { get; }
        public bool IsActive { get; private set; }
        public int TurnIndex { get; }

        private double _unrealizedNetProfit;
        public double UnrealizedNetProfit { get { return _unrealizedNetProfit; } }

        private double _unrealizedGrossProfit;
        public double UnrealizedGrossProfit { get { return _unrealizedGrossProfit; } }

        public RecoveryTurn(IActiveTurn activeTurn, RecoveryTurn previousTurn, ZoneLevels zoneLevel, MarketPosition entryPosition, MarketPosition turnPosition, double entryBidPrice, double entryAskPrice, double lotSize, double spread, double commissionRate, double slippage)
        {
            _activeTurn = activeTurn;

            _activeTurn.Update(this);

            if (previousTurn==null)
            {
                TurnIndex = 0;            
            }
            else
            {
                TurnIndex = previousTurn.TurnIndex + 1;
            }

            _zoneLevels = zoneLevel;
            _entryPosition = entryPosition;
            PreviousTurn = previousTurn;

            IsActive = true;
            Position = turnPosition;
            if (Position == MarketPosition.Long)
            {
                EntryPrice = entryAskPrice;
            }
            if (Position == MarketPosition.Short)
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

        public Tuple<PriceActionResult, RecoveryTurn> PriceAction(double bid, double ask)
        {
            PreviousTurn?.PriceAction(bid, ask);

            _previousPrice = _currentPrice;
            _currentPrice = bid;

            if (_entryPosition == MarketPosition.Long)
            {
                if (Position == MarketPosition.Long)
                {
                    _currentPrice = ask;

                    _unrealizedNetProfit = ((_currentPrice - EntryPrice) * LotSize) - Commission;
                    _unrealizedGrossProfit = (_currentPrice - EntryPrice) * LotSize;

                    if (IsActive && _currentPrice >= _zoneLevels.UpperTradingZone)
                    {
                        return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.TakeProfitLevelHit, null);
                    }
                    else if (_currentPrice <= _zoneLevels.LowerRecoveryZone)
                    {
                        if (IsMaximumSlippageHit(_currentPrice))
                        {
                            return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.MaxSlippageLevelHit, null);
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

                            return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.RecoveryLevelHit, new RecoveryTurn(_activeTurn, this, _zoneLevels, _entryPosition, newPosition, bid, ask, lotSize, spread, commissionRate, MaxSlippageRate));
                        }
                    }
                    else
                    {
                        return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.Nothing, null);
                    }
                }
                else if (Position == MarketPosition.Short)
                {
                    _unrealizedNetProfit = ((EntryPrice - _currentPrice) * LotSize) - Commission;
                    _unrealizedGrossProfit = (EntryPrice - _currentPrice) * LotSize;

                    if (_currentPrice <= _zoneLevels.LowerTradingZone)
                    {
                        return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.TakeProfitLevelHit, null);
                    }
                    else if (IsActive && _previousPrice < _zoneLevels.UpperRecoveryZone && _currentPrice >= _zoneLevels.UpperRecoveryZone)
                    {
                        if (IsMaximumSlippageHit(_currentPrice))
                        {
                            return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.MaxSlippageLevelHit, null);
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

                            return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.RecoveryLevelHit, new RecoveryTurn(_activeTurn, this, _zoneLevels, _entryPosition, newPosition, bid, ask, lotSize, spread, commissionRate, MaxSlippageRate));
                        }
                    }
                    else
                    {
                        return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.Nothing, null);
                    }
                }
            }
            else if (_entryPosition == MarketPosition.Short)
            {
                if (Position == MarketPosition.Long)
                {
                    _currentPrice = ask;

                    _unrealizedNetProfit = ((_currentPrice - EntryPrice) * LotSize) - Commission;
                    _unrealizedGrossProfit = (_currentPrice - EntryPrice) * LotSize;

                    if (_currentPrice >= _zoneLevels.UpperTradingZone)
                    {
                        return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.StopLossLevelHit, null);
                    }
                    else if (IsActive && _currentPrice <= _zoneLevels.LowerRecoveryZone)
                    {
                        if (IsMaximumSlippageHit(_currentPrice))
                        {
                            return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.MaxSlippageLevelHit, null);
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

                            return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.RecoveryLevelHit, new RecoveryTurn(_activeTurn, this, _zoneLevels, _entryPosition, newPosition, bid, ask, lotSize, spread, commissionRate, MaxSlippageRate));
                        }
                    }
                    else
                    {
                        return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.Nothing, null);
                    }
                }
                else if (Position == MarketPosition.Short)
                {
                    _unrealizedNetProfit = ((EntryPrice - _currentPrice) * LotSize) - Commission;
                    _unrealizedGrossProfit = (EntryPrice - _currentPrice) * LotSize;

                    if (_currentPrice <= _zoneLevels.LowerTradingZone)
                    {
                        return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.TakeProfitLevelHit, null);
                    }
                    else if (IsActive && _previousPrice < _zoneLevels.UpperRecoveryZone && _currentPrice >= _zoneLevels.UpperRecoveryZone)
                    {
                        if (IsMaximumSlippageHit(_currentPrice))
                        {
                            return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.MaxSlippageLevelHit, null);
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

                            return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.RecoveryLevelHit, new RecoveryTurn(_activeTurn, this, _zoneLevels, _entryPosition, newPosition, bid, ask, lotSize, spread, commissionRate, MaxSlippageRate));
                        }
                    }
                    else
                    {
                        return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.Nothing, null);
                    }
                }
            }


            return new Tuple<PriceActionResult, RecoveryTurn>(PriceActionResult.Nothing, null);
        }

        private bool IsMaximumSlippageHit(double currentPrice)
        {
            if (Position == MarketPosition.Long)
            {
                return (_zoneLevels.LowerRecoveryZone - currentPrice) / (_zoneLevels.UpperRecoveryZone - _zoneLevels.LowerRecoveryZone) > MaxSlippageRate;
            }
            else if (Position == MarketPosition.Short)
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

            if (Position == MarketPosition.Long)
            {
                return (LotSize * (EntryPrice - zoneLevels.LowerTradingZone) + Commission - previousTurnTargetNetReturns) / (zoneLevels.LowerRecoveryZone - zoneLevels.LowerTradingZone - _commissionRate);
            }
            else if (Position == MarketPosition.Short)
            {
                return (LotSize * (zoneLevels.UpperTradingZone - EntryPrice) + Commission - previousTurnTargetNetReturns) / (zoneLevels.UpperTradingZone - zoneLevels.UpperRecoveryZone - _commissionRate);
            }
            else
            {
                return 0; //Invalid position
            }
        }

        private double CalculatePreviousTurnNetReturns(MarketPosition entryPosition, RecoveryTurn previousTurn)
        {
            var turn = previousTurn;
            double totalNetReturns = 0;
            while (turn != null)
            {
                if (_entryPosition == MarketPosition.Long)
                {
                    if (entryPosition == MarketPosition.Short)
                    {
                        if (turn.Position == MarketPosition.Long)
                        {
                            totalNetReturns -= turn.LotSize * (_zoneLevels.UpperRecoveryZone - _zoneLevels.LowerTradingZone) + turn.Commission;
                        }
                        else if (turn.Position == MarketPosition.Short)
                        {
                            totalNetReturns += turn.LotSize * (_zoneLevels.LowerRecoveryZone - _zoneLevels.LowerTradingZone) - turn.Commission;
                        }
                    }
                    else if (entryPosition == MarketPosition.Long)
                    {
                        if (turn.Position == MarketPosition.Long)
                        {
                            totalNetReturns += turn.LotSize * (_zoneLevels.UpperTradingZone - _zoneLevels.UpperRecoveryZone) - turn.Commission;
                        }
                        else if (turn.Position == MarketPosition.Short)
                        {
                            totalNetReturns -= turn.LotSize * (_zoneLevels.UpperTradingZone - _zoneLevels.LowerRecoveryZone) + turn.Commission;
                        }
                    }
                }
                else if (_entryPosition == MarketPosition.Short)
                {
                    if (entryPosition == MarketPosition.Long)
                    {
                        totalNetReturns += turn.LotSize * (_zoneLevels.UpperRecoveryZone - _zoneLevels.LowerTradingZone) - turn.Commission;
                    }
                    else if (entryPosition == MarketPosition.Short)
                    {
                        totalNetReturns += turn.LotSize * (_zoneLevels.LowerRecoveryZone - _zoneLevels.LowerTradingZone) - turn.Commission;
                    }
                }
                turn = turn.PreviousTurn;
            }

            return totalNetReturns;
        }

    }
}
