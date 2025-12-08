using MonoDetour;
using MonoDetour.HookGen;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
namespace LordsItemEdits.ItemEdits;


[MonoDetourTargets(typeof(GlobalEventManager))]
public static class PocketICBM
{
    private const float _initialMult = 3f;
    private const float _stackMult = 1.5f;

    [MonoDetourHookInitialize]
    internal static void Setup()
    {
        if (!ConfigOptions.PocketICBM.EnableEdit.Value)
        {
            return;
        }

        ModLanguage.LangFilesToLoad.Add("PocketICBM");
        // other effects of ICBM are handled in their respective files
    }

    public static float GetICBMDamageMult(CharacterBody characterBody)
    {
        int icbmCount = 0;
        if (characterBody != null && characterBody.inventory != null)
        {
            icbmCount = characterBody.inventory.GetItemCountEffective(DLC1Content.Items.MoreMissile);
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