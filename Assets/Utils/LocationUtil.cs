using UnityEngine;

namespace BlobSimulation.Utils
{
    public static class LocationUtil
    {
        public static Vector3 GetRandomSpot(float spawnRadius)
        {
            Vector2 random2DPoint = Random.insideUnitCircle;
            Vector3 randomSpawnPosition = new Vector3(random2DPoint.x * spawnRadius, 0.18f, random2DPoint.y * spawnRadius);
            return randomSpawnPosition;
        }
    }
}
