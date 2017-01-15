﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNSA_Unpacker.classes
{
    class BNSAFile
    {
        string filepath;

        public long TilesetStartPointer;
        public long PaletteStartPointer;
        public long ProbablyPaletteStartPointer = long.MaxValue; //will always force min

        public int AnimationCount = 0;
        public int LargestTileset = 0;

        public List<Animation> Animations = new List<Animation>();
        public List<Tileset> Tilesets = new List<Tileset>();
        public List<Palette> Palettes = new List<Palette>();

        public Boolean ValidBNSA = false;
        public BNSAFile(string path)
        {
            using (FileStream bnsaStream = File.OpenRead(path))
            {
                //HEADER
                LargestTileset = bnsaStream.ReadByte();
                //Magic Number Test
                if (bnsaStream.ReadByte() != 0 || bnsaStream.ReadByte() != 1) //0x1 and 0x2 positions
                {
                    //Invalid Magic Number
                    ValidBNSA = false;
                    return;
                }
                AnimationCount = bnsaStream.ReadByte();
                Console.WriteLine("Number of Animations: " + AnimationCount);
                //Read Animation Pointers
                for (int i = 0; i < AnimationCount; i++)
                {
                    int animationPointer = ReadIntegerFromStream(bnsaStream);
                    long nextPosition = bnsaStream.Position;
                    Animation animation = new Animation(this,animationPointer, bnsaStream);
                    Animations.Add(animation);
                    if (i < AnimationCount - 1)
                    {
                        bnsaStream.Seek(nextPosition, SeekOrigin.Begin); //reset position to next pointer
                    }
                }

                //Read Tilesets
                TilesetStartPointer = bnsaStream.Position;
                Console.WriteLine("Reading Tilesets, starting at 0x" + TilesetStartPointer.ToString("X6"));
                while (bnsaStream.Position < ProbablyPaletteStartPointer)
                {
                    //Read Tilesets
                    Tileset ts = new Tileset(bnsaStream);
                    Tilesets.Add(ts); //Might need some extra checking...
                }

                //Read Palettes
                PaletteStartPointer = bnsaStream.Position;
                Console.WriteLine("Found start of Palettes at 0x" + PaletteStartPointer.ToString("X6"));
                if (ReadIntegerFromStream(bnsaStream) == 0x20)
                {
                    while (true)
                    {
                        long pos = bnsaStream.Position;

                        //Verify next item is a palette...
                        if (ReadIntegerFromStream(bnsaStream) == 0x4 && bnsaStream.ReadByte() == 0x0 && bnsaStream.ReadByte() == 0x01 && bnsaStream.ReadByte() == 0x80)
                        {
                            bnsaStream.Seek(-0x7, SeekOrigin.Current);
                            break; //Not a palette. This code needs to be improved to account for mugshots.
                        }
                        bnsaStream.Seek(pos, SeekOrigin.Begin);
                        Console.WriteLine("Reading Palette 0x" + bnsaStream.Position.ToString("X6"));
                        Palette palette = new Palette(bnsaStream);
                        Palettes.Add(palette);
                        //Console.WriteLine("Reading next Palette 0x" + bnsaStream.Position.ToString("X6"));

                    }


                    //Read Palette - Reading will move position back 4 so it reads the full palette and advance to the next size position.

                }//} else
                 //{
                 //    Console.WriteLine("Palettes should start here, but we didn't find 0x00000020! (At position  0x" + PaletteStartPointer.ToString("X6") + ")");
                 //    return;
                 //}

                //Read Mini-Animations and Object Lists
                Console.WriteLine("Reading MiniAnim Data at " + bnsaStream.Position.ToString("X6"));

            }
        }

        /// <summary>
        /// Reads a 32-bit integer value from the stream, advancing it 4 bytes forward.
        /// </summary>
        /// <param name="stream">Stream to read</param>
        /// <returns>Integer</returns>
        public static int ReadIntegerFromStream(FileStream stream)
        {
            byte[] pointer = new byte[4]; //32-bit pointer
            stream.Read(pointer, 0, 4);
            return BitConverter.ToInt32(pointer, 0);
        }

        //public int findTilesetDataEnd(FileStream stream)
        //{
        //    while (true)
        //    {
        //        if (verifyValidTilesetSize(archiveFile, position) == true)
        //        {
        //            int size = BitConverter.ToInt32(archiveFile, position);
        //            position += size + 4;
        //            //if (size == 0x20)
        //            //{
        //            //    endOffset = position;
        //            //}
        //        }
        //        else
        //        {
        //            endOffset = position;
        //        }

        //    }

        //    return endOffset;
        //}
    }
}