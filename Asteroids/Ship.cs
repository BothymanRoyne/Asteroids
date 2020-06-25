using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids
{
    internal class Ship : ShapeBase
    {
        public readonly List<Missile> Missiles = new List<Missile>(); //collection of current missiles in play

        public const int MAX_NUM_MISSILES = 8;             //maximum missiles on screen at once
        public bool Alive { get; set; } = true;            //controls if the player should die
        public int GunCooldown { get; set; } = 15;         //cooldown between missile shots
        public int CurrentGunCooldown { get; set; } = 0;   //current cooldown
        public int InvincibleTimer = 100;                  //amount of time the player is invincible after spawning
        public bool Invincible;                            //controls if the player is invincible
        public bool Thruster = false;                      //controls if the thruster is engaged
        public GraphicsPath ThrusterModel;                 //thruster's model
        public GraphicsPath ShipModel;                     //ship's model

        public Ship(PointF loc) : base(loc)
        {
            Model = GenerateShipWithThruster();
            ShipModel = GenerateShip();
            ThrusterModel = GenerateThruster();
            Location = loc;
            Rotation = 180; //need to be pointing up initially
            Invincible = true;
        }

        /// <summary>
        /// Generates a GraphicsPath representing both the player's ship and the ship's thruster.
        /// </summary>
        /// <returns></returns>
        public GraphicsPath GenerateShipWithThruster()
        {
            PointF[] outputTriangle = new PointF[4];
            outputTriangle[0] = new PointF(0, 13);
            outputTriangle[1] = new PointF(8, -9);
            outputTriangle[2] = new PointF(0, -1);
            outputTriangle[3] = new PointF(-8, -9);
            GraphicsPath gp = new GraphicsPath();
            gp.AddPolygon(outputTriangle);

            PointF[] outputTriangle2 = new PointF[4];
            outputTriangle2[0] = new PointF(0, -1);
            outputTriangle2[1] = new PointF(-2, -3);
            outputTriangle2[2] = new PointF(0, -13);
            outputTriangle2[3] = new PointF(2, -3);
            GraphicsPath gp2 = new GraphicsPath();
            gp2.AddPolygon(outputTriangle2);

            gp.AddPath(gp2, true); //combine the paths
            return gp;
        }

        /// <summary>
        /// Generates a GraphicsPath representing the player's ship.
        /// </summary>
        /// <returns></returns>
        public GraphicsPath GenerateShip()
        {
            PointF[] outputTriangle = new PointF[4];
            outputTriangle[0] = new PointF(0, 13);
            outputTriangle[1] = new PointF(8, -9);
            outputTriangle[2] = new PointF(0, -1);
            outputTriangle[3] = new PointF(-8, -9);
            GraphicsPath gp = new GraphicsPath();
            gp.AddPolygon(outputTriangle);
            return gp;
        }

        /// <summary>
        /// Generates a GraphicsPath representing the thruster of the ship.
        /// </summary>
        /// <returns></returns>
        public GraphicsPath GenerateThruster()
        {
            PointF[] outputTriangle = new PointF[4];
            outputTriangle[0] = new PointF(0, -1);
            outputTriangle[1] = new PointF(-2, -3);
            outputTriangle[2] = new PointF(0, -6);
            outputTriangle[3] = new PointF(2, -3);
            GraphicsPath gp = new GraphicsPath();
            gp.AddPolygon(outputTriangle);
            return gp;
        }

        /// <summary>
        /// Translates and rotates the GraphicsPath passed to it.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public GraphicsPath GetPath(GraphicsPath model)
        {
            Matrix m = new Matrix(); //matrix to store our results
            GraphicsPath gp = (GraphicsPath)model.Clone(); //clone the model

            m.Translate(Location.X, Location.Y); //translate current point onto this matrix
            m.Rotate(Rotation, MatrixOrder.Prepend); //rotate the matrix by our object's rotation amount
            gp.Transform(m); //then transform the GP according to our above matrix

            return gp;
        }

        /// <summary>
        /// Draws the ship and thruster models.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        public override void Render(Graphics g, Color c)
        {
            //draw the ship
            if (Invincible && Thruster)
                g.DrawPath(new Pen(RandomColor()), GetPath(Model));
            else if (!Invincible && Thruster)
                g.DrawPath(new Pen(c), GetPath(Model));
            else if (Invincible && !Thruster)
                g.DrawPath(new Pen(RandomColor()), GetPath(ShipModel));
            else if (!Invincible && !Thruster)
                g.DrawPath(new Pen(c), GetPath(ShipModel));

            //draw the missiles
            Missiles.ForEach(x => x.Render(g, Color.White));
        }

        /// <summary>
        /// Fires the gun.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="dir"></param>
        /// <param name="spd"></param>
        public void FireGun(PointF loc, float dir, float spd) => Missiles.Add(new Missile(loc, dir, spd));

        /// <summary>
        /// Updates the location, speed, direction, and rotation of the ship. Curates the Missile list too.
        /// </summary>
        /// <param name="sz"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="thrust"></param>
        /// <param name="shoot"></param>
        public void Tick(Size sz, bool left, bool right, bool thrust, bool shoot)
        {
            if (InvincibleTimer-- <= 0)
                Invincible = false;

            //update ship's rotation
            if (left)
                Rotation -= RotationIncrement;
            if (right)
                Rotation += RotationIncrement;

            //calculate distance traveled in this tick
            if (thrust)
            {
                DeltaX += (-(float)Math.Sin(Rotation * Math.PI / 180) * Acceleration).LimitToRange(-20, 20);
                DeltaY += ((float)Math.Cos(Rotation * Math.PI / 180) * Acceleration).LimitToRange(-20, 20);
            }

            //need to limit the speed somehow
            Speed = ((float)Math.Sqrt(Math.Pow(DeltaX, 2) + Math.Pow(DeltaY, 2))).LimitToRange(0, 20);
            Direction = Rotation; //save the current rotation so that the missile can inherit it

            //update ship position
            if (Location.X + DeltaX < 0)
                Location.X = sz.Width; //wrap horizontaly left to right
            else if (Location.X + DeltaX >= sz.Width)
                Location.X = 0; //wrap horizontally right to left
            else
                Location.X += DeltaX; //update X coordinate normally if we aren't wrapping

            if (Location.Y + DeltaY < 0)
                Location.Y = sz.Height; //wrap vertically top to bottom
            else if (Location.Y + DeltaY >= sz.Height)
                Location.Y = 0; //wrap vertically bottom to top
            else
                Location.Y += DeltaY; //update Y coordinate normally if we aren't wrapping

            if (CurrentGunCooldown-- < 0) //decrease the gun cooldown by 1 tick
                CurrentGunCooldown = 0; //allow for the ship to fire

            if (shoot && CurrentGunCooldown <= 0 && Missiles.Count < MAX_NUM_MISSILES) //can only have 8 missiles on screen at once
            {
                FireGun(Location, Direction, Speed); //fire the gun 
                CurrentGunCooldown = GunCooldown; //reset gun cooldown
            }

            Missiles.RemoveAll(x => x.MarkedForDeath); //remove the expired asteroids

            Missiles.ForEach(x => x.Tick(sz)); //update ship's missiles
        }
    }
}