using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using MyBox;
using System;
using System.Reflection;
using Photon.Pun;

[Serializable]
public class CardData
{
    public string cardName;
    public int startingHealth;
    public AbilityType typeOne;
    public AbilityType typeTwo;
    public string artCredit;
    public Sprite sprite;
}

public class Translator : PhotonCompatible
{

#region Setup

    public static Translator inst;
    Dictionary<string, Dictionary<string, string>> keyTranslate = new();
    public List<CardData> playerCardFiles { get; private set; }
    [Scene][SerializeField] string toLoad;

    protected override void Awake()
    {
        if (inst == null)
        {
            inst = this;
            DontDestroyOnLoad(this.gameObject);
            Application.targetFrameRate = 60;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("Language"))
            PlayerPrefs.SetString("Language", "English");

        TxtLanguages();
        CsvLanguages(ReadFile("Csv Languages"));
        playerCardFiles = GetCardData<CardData>(ReadFile("Player Cards"));
        KeywordTooltip.instance.SwitchLanguage();
        SceneManager.LoadScene(toLoad);
    }

    #endregion

#region Reading Files

    public static string[][] ReadFile(string range)
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

    void CsvLanguages(string[][] data)
    {
        for (int i = 1; i < data[1].Length; i++)
        {
            data[1][i] = data[1][i].Replace("\"", "").Trim();
            Dictionary<string, string> newDictionary = new();
            keyTranslate.Add(data[1][i], newDictionary);
        }

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
                }
            }
        }
    }

    List<T> GetCardData<T>(string[][] data) where T : new()
    {
        Dictionary<string, int> columnIndex = new();
        List<T> toReturn = new();

        for (int i = 0; i < data[1].Length; i++)
        {
            string nextLine = data[1][i].Trim().Replace("\"", "");
            if (!columnIndex.ContainsKey(nextLine))
            {
                columnIndex.Add(nextLine, i);
            }
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
                    string sheetValue = data[i][index];
                    if (field.FieldType == typeof(int))
                        field.SetValue(nextData, StringToInt(sheetValue));
                    else if (field.FieldType == typeof(bool))
                        field.SetValue(nextData, StringToBool(sheetValue));
                    else if (field.FieldType == typeof(string))
                        field.SetValue(nextData, sheetValue);
                    else if (field.FieldType == typeof(AbilityType))
                        field.SetValue(nextData, StringToAbilityType(sheetValue));

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

                    AbilityType StringToAbilityType(string line)
                    {
                        line = line.Trim();
                        try
                        {
                            return (AbilityType)Enum.Parse(typeof(AbilityType), line);
                        }
                        catch (FormatException)
                        {
                            return AbilityType.None;
                        }
                    }

                    bool StringToBool(string line)
                    {
                        line = line.Trim();
                        return line == "TRUE";
                    }
                }
                else
                {
                    if (field.FieldType == typeof(Sprite))
                        field.SetValue(nextData, Resources.Load<Sprite>($"Card Art/{data[i][0]}"));
                }
            }
        }

        return toReturn;
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

        if (TranslationExists($"{splitUp[0]} Others") && (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerProp.Position.ToString()] != owner)
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
                //Debug.Log($"{key} failed to translate in {PlayerPrefs.GetString("Language")}");
            }
            catch
            {
                //Debug.Log($"{key} failed to translate at all");
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
