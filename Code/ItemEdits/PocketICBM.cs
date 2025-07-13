using System;
using System.Collections.Generic;
using System.Text;

namespace LordsItemEdits.ItemEdits
{
    public static class PocketICBM
    {
        // there's no setup method here because this item exists to checked for for certain items/skills to add the special functionality

        private const float _initialMult = 3f;
        private const float _stackMult = 1.5f;

        public static float GetICBMDamageMult(int icbmCount)
        {
            return _initialMult + (_stackMult * (icbmCount - 1));
        }
    }
}