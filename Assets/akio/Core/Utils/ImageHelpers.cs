using UnityEngine;

public static class ImageHelpers
{
	public static Texture2D AlphaBlend(this Texture2D aBottom, Texture2D aTop)
	{

		if (aBottom.width != aTop.width || aBottom.height != aTop.height)
			throw new System.InvalidOperationException("AlphaBlend only works with two equal sized images");
		var bData = aBottom.GetPixels();
		var tData = aTop.GetPixels();
		int count = bData.Length;
		var rData = new Color[count];
		for(int i = 0; i < count; i++)
		{
			Color B = bData[i];
			Color T = tData[i];
			float srcF = T.a;
			float destF = 1f - T.a;
			float alpha = srcF + destF * B.a;
			Color R = (T * srcF + B * B.a * destF)/alpha;
			R.a = alpha;
			rData[i] = R;
		}
		var res = new Texture2D(aTop.width, aTop.height);
		res.SetPixels(rData);
		res.Apply();
		return res;
	}

	public static Texture2D JoinTextures(Texture2D[] textures)
	{
		Texture2D newTexture = new Texture2D(textures[0].width * 2,textures[0].height * 2);
		Color32[] pixels = new Color32[newTexture.width*newTexture.height];
		//xOff + (int)offset.x + (int)width * (zOff + (int)offset.y);

		for (int i = 0; i < 4; i++) {
		
			switch (i) {
			case 0:
				newTexture = AddPixelsToPixels (textures[i], new Vector2 (256, 0), newTexture);
				break;
			case 1 :
				newTexture = AddPixelsToPixels (textures[i], new Vector2 (0, 0), newTexture);
				break;
			case 2 :
				newTexture = AddPixelsToPixels (textures[i], new Vector2 (0, 256), newTexture);
				break;
			case 3 :
				newTexture = AddPixelsToPixels (textures[i], new Vector2 (256, 256), newTexture);
				break;
			}
		}


		newTexture.wrapMode = TextureWrapMode.Clamp;
		newTexture.Apply ();
		return newTexture;
	}

	private static Texture2D AddPixelsToPixels (Texture2D source, Vector2 offset, Texture2D texture) {

		for (int i = 0; i < source.width; i++) {
			for (int j = 0; j < source.height; j++) {

				int x = i + (int)offset.x;
				int z = j + (int)offset.y;
				texture.SetPixel (x, z, source.GetPixel(i,j));
			}
		}

		return texture;
	}

	private static Texture2D AddColorToPixels (Color32 color, Vector2 offset, Texture2D texture) {

		for (int i = 0; i < 256; i++) {
			for (int j = 0; j < 256; j++) {
				texture.SetPixel (i+(int)offset.x, j+(int)offset.y, color);
			}
		}

		return texture;
	}

}