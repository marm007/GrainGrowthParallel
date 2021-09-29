using System;
using System.Collections.Generic;
using System.Linq;
using static Config;

public static class NeighbourhoodFactory
{
    private static Random random = new Random();
    private static int ERROR = -155;

    public static IEnumerable<T> Mode<T>(this IEnumerable<T> input)
    {
        var dict = input.ToLookup(x => x);
        if (dict.Count == 0)
            return Enumerable.Empty<T>();
        var maxCount = dict.Max(x => x.Count());
        return dict.Where(x => x.Count() == maxCount).Select(x => x.Key);
    }

    static int mostFrequent(int[] arr,
                           int n)
    {
        // Insert all elements in hash 
        Dictionary<int, int> hp =
                    new Dictionary<int, int>();

        for (int i = 0; i < n; i++)
        {
            int key = arr[i];
            if (hp.ContainsKey(key))
            {
                int freq = hp[key];
                freq++;
                hp[key] = freq;
            }
            else
                hp.Add(key, 1);
        }

        // find max frequency. 
        int min_count = 0, res = -1;

        foreach (KeyValuePair<int,
                    int> pair in hp)
        {
            if (min_count < pair.Value)
            {
                res = pair.Key;
                min_count = pair.Value;
            }
        }
        return res;
    }

    public static int GetState(Grain grain)
    {
        var countedItems = new Dictionary<int, int>();

        foreach (Grain g in grain.Neighbours)
        {
            if (g.State != 0)
            {
                int count = 0;
                countedItems.TryGetValue(g.State, out count);
                count++;
                countedItems[g.State] = count;
            }
        }

        if (countedItems.Count == 0)
            return 0;

        var maxValues = countedItems.GroupBy(kv => kv.Value).OrderByDescending(g => g.Key).First();

        return maxValues.ElementAt(random.Next(maxValues.Count())).Key;
    }

    public static List<Grain> GetNeighboursGrains(Grain[,,] grains, Grain grain, Config config)
    {
        switch (config.Neighbourhood)
        {
            case ENeighbourhood.vonNeumann:
                return VonNeumannGrains(grains, grain.X, grain.Y, grain.Z, config);
            case ENeighbourhood.Moore:
                return MooreGrains(grains, grain.X, grain.Y, grain.Z, config);
            default:
                return null;
        }
    }

    private static int FindNeighboor(int valueToCheck, int value, int size, EBoundaryCondition BoundaryCondition)
    {
        if (BoundaryCondition == EBoundaryCondition.Periodic)
        {
            if (valueToCheck < 0)
                return size - 1;
            else if (valueToCheck >= size)
                return 0;
        }
        else
        {
            if (valueToCheck < 0 || valueToCheck >= size)
                return ERROR;
        }
            
        return valueToCheck;
    }


    public static List<Grain> VonNeumannGrains(Grain[,,] grains, int x, int y, int z, Config config)
    {
        HashSet<Grain> neighbours = new HashSet<Grain>();

        for (int _x = x - 1; _x <= x + 1; _x++)
        {
            int valueX = FindNeighboor(_x, x, config.SizeX, config.BoundaryCondition);

            if (valueX == ERROR)
                continue;

            for (int _y = y - 1; _y <= y + 1; _y++)
            {
                int valueY = FindNeighboor(_y, y, config.SizeY, config.BoundaryCondition);
                if (valueY == ERROR)
                    continue;

                for (int _z = z - 1; _z <= z + 1; _z++)
                {
                    int valueZ = FindNeighboor(_z, z, config.SizeZ, config.BoundaryCondition);
                    if (valueZ == ERROR)
                        continue;

                    if(config.BoundaryCondition == EBoundaryCondition.Periodic)
                    {
                        if ((config.SizeZ > 1 && valueX == x && valueY == y && (Math.Abs(valueZ - z) == 1 || (valueZ == config.SizeZ - 1) || (valueZ == 0))) ||
                             (config.SizeX > 1 && (Math.Abs(valueX - x) == 1 || (valueX == config.SizeX - 1) || (valueX == 0)) && valueY == y && valueZ == z) ||
                             (config.SizeY > 1 && valueX == x && (Math.Abs(valueY - y) == 1 || (valueY == config.SizeY - 1) || (valueY == 0)) && valueZ == z))
                        {
                            neighbours.Add(grains[valueX, valueY, valueZ]);
                        }
                    } else
                    {
                        if ((valueX == x && valueY == y && Math.Abs(valueZ - z) == 1) ||
                             (Math.Abs(valueX - x) == 1 && valueY == y && valueZ == z) ||
                             (valueX == x && Math.Abs(valueY - y) == 1 && valueZ == z))
                        {
                            neighbours.Add(grains[valueX, valueY, valueZ]);
                        }
                    }
                 
                }
            }
        }

        if (config.BoundaryCondition == EBoundaryCondition.Periodic)
        {
            neighbours.Remove(grains[x, y, z]);

           if ((config.SizeZ == 1 && neighbours.Count() != 4) || (config.SizeZ > 1 && neighbours.Count() != 6))
                Console.WriteLine("Bad added: " + neighbours.Count());


        }

        else
            if ((config.SizeZ == 1 && neighbours.Count() > 4) || (config.SizeZ > 1 && neighbours.Count() > 6))
            Console.WriteLine("WARNING added: " + neighbours.Count());

        return neighbours.ToList();

    }

    public static List<Grain> MooreGrains(Grain[,,] grains, int x, int y, int z, Config config)
    {
        HashSet<Grain> neighbours = new HashSet<Grain>();
        int count = 0;

        for (int _x = x - 1; _x <= x + 1; _x++)
        {
            int valueX = FindNeighboor(_x, x, config.SizeX, config.BoundaryCondition);
            if (valueX == ERROR)
                continue;

            for (int _y = y - 1; _y <= y + 1; _y++)
            {
                int valueY = FindNeighboor(_y, y, config.SizeY, config.BoundaryCondition);
                if (valueY == ERROR)
                    continue;

                for (int _z = z - 1; _z <= z + 1; _z++)
                {
                    int valueZ = FindNeighboor(_z, z, config.SizeZ, config.BoundaryCondition);
                    if (valueZ == ERROR)
                        continue;

                    if (!(valueX == x && valueY == y && valueZ == z))
                    {
                        count += 1;
                        neighbours.Add(grains[valueX, valueY, valueZ]);

                    }
                }
            }
        }

        if (config.BoundaryCondition == EBoundaryCondition.Periodic)
        {

            if ((config.SizeZ == 1 && neighbours.Count() != 8) || (config.SizeZ > 1 && neighbours.Count() != 26))
                Console.WriteLine("Bad added: " + neighbours.Count());

        }

        else
           if ((config.SizeZ == 1 && neighbours.Count() > 8) || (config.SizeZ > 1 && neighbours.Count() > 26))
            Console.WriteLine("WARNING added: " + count);
        return neighbours.ToList();

    }
}
