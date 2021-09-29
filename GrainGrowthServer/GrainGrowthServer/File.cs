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
                    lines.Add("dim " + SizeX + " " + SizeY );
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
                    lines.Add("dim " + SizeX + " " + SizeY );

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
    }
}
