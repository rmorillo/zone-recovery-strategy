using System;
using System.Collections.Generic;
using System.Text;

namespace ZoneRecoveryAlgorithm
{
    public interface IActiveTurn
    {
        void Update(RecoveryTurn activeTurn);
    }
}
