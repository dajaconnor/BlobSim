using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int depth)
    {
        var texture = new Texture2D(width, depth);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        var width = heightMap.GetLength(0);
        var depth = heightMap.GetLength(1);

        var colorMap = new Color[width * depth];

        for (int y = 0; y < depth; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        return TextureFromColorMap(colorMap, width, depth);
    }
}
