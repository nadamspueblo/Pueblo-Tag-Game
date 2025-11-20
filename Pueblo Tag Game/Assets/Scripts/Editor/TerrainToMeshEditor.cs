using UnityEngine;
using UnityEditor;

public class TerrainToMeshEditor : MonoBehaviour
{
    [MenuItem("Tools/Pueblo Tag/Bake Terrain for Physics (Invisible)")]
    public static void ConvertTerrain()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            EditorUtility.DisplayDialog("Error", "No Active Terrain found in the scene!", "OK");
            return;
        }

        TerrainData data = terrain.terrainData;
        
        // Resolution: 128 is usually sufficient for physics collision. 
        // Too high = slow physics. Too low = players float above ground.
        int meshRes = 128; 

        Vector3[] vertices = new Vector3[meshRes * meshRes];
        int[] triangles = new int[(meshRes - 1) * (meshRes - 1) * 6];
        Vector2[] uvs = new Vector2[meshRes * meshRes];

        for (int z = 0; z < meshRes; z++)
        {
            for (int x = 0; x < meshRes; x++)
            {
                // Normalized position (0.0 to 1.0)
                float normX = (float)x / (meshRes - 1);
                float normZ = (float)z / (meshRes - 1);

                // 1. Geometry
                float height = data.GetInterpolatedHeight(normX, normZ);
                vertices[z * meshRes + x] = new Vector3(normX * data.size.x, height, normZ * data.size.z);
                uvs[z * meshRes + x] = new Vector2(normX * data.size.x, normZ * data.size.z);
            }
        }

        // Build Triangles
        int triIndex = 0;
        for (int z = 0; z < meshRes - 1; z++)
        {
            for (int x = 0; x < meshRes - 1; x++)
            {
                triangles[triIndex++] = z * meshRes + x;
                triangles[triIndex++] = (z + 1) * meshRes + x;
                triangles[triIndex++] = z * meshRes + x + 1;
                triangles[triIndex++] = (z + 1) * meshRes + x;
                triangles[triIndex++] = (z + 1) * meshRes + x + 1;
                triangles[triIndex++] = z * meshRes + x + 1;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "TerrainPhysicsMesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Save Asset
        string folderPath = "Assets/Art/Models/Environment";
        if (!AssetDatabase.IsValidFolder(folderPath)) AssetDatabase.CreateFolder("Assets/Art/Models", "Environment");
        string meshPath = folderPath + "/GeneratedTerrainPhysics.asset";
        AssetDatabase.CreateAsset(mesh, meshPath);
        AssetDatabase.SaveAssets();

        // Create Game Object
        string objName = "Terrain_Physics_Baked";
        GameObject existing = GameObject.Find(objName);
        if (existing) DestroyImmediate(existing);

        GameObject go = new GameObject(objName);
        
        // Apply slight offset to prevent visual Z-fighting with the real terrain
        Vector3 pos = terrain.transform.position;
        pos.y -= 0.01f; 
        go.transform.position = pos;

        // Add Components
        go.AddComponent<MeshFilter>().mesh = mesh;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.enabled = false; // Hide visuals, physics only!
        
        // IMPORTANT: Assign a default material even if hidden to prevent internal errors
        mr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");

        go.AddComponent<MeshCollider>().sharedMesh = mesh;

        Debug.Log($"Terrain Physics Mesh Baked! Saved to {meshPath}. Renderer is disabled.");
        Selection.activeGameObject = go;
    }
}