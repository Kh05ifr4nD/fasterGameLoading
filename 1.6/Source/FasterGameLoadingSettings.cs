using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FasterGameLoading
{
    public class FasterGameLoadingSettings : ModSettings
    {
        public static Dictionary<string, string> loadedTexturesSinceLastSession = new Dictionary<string, string>();
        public static Dictionary<string, ModContentPack> modsByPackageIds = new Dictionary<string, ModContentPack>();
        public static Dictionary<string, string> loadedTypesByFullNameSinceLastSession = new Dictionary<string, string>();
        public static List<string> modsInLastSession = new List<string>();
        public static HashSet<string> successfulXMLPathesSinceLastSession = new HashSet<string>();
        public static HashSet<string> failedXMLPathesSinceLastSession = new HashSet<string>();
        public static bool delayGraphicLoading = true;
        public static bool earlyModContentLoading = true;
        public static bool disableStaticAtlasesBaking;
        public static ModContentPack GetModContent(string packageId)
        {
            var packageLower = packageId.ToLower();
            if (!modsByPackageIds.TryGetValue(packageLower, out var mod))
            {
                modsByPackageIds[packageLower] = mod = LoadedModManager.RunningModsListForReading.FirstOrDefault(x =>
                    x.PackageIdPlayerFacing.ToLower() == packageLower);
            }
            return mod;
        }

        public static void DoSettingsWindowContents(Rect inRect)
        {
            var ls = new Listing_Standard();
            ls.Begin(new Rect(inRect.x, inRect.y, inRect.width, 500));
            ls.CheckboxLabeled("FGL_EarlyContentLoading_Label".Translate(), ref earlyModContentLoading);
            ls.CheckboxLabeled("FGL_DelayGraphicLoading_Label".Translate(), ref delayGraphicLoading);
            ls.CheckboxLabeled("FGL_DisableStaticAtlasesBaking_Label".Translate(), ref disableStaticAtlasesBaking);
            ls.GapLine();
            var explanation = "FGL_TextureDownscale_Description".Translate();
            if (ls.ButtonTextLabeled(explanation, "FGL_TextureDownscale_Button".Translate()))
            {
                Find.WindowStack.Add(new Dialog_MessageBox("FGL_TextureDownscale_Confirm".Translate(), "Confirm".Translate(), delegate
                {
                    TextureResize.DoTextureResizing();
                }, "GoBack".Translate()));
            }
            ls.End();
        }

        public bool ButtonText(Listing_Standard ls, string label, string tooltip = null, float widthPct = 1f)
        {
            Rect rect = ls.GetRect(30f, widthPct);
            bool result = false;
            if (!ls.BoundingRectCached.HasValue || rect.Overlaps(ls.BoundingRectCached.Value))
            {
                result = Widgets.ButtonText(rect, label);
                if (tooltip != null)
                {
                    TooltipHandler.TipRegion(rect, tooltip);
                }
            }
            ls.Gap(ls.verticalSpacing);
            return result;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref loadedTexturesSinceLastSession, "loadedTexturesSinceLastSession", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref loadedTypesByFullNameSinceLastSession, "loadedTypesByFullNameSinceLastSession", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref successfulXMLPathesSinceLastSession, "successfulXMLPathesSinceLastSession", LookMode.Value);
            Scribe_Collections.Look(ref failedXMLPathesSinceLastSession, "failedXMLPathesSinceLastSession", LookMode.Value);
            Scribe_Collections.Look(ref modsInLastSession, "modsInLastSession", LookMode.Value);
            Scribe_Values.Look(ref disableStaticAtlasesBaking, "disableStaticAtlasesBaking");
            Scribe_Values.Look(ref delayGraphicLoading, "delayGraphicLoading", true);
            Scribe_Values.Look(ref earlyModContentLoading, "earlyModContentLoading", true);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                loadedTexturesSinceLastSession ??= new Dictionary<string, string>();
                loadedTypesByFullNameSinceLastSession ??= new Dictionary<string, string>();
                failedXMLPathesSinceLastSession ??= new HashSet<string>();
                successfulXMLPathesSinceLastSession ??= new HashSet<string>();
                modsInLastSession ??= new List<string>();
                if (!modsInLastSession.SequenceEqual(ModsConfig.ActiveModsInLoadOrder.Select(x => x.packageIdLowerCase)))
                {
                    loadedTexturesSinceLastSession.Clear();
                    loadedTypesByFullNameSinceLastSession.Clear();
                    failedXMLPathesSinceLastSession.Clear();
                    successfulXMLPathesSinceLastSession.Clear();
                }
            }
        }
    }
}

