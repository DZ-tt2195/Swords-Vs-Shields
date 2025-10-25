using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.SceneManagement;
using MyBox;
using TMPro;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEngine.UI;
using System;
using System.Reflection;
using Photon.Pun;

[Serializable]
public class CardData
{
    public string cardName;
    public int startingBox;
    public CardAreas startingArea;
    public string artCredit;
    public Sprite sprite;
}

public class Translator : PhotonCompatible
{

#region Setup

    public static Translator inst;
    Dictionary<string, Dictionary<string, string>> keyTranslate = new();
    public List<CardData> playerCardFiles { get; private set; }

    string sheetURL = "1_lM8HoBF4oWhX33mzhJ8Cge4M0qnmuUyocGEP_0WjNQ";
    string apiKey = "AIzaSyCl_GqHd1-WROqf7i2YddE3zH6vSv3sNTA";
    string baseUrl = "https://sheets.googleapis.com/v4/spreadsheets/";
    [Scene][SerializeField] string toLoad;

    private void Awake()
    {
        //Debug.Log(string.Format($"hi {0}", "lol"));
        //Debug.Log(string.Format($"hi {{0}}", "lol"));

        if (inst == null)
        {
            inst = this;
            DontDestroyOnLoad(this.gameObject);
            Application.targetFrameRate = 60;

            if (!PlayerPrefs.HasKey("Language")) PlayerPrefs.SetString("Language", "English");
            TxtLanguages();
            StartCoroutine(DownloadLanguages());
            StartCoroutine(GetCardData());
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void TxtLanguages()
    {
        TextAsset[] languageFiles = Resources.LoadAll<TextAsset>("Txt Languages");
        foreach (TextAsset language in languageFiles)
        {
            (bool success, string converted) = ConvertTxtName(language);
            if (success)
            {
                Dictionary<string, string> newDictionary = new();
                keyTranslate.Add(converted, newDictionary);
                string[] lines = language.text.Split('\n');

                foreach (string line in lines)
                {
                    if (line != "")
                    {
                        string[] parts = line.Split('=');
                        newDictionary[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }
        }

        (bool, string) ConvertTxtName(TextAsset asset)
        {
            //pattern: "0. English"
            string pattern = @"^\d+\.\s*(.+)$";
            Match match = Regex.Match(asset.name, pattern);
            if (match.Success)
                return (true, match.Groups[1].Value);
            else
                return (false, "");
        }
    }

    #endregion

#region Downloading

    IEnumerator Download(string range)
    {
        if (Application.isEditor)
        {
            string url = $"{baseUrl}{sheetURL}/values/{range}?key={apiKey}";
            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Download failed: {www.error}");
            }
            else
            {
                string filePath = $"Assets/Resources/{range}.txt";
                File.WriteAllText($"{filePath}", www.downloadHandler.text);

                string[] allLines = File.ReadAllLines($"{filePath}");
                List<string> modifiedLines = allLines.ToList();
                modifiedLines.RemoveRange(1, 3);
                File.WriteAllLines($"{filePath}", modifiedLines.ToArray());
                //Debug.Log($"downloaded {range}");
            }
        }
    }

    IEnumerator DownloadLanguages()
    {
        yield return Download("Csv Languages");
        GetLanguages(ReadFile("Csv Languages"));

        string[][] ReadFile(string range)
        {
            TextAsset data = Resources.Load($"{range}") as TextAsset;

            string editData = data.text;
            editData = editData.Replace("],", "").Replace("{", "").Replace("}", "");

            string[] numLines = editData.Split("[");
            string[][] list = new string[numLines.Length][];

            for (int i = 0; i < numLines.Length; i++)
                list[i] = numLines[i].Split("\",");
            return list;
        }
        SceneManager.LoadScene(toLoad);
    }

    void GetLanguages(string[][] data)
    {
        for (int i = 1; i < data[1].Length; i++)
        {
            data[1][i] = data[1][i].Replace("\"", "").Trim();
            Dictionary<string, string> newDictionary = new();
            keyTranslate.Add(data[1][i], newDictionary);
        }

        List<(string, string)> listOfKeys = new();
        for (int i = 2; i < data.Length; i++)
        {
            for (int j = 0; j < data[i].Length; j++)
            {
                data[i][j] = data[i][j].Replace("\"", "").Replace("\\", "").Replace("]", "").Replace("|", "\n").Trim();
                if (j > 0)
                {
                    string language = data[1][j];
                    string key = data[i][0];
                    keyTranslate[language][key] = data[i][j];

                    if (j == 1)
                        listOfKeys.Add((data[i][0], data[i][1]));
                }
            }
        }
        KeywordTooltip.instance.SwitchLanguage();
        CreateBaseTxtFile(listOfKeys);
    }

    void CreateBaseTxtFile(List<(string, string)> listOfKeys)
    {
        if (Application.isEditor)
        {
            string baseText = "";
            foreach ((string key, string value) in listOfKeys)
                baseText += $"{key}={value}\n";

            string filePath = $"Assets/Resources/BaseTxtFile.txt";
            File.WriteAllText($"{filePath}", baseText);

            /*
            string filePath = Path.Combine(Application.persistentDataPath, "BaseTxtFile.txt");
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (string input in listOfKeys)
                    writer.WriteLine($"{input}=");
            }*/
        }
    }

    IEnumerator GetCardData()
    {
        CoroutineGroup group = new(this);
        group.StartCoroutine(Download("Player Cards"));

        while (group.AnyProcessing)
            yield return null;

        playerCardFiles = GetDataFiles<CardData>(ReadFile("Player Cards"));

        string[][] ReadFile(string range)
        {
            TextAsset data = Resources.Load($"{range}") as TextAsset;

            string editData = data.text;
            editData = editData.Replace("],", "").Replace("{", "").Replace("}", "");

            string[] numLines = editData.Split("[");
            string[][] list = new string[numLines.Length][];

            for (int i = 0; i < numLines.Length; i++)
            {
                list[i] = numLines[i].Split("\",");
            }
            return list;
        }
        List<T> GetDataFiles<T>(string[][] data) where T : new()
        {
            Dictionary<string, int> columnIndex = new();
            List<T> toReturn = new();

            for (int i = 0; i < data[1].Length; i++)
            {
                string nextLine = data[1][i].Trim().Replace("\"", "");
                if (!columnIndex.ContainsKey(nextLine))
                    columnIndex.Add(nextLine, i);
            }

            for (int i = 2; i < data.Length; i++)
            {
                for (int j = 0; j < data[i].Length; j++)
                    data[i][j] = data[i][j].Trim().Replace("\"", "").Replace("\\", "").Replace("]", "");

                if (data[i][0].IsNullOrEmpty())
                    continue;

                T nextData = new();
                toReturn.Add(nextData);

                foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (columnIndex.TryGetValue(field.Name, out int index))
                    {
                        try
                        {
                            string sheetValue = data[i][index];
                            if (field.FieldType == typeof(int))
                                field.SetValue(nextData, StringToInt(sheetValue));
                            else if (field.FieldType == typeof(bool))
                                field.SetValue(nextData, StringToBool(sheetValue));
                            else if (field.FieldType == typeof(string))
                                field.SetValue(nextData, sheetValue);
                            else if (field.FieldType == typeof(CardAreas))
                                field.SetValue(nextData, StringToArea(sheetValue));
                            else if (field.FieldType == typeof(Sprite))
                                field.SetValue(nextData, Resources.Load<Sprite>($"Card Art/{data[i][0]}"));
                        }
                        catch
                        {
                            Debug.LogError($"{field.Name}, {index}");
                        }
                    }
                }
            }

            return toReturn;
        }
    }

    int StringToInt(string line)
    {
        line = line.Trim();
        try
        {
            return (line.Equals("")) ? -1 : int.Parse(line);
        }
        catch (FormatException)
        {
            return -1;
        }
    }

    CardAreas StringToArea(string line)
    {
        line = line.Trim();
        try
        {
            return (CardAreas)Enum.Parse(typeof(CardAreas), line);
        }
        catch (FormatException)
        {
            return CardAreas.City;
        }
    }

    bool StringToBool(string line)
    {
        line = line.Trim();
        return line == "TRUE";
    }

    #endregion

#region Helpers

    bool TranslationExists(string key)
    {
        return keyTranslate["English"].ContainsKey(key);
    }

    public string SplitAndTranslate(int owner, string logText, int indent = 0)
    {
        string targetText = "";
        for (int i = 0; i < indent; i++)
            targetText += "     ";

        string[] splitUp = logText.Split('-');
        List<(string, string)> toTranslate = new();

        for (int i = 1; i < splitUp.Length; i += 2)
        {
            string first = splitUp[i];
            string second = splitUp[i + 1];
            if (first.Equals("Card") || first.Equals("Area"))
                second = Translate(splitUp[i + 1]);
            toTranslate.Add((first, second));
        }

        if (TranslationExists($"{splitUp[0]} Others") && PhotonNetwork.LocalPlayer.ActorNumber != owner)
            targetText += Translate($"{splitUp[0]} Others", toTranslate);
        else
            targetText += Translate($"{splitUp[0]}", toTranslate);
        return KeywordTooltip.instance.EditText(targetText);
    }

    public string Translate(string key, List<(string, string)> toReplace = null)
    {
        if (key == "" || int.TryParse(key, out _))
            return key;

        string answer;
        try
        {
            answer = keyTranslate[PlayerPrefs.GetString("Language")][key];
        }
        catch
        {
            try
            {
                answer = keyTranslate[("English")][key];
                Debug.Log($"{key} failed to translate in {PlayerPrefs.GetString("Language")}");
            }
            catch
            {
                Debug.Log($"{key} failed to translate at all");
                return key;
            }
        }

        if (toReplace != null)
        {
            foreach ((string one, string two) in toReplace)
                answer = answer.Replace($"${one}$", two);
        }
        return answer;
    }

    public Dictionary<string, Dictionary<string, string>> GetTranslations()
    {
        return keyTranslate;
    }

    public void ChangeLanguage(string newLanguage)
    {
        if (!PlayerPrefs.GetString("Language").Equals(newLanguage))
        {
            PlayerPrefs.SetString("Language", newLanguage);
            KeywordTooltip.instance.SwitchLanguage();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    #endregion

}
