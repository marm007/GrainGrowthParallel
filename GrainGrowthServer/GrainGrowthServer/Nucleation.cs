using System;
using System.Collections.Generic;
using static Config;

public class Nucleation
{
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

        List<Grain> emptyGrains = new List<Grain>();

        for (int i = 0; i < config.SizeX; i++)
        {
            for (int j = 0; j < config.SizeY; j++)
            {
                for (int k = 0; k < config.SizeZ; k++)
                {
                    if (grains[i, j, k].State == 0)
                    {
                        grains[i, j, k].State = rnd.Next(number);
                        grains[i, j, k].PrevState = grains[i ,j ,k].State;
                    }
                }
            }
        }
    }
}

