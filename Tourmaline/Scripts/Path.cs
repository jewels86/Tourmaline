using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tourmaline.Scripts
{
    public struct Path
    {
        public string URL;
        public int Status;
        public string Type;

        public override string ToString()
        {
            return $"{URL} ({Type} - {Status})";
        }
    }
}
