using System;
using System.Linq;
using UnityEngine;

namespace UnitSimulation.Utils
{
    public static class LocationUtil
    {
        public static float groundHeight = 0.18f;

        public static Vector3 GetRandomSpot(float spawnRadius, MapGenerator map)
        {
            Vector2 random2DPoint = UnityEngine.Random.insideUnitCircle;
            Vector3 randomSpawnPosition = new Vector3(random2DPoint.x * spawnRadius, GetHeight(random2DPoint.x, random2DPoint.y, map) + groundHeight, random2DPoint.y * spawnRadius);
            return randomSpawnPosition;
        }

        public static Vector3 GetRandomSpotAroundPosition(float spawnRadius, Vector3 position, MapGenerator map)
        {
            Vector2 random2DPoint = UnityEngine.Random.insideUnitCircle;
            Vector3 randomSpawnPosition = new Vector3(random2DPoint.x * spawnRadius + position.x, GetHeight(random2DPoint.x, random2DPoint.y, map) + groundHeight, random2DPoint.y * spawnRadius + position.z);
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

        public static float GetHeight(Vector3 position, MapGenerator ground)
        {
            return GetHeight(position.x, position.z, ground);
        }

        public static float GetHeight(float x, float y, MapGenerator ground)
        {
            //return 0;

            if (ground == null) return 0;
            if (MeshGenerator.MeshHeightField == null) ground.GenerateMap();

            return ground.terrain.SampleHeight(new Vector3(x, 0, y));

            //x /= ground.scale;
            //y /= ground.scale;

            //var width = MeshGenerator.MeshHeightField.GetLength(0);
            //var depth = MeshGenerator.MeshHeightField.GetLength(1);

            //var centerX = (width - 1) / 2;
            //var centerY = (depth - 1) / 2;

            //x /= ground.transform.localScale.x;
            //y /= ground.transform.localScale.y;

            //x += centerX;
            //y += centerY;

            //if (x >= width || x < 0 || y >= depth || y < 0) return 0;

            //var leftx = (int)Math.Ceiling(x);
            //var rightx = (int)x;
            //var upy = (int)Math.Ceiling(y);
            //var downy = (int)y;

            //var point = new Vector2(x, y);

            //leftx = SetInRange(leftx, MeshGenerator.MeshHeightField.GetLength(0));
            //upy = SetInRange(upy, MeshGenerator.MeshHeightField.GetLength(1));
            //rightx = SetInRange(rightx, MeshGenerator.MeshHeightField.GetLength(0));
            //downy = SetInRange(downy, MeshGenerator.MeshHeightField.GetLength(1));

            //var topLeft = ground.meshHeightCurve.Evaluate(MeshGenerator.MeshHeightField[leftx, upy]);
            //var bottomRight = ground.meshHeightCurve.Evaluate(MeshGenerator.MeshHeightField[rightx, downy]);
            //var locationHeight = 0f;

            //if (x % 1f + y % 1f < 1)
            //{
            //    var bottomLeft = ground.meshHeightCurve.Evaluate(MeshGenerator.MeshHeightField[leftx, downy]);

            //    var height = (1 - (new Vector2(leftx, upy) - point).magnitude) * topLeft
            //        + (1 - (new Vector2(leftx, downy) - point).magnitude) * bottomLeft
            //        + (1 - (new Vector2(rightx, downy) - point).magnitude) * bottomRight;

            //    locationHeight = AdjustByMultiplier(height, ground);
            //}

            //else
            //{
            //    var topRight = ground.meshHeightCurve.Evaluate(MeshGenerator.MeshHeightField[rightx, upy]);

            //    var height = (1 - (new Vector2(leftx, upy) - point).magnitude) * topLeft
            //        + (1 - (new Vector2(rightx, upy) - point).magnitude) * topRight
            //        + (1 - (new Vector2(rightx, downy) - point).magnitude) * bottomRight;

            //    locationHeight = AdjustByMultiplier(height, ground);
            //}

            //return locationHeight;
        }

        private static int SetInRange(int leftx, int upperLimit)
        {
            if (leftx < 0) leftx = 0;
            if (leftx >= upperLimit) leftx = upperLimit - 1;
            return leftx;
        }

        private static float AdjustByMultiplier(float height, MapGenerator ground)
        {
            return height * ground.heightMultiplier * ground.heightMultiplier;
        }
    }
}
