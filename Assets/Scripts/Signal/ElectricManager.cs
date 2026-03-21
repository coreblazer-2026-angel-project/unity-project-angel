using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//public class ElectricManager : ManagerBase<ElectricManager> {
public class ElectricManager : MonoBehaviour {
    public PowerSource powerSource;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
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
}
