using System;
using System.Linq;
using UnityEngine;

namespace BlobSimulation.Utils
{
    public static class LocationUtil
    {
        public static float groundHeight = 0.18f;
        private static MapGenerator mapGen = UnityEngine.Object.FindObjectOfType<MapGenerator>();

        public static Vector3 GetRandomSpot(float spawnRadius)
        {
            Vector2 random2DPoint = UnityEngine.Random.insideUnitCircle;
            Vector3 randomSpawnPosition = new Vector3(random2DPoint.x * spawnRadius, GetHeight(random2DPoint.x, random2DPoint.y) + groundHeight, random2DPoint.y * spawnRadius);
            return randomSpawnPosition;
        }

        public static Vector3 GetRandomSpotAroundPosition(float spawnRadius, Vector3 position)
        {
            Vector2 random2DPoint = UnityEngine.Random.insideUnitCircle;
            Vector3 randomSpawnPosition = new Vector3(random2DPoint.x * spawnRadius + position.x, GetHeight(random2DPoint.x, random2DPoint.y) + groundHeight, random2DPoint.y * spawnRadius + position.z);
            return randomSpawnPosition;
        }

        public static bool IsOnMap(Vector3 position)
        {
            var localScale = UnityEngine.Object.FindObjectsOfType<FruitSpawner>().First().transform.localScale;
            return new Vector2(position.x, position.z).magnitude < localScale.x * 0.48;
        }

        public static float GetDistance(Vector3 here, Vector3 there)
        {
            return (there - here).magnitude;
        }

        public static bool IsInShade(Vector2 position)
        {
            (var closestTree, var distance) = FindClosestTree(position);

            return closestTree != null && distance < closestTree.transform.localScale.x;
        }

        public static (TreeBehavior, float) FindClosestTree(Vector3 position)
        {
            var trees = UnityEngine.Object.FindObjectsOfType<TreeBehavior>();

            if (trees.Length == 0) return (null, 0f);

            TreeBehavior currentClosest = trees.First();
            var currentDistance = GetDistance(position, currentClosest.transform.position);

            for (int i = 1; i < trees.Length; i++)
            {
                var thisDistance = GetDistance(position, trees[i].transform.position);

                if (thisDistance < currentDistance)
                {
                    currentClosest = trees[i];
                    currentDistance = thisDistance;
                }
            }

            return (currentClosest, currentDistance);
        }

        public static float GetHeight(Vector3 position)
        {
            return GetHeight(position.x, position.z);
        }

        public static float GetHeight(float x, float y)
        {
            if (mapGen == null) mapGen = UnityEngine.Object.FindObjectOfType<MapGenerator>();
            if (mapGen == null || mapGen.heightMap == null) return 0;
 
            var width = mapGen.heightMap.GetLength(0);
            var depth = mapGen.heightMap.GetLength(1);

            var centerX = (width - 1) / 2;
            var centerY = (depth - 1) / 2;

            x /= mapGen.transform.localScale.x;
            y /= mapGen.transform.localScale.y;

            x += centerX;
            y += centerY;

            if (x >= width || x < 0 || y >= depth || y < 0) return 0;

            var leftx =(int)Math.Ceiling(x);
            var rightx = (int) x;
            var upy = (int)Math.Ceiling(y);
            var downy = (int)y;

            var point = new Vector2(x, y);
            var topLeft = mapGen.meshHeightCurve.Evaluate(mapGen.heightMap[leftx, upy]);
            var bottomRight = mapGen.meshHeightCurve.Evaluate(mapGen.heightMap[rightx, downy]);

            if (x % 1f + y % 1f < 1)
            {
                var bottomLeft = mapGen.meshHeightCurve.Evaluate(mapGen.heightMap[leftx, downy]);
                
                var height = (1 - (new Vector2(leftx, upy) - point).magnitude) * topLeft
                    + (1 - (new Vector2(leftx, downy) - point).magnitude) * bottomLeft
                    + (1 - (new Vector2(rightx, downy) - point).magnitude) * bottomRight;

                return AdjustByMultiplier(height);
            }

            else {
                var topRight = mapGen.meshHeightCurve.Evaluate(mapGen.heightMap[rightx, upy]);

                var height = (1 - (new Vector2(leftx, upy) - point).magnitude) * topLeft
                    + (1 - (new Vector2(rightx, upy) - point).magnitude) * topRight
                    + (1 - (new Vector2(rightx, downy) - point).magnitude) * bottomRight;

                return AdjustByMultiplier(height);
            }
        }

        private static float AdjustByMultiplier(float height)
        {
            return height * mapGen.heighMultiplier / 3;
        }
    }
}
