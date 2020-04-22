using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WolvenKit.Render.Terrain
{
    class TileGroup
    {
        public string lod1 { get; set; }
        public string lod2 { get; set; }
        public string lod3 { get; set; }
        public int resolution { get; set; }

        public TileGroup() { }

        public override string ToString()
        {
            return "\tlod1: " + lod1 + "\n\tlod2: " + lod2 + "\n\tlod3: " + lod3 + "\nRESOLUTION:" + resolution;
        }

    }
}
