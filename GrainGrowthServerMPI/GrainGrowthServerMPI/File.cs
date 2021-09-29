using System;
using System.Collections.Generic;
using System.Xml;
using static Config;

namespace GrainGrowthServer
{
    public  class File
    {
        public void Deserialize(Config config)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(config.XMLPath);

            int sizeX = 0;
            int sizeY = 0;
            int sizeZ = 0;

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string text = node.InnerText;
                string name = node.Name;

                switch(name)
                {
                    case "FilePath":
                        config.FileName = text;
                        break;
                    case "Nucleons":
                        config.NumberOfNucleons = Int32.Parse(text);
                        break;
                    case "SizeX":
                        sizeX =  Int32.Parse(text);
                        break;
                    case "SizeY":
                        sizeY = Int32.Parse(text);
                        break;
                    case "SizeZ":
                        sizeZ = Int32.Parse(text);
                        break;
                    case "KT":
                        config.KT = float.Parse(text);
                        break;
                    case "J":
                        config.J = float.Parse(text);
                        break;
                    case "Simulation":
                        switch(text)
                        {
                            case "CA":
                                config.Simulation = ESimulation.CA;
                                break;
                            case "MC":
                                config.Simulation = ESimulation.MonteCarlo;
                                break;
                        }
                        break;
                    case "NumberOfIterations":
                        config.NumberOfIterations = Int32.Parse(text);
                        break;
                    case "Neighbourhood":
                        switch(text)
                        {
                            case "Moore":
                                config.Neighbourhood = ENeighbourhood.Moore;
                                break;
                            case "VonNeumann":
                                config.Neighbourhood = ENeighbourhood.vonNeumann;
                                break;
                        }
                        break;
                    case "BC":
                        switch (text)
                        {
                            case "Periodic":
                                config.BoundaryCondition = EBoundaryCondition.Periodic;
                                break;
                            default:
                                config.BoundaryCondition = EBoundaryCondition.Nonperiodic;
                                break;
                        }
                        break;
                }
            }

            config.SetSizes(sizeX, sizeY, sizeZ);
        }

        public string Serialize(Grain [,,] grains, string type, Config config)
        {
            int SizeX = config.SizeX;
            int SizeZ = config.SizeZ;
            int SizeY = config.SizeY;

            string structure = "";
            List<string> lines = new List<string>();

            if ( SizeZ > 1 && SizeX > 1 && SizeY > 1)
            {

                lines.Add("dim " + SizeX + " " + SizeY);

                for (int i = 0; i < SizeX; i++)
                {
                    for(int j = 0; j < SizeY; j++)
                    {
                        structure += grains[i, j, 0].State;
                        if (j != SizeY - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }

                lines.Add("dim " + SizeX + " " + SizeY);

                for (int i = 0; i < SizeX; i++)
                {
                    for (int j = 0; j < SizeY; j++)
                    {
                        structure += grains[i, j, SizeZ - 1].State;
                        if (j != SizeY - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }

                lines.Add("dim " + SizeY + " " + SizeZ);

                for (int i = 0; i < SizeY; i++)
                {
                    for (int j = 0; j < SizeZ; j++)
                    {
                        structure += grains[0, i, j].State;
                        if (j != SizeZ - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }
                
                lines.Add("dim " + SizeY + " " + SizeZ);

                for (int i = 0; i < SizeY; i++)
                {
                    for (int j = 0; j < SizeZ; j++)
                    {
                        structure += grains[SizeX - 1, i, j].State;
                        if (j != SizeZ - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }

                lines.Add("dim " + SizeZ + " " + SizeX);

                for (int i = 0; i < SizeZ; i++)
                {
                    for (int j = 0; j < SizeX; j++)
                    {
                        structure += grains[j, 0, i].State;
                        if (j != SizeX - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }

                lines.Add("dim " + SizeZ + " " + SizeX);

                for (int i = 0; i < SizeZ; i++)
                {
                    for (int j = 0; j < SizeX; j++)
                    {
                        structure += grains[j, SizeY - 1, i].State;
                        if (j != SizeX - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }
            }
            else
            {


                if (SizeX > 1 && SizeY > 1)
                {
                    lines.Add("dim " + SizeX + " " + SizeY);
                    for (int i = 0; i < SizeX; i++)
                    {
                        for (int j = 0; j < SizeY; j++)
                        {
                            structure += grains[i, j, 0].State;
                            if (j != SizeY - 1)
                                structure += " ";
                        }
                        lines.Add(structure);
                        structure = "";
                    }
                } else if (SizeX > 1 && SizeZ > 1)
                {
                    lines.Add("dim " + SizeX + " " + SizeZ );
                    for (int i = 0; i < SizeX; i++)
                    {
                        for (int j = 0; j < SizeZ; j++)
                        {
                            structure += grains[i, 0, j].State;
                            if (j != SizeZ - 1)
                                structure += " ";
                        }
                        lines.Add(structure);
                        structure = "";
                    }
                } else if (SizeY > 1 && SizeZ > 1)
                {
                    lines.Add("dim " + SizeY + " " + SizeZ );

                    for (int i = 0; i < SizeY; i++)
                    {
                        for (int j = 0; j < SizeZ; j++)
                        {
                            structure += grains[0, i, j].State;
                            if (j != SizeZ - 1)
                                structure += " ";
                        }
                        lines.Add(structure);
                        structure = "";
                    }
                }

            }


            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            string[] fileName = config.FileName.Split('.');
            fileName[0] += "_" + type + "_" + milliseconds.ToString();

            string filePath = config.FilePath + fileName[0] + "." + fileName[1];

            System.IO.File.AppendAllLines(filePath, lines);

            return filePath;
        }

        public string Serialize(List<string> walls, string type, Config config)
        {
            int SizeX = config.SizeX;
            int SizeZ = config.SizeZ;
            int SizeY = config.SizeY;

            List<string> lines = new List<string>();

            if (SizeZ > 1 && SizeX > 1 && SizeY > 1)
            {

                lines.Add("dim " + SizeX + " " + SizeY);
                lines.Add(walls[0]);

                lines.Add("dim " + SizeX + " " + SizeY);
                lines.Add(walls[1]);

                lines.Add("dim " + SizeY + " " + SizeZ);
                lines.Add(walls[2]);

                lines.Add("dim " + SizeY + " " + SizeZ);
                lines.Add(walls[3]);

                lines.Add("dim " + SizeZ + " " + SizeX);
                lines.Add(walls[4]);

                lines.Add("dim " + SizeZ + " " + SizeX);
                lines.Add(walls[5]);
            }
            else
            {


                if (SizeX > 1 && SizeY > 1)
                {
                    lines.Add("dim " + SizeX + " " + SizeY);
                    
                }
                else if (SizeX > 1 && SizeZ > 1)
                {
                    lines.Add("dim " + SizeX + " " + SizeZ);
                   
                }
                else if (SizeY > 1 && SizeZ > 1)
                {
                    lines.Add("dim " + SizeY + " " + SizeZ);
                }
                lines.Add(walls[0]);
            }

            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            string[] fileName = config.FileName.Split('.');
            fileName[0] += "_" + type + "_" + milliseconds.ToString();

            string filePath = config.FilePath + fileName[0] + "." + fileName[1];

            System.IO.File.AppendAllLines(filePath, lines);

            return filePath;
        }

        public string Serialize(List<System.IO.StreamReader> fileNames, Config config )
        {

            List<string[]> allList = new List<string[]>();


            foreach (System.IO.StreamReader reader in fileNames)
            {
                string[] all = reader.ReadToEnd().Split("*");
                allList.Add(all);
            }

            List<string> walls = new List<string>();

            for (int i = 0; i < allList[0].Length; i++)
            {
                if (i == 0 || i == 1)
                {
                    string tmp = "";

                    foreach (string[] s in allList)
                    {
                        tmp += s[i].TrimStart();
                    }

                    walls.Add(tmp.TrimEnd());
                }
                else if (i == 2 || i == 3)
                {
                    foreach (string[] s in allList)
                    {
                        if (!s[i].Contains("#"))
                        {
                            walls.Add(s[i].Trim());
                            break;
                        }
                    }

                }
                else
                {
                    string tmp = "";
                    int length = allList[0][i].TrimStart().Split("\n").Length;

                    for (int j = 0; j < length; j++)
                    {
                        int jj = 0;
                        foreach (string[] s in allList)
                        {
                            if (jj < allList.Count - 1)
                            {
                                tmp += s[i].TrimStart().Split("\n")[j].Trim() + " ";

                            }
                            else
                            {
                                tmp += s[i].TrimStart().Split("\n")[j].Trim();
                            }
                            jj++;
                        }
                        if (j < length - 1)
                            tmp += "\n";
                    }


                    walls.Add(tmp.TrimEnd());
                }
            }
            string filePath = Serialize(walls, config.Simulation.ToString(), config);
            return filePath;
        }

        public string Serialize(Grain[,,] grains, string type, Config config, 
            SimulationParams simulationParams, int rank, int commSize)
        {
            int SizeX = config.SizeX;
            int SizeZ = config.SizeZ;
            int SizeY = config.SizeY;

            string structure = "";
            List<string> lines = new List<string>();

            if (SizeZ > 1 && SizeX > 1 && SizeY > 1)
            {
                for (int i = 1; i < SizeX - 1; i++)
                {
                    for (int j = 0; j < SizeY; j++)
                    {
                        structure += grains[i, j, 0].State;
                        if (j != SizeY - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }

                lines.Add("*");

                for (int i = 1; i < SizeX - 1; i++)
                {
                    for (int j = 0; j < SizeY; j++)
                    {
                        structure += grains[i, j, SizeZ - 1].State;
                        if (j != SizeY - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }

                lines.Add("*");

                for (int i = 0; i < SizeY; i++)
                {
                    for (int j = 0; j < SizeZ; j++)
                    {
                        if (rank == 1)
                            structure += grains[1, i, j].State;
                        else
                            structure += "#";

                        if (j != SizeZ - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }

                lines.Add("*");

                for (int i = 0; i < SizeY; i++)
                {
                    for (int j = 0; j < SizeZ; j++)
                    {
                        if (rank == commSize - 1)
                            structure += grains[SizeX - 2, i, j].State;
                        else
                            structure += "#";

                        if (j != SizeZ - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }

                lines.Add("*");

                for (int i = 0; i < SizeZ; i++)
                {
                    for (int j = 1; j < SizeX - 1; j++)
                    {
                        structure += grains[j, 0, i].State;
                        if (j != SizeX - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }

                lines.Add("*");

                for (int i = 0; i < SizeZ; i++)
                {
                    for (int j = 1; j < SizeX - 1; j++)
                    {
                        structure += grains[j, SizeY - 1, i].State;
                        if (j != SizeX - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }
            }
            else
            {

                if (SizeX > 1 && SizeY > 1)
                {
                    for (int i = 1; i < SizeX - 1; i++)
                    {
                        for (int j = 0; j < SizeY; j++)
                        {
                            structure += grains[i, j, 0].State;
                            if (j != SizeY - 1)
                                structure += " ";
                        }
                        lines.Add(structure);
                        structure = "";
                    }
                }
                else if (SizeX > 1 && SizeZ > 1)
                {
                    for (int i = 1; i < SizeX - 1; i++)
                    {
                        for (int j = 0; j < SizeZ; j++)
                        {
                            structure += grains[i, 0, j].State;
                            if (j != SizeZ - 1)
                                structure += " ";
                        }
                        lines.Add(structure);
                        structure = "";
                    }
                }
                else if (SizeY > 1 && SizeZ > 1)
                {
                        
                }

            }


            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            string[] fileName = config.FileName.Split('.');
            fileName[0] = rank + "_" + fileName[0] + "_" + type + "_"  + milliseconds.ToString();

            string filePath = config.FilePath + fileName[0] + "." + fileName[1];

            System.IO.File.AppendAllLines(filePath, lines);

            return filePath;
        }

        public void Serialize(Grain[,,] grains, string type, Config config,
          SimulationParams simulationParams, int rank, int commSize, string filename)
        {
            int SizeX = config.SizeX;
            int SizeZ = config.SizeZ;
            int SizeY = config.SizeY;

            string structure = "";
            List<string> lines = new List<string>();

            if (SizeZ > 1 && SizeX > 1 && SizeY > 1)
            {
                for (int i = 1; i < SizeX - 1; i++)
                {
                    for (int j = 0; j < SizeY; j++)
                    {
                        structure += grains[i, j, 0].State;
                        if (j != SizeY - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }

                lines.Add("*");

                for (int i = 1; i < SizeX - 1; i++)
                {
                    for (int j = 0; j < SizeY; j++)
                    {
                        structure += grains[i, j, SizeZ - 1].State;
                        if (j != SizeY - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }

                lines.Add("*");

                for (int i = 0; i < SizeY; i++)
                {
                    for (int j = 0; j < SizeZ; j++)
                    {
                        if (rank == 1)
                            structure += grains[1, i, j].State;
                        else
                            structure += " ";

                        if (j != SizeZ - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }

                lines.Add("*");

                for (int i = 0; i < SizeY; i++)
                {
                    for (int j = 0; j < SizeZ; j++)
                    {
                        if (rank == commSize - 1)
                            structure += grains[SizeX - 2, i, j].State;
                        else
                            structure += " ";

                        if (j != SizeZ - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }

                lines.Add("*");

                for (int i = 0; i < SizeZ; i++)
                {
                    for (int j = 1; j < SizeX - 1; j++)
                    {
                        structure += grains[j, 0, i].State;
                        if (j != SizeX - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }

                lines.Add("*");

                for (int i = 0; i < SizeZ; i++)
                {
                    for (int j = 1; j < SizeX - 1; j++)
                    {
                        structure += grains[j, SizeY - 1, i].State;
                        if (j != SizeX - 1)
                            structure += " ";
                    }
                    lines.Add(structure);
                    structure = "";
                }
            }
            else
            {

                if (SizeX > 1 && SizeY > 1)
                {
                    for (int i = 1; i < SizeX - 1; i++)
                    {
                        for (int j = 0; j < SizeY; j++)
                        {
                            structure += grains[i, j, 0].State;
                            if (j != SizeY - 1)
                                structure += " ";
                        }
                        lines.Add(structure);
                        structure = "";
                    }
                }
                else if (SizeX > 1 && SizeZ > 1)
                {
                    for (int i = 1; i < SizeX - 1; i++)
                    {
                        for (int j = 0; j < SizeZ; j++)
                        {
                            structure += grains[i, 0, j].State;
                            if (j != SizeZ - 1)
                                structure += " ";
                        }
                        lines.Add(structure);
                        structure = "";
                    }
                }
                else if (SizeY > 1 && SizeZ > 1)
                {

                }

            }


            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            string[] fileName = config.FileName.Split('.');
            fileName[0] = rank + "_" + fileName[0] + "_" + type + "_" + milliseconds.ToString();

            string filePath = config.FilePath + filename + ".txt"; // fileName[0] + "." + fileName[1];
            lock (locker)
            {
                System.IO.File.WriteAllLines(filePath, lines);
            }
        }
        static object locker = new object();
    }

}
