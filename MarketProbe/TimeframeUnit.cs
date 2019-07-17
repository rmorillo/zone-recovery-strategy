using System;
using System.Collections.Generic;
using System.Text;

namespace MarketProbe
{
    public class TimeframeUnit : FeedInterval
    {
        public TimeframeUnit(TimeframeOption timeframe)
        {
            Unit = IntervalOption.Timeframe;
            UnitId = (int)timeframe;
            UnitType = typeof(TimeframeOption);
        }

        public TimeframeOption Timeframe
        {
            get
            {
                return (TimeframeOption)UnitId;
            }
        }
    }
}
