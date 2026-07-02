using System.Collections.Generic;

public class Stats
{
    private readonly Dictionary<string, IntStat> intStats = new();
    public IntStat GetInt(string name) => intStats[name];
    public Stats SetInt(string name, IntStat value) {
        intStats[name] = value;
        return this;
    }
    
    private readonly Dictionary<string, FloatStat> floatStats = new();
    public FloatStat GetFloat(string name) => floatStats[name];
    public Stats SetFloat(string name, FloatStat value) {
        floatStats[name] = value;
        return this;
    }

    public override string ToString()
    {
        string statsString = "";
        foreach ((string statName, IntStat statValue) in intStats)
        {
            statsString += $"  {statName}: {statValue}\n";
        }
        foreach ((string statName, FloatStat statValue) in floatStats)
        {
            statsString += $"  {statName}: {statValue}\n";
        }

        return statsString;
    }
}