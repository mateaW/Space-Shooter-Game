using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OTTER
{
    /// <summary>
    /// -
    /// </summary>
    public partial class BGL : Form
    {
        public Form frmIzbornik;
        private string player;
        public string Player
        {
            get { return player; }
            set
            {
                if (value == "")
                    player = "Nepoznat";
                else
                    player = value;
            }
        }
        /* ------------------- */
        #region Environment Variables

        List<Func<int>> GreenFlagScripts = new List<Func<int>>();

        /// <summary>
        /// Uvjet izvršavanja igre. Ako je <c>START == true</c> igra će se izvršavati.
        /// </summary>
        /// <example><c>START</c> se često koristi za beskonačnu petlju. Primjer metode/skripte:
        /// <code>
        /// private int MojaMetoda()
        /// {
        ///     while(START)
        ///     {
        ///       //ovdje ide kod
        ///     }
        ///     return 0;
        /// }</code>
        /// </example>
        public static bool START = true;

        //sprites
        /// <summary>
        /// Broj likova.
        /// </summary>
        public static int spriteCount = 0, soundCount = 0;

        /// <summary>
        /// Lista svih likova.
        /// </summary>
        //public static List<Sprite> allSprites = new List<Sprite>();
        public static SpriteList<Sprite> allSprites = new SpriteList<Sprite>();

        //sensing
        int mouseX, mouseY;
        Sensing sensing = new Sensing();

        //background
        List<string> backgroundImages = new List<string>();
        int backgroundImageIndex = 0;
        string ISPIS = "";

        SoundPlayer[] sounds = new SoundPlayer[1000];
        TextReader[] readFiles = new StreamReader[1000];
        TextWriter[] writeFiles = new StreamWriter[1000];
        bool showSync = false;
        int loopcount;
        DateTime dt = new DateTime();
        String time;
        double lastTime, thisTime, diff;

        #endregion
        /* ------------------- */
        #region Events

        private void Draw(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            try
            {                
                foreach (Sprite sprite in allSprites)
                {                    
                    if (sprite != null)
                        if (sprite.Show == true)
                        {
                            g.DrawImage(sprite.CurrentCostume, new Rectangle(sprite.X, sprite.Y, sprite.Width, sprite.Heigth));
                        }
                    if (allSprites.Change)
                        break;
                }
                if (allSprites.Change)
                    allSprites.Change = false;
            }
            catch
            {
                //ako se doda sprite dok crta onda se mijenja allSprites
                MessageBox.Show("Greška!");
            }
        }

        private void startTimer(object sender, EventArgs e)
        {
            this.frmIzbornik.Hide();
            timer1.Start();
            timer2.Start();
            Init();
        }

        private void updateFrameRate(object sender, EventArgs e)
        {
            updateSyncRate();
        }

        /// <summary>
        /// Crta tekst po pozornici.
        /// </summary>
        /// <param name="sender">-</param>
        /// <param name="e">-</param>
        public void DrawTextOnScreen(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            var brush = new SolidBrush(Color.WhiteSmoke);
            string text = ISPIS;

            SizeF stringSize = new SizeF();
            Font stringFont = new Font("Arial", 14);
            stringSize = e.Graphics.MeasureString(text, stringFont);

            using (Font font1 = stringFont)
            {
                RectangleF rectF1 = new RectangleF(0, 0, stringSize.Width, stringSize.Height);
                e.Graphics.FillRectangle(brush, Rectangle.Round(rectF1));
                e.Graphics.DrawString(text, font1, Brushes.Black, rectF1);
            }
        }

        private void mouseClicked(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = true;
            sensing.MouseDown = true;
        }

        private void mouseDown(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = true;
            sensing.MouseDown = true;            
        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = false;
            sensing.MouseDown = false;
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;

            //sensing.MouseX = e.X;
            //sensing.MouseY = e.Y;
            //Sensing.Mouse.x = e.X;
            //Sensing.Mouse.y = e.Y;
            sensing.Mouse.X = e.X;
            sensing.Mouse.Y = e.Y;

        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            sensing.Key = e.KeyCode.ToString();
            sensing.KeyPressedTest = true;
        }

        private void keyUp(object sender, KeyEventArgs e)
        {
            sensing.Key = "";
            sensing.KeyPressedTest = false;
        }

        private void Update(object sender, EventArgs e)
        {
            if (sensing.KeyPressed(Keys.Escape))
            {
                START = false;
            }

            if (START)
            {
                this.Refresh();
            }
        }

        #endregion
        /* ------------------- */
        #region Start of Game Methods

        //my
        #region my

        //private void StartScriptAndWait(Func<int> scriptName)
        //{
        //    Task t = Task.Factory.StartNew(scriptName);
        //    t.Wait();
        //}

        //private void StartScript(Func<int> scriptName)
        //{
        //    Task t;
        //    t = Task.Factory.StartNew(scriptName);
        //}

        private int AnimateBackground(int intervalMS)
        {
            while (START)
            {
                setBackgroundPicture(backgroundImages[backgroundImageIndex]);
                Game.WaitMS(intervalMS);
                backgroundImageIndex++;
                if (backgroundImageIndex == 3)
                    backgroundImageIndex = 0;
            }
            return 0;
        }

        private void KlikNaZastavicu()
        {
            foreach (Func<int> f in GreenFlagScripts)
            {
                Task.Factory.StartNew(f);
            }
        }

        #endregion

        /// <summary>
        /// BGL
        /// </summary>
        public BGL()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Pričekaj (pauza) u sekundama.
        /// </summary>
        /// <example>Pričekaj pola sekunde: <code>Wait(0.5);</code></example>
        /// <param name="sekunde">Realan broj.</param>
        public void Wait(double sekunde)
        {
            int ms = (int)(sekunde * 1000);
            Thread.Sleep(ms);
        }

        //private int SlucajanBroj(int min, int max)
        //{
        //    Random r = new Random();
        //    int br = r.Next(min, max + 1);
        //    return br;
        //}

        /// <summary>
        /// -
        /// </summary>
        public void Init()
        {
            if (dt == null) time = dt.TimeOfDay.ToString();
            loopcount++;
            //Load resources and level here
            this.Paint += new PaintEventHandler(DrawTextOnScreen);
            SetupGame();
        }

        /// <summary>
        /// -
        /// </summary>
        /// <param name="val">-</param>
        public void showSyncRate(bool val)
        {
            showSync = val;
            if (val == true) syncRate.Show();
            if (val == false) syncRate.Hide();
        }

        /// <summary>
        /// -
        /// </summary>
        public void updateSyncRate()
        {
            if (showSync == true)
            {
                thisTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                diff = thisTime - lastTime;
                lastTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

                double fr = (1000 / diff) / 1000;

                int fr2 = Convert.ToInt32(fr);

                syncRate.Text = fr2.ToString();
            }

        }

        //stage
        #region Stage

        /// <summary>
        /// Postavi naslov pozornice.
        /// </summary>
        /// <param name="title">tekst koji će se ispisati na vrhu (naslovnoj traci).</param>
        public void SetStageTitle(string title)
        {
            this.Text = title;
        }

        /// <summary>
        /// Postavi boju pozadine.
        /// </summary>
        /// <param name="r">r</param>
        /// <param name="g">g</param>
        /// <param name="b">b</param>
        public void setBackgroundColor(int r, int g, int b)
        {
            this.BackColor = Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// Postavi boju pozornice. <c>Color</c> je ugrađeni tip.
        /// </summary>
        /// <param name="color"></param>
        public void setBackgroundColor(Color color)
        {
            this.BackColor = color;
        }

        /// <summary>
        /// Postavi sliku pozornice.
        /// </summary>
        /// <param name="backgroundImage">Naziv (putanja) slike.</param>
        public void setBackgroundPicture(string backgroundImage)
        {
            this.BackgroundImage = new Bitmap(backgroundImage);
        }

        /// <summary>
        /// Izgled slike.
        /// </summary>
        /// <param name="layout">none, tile, stretch, center, zoom</param>
        public void setPictureLayout(string layout)
        {
            if (layout.ToLower() == "none") this.BackgroundImageLayout = ImageLayout.None;
            if (layout.ToLower() == "tile") this.BackgroundImageLayout = ImageLayout.Tile;
            if (layout.ToLower() == "stretch") this.BackgroundImageLayout = ImageLayout.Stretch;
            if (layout.ToLower() == "center") this.BackgroundImageLayout = ImageLayout.Center;
            if (layout.ToLower() == "zoom") this.BackgroundImageLayout = ImageLayout.Zoom;
        }

        #endregion

        //sound
        #region sound methods

        /// <summary>
        /// Učitaj zvuk.
        /// </summary>
        /// <param name="soundNum">-</param>
        /// <param name="file">-</param>
        public void loadSound(int soundNum, string file)
        {
            soundCount++;
            sounds[soundNum] = new SoundPlayer(file);
        }

        /// <summary>
        /// Sviraj zvuk.
        /// </summary>
        /// <param name="soundNum">-</param>
        public void playSound(int soundNum)
        {
            sounds[soundNum].Play();
        }

        /// <summary>
        /// loopSound
        /// </summary>
        /// <param name="soundNum">-</param>
        public void loopSound(int soundNum)
        {
            sounds[soundNum].PlayLooping();
        }

        /// <summary>
        /// Zaustavi zvuk.
        /// </summary>
        /// <param name="soundNum">broj</param>
        public void stopSound(int soundNum)
        {
            sounds[soundNum].Stop();
        }

        #endregion

        //file
        #region file methods

        /// <summary>
        /// Otvori datoteku za čitanje.
        /// </summary>
        /// <param name="fileName">naziv datoteke</param>
        /// <param name="fileNum">broj</param>
        public void openFileToRead(string fileName, int fileNum)
        {
            readFiles[fileNum] = new StreamReader(fileName);
        }

        /// <summary>
        /// Zatvori datoteku.
        /// </summary>
        /// <param name="fileNum">broj</param>
        public void closeFileToRead(int fileNum)
        {
            readFiles[fileNum].Close();
        }

        /// <summary>
        /// Otvori datoteku za pisanje.
        /// </summary>
        /// <param name="fileName">naziv datoteke</param>
        /// <param name="fileNum">broj</param>
        public void openFileToWrite(string fileName, int fileNum)
        {
            writeFiles[fileNum] = new StreamWriter(fileName);
        }

        /// <summary>
        /// Zatvori datoteku.
        /// </summary>
        /// <param name="fileNum">broj</param>
        public void closeFileToWrite(int fileNum)
        {
            writeFiles[fileNum].Close();
        }

        /// <summary>
        /// Zapiši liniju u datoteku.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <param name="line">linija</param>
        public void writeLine(int fileNum, string line)
        {
            writeFiles[fileNum].WriteLine(line);
        }

        /// <summary>
        /// Pročitaj liniju iz datoteke.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <returns>vraća pročitanu liniju</returns>
        public string readLine(int fileNum)
        {
            return readFiles[fileNum].ReadLine();
        }

        /// <summary>
        /// Čita sadržaj datoteke.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <returns>vraća sadržaj</returns>
        public string readFile(int fileNum)
        {
            return readFiles[fileNum].ReadToEnd();
        }

        #endregion

        //mouse & keys
        #region mouse methods

        /// <summary>
        /// Sakrij strelicu miša.
        /// </summary>
        public void hideMouse()
        {
            Cursor.Hide();
        }

        /// <summary>
        /// Pokaži strelicu miša.
        /// </summary>
        public void showMouse()
        {
            Cursor.Show();
        }

        /// <summary>
        /// Provjerava je li miš pritisnut.
        /// </summary>
        /// <returns>true/false</returns>
        public bool isMousePressed()
        {
            //return sensing.MouseDown;
            return sensing.MouseDown;
        }

        /// <summary>
        /// Provjerava je li tipka pritisnuta.
        /// </summary>
        /// <param name="key">naziv tipke</param>
        /// <returns></returns>
        public bool isKeyPressed(string key)
        {
            if (sensing.Key == key)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Provjerava je li tipka pritisnuta.
        /// </summary>
        /// <param name="key">tipka</param>
        /// <returns>true/false</returns>
        public bool isKeyPressed(Keys key)
        {
            if (sensing.Key == key.ToString())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #endregion
        /* ------------------- */

        /* ------------ GAME CODE START ------------ */

        /* Game variables */


        /* Initialization */
        int stoperica = 45;

        int score = 0;

        Igrac igrac;

        Laser metak1;
        Laser metak2;
        Laser metak3;
        Laser metak4;
        Laser metak5;
        Laser metak6;
        Laser metak7;
        Laser metak8;
        Laser metak9;
        Laser metak10;
        Laser metak11;
        Laser metak12;
        Laser metak13;
        Laser metak14;
        Laser metak15;
        Laser metak16;
        Laser metak17;
        Laser metak18;
        Laser metak19;
        Laser metak20;

        Neprijatelj n1;
        Neprijatelj n2;
        Neprijatelj n3;
        Neprijatelj n4;
        Neprijatelj n5;
        Neprijatelj n6;
        Neprijatelj n7;
        Neprijatelj n8;
        Neprijatelj n9;
        Neprijatelj n10;
        Neprijatelj n11;
        Neprijatelj n12;
        Neprijatelj n13;
        Neprijatelj n14;
        Neprijatelj n15;
        Neprijatelj n16;
        Neprijatelj n17;
        Neprijatelj n18;
        Neprijatelj n19;
        Neprijatelj n20;


        List<Laser> metci = new List<Laser>();
        List<Neprijatelj> n = new List<Neprijatelj>();

        private void SetupGame()
        {
            //1. setup stage
            //SetStageTitle("PMF");
            SetStageTitle(this.Player);
            //setBackgroundColor(Color.WhiteSmoke);            
            setBackgroundPicture("backgrounds\\purple.png");
            //none, tile, stretch, center, zoom
            setPictureLayout("stretch");

            //2. add sprites

            igrac = new Igrac("sprites\\player.png", 280, 715);
            igrac.SetSize(40);
            Game.AddSprite(igrac);

            n1 = new Neprijatelj("sprites\\1.png", 5, 0);
            n1.SetSize(30);
            n1.SetVisible(false);
            Game.AddSprite(n1);
            n2 = new Neprijatelj("sprites\\2.png", 50, 0);
            n2.SetSize(30);
            n2.SetVisible(false);
            Game.AddSprite(n2);
            n3 = new Neprijatelj("sprites\\3.png", 100, 0);
            n3.SetSize(30);
            n3.SetVisible(false);
            Game.AddSprite(n3);
            n4 = new Neprijatelj("sprites\\4.png", 150, 0);
            n4.SetSize(30);
            n4.SetVisible(false);
            Game.AddSprite(n4);
            n5 = new Neprijatelj("sprites\\5.png", 200, 0);
            n5.SetSize(30);
            n5.SetVisible(false);
            Game.AddSprite(n5);
            n6 = new Neprijatelj("sprites\\1.png", 250, 0);
            n6.SetSize(30);
            n6.SetVisible(false);
            Game.AddSprite(n6);
            n7 = new Neprijatelj("sprites\\2.png", 300, 0);
            n7.SetSize(30);
            n7.SetVisible(false);
            Game.AddSprite(n7);
            n8 = new Neprijatelj("sprites\\3.png", 350, 0);
            n8.SetSize(30);
            n8.SetVisible(false);
            Game.AddSprite(n8);
            n9 = new Neprijatelj("sprites\\4.png", 410, 0);
            n9.SetSize(30);
            n9.SetVisible(false);
            Game.AddSprite(n9);
            n10 = new Neprijatelj("sprites\\5.png", 520, 0);
            n10.SetSize(30);
            n10.SetVisible(false);
            Game.AddSprite(n10);
            n11 = new Neprijatelj("sprites\\1.png", 630, 0);
            n11.SetSize(30);
            n11.SetVisible(false);
            Game.AddSprite(n11);
            n12 = new Neprijatelj("sprites\\2.png", 670, 0);
            n12.SetSize(30);
            n12.SetVisible(false);
            Game.AddSprite(n12);
            n13 = new Neprijatelj("sprites\\2.png", 40, 0);
            n13.SetSize(30);
            n13.SetVisible(false);
            Game.AddSprite(n13);
            n14 = new Neprijatelj("sprites\\1.png", 70, 0);
            n14.SetSize(30);
            n14.SetVisible(false);
            Game.AddSprite(n14);
            n15 = new Neprijatelj("sprites\\2.png", 120, 0);
            n15.SetSize(30);
            n15.SetVisible(false);
            Game.AddSprite(n15);
            n16 = new Neprijatelj("sprites\\3.png", 170, 0);
            n16.SetSize(30);
            n16.SetVisible(false);
            Game.AddSprite(n16);
            n17 = new Neprijatelj("sprites\\4.png", 220, 0);
            n17.SetSize(30);
            n17.SetVisible(false);
            Game.AddSprite(n17);
            n18 = new Neprijatelj("sprites\\5.png", 280, 0);
            n18.SetSize(30);
            n18.SetVisible(false);
            Game.AddSprite(n18);
            n19 = new Neprijatelj("sprites\\1.png", 320, 0);
            n19.SetSize(30);
            n19.SetVisible(false);
            Game.AddSprite(n19);
            n20 = new Neprijatelj("sprites\\2.png", 660, 0);
            n20.SetSize(30);
            n20.SetVisible(false);
            Game.AddSprite(n20);

            metak1 = new Laser("sprites\\laser.png", 0, 715);
            metak1.SetSize(15);
            metak1.SetVisible(false);
            Game.AddSprite(metak1);
            metak2 = new Laser("sprites\\laser.png", 0, 715);
            metak2.SetSize(15);
            metak2.SetVisible(false);
            Game.AddSprite(metak2);
            metak3 = new Laser("sprites\\laser.png", 0, 715);
            metak3.SetSize(15);
            metak3.SetVisible(false);
            Game.AddSprite(metak3);
            metak4 = new Laser("sprites\\laser.png", 0, 715);
            metak4.SetSize(15);
            metak4.SetVisible(false);
            Game.AddSprite(metak4);
            metak5 = new Laser("sprites\\laser.png", 0, 715);
            metak5.SetSize(15);
            metak5.SetVisible(false);
            Game.AddSprite(metak5);
            metak6 = new Laser("sprites\\laser.png", 0, 715);
            metak6.SetSize(15);
            metak6.SetVisible(false);
            Game.AddSprite(metak6);
            metak7 = new Laser("sprites\\laser.png", 0, 715);
            metak7.SetSize(15);
            metak7.SetVisible(false);
            Game.AddSprite(metak7);
            metak8 = new Laser("sprites\\laser.png", 0, 715);
            metak8.SetSize(15);
            metak8.SetVisible(false);
            Game.AddSprite(metak8);
            metak9 = new Laser("sprites\\laser.png", 0, 715);
            metak9.SetSize(15);
            metak9.SetVisible(false);
            Game.AddSprite(metak9);
            metak10 = new Laser("sprites\\laser.png", 0, 715);
            metak10.SetSize(15);
            metak10.SetVisible(false);
            Game.AddSprite(metak10);
            metak11 = new Laser("sprites\\laser.png", 0, 715);
            metak11.SetSize(15);
            metak11.SetVisible(false);
            Game.AddSprite(metak11);
            metak12 = new Laser("sprites\\laser.png", 0, 715);
            metak12.SetSize(15);
            metak12.SetVisible(false);
            Game.AddSprite(metak12);
            metak13 = new Laser("sprites\\laser.png", 0, 715);
            metak13.SetSize(15);
            metak13.SetVisible(false);
            Game.AddSprite(metak13);
            metak14 = new Laser("sprites\\laser.png", 0, 715);
            metak14.SetSize(15);
            metak14.SetVisible(false);
            Game.AddSprite(metak14);
            metak15 = new Laser("sprites\\laser.png", 0, 715);
            metak15.SetSize(15);
            metak15.SetVisible(false);
            Game.AddSprite(metak15);
            metak16 = new Laser("sprites\\laser.png", 0, 715);
            metak16.SetSize(15);
            metak16.SetVisible(false);
            Game.AddSprite(metak16);
            metak17 = new Laser("sprites\\laser.png", 0, 715);
            metak17.SetSize(15);
            metak17.SetVisible(false);
            Game.AddSprite(metak17);
            metak18 = new Laser("sprites\\laser.png", 0, 715);
            metak18.SetSize(15);
            metak18.SetVisible(false);
            Game.AddSprite(metak18);
            metak19 = new Laser("sprites\\laser.png", 0, 715);
            metak19.SetSize(15);
            metak19.SetVisible(false);
            Game.AddSprite(metak19);
            metak20 = new Laser("sprites\\laser.png", 0, 715);
            metak20.SetSize(15);
            metak20.SetVisible(false);
            Game.AddSprite(metak20);

            n.Add(n1);
            n.Add(n2);
            n.Add(n3);
            n.Add(n4);
            n.Add(n5);
            n.Add(n6);
            n.Add(n7);
            n.Add(n8);
            n.Add(n9);
            n.Add(n10);
            n.Add(n11);
            n.Add(n12);
            n.Add(n13);
            n.Add(n14);
            n.Add(n15);
            n.Add(n16);
            n.Add(n17);
            n.Add(n18);
            n.Add(n19);
            n.Add(n20);

            metci.Add(metak1);
            metci.Add(metak2);
            metci.Add(metak3);
            metci.Add(metak4);
            metci.Add(metak5);
            metci.Add(metak6);
            metci.Add(metak7);
            metci.Add(metak8);
            metci.Add(metak9);
            metci.Add(metak10);
            metci.Add(metak11);
            metci.Add(metak12);
            metci.Add(metak13);
            metci.Add(metak14);
            metci.Add(metak15);
            metci.Add(metak16);
            metci.Add(metak17);
            metci.Add(metak18);
            metci.Add(metak19);
            metci.Add(metak20);

            //3. scripts that start
           
            Game.StartScript(Igrac_Kretanje);
            Game.StartScript(Pucaj);
            Game.StartScript(KretanjeMetaka);
            Game.StartScript(Stoperica);
            Game.StartScript(Nestao);
            Game.StartScript(Moving_Neprijatelj10);
            Game.StartScript(Moving_Neprijatelj16);
            Game.StartScript(Moving_Neprijatelj12);
            Game.StartScript(Moving_Neprijatelj7);        
            Game.StartScript(Moving_Neprijatelj13);
            Game.StartScript(Moving_Neprijatelj4);
            Game.StartScript(Moving_Neprijatelj5);
            Game.StartScript(Moving_Neprijatelj20);
            Game.StartScript(Moving_Neprijatelj1);
            Game.StartScript(Moving_Neprijatelj18);
            Game.StartScript(Moving_Neprijatelj14);
            Game.StartScript(Moving_Neprijatelj11);
            Game.StartScript(Moving_Neprijatelj2);
            Game.StartScript(Moving_Neprijatelj9);
            Game.StartScript(Moving_Neprijatelj3);
            Game.StartScript(Moving_Neprijatelj15);
            Game.StartScript(Moving_Neprijatelj8);
            Game.StartScript(Moving_Neprijatelj19);
            Game.StartScript(Moving_Neprijatelj6);
            Game.StartScript(Moving_Neprijatelj17);
        }

        /* Scripts */

        private int Moving_Neprijatelj1()
        {
            while (START)
            {
                n1.SetVisible(true);
                n1.Y = n1.Y + 11;
                Wait(0.1);
            }

            return 0;
        }
        private int Moving_Neprijatelj2()
        {
            while (START)
            {
                n2.SetVisible(true);
                n2.Y = n2.Y + 14;
                Wait(0.1);
            }
            return 0;
        }
        private int Moving_Neprijatelj3()
        {
            while (START)
            {
                n3.SetVisible(true);
                n3.Y = n3.Y + 6;
                Wait(0.05);
            }
            return 0;
        }
        private int Moving_Neprijatelj4()
        {
            while (START)
            {
                n4.SetVisible(true);
                n4.Y = n4.Y + 7;
                Wait(0.1);
            }
            return 0;
        }
        private int Moving_Neprijatelj5()
        {
            while (START)
            {
                n5.SetVisible(true);
                n5.Y = n5.Y + 16;
                Wait(0.1);
            }
            return 0;
        }
        private int Moving_Neprijatelj6()
        {
            while (START)
            {
                n6.SetVisible(true);
                n6.Y = n6.Y + 12;
                Wait(0.1);
            }
            return 0;
        }
        private int Moving_Neprijatelj7()
        {
            while (START)
            {
                n7.SetVisible(true);
                n7.Y = n7.Y + 5;
                Wait(0.01);
            }
            return 0;
        }
        private int Moving_Neprijatelj8()
        {
            while (START)
            {
                n8.SetVisible(true);
                n8.Y = n8.Y + 9;
                Wait(0.1);
            }
            return 0;
        }
        private int Moving_Neprijatelj9()
        {
            while (START)
            {
                n9.SetVisible(true);
                n9.Y = n9.Y + 13;
                Wait(0.1);
            }
            return 0;
        }
        private int Moving_Neprijatelj10()
        {
            while (START)
            {
                n10.SetVisible(true);
                n10.Y = n10.Y + 8;
                Wait(0.1);
            }
            return 0;
        }
        private int Moving_Neprijatelj11()
        {
            while (START)
            {
                n11.SetVisible(true);
                n11.Y = n11.Y + 10;
                Wait(0.1);
            }
            return 0;
        }
        private int Moving_Neprijatelj12()
        {
            while (START)
            {
                n12.SetVisible(true);
                n12.Y = n12.Y + 11;
                Wait(0.1);
            }
            return 0;
        }
        private int Moving_Neprijatelj13()
        {
            while (START)
            {
                n13.SetVisible(true);
                n13.Y = n13.Y + 9;
                Wait(0.1);
            }
            return 0;
        }
        private int Moving_Neprijatelj14()
        {
            while (START)
            {
                n14.SetVisible(true);
                n14.Y = n14.Y + 11;
                Wait(0.1);
            }
            return 0;
        }
        private int Moving_Neprijatelj15()
        {
            while (START)
            {
                n15.SetVisible(true);
                n15.Y = n15.Y + 6;
                Wait(0.05);
            }
            return 0;
        }
        private int Moving_Neprijatelj16()
        {
            while (START)
            {
                n16.SetVisible(true);
                n16.Y = n16.Y + 12;
                Wait(0.2);
            }
            return 0;
        }
        private int Moving_Neprijatelj17()
        {
            while (START)
            {
                n17.SetVisible(true);
                n17.Y = n17.Y + 15;
                Wait(0.1);
            }
            return 0;
        }
        private int Moving_Neprijatelj18()
        {
            while (START)
            {
                n18.SetVisible(true);
                n18.Y = n18.Y + 7;
                Wait(0.1);
            }
            return 0;
        }
        private int Moving_Neprijatelj19()
        {
            while (START)
            {
                n19.SetVisible(true);
                n19.Y = n19.Y + 13;
                Wait(0.2);
            }
            return 0;
        }
        private int Moving_Neprijatelj20()
        {
            while (START)
            {
                n20.SetVisible(true);
                n20.Y = n10.Y + 14;
                Wait(0.1);
            }
            return 0;
        }        
        private int Igrac_Kretanje()
        {
            while (START)
            {
                if (sensing.KeyPressed(Keys.Right))
                    igrac.X += 10;
                if (sensing.KeyPressed(Keys.Left))
                    igrac.X -= 10;
                Wait(0.01);
            }
            return 0;
        }   

        private int Pucaj()
        {
            while (START)
            {
                Wait(0.1);
                if (sensing.KeyPressed(Keys.Space))
                {
                    for (int i = 0; i < metci.Count; i++)
                    {
                        if (metci[i].Ispucan == false)
                        {
                            metci[i].Goto_Sprite(igrac);
                            metci[i].Ispucan = true;
                            metci[i].SetVisible(true);
                            break;
                        }
                    }                       
                }
            }
            return 0;               
        }
        
        private int KretanjeMetaka()
        {
            while(START)
            {
                int br = 0;
                Wait(0.005);
                for (int i = 0; i < metci.Count; i++)
                {
                    if (metci[i].Ispucan == true)
                    {
                        br++;
                        metci[i].Y -= 10;
                    }
                    if(br==20)
                    {
                        START = false;
                        MessageBox.Show("GAME OVER" + "\n Potrošili ste sve metke!" + "\n Score:" + score);
                    }
                }                
            }
            return 0;
        }
        
        private int Nestao()
        {
            while (START)
            {
                for (int i = 0; i < metci.Count; i++)
                {
                    for (int j = 0; j < n.Count; j++)
                    { 
                        if (metci[i].TouchingSprite(n[j]))
                        {
                            n[j].GotoXY(900,900);
                            score += 1;
                            PostaviTekstNaLabelu(score.ToString());
                        }                        
                    }
                }
                
            }
            return 0;
        }

        private void BGL_FormClosed(object sender, FormClosedEventArgs e)
        {

            
        }

        private int Stoperica()
        {
            while (START)
            {
                Wait(1);
                PostaviTekstNaLabeluStoperica(stoperica.ToString());
                stoperica--;
                if (stoperica == 0)
                {
                    START = false;
                    MessageBox.Show("GAME OVER" + "\n Vrijeme je isteklo!" + "\n Score:" + score);
                }
            }
            stoperica = 45;
            return 0;
        }
        delegate void DelegatTipaVoid(string text);
        private void PostaviTekstNaLabeluStoperica(string t)
        {
            if (this.lblStop.InvokeRequired)
            {
                DelegatTipaVoid d = new DelegatTipaVoid(PostaviTekstNaLabeluStoperica);
                this.Invoke(d, new object[] { t });
            }
            else
            {
                this.lblStop.Text = t;
            }
        }
        private void PostaviTekstNaLabelu(string t)
        {
            if (this.InvokeRequired)
            {
                DelegatTipaVoid d = new DelegatTipaVoid(PostaviTekstNaLabelu);
                this.Invoke(d, new object[] { t });
            }
            else
            {
                this.lblScore.Text = t;
            }
        }












        /* ------------ GAME CODE END ------------ */


    }
}
