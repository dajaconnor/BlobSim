using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDistance = 30;
    private Transform viewer;

    int chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleChunks = new List<TerrainChunk>();

    void Start()
    {
        viewer = FindObjectOfType<CameraBehavior>().transform;
        chunkSize = MapGenerator.mapChunkVertices - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt( maxViewDistance - chunkSize );
    }

    void Update()
    {
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        foreach(var chunk in visibleChunks)
        {
            chunk.SetVisible(false);
        }

        visibleChunks.Clear();
        int currentChunkCoordX = Mathf.RoundToInt(viewer.position.x - chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewer.position.z - chunkSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                var viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    var chunk = terrainChunkDictionary[viewedChunkCoord];

                    chunk.UpdateTerrainChunk();
                    if (chunk.IsVisible())
                    {
                        visibleChunks.Add(chunk);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform));
                }
            }
        }
            
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        private Transform viewer;

        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            viewer = FindObjectOfType<CameraBehavior>().transform;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            var positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;

            SetVisible(false);
        }

        public void UpdateTerrainChunk() {
            var viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewer.position));
            var isVisible = viewerDistanceFromNearestEdge <= maxViewDistance;
        }

        public void SetVisible(bool isVisible)
        {
            meshObject.SetActive(isVisible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
}
