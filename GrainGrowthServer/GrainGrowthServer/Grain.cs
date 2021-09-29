using System;
using System.Collections.Generic;
using static Config;

public class Grain : IEquatable<Grain>
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

    public Grain(int x, int y, int state)
    {
        this.neighbours = new List<Grain>();

        this.state = state;
        this.previousState = state;
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
    public int PrevState { get { return previousState; } set { previousState = value; } }
    public int X { get { return x; } }
    public int Y { get { return y; } }
    public int Z { get { return z; } }
    public int Q { get { return q; } set { q = value; } }

    public bool IsOnBroder { get { return isOnBorder; } set { isOnBorder = value; } }

    public Grain Copy()
    {
        Grain grain = new Grain(this.x, this.y, this.z, this.state);
        grain.isOnBorder = this.isOnBorder;
        return grain;
    }

    public bool Equals(Grain other)
    {
        return this.X == other.X &&
               this.Y == other.Y &&
               this.Z == other.Z;
    }

}
