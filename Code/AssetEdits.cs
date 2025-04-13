using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2;

namespace LordsItemEdits
{
    internal static class AssetEdits
    {
        internal static void DoEdits()
        {
            if (ConfigOptions.EnableVoidDiosEdit.Value)
            {
                // no way i'm letting engi turrets get this new void dios lmao
                BlacklistVoidDiosFromEngiTurrets();
                // void allies are playable now so i need to increase all of their interaction ranges since they can't reach anything normally
                GiveReaverAllyBodyInteractionDistance();
                GiveJailerAllyBodyInteractionDistance();
                GiveDevastatorAllyBodyInteractionDistance();
            }
        }



        private static void BlacklistVoidDiosFromEngiTurrets()
        {
            ItemDef voidDiosItemDef = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/ExtraLifeVoid/ExtraLifeVoid.asset").WaitForCompletion();
            voidDiosItemDef.tags = [.. voidDiosItemDef.tags, ItemTag.CannotCopy];
        }

        private static void GiveReaverAllyBodyInteractionDistance()
        {
            GameObject reaverAllyBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierAllyBody.prefab").WaitForCompletion();
            Interactor reaverAllyInteractor = reaverAllyBody.GetComponent<Interactor>();
            reaverAllyInteractor.maxInteractionDistance = 9;
        }

        private static void GiveJailerAllyBodyInteractionDistance()
        {
            GameObject jailerAllyBody = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidJailer/VoidJailerAllyBody.prefab").WaitForCompletion();
            Interactor jailerAllyInteractor = jailerAllyBody.GetComponent<Interactor>();
            jailerAllyInteractor.maxInteractionDistance = 12;
        }

        private static void GiveDevastatorAllyBodyInteractionDistance()
        {
            GameObject devastatorAllyBody = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabAllyBody.prefab").WaitForCompletion();
            Interactor devastatorInteractor = devastatorAllyBody.GetComponent<Interactor>();
            devastatorInteractor.maxInteractionDistance = 15;
        }
    }
}
