using System;
using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.BitmapFonts {
	[Serializable]
	public class BitmapFontProperties {
		public string Name;
		public int GlyphHeight = 16;
		public List<BitmapFontGlyphProperties> Glyphs = new List<BitmapFontGlyphProperties>();
	}
}