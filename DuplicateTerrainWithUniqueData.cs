// Assets/Editor/DuplicateTerrainWithUniqueData.cs
using UnityEngine;
using UnityEditor;
using System.IO;

public class DuplicateTerrainWithUniqueData
{
    [MenuItem("Tools/Duplicate Terrain (Unique Data)")]
    static void DuplicateSelectedTerrain()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("Terrain seçili deðil.");
            return;
        }

        Terrain srcTerrain = Selection.activeGameObject.GetComponent<Terrain>();
        if (srcTerrain == null)
        {
            Debug.LogWarning("Seçili GameObject bir Terrain deðil.");
            return;
        }

        TerrainData srcData = srcTerrain.terrainData;
        if (srcData == null)
        {
            Debug.LogWarning("Kaynak TerrainData bulunamadý.");
            return;
        }

        // Yeni TerrainData oluþtur (kopya)
        TerrainData newData = Object.Instantiate(srcData);
        string folder = "Assets/ClonedTerrains";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "ClonedTerrains");

        string assetPath = $"{folder}/TerrainData_clone_{System.DateTime.Now.ToString("yyyyMMdd_HHmmss")}.asset";
        AssetDatabase.CreateAsset(newData, assetPath);
        AssetDatabase.SaveAssets();

        // Yeni Terrain GameObject oluþtur
        GameObject newTerrainGO = Terrain.CreateTerrainGameObject(newData);
        newTerrainGO.name = srcTerrain.gameObject.name + "_Clone";

        // Pozisyonu biraz saða kaydýr (terrain geniþliði kadar)
        Vector3 offset = new Vector3(srcData.size.x + 2f, 0f, 0f);
        newTerrainGO.transform.position = srcTerrain.transform.position + offset;

        // Kopyalanan veriler zaten newData içeriyor (height, alphamap, details, trees)
        // Ýsterseniz burada yeni aðaçlarý ekleyebilirsiniz (aþaðýdaki örneðe bakýn)

        Selection.activeGameObject = newTerrainGO;
        EditorUtility.DisplayDialog("Duplicate Terrain", "Terrain kopyalandý ve yeni TerrainData asset olarak oluþturuldu:\n" + assetPath, "Tamam");
    }
}
