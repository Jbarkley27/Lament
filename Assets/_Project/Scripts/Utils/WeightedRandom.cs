using System.Collections.Generic;
using UnityEngine;

public static class WeightedRandom
{
    public static T Choose<T>(List<(T item, float weight)> weightedItems)
    {
        float totalWeight = 0f;

        // Sum all weights
        foreach (var (item, weight) in weightedItems)
            totalWeight += weight;

        // Pick a random point between 0 and totalWeight
        float randomPoint = Random.value * totalWeight;

        // Walk through list until we pass the random point
        foreach (var (item, weight) in weightedItems)
        {
            if (randomPoint < weight)
                return item;
            randomPoint -= weight;
        }

        // Fallback (shouldn't happen)
        return weightedItems[^1].item;
    }
}
