using CUE4Parse.UE4.Assets.Exports;
using FortnitePorting.Utils;
using static FortnitePorting.Program;

namespace FortnitePorting.Types
{
    public static class Character
    {
        public static void ProcessCharacter(string input)
        {
            var path = "FortniteGame/Content/Athena/Items/Cosmetics/Characters/" + input;
            if (!input.StartsWith("CID_"))
            {
                path = BenbotApi.GetCosmeticPath(BenbotApi.EBackendType.AthenaCharacter, input);
            }

            if (Provider.TryLoadObject(path, out var CharacterItemDefinition))
            {
                var characterParts = CharacterItemDefinition.Get<UObject>("HeroDefinition")
                    .Get<UObject[]>("Specializations")[0]
                    .Get<UObject[]>("CharacterParts");

                if (characterParts.Length > 0)
                {
                    // TODO AssetUtils.FillCharacterParts(characterParts);
                }

                if (CharacterItemDefinition.TryGetValue(out UObject[] ItemVariants, "ItemVariants"))
                {
                    // TODO AssetUtils.FillVariants(ItemVariants);
                }
            }
        }   
    }
}