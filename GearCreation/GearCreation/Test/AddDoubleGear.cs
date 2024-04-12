using System;
using System.Collections.Generic;
using GearCreation.Geometry;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;

namespace GearCreation.Test
{
    public class AddDoubleGear : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AddDoubleGear class.
        /// </summary>
        public AddDoubleGear()
          : base("AddDoubleGear", "DoubleGear",
              "This component adds a Double gear to the view",
              "GearCreation", "Test")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Button", "B", "Button to create gear", GH_ParamAccess.item);
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
            if (!DA.GetData(0, ref isPressed))
            {
                return;
            }

            if (isPressed)
            {
                Point3d center_point = new Point3d(0, 0, 0);
                Vector3d gear_direction = new Vector3d(0, 0, 0);
                Vector3d gear_x_dir = new Vector3d(0, 0, 0);
                int first_teethNum = 10;
                int second_teethNum = 20;
                double mod = 1.5;
                double pressure_angle = 20;
                double Thickness = 5;
                double selfRotAngle = 0;
                double coneAngle = 90;
                bool movable = true;
                RhinoDoc myDoc = RhinoDoc.ActiveDoc;

                SpurGear gear = new SpurGear(center_point, gear_direction, gear_x_dir, first_teethNum, mod, pressure_angle, Thickness, selfRotAngle, movable);

                Point3d second_center_point = new Point3d(0, 0, 5);
                SpurGear second_gear = new SpurGear(second_center_point, gear_direction, gear_x_dir, second_teethNum, mod, pressure_angle, Thickness, selfRotAngle, movable);


                myDoc.Objects.Add(gear.Model);
                myDoc.Objects.Add(second_gear.Model);
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
            get { return new Guid("E9AE36E6-B9E3-4E65-87CD-627E4B4EEBF2"); }
        }
    }
}