using System;
using UnityEngine;

namespace Poopline
{
    [Serializable]
    public sealed class JourneyRecord
    {
        public int format=1, layout=PooplineGame.LayoutVersion, level, checkpoint=-1;
        public bool sluiceSolved, sluiceSeen, completed;
        // Optional in layout-10 saves from 0.6.1; missing fields safely default to zero.
        public int sluiceHintStep;
        public float sluiceHintElapsed;
        public bool sluiceValveFound;
        public float elapsed, levelElapsed;
        public float[] stageTimes;
        public string[] maps;
        public string savedAt;
    }
    public static class JourneySave
    {
        public static string Key => ExplorationMap.SaveNamespace+".Journey";
        public static bool Valid(JourneyRecord record,SewerLevel[] levels)
        {
            if(record==null || record.format!=1 || record.layout!=PooplineGame.LayoutVersion || record.level<0 || record.level>=levels.Length) return false;
            if(record.checkpoint< -1 || record.checkpoint>=levels[record.level].checkpoints.Length || record.maps==null || record.maps.Length!=levels.Length || record.stageTimes==null || record.stageTimes.Length!=levels.Length) return false;
            if(!Finite(record.elapsed) || !Finite(record.levelElapsed) || record.levelElapsed>record.elapsed+.1f) return false;
            if(record.sluiceHintStep<0 || record.sluiceHintStep>3 || !Finite(record.sluiceHintElapsed) || record.sluiceHintElapsed>SluicePuzzle.ValveHintDelay+.5f) return false;
            foreach(float t in record.stageTimes) if(!Finite(t)) return false;
            for(int i=0;i<record.maps.Length;i++)
            {
                if(record.maps[i]==null) return false;
                try
                {
                    var bytes=Convert.FromBase64String(record.maps[i]);
                    if(bytes.Length!=levels[i].mapCells.Length) return false;
                    foreach(byte b in bytes) if(b>1) return false;
                }
                catch(FormatException) {return false;}
            }
            // Reaching the second stage requires the first sluice to have been solved.
            return (record.level==0 || record.sluiceSolved) && (!record.completed || record.level==levels.Length-1);
        }
        private static bool Finite(float f) => !float.IsNaN(f) && !float.IsInfinity(f) && f>=0;
        private static JourneyRecord Decode(string key,SewerLevel[] levels)
        {
            string json=PlayerPrefs.GetString(key,"");
            if(string.IsNullOrEmpty(json)) return null;
            try {var record=JsonUtility.FromJson<JourneyRecord>(json);return Valid(record,levels)?record:null;}
            catch(ArgumentException) {return null;}
        }
        public static JourneyRecord Load(SewerLevel[] levels) => Decode(Key,levels) ?? Decode(Key+".backup",levels);
        public static void Write(JourneyRecord record,SewerLevel[] levels)
        {
            if(!Valid(record,levels)) return;
            if(Decode(Key,levels)!=null) PlayerPrefs.SetString(Key+".backup",PlayerPrefs.GetString(Key));
            PlayerPrefs.SetString(Key,JsonUtility.ToJson(record));PlayerPrefs.Save();
        }
        public static void Clear() {PlayerPrefs.DeleteKey(Key);PlayerPrefs.DeleteKey(Key+".backup");PlayerPrefs.Save();}
    }
    public sealed class SewerSettings
    {
        public float Volume=.65f, Ambience=.55f;
        public bool MenuMotion=true;
        private readonly string key=ExplorationMap.SaveNamespace+".Settings";
        public SewerSettings()
        {
            Volume=Mathf.Clamp01(PlayerPrefs.GetFloat(key+".volume",.65f));
            Ambience=Mathf.Clamp01(PlayerPrefs.GetFloat(key+".ambience",.55f));
            MenuMotion=PlayerPrefs.GetInt(key+".motion",1)!=0;
        }
        public void Save()
        {
            PlayerPrefs.SetFloat(key+".volume",Mathf.Clamp01(Volume));
            PlayerPrefs.SetFloat(key+".ambience",Mathf.Clamp01(Ambience));
            PlayerPrefs.SetInt(key+".motion",MenuMotion?1:0);PlayerPrefs.Save();
        }
    }
}
