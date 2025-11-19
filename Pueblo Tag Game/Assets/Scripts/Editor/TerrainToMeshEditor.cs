using UnityEngine;
using UnityEditor;
using System.IO;

public class TerrainToMeshEditor : MonoBehaviour
{
    [MenuItem("Tools/Pueblo Tag/Bake Terrain to Mesh")]
    public static void ConvertTerrain()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            EditorUtility.DisplayDialog("Error", "No Active Terrain found in the scene!", "OK");
            return;
        }

        TerrainData data = terrain.terrainData;

        // ---------------------------------------------------------
        // 1. BAKE THE TEXTURE (Splatmap Blending)
        // ---------------------------------------------------------
        int texResolution = 1024*16; // 1024x1024 is a good balance of quality vs file size
        Texture2D bakedTexture = new Texture2D(texResolution, texResolution, TextureFormat.RGBA32, false);
        
        // Get the blending data (alphas) from the terrain
        // format: [z, x, layerIndex]
        float[,,] splatMapData = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);
        int alphaWidth = data.alphamapWidth;
        int alphaHeight = data.alphamapHeight;

        // Prepare layer textures (ensure they are readable)
        TerrainLayer[] layers = data.terrainLayers;
        Texture2D[] layerTextures = new Texture2D[layers.Length];
        Vector2[] layerTilings = new Vector2[layers.Length];

        for (int i = 0; i < layers.Length; i++)
        {
            layerTextures[i] = layers[i].diffuseTexture;
            layerTilings[i] = layers[i].tileSize;

            // IMPORTANT: Ensure texture is readable so we can grab pixels
            SetTextureReadable(layerTextures[i]);
        }

        // Loop through every pixel of our new texture
        for (int y = 0; y < texResolution; y++)
        {
            for (int x = 0; x < texResolution; x++)
            {
                // Normalized coordinates (0.0 to 1.0)
                float normX = (float)x / (texResolution - 1);
                float normY = (float)y / (texResolution - 1);

                // Calculate position in the splatmap array
                int mapX = Mathf.RoundToInt(normX * (alphaWidth - 1));
                int mapY = Mathf.RoundToInt(normY * (alphaHeight - 1));

                Color finalColor = Color.black;

                // Blend layers based on weights
                for (int i = 0; i < layers.Length; i++)
                {
                    float weight = splatMapData[mapY, mapX, i];
                    
                    // Optimization: Don't sample if weight is effectively zero
                    if (weight < 0.01f) continue;

                    // Calculate UV for tiling textures
                    // World size covered by this pixel
                    float worldPosX = normX * data.size.x;
                    float worldPosY = normY * data.size.z;

                    // Sample the source texture
                    float u = (worldPosX % layerTilings[i].x) / layerTilings[i].x;
                    float v = (worldPosY % layerTilings[i].y) / layerTilings[i].y;
                    
                    Color layerColor = layerTextures[i].GetPixelBilinear(u, v);
                    
                    finalColor += layerColor * weight;
                }
                
                bakedTexture.SetPixel(x, y, finalColor);
            }
        }
        bakedTexture.Apply();

        // Save Texture to Disk
        byte[] bytes = bakedTexture.EncodeToPNG();
        string folderPath = "Assets/Art/Models/Environment";
        if (!AssetDatabase.IsValidFolder(folderPath)) Directory.CreateDirectory(folderPath); // Basic check
        string texPath = folderPath + "/BakedTerrainTexture.png";
        File.WriteAllBytes(texPath, bytes);
        AssetDatabase.Refresh(); // Refresh so Unity sees the new file


        // ---------------------------------------------------------
        // 2. CREATE MESH
        // ---------------------------------------------------------
        int meshRes = 128; // Geometry resolution
        Vector3[] vertices = new Vector3[meshRes * meshRes];
        int[] triangles = new int[(meshRes - 1) * (meshRes - 1) * 6];
        Vector2[] uvs = new Vector2[meshRes * meshRes];

        for (int z = 0; z < meshRes; z++)
        {
            for (int x = 0; x < meshRes; x++)
            {
                float xNorm = (float)x / (meshRes - 1);
                float zNorm = (float)z / (meshRes - 1);
                float height = data.GetHeight(Mathf.RoundToInt(xNorm * data.heightmapResolution), Mathf.RoundToInt(zNorm * data.heightmapResolution));
                
                vertices[z * meshRes + x] = new Vector3(xNorm * data.size.x, height, zNorm * data.size.z);
                uvs[z * meshRes + x] = new Vector2(xNorm, zNorm);
            }
        }

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
        mesh.name = "BakedTerrainMesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        string meshPath = folderPath + "/GeneratedTerrain.asset";
        AssetDatabase.CreateAsset(mesh, meshPath);

        // ---------------------------------------------------------
        // 3. CREATE MATERIAL
        // ---------------------------------------------------------
        string matPath = folderPath + "/GeneratedTerrainMaterial.mat";
        Material bakedMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        bakedMat.name = "BakedTerrainMaterial";
        
        // Load the texture we just saved as a proper Asset
        Texture2D loadedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        bakedMat.SetTexture("_BaseMap", loadedTex);
        bakedMat.SetFloat("_Smoothness", 0.0f); // Terrains are usually matte
        
        AssetDatabase.CreateAsset(bakedMat, matPath);
        AssetDatabase.SaveAssets();


        // ---------------------------------------------------------
        // 4. INSTANTIATE GAMEOBJECT
        // ---------------------------------------------------------
        string objName = "Terrain_Visuals_Baked";
        GameObject existing = GameObject.Find(objName);
        if (existing) DestroyImmediate(existing);

        GameObject go = new GameObject(objName);
        go.transform.position = terrain.transform.position;
        go.AddComponent<MeshFilter>().mesh = mesh;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = bakedMat;
        go.AddComponent<MeshCollider>().sharedMesh = mesh;

        Selection.activeGameObject = go;
        Debug.Log($"Terrain Baked! Texture saved to {texPath}");
    }

    // Helper to ensure we can read pixel data from textures
    static void SetTextureReadable(Texture2D tex)
    {
        if (tex == null) return;
        string path = AssetDatabase.GetAssetPath(tex);
        if (string.IsNullOrEmpty(path)) return;

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
            Debug.Log($"Made texture readable: {path}");
        }
    }
}