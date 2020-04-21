using System;

namespace ZoneRecoveryAlgorithm
{
    public enum MarketPosition
    {
        None = 0,
        Long = 1,
        Short = -1
    }

    public static class Extensions
    {
        public static MarketPosition Reverse(this MarketPosition position)
        {
            if (position == MarketPosition.Long)
                return MarketPosition.Short;
            else if (position == MarketPosition.Short)
                return MarketPosition.Long;
            else
                return position;
        }

        public static sbyte GetValue(this MarketPosition position)
        {
            return (sbyte)position;
        }
    }
}
