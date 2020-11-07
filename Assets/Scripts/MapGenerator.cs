using Assets.Utils;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, Mesh };
    public DrawMode drawMode;

    public const int mapChunkVertices = 241;
    [Range(0,6)]
    public int levelOfDetail;
    public float scale;

    public int octaves;
    [Range(0,1)]
    public float persistence;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float heighMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrainType[] regions;
    public float[,] heightMap;

    public MapData GenerateMap()
    {
        heightMap = NoiseUtil.MakeNoise(mapChunkVertices, mapChunkVertices, seed, scale, octaves, persistence, lacunarity, offset);

        var colorMap = DrawMapInEditor();

        return new MapData(heightMap, colorMap);
    }

    public Color[] DrawMapInEditor()
    {
        var display = FindObjectOfType<MapDisplay>();
        Color[] colorMap = new Color[0];

        if (drawMode == DrawMode.NoiseMap) display.DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
        else if (drawMode == DrawMode.ColorMap)
        {
            colorMap = GenerateColorMap(heightMap);
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkVertices, mapChunkVertices));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            colorMap = GenerateColorMap(heightMap);
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap, heighMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, mapChunkVertices, mapChunkVertices));
        }

        return colorMap;
    }

    private Color[] GenerateColorMap(float[,] map)
    {
        var colorMap = new Color[mapChunkVertices * mapChunkVertices];

        for (int y = 0; y < mapChunkVertices; y++)
        {
            for (int x = 0; x < mapChunkVertices; x++)
            {
                var currentHeight = map[x, y];

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapChunkVertices + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        return colorMap;
    }

    void OnValidate()
    {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public float[,] heightMap;
    public Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}