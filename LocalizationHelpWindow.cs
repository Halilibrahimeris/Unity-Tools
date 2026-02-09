using UnityEngine;
using UnityEditor;

public class LocalizationHelpWindow : EditorWindow
{
    private Vector2 mainScrollPos; // Ana pencere scroll'u
    private Vector2 codeScrollPos; // Kod kutusu scroll'u (YENÝ)
    private GUIStyle helpBoxStyle;
    public static void ShowWindow()
    {
        LocalizationHelpWindow win = GetWindow<LocalizationHelpWindow>(true, "Setup Guide / Kurulum", true);
        win.minSize = new Vector2(450, 650);
    }

    private void OnGUI()
    {
        // --- STÝL AYARLARI ---
        if (helpBoxStyle == null)
        {
            helpBoxStyle = new GUIStyle(EditorStyles.helpBox);
            helpBoxStyle.richText = true;
            helpBoxStyle.wordWrap = true;
            helpBoxStyle.padding = new RectOffset(10, 10, 10, 10);
            helpBoxStyle.fontSize = 12;
            helpBoxStyle.alignment = TextAnchor.UpperLeft;
        }

        // --- ÜST BAR (Google Sheets Butonu) ---
        GUILayout.Space(10);
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
        if (GUILayout.Button("Go to Google Sheets (Create New)", GUILayout.Height(40)))
        {
            Application.OpenURL("https://docs.google.com/spreadsheets/u/0/");
        }
        GUI.backgroundColor = Color.white;
        GUILayout.Space(10);

        // --- ANA ÝÇERÝK SCROLL BAÞLANGICI ---
        mainScrollPos = EditorGUILayout.BeginScrollView(mainScrollPos);

        // Ýngilizce Bölüm
        EditorGUILayout.LabelField("ENGLISH INSTRUCTIONS", EditorStyles.boldLabel);
        GUILayout.Label(
            "1. Click the green button above to open Google Sheets.\n" +
            "2. Create a new empty spreadsheet.\n" +
            "3. Go to <b>Extensions > Apps Script</b>.\n" +
            "4. Delete existing code and paste the <b>APPS SCRIPT CODE</b> below.\n" +
            "5. Click <b>Deploy > New Deployment</b>.\n" +
            "6. Select type: <b>Web App</b>.\n" +
            "7. Set 'Who has access' to: <b>Anyone</b> (Important!).\n" +
            "8. Copy the URL and paste it into the tool.",
            helpBoxStyle);

        GUILayout.Space(15);

        // Türkçe Bölüm
        EditorGUILayout.LabelField("TÜRKÇE KURULUM", EditorStyles.boldLabel);
        GUILayout.Label(
            "1. Yukarýdaki yeþil butona basarak Google Sheets'i açýn.\n" +
            "2. Yeni boþ bir tablo oluþturun.\n" +
            "3. Menüden <b>Uzantýlar > Apps Script</b> yolunu izleyin.\n" +
            "4. Oradaki kodlarý silin ve aþaðýdaki <b>APPS SCRIPT CODE</b> kýsmýný yapýþtýrýn.\n" +
            "5. <b>Daðýt (Deploy) > Yeni Daðýtým</b>'a týklayýn.\n" +
            "6. Tür olarak <b>Web Uygulamasý</b>'ný seçin.\n" +
            "7. 'Eriþimi olanlar' kýsmýný <b>Herkes (Anyone)</b> yapýn (Önemli!).\n" +
            "8. Verilen URL'yi kopyalayýp tool'a yapýþtýrýn.",
            helpBoxStyle);

        GUILayout.Space(20);

        // --- KOD BÖLÜMÜ ---
        EditorGUILayout.LabelField("APPS SCRIPT CODE (Copy & Paste)", EditorStyles.boldLabel);

        string code = GetAppsScriptCode();

        // >>> YENÝ EKLENEN KISIM: KOD KUTUSU ÝÇÝN SCROLL <<<
        // 300 pixel yükseklikte bir pencere açýyoruz
        codeScrollPos = EditorGUILayout.BeginScrollView(codeScrollPos, GUILayout.Height(300));

        // Ýçerideki TextArea'nýn yüksekliðini serbest býrakýyoruz (ExpandHeight)
        // Böylece scrollview içinde uzayabiliyor.
        EditorGUILayout.TextArea(code, GUILayout.ExpandHeight(true));

        EditorGUILayout.EndScrollView();
        // >>> BÝTÝÞ <<<

        GUILayout.Space(5);

        if (GUILayout.Button("Copy Code to Clipboard", GUILayout.Height(30)))
        {
            EditorGUIUtility.systemCopyBuffer = code;
            EditorUtility.DisplayDialog("Copied / Kopyalandý", "Code copied to clipboard!\nKod panoya kopyalandý!", "OK");
        }

        GUILayout.Space(20);

        // --- ANA ÝÇERÝK SCROLL BÝTÝÞÝ ---
        EditorGUILayout.EndScrollView();
    }

    private string GetAppsScriptCode()
    {
        return @"function doGet(e) {
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getSheets()[0]; // Ýlk sayfa
  var data = sheet.getDataRange().getValues();
  
  // Veriyi JSON objesine çevir
  var result = {
    ""status"": ""success"",
    ""data"": data
  };
  
  return ContentService.createTextOutput(JSON.stringify(result))
    .setMimeType(ContentService.MimeType.JSON);
}

function doPost(e) {
  try {
    var jsonString = e.postData.contents;
    var payload = JSON.parse(jsonString);
    var sheet = SpreadsheetApp.getActiveSpreadsheet().getSheets()[0];
    
    // Ýþlem 'push' ise sayfayý temizle ve yeniden yaz
    if (payload.action === ""push"") {
      sheet.clear();
      var rows = payload.rows;
      if (rows && rows.length > 0) {
        sheet.getRange(1, 1, rows.length, rows[0].length).setValues(rows);
      }
    }
    
    return ContentService.createTextOutput(JSON.stringify({""status"": ""success""}));
  } catch (error) {
    return ContentService.createTextOutput(JSON.stringify({""status"": ""error"", ""message"": error.toString()}));
  }
}";
    }
}