using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Asteroids
{
    public abstract class ShapeBase
    {
        public static Random RNG { get; set; }                //RNG for the whole project
        public float Rotation { get; set; }                   //current rotation/angle of the shape
        public float RotationIncrement { get; set; } = 5.0f;  //amount to increment rotation/angle by
        public float Direction { get; set; }                  //the heading of our shape
        public float DeltaX { get; set; }                     //amount traveled in X axis per tick
        public float DeltaY { get; set; }                     //amount traveled in Y axis per tick
        public float Speed { get; set; } //= 0.1f;              //speed of shape
        public float Acceleration { get; set; } = 0.05f;       //acceleration of shape
        public PointF Location;                               //location of shape
        public bool MarkedForDeath { get; set; }              //controls if the shape should be removed
        public GraphicsPath Model { get; set; }               //shape's model

        static ShapeBase() => RNG = new Random();

        protected ShapeBase(PointF loc) => Location = loc;

        /// <summary>
        /// Generates a random color.
        /// </summary>
        /// <returns></returns>
        public Color RandomColor() => Color.FromArgb(RNG.Next(256), RNG.Next(256), RNG.Next(256));

        /// <summary>
        /// Translates and rotates the shape's model according to its location and rotation.
        /// </summary>
        /// <returns></returns>
        public virtual GraphicsPath GetPath()
        {
            Matrix m = new Matrix(); //matrix to store our results
            GraphicsPath gp = (GraphicsPath)Model.Clone(); //clone the model

            m.Translate(Location.X, Location.Y); //translate current point onto this matrix
            m.Rotate(Rotation, MatrixOrder.Prepend); //rotate the matrix by our object's rotation amount
            gp.Transform(m); //then transform the GP according to our above matrix

            return gp;
        }

        /// <summary>
        /// Render the shape.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        public abstract void Render(Graphics g, Color c);
    }
}
