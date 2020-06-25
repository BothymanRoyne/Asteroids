using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids
{
    public class Missile : ShapeBase
    {
        public float Size { get; } = 2;           //size of missile
        public int Lifespan { get; set; } = 100;  //number of ticks missile should exist

        public Missile(PointF pos, float direction, float speed) : base(pos)
        {
            Location = new PointF(pos.X - 3, pos.Y - 5); //inherit position from ship, but fine-tune it
            Direction = direction; //inherit direction from ship
            Speed = 8; //initialize missile speed
            Speed += speed; //inherit speed from ship
            Model = GenerateMissile();
            MarkedForDeath = false;
        }

        /// <summary>
        /// Generates a GraphicsPath representing a missile shot from the ship.
        /// </summary>
        /// <returns></returns>
        public GraphicsPath GenerateMissile()
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(Size / 2, Size / 2, Size, Size);
            return gp;
        }

        /// <summary>
        /// Renders the missile.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        public override void Render(Graphics g, Color c) => g.FillPath(new SolidBrush(c), GetPath());

        /// <summary>
        /// Updates the lifespan and location of the missile.
        /// </summary>
        /// <param name="sz"></param>
        public void Tick(Size sz)
        {
            //don't fear the reaper
            if (Lifespan-- == 0)
                MarkedForDeath = true;

            //calculate distance traveled in this tick
            DeltaX = -(float)Math.Sin(Direction * Math.PI / 180) * Speed;
            DeltaY = (float)Math.Cos(Direction * Math.PI / 180) * Speed;

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
        }
    }
}
