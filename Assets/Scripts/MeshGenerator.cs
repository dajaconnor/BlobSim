using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve meshHeightCurve, int levelOfDetail)
    {
        var width = heightMap.GetLength(0);
        var height = heightMap.GetLength(1);
        var topLeftX = (width - 1) / -2f;
        var topLeftZ = (height - 1) / 2f;

        var simplificationIncrement = levelOfDetail <= 0 ? 1 : levelOfDetail * 2;
        var verticesPerLine = (width - 1) / simplificationIncrement + 1;

        var data = new MeshData(verticesPerLine, verticesPerLine);
        var vertexIndex = 0;

        for (int y = 0; y < height; y+= simplificationIncrement)
        {
            for (int x = 0; x < width; x += simplificationIncrement)
            {
                data.vertices[vertexIndex] = new Vector3(topLeftX + x, meshHeightCurve.Evaluate(heightMap[x,y]) * heightMultiplier, topLeftZ - y);
                data.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1)
                {
                    data.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    data.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + verticesPerLine + 1);
                }

                vertexIndex++;
            }
        }

        return data;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    public int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshHeight * meshWidth];
        uvs = new Vector2[meshHeight * meshWidth];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        var mesh = new Mesh {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };

        mesh.RecalculateNormals();

        return mesh;
    }
}