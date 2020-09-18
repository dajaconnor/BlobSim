using System.Linq;
using UnityEngine;

namespace BlobSimulation.Utils
{
    public static class LocationUtil
    {
        public static float groundHeight = 0.18f;

        public static Vector3 GetRandomSpot(float spawnRadius)
        {
            Vector2 random2DPoint = Random.insideUnitCircle;
            Vector3 randomSpawnPosition = new Vector3(random2DPoint.x * spawnRadius, groundHeight, random2DPoint.y * spawnRadius);
            return randomSpawnPosition;
        }

        public static Vector3 GetRandomSpotAroundPosition(float spawnRadius, Vector3 position)
        {
            Vector2 random2DPoint = Random.insideUnitCircle;
            Vector3 randomSpawnPosition = new Vector3(random2DPoint.x * spawnRadius + position.x, groundHeight, random2DPoint.y * spawnRadius + position.z);
            return randomSpawnPosition;
        }

        public static bool IsOnMap(Vector3 position)
        {
            var localScale = Object.FindObjectsOfType<FruitSpawner>().First().transform.localScale;
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
            var trees = Object.FindObjectsOfType<TreeBehavior>();

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
    }
}
