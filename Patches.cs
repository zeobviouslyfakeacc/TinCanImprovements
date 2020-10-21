using Harmony;
using UnhollowerBaseLib;
using UnityEngine;

namespace TinCanImprovements {
	internal static class Patches {

		private const float TIN_CAN_WEIGHT_KG = 0.05f;
		private const string TIN_CAN_NAME = "GEAR_RecycledCan";
		private const string SCRAP_METAL_NAME = "GEAR_ScrapMetal";
		private const string SCRAP_METAL_CRAFTING_ICON_NAME = "ico_CraftItem__ScrapMetal";

		[HarmonyPatch(typeof(GearItem), "ManualStart")]
		private static class ChangeTinCanWeightPatch {
			internal static void Postfix(GearItem __instance) {
				if (__instance.name == TIN_CAN_NAME) {
					__instance.m_WeightKG = TIN_CAN_WEIGHT_KG;
				}
			}
		}

		[HarmonyPatch(typeof(GameManager), "Awake")]
		private static class AddBreakDownRecipe {
			internal static void Postfix() {
				BlueprintItem blueprint = GameManager.GetBlueprints().AddComponent<BlueprintItem>();

				// Inputs
				blueprint.m_RequiredGear = new Il2CppReferenceArray<GearItem>(1) { [0] = GetGearItemPrefab(TIN_CAN_NAME) };
				blueprint.m_RequiredGearUnits = new Il2CppStructArray<int>(1) { [0] = 4 };
				blueprint.m_KeroseneLitersRequired = 0f;
				blueprint.m_GunpowderKGRequired = 0f;
				blueprint.m_RequiredTool = null;
				blueprint.m_OptionalTools = new Il2CppReferenceArray<ToolsItem>(0);

				// Outputs
				blueprint.m_CraftedResult = GetGearItemPrefab(SCRAP_METAL_NAME);
				blueprint.m_CraftedResultCount = 1;

				// Process
				blueprint.m_Locked = false;
				blueprint.m_AppearsInStoryOnly = false;
				blueprint.m_RequiresLight = false;
				blueprint.m_RequiresLitFire = false;
				blueprint.m_RequiredCraftingLocation = CraftingLocation.Anywhere;
				blueprint.m_DurationMinutes = 20;
				blueprint.m_CraftingAudio = "PLAY_CRAFTINGGENERIC";
				blueprint.m_AppliedSkill = SkillType.None;
				blueprint.m_ImprovedSkill = SkillType.None;
			}

			private static GearItem GetGearItemPrefab(string name) => Resources.Load(name).Cast<GameObject>().GetComponent<GearItem>();
		}

		[HarmonyPatch(typeof(Panel_Crafting), "ItemPassesFilter")]
		private static class ShowScrapMetalRecipeInToolsRecipes {
			internal static void Postfix(Panel_Crafting __instance, ref bool __result, BlueprintItem bpi) {
				if (bpi?.m_CraftedResult?.name == SCRAP_METAL_NAME && __instance.m_CurrentCategory == Panel_Crafting.Category.Tools) {
					__result = true;
				}
			}
		}

		[HarmonyPatch(typeof(BlueprintDisplayItem), "Setup")]
		private static class FixScrapMetalRecipeIcon {
			internal static void Postfix(BlueprintDisplayItem __instance, BlueprintItem bpi) {
				if (bpi?.m_CraftedResult?.name == SCRAP_METAL_NAME) {
					Texture2D scrapMetalTexture = Utils.GetCachedTexture(SCRAP_METAL_CRAFTING_ICON_NAME);
					if (!scrapMetalTexture) {
						scrapMetalTexture = TinCanImprovementsMod.assetBundle.LoadAsset(SCRAP_METAL_CRAFTING_ICON_NAME).Cast<Texture2D>();
						Utils.CacheTexture(SCRAP_METAL_CRAFTING_ICON_NAME, scrapMetalTexture);
					}
					__instance.m_Icon.mTexture = scrapMetalTexture;
				}
			}
		}

		[HarmonyPatch(typeof(GearItem), "GetSingleItemWeightKG")]
		private static class GetSingleItemWeightKG_Patch {
			internal static void Prefix(GearItem __instance, ref float __state) => WeightPrefix(__instance, ref __state);
			internal static void Postfix(GearItem __instance, ref float __state, ref float __result) => WeightPostfix(__instance, ref __state, ref __result);
		}

		[HarmonyPatch(typeof(GearItem), "GetItemWeightKG")]
		private static class GetItemWeightKG_Patch {
			internal static void Prefix(GearItem __instance, ref float __state) => WeightPrefix(__instance, ref __state);
			internal static void Postfix(GearItem __instance, ref float __state, ref float __result) => WeightPostfix(__instance, ref __state, ref __result);
		}

		[HarmonyPatch(typeof(GearItem), "GetItemWeightIgnoreClothingWornBonusKG")]
		private static class GetItemWeightIgnoreClothingWornBonusKG_Patch {
			internal static void Prefix(GearItem __instance, ref float __state) => WeightPrefix(__instance, ref __state);
			internal static void Postfix(GearItem __instance, ref float __state, ref float __result) => WeightPostfix(__instance, ref __state, ref __result);
		}

		private static void WeightPrefix(GearItem __instance, ref float __state) {
			if (ShouldAdjustWeight(__instance)) {
				float oldWeight = __instance.m_WeightKG;
				__state = oldWeight;
				__instance.m_WeightKG = oldWeight - TIN_CAN_WEIGHT_KG;
			} else {
				__state = -1f;
			}
		}

		private static void WeightPostfix(GearItem __instance, ref float __state, ref float __result) {
			if (__state >= 0) {
				__result += TIN_CAN_WEIGHT_KG;
				__instance.m_WeightKG = __state;
			}
		}

		private static bool ShouldAdjustWeight(GearItem gearItem) {
			if (!gearItem) return false;

			FoodItem foodItem = gearItem.m_FoodItem;
			if (!foodItem) return false;
			GameObject canPrefab = foodItem.m_GearPrefabHarvestAfterFinishEatingNormal;
			if (!canPrefab || canPrefab.name != TIN_CAN_NAME) return false;

			return true;
		}
	}
}
