using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Test : MonoBehaviour
{
    public int genSize;
    List<Dna> pool;

    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.K)) 
        {
            InitialisePool();
            List<int> threeDupGens = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                if (threeDupGens.Count >3) break;
                var newPool = new List<Dna>(genSize);
                for (int j = 0; j < genSize; j+= 2)
                {
                    newPool.AddRange(breedRandomParents(pool));
                }

                int dups = 0;
                foreach (var k in newPool)
                {
                    foreach (var otherK in newPool)
                    {
                        if (k != otherK && k.IsEqual(otherK)) dups++;
                    }
                }

                if(dups > 0) Debug.Log("Gen: " + i + " dups: " + dups);
                if(dups > 0) threeDupGens.Add(i);
                pool = newPool;
            }
        }
    }

    void InitialisePool()
    {
        pool = new List<Dna>(genSize);
        for (int i = 0; i < genSize; i++)
        {
            pool.Add(new Dna(5,2,1,5));
        }

        int dups = 0;
        foreach (var k in pool)
        {
            foreach (var otherK in pool)
            {
                if (k != otherK && k.IsEqual(otherK)) dups++;
            }
        }
        if (dups > 0 )Debug.LogError("dups in seed gen");
    }

    Dna[] breedRandomParents(List<Dna> pool)
    {
        var tempPool = pool.OrderBy(d => Random.Range(0f , 1f)).ToList();
        return tempPool[0].Splice(tempPool[1]);
    }
}
