using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;

namespace GearCreation.Test
{
    public class MakeBrep : GH_Component
    {

        private Brep currModel;
        private List<Point3d> surfacePts;
        private List<Point3d> selectedPts;
        private Guid currModelObjId;
        private RhinoDoc myDoc;

        /// <summary>
        /// Initializes a new instance of the MakeBrep class.
        /// </summary>
        public MakeBrep()
          : base("MakeBrep", "MakeBrep",
              "Make a mesh to brep",
              "GearCreation", "Test")
        {
            myDoc = RhinoDoc.ActiveDoc;
            currModelObjId = Guid.Empty;
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
            bool start_button_clicked = false;


            if (!DA.GetData(0, ref start_button_clicked))
                return;
            ObjRef objSel_ref1;

            if(start_button_clicked)
            {
                var rc = RhinoGet.GetOneObject("Select a model (geometry): ", false, ObjectType.AnyObject, out objSel_ref1);

                if (rc == Rhino.Commands.Result.Success)
                {
                    currModelObjId = objSel_ref1.ObjectId;
                    ObjRef currObj = new ObjRef(currModelObjId);
                    currModel = currObj.Brep(); //The model body

                    if (currObj.Geometry().ObjectType == ObjectType.Mesh)
                    {
                        //Mesh
                        Mesh currModel_Mesh = currObj.Mesh();

                        //TODO: Convert Mesh into Brep; or just throw an error to user saying that only breps are allowed 
                        currModel = Brep.CreateFromMesh(currModel_Mesh, false);
                        currModelObjId = myDoc.Objects.AddBrep(currModel);
                        myDoc.Objects.Delete(currObj.ObjectId, false);

                        myDoc.Views.Redraw();
                    }

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
            get { return new Guid("06BF318C-B06C-4779-98AF-A8F8D354DD8D"); }
        }
    }
}