using R2API;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;

namespace LordsItemEdits
{
    internal static class ModLanguage
    {
        internal static List<string> LangFilesToLoad = [];

        internal static void AddNewLangTokens()
        {
            // DOING R2API.LANGUAGE'S ADDOVERLAYPATH DOESN'T WORK SO WE'RE MANUALLY ADDING EACH LANG TOKEN OURSELVES !!!!!!!!!!!!!!!!!!!
            string rootLangFolderLocation = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Plugin.PluginInfo.Location), "Language");
            foreach (string itemName in LangFilesToLoad)
            {
                string itemLangFileLocation = System.IO.Path.Combine(rootLangFolderLocation, $"{itemName}.json");
                string langFileText = System.IO.File.ReadAllText(itemLangFileLocation);
                Dictionary<string, Dictionary<string, string>> langFileDict = LoadFile(langFileText);
                foreach (var langKV in langFileDict)
                {
                    string currentLang = langKV.Key;
                    foreach (var langTokenKV in langKV.Value)
                    {
                        LanguageAPI.AddOverlay(langTokenKV.Key, langTokenKV.Value, currentLang);
                    }
                }
            }
        }



        // WE'RE COPYING CODE IN THIS BITCHHHHHHH
        // this is from r2api.language
        private static Dictionary<string, Dictionary<string, string>>? LoadFile(string fileContent)
        {
            Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                JSONNode jsonNode = JSON.Parse(fileContent);
                if (jsonNode == null)
                {
                    return null;
                }

                var languages = jsonNode.Keys;
                foreach (var language in languages)
                {
                    JSONNode languageTokens = jsonNode[language];
                    if (languageTokens == null)
                    {
                        continue;
                    }

                    var languagename = language;
                    if (languagename == "strings")
                    {
                        languagename = "generic";
                    }

                    if (!dict.ContainsKey(languagename))
                    {
                        dict.Add(languagename, []);
                    }
                    var languagedict = dict[languagename];

                    foreach (var key in languageTokens.Keys)
                    {
                        languagedict.Add(key, languageTokens[key].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Parsing error in language file, Error: {ex}");
                return null;
            }
            if (dict.Count == 0)
            {
                return null;
            }
            return dict;
        }
    }
}