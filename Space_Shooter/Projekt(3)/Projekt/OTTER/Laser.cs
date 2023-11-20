using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTTER
{
    class Laser : Sprite
    {                                      
        public Laser(string slika, int x, int y)
            : base(slika, x, y)
        {
            Ispucan = false;
        }
        private bool ispucan;
        public bool Ispucan
        {
            get { return ispucan; }
            set 
            {
                ispucan = value;
            }
        }       
        private bool aktivan;
        public bool Aktivan
        {
            get { return aktivan; }
            set { aktivan = value; }
        }
    }
}
