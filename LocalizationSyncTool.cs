using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Networking;
using UnityEditor.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class LocalizationSyncTool : EditorWindow
{
    private string webAppUrl = ""; // Kullanýcýnýn yapýþtýracaðý URL
    private StringTableCollection targetCollection;

    // --- DURUM BÝLDÝRÝM DEÐÝÞKENLERÝ ---
    private string statusMessage = "";
    private MessageType statusType = MessageType.None;
    // ------------------------------------

    private string PrefsKey => $"LocSyncUrl_{Application.productName}";

    [MenuItem("Tools/Localization Sync/Web App Sync")]
    public static void ShowWindow()
    {
        GetWindow<LocalizationSyncTool>("Loc Sync Tool");
    }

    private void OnEnable()
    {
        webAppUrl = EditorPrefs.GetString(PrefsKey, "");
    }

    private void OnGUI()
    {
        GUILayout.Label("Google Sheets Sync (No DLLs)", EditorStyles.boldLabel);
        GUILayout.Space(10);

        targetCollection = (StringTableCollection)EditorGUILayout.ObjectField("Table Collection", targetCollection, typeof(StringTableCollection), false);
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();

        webAppUrl = EditorGUILayout.TextField("Web App URL", webAppUrl);

        if (GUILayout.Button(EditorGUIUtility.IconContent("_Help"), GUILayout.Width(25), GUILayout.Height(20)))
        {
            LocalizationHelpWindow.ShowWindow();
        }

        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString(PrefsKey, webAppUrl);
        }

        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Full Push -> (Unity to Sheet)"))
        {
            if (CheckSettings()) EditorCoroutineUtility.StartCoroutine(PushToSheet(), this);
        }

        if (GUILayout.Button("Full Pull <- (Sheet to Unity)"))
        {
            if (CheckSettings()) EditorCoroutineUtility.StartCoroutine(PullFromSheet(), this);
        }
        GUILayout.EndHorizontal();

        // --- DURUM PANELÝ (Mesaj varsa göster) ---
        GUILayout.Space(15);
        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.HelpBox(statusMessage, statusType);

            // Mesajý temizlemek için küçük bir buton (Opsiyonel ama kullanýþlý)
            if (GUILayout.Button("Clear Log / Temizle", GUILayout.Width(120)))
            {
                statusMessage = "";
                statusType = MessageType.None;
            }
        }
    }

    // --- YARDIMCI FONKSÝYON: UI GÜNCELLEME ---
    private void SetStatus(string message, MessageType type)
    {
        statusMessage = message;
        statusType = type;
        Repaint(); // UI'ý anýnda yenile ki mesaj görünsün
    }

    private bool CheckSettings()
    {
        if (targetCollection == null || string.IsNullOrEmpty(webAppUrl))
        {
            // Popup yerine Panelde hata gösterelim
            SetStatus("Hata: Table Collection ve Web App URL zorunludur.", MessageType.Error);
            return false;
        }
        // Ýþlem baþladýðýnda eski mesajý temizle
        SetStatus("Ýþlem baþlatýlýyor...", MessageType.Info);
        return true;
    }

    // ================= PUSH (UNITY -> SHEET) =================
    private IEnumerator PushToSheet()
    {
        EditorUtility.DisplayProgressBar("Sync", "Veriler Hazýrlanýyor...", 0.1f);

        var sharedData = targetCollection.SharedData;
        var locales = LocalizationEditorSettings.GetLocales();

        List<List<string>> rows = new List<List<string>>();
        List<string> headers = new List<string> { "Key", "Id" };
        List<StringTable> tables = new List<StringTable>();

        foreach (var locale in locales)
        {
            string displayName = locale.Identifier.CultureInfo != null ? locale.Identifier.CultureInfo.DisplayName : locale.LocaleName;
            headers.Add($"{displayName} ({locale.Identifier.Code})");
            tables.Add(targetCollection.GetTable(locale.Identifier) as StringTable);
        }
        rows.Add(headers);

        foreach (var entry in sharedData.Entries)
        {
            List<string> row = new List<string>();
            row.Add(entry.Key);
            row.Add(entry.Id.ToString());

            foreach (var table in tables)
            {
                string value = "";
                if (table != null)
                {
                    var tableEntry = table.GetEntry(entry.Id);
                    if (tableEntry != null) value = tableEntry.LocalizedValue;
                }
                row.Add(value);
            }
            rows.Add(row);
        }

        SheetPayload payload = new SheetPayload();
        payload.action = "push";
        payload.rows = rows;

        string json = SimpleJsonBuilder(rows);

        var request = new UnityWebRequest(webAppUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        EditorUtility.DisplayProgressBar("Sync", "Google'a Gönderiliyor...", 0.5f);
        yield return request.SendWebRequest();

        EditorUtility.ClearProgressBar();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // BAÞARI DURUMU -> PANEL
            SetStatus("Push Baþarýlý! Veriler Google Sheet'e yazýldý.", MessageType.Info);
        }
        else
        {
            // HATA DURUMU -> PANEL
            SetStatus("Baðlantý Hatasý: " + request.error, MessageType.Error);
        }
    }

    // ================= PULL (SHEET -> UNITY) =================
    private IEnumerator PullFromSheet()
    {
        EditorUtility.DisplayProgressBar("Sync", "Google'dan Ýndiriliyor...", 0.3f);

        UnityWebRequest request = UnityWebRequest.Get(webAppUrl);
        yield return request.SendWebRequest();

        EditorUtility.ClearProgressBar();

        if (request.result != UnityWebRequest.Result.Success)
        {
            SetStatus("Ýndirme Hatasý: " + request.error, MessageType.Error);
            yield break;
        }

        string json = request.downloadHandler.text;
        var data = ParseGoogleSheetJson(json);

        if (data == null || data.Count == 0)
        {
            // Hata mesajý Parse fonksiyonu içinden set edilmiþ olabilir, deðilse buradan set et.
            if (string.IsNullOrEmpty(statusMessage))
                SetStatus("Veri boþ veya okunamadý!", MessageType.Warning);

            yield break;
        }

        ProcessPullData(data);
    }

    private void ProcessPullData(List<List<string>> values)
    {
        var headerRow = values[0];
        Dictionary<int, Locale> columnLocaleMap = new Dictionary<int, Locale>();
        var allLocales = LocalizationEditorSettings.GetLocales();

        for (int i = 2; i < headerRow.Count; i++)
        {
            string header = headerRow[i];
            string code = "";

            int lastOpen = header.LastIndexOf('(');
            int lastClose = header.LastIndexOf(')');

            if (lastOpen != -1 && lastClose > lastOpen)
            {
                code = header.Substring(lastOpen + 1, lastClose - lastOpen - 1);
            }
            else
            {
                code = header;
            }

            var locale = allLocales.FirstOrDefault(x => x.Identifier.Code == code);
            if (locale != null)
            {
                columnLocaleMap.Add(i, locale);
            }
            // Bulunamayan dilleri loglamýyoruz, sadece panelde hata varsa gösteririz.
        }

        if (columnLocaleMap.Count == 0)
        {
            SetStatus("HATA: Hiçbir dil sütunu eþleþtirilemedi! Excel baþlýklarýný kontrol edin.", MessageType.Error);
            return;
        }

        var sharedData = targetCollection.SharedData;
        int updatedCount = 0;

        for (int r = 1; r < values.Count; r++)
        {
            var row = values[r];
            if (row.Count < 2) continue;

            string keyName = row[0];
            if (!long.TryParse(row[1], out long keyId)) continue;

            if (!sharedData.Contains(keyId))
            {
                sharedData.AddKey(keyName, keyId);
            }

            foreach (var kvp in columnLocaleMap)
            {
                if (kvp.Key < row.Count)
                {
                    string newValue = row[kvp.Key];
                    var locale = kvp.Value;

                    var stringTable = targetCollection.GetTable(locale.Identifier) as StringTable;
                    if (stringTable == null)
                    {
                        stringTable = targetCollection.AddNewTable(locale.Identifier) as StringTable;
                    }

                    stringTable.AddEntry(keyId, newValue);
                    EditorUtility.SetDirty(stringTable);
                }
            }
            updatedCount++;
        }

        EditorUtility.SetDirty(targetCollection);
        EditorUtility.SetDirty(sharedData);
        AssetDatabase.SaveAssets();

        // SONUÇ BÝLDÝRÝMÝ -> PANEL
        SetStatus($"Ýþlem Tamamlandý: {updatedCount} satýr güncellendi.", MessageType.Info);
    }

    private string SimpleJsonBuilder(List<List<string>> rows)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{\"action\":\"push\",\"rows\":[");
        for (int i = 0; i < rows.Count; i++)
        {
            sb.Append("[");
            for (int j = 0; j < rows[i].Count; j++)
            {
                string val = rows[i][j].Replace("\"", "\\\"").Replace("\n", "\\n");
                sb.Append($"\"{val}\"");
                if (j < rows[i].Count - 1) sb.Append(",");
            }
            sb.Append("]");
            if (i < rows.Count - 1) sb.Append(",");
        }
        sb.Append("]}");
        return sb.ToString();
    }

    private List<List<string>> ParseGoogleSheetJson(string json)
    {
        try
        {
            JObject root = JObject.Parse(json);

            // Apps Script hatasý kontrolü ("status": "error" dönerse)
            if (root["status"] != null && root["status"].ToString() == "error")
            {
                SetStatus("Google Apps Script Hatasý: " + root["message"], MessageType.Error);
                return null;
            }

            JArray dataArray = (JArray)root["data"];
            List<List<string>> result = new List<List<string>>();

            foreach (var row in dataArray)
            {
                List<string> rowList = new List<string>();
                foreach (var cell in row)
                {
                    rowList.Add(cell == null ? "" : cell.ToString());
                }
                result.Add(rowList);
            }
            return result;
        }
        catch (System.Exception e)
        {
            SetStatus("JSON Parse Hatasý: " + e.Message, MessageType.Error);
            return null;
        }
    }

    [System.Serializable]
    public class SheetPayload
    {
        public string action;
        public List<List<string>> rows;
    }
}