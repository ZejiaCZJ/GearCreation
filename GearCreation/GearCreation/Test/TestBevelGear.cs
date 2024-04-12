using System;
using System.Collections.Generic;
using GearCreation.Geometry;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;

namespace GearCreation.Test
{
    public class TestBevelGear : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the gra class.
        /// </summary>
        public TestBevelGear()
          : base("TestBevelGear", "TestBevel",
              "Test Bevel Gear angles",
              "GearCreation", "Test")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Button", "B", "Button to create gear", GH_ParamAccess.item);
            pManager.AddNumberParameter("Cone Angle", "A", "Cone Angle", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool isPressed = false;
            double coneAngle = 30;
            double mod = 1.5;
            double pressure_angle = 20;
            double Thickness = 5;
            double selfRotAngle = 0;
            bool movable = true;
            RhinoDoc myDoc = RhinoDoc.ActiveDoc;

            if (!DA.GetData(0, ref isPressed))
            {
                return;
            }
            if (!DA.GetData(1, ref coneAngle))
            {
                return;
            }

            if (isPressed)
            {
                if (coneAngle > 90)
                {
                    Point3d center_point = new Point3d(0, 0, 5);
                    Vector3d gear_direction = new Vector3d(0, 0, -1);
                    Vector3d gear_x_dir = new Vector3d(0, 0, 0);
                    int first_teethNum = 20;
                    BevelGear gear = new BevelGear(center_point, gear_direction, gear_x_dir, first_teethNum, mod, pressure_angle, Thickness, selfRotAngle, coneAngle - 90, movable);

                    Point3d second_center_point = new Point3d(5, 0, 5);
                    Vector3d second_gear_direction = new Vector3d(-1, 0, 0);
                    Vector3d second_gear_x_dir = new Vector3d(0, 0, 0);
                    int second_teethNum = 20;
                    BevelGear second_gear = new BevelGear(second_center_point, second_gear_direction, second_gear_x_dir, second_teethNum, mod, pressure_angle, Thickness, selfRotAngle, coneAngle - 90, movable);

                    Point3d third_center_point = new Point3d(10, 0, 5);
                    Vector3d third_gear_direction = new Vector3d(-1, 0, 0);
                    Vector3d third_gear_x_dir = new Vector3d(0, 0, 0);
                    int third_teethNum = 30;
                    BevelGear third_gear = new BevelGear(third_center_point, third_gear_direction, third_gear_x_dir, third_teethNum, mod, pressure_angle, Thickness, selfRotAngle, 90, movable);

                    Point3d fourth_center_point = new Point3d(0, 0, 0);
                    Vector3d fourth_gear_direction = new Vector3d(-1, 0, 0);
                    Vector3d fourth_gear_x_dir = new Vector3d(0, 0, 0);
                    int fourth_teethNum = 30;
                    BevelGear fourth_gear = new BevelGear(fourth_center_point, fourth_gear_direction, fourth_gear_x_dir, fourth_teethNum, mod, pressure_angle, Thickness, selfRotAngle, 90, movable);


                    myDoc.Objects.Add(gear.Model);
                    myDoc.Objects.Add(second_gear.Model);
                    myDoc.Objects.Add(third_gear.Model);
                    myDoc.Objects.Add(fourth_gear.Model);
                }
                else
                {
                    Point3d center_point = new Point3d(0, 0, 0);
                    Vector3d gear_direction = new Vector3d(-1, 0, 0);
                    Vector3d gear_x_dir = new Vector3d(0, 0, 0);
                    int first_teethNum = 10;
                    BevelGear gear = new BevelGear(center_point, gear_direction, gear_x_dir, first_teethNum, mod, pressure_angle, Thickness, selfRotAngle, coneAngle, movable);

                    Point3d second_center_point = new Point3d(0, 0, 5);
                    Vector3d second_gear_direction = new Vector3d(0, 0, 1);
                    Vector3d second_gear_x_dir = new Vector3d(0, 0, 0);
                    int second_teethNum = 20;
                    BevelGear second_gear = new BevelGear(second_center_point, second_gear_direction, second_gear_x_dir, second_teethNum, mod, pressure_angle, Thickness, selfRotAngle, coneAngle, movable);


                    myDoc.Objects.Add(gear.Model);
                    myDoc.Objects.Add(second_gear.Model);
                }

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
            get { return new Guid("37C894AD-1B24-45D2-A9C2-D56BF76D542E"); }
        }
    }
}