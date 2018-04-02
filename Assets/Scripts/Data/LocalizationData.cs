using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using ExtensionMethods;

public enum Section { DEFAULT, USER_PROFILE_ITEMS, USER_PROFILE_GEAR }

public class LocalizationManager {
    public static LocalizationManager Instance;

    public class TextEntry {
        public string id;
        public string text;
    }

    private static string _INVALID_SECTION_OR_ID = "*invalid-section-or-ID*";

    ListDictionary<Section, TextEntry> _sectionData;

    Section _defaultSection = Section.DEFAULT;

    public static void ParseData(JSONNode json) {
        var loc = Instance = new LocalizationManager();
        
        loc._sectionData = new ListDictionary<Section, TextEntry>();

        JSONArray entries = json["data"].AsArray;

        string language = "text-english";

        foreach (JSONNode entry in entries) {
            loc._sectionData.AddToList(
                entry["section"].AsEnum<Section>(),
                new TextEntry {
                    id = entry["id"].AsDecodedURL().ToLower(),
                    text = entry[language].AsDecodedURL()
                }
            );
        }

        Tracer.trace("LOADING", "~~~ Done Parsing Localization data. ~~~");

        // -- Example of how to GetText of a specific section:
        // trace(GetText("CATEGORY_SCROLLS", Section.USER_PROFILE));
        //
        // -- or...
        //
        // var test = GetSection(Section.USER_PROFILE);
        // trace(test.GetText("CATEGORY_SCROLLS"));
    }

    public static LocalizationManager GetSection(Section section) {
        var loc = new LocalizationManager();
        loc._sectionData = Instance._sectionData;
        loc._defaultSection = section;
        return loc;
    }

    public string GetText(string ID, Section section = Section.DEFAULT) {
        ID = ID.ToLower();
        if (section == Section.DEFAULT) section = _defaultSection;
        if(_sectionData==null) {
            Tracer.traceError("MISSING SECTION-DATA!!!");
            return "MISSING SECTION-DATA!";
        }
        if (!_sectionData.ContainsKey(section)) return _INVALID_SECTION_OR_ID;

        var entries = _sectionData[section];
        var entry = entries.Find(e => e.id == ID);

        if (entry==null) return _INVALID_SECTION_OR_ID;

        return entry.text;
    }
}
