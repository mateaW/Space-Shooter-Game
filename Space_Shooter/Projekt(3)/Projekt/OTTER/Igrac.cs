using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTTER
{
    class Igrac : Sprite
    {
        private string rub;
        public string Rub
        {
            get { return rub; }
            set { rub = value; }
        }
        public override int X
        {
            get { return base.X; }
            set
            {
                if (value + this.Width > 700)
                {
                    base.x = 700 - this.Width;
                    this.Rub = "desno";
                }
                else if (value < 0)
                {
                    base.x = 0;
                    this.Rub = "lijevo";
                }
                else
                {
                    base.x = value;
                    this.Rub = "";
                }
            }
        }
        public Igrac(string pic, int x, int y)
            : base(pic, x, y)
        {
            Rub = "";
        }
    }
}
