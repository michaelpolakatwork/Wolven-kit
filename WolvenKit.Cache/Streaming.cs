using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WolvenKit.CR2W;

namespace WolvenKit.Cache
{
    class Streaming
    {
        public static void Main()
        {
            var filenames = File.ReadAllLines(@"D:\Repos\Wolven-kit\WolvenKit\bin\Debug\ManagerCache\pathhashes.csv").Select(x => x.Split(',')).ToList();
            var of = new FileStream(@"D:\SteamLibrary\steamapps\common\The Witcher 3\DLC\bob\content\streaming.cache", FileMode.Open);
            var br = new BinaryReader(of);
            Console.WriteLine("Reading streaming.cache");
            var magic = br.ReadUInt32();
            var size = br.ReadSingle();
            Console.WriteLine("Size: " + size);
            var cellc = br.ReadUInt32();
            var cellsum = br.ReadUInt32();
            Console.WriteLine("Summary of cell's cellcount: " + cellsum);
            Console.WriteLine("Cell count: " + cellc);
            List<Cell> cells = new List<Cell>();
            for (int i = 0; i < cellc; i++)
            {
                var c = new Cell();
                c.Read(br);
                cells.Add(c);
                Console.WriteLine($"Cell [{i}] =>" + c.ToString());
            }

            Console.WriteLine("Max cell -> " + cells.First(y => y.offset == cells.Max(z => z.offset)).ToString());

            Console.ReadKey();

            foreach(var cell in cells)
            {
                var ent = new CellEntry();
                ent.Read(br);
                Console.WriteLine("\t" + ent.ToString());
                var reff = filenames.Where(y => y[1] == ent.resourcehash.ToString()).ToArray()[0][0];
                Console.WriteLine("\t\tReferenced file --> " + reff);
                cell.entries.Add(ent);
            }

            if(br.BaseStream.Position == br.BaseStream.Length)
            {
                Console.WriteLine("We reached the end of the file :) length==pos");
            }
            else
            {
                Console.WriteLine($"File read {br.BaseStream.Position}/{br.BaseStream.Length}!");
            }
            Console.ReadKey();

        }
    }

    class Cell
    {
        public UInt32 level;
        public UInt32 x;
        public UInt32 y;
        public UInt32 first;
        public UInt32 count;
        public UInt32 size;
        public UInt64 offset;

        public List<CellEntry> entries = new List<CellEntry>();

        public void Read(BinaryReader br)
        {
            level = br.ReadUInt32();
            x = br.ReadUInt32();
            y = br.ReadUInt32();
            first = br.ReadUInt32();
            count = br.ReadUInt32();
            size = br.ReadUInt32();
            offset = br.ReadUInt64();
        }

        public override string ToString()
        {
            return $"\tLevel - {level}\tX[{x}]\tY[{y}]\tFirst - ({first})\tCount - ({count})\tSize - ({size})\tOffset - ({offset})";
        }
    }

    class CellEntry
    {
        public UInt32 dataoffset;
        public UInt32 datasize;
        public UInt32 uncompressedsize;
        public UInt16 compressiontype;
        public UInt16 cellindex;
        public UInt64 resourcehash;

        public void Read(BinaryReader br)
        {
            dataoffset = br.ReadUInt32();
            datasize = br.ReadUInt32();
            uncompressedsize = br.ReadUInt32();
            compressiontype = br.ReadUInt16();
            cellindex = br.ReadUInt16();
            resourcehash = br.ReadUInt64();
        }

        public override string ToString()
        {
            return $"\tDataoffset:{dataoffset}\tDatasize:{datasize}\tUncompressedsize:{uncompressedsize}\tComtype:{compressiontype}\tCellindex:{cellindex}\tHash:{resourcehash}";
        }
    }
}
