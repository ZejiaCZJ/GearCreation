﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Components;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.DocObjects;
using Rhino.Collections;
using Rhino.Input.Custom;
using Rhino;
using Grasshopper.Kernel.Types.Transforms;
using System.Windows.Forms;

namespace GearCreation.Geometry
{
    public class Movement
    {
        //Define a physical movement, including linear and rotating movements.
        private Entity obj;
        private int type;//1 means linear movement, 2 means self-rotate, 3 means helix squeezing, and 4 means spiral rotating
        private double movementValue = 0;

        //private Point3d rotateCenter = Point3d.Unset;
        private Transform trans = Transform.Unset;
        private bool converge = false;
        public Entity Obj { get => obj; private set => obj = value; }

        /// <summary>
        /// 1 means linear movement, 2 means self-rotate, 3 means helix squeezing, and 4 means spiral rotating
        /// </summary>
        public int Type { get => type; private set => type = value; }
        //public Point3d RotateCenter { get => rotateCenter;private set => rotateCenter = value; }
        public Transform Trans { get => trans; set => trans = value; }
        public double MovementValue { get => movementValue; private set => movementValue = value; }
        public bool Converge { get => converge; protected set => converge = value; }

        /// <summary>
        /// Constructor for linear movement.
        /// </summary>
        /// <param name="Object">Object to be transformed</param>
        /// <param name="Tp">Type of movement, here should be 1 for linear movement</param>
        /// <param name="m">Transform</param>
        public Movement(Entity Object, int Tp, Transform m)
        {
            obj = Object;
            Type = Tp;
            Trans = m;
            converge = false;
        }
        public Movement(Entity Object, int Tp, double Value, Transform m)
        {
            obj = Object;
            Type = Tp;
            Trans = m;
            converge = false;
            movementValue = Value;
        }
        /// <summary>
        /// Constructor for self-rotation movement
        /// </summary>
        /// <param name="Object">Object to be transformed,here should be a gear</param>
        /// <param name="Tp">Type of movement, here should be 2 for self rotation</param>
        /// <param name="deg">Degree of rotation</param>
        public Movement(Entity Object, int Tp, double value)
        {
            if (Tp == 1)
            {
                obj = Object;
                Type = Tp;
                movementValue = value;
            }
            else if (Tp == 2)
            {
                if (Object.GetType() != typeof(SpurGear))
                {
                    obj = Object;
                    Type = Tp;
                    movementValue = value;
                    trans = Transform.Identity;
                }
                else
                {
                    obj = Object;
                    SpurGear g = (SpurGear)Object;
                    Type = Tp;
                    Trans = Transform.Rotation(movementValue / 180 * Math.PI, g.Direction, g.CenterPoint);
                    movementValue = value;
                }
            }
            //else if (Tp == 3)
            //{
            //    if (Object.GetType() != typeof(Helix))
            //    { throw new Exception("Movement of type3 only support springs."); }
            //    obj = Object;
            //    Type = Tp;
            //    movementValue = value;
            //}
            //else if (Tp == 4)
            //{
            //    if (Object.GetType() != typeof(Spiral))
            //    { throw new Exception("Movement of type4 only support spirals."); }
            //    obj = Object;
            //    Spiral s = (Spiral)obj;
            //    Type = Tp;
            //    movementValue = value;
            //    trans = Transform.Rotation(-value, s.Direction, s.CenterPoint);
            //}
            else
            { throw new Exception("Movement of this type hasn't been implemented."); }
        }

        public bool Activate()
        {
            //string body = string.Format("A movement on {0} typed {1} with value {2} is called in movement", this.Obj.GetType(),this.type,this.MovementValue);
            //Rhino.RhinoApp.WriteLine(body);
            return obj.Move(this);
        }
        public void SetConverge()
        {
            converge = true;
        }

    }
}