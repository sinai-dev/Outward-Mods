using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using SideLoader;
using SharedModConfig;
using UnityEngine;
using UnityEngine.UI;

namespace CoopUIScalerFix
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CoopUIScalerMod : BaseUnityPlugin
    {
		internal const string GUID = "com.sinai.coopuiscalerfix";
		internal const string NAME = "Coop UI Scaler Fix";
		internal const string VERSION = "1.0.0";

		private static int lastScreenHeight;
		private static int lastScreenWidth;

		//private static bool moveGlobalUiToPlayer1 = true;
		//private static float scaleFactor = 1f;

		public enum Settings
        {
			moveGlobalUiToPlayer1,
			splitMode,
			scaleFactor,
		}

		internal static ModConfig CONFIG = new ModConfig
		{
			ModName = NAME,
			SettingsVersion = 1.0,
			Settings = new List<BBSetting>
            {
				new BoolSetting
                {
					Name = Settings.moveGlobalUiToPlayer1.ToString(),
					Description = "Move Global UI to Player 1? (default true)",
					DefaultValue = true,
                },
				new FloatSetting
                {
					Name = Settings.scaleFactor.ToString(),
					Description = "Scale Factor (default 1.0x)",
					DefaultValue = 1.0f,
					Increment = 0.1f,
					RoundTo = 1,
					MinValue = 0.1f,
					MaxValue = 10f,
                }
            }
		};

		internal void Awake()
        {
			new Harmony("com.sinai.coopuiscaler").PatchAll();
			CONFIG.Register();
            CONFIG.OnSettingsSaved += CONFIG_OnSettingsSaved;
        }

        private void CONFIG_OnSettingsSaved()
        {
			m_refreshWanted = true;
        }

		internal static bool m_refreshWanted;

        [HarmonyPatch(typeof(MapDisplay), nameof(MapDisplay.Show), new Type[] { typeof(CharacterUI) })]
		public class MapDisplay_Show
        {
			[HarmonyPostfix]
			public static void Postfix(MapDisplay __instance, CharacterUI _owner)
			{
				if ((bool)CONFIG.GetValue(Settings.moveGlobalUiToPlayer1.ToString()))
				{
					var rect = (RectTransform)At.GetField(_owner, "m_rectTransform");
					__instance.RectTransform.anchoredPosition = rect.anchoredPosition;
					__instance.RectTransform.sizeDelta = rect.sizeDelta;
				}
				else
				{
					__instance.RectTransform.anchoredPosition = Vector2.zero;
					__instance.RectTransform.sizeDelta = Vector2.zero;
				}
			}
        }

		[HarmonyPatch(typeof(OptionsPanel), "StartInit")]
		public class OptionsPanel_StartInit
        {
			[HarmonyPostfix]
			public static void Postfix(OptionsPanel __instance)
            {
				var slider = (Slider)At.GetField(__instance, "m_sldFoVSplit");
				if (slider)
					slider.maxValue = 90f;
			}
        }

		[HarmonyPatch(typeof(SplitScreenManager), "Update")]
		public class SplitScreenManager_Update
        {
			[HarmonyPrefix]
			public static void Prefix(SplitScreenManager __instance)
            {
				if (Input.GetKey(KeyCode.Home))
                {
					if (Input.GetKeyUp(KeyCode.H))
					{
						CONFIG.SetValue(Settings.moveGlobalUiToPlayer1.ToString(), false);
						CONFIG.SetValue(Settings.scaleFactor.ToString(), 0.9f);
						__instance.CurrentSplitType = SplitScreenManager.SplitType.Vertical;
						m_refreshWanted = true;
					}
					else if (Input.GetKeyUp(KeyCode.V))
					{
						CONFIG.SetValue(Settings.moveGlobalUiToPlayer1.ToString(), true);
						CONFIG.SetValue(Settings.scaleFactor.ToString(), 1.0f);
						__instance.CurrentSplitType = SplitScreenManager.SplitType.Horizontal;
						m_refreshWanted = true;
					}
					else if (Input.GetKeyUp(KeyCode.M))
                    {
						CONFIG.SetValue(Settings.moveGlobalUiToPlayer1.ToString(), !(bool)CONFIG.GetValue(Settings.moveGlobalUiToPlayer1.ToString()));
						m_refreshWanted = true;
					}
				}

				if (lastScreenHeight != Screen.height || lastScreenWidth != Screen.width)
                {
					m_refreshWanted = true;
					lastScreenHeight = Screen.height;
					lastScreenWidth = Screen.width;
                }

				if (m_refreshWanted)
				{
					m_refreshWanted = false;
					__instance.ForceRefreshRatio = true;
				}
			}
        }

		[HarmonyPatch(typeof(SplitScreenManager), "DelayedRefreshSplitScreen")]
		public class SplitScreenManager_DelayedRefreshSplitScreen
        {
			[HarmonyPrefix]
			public static bool Prefix(SplitScreenManager __instance, DictionaryExt<int, SplitPlayer> ___m_localCharacterUIs)
            {
				if (__instance.CurrentSplitType == SplitScreenManager.SplitType.Horizontal)
				{
					//Orig_DelayedRefresh(__instance, ___m_localCharacterUIs);
					return true;
				}
				else
                {
					RefreshVertical(__instance, ___m_localCharacterUIs);
                }

				return false;
            }

			[HarmonyPostfix]
			public static void Postfix(SplitScreenManager __instance)
            {
				SL.Log("DelayedRefresh Postfix");

				if (__instance.CurrentSplitType == SplitScreenManager.SplitType.Horizontal)
                {
					RefreshHorizontal(__instance);
				}

				CanvasScaler[] componentsInChildren = MenuManager.Instance.GetComponentsInChildren<CanvasScaler>();
				for (int i = 0; i < componentsInChildren.Length; i++)
					componentsInChildren[i].matchWidthOrHeight = ((Screen.height > Screen.width) ? 0f : 1f);

				Canvas[] componentsInChildren2 = MenuManager.Instance.GetComponentsInChildren<Canvas>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
					componentsInChildren2[j].scaleFactor = (float)CONFIG.GetValue(Settings.scaleFactor.ToString());
			}
        }
		
		//public static void Orig_DelayedRefresh(SplitScreenManager self, DictionaryExt<int, SplitPlayer> localUIs)
  //      {
		//	for (int i = 0; i < localUIs.Count; i++)
		//	{
		//		SplitPlayer splitPlayer = localUIs.Values[i];
		//		float foV = 0f;
		//		Vector3 default_OFFSET = CharacterCamera.DEFAULT_OFFSET;
		//		Vector2 zero = Vector2.zero;
		//		Vector2 zero2 = Vector2.zero;
		//		Rect splitRect = new Rect(0f, 0f, 0f, 0f);
		//		RawImage rawImage = self.RenderInImage ? GameDisplayInUI.Instance.Screens[i] : null;
		//		if (localUIs.Count == 1)
		//		{
		//			splitRect.position = Vector2.zero;
		//			splitRect.size = Vector2.one;
		//			foV = OptionManager.Instance.GetFoVSolo(i);
		//			if (self.RenderInImage)
		//			{
		//				rawImage.rectTransform.localScale = Vector3.one;
		//				GameDisplayInUI.Instance.Screens[1].gameObject.SetActive(false);
		//			}
		//			GameDisplayInUI.Instance.SetMultiDisplayActive(false);
		//		}
		//		else if (localUIs.Count == 2)
		//		{
		//			if (self.CurrentSplitType == SplitScreenManager.SplitType.Horizontal)
		//			{
		//				if (self.RenderInImage)
		//				{
		//					splitRect.position = ((i == 0) ? new Vector2(0.5f, 0.5f) : Vector2.zero);
		//					splitRect.size = new Vector2(1f, 0.5f);
		//				}
		//				else
		//				{
		//					splitRect.position = new Vector2(0f, 0.5f * (float)((i == 0) ? 1 : -1));
		//					splitRect.size = Vector2.one;
		//				}
		//				foV = OptionManager.Instance.GetFoVSplit(i);
		//				default_OFFSET.z = -5f;
		//				default_OFFSET.y = 0.6f;
		//				zero2.y = -0.5f;
		//				zero.y = (float)((i % 2 == 1) ? 1 : -1) * 0.5f;
		//				GameDisplayInUI.Instance.SetMultiDisplayActive(false);
		//				if (self.RenderInImage)
		//				{
		//					rawImage.rectTransform.localScale = new Vector3(1f, 0.5f, 1f);
		//				}
		//			}
		//			else if (self.CurrentSplitType == SplitScreenManager.SplitType.Vertical)
		//			{
		//				if (self.RenderInImage)
		//				{
		//					splitRect.position = ((i == 0) ? Vector2.zero : new Vector2(0.5f, 0f));
		//					splitRect.size = new Vector2(0.5f, 1f);
		//				}
		//				else
		//				{
		//					splitRect.position = new Vector2(0.5f * (float)((i == 0) ? -1 : 1), 0f);
		//					splitRect.size = Vector2.one;
		//				}
		//				foV = self.VSplitFoV;
		//				default_OFFSET.z = -2.5f;
		//				zero2.x = -0.5f;
		//				zero.x = (float)((i % 2 == 1) ? 1 : -1) * 0.5f;
		//			}
		//			else if (self.CurrentSplitType == SplitScreenManager.SplitType.MultiDisplay)
		//			{
		//				splitRect.position = Vector2.zero;
		//				splitRect.size = Vector2.one;
		//				foV = OptionManager.Instance.GetFoVSolo(i);
		//				if (self.RenderInImage)
		//				{
		//					rawImage.rectTransform.localScale = Vector3.one;
		//				}
		//				GameDisplayInUI.Instance.SetMultiDisplayActive(true);
		//			}
		//			if (self.RenderInImage)
		//			{
		//				GameDisplayInUI.Instance.Screens[1].gameObject.SetActive(true);
		//			}
		//		}
		//		else
		//		{
		//			splitRect.size = new Vector2(0.5f, 0.5f);
		//			Vector2 zero3 = Vector2.zero;
		//			zero3.x = ((i == 0 || i == 2) ? 0f : 0.5f);
		//			zero3.y = ((i < 2) ? 0.5f : 0f);
		//			splitRect.position = zero3;
		//			zero2.y = -0.5f;
		//			zero.y = (float)((i % 2 == 1) ? 1 : -1) * 0.5f;
		//			zero2.x = -0.5f;
		//			zero.x = (float)((i % 2 == 1) ? 1 : -1) * 0.5f;
		//		}
		//		CameraSettings settings;
		//		settings.FoV = foV;
		//		settings.SplitRect = splitRect;
		//		settings.Offset = default_OFFSET;
		//		settings.CameraDepth = 2 * i;
		//		settings.Image = rawImage;
		//		splitPlayer.RefreshSplitScreen(zero, zero2, settings);
		//	}
		//}

		public static void RefreshHorizontal(SplitScreenManager self)
		{
			if ((bool)CONFIG.GetValue(Settings.moveGlobalUiToPlayer1.ToString()))
            {
				var zero = Vector2.zero;
				var zero2 = Vector2.zero;
				if (self.LocalPlayers.Count == 2)
				{
					zero2.y = -0.5f;
					zero.y = -0.5f;
				}

				var vector = Vector2.Scale(zero2, MenuManager.Instance.ScreenSize);
				var anchoredPosition = Vector2.Scale(zero, vector);

				if (At.GetField(MenuManager.Instance, "m_masterLoading") is LoadingFade loadingFade)
				{
					if (loadingFade.GetComponentInChildren<RectTransform>() is RectTransform loadingRect)
					{
						loadingRect.sizeDelta = vector;
						loadingRect.anchoredPosition = anchoredPosition;
					}
				}

				if (At.GetField(MenuManager.Instance, "m_prologueScreen") is ProloguePanel prologuePanel)
				{
					var rectTransform = prologuePanel.RectTransform;
					rectTransform.sizeDelta = vector;
					rectTransform.anchoredPosition = anchoredPosition;
				}
			}
		}

		public static void RefreshVertical(SplitScreenManager self, DictionaryExt<int, SplitPlayer> localUIs)
		{
			if (GameDisplayInUI.Instance.gameObject.activeSelf != self.RenderInImage)
				GameDisplayInUI.Instance.gameObject.SetActive(self.RenderInImage);

			for (int i = 0; i < localUIs.Count; i++)
			{
				SplitPlayer splitPlayer = localUIs.Values[i];
				Vector3 default_OFFSET = CharacterCamera.DEFAULT_OFFSET;
				Vector2 zero = Vector2.zero;
				Vector2 zero2 = Vector2.zero;
				Rect splitRect = new Rect(0f, 0f, 0f, 0f);
				RawImage rawImage = (!self.RenderInImage) ? null : GameDisplayInUI.Instance.Screens[i];
				float foV;
				if (localUIs.Count == 1)
				{
					splitRect.position = Vector2.zero;
					splitRect.size = Vector2.one;
					foV = OptionManager.Instance.GetFoVSolo(i);
					if (self.RenderInImage)
					{
						rawImage.rectTransform.localScale = Vector3.one;
						GameDisplayInUI.Instance.Screens[1].gameObject.SetActive(false);
					}
					GameDisplayInUI.Instance.SetMultiDisplayActive(false);
				}
				else
				{
					if (localUIs.Count != 2)
						throw new NotImplementedException("Support for more than 2 players is not implemented.");

					int num = i + 1;
					if (self.RenderInImage)
					{
						splitRect.position = ((i != 0) ? new Vector2(0.5f, 0f) : Vector2.zero);
						splitRect.size = new Vector2(0.5f, 1f);
					}
					else
					{
						splitRect.position = new Vector2(0.5f * (float)((i != 0) ? 1 : -1), 0f);
						splitRect.size = Vector2.one;
					}

					foV = OptionManager.Instance.GetFoVSplit(i);
					default_OFFSET.z = -2.5f;
					zero2.x = -0.5f;
					zero.x = (float)((num % 2 != 1) ? -1 : 1) * 0.5f;

					if (self.RenderInImage)
						GameDisplayInUI.Instance.Screens[1].gameObject.SetActive(true);
				}

				CameraSettings settings;
				settings.FoV = foV;
				settings.SplitRect = splitRect;
				settings.Offset = default_OFFSET;
				settings.CameraDepth = 2 * i;
				settings.Image = rawImage;
				splitPlayer.RefreshSplitScreen(zero, zero2, settings);
			}

			if ((bool)CONFIG.GetValue(Settings.moveGlobalUiToPlayer1.ToString()))
			{
				Vector2 zero3 = Vector2.zero;
				Vector2 zero4 = Vector2.zero;
				if (self.LocalPlayers.Count == 2)
				{
					zero4.x = -0.5f;
					zero3.x = 0.5f;
				}

				Vector2 vector = Vector2.Scale(zero4, MenuManager.Instance.ScreenSize);
				Vector2 anchoredPosition = Vector2.Scale(zero3, vector);

				if (At.GetField(MenuManager.Instance, "m_masterLoading") is LoadingFade loadingFade)
				{
					RectTransform componentInChildren = loadingFade.GetComponentInChildren<RectTransform>();
					bool flag7 = componentInChildren != null;
					if (flag7)
					{
						componentInChildren.sizeDelta = vector;
						componentInChildren.anchoredPosition = anchoredPosition;
					}
				}

				if (At.GetField(MenuManager.Instance, "m_prologueScreen") is ProloguePanel prologuePanel)
				{
					RectTransform rectTransform = prologuePanel.RectTransform;
					rectTransform.sizeDelta = vector;
					rectTransform.anchoredPosition = anchoredPosition;
				}
			}
		}

	}
}
