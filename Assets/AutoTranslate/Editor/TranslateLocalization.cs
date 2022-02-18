using GoodTime.Tools.GoogleApiTranslate;
using System.Collections.Generic;
using UnityEngine.Localization.Tables;

namespace GoodTime.HernetsMaksym.AutoTranslate
{
    public class TranslateLocalization
    {
        public static IEnumerable<TranslateStatus> Make(TranslateParameters translateParameters, TranslateData translateData)
        {
            GoogleApiTranslate translator = new GoogleApiTranslate();

            float progressRate = 0.9f / translateData.stringTables.Count;
            int indexTable = 0;
            int indexTableCollection = -1;

            foreach (var sharedtable in translateData.sharedtables)
            {
                ++indexTableCollection;
                if (translateParameters.canTranslateTableCollections[indexTableCollection] == false)
                {
                    continue;
                }
                StringTable sourceLanguageTable = new StringTable();
                List<StringTable> tablesForTranslate = new List<StringTable>();
                foreach (var table in translateData.stringTables)
                {
                    if (table.TableCollectionName == sharedtable.TableCollectionName)
                    {
                        if (table.LocaleIdentifier != translateData.selectedLocale.Identifier)
                        {
                            tablesForTranslate.Add(table);
                        }
                        else
                        {
                            sourceLanguageTable = table;
                        }
                    }

                }

                foreach (StringTable targetLanguageTable in tablesForTranslate)
                {
                    ++indexTable;
                    float progress = 0.1f + indexTable * progressRate;

                    yield return new TranslateStatus(progress, sharedtable.TableCollectionName, targetLanguageTable.LocaleIdentifier.CultureInfo.DisplayName);

                    foreach (var entry in sharedtable.Entries)
                    {
                        StringTableEntry sourceWord;
                        if (!sourceLanguageTable.TryGetValue(entry.Id, out sourceWord))
                        {
                            continue;
                        }
                        if (sourceWord == null)
                        {
                            continue;
                        }
                        if (sourceWord.IsSmart == true && translateParameters.canTranslateSmartWords == false)
                        {
                            continue;
                        }

                        StringTableEntry targetWord;
                        if (targetLanguageTable.TryGetValue(entry.Id, out targetWord))
                        {
                            if (translateParameters.canOverrideWords == false)
                            {
                                continue;
                            }
                        }

                        string result = translator.Translate(sourceWord.Value, sourceLanguageTable.LocaleIdentifier.Code, targetLanguageTable.LocaleIdentifier.Code);
                        targetLanguageTable.AddEntry(entry.Key, result);
                    }
                }
            }

            yield return new TranslateStatus(1, string.Empty, string.Empty);
        }
    }
}