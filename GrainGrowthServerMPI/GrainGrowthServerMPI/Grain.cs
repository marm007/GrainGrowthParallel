using System;
using System.Collections.Generic;
using static Config;

public interface IGrain : IEquatable<IGrain>
{
    int X { get; set; }
    int Y { get; set; }
    int Z { get; set; }
}

[Serializable]
public struct SGrain
{
    public SGrain(int x, int y, int z, int state)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
        this.State = state;
    }

    
    public int X { get; set; }
    public int Y { get; set;  }
    public int Z { get; set;  }
    public int State { get; set; }

    
}

public class Grain : IGrain
{
    private List<Grain> neighbours;

    private int state;
    private int previousState;

    private int x;
    private int y;
    private int z;

    private int q;

    private bool isOnBorder;



    public List<Grain> Neighbours { get { return neighbours; } set { neighbours = value; } }

    public Grain() { }

    public Grain(int x, int y, int state)
    {
        this.neighbours = new List<Grain>();

        this.state = state;
        this.x = x;
        this.y = y;
        this.z = 0;
        this.isOnBorder = false;
    }

    public Grain(int x, int y, int z, int state)
    {
        this.neighbours = new List<Grain>();

        this.state = state;
        this.previousState = state;
        this.x = x;
        this.y = y;
        this.z = z;
        this.isOnBorder = false;

    }

    public int State { get { return state; } set {  state = value; } }
    public bool IsOnBorder { get { return isOnBorder; } set { isOnBorder = value; } }
    public int PrevState { get { return previousState; } set { previousState = value; } }
    public int X { get { return x; } set { x = value; } }
    public int Y { get { return y; } set { y = value; } }
    public int Z { get { return z; } set { z = value; } }
    public int Q { get { return q; } set { q = value; } }
    
    public Grain Copy()
    {
        Grain grain = new Grain(this.x, this.y, this.z, this.state);
        return grain;
    }

    public bool Equals(IGrain other)
    {
        return this.X == other.X &&
               this.Y == other.Y &&
               this.Z == other.Z;
    }


}
