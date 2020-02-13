using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WolvenKit.Cache
{
    class WitcherShaderCache
    {
        public byte[] mw = { (byte)'R', (byte)'D', (byte)'H', (byte)'S' };
        public int version;

        public List<Shader> shaders = new List<Shader>();
    }

    class Shader
    {
        byte[] Contents;
    }

    class ShaderCache
    {
        [STAThread]
        public static void Main()
        {
            Console.WriteLine("The Witcher 3: Wild Hunt - ShaderCache tool by Traderain v0.1");
            Console.Title = "ShaderCache tool by Traderain v0.1";
            using(var of = new OpenFileDialog())
            {
                of.InitialDirectory = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\The Witcher 3\\content";
                of.Filter = "Shader Cache | *.cache";


                if(of.ShowDialog() == DialogResult.OK)
                {
                    using (var br = new BinaryReader(new FileStream(of.FileName, FileMode.Open)))
                    {
                        WitcherShaderCache sc = new WitcherShaderCache();
                        br.BaseStream.Seek(-8, SeekOrigin.End);
                        var mw = br.ReadBytes(4);
                        int ver = br.ReadInt32();
                        Console.WriteLine("Magic => \"" + Encoding.ASCII.GetString(mw) + "\"");
                        Console.WriteLine("Version: " + ver);
                        if(ver != 2)
                            Console.WriteLine("Version 3 not supported yet!");
                        br.BaseStream.Seek(-0x1C, SeekOrigin.End);
                        var shadercount = br.ReadInt32();
                        br.BaseStream.Seek(0, SeekOrigin.Begin);
                        for(int i = 0; i < shadercount; i++)
                        {
                            var shad = new Shader();
                            br.ReadInt32();
                            br.ReadInt32();
                            br.ReadInt32();
                            br.ReadInt32();
                            var len = br.ReadInt32();
                           
                            var shader = br.ReadBytes(len);
                            Console.WriteLine("Cache file => " + shader.Length + "bytes");
                            sc.shaders.Add(shad);
                        }
                        Console.WriteLine("File count: " + shadercount);
                        Console.WriteLine("Current: " + br.BaseStream.Position);
                        Console.WriteLine("Size: " + br.BaseStream.Length);
                        Console.WriteLine("Remaining bytes: " + (br.BaseStream.Length - br.BaseStream.Position) + "bytes");
                        Console.ReadKey();
                    }
                }
            }
        }
    }
}
