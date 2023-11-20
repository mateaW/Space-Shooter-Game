using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTTER
{
    class Neprijatelj : Sprite
    {
        public Neprijatelj(string slika, int x, int y)
            : base(slika, x, y)
        {
                  
        }
        private bool aktivan;
        public bool Aktivan
        {
            get { return aktivan; }
            set { aktivan = value; }
        }
        public override int Y
        {
            get { return base.Y; }
            set
            {
                if (value > 800)
                {
                    this.y = 0;
                    this.Aktivan = false;
                    this.SetVisible(false);
                }
                else
                    base.Y = value;
            }
        }
    }
    
}
