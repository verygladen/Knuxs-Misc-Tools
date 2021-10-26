﻿// Data structure taken from: http://info.sonicretro.org/Shadow_the_Hedgehog_(game)/Technical_information/Object_layout_format

using Marathon.IO;
using Marathon.Formats.Placement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Knux_s_Misc_Tools
{
    public class ShadowSET
    {
        /// <summary>
        /// Data structure of a Shadow The Hedgehog object.
        /// </summary>
        public class SETObject
        {
            // X, Y, Z Position of this object.
            public Vector3 Position { get; set; }
            
            // X, Y, Z Rotation of this object, in euler angles.
            public Vector3 Rotation { get; set; }

            public byte UnknownByte1 { get; set; }

            public byte UnknownByte2 { get; set; }

            public byte UnknownByte3 { get; set; }

            public byte UnknownByte4 { get; set; }

            public byte[] UnknownByteArray { get; set; }

            // Object Type in setid.bin.
            public byte ObjectType { get; set; }

            // Object List this object is in in setid.bin.
            public byte ObjectList { get; set; }

            public byte LinkID { get; set; }

            public byte RenderDistance { get; set; }

            // Parameters
            public List<int> MiscData = new();

        }

        // List of Objects.
        public List<SETObject> Objects = new();

        /// <summary>
        /// Loads a Shadow The Hedgehog SET file.
        /// </summary>
        /// <param name="filepath">File to load.</param>
        public void Load(string filepath)
        {
            // Set up the Extended Binary Reader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read this SETs header.
            reader.ReadSignature(4, "sky2");
            int objectCount   = reader.ReadInt32();
            int miscLength    = reader.ReadInt32();
            long miscPosition = 0xC + (objectCount * 0x2C);

            // Loop through based on the amount of objects in this file.
            for (int i = 0; i < objectCount; i++)
            {
                // Read this object.
                SETObject obj = new()
                {
                    Position         = reader.ReadVector3(),
                    Rotation         = reader.ReadVector3(),
                    UnknownByte1     = reader.ReadByte(),
                    UnknownByte2     = reader.ReadByte(),
                    UnknownByte3     = reader.ReadByte(),
                    UnknownByte4     = reader.ReadByte(),
                    UnknownByteArray = reader.ReadBytes(0x4),
                    ObjectType       = reader.ReadByte(),
                    ObjectList       = reader.ReadByte(),
                    LinkID           = reader.ReadByte(),
                    RenderDistance   = reader.ReadByte()
                };
                int objMiscLength = reader.ReadInt32();
                reader.JumpAhead(0x4);

                // Save position for the next object.
                long pos = reader.BaseStream.Position;

                // Jump to the value in miscPosition
                reader.JumpTo(miscPosition);

                // Read the amount of parameters specified in this object.
                for (int m = 0; m < objMiscLength / 4; m++)
                    obj.MiscData.Add(reader.ReadInt32());

                // Save the position as the new miscPosition value.
                miscPosition = reader.BaseStream.Position;

                // Jump back to the save position.
                reader.JumpTo(pos);

                // Save this object.
                Objects.Add(obj);
            }
        }

        /// <summary>
        /// Exports data in the Objects List to a Sonic '06 SET.
        /// </summary>
        /// <param name="filepath">The path to save the SET to.</param>
        public void Export06(string filepath)
        {
            // Create the new Sonic '06 SET.
            ObjectPlacement set = new();
            set.Data.Name = "test";

            // Loop through the objects.
            for (int i = 0; i < Objects.Count; i++)
            {
                #region Generic Shit
                // Spring
                if (Objects[i].ObjectType == 0x01 && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"spring{i}",
                        Type = "spring",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter(3000f, typeof(float)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0.5f, typeof(float)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(4294967295, typeof(uint)));

                    set.Data.Objects.Add(s06obj);
                }
                
                // Wide Spring
                if (Objects[i].ObjectType == 0x02 && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"widespring{i}",
                        Type = "widespring",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter(3000f, typeof(float)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0.5f, typeof(float)));

                    set.Data.Objects.Add(s06obj);
                }
                
                // Dash Ramp
                if (Objects[i].ObjectType == 0x04 && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"jumppanel{i}",
                        Type = "jumppanel",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y, 180)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter(20f, typeof(float)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(3000f, typeof(float)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0.5f, typeof(float)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(4294967295, typeof(uint)));

                    set.Data.Objects.Add(s06obj);
                }
                
                // Checkpoint
                if (Objects[i].ObjectType == 0x05 && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"savepoint{i}",
                        Type = "savepoint",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y, 180)
                    };

                    set.Data.Objects.Add(s06obj);
                }
                
                // Dash Ring
                if (Objects[i].ObjectType == 0x06 && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"common_dashring{i}",
                        Type = "common_dashring",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y, 180)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter(10f, typeof(float)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0f, typeof(float)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(3000f, typeof(float)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0.5f, typeof(float)));

                    set.Data.Objects.Add(s06obj);
                }
                
                // Locked Case
                if (Objects[i].ObjectType == 0x07 && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"common_cage{i}",
                        Type = "common_cage",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    // Can be scaled in ShTH, but I don't know if I want to do that.
                    // Also could be scaled on each axis rather than just one like in '06.
                    s06obj.Parameters.Add(Helpers.Add06Parameter(1f, typeof(float)));

                    set.Data.Objects.Add(s06obj);
                }
                
                // Pulley
                if (Objects[i].ObjectType == 0x08 && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"updownreel{i}",
                        Type = "updownreel",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    byte[] floatVals = BitConverter.GetBytes(Objects[i].MiscData[0]);
                    float f = BitConverter.ToSingle(floatVals, 0);
                    s06obj.Parameters.Add(Helpers.Add06Parameter(-(f * 100), typeof(float)));

                    floatVals = BitConverter.GetBytes(Objects[i].MiscData[1]);
                    f = BitConverter.ToSingle(floatVals, 0);
                    s06obj.Parameters.Add(Helpers.Add06Parameter(-(f * 100), typeof(float)));

                    s06obj.Parameters.Add(Helpers.Add06Parameter(1.5f, typeof(float)));

                    set.Data.Objects.Add(s06obj);
                }
                
                // Wood Box
                if (Objects[i].ObjectType == 0x09 && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"objectphysics{i}",
                        Type = "objectphysics",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter("WoodBox", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(false, typeof(bool)));

                    set.Data.Objects.Add(s06obj);
                }

                // Metal Box
                if (Objects[i].ObjectType == 0x0A && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"objectphysics{i}",
                        Type = "objectphysics",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter("IronBox", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(false, typeof(bool)));

                    set.Data.Objects.Add(s06obj);
                }

                // Weapon Box
                if (Objects[i].ObjectType == 0x0C && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"objectphysics{i}",
                        Type = "objectphysics",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter("FlashBox", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(false, typeof(bool)));

                    set.Data.Objects.Add(s06obj);
                }

                // GUN Bomb
                if (Objects[i].ObjectType == 0x0D && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"objectphysics{i}",
                        Type = "objectphysics",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter("BombBox", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(false, typeof(bool)));

                    set.Data.Objects.Add(s06obj);
                }
                
                // Rings
                if (Objects[i].ObjectType == 0x10 && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"ring{i}",
                        Type = "ring",
                        Position = new(Objects[i].Position.X * 10, (Objects[i].Position.Y * 10) - 40, Objects[i].Position.Z * 10),
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter(true, typeof(bool)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0f, typeof(float)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter("", typeof(string)));

                    set.Data.Objects.Add(s06obj);
                }
                
                // Item Capsule
                if (Objects[i].ObjectType == 0x12 && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"itemboxa{i}",
                        Type = "itemboxa",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };
                    
                    switch (Objects[i].MiscData[0])
                    {
                        case 0: s06obj.Parameters.Add(Helpers.Add06Parameter(1, typeof(int))); break;
                        case 1: s06obj.Parameters.Add(Helpers.Add06Parameter(2, typeof(int))); break;
                        case 2: s06obj.Parameters.Add(Helpers.Add06Parameter(3, typeof(int))); break;
                        case 3: s06obj.Parameters.Add(Helpers.Add06Parameter(8, typeof(int))); break;
                        case 4: s06obj.Parameters.Add(Helpers.Add06Parameter(8, typeof(int))); break;
                        case 5: s06obj.Parameters.Add(Helpers.Add06Parameter(8, typeof(int))); break;
                        case 6: s06obj.Parameters.Add(Helpers.Add06Parameter(7, typeof(int))); break;
                        case 8: s06obj.Parameters.Add(Helpers.Add06Parameter(4, typeof(int))); break;
                        default: s06obj.Parameters.Add(Helpers.Add06Parameter(6, typeof(int))); break;
                    }
                    
                    set.Data.Objects.Add(s06obj);
                }
                
                // Roadblock
                if (Objects[i].ObjectType == 0x1B && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"objectphysics{i}",
                        Type = "objectphysics",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y + 90)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter("twn_barricade", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(false, typeof(bool)));

                    set.Data.Objects.Add(s06obj);
                }
                
                // Secret Key
                if (Objects[i].ObjectType == 0x1D && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"medal_of_royal_bronze{i}",
                        Type = "medal_of_royal_bronze",
                        Position = new(Objects[i].Position.X * 10, (Objects[i].Position.Y * 10) + 40, Objects[i].Position.Z * 10),
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    set.Data.Objects.Add(s06obj);
                }

                // Killer Plant
                if (Objects[i].ObjectType == 0x31 && Objects[i].ObjectList == 0x11)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"enemy{i}",
                        Type = "enemy",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter("cGazer", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(1, typeof(int)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter("cGazer_Fix", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0, typeof(float)));

                    set.Data.Objects.Add(s06obj);
                }

                // Energy Core
                if (Objects[i].ObjectType == 0x33 && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"itemboxa{i}",
                        Type = "itemboxa",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter(6, typeof(int)));

                    set.Data.Objects.Add(s06obj);
                }
                
                // Weapon Box
                if (Objects[i].ObjectType == 0x3A && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"objectphysics{i}",
                        Type = "objectphysics",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter("FlashBox", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(false, typeof(bool)));

                    set.Data.Objects.Add(s06obj);
                }
                
                // Vehicle
                if (Objects[i].ObjectType == 0x4F && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"vehicle{i}",
                        Type = "vehicle",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    // TODO: Replace the first parameter with a switch for other vehicle types.
                    s06obj.Parameters.Add(Helpers.Add06Parameter(3, typeof(int)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter("", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(false, typeof(bool)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(false, typeof(bool)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter("", typeof(string)));

                    set.Data.Objects.Add(s06obj);
                }
                
                // GUN Beetle
                if (Objects[i].ObjectType == 0x65 && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"enemy{i}",
                        Type = "enemy",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter("eFlyer", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0, typeof(int)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter("eFlyer_Fix", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0, typeof(float)));

                    set.Data.Objects.Add(s06obj);
                }
                
                // GUN Robot
                if (Objects[i].ObjectType == 0x68 && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"enemy{i}",
                        Type = "enemy",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter("eGunner", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0, typeof(int)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter("eGunner_Fix", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0, typeof(float)));

                    set.Data.Objects.Add(s06obj);
                }

                // Black Arms Warrior
                if (Objects[i].ObjectType == 0x8D && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"enemy{i}",
                        Type = "enemy",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter("cBiter", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0, typeof(int)));
                    if (Objects[i].MiscData[9] == 0)
                        s06obj.Parameters.Add(Helpers.Add06Parameter("cBiter_Fix", typeof(string)));
                    else
                        s06obj.Parameters.Add(Helpers.Add06Parameter("cBiter_Normal", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0, typeof(float)));

                    set.Data.Objects.Add(s06obj);
                }

                // Black Arms Worm
                if (Objects[i].ObjectType == 0x90 && Objects[i].ObjectList == 0x00)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"enemy{i}",
                        Type = "enemy",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y)
                    };

                    s06obj.Parameters.Add(Helpers.Add06Parameter("cCrawler", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0, typeof(int)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter("cCrawler_Normal", typeof(string)));
                    s06obj.Parameters.Add(Helpers.Add06Parameter(0, typeof(float)));

                    set.Data.Objects.Add(s06obj);
                }
                #endregion

                #region Stuff for Prison Island
                if (Objects[i].ObjectType == 0x88 && Objects[i].ObjectList == 0x25)
                {
                    SetObject s06obj = new()
                    {
                        Name = $"common_path_obj{i}",
                        Type = "common_path_obj",
                        Position = Objects[i].Position * 10,
                        Rotation = Helpers.ConvertToQuat(Objects[i].Rotation.Y),
                        DrawDistance = 999999
                    };
                    SetParameter s06param;

                    string type = "unhandled";
                    switch (Objects[i].MiscData[0])
                    {
                        case 0:
                            type = "JG0301DGPASS01";
                            break;

                        case 1:
                            type = "JG0301DGPASS02";
                            break;

                        case 2:
                            type = "JG0301DRAIN01";
                            if (Objects[i].MiscData[1] == 0x40400000)
                                type = "JG0301DRAIN01_3x";
                            break;

                        case 3:
                            type = "JG0301RIVER01D";
                            break;

                        case 4:
                            type = "JG0301RIVER02D";
                            break;

                        case 5:
                            type = "JG0301RIVER03D";
                            break;

                        case 6:
                            type = "JG0301RIVER04D";
                            break;

                        case 7:
                            type = "JG0301RIVER05D";
                            break;

                        case 8:
                            type = "JG0301RIVER06D";
                            break;

                        case 9:
                            type = "JG0301WALLPLATE";
                            if (Objects[i].MiscData[1] == 0x3F999998)
                                type = "JG0301WALLPLATE_1_2x";
                            if (Objects[i].MiscData[1] == 0x3FA66664)
                                type = "JG0301WALLPLATE_1_3x";
                            break;

                        default:
                            continue;
                    }


                    s06param = new()
                    {
                        Data = type,
                        DataType = typeof(string)
                    };
                    s06obj.Parameters.Add(s06param);

                    s06param = new()
                    {
                        Data = "",
                        DataType = typeof(string)
                    };
                    s06obj.Parameters.Add(s06param);

                    s06param = new()
                    {
                        Data = 0f,
                        DataType = typeof(float)
                    };
                    s06obj.Parameters.Add(s06param);

                    s06param = new()
                    {
                        Data = 0f,
                        DataType = typeof(float)
                    };
                    s06obj.Parameters.Add(s06param);

                    set.Data.Objects.Add(s06obj);
                }
                #endregion
            }

            // Save the SET to the specified path.
            set.Save(filepath);
        }
    
        /// <summary>
        /// Hardcoded thing to allow me to create MaxScripts for easy importing of props into a Max Scene.
        /// </summary>
        /// <param name="filepath">The path to save the MaxScript to.</param>
        /// <param name="ObjectType">The object type to check against.</param>
        /// <param name="ObjectList">The object list to check against.</param>
        public void ExportMAXScript(string filepath, int ObjectType, int ObjectList)
        {
            // Delete the MaxScript if it exists, as I append to it.
            File.Delete(filepath);

            // Setup a value so I can number the objects linearlly.
            int objectNumber = 0;

            // Loop through the objects.
            foreach (var obj in Objects)
            {
                if (obj.ObjectType == ObjectType && obj.ObjectList == ObjectList)
                {
                    if (obj.MiscData[0] == 4)
                    {
                        // Setup a StreamWriter.
                        using (StreamWriter log = new(filepath, append: true))
                        {
                            /* Info on how this setup works.
                            
                            The Max file that the script will merge, set to use merged material duplicates.
                            log.WriteLine($"mergeMAXFile \"[.MAX FILE NAME GOES HERE]\" #useMergedMtlDups");

                            The mesh in the file to select for renaming and positioning. Every line from here on needs to be duped.
                            log.WriteLine("a = $[MESHNAME GOES HERE]");

                            The name I change the selected mesh to. Often, I append _{objectNumber} to count them.
                            log.WriteLine($"a.Name = \"[NEW MESHNAME GOES HERE]\"");

                            The rotation of the mesh, X gets reduced by 90 as I tend to have weird rotation values.
                            log.WriteLine("a.rotation = eulerAngles " + (obj.Rotation.X - 90) + " " + -obj.Rotation.Y + " " + obj.Rotation.Z);

                            The X Position of the mesh, I mutiply them by 10 due to differences in scale between '06 and ShTH.
                            log.WriteLine("a.pos.x = " + obj.Position.X * 10);
                            
                            The Y Position of the mesh, this uses the Z position inverted due to Y-Up VS Z-Up differences.
                            log.WriteLine("a.pos.y = " + -(obj.Position.Z * 10));
                            
                            The Z Position of the mesh, this uses the Y position due to Y-Up VS Z-Up differences.
                            log.WriteLine("a.pos.z = " + obj.Position.Y * 10);

                            */

                            log.WriteLine($"mergeMAXFile \"C:\\Users\\Knuxf\\Documents\\3dsMax\\scenes\\Prison Island Props\\JG0000FOOTRIVERGNFH_coli.max\" #useMergedMtlDups");

                            // Mesh
                            //log.WriteLine("a = $Dummy002");
                            //log.WriteLine($"a.Name = \"JG0000FOOTRIVERGNFH_{objectNumber}\"");
                            //log.WriteLine("a.rotation = eulerAngles " + (obj.Rotation.X - 90) + " " + -obj.Rotation.Y + " " + obj.Rotation.Z);
                            //log.WriteLine("a.pos.x = " + obj.Position.X * 10);
                            //log.WriteLine("a.pos.y = " + -(obj.Position.Z * 10));
                            //log.WriteLine("a.pos.z = " + obj.Position.Y * 10);

                            // Collision
                            log.WriteLine("a = $floorcoli_at_3");
                            log.WriteLine($"a.Name = \"JG0000FOOTRIVERGNFH_{objectNumber}_floorcoli_at_3\"");
                            log.WriteLine("a.rotation = eulerAngles " + (obj.Rotation.X - 90) + " " + -obj.Rotation.Y + " " + obj.Rotation.Z);
                            log.WriteLine("a.pos.x = " + obj.Position.X * 10);
                            log.WriteLine("a.pos.y = " + -(obj.Position.Z * 10));
                            log.WriteLine("a.pos.z = " + obj.Position.Y * 10);
                            log.WriteLine("a = $wallcoli_at_40003");
                            log.WriteLine($"a.Name = \"JG0000FOOTRIVERGNFH_{objectNumber}_wallcoli_at_40003\"");
                            log.WriteLine("a.rotation = eulerAngles " + (obj.Rotation.X - 90) + " " + -obj.Rotation.Y + " " + obj.Rotation.Z);
                            log.WriteLine("a.pos.x = " + obj.Position.X * 10);
                            log.WriteLine("a.pos.y = " + -(obj.Position.Z * 10));
                            log.WriteLine("a.pos.z = " + obj.Position.Y * 10);
                        }

                        // Increment the objectNumber count.
                        objectNumber++;
                    }
                }
            }
        }
    }
}
