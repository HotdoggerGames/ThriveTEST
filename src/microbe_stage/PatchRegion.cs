﻿using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json;
using Nito.Collections;


public class PatchRegion
{
    [JsonProperty]
    public readonly int ID;

    [JsonProperty]
    public readonly ISet<PatchRegion> Adjacent = new HashSet<PatchRegion>();

    [JsonProperty]
    public LocalizedString Name { get; private set; }
    public float PatchNodeWidth = 64.0f;
    public float PatchNodeHeight = 64.0f;
    public string RegionType = null!;

    /// <summary>
    ///   Coordinates this region is to be displayed in the GUI
    /// </summary>
    public Vector2 ScreenCoordinates { get; set; } = new(0, 0);

    public float PatchMargin = 4f;

    public float Height;

    public float Width;

    public List<Patch> Patches;

    public PatchRegion(int Id, LocalizedString name, string regionType, Vector2 coordinates)
    {
        ID = Id;
        Patches = new List<Patch>();
        Name = name;
        Height = 0;
        Width = 0;
        RegionType = regionType;
        ScreenCoordinates = coordinates;
    }

    public void AddPatch(Patch patch)
    {
        Patches.Add(patch);
    }

    public void BuildPatches()
    {
        Patches.Sort((x,y) => x.Depth[1].CompareTo(y.Depth[1]));
        if (RegionType != "continent" && Patches.Count == 5)
        {
            Patches.Add(Patches[Patches.Count - 2]);
            Patches.RemoveAt(Patches.Count - 3);
        }

        // Patch linking first
        for (int i = 0; i < Patches.Count - 1; i++)
        {
            if (RegionType == "sea" || RegionType == "ocean")
            {
                LinkPatches(Patches[i], Patches[i+1]);
            }

            if (RegionType == "continent")
            {
                for (int j = 0; j < Patches.Count; j++)
                {
                    if (j != i)
                    {
                        LinkPatches(Patches[i], Patches[j]);
                    }
                }

            }
        }
        if (RegionType == "vents" || RegionType == "underwatercave")
        {
            foreach (var adjacent in Adjacent)
                LinkPatches(Patches[0], adjacent.Patches[adjacent.Patches.Count - 1]);
        }

        // Patches position configuration
        for (int i = 0; i < Patches.Count; i++)
        {
            
            if ( RegionType == "sea" || RegionType == "ocean")
                Patches[i].ScreenCoordinates = new Vector2 (ScreenCoordinates.x + PatchMargin, ScreenCoordinates.y + i * (64f + PatchMargin) + PatchMargin);

            if (RegionType == "continent")
            {
                if (i % 2 == 0)
                {
                    if (i == 0)
                        Patches[i].ScreenCoordinates = new Vector2 (ScreenCoordinates.x + PatchMargin, ScreenCoordinates.y + PatchMargin);
                    else
                        Patches[i].ScreenCoordinates = new Vector2 (ScreenCoordinates.x + PatchMargin, ScreenCoordinates.y + 64f + 2* PatchMargin);
                }
                else
                {
                    if (i == 1)
                        Patches[i].ScreenCoordinates = new Vector2 (ScreenCoordinates.x + 2 * PatchMargin + 64f, ScreenCoordinates.y + PatchMargin);
                    else
                        Patches[i].ScreenCoordinates = new Vector2 (ScreenCoordinates.x + 2 * PatchMargin + 64f, ScreenCoordinates.y + 2* PatchMargin + 64f);
                }   
            }

            if (RegionType == "vents" || RegionType == "underwatercave")
            {
                Patches[0].ScreenCoordinates = new Vector2 (ScreenCoordinates.x + PatchMargin, ScreenCoordinates.y + PatchMargin);
            }
        }


    }
    public void BuildRegion()
    {
        // Region size configuration
        Width += 64f + 2 * PatchMargin;
        
        if (RegionType == "continent")
        {
            Height = 64f + 2*PatchMargin;
            if (Patches.Count > 1)
                Width += 64f + PatchMargin;
            
            if (Patches.Count > 2)
                Height = 3 * PatchMargin + 2 * 64f;
        }

        if (RegionType == "ocean" || RegionType == "sea")
        {
            Height += 64f * Patches.Count + (Patches.Count + 1) * PatchMargin;
        }
        if (RegionType == "vents")
        {
            foreach (var adjacent in Adjacent)
                ScreenCoordinates = adjacent.ScreenCoordinates + new Vector2(0, adjacent.Height) + new Vector2 (0, 20);
            Height = Width = 64 + 2 * PatchMargin;
        }

    }

    private void LinkPatches(Patch patch1, Patch patch2)
    {
        patch1.AddNeighbour(patch2);
        patch2.AddNeighbour(patch1);
    }
    
    /// <summary>
    ///   Adds a connection to patch
    /// </summary>
    /// <returns>True if this was new, false if already added</returns>
    public bool AddNeighbour(PatchRegion region)
    {
        return Adjacent.Add(region);
    }

    public Vector2 GetSize()
    {
        return new Vector2(Width, Height);
    }

}