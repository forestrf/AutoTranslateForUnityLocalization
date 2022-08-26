using GoodTime.Tools.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Localization.Tables.SharedTableData;

namespace GoodTime.HernetsMaksym.AutoTranslate.Editor
{
	public static class SearchTextForLocalization 
    {
        public static string Search(SearchTextParameters parameters, Scene scene)
		{
            GameObject[] gameObjects = DatabaseProject.GetGameObjects(scene.name);
            if (gameObjects.Length == 0) return "gameObjects is 0";
            string result = AddLocalization(parameters, gameObjects);
            EditorSceneManager.SaveScene(scene);
            return result;
        }

        public static string Search(SearchTextParameters parameters, GameObject mainPrefab)
        {
            GameObject[] gameObjects = new GameObject[1];
            gameObjects[0] = mainPrefab;
            string result = AddLocalization(parameters, gameObjects);
            EditorUtility.SetDirty(mainPrefab);
            return result;
        }

        public static List<string> GetAvailableForSearchUIElements()
		{
            List<string> Checklists = new List<string>();
            Checklists.Add("Text Legacy");
            Checklists.Add("Dropdown Legacy (not work)");
            Checklists.Add("Text Mesh Pro (not work)");
            Checklists.Add("Dropdown Mesh Pro (not work)");
            return Checklists;
        }

        public static string AddLocalization(SearchTextParameters parameters, GameObject[] gameObjects)
        {
            StatusLocalizationScene statusLocalizationScene = SearchForLocalization(gameObjects, parameters.IsSkipPrefab);

            SharedTableData sharedTable = SharedTableDataExtension.GetOrAdd_SharedTableData(parameters.NameTable);

            LocalizeStringEvent localizeStringEvent = default(LocalizeStringEvent);
            SharedTableEntry sharedTableEntry = default(SharedTableEntry);
            StringTable stringTable = default(StringTable);

            foreach (Text text in statusLocalizationScene.LegacyTexts)
            {
                if (parameters.IsSkipPrefab == true && PrefabUtility.IsPartOfAnyPrefab(text.gameObject)) continue;

                localizeStringEvent = GetOrAdd_LocalizeStringEventComponent(text.gameObject);

                sharedTableEntry = SharedTableData_AddEntry(sharedTable, text, "TextLegacy");

                stringTable = SimpleInterfaceStringTable.GetStringTable(sharedTable, parameters.SourceLocale);
                stringTable.AddEntry(sharedTableEntry.Key, text.text);

                localizeStringEvent.Clear_OnUpdateString();
                localizeStringEvent.Sign_ReferenceTable(sharedTable.TableCollectionName, sharedTableEntry.Key);
                localizeStringEvent.Sign_OnUpdateString(text);
            }

            return "Completed";
        }

        public static StatusLocalizationScene CheckTextAboutLocalization(Scene scene, bool skipPrefab)
        {
            GameObject[] gameObjects = DatabaseProject.GetGameObjects(scene.name);

            return SearchForLocalization(gameObjects, skipPrefab);
        }

        public static StatusLocalizationScene CheckTextAboutLocalization(GameObject gameObject, bool skipPrefab)
        {
            GameObject[] gameObjects = new GameObject[1];
            gameObjects[0] = gameObject;

            return SearchForLocalization(gameObjects, skipPrefab);
        }

        private static StatusLocalizationScene SearchForLocalization(GameObject[] gameObjects, bool skipPrefab)
		{
            StatusLocalizationScene statusLocalizationScene = new StatusLocalizationScene();

            List<Text> texts = GameObjectHelper.GetComponentsInChildrens<Text>(gameObjects);
            statusLocalizationScene.LegacyTexts = FilterTextLegacy(texts, skipPrefab, statusLocalizationScene);

            List<Dropdown> dropdowns = GameObjectHelper.GetComponentsInChildrens<Dropdown>(gameObjects);
            statusLocalizationScene.LegacyDropdowns = FilterDropdownLegacy(dropdowns, skipPrefab, statusLocalizationScene);

            statusLocalizationScene.Prefabs = GameObjectHelper.DetectPrefabs(gameObjects);

            return statusLocalizationScene;
        }

        public static List<Text> FilterTextLegacy(List<Text> texts, bool skipPrefab, StatusLocalizationScene statusLocalizationScene)
		{
            LocalizeStringEvent localizeStringEvent = default(LocalizeStringEvent);
            List<Text> result = new List<Text>();

            foreach (Text text in texts)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(text.gameObject) && skipPrefab == true) 
                    continue;

                if (text.gameObject.TryGetComponent<LocalizeStringEvent>(out localizeStringEvent))
                    statusLocalizationScene.LocalizeStringEvents.Add(localizeStringEvent);

                result.Add(text);
            }

            return result;
        }

        public static List<Dropdown> FilterDropdownLegacy(List<Dropdown> texts, bool skipPrefab, StatusLocalizationScene statusLocalizationScene)
        {
            LocalizeStringEvent localizeStringEvent = default(LocalizeStringEvent);
            List<Dropdown> result = new List<Dropdown>();

            foreach (Dropdown text in texts)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(text.gameObject) && skipPrefab == true)
                    continue;

                if (text.gameObject.TryGetComponent<LocalizeStringEvent>(out localizeStringEvent))
                    statusLocalizationScene.LocalizeStringEvents.Add(localizeStringEvent);

                result.Add(text);
            }

            return result;
        }

        private static LocalizeStringEvent GetOrAdd_LocalizeStringEventComponent(GameObject gameObject)
		{
            LocalizeStringEvent localizeStringEvent = default(LocalizeStringEvent);
            if (gameObject.TryGetComponent<LocalizeStringEvent>(out localizeStringEvent))
                return gameObject.GetComponent<LocalizeStringEvent>();
            else
                return gameObject.AddComponent<LocalizeStringEvent>();
        }

        private static SharedTableEntry SharedTableData_AddEntry(SharedTableData sharedTable, Text text, string typeText)
		{
            string name = String.Format("[{0}][{1}][{2}]", text.gameObject.name, text.gameObject.transform.parent?.name, typeText);
            int variants = 1;
            while (sharedTable.Contains(name))
            {
                name = String.Format("[{0}][{1}][{2}][{3}]", text.gameObject.name, text.gameObject.transform.parent?.name, typeText, variants);
                ++variants;
            }
            SharedTableEntry sharedTableEntry = sharedTable.AddKey(name);
            return sharedTableEntry;
        }
    }
}