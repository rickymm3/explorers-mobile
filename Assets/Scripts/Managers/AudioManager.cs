#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using ExtensionMethods;

public class AudioManager : ManagerSingleton<AudioManager> {

    private AudioSource _source;
    private GameObject _audioSourcesGO;

    public bool isMuted = false;

    [Header("=== Playback Conflict-delay & Allocations ===")]
    [MinMaxRange(0,2)] public float timeForSFXConflicts = 0.25f;
    [Inspectionary] public SFXTypeAllocations sourceAllocations = new SFXTypeAllocations();

    [Header("=== SAE Setup (SimpleAudioEvents) ===")]
    [Inspectionary] public UIAudioDictionary UIEventSFXList = new UIAudioDictionary();
    [Inspectionary] public MusicAudioDictionary MusicEventSFXList = new MusicAudioDictionary();
    [Inspectionary] public AttacksAudioDictionary AttacksEventSFXList = new AttacksAudioDictionary();
    [Inspectionary] public DefensesAudioDictionary DefensesEventSFXList = new DefensesAudioDictionary();

    [ContextMenu("Play Click")]
    public void PlayClick() {
        Play(SFX_UI.Click);
    }

    public AudioSource source {
        get {
            if(_source==null) _source = GetComponentInChildren<AudioSource>();
            return _source;
        }
    }

    ////////////////////////////////////////////////

    void Start() {
        _audioSourcesGO = GetDynamics("AudioSources");

        CheckSFXLengthAndAllocations(UIEventSFXList, SFX_Types.UI);
        CheckSFXLengthAndAllocations(MusicEventSFXList, SFX_Types.Music);
        CheckSFXLengthAndAllocations(AttacksEventSFXList, SFX_Types.Attacks);
        CheckSFXLengthAndAllocations(DefensesEventSFXList, SFX_Types.Defenses);
    }

    private void CheckSFXLengthAndAllocations<T>(AudioEventDictionary<T> list, SFX_Types sfxType) {
        if(Enum.GetValues(typeof(T)).Length != list.Count) {
            traceWarn("WARNING: Mismatched Length for Audio Clips for enum: " + typeof(T));
        }

        list.sourceList = new List<AudioSource>();
        list.sfxType = sfxType;

        if (!sourceAllocations.ContainsKey(sfxType)) {
            traceWarn("Missing Audiosource allocation-count for SFX type: " + sfxType);
            return;
        }

        int numSources = sourceAllocations[sfxType];

        for (int s=numSources; --s>=0;) {
            AudioSource dup = _audioSourcesGO.AddClone(source);
            dup.gameObject.name = dup.gameObject.name.Replace("(Template)", "");
            list.sourceList.Add(dup);
        }
    }

    private AudioEventDictionary<T> ResolveSFXList<T>(T sfxEnum) {
        AudioEventDictionary<T> list = null;

        if (sfxEnum is SFX_UI) {
            list = (AudioEventDictionary<T>)(object)UIEventSFXList;
        } else if (sfxEnum is SFX_Music) {
            list = (AudioEventDictionary<T>)(object)MusicEventSFXList;
        } else if (sfxEnum is SFX_Attacks) {
            list = (AudioEventDictionary<T>)(object)AttacksEventSFXList;
        } else if (sfxEnum is SFX_Defenses) {
            list = (AudioEventDictionary<T>)(object)DefensesEventSFXList;
        }

        if (!list.ContainsKey(sfxEnum)) {
            traceError("Cannot play sound because it's missing / not implemented: " + sfxEnum);
            return null;
        }

        if(list[sfxEnum]==null) {
            traceError("Missing SFX at enum: " + sfxEnum);
            return null;
        }

        return list;
    }

    public void Stop<T>(T sfxEnum) {
        var list = ResolveSFXList(sfxEnum);
        if (list == null) return;

        // Generally, only 1st clip is defined anyways.
        AudioClip clip = list[sfxEnum].clips[0];

        foreach (AudioSource source in list.sourceList) {
            if(source.clip!=clip) continue;
            source.Stop();
        }
    }

    //////////////////////////////////////////////// Play!!! The most important method pretty much:

    public void Play<T>(T sfxEnum) {
        if(isMuted) return;

        var list = ResolveSFXList(sfxEnum);
        if(list==null) return;

        // Default source (in case of playing in the Editor)
        AudioSource source = this.source;

        if (Application.isPlaying) {
            if (list.sourceList.Count == 0) {
                traceError("Can't call 'Play' while the SFX source-list is empty! " + list.sfxType);
                return;
            }

            source = list.GetNextSource();
        }

        //Skip this sound if it has already played in the past 'timeForSFXConflicts' time.
        if (PlayedRecently(list, sfxEnum)) return;

        list[sfxEnum].Play(source);
    }

    private bool PlayedRecently<T>(AudioEventDictionary<T> list, T sfxEnum) {
        float timeNow = Time.time;
        var lastUsed = list._lastUsed;
        if (lastUsed.ContainsKey(sfxEnum)) {
            float timeDiff = timeNow - lastUsed[sfxEnum];
            if (timeDiff < timeForSFXConflicts) {
                return true;
            }
        }
        
        lastUsed[sfxEnum] = timeNow;

        return false;
    }

#if UNITY_EDITOR
    [MenuItem("ERDS/Generate SimpleAudioEvents from WAV,OGG,MP3 files...")]
    static public void GenerateSAEsFromFolder() {
        if(Selection.objects.Length==0) return;

        string pathTemplate = "Assets/ScriptableObjects/Audio/{0}.asset";

        foreach (UnityEngine.Object obj in Selection.objects) {
            if(!(obj is AudioClip)) continue;

            AudioClip clip = (AudioClip) obj;
            SimpleAudioEvent sae = new SimpleAudioEvent();
            sae.clips = new AudioClip[1];
            sae.clips[0] = clip;

            string saeName = clip.name.Replace("sfx_", "sae_");
            string pathAsset = String.Format(pathTemplate, saeName);
            AssetDatabase.CreateAsset(sae, pathAsset);
        }
    }
#endif

    [Serializable] public class UIAudioDictionary : AudioEventDictionary<SFX_UI> { }
    [Serializable] public class MusicAudioDictionary : AudioEventDictionary<SFX_Music> { }
    [Serializable] public class AttacksAudioDictionary : AudioEventDictionary<SFX_Attacks> { }
    [Serializable] public class DefensesAudioDictionary : AudioEventDictionary<SFX_Defenses> { }

    [Serializable]
    public class AudioEventDictionary<T> : SerializableDictionary<T, SimpleAudioEvent> {
        [SerializeField] public List<AudioSource> sourceList;
        [SerializeField] public SFX_Types sfxType;
        public int _currentSourceID = 0;
        public Dictionary<T, float> _lastUsed = new Dictionary<T, float>();

        public AudioSource GetNextSource() {
            _currentSourceID %= sourceList.Count;
            return sourceList[_currentSourceID++];
        }
    }
}

