using System;
using System.Collections.Generic;
using UnityEngine;

namespace Poopline
{
    // One record per level and layout revision. Tests can use an isolated save namespace.
    public sealed class ExplorationMap
    {
        public static string SaveNamespace = "Poopline.Exploration";
        public readonly SewerLevel Level;
        private readonly Dictionary<Vector2Int,int> cells = new Dictionary<Vector2Int,int>();
        private readonly byte[] discovered;
        private readonly string key;
        private bool dirty;
        private float nextScan, nextSave;
        public int Count { get; private set; }
        public string SaveKey => key;
        public ExplorationMap(SewerLevel level, int index)
        {
            Level=level;
            discovered=new byte[level.mapCells.Length];
            for(int i=0;i<level.mapCells.Length;i++) cells[Cell(level.mapCells[i].position)]=i;
            key=SaveNamespace+".v"+PooplineGame.LayoutVersion+".level"+index;
            Restore(PlayerPrefs.GetString(key,""));
        }
        private static Vector2Int Cell(Vector2 p) => new Vector2Int(Mathf.FloorToInt(p.x),Mathf.FloorToInt(p.y));
        public bool IsDiscovered(int index) => index>=0 && index<discovered.Length && discovered[index]!=0;
        public bool IsDiscovered(Vector3 point) => cells.TryGetValue(Cell(new Vector2(point.x,point.z)),out int i) && IsDiscovered(i);
        public bool HasCell(Vector2Int cell) => cells.ContainsKey(cell);
        public bool IsFrontier(int index)
        {
            if(!IsDiscovered(index)) return false;
            Vector2Int p=Cell(Level.mapCells[index].position);
            foreach(var d in Neighbors)
                if(cells.TryGetValue(p+d,out int other) && !IsDiscovered(other)) return true;
            return false;
        }
        private static readonly Vector2Int[] Neighbors={Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
        // Floor connectivity blocks discovery through a wall, even a low cutaway wall.
        private bool ConnectedSight(Vector2 from,Vector2 to)
        {
            int steps=Mathf.CeilToInt(Vector2.Distance(from,to)*5);
            for(int n=1;n<=steps;n++)
                if(!cells.ContainsKey(Cell(Vector2.Lerp(from,to,n/(float)steps)))) return false;
            return true;
        }
        public void Discover(Vector3 feet, bool immediate=false)
        {
            if(!immediate && Time.unscaledTime<nextScan) return;
            nextScan=Time.unscaledTime+.18f;
            Vector2 from=new Vector2(feet.x,feet.z);
            for(int i=0;i<discovered.Length;i++)
            {
                if(discovered[i]!=0) continue;
                Vector2 p=Level.mapCells[i].position;
                if((p-from).sqrMagnitude>20.25f || !ConnectedSight(from,p)) continue;
                // Machinery and solid architecture also occlude the surveyed floor.
                Vector3 target=new Vector3(p.x,feet.y+1.1f,p.y);
                if(Physics.Linecast(feet+Vector3.up*1.1f,target,~0,QueryTriggerInteraction.Ignore)) continue;
                discovered[i]=1;Count++;dirty=true;
            }
            if(dirty && Time.unscaledTime>=nextSave) { Save(); nextSave=Time.unscaledTime+2; }
        }
        public string Export() => Convert.ToBase64String(discovered);
        public void Restore(string encoded)
        {
            if(string.IsNullOrEmpty(encoded)) return;
            try
            {
                byte[] data=Convert.FromBase64String(encoded);
                if(data.Length!=discovered.Length) return;
                for(int i=0;i<data.Length;i++) if(data[i]>1) return;
                Array.Copy(data,discovered,data.Length);Count=0;
                foreach(byte b in discovered) if(b!=0) Count++;
            }
            catch(FormatException) { /* An invalid or old record starts an unexplored map. */ }
        }
        public void Clear()
        {
            Array.Clear(discovered,0,discovered.Length);Count=0;dirty=true;Save();
        }
        public void Save()
        {
            if(!dirty) return;
            PlayerPrefs.SetString(key,Export());PlayerPrefs.Save();dirty=false;
        }
    }
}
