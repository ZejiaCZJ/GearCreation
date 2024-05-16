﻿using System;
using System.Collections.Generic;
using GearCreation.Geometry;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;

namespace GearCreation.Test
{
    public class AddBevelGearSet_135 : GH_Component
    {

        double module = 1.5;
        double pressure_angle = 20;
        double thickness = 5;
        

        /// <summary>
        /// Initializes a new instance of the AddBevelGearSet_135 class.
        /// </summary>
        public AddBevelGearSet_135()
          : base("AddBevelGearSet_135", "BevelGearSet",
              "This component adds a set of bevel gear with 135 degree of cone angle",
              "GearCreation", "Test")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Button", "B", "Button to create gear", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool isPressed = false;
            if (!DA.GetData(0, ref isPressed))
            {
                return;
            }

            if (isPressed)
            {
                #region first bevel gear
                Point3d first_center_point = new Point3d(0, 0, 0);
                Vector3d first_gear_direction = new Vector3d(0, 0, 1);
                Vector3d first_gear_x_dir = new Vector3d(0, 0, 0);
                int first_teethNum = 30;
                double first_selfRotAngle = 0;
                bool first_movable = true;
                double first_coneAngle = 90;
                RhinoDoc myDoc = RhinoDoc.ActiveDoc;

                BevelGear first_gear = new BevelGear(first_center_point, first_gear_direction, first_gear_x_dir, first_teethNum, module, pressure_angle, thickness, first_selfRotAngle, first_coneAngle, first_movable);

                myDoc.Objects.Add(first_gear.Model);

                //齿轮中间柱子
                Point3d startPt = new Point3d(0, 0, first_center_point.Z + 42);
                Point3d endPt = new Point3d(0, 0, first_gear.Model.GetBoundingBox(true).Min.Z - 2);
                Line line = new Line(startPt, endPt);
                Curve pipeCurve = line.ToNurbsCurve();
                Brep firstPipe = Brep.CreatePipe(pipeCurve, 2, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];

                //齿轮下方垫片
                startPt = new Point3d(0, 0, first_gear.Model.GetBoundingBox(true).Min.Z - 1);
                line = new Line(startPt, endPt);
                pipeCurve = line.ToNurbsCurve();
                Brep firstBottomGasketPipe = Brep.CreatePipe(pipeCurve, 4, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];

                //齿轮上方垫片
                startPt = new Point3d(0, 0, first_gear.Model.GetBoundingBox(true).Max.Z + 2);
                endPt = new Point3d(0, 0, first_gear.Model.GetBoundingBox(true).Max.Z + 1);
                line = new Line(startPt, endPt);
                pipeCurve = line.ToNurbsCurve();
                Brep firstTopGasketPipe = Brep.CreatePipe(pipeCurve, 6, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];


                //把齿轮柱子和垫片合在一起
                List<Brep> breps = new List<Brep>();
                breps.Add(firstPipe);
                breps.Add(firstBottomGasketPipe);
                breps.Add(firstTopGasketPipe);
                firstPipe = Brep.CreateBooleanUnion(breps, myDoc.ModelAbsoluteTolerance)[0];
                myDoc.Objects.Add(firstPipe);
                #endregion

                #region second bevel gear
                int second_teethNum = 30;
                double second_selfRotAngle = 360 / 30 / 2;
                bool second_movable = true;
                Point3d second_center_point = new Point3d(0, 0, 0);
                Vector3d second_gear_direction = new Vector3d(-1, 0, 0);
                Vector3d second_gear_x_dir = new Vector3d(0, 0, 0);

                double second_pitchDiameter = module * second_teethNum;
                double second_outDiameter = second_pitchDiameter + 2 * module;

                second_center_point.X = first_gear.Model.GetBoundingBox(true).Max.X;
                second_center_point.Z = first_gear.Model.GetBoundingBox(true).Min.Z + second_outDiameter / 2;

                //齿轮
                BevelGear second_gear = new BevelGear(second_center_point, second_gear_direction, second_gear_x_dir, second_teethNum, module, pressure_angle, thickness, second_selfRotAngle, 90.0, second_movable);
                myDoc.Objects.Add(second_gear.Model);
                #endregion

                #region third bevel gear
                int third_teethNum = 20;
                double third_selfRotAngle = 0;
                bool third_movable = true;
                Point3d third_centerPoint = new Point3d(0, 0, 0);

                third_centerPoint.X = second_gear.Model.GetBoundingBox(true).Min.X-0.65;
                third_centerPoint.Z = second_center_point.Z;

                BevelGear third_gear = new BevelGear(third_centerPoint, second_gear_direction, second_gear_x_dir, third_teethNum, module, pressure_angle, thickness, third_selfRotAngle, 45, third_movable);
                myDoc.Objects.Add(third_gear.Model);

                #endregion

                #region 给第二个和第三个齿轮加柱子和垫片
                //齿轮中间柱子
                startPt = new Point3d(second_center_point.X - 42, 0, second_center_point.Z);
                endPt = new Point3d(second_gear.Model.GetBoundingBox(true).Max.X + 2, 0, second_center_point.Z);
                line = new Line(startPt, endPt);
                pipeCurve = line.ToNurbsCurve();
                Brep secondPipe = Brep.CreatePipe(pipeCurve, 2, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];


                //齿轮底部垫片
                startPt = new Point3d(second_gear.Model.GetBoundingBox(true).Max.X + 1, 0, second_center_point.Z);
                line = new Line(startPt, endPt);
                pipeCurve = line.ToNurbsCurve();
                Brep secondBottomGasketPipe = Brep.CreatePipe(pipeCurve, 4, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];

                //齿轮上方垫片
                startPt = new Point3d(third_gear.Model.GetBoundingBox(true).Min.X - 2, 0, second_center_point.Z);
                endPt = new Point3d(third_gear.Model.GetBoundingBox(true).Min.X - 1, 0, second_center_point.Z);
                line = new Line(startPt, endPt);
                pipeCurve = line.ToNurbsCurve();
                Brep secondTopGasketPipe = Brep.CreatePipe(pipeCurve, 6, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];

                breps = new List<Brep>();
                breps.Add(secondPipe);
                breps.Add(secondBottomGasketPipe);
                breps.Add(secondTopGasketPipe);
                secondPipe = Brep.CreateBooleanUnion(breps, myDoc.ModelAbsoluteTolerance)[0];
                myDoc.Objects.Add(secondPipe);
                #endregion

                #region fourth bevel gear
                int fourth_teethNum = 20;
                Vector3d fourth_gear_direction = new Vector3d(-1, 0, -1);
                Vector3d fourth_gear_xdir = new Vector3d(0, 0, 0);
                double fourth_selfRotAngle = 360/fourth_teethNum/2;
                bool fourth_movable = true;
                Point3d fourth_centerPoint = new Point3d(8.5, 0, third_gear.Model.GetBoundingBox(true).Max.Z+10);

                BevelGear fourth_gear = new BevelGear(fourth_centerPoint, fourth_gear_direction, fourth_gear_xdir, fourth_teethNum, module, pressure_angle, thickness, fourth_selfRotAngle, 45, fourth_movable);
                myDoc.Objects.Add(fourth_gear.Model);
                #endregion
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C1F6E0CB-3209-44BA-92C2-8C62939B45B8"); }
        }
    }
}