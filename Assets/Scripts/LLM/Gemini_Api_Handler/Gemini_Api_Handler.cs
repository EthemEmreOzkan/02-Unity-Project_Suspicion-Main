using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class Gemini_Api_Handler : MonoBehaviour
{
    //*-----------------------------------------------------------------------------------------//
    #region Inspector Tab

    [Header("Gemini Ayarları -----------------------------------------------------------")]
    [Space]
    [SerializeField] private string model = "gemini-2.5-flash";
    [SerializeField] private string apiKey = "YOUR_GEMINI_API_KEY_HERE";
    [Space]
    [Header("Test Metni -----------------------------------------------------------------")]
    [Space]
    [SerializeField] private KeyCode Test_Tusu = KeyCode.W;
    [TextArea(3, 10)]
    [Space]
    [SerializeField] private string Test_Prompt;
    [Space]
    [Header("Yanıt ----------------------------------------------------------------------")]
    [Space]
    [TextArea(3, 10)]
    public string Last_Response = "";
    public bool Is_Response_Received = false;
    public bool Is_Request_In_Progress = false;

    #endregion
    //*-----------------------------------------------------------------------------------------//
    #region Unity Life Cycle
    void Update()
    {
        if (Input.GetKeyDown(Test_Tusu))
        {
            Send_Prompt(Test_Prompt);
        }
    }
    #endregion
    //*-----------------------------------------------------------------------------------------//
    #region Send_Prompt Public Func

    public void Send_Prompt(string prompt)
    {
        // API anahtarının ayarlanıp ayarlanmadığını kontrol edin
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
        {
            string error = "Hata: Lütfen Gemini API Anahtarınızı atayın!";
            Debug.LogError(error);
            Last_Response = error;
            Is_Response_Received = true;
            Is_Request_In_Progress = false;
            return;
        }

        if (Is_Request_In_Progress)
        {
            Debug.LogWarning("Uyarı: Önceki istek tamamlanmadan yeni bir istek gönderilemez.");
            return;
        }

        // Durum değişkenlerini sıfırla ve isteği başlat
        Is_Response_Received = false;
        Is_Request_In_Progress = true;
        Last_Response = "";

        StartCoroutine(Send_Prompt_To_Gemini(prompt));
    }

    #endregion
    //*-----------------------------------------------------------------------------------------//
    #region Private Funcs

    private IEnumerator Send_Prompt_To_Gemini(string prompt)
    {
        // API URL'si
        string url = $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key={apiKey}";

        // Gönderilecek JSON gövdesi (EscapeJson kullanılarak prompt güvenli hale getirildi)
        string jsonBody = "{\"contents\": [{\"parts\": [{\"text\": \"" + EscapeJson(prompt) + "\"}]}]}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($"Gemini'ye istek gönderiliyor ({model})...");

        // İsteği gönder ve yanıtı bekle
        yield return request.SendWebRequest();

        string result;

        if (request.result != UnityWebRequest.Result.Success)
        {
            // Hata durumunda
            result = $"API Hatası: {request.error}";
            Debug.LogError("Gemini API Hatası: " + request.error + "\nYanıt: " + request.downloadHandler.text);
        }
        else
        {
            // Başarılı yanıt durumunda
            string json = request.downloadHandler.text;
            result = ExtractTextFromGeminiResponse(json);
            Debug.Log("Gemini Yanıtı:\n" + result);
        }

        Last_Response = result;
        Is_Response_Received = true;
        Is_Request_In_Progress = false;
    }

    //*-----------------------------------------------------------------------------------------//

    private string EscapeJson(string s)
    {
        return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r");
    }

    //*-----------------------------------------------------------------------------------------//

    private string ExtractTextFromGeminiResponse(string json)
    {
        const string marker = "\"text\": \"";
        int start = json.IndexOf(marker);

        if (start == -1)
        {
            return $"Yanıt çözülemedi veya filtrelendi. Tam JSON: {json}";
        }

        start += marker.Length;

        int end = json.IndexOf("\"", start);

        if (end == -1) return "Yanıt çözülemedi.";

        string result = json.Substring(start, end - start);
        return result.Replace("\\n", "\n").Replace("\\\"", "\"");
    }

    #endregion
    //*-----------------------------------------------------------------------------------------//
}