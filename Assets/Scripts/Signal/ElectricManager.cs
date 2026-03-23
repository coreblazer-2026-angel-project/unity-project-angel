using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ElectricManager : ManagerBase<ElectricManager> {
    public PowerSource powerSource;
    public int curId = 0;
    public Dictionary<int, ElectricElementBase> ElectricElements;

    void Start() {

    }

    void Update() {

    }

    [ContextMenu("Begin Simulate")]
    public void BeginSimulate() {
        if (powerSource == null) {
            Debug.Log("PowerSource Not Selected!");
            return;
        }

        Queue<ElectricElementBase> queue = new();
        HashSet<ElectricElementBase> visited = new();

        queue.Enqueue(powerSource);
        visited.Add(powerSource);
        powerSource.intensity = powerSource.workIntensity;

        while (queue.Count > 0) {
            var cur = queue.Dequeue();
            if (cur.intensity >= cur.workIntensity) {
                cur.Activate();
            }
            else {
                cur.Deactive();
            }

            foreach (var next in cur.neighborElements) {
                if (visited.Contains(next)) continue;

                visited.Add(next);
                next.intensity = Mathf.Max(0, cur.intensity - 1);
                queue.Enqueue(next);
            }
        }
    }

    public void AddElement(ElectricElementBase electricElement) {
        electricElement.ID = curId;
        ElectricElements.Add(electricElement.ID, electricElement);
        ++curId;
    }

    public void RemoveElement(ElectricElementBase electricElementBase) {
        ElectricElements.Remove(electricElementBase.ID);
        Destroy(electricElementBase.gameObject);
    }

    class UnionFind {
        private int[] parent;

        public UnionFind(int n) {
            parent = new int[n];
            for (int i = 0; i < n; i++)
                parent[i] = i;
        }

        public int Find(int x) {
            if (parent[x] != x)
                parent[x] = Find(parent[x]);
            return parent[x];
        }

        public bool Union(int a, int b) {
            int rootA = Find(a);
            int rootB = Find(b);

            if (rootA == rootB) {
                return false;
            }

            parent[rootA] = rootB;
            return true;
        }
    }

    public bool CheckRing() {
        UnionFind uf = new UnionFind(curId);

        foreach (var kv in ElectricElements) {
            var element = kv.Value;

            foreach (var neighbor in element.neighborElements) {
                if (element.ID < neighbor.ID) {
                    if (!uf.Union(element.ID, neighbor.ID)) {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
