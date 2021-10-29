using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.EditorHelpers {
	public static class SuperFormsStyles {
#if UNITY_EDITOR
		private const string EditorAssetPath = "Assets/Boomerang2DFramework/Framework/EditorAssets/";
		public const int Spacing = 10;
		private static RectOffset RowElementMargin => new RectOffset(5, 5, 5, 5);

		public enum B2DEditorTextures {
			TitleBackground,
			SelectionBarBackground,
			MainBackground,
			IconAdd,
			IconAddHover,
			DropDown,
			DropDownHover,
			DropDownLarge,
			DropDownLargeHover,
			IconSave,
			IconSaveAlert,
			IconSaveHover,
			IconDelete,
			IconDeleteHover,
			IconRename,
			IconRenameHover,
			IconEdit,
			IconEditHover,
			IconEye,
			IconEyeHover,
			IconEyeClosed,
			IconEyeClosedHover,
			IconRefresh,
			IconRefreshHover,
			IconClone,
			IconCloneHover,
			IconArrowUp,
			IconArrowUpHover,
			IconArrowDown,
			IconArrowDownHover,
			IconLock,
			IconLockHover,
			IconUnlock,
			IconUnlockHover,
			IconRepeat,
			IconRepeatHover,
			BoxBackground,
			PlateBoxBackground,
			PlateBoxBackgroundHover,
			PlateBoxBackgroundSelected,
			InputStyle,
			ScrollContainer,
			ButtonBackground,
			ButtonBackgroundActive,
			BoxHeaderBackground,
			SubBoxBackground,
			BoundingBox,
			Rays,
			Views,
			ViewsFill,
			Regions,
			RegionsFill,
			TransparentTile,
			GridBackground,
			Transparent,
			BrushModeAtlas,
			MapEditorIconDimensions,
			MapEditorIconDimensionsHover,
			MapEditorIconTiles,
			MapEditorIconTilesHover,
			MapEditorIconActors,
			MapEditorIconActorsHover,
			MapEditorIconPrefabs,
			MapEditorIconPrefabsHover,
			MapEditorIconViews,
			MapEditorIconViewsHover,
			MapEditorIconRegions,
			MapEditorIconRegionsHover,
			MapEditorPrefabPreview
		}

		public static readonly Dictionary<B2DEditorTextures, Texture2D> GuiTextures =
			new Dictionary<B2DEditorTextures, Texture2D> {
				{B2DEditorTextures.TitleBackground, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "colorAlmostBlack.png")},
				{B2DEditorTextures.SelectionBarBackground, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "colorMainBar.png")},
				{B2DEditorTextures.MainBackground, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "colorMainBackground.png")},
				{B2DEditorTextures.BoundingBox, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "colorBoundingBox.png")},
				{B2DEditorTextures.Rays, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "colorRays.png")},
				{B2DEditorTextures.Views, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "colorViews.png")},
				{B2DEditorTextures.ViewsFill, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "colorViewsFill.png")},
				{B2DEditorTextures.Regions, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "colorRegions.png")},
				{B2DEditorTextures.RegionsFill, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "colorRegionsFill.png")},
				{B2DEditorTextures.IconAdd, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconAdd.png")},
				{B2DEditorTextures.IconAddHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconAddHover.png")},
				{B2DEditorTextures.DropDownLarge, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "dropdownLarge.png")},
				{B2DEditorTextures.DropDownLargeHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "dropdownLargeHover.png")},
				{B2DEditorTextures.DropDown, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "dropdown.png")},
				{B2DEditorTextures.DropDownHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "dropdownHover.png")},
				{B2DEditorTextures.IconRename, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconRename.png")},
				{B2DEditorTextures.IconRenameHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconRenameHover.png")},
				{B2DEditorTextures.IconEdit, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconEdit.png")},
				{B2DEditorTextures.IconEditHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconEditHover.png")},
				{B2DEditorTextures.IconSave, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconSave.png")},
				{B2DEditorTextures.IconSaveAlert, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconSaveAlert.png")},
				{B2DEditorTextures.IconSaveHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconSaveHover.png")},
				{B2DEditorTextures.IconDelete, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconDelete.png")},
				{B2DEditorTextures.IconDeleteHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconDeleteHover.png")},
				{B2DEditorTextures.IconEye, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconEye.png")},
				{B2DEditorTextures.IconEyeHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconEyeHover.png")},
				{B2DEditorTextures.IconEyeClosed, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconEyeClosed.png")},
				{B2DEditorTextures.IconEyeClosedHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconEyeClosedHover.png")},
				{B2DEditorTextures.IconRefresh, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconRefresh.png")},
				{B2DEditorTextures.IconRefreshHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconRefreshHover.png")},
				{B2DEditorTextures.IconClone, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconClone.png")},
				{B2DEditorTextures.IconCloneHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconCloneHover.png")},
				{B2DEditorTextures.IconArrowUp, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconArrowUp.png")},
				{B2DEditorTextures.IconArrowUpHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconArrowUpHover.png")},
				{B2DEditorTextures.IconArrowDown, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconArrowDown.png")},
				{B2DEditorTextures.IconArrowDownHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconArrowDownHover.png")},
				{B2DEditorTextures.IconLock, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconLock.png")},
				{B2DEditorTextures.IconLockHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconLockHover.png")},
				{B2DEditorTextures.IconUnlock, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconUnlock.png")},
				{B2DEditorTextures.IconUnlockHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "iconUnlockHover.png")},
				{B2DEditorTextures.IconRepeat, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "IconRepeat.png")},
				{B2DEditorTextures.IconRepeatHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "IconRepeatHover.png")},
				{B2DEditorTextures.MapEditorIconDimensions, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "mapEditorIconDimensions.png")}, {
					B2DEditorTextures.MapEditorIconDimensionsHover,
					AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "mapEditorIconDimensionsHover.png")
				},
				{B2DEditorTextures.MapEditorIconTiles, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "mapEditorIconTiles.png")},
				{B2DEditorTextures.MapEditorIconTilesHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "mapEditorIconTilesHover.png")},
				{B2DEditorTextures.MapEditorIconActors, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "mapEditorIconActors.png")},
				{B2DEditorTextures.MapEditorIconActorsHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "mapEditorIconActorsHover.png")},
				{B2DEditorTextures.MapEditorIconPrefabs, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "mapEditorIconPrefabs.png")},
				{B2DEditorTextures.MapEditorIconPrefabsHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "mapEditorIconPrefabsHover.png")},
				{B2DEditorTextures.MapEditorIconViews, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "mapEditorIconViews.png")},
				{B2DEditorTextures.MapEditorIconViewsHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "mapEditorIconViewsHover.png")},
				{B2DEditorTextures.MapEditorIconRegions, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "mapEditorIconRegions.png")},
				{B2DEditorTextures.MapEditorIconRegionsHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "mapEditorIconRegionsHover.png")},
				{B2DEditorTextures.MapEditorPrefabPreview, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "mapEditorPrefabPreview.png")},
				{B2DEditorTextures.BoxHeaderBackground, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "boxHeaderBackground.png")},
				{B2DEditorTextures.BoxBackground, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "boxBackground.png")},
				{B2DEditorTextures.PlateBoxBackground, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "plateBoxBackground.png")},
				{B2DEditorTextures.PlateBoxBackgroundHover, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "plateBoxBackgroundHovered.png")},
				{B2DEditorTextures.PlateBoxBackgroundSelected, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "plateBoxBackgroundSelected.png")},
				{B2DEditorTextures.SubBoxBackground, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "subBoxBackground.png")},
				{B2DEditorTextures.InputStyle, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "inputBackground.png")},
				{B2DEditorTextures.ScrollContainer, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "scrollContainer.png")},
				{B2DEditorTextures.ButtonBackground, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "buttonBackground.png")},
				{B2DEditorTextures.ButtonBackgroundActive, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "buttonBackgroundActive.png")},
				{B2DEditorTextures.TransparentTile, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "transparentBackground.png")},
				{B2DEditorTextures.GridBackground, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "InputBackground")},
				{B2DEditorTextures.Transparent, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "transparentPixels.png")},
				{B2DEditorTextures.BrushModeAtlas, AssetDatabase.LoadAssetAtPath<Texture2D>(EditorAssetPath + "tilesetStudioBrushModeAtlas.png")}
			};

		private static readonly Font EditorFont = AssetDatabase.LoadAssetAtPath<Font>(EditorAssetPath + "contm.ttf");

		private enum B2DEditorColors {
			MainColor,
			MainColorDark,
			HoverColor,
			HeaderColor
		}

		private static readonly Dictionary<B2DEditorColors, Color> GuiColors = new Dictionary<B2DEditorColors, Color> {
			{B2DEditorColors.MainColor, new Color(195 / 255f, 197 / 255f, 204 / 255f, 1)},
			{B2DEditorColors.MainColorDark, new Color(20 / 255f, 23 / 255f, 25 / 255f, 1)},
			{B2DEditorColors.HoverColor, new Color(255 / 255f, 204 / 255f, 128 / 255f, 1)},
			{B2DEditorColors.HeaderColor, new Color(195 / 255f, 197 / 255f, 204 / 255f, 1)}
		};

		public static readonly GUIStyle TitleStyle = new GUIStyle {
			fontSize = 28,
			fontStyle = FontStyle.Bold,
			font = EditorFont,
			fixedHeight = 50,
			alignment = TextAnchor.MiddleCenter,
			normal = {
				textColor = Color.white,
				background = GuiTextures[B2DEditorTextures.TitleBackground]
			}
		};

		public static readonly GUIStyle MainOptionBar = new GUIStyle {
			fontSize = 28,
			fontStyle = FontStyle.Bold,
			font = EditorFont,
			fixedHeight = 31,
			alignment = TextAnchor.MiddleCenter,
			normal = {
				background = GuiTextures[B2DEditorTextures.SelectionBarBackground]
			}
		};

		public static readonly GUIStyle MainAreaStyle = new GUIStyle {
			normal = {
				background = GuiTextures[B2DEditorTextures.MainBackground]
			}
		};

		public static readonly GUIStyle ButtonAdd = new GUIStyle {
			fixedHeight = 16,
			fixedWidth = 16,
			margin = RowElementMargin,
			normal = {
				background = GuiTextures[B2DEditorTextures.IconAdd]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconAddHover]
			}
		};

		public static readonly GUIStyle ButtonAddLarge = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.IconAdd]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconAddHover]
			}
		};

		public static readonly GUIStyle ButtonRenameLarge = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.IconRename]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconRenameHover]
			}
		};

		public static readonly GUIStyle ButtonRename = new GUIStyle {
			fixedHeight = 16,
			fixedWidth = 16,
			margin = RowElementMargin,
			normal = {
				background = GuiTextures[B2DEditorTextures.IconRename]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconRenameHover]
			}
		};

		public static readonly GUIStyle ButtonEdit = new GUIStyle {
			fixedHeight = 16,
			fixedWidth = 16,
			margin = RowElementMargin,
			normal = {
				background = GuiTextures[B2DEditorTextures.IconEdit]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconEditHover]
			}
		};

		public static readonly GUIStyle DropdownLarge = new GUIStyle {
			fixedHeight = 30,
			margin = new RectOffset(5, 10, 0, 0),
			border = new RectOffset(1, 30, 1, 1),
			padding = new RectOffset(5, 30, 0, 0),
			alignment = TextAnchor.MiddleLeft,
			fontSize = 16,
			font = EditorFont,
			clipping = TextClipping.Clip,
			normal = {
				background = GuiTextures[B2DEditorTextures.DropDownLarge],
				textColor = GuiColors[B2DEditorColors.MainColor]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.DropDownLargeHover],
				textColor = GuiColors[B2DEditorColors.HoverColor]
			}
		};

		public static readonly GUIStyle Dropdown = new GUIStyle {
			fixedHeight = 16,
			margin = RowElementMargin,
			border = new RectOffset(1, 30, 1, 1),
			padding = new RectOffset(5, 14, 0, 0),
			alignment = TextAnchor.MiddleLeft,
			clipping = TextClipping.Clip,
			normal = {
				background = GuiTextures[B2DEditorTextures.DropDown],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.DropDownHover],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			}
		};

		public static readonly GUIStyle ButtonSaveLarge = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.IconSave]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconSaveHover]
			}
		};

		public static readonly GUIStyle ButtonSaveAlertLarge = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.IconSaveAlert]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconSaveHover]
			}
		};

		public static readonly GUIStyle ButtonDelete = new GUIStyle {
			fixedHeight = 16,
			fixedWidth = 16,
			margin = RowElementMargin,
			normal = {
				background = GuiTextures[B2DEditorTextures.IconDelete]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconDeleteHover]
			}
		};

		public static readonly GUIStyle ButtonDeleteLarge = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.IconDelete]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconDeleteHover]
			}
		};

		public static readonly GUIStyle ButtonEye = new GUIStyle {
			fixedHeight = 16,
			fixedWidth = 16,
			margin = RowElementMargin,
			normal = {
				background = GuiTextures[B2DEditorTextures.IconEye]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconEyeHover]
			}
		};

		public static readonly GUIStyle ButtonEyeLarge = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.IconEye]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconEyeHover]
			}
		};

		public static readonly GUIStyle ButtonEyeClosed = new GUIStyle {
			fixedHeight = 16,
			fixedWidth = 16,
			margin = RowElementMargin,
			normal = {
				background = GuiTextures[B2DEditorTextures.IconEyeClosed]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconEyeClosedHover]
			}
		};

		public static readonly GUIStyle ButtonEyeClosedLarge = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.IconEyeClosed]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconEyeClosedHover]
			}
		};

		public static readonly GUIStyle ButtonRefreshLarge = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.IconRefresh]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconRefreshHover]
			}
		};

		public static readonly GUIStyle ButtonClone = new GUIStyle {
			fixedHeight = 16,
			fixedWidth = 16,
			margin = RowElementMargin,
			normal = {
				background = GuiTextures[B2DEditorTextures.IconClone]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconCloneHover]
			}
		};

		public static readonly GUIStyle ButtonCloneLarge = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.IconClone]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconCloneHover]
			}
		};

		public static readonly GUIStyle ButtonArrowUp = new GUIStyle {
			fixedHeight = 16,
			fixedWidth = 16,
			margin = RowElementMargin,
			normal = {
				background = GuiTextures[B2DEditorTextures.IconArrowUp]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconArrowUpHover]
			}
		};

		public static readonly GUIStyle ButtonArrowDown = new GUIStyle {
			fixedHeight = 16,
			fixedWidth = 16,
			margin = RowElementMargin,
			normal = {
				background = GuiTextures[B2DEditorTextures.IconArrowDown]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconArrowDownHover]
			}
		};

		public static readonly GUIStyle ButtonLock = new GUIStyle {
			fixedHeight = 16,
			fixedWidth = 16,
			margin = RowElementMargin,
			normal = {
				background = GuiTextures[B2DEditorTextures.IconLock]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconLockHover]
			}
		};

		public static readonly GUIStyle ButtonUnlock = new GUIStyle {
			fixedHeight = 16,
			fixedWidth = 16,
			margin = RowElementMargin,
			normal = {
				background = GuiTextures[B2DEditorTextures.IconUnlock]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.IconUnlockHover]
			}
		};

		public static readonly GUIStyle MapEditorDimensions = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconDimensions]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconDimensionsHover]
			}
		};

		public static readonly GUIStyle MapEditorDimensionsSelected = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconDimensionsHover]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconDimensionsHover]
			}
		};

		public static readonly GUIStyle MapEditorTiles = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconTiles]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconTilesHover]
			}
		};

		public static readonly GUIStyle MapEditorTilesSelected = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconTilesHover]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconTilesHover]
			}
		};

		public static readonly GUIStyle MapEditorActors = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconActors]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconActorsHover]
			}
		};

		public static readonly GUIStyle MapEditorActorsSelected = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconActorsHover]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconActorsHover]
			}
		};

		public static readonly GUIStyle MapEditorPrefabs = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconPrefabs]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconPrefabsHover]
			}
		};

		public static readonly GUIStyle MapEditorPrefabsSelected = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconPrefabsHover]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconPrefabsHover]
			}
		};

		public static readonly GUIStyle MapEditorViews = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconViews]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconViewsHover]
			}
		};

		public static readonly GUIStyle MapEditorViewsSelected = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconViewsHover]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconViewsHover]
			}
		};

		public static readonly GUIStyle MapEditorRegions = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconRegions]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconRegionsHover]
			}
		};

		public static readonly GUIStyle MapEditorRegionsSelected = new GUIStyle {
			fixedHeight = 30,
			fixedWidth = 30,
			margin = new RectOffset(0, 5, 0, 0),
			normal = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconRegionsHover]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.MapEditorIconRegionsHover]
			}
		};

		public static readonly GUIStyle PaddedArea = new GUIStyle {
			padding = new RectOffset(10, 10, 0, 0)
		};

		public static readonly GUIStyle BoxStyle = new GUIStyle {
			border = new RectOffset(1, 1, 1, 1),
			margin = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(0, 0, 0, 5),
			normal = {
				textColor = GuiColors[B2DEditorColors.MainColor],
				background = GuiTextures[B2DEditorTextures.BoxBackground]
			}
		};

		public static readonly GUIStyle PaddedBoxStyle = new GUIStyle {
			border = new RectOffset(1, 1, 1, 1),
			margin = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(5, 5, 5, 5),
			normal = {
				textColor = GuiColors[B2DEditorColors.MainColor],
				background = GuiTextures[B2DEditorTextures.BoxBackground]
			}
		};

		public static readonly GUIStyle SubBoxStyle = new GUIStyle {
			border = new RectOffset(4, 1, 1, 1),
			margin = new RectOffset(5, 5, 5, 5),
			padding = new RectOffset(10, 0, 5, 5),
			normal = {
				textColor = GuiColors[B2DEditorColors.MainColor],
				background = GuiTextures[B2DEditorTextures.SubBoxBackground]
			}
		};

		public static readonly GUIStyle IndentedBox = new GUIStyle {
			border = new RectOffset(4, 1, 1, 1),
			margin = new RectOffset(5, 5, 0, 0),
			padding = new RectOffset(10, 0, 5, 5),
			normal = {
				textColor = GuiColors[B2DEditorColors.MainColor]
			}
		};

		public static readonly GUIStyle BoxedLabel = new GUIStyle {
			fixedWidth = 140f,
			fixedHeight = 16f,
			alignment = TextAnchor.MiddleLeft,
			padding = new RectOffset(5, 5, 0, 0),
			margin = RowElementMargin,
			clipping = TextClipping.Clip,
			normal = {
				textColor = GuiColors[B2DEditorColors.MainColor]
			}
		};

		public static readonly GUIStyle BoxedLabelFull = new GUIStyle {
			fixedHeight = 16f,
			alignment = TextAnchor.MiddleLeft,
			padding = new RectOffset(5, 5, 0, 0),
			margin = RowElementMargin,
			clipping = TextClipping.Clip,
			normal = {
				textColor = GuiColors[B2DEditorColors.MainColor]
			}
		};

		public static readonly GUIStyle ParagraphLabel = new GUIStyle {
			alignment = TextAnchor.MiddleLeft,
			padding = new RectOffset(5, 5, 0, 0),
			margin = RowElementMargin,
			clipping = TextClipping.Clip,
			wordWrap = true,
			normal = {
				textColor = GuiColors[B2DEditorColors.MainColor]
			}
		};

		public static readonly GUIStyle Blank = new GUIStyle {
			padding = new RectOffset(0, 0, 0, 0),
			margin = new RectOffset(0, 0, 0, 0),
			border = new RectOffset(0, 0, 0, 0),
			clipping = TextClipping.Clip
		};

		public static readonly GUIStyle BoxHeader = new GUIStyle {
			fontSize = 16,
			font = EditorFont,
			padding = new RectOffset(5, 5, 5, 5),
			margin = new RectOffset(0, 0, 0, 0),
			border = new RectOffset(5, 5, 1, 1),
			alignment = TextAnchor.MiddleLeft,
			normal = {
				background = GuiTextures[B2DEditorTextures.BoxHeaderBackground],
				textColor = GuiColors[B2DEditorColors.HeaderColor]
			}
		};

		public static readonly GUIStyle BoxSubHeader = new GUIStyle {
			fontSize = 12,
			padding = new RectOffset(5, 5, 0, 0),
			margin = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			fontStyle = FontStyle.Bold,
			normal = {
				textColor = GuiColors[B2DEditorColors.HeaderColor]
			}
		};

		public static readonly GUIStyle PlateBoxStyle = new GUIStyle {
			border = new RectOffset(1, 1, 1, 1),
			margin = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(0, 0, 0, 5),
			normal = {
				textColor = GuiColors[B2DEditorColors.MainColor],
				background = GuiTextures[B2DEditorTextures.PlateBoxBackground]
			}
		};

		public static readonly GUIStyle PlateBoxHighlightedStyle = new GUIStyle {
			border = new RectOffset(1, 1, 1, 1),
			margin = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(0, 0, 0, 5),
			normal = {
				textColor = GuiColors[B2DEditorColors.MainColor],
				background = GuiTextures[B2DEditorTextures.PlateBoxBackgroundHover]
			}
		};

		public static readonly GUIStyle PlateBoxSelectedStyle = new GUIStyle {
			border = new RectOffset(1, 1, 1, 1),
			margin = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(0, 0, 0, 5),
			normal = {
				textColor = GuiColors[B2DEditorColors.MainColor],
				background = GuiTextures[B2DEditorTextures.PlateBoxBackgroundSelected]
			}
		};

		public static readonly GUIStyle InputStyle = new GUIStyle {
			fixedHeight = 16,
			fontSize = 12,
			margin = RowElementMargin,
			border = new RectOffset(3, 2, 3, 2),
			padding = new RectOffset(5, 5, 0, 0),
			alignment = TextAnchor.UpperLeft,
			clipping = TextClipping.Clip,

			normal = {
				background = GuiTextures[B2DEditorTextures.InputStyle],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			}
		};

		public static readonly GUIStyle TextBoxStyle = new GUIStyle {
			fontSize = 12,
			margin = RowElementMargin,
			border = new RectOffset(3, 2, 3, 2),
			padding = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.UpperLeft,
			clipping = TextClipping.Clip,
			imagePosition = ImagePosition.TextOnly,

			normal = {
				background = GuiTextures[B2DEditorTextures.InputStyle],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			}
		};

		public static readonly GUIStyle Checkbox = new GUIStyle {
			fixedHeight = 16,
			fixedWidth = 24,
			margin = RowElementMargin,
			alignment = TextAnchor.MiddleCenter
		};

		public static readonly GUIStyle Button = new GUIStyle {
			margin = RowElementMargin,
			border = new RectOffset(1, 1, 1, 1),
			padding = new RectOffset(5, 5, 0, 0),
			fixedHeight = 16,
			alignment = TextAnchor.MiddleCenter,
			normal = {
				background = GuiTextures[B2DEditorTextures.ButtonBackground],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			},
			active = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			}
		};

		public static readonly GUIStyle ButtonSelected = new GUIStyle {
			margin = RowElementMargin,
			border = new RectOffset(1, 1, 1, 1),
			padding = new RectOffset(5, 5, 0, 0),
			fixedHeight = 16,
			alignment = TextAnchor.MiddleCenter,
			normal = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			},
			active = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			}
		};

		public static readonly GUIStyle TinyButton = new GUIStyle {
			margin = RowElementMargin,
			border = new RectOffset(1, 1, 1, 1),
			padding = new RectOffset(5, 5, 0, 0),
			fixedHeight = 16f,
			fixedWidth = 16f,
			alignment = TextAnchor.MiddleCenter,
			normal = {
				background = GuiTextures[B2DEditorTextures.ButtonBackground],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			},
			active = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			}
		};

		public static readonly GUIStyle TinyButtonSelected = new GUIStyle {
			margin = RowElementMargin,
			border = new RectOffset(1, 1, 1, 1),
			padding = new RectOffset(5, 5, 0, 0),
			fixedHeight = 16f,
			alignment = TextAnchor.MiddleCenter,
			normal = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			},
			active = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			}
		};

		public static readonly GUIStyle SelectionGrid = new GUIStyle {
			margin = new RectOffset(0, 0, 0, 10),
			border = new RectOffset(1, 1, 1, 1),
			padding = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleCenter,
			fontStyle = FontStyle.Bold,
			normal = {
				background = GuiTextures[B2DEditorTextures.BoxHeaderBackground],
				textColor = GuiColors[B2DEditorColors.MainColor]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.BoxHeaderBackground],
				textColor = GuiColors[B2DEditorColors.MainColor]
			},
			active = {
				background = GuiTextures[B2DEditorTextures.BoxHeaderBackground],
				textColor = GuiColors[B2DEditorColors.MainColor]
			},
			onNormal = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			},
			onHover = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			},
			onActive = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			}
		};

		public static readonly GUIStyle VerticalSelectionGrid = new GUIStyle {
			margin = new RectOffset(0, 0, 0, 0),
			border = new RectOffset(1, 1, 1, 1),
			padding = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			fontStyle = FontStyle.Bold,
			normal = {
				background = GuiTextures[B2DEditorTextures.BoxHeaderBackground],
				textColor = GuiColors[B2DEditorColors.MainColor]
			},
			hover = {
				background = GuiTextures[B2DEditorTextures.BoxHeaderBackground],
				textColor = GuiColors[B2DEditorColors.MainColor]
			},
			active = {
				background = GuiTextures[B2DEditorTextures.BoxHeaderBackground],
				textColor = GuiColors[B2DEditorColors.MainColor]
			},
			onNormal = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			},
			onHover = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			},
			onActive = {
				background = GuiTextures[B2DEditorTextures.ButtonBackgroundActive],
				textColor = GuiColors[B2DEditorColors.MainColorDark]
			}
		};
#endif
	}
}