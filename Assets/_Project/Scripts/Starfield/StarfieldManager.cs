using UnityEngine;
using System.Collections.Generic;

public class StarfieldManager : MonoBehaviour
{
    public List<StarfieldTile> ActiveStarfieldTiles = new List<StarfieldTile>();
    public int ActivatedTiles = 0;
    public static StarfieldManager Instance;
    public ParticleSystem BoostVFX;
    public Color DeadSpaceBackgroundColor;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject); // Optional: persist across scenes
    }



    void Update()
    {
        ActivatedTiles = ActiveStarfieldTiles.Count;
    }

    public void RemoveFromList(StarfieldTile tile)
    {
        if (ActiveStarfieldTiles.Contains(tile))
            ActiveStarfieldTiles.Remove(tile);
    }

    public void AddToList(StarfieldTile tile)
    {
        if (!ActiveStarfieldTiles.Contains(tile))
            ActiveStarfieldTiles.Add(tile);
    }

    public bool InList(StarfieldTile tile)
    {
        return ActiveStarfieldTiles.Contains(tile);
    }
}
