using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids
{
    public class Asteroid : ShapeBase
    {
        public bool Active { get; set; }    //controls if the asteroid can collide with ship or not
        public int CurrentSafeTime = 0;     //current number of ticks that the asteroid has no collision
        public int SafeTimer = 100;         //total number of ticks that the asteroid has no collision
        public enum ASize                   //represents the different sizes of asteroid
        {
            Small,
            Medium,
            Large
        };
        public ASize AsteroidSize;

        public Asteroid(PointF pos, ASize size, float dir) : base(pos)
        {
            RotationIncrement = RNG.NextDoubleRange(-3, 3);
            DeltaX = RNG.NextDoubleRange(-6, 6);
            DeltaY = RNG.NextDoubleRange(-6, 6);
            Acceleration = RNG.NextDoubleRange(0.1f, 3.0f);
            Model = GenerateAsteroid(50, RNG.Next(6, 14), 0.7f, size);
            Active = false;
            CurrentSafeTime = SafeTimer;
            AsteroidSize = size;
            Direction = dir;
        }

        /// <summary>
        /// Generates an irregularly shaped polygon based on the radius, vertices, variance and size being passed in.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="vertices"></param>
        /// <param name="variance"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static GraphicsPath GenerateAsteroid(int radius, int vertices, float variance, ASize size)
        {
            List<PointF> points = new List<PointF>();
            GraphicsPath gp = new GraphicsPath();
            Matrix m = new Matrix();
            double angle = 0;

            for (int i = 0; i < vertices; i++, angle = (2 * Math.PI / vertices) * i)
                points.Add(new PointF((float)(Math.Cos(angle) * (radius - (RNG.NextDouble() * radius * variance))), (float)(Math.Sin(angle) * (radius - (RNG.NextDouble() * radius * variance)))));

            gp.AddPolygon(points.ToArray());
            switch (size)
            {
                case ASize.Large:
                    m.Scale(1.5f, 1.5f);
                    break;
                case ASize.Medium:
                    m.Scale(1.0f, 1.0f);
                    break;
                case ASize.Small:
                    m.Scale(0.4f, 0.4f);
                    break;
            }
            gp.Transform(m);
            return gp;
        }

        /// <summary>
        /// Renders the asteroid.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        public override void Render(Graphics g, Color c)
        {
            if (Active)
                g.DrawPath(new Pen(c), GetPath());
            else
                g.DrawPath(new Pen(Color.Green), GetPath());
        }

        /// <summary>
        /// Updates the safe time, location and rotation of the asteroid.
        /// </summary>
        /// <param name="sz"></param>
        public void Tick(Size sz)
        {
            if (CurrentSafeTime-- <= 0)
                Active = true;

            //calculate distance traveled 
            DeltaX = -(float)Math.Sin(Direction * Math.PI / 180) * Acceleration;
            DeltaY = (float)Math.Cos(Direction * Math.PI / 180) * Acceleration;

            if (Location.X + DeltaX >= sz.Width)
                Location.X = 0; //wrap horizontally right to left
            else if (Location.X + DeltaX < 0)
                Location.X = sz.Width; //wrap horizontally left to right
            else
                Location.X += DeltaX; //update X coordinate normally if we aren't wrapping

            if (Location.Y + DeltaY >= sz.Height)
                Location.Y = 0; //wrap vertically bottom to top
            else if (Location.Y + DeltaY < 0)
                Location.Y = sz.Height; //wrap vertically top to bottom
            else
                Location.Y += DeltaY; //update Y coordinate normally if we aren't wrapping

            Rotation += RotationIncrement; //update rotation
        }
    }
}
