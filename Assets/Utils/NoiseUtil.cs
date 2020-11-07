using UnityEngine;

namespace Assets.Utils
{
    public static class NoiseUtil
    {
        public static float[,] MakeNoise(int mapWidth, int mapDepth, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset)
        {
            var map = new float[mapWidth, mapDepth];

            var prng = new System.Random(seed);
            var octaveOffsets = new Vector2[octaves];

            for (int i = 0; i< octaves; i++)
            {
                var offsetX = prng.Next(-100000, 100000) + offset.x;
                var offsetY = prng.Next(-100000, 100000) + offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            if (scale <= 0) scale = 0.0001f;

            var maxNoiseHeight = float.MinValue;
            var minNoiseHeight = float.MaxValue;

            var halfWidth = mapWidth / 2;
            var halfDepth = mapDepth / 2;


            for (int y = 0; y < mapDepth; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++)
                    {
                        var sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;
                        var sampleY = (y-halfDepth) / scale * frequency + octaveOffsets[i].y;

                        // set to -1 - 1 range
                        var perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        map[x, y] = perlinValue;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    map[x, y] = noiseHeight;

                    if (maxNoiseHeight < noiseHeight) maxNoiseHeight = noiseHeight;
                    else if (minNoiseHeight > noiseHeight) minNoiseHeight = noiseHeight;
                }
            }

            for (int y = 0; y < mapDepth; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    map[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, map[x, y]);
                }
            }

            return map;
        }
    }
}
