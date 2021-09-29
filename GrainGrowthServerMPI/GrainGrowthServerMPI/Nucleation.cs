using System;
using System.Collections;
using System.Collections.Generic;
using static Config;

public class Nucleation
{

    public static List<SGrain> Random(int number, Config config)
    {
        List<SGrain> nucelons = new List<SGrain>();

        Random rnd = new Random();

        int Index = 1;

        while (nucelons.Count != number)
        {
            int x = rnd.Next(config.SizeX);
            int y = rnd.Next(config.SizeY);
            int z = rnd.Next(config.SizeZ);


            SGrain sGrain = new SGrain(x, y, z, Index);

            if (!nucelons.Contains(sGrain))
            {
                nucelons.Add(sGrain);
                Index++;
            }
        }

        return nucelons;
    }

    public static void Random(Grain [,,] grains, int number, Config config)
    {
        Random rnd = new Random();

        List<Grain> emptyGrains = new List<Grain>();

        for (int i = 0; i < config.SizeX; i++)
        {
            for (int j = 0; j < config.SizeY; j++)
            {
                for(int k = 0; k < config.SizeZ; k++)
                {
                    if (grains[i, j, k].State == 0)
                    {
                        emptyGrains.Add(grains[i, j, k]);
                    }
                }
            }
        }

        int Index = 1;

        for (int i = 0; i < number; i++)
        {
            if (emptyGrains.Count > 0)
            {
                int index = rnd.Next(emptyGrains.Count);
                emptyGrains[index].State = Index;
                Index++;
                emptyGrains.RemoveAt(index);
            }
        }
    }

    public static void RandomMC(Grain[,,] grains, int number, Config config)
    {
        Random rnd = new Random();

        for (int i = 0; i < config.SizeX; i++)
        {
            for (int j = 0; j < config.SizeY; j++)
            {
                for (int k = 0; k < config.SizeZ; k++)
                {
                    if (grains[i, j, k].State == 0 )
                    {
                        grains[i, j, k].State = rnd.Next(number);
                    }
                }
            }
        }
    }

    public static void RandomMC(Stack grains, int number, Config config)
    {
        Random rnd = new Random();

        List<Grain> emptyGrains = new List<Grain>();
        foreach (Grain g in grains)
        {
            if(g.State == 0) 
                g.State = rnd.Next(number) + 1;
        }
    }

    public static void RandomMC1(Grain[,,] grains, int number, Config config)
    {
        Random rnd = new Random();

        List<Grain> emptyGrains = new List<Grain>();
        foreach (Grain g in grains)
        {
            if(g.State == 0) 
                g.State = rnd.Next(number) + 1;
        }
    }
}

