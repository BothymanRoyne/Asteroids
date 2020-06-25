using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Asteroids
{
    public partial class Asteroids : Form
    {
        public Font GAME_FONT = new Font(FontFamily.GenericMonospace, 20);
        public System.Drawing.Color SHIP_COLOR = System.Drawing.Color.Yellow;
        public System.Drawing.Color ASTEROID_COLOR = System.Drawing.Color.Red;
        public System.Drawing.Color MISSILE_COLOR = System.Drawing.Color.White;
        public System.Drawing.Color FONT_COLOR = System.Drawing.Color.White;

        private readonly List<Asteroid> AsteroidList = new List<Asteroid>(); //current asteroids in play
        private readonly List<Ship> Lives = new List<Ship>();                //strictly for the number of lives display, initially 3
        private readonly List<int> HighscoreList = new List<int>();          //list of highscores 

        private readonly string WorkingDirectory;     //project directory
        private readonly string HighscoresFile;       //path to highscores.txt

        private readonly Timer T;                     //game loop
        private Ship Man;                             //the player

        private readonly int NumLives = 3;            //initial number of lives
        private int CurrentLives;                     //current number of lives
        private int Score = 0;                        //current score
        private int PreviousScore;                    //previous score
        private int NumAsteroidsKilled = 0;           //number of asteroids destroyed
        private int SpawnRate = 500;

        private bool GameOver { get; set; } = false;  //controls if gameover or not
        private bool FirstRun { get; set; }           //true on the first run of the game, false otherwise

        public static bool UpInput;                   //thrust input
        public static bool LeftInput;                 //turn left
        public static bool RightInput;                //turn right
        public static bool SpaceInput;                //shoot the gun
        public static bool PauseInput;                //pause the game
        private GamePadState GamePadState;            //represents the xbox controller

        public int Ticks = 0;

        public Asteroids()
        {
            InitializeComponent();

            WorkingDirectory = Environment.CurrentDirectory;
            HighscoresFile =  Directory.GetParent(WorkingDirectory).Parent.FullName + @"\highscores.txt";

            //read the highscores into a list for display on screen
            using (var sr = new StreamReader(HighscoresFile))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (int.TryParse(line, out int result))
                        HighscoreList.Add(result);
                }
            }

            //our game loop
            T = new Timer
            {
                Interval = 10
            };

            T.Tick += Tick;
            KeyUp += Asteroids_KeyUp;
            KeyDown += Asteroids_KeyDown;
            Load += (s, e) => CenterToScreen();

            T.Start();

            //spawn the player
            Man = new Ship(new PointF(ClientSize.Width / 2, ClientSize.Height / 2));

            CurrentLives = NumLives; //initialize lives
            PreviousScore = Score; //initialize score

            //boolean initializations
            PauseInput = false;
            GameOver = true;
            FirstRun = true;
        }

        //create an asteroid at the specified location with specified size and direction
        private void CreateAsteroid(PointF loc, Asteroid.ASize size, float dir) => AsteroidList.Add(new Asteroid(loc, size, dir));

        //input has been abstracted to a set of bools, appropriately set based on button inputs
        private void Asteroids_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.W:
                    UpInput = true;
                    Man.Thruster = true;
                    break;
                case System.Windows.Forms.Keys.A:
                    LeftInput = true;
                    break;
                case System.Windows.Forms.Keys.D:
                    RightInput = true;
                    break;
                case System.Windows.Forms.Keys.Space:
                    SpaceInput = true;
                    break;
                case System.Windows.Forms.Keys.P:
                    PauseInput = !PauseInput;
                    break;
                case System.Windows.Forms.Keys.F:
                    CreateAsteroid(new PointF(0, 0), Asteroid.ASize.Large, Asteroid.RNG.Next(361));
                    Score += 10000;
                    break;
            }
        }

        //input has been abstracted to a set of bools, appropriately set based on button inputs
        private void Asteroids_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.W:
                    UpInput = false;
                    Man.Thruster = false;
                    break;
                case System.Windows.Forms.Keys.A:
                    LeftInput = false;
                    break;
                case System.Windows.Forms.Keys.D:
                    RightInput = false;
                    break;
                case System.Windows.Forms.Keys.Space:
                    SpaceInput = false;
                    break;
            }
        }

        private void Tick(object sender, EventArgs e)
        {
            GamePadState = GamePad.GetState(PlayerIndex.One);
            if (GamePadState.IsConnected)
            {
                UpInput = GamePadState.IsButtonDown(Buttons.DPadUp);
                LeftInput = GamePadState.IsButtonDown(Buttons.DPadLeft);
                RightInput = GamePadState.IsButtonDown(Buttons.DPadRight);
                SpaceInput = GamePadState.IsButtonDown(Buttons.A);
                PauseInput = GamePadState.IsButtonDown(Buttons.Start);
            }

            using (BufferedGraphicsContext bgc = new BufferedGraphicsContext())
            using (BufferedGraphics bg = bgc.Allocate(CreateGraphics(), ClientRectangle))
            {

                bg.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                bg.Graphics.Clear(System.Drawing.Color.Black); //reset our screen
                DrawScore(bg.Graphics, System.Drawing.Color.White); //draw the score in top left
                DrawLives(bg.Graphics, System.Drawing.Color.White); //draw lives below score
                DrawNumAsteroidsKilled(bg.Graphics, System.Drawing.Color.White); //draw number of asteroids killed in bottom left

                if (GameOver)
                {
                    if (!FirstRun) //don't draw gameover on first run but do draw instructions
                    {
                        DrawMenu(bg.Graphics, System.Drawing.Color.White, !FirstRun);
                        DrawHighscores(bg.Graphics, System.Drawing.Color.White);
                    }
                    else //draw gameover + instructions
                    {
                        DrawMenu(bg.Graphics, System.Drawing.Color.White, !FirstRun);
                        DrawHighscores(bg.Graphics, System.Drawing.Color.White);
                    }
                    if (SpaceInput && FirstRun)
                    {
                        ResetGame();
                        FirstRun = false;
                    }
                    if (SpaceInput && !FirstRun)
                    {
                        ResetGame();
                    }
                }
                else
                {
                    if (!PauseInput)
                    {
                        Text = Ticks++.ToString() + " " + SpawnRate.ToString(); //debug

                        #region collision detection between asteroids and ship
                        Region shipRegion = new Region(Man.GetPath());
                        //for (int i = 0; i < AsteroidList.Count; i++)
                        //{
                        //    if (AsteroidList[i].Active && !Man.Invincible) //if the asteroid was "faded in" and the player is not invincible
                        //    {
                        //        Region asteroidRegion = new Region(AsteroidList[i].GetPath()); //build region from current asteroid
                        //        asteroidRegion.Intersect(shipRegion); //check for intersection
                        //        if (asteroidRegion.GetRegionScans(new System.Drawing.Drawing2D.Matrix()).Length > 0)
                        //        {
                        //            Man.Alive = false; //kill current player
                        //            Man = new Ship(new PointF(ClientSize.Width / 2, ClientSize.Height / 2)); //spawn new player
                        //            if (Lives.Count > 0)
                        //            {
                        //                Lives.RemoveAt(Lives.Count - 1); //remove a life if player is hit
                        //                CurrentLives--;
                        //            }

                        //            if (CurrentLives <= 0)
                        //            {
                        //                GameOver = true;

                        //                //write the scores to file
                        //                using (var sw = new StreamWriter(HighscoresFile, true))
                        //                    sw.WriteLine(Score);

                        //                //read the scores from file
                        //                using (var sr = new StreamReader(HighscoresFile))
                        //                {
                        //                    string line;

                        //                    while ((line = sr.ReadLine()) != null)
                        //                    {
                        //                        if (int.TryParse(line, out int result))
                        //                            HighscoreList.Add(result);
                        //                    }
                        //                }
                        //            }
                        //        }
                        //    }
                        //}

                        //for (int i = 0; i < AsteroidList.Count; i++)
                        foreach (var asteroid in AsteroidList)
                        {
                            //if (AsteroidList[i].Active && !Man.Invincible) //if the asteroid was "faded in" and the player is not invincible
                            if (asteroid.Active && !Man.Invincible)
                            {
                                //Region asteroidRegion = new Region(AsteroidList[i].GetPath()); //build region from current asteroid
                                Region asteroidRegion = new Region(asteroid.GetPath());
                                asteroidRegion.Intersect(shipRegion); //check for intersection
                                if (asteroidRegion.GetRegionScans(new System.Drawing.Drawing2D.Matrix()).Length > 0)
                                {
                                    Man.Alive = false; //kill current player
                                    Man = new Ship(new PointF(ClientSize.Width / 2, ClientSize.Height / 2)); //spawn new player
                                    if (Lives.Count > 0)
                                    {
                                        Lives.RemoveAt(Lives.Count - 1); //remove a life if player is hit
                                        CurrentLives--;
                                    }

                                    if (CurrentLives <= 0)
                                    {
                                        GameOver = true;

                                        //write the scores to file
                                        using (var sw = new StreamWriter(HighscoresFile, true))
                                            sw.WriteLine(Score);

                                        //read the scores from file
                                        using (var sr = new StreamReader(HighscoresFile))
                                        {
                                            string line;

                                            while ((line = sr.ReadLine()) != null)
                                            {
                                                if (int.TryParse(line, out int result))
                                                    HighscoreList.Add(result);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        #region collision detection between asteroids and missiles
                        foreach (var missile in Man.Missiles)
                        {
                            foreach (var asteroid in AsteroidList)
                            {
                                //build regions based on the current missile and asteroid
                                Region bulletRegion = new Region(missile.GetPath());
                                Region asteroidRegion = new Region(asteroid.GetPath());

                                asteroidRegion.Intersect(bulletRegion); //check for intersection
                                if (asteroidRegion.GetRegionScans(new System.Drawing.Drawing2D.Matrix()).Length > 0) //we've hit an asteroid
                                {
                                    NumAsteroidsKilled++;
                                    missile.MarkedForDeath = true;
                                    asteroid.MarkedForDeath = true;

                                    switch (asteroid.AsteroidSize)
                                    {
                                        case Asteroid.ASize.Large:
                                            Score += 100;
                                            for (int k = 0, spread = -60; k < 3; k++, spread += 60) //children asteroids get a spread and their parent's direction
                                                CreateAsteroid(asteroid.Location, Asteroid.ASize.Medium, Man.Direction + spread);
                                            break;
                                        case Asteroid.ASize.Medium:
                                            Score += 200;
                                            for (int k = 0, spread = -60; k < 3; k++, spread += 60) //children asteroids get a spread and their parent's direction
                                                CreateAsteroid(asteroid.Location, Asteroid.ASize.Small, Man.Direction + spread);
                                            break;
                                        case Asteroid.ASize.Small:
                                            Score += 300;
                                            break;
                                    }
                                    break;
                                }
                            }
                        }
                        #endregion

                        //create a new asteroid every so often
                        if (Ticks % SpawnRate == 0)
                        {
                            CreateAsteroid(new PointF(ShapeBase.RNG.Next(ClientSize.Width), ShapeBase.RNG.Next(ClientSize.Height)), Asteroid.ASize.Large, ShapeBase.RNG.Next(361));
                            Console.WriteLine("Spawned asteroid");
                        }

                        //vary the asteroid spawn rate based on time progression of the game
                        if (Ticks % 2500 == 0 && SpawnRate > 400)
                            SpawnRate = 400;
                        if (Ticks % 5000 == 0 && SpawnRate > 300)
                            SpawnRate = 300;
                        if (Ticks % 7500 == 0 && SpawnRate > 200)
                            SpawnRate = 200;
                        if (Ticks % 10000 == 0 && SpawnRate > 100)
                            SpawnRate = 100;

                        //add an extra life every 10000 points
                        if (Score != PreviousScore)
                        {
                            if (Score % 10000 == 0)
                            {
                                Lives.Add(new Ship(new PointF(Lives.Last().Location.X + 20, Lives.Last().Location.Y)));
                                CurrentLives++;
                            }
                            PreviousScore = Score;
                        }

                        Man.Tick(ClientSize, LeftInput, RightInput, UpInput, SpaceInput); //update the ship + missiles, draws the ship/missiles too
                        AsteroidList.ForEach(x => x.Tick(ClientSize)); //update all asteroids still in play
                        AsteroidList.RemoveAll(x => x.MarkedForDeath); //remove all destroyed asteroids

                        Man.Render(bg.Graphics, SHIP_COLOR); //render the ship
                        AsteroidList.ForEach(x => x.Render(bg.Graphics, ASTEROID_COLOR)); //render the asteroids
                    }
                    else
                        DrawPause(bg.Graphics, FONT_COLOR); //draw the pause screen, stop rendering other things
                }

                bg.Render(); //draw everything
            }
        }

        /// <summary>
        /// Resets the relevant variables to their initial states.
        /// </summary>
        private void ResetGame()
        {
            //reset appropriate variables
            GameOver = false;
            Score = 0;
            NumAsteroidsKilled = 0;
            AsteroidList.Clear();
            Lives.Clear();
            HighscoreList.Clear();

            //draw the lives in top left corner
            for (int i = 1; i < NumLives + 1; i++)
                Lives.Add(new Ship(new PointF(i * 20, 50)));
            CurrentLives = NumLives; //reset current lives to 3
        }

        /// <summary>
        /// Draws the score on screen.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        private void DrawScore(Graphics g, System.Drawing.Color c) => g.DrawString(Score.ToString("D6"), GAME_FONT, new SolidBrush(c), new PointF(0, 0));

        /// <summary>
        /// Draws the pause menu on screen.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        private void DrawPause(Graphics g, System.Drawing.Color c) => g.DrawString("PAUSE", GAME_FONT, new SolidBrush(c), new PointF((ClientSize.Width / 2) - 45, (ClientSize.Height / 2) - 28));

        /// <summary>
        /// Draws the number of asteroids killed on screen.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        private void DrawNumAsteroidsKilled(Graphics g, System.Drawing.Color c) => g.DrawString(NumAsteroidsKilled.ToString(), GAME_FONT, new SolidBrush(c), new PointF(0, ClientSize.Height - 24));

        /// <summary>
        /// Draws the number of lives on screen.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        private void DrawLives(Graphics g, System.Drawing.Color c) => Lives.ForEach(x => x.Render(g, c));

        /// <summary>
        /// Draws the gameover and game instructions on screen.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        /// <param name="firstrun"></param>
        private void DrawMenu(Graphics g, System.Drawing.Color c, bool firstrun)
        {
            if (firstrun)
                g.DrawString("GAME OVER", GAME_FONT, new SolidBrush(System.Drawing.Color.Red), new PointF((ClientSize.Width / 2) - 80, (ClientSize.Height / 2) - 40));

            g.DrawString("W or DpadUP = thruster", GAME_FONT, new SolidBrush(c), new PointF(0, 60));
            g.DrawString("A or DpadLEFT = left rotation", GAME_FONT, new SolidBrush(c), new PointF(0, 80));
            g.DrawString("D or DpadRIGHT = right rotation", GAME_FONT, new SolidBrush(c), new PointF(0, 100));
            g.DrawString("Space or A = shoot", GAME_FONT, new SolidBrush(c), new PointF(0, 120));
            g.DrawString("P or Start = pause game", GAME_FONT, new SolidBrush(c), new PointF(0, 140));
            g.DrawString("F = spawn asteroid (debug)", GAME_FONT, new SolidBrush(c), new PointF(0, 160));
            g.DrawString("Asteroids Killed", GAME_FONT, new SolidBrush(c), new PointF(0, ClientSize.Height - 46));

            g.DrawString("Press Space to start!", GAME_FONT, new SolidBrush(System.Drawing.Color.Orange), new PointF((ClientSize.Width / 2) - 180, ClientSize.Height / 2));
        }

        private void DrawHighscores(Graphics g, System.Drawing.Color c)
        {
            g.DrawString("HIGHSCORES", GAME_FONT, new SolidBrush(System.Drawing.Color.HotPink), new PointF(0, 270));

            HighscoreList.Sort((x, y) => y.CompareTo(x));
            var scoresToDraw = HighscoreList.Take(5).ToList();

            for (int i = 0, yCoord = 290; i < scoresToDraw.Count; i++, yCoord += 20)
            {
                g.DrawString(scoresToDraw[i].ToString(), GAME_FONT, new SolidBrush(c), new PointF(0, yCoord));
            }
        }
    }
}
