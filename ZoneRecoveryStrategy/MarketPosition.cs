namespace ZoneRecoveryAlgorithm
{
    public enum MarketPosition
    {
        None,
        Long,
        Short
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
    }
}
