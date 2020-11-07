using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var mapGen = (MapGenerator) target;

        if (DrawDefaultInspector() && mapGen.autoUpdate)
        {
            mapGen.GenerateMap();
        }

        if (GUILayout.Button("Generate Map"))
        {
            mapGen.GenerateMap();
        }
    }
}
