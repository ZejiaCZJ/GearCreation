using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using ObjRef = Rhino.DocObjects.ObjRef;
using System.CodeDom;
using GearCreation.Geometry;

namespace GearCreation
{
    public class ScaleCutter : GH_Component
    {
        RhinoDoc myDoc = RhinoDoc.ActiveDoc;
        private static double old_factor = 1.0;
        private static double old_thickness = 1.0;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ScaleCutter()
          : base("ScaleCutter", "scale",
              "This component is for scaling the cutter brep",
              "GearCreation", "Main")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Essentials", "E", "The essential that contains: main model, cutter, cutter plane, cutter thickness, cutted models", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale Factor", "factor", "The scale factor of the cutter", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness", "thick", "The thickness of the cutter", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Essentials", "E", "The essential that contains: main model, cutter, cutter plane, cutter thickness, cutted models", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Essentials original_essentials = new Essentials();
            double factor = 1;
            double thickness = 1;

            if (!DA.GetData(0, ref original_essentials))
                return;
            if (!DA.GetData(1, ref factor))
                return;
            if (!DA.GetData(2, ref thickness))
                return;

            Essentials essentials = original_essentials.Duplicate(original_essentials);

            ObjRef objRef = new ObjRef(essentials.Cutter);
            Brep cutter = objRef.Brep();


            double new_factor = factor/old_factor;
            double new_thickness = thickness/old_thickness;

            Point3d center = cutter.GetBoundingBox(true).Center;
            Transform centerTrans = Transform.Scale(center, new_factor);

            if(new_factor == 1)
            {
                Transform thicknessTrans = Transform.Scale(essentials.CutterPlane, 1, 1, new_thickness);
                myDoc.Objects.Transform(objRef, thicknessTrans, true);
                essentials.CutterThickness = essentials.CutterThickness * new_thickness;
            }
            else
            {
                myDoc.Objects.Transform(objRef, centerTrans, true);
                essentials.CutterThickness = essentials.CutterThickness * new_factor;
            }
               

            

            DA.SetData(0, essentials);

            old_factor = factor;
            old_thickness = thickness;
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
            get { return new Guid("91A988B0-53C2-4DCF-81C0-F899E4D5FE4A"); }
        }
    }
}