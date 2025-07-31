using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace LordsItemEdits.ItemEdits
{
    public static class PocketICBM
    {
        private const float _initialMult = 3f;
        private const float _stackMult = 1.5f;

        internal static void Setup()
        {
            if (!ConfigOptions.PocketICBM.EnableEdit.Value)
            {
                return;
            }

            LanguageAPI.AddOverlayPath(ModUtil.GetLangFileLocation("PocketICBM"));
            // other effects of ICBM are handled in their respective files
        }

        public static float GetICBMDamageMult(CharacterBody characterBody)
        {
            int icbmCount = 0;
            if (characterBody != null && characterBody.inventory != null)
            {
                icbmCount = characterBody.inventory.GetItemCount(DLC1Content.Items.MoreMissile);
            }

            if (icbmCount > 0)
            {
                return _initialMult + (_stackMult * (icbmCount - 1));
            }
            else
            {
                return 1;
            }
        }

        public static float GetICBMDamageMult(int icbmCount)
        {
            return _initialMult + (_stackMult * (icbmCount - 1));
        }
    }
}