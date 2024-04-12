using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GearCreation.Geometry;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Input;

namespace GearCreation
{
    public class CreateEndGearCutter : GH_Component
    {

        Point3d customized_part_location = new Point3d(0, 0, 0);
        private Brep currModel;
        private List<Point3d> surfacePts;
        private Point3dList selectedPts;
        private Guid currModelObjId;
        private RhinoDoc myDoc;
        ObjectAttributes solidAttribute, lightGuideAttribute, redAttribute, yellowAttribute, soluableAttribute;
        private bool end_button_clicked;
        Brep cutter = new Brep();
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public CreateEndGearCutter()
          : base("CreateEndGearCutter", "GearTrain",
              "This component create a gear train",
              "GearCreation", "Main")
        {
            myDoc = RhinoDoc.ActiveDoc;
            surfacePts = new List<Point3d>();
            selectedPts = new Point3dList();

            int solidIndex = myDoc.Materials.Add();
            Material solidMat = myDoc.Materials[solidIndex];
            solidMat.DiffuseColor = Color.White;
            solidMat.SpecularColor = Color.White;
            solidMat.Transparency = 0;
            solidMat.CommitChanges();
            solidAttribute = new ObjectAttributes();
            //solidAttribute.LayerIndex = 2;
            solidAttribute.MaterialIndex = solidIndex;
            solidAttribute.MaterialSource = ObjectMaterialSource.MaterialFromObject;
            solidAttribute.ObjectColor = Color.White;
            solidAttribute.ColorSource = ObjectColorSource.ColorFromObject;

            int lightGuideIndex = myDoc.Materials.Add();
            Material lightGuideMat = myDoc.Materials[lightGuideIndex];
            lightGuideMat.DiffuseColor = Color.Orange;
            lightGuideMat.Transparency = 0.3;
            lightGuideMat.SpecularColor = Color.Orange;
            lightGuideMat.CommitChanges();
            lightGuideAttribute = new ObjectAttributes();
            //orangeAttribute.LayerIndex = 3;
            lightGuideAttribute.MaterialIndex = lightGuideIndex;
            lightGuideAttribute.MaterialSource = ObjectMaterialSource.MaterialFromObject;
            lightGuideAttribute.ObjectColor = Color.Orange;
            lightGuideAttribute.ColorSource = ObjectColorSource.ColorFromObject;

            int redIndex = myDoc.Materials.Add();
            Material redMat = myDoc.Materials[redIndex];
            redMat.DiffuseColor = Color.Red;
            redMat.Transparency = 0.3;
            redMat.SpecularColor = Color.Red;
            redMat.CommitChanges();
            redAttribute = new ObjectAttributes();
            //redAttribute.LayerIndex = 4;
            redAttribute.MaterialIndex = redIndex;
            redAttribute.MaterialSource = ObjectMaterialSource.MaterialFromObject;
            redAttribute.ObjectColor = Color.Red;
            redAttribute.ColorSource = ObjectColorSource.ColorFromObject;

            int yellowIndex = myDoc.Materials.Add();
            Material yellowMat = myDoc.Materials[yellowIndex];
            yellowMat.DiffuseColor = Color.Yellow;
            yellowMat.Transparency = 0.3;
            yellowMat.SpecularColor = Color.Yellow;
            yellowMat.CommitChanges();
            yellowAttribute = new ObjectAttributes();
            //yellowAttribute.LayerIndex = 4;
            yellowAttribute.MaterialIndex = yellowIndex;
            yellowAttribute.MaterialSource = ObjectMaterialSource.MaterialFromObject;
            yellowAttribute.ObjectColor = Color.Yellow;
            yellowAttribute.ColorSource = ObjectColorSource.ColorFromObject;

            int soluableIndex = myDoc.Materials.Add();
            Material soluableMat = myDoc.Materials[soluableIndex];
            soluableMat.DiffuseColor = Color.Green;
            soluableMat.Transparency = 0.3;
            soluableMat.SpecularColor = Color.Green;
            soluableMat.CommitChanges();
            soluableAttribute = new ObjectAttributes();
            //yellowAttribute.LayerIndex = 4;
            soluableAttribute.MaterialIndex = soluableIndex;
            soluableAttribute.MaterialSource = ObjectMaterialSource.MaterialFromObject;
            soluableAttribute.ObjectColor = Color.Green;
            soluableAttribute.ColorSource = ObjectColorSource.ColorFromObject;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Button", "B", "Button to create cutter", GH_ParamAccess.item);
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
            bool isPressed = false;
            if (!DA.GetData(0, ref isPressed))
            {
                return;
            }
            if (isPressed)
            {
                
                Brep brep = SelectGearExit(out Plane cutterPlane);

                if(brep != new Brep())
                {
                    Guid guid = myDoc.Objects.Add(cutter);
                    Essentials essentials = new Essentials();
                    essentials.Cutter = guid;
                    essentials.CutterPlane = cutterPlane;
                    essentials.CutterThickness = 3;
                    essentials.MainModel = currModelObjId;

                    DA.SetData(0, essentials);
                }
                    

            }
        }

        private Brep SelectGearExit(out Plane cutterPlane)
        {
            ObjRef objSel_ref1;
            end_button_clicked = false;


            var rc = RhinoGet.GetOneObject("Select a model (geometry): ", false, ObjectType.AnyObject, out objSel_ref1);
            if (rc == Rhino.Commands.Result.Success)
            {
                currModelObjId = objSel_ref1.ObjectId;
                ObjRef currObj = new ObjRef(currModelObjId);
                currModel = currObj.Brep(); //The model body

                ObjectType objectType = currObj.Geometry().ObjectType;

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


                #region Generate the base PCB part
                BoundingBox boundingBox = currModel.GetBoundingBox(true);
                double base_z = boundingBox.Min.Z;
                double base_x = (boundingBox.Max.X - boundingBox.Min.X) / 2 + boundingBox.Min.X;
                double base_y = (boundingBox.Max.Y - boundingBox.Min.Y) / 2 + boundingBox.Min.Y;
                Point3d base_center = new Point3d(base_x, base_y, base_z);
                #endregion


                #region Create interactive dots around the model body for users to select

                if (currModel == null)
                {
                    cutterPlane = new Plane();
                    return cutter;
                }
                    


                // Find the candidate positions to place dots
                BoundingBox meshBox = currModel.GetBoundingBox(true);
                double w = meshBox.Max.X - meshBox.Min.X;
                double l = meshBox.Max.Y - meshBox.Min.Y;
                double h = meshBox.Max.Z - meshBox.Min.Z;
                double offset = 5;

                for (double i = 0; i < h+10; i += 1)
                {
                    Point3d Origin = new Point3d(w / 2, l / 2, i);
                    Point3d xPoint = new Point3d(boundingBox.Max.X + offset, l / 2, i);
                    Point3d yPoint = new Point3d(w / 2, boundingBox.Max.Y + offset, i);

                    Plane plane = new Plane(Origin, xPoint, yPoint);
                    PlaneSurface planeSurface = PlaneSurface.CreateThroughBox(plane, boundingBox);
                    //myDoc.Objects.Add(planeSurface);

                    Intersection.BrepSurface(currModel, planeSurface, myDoc.ModelAbsoluteTolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints);

                    //Create Points on the Curve
                    if (intersectionCurves != null)
                    {
                        if (intersectionCurves.Length != 0)
                        {
                            foreach (Curve curve in intersectionCurves)
                            {
                                double[] curveParams = curve.DivideByLength(2, true, out Point3d[] points);
                                if(points != null)
                                    surfacePts.AddRange(points);
                            }
                        }
                    }
                }

                // Create a copy of the current model to put on dots
                Brep solidDupBrep = currModel.DuplicateBrep();
                Guid dupObjID = myDoc.Objects.AddBrep(solidDupBrep, yellowAttribute);
                myDoc.Objects.Hide(currModelObjId, true);

                // Put dots on the copy
                List<Guid> pts_normals = new List<Guid>();
                foreach (Point3d point in surfacePts)
                {
                    Guid pointID = myDoc.Objects.AddPoint(point);
                    pts_normals.Add(pointID);
                }
                myDoc.Views.Redraw();

                #endregion

                #region ask the user to select points for a cutter plane

                #region Create a line that perfectly projected on the surface of the main model (Not complete) ----------------------> Commented
                //List<Guid> cutterCurves = new List<Guid>();

                //RhinoApp.WriteLine("We need to cut your model for your end effector. Please select 5 points that best describe your cutter plane ---------------------------------------------------");

                //while (!end_button_clicked)
                //{
                //    ObjRef pointRef;

                //    var getSelectedPts = RhinoGet.GetOneObject("Please select your Point " +(selectedPts.Count + 1) + ": ", false, ObjectType.Point, out pointRef);
                //    RhinoApp.WriteLine("You selected a point at: " + pointRef.Point().Location.X + ", " + pointRef.Point().Location.Y + ", " + pointRef.Point().Location.Z);

                //    if (pointRef != null)
                //    {
                //        Point3d tempPt = new Point3d(pointRef.Point().Location.X, pointRef.Point().Location.Y, pointRef.Point().Location.Z);
                //        double x = pointRef.Point().Location.X;
                //        double y = pointRef.Point().Location.Y;
                //        double z = pointRef.Point().Location.Z;

                //        //Check if the selected point is in selectedPts
                //        //1. If so, Get rid of the bounding box
                //        //2. If not, store the selected point and display bounding box

                //        if(selectedPts.Any(pt => pt.Equals(tempPt)))
                //        {
                //            continue;
                //        }
                //        selectedPts.Add(tempPt);

                //        Guid tempPt_ID = pointRef.ObjectId;


                //        #region Display the line from selectedPts[0] to selectedPts[n]
                //        if(selectedPts.Count > 1)
                //        {

                //            Line line = new Line(selectedPts[selectedPts.Count-2], tempPt);
                //            Curve curve = line.ToNurbsCurve();
                //            Vector3d direction = new Vector3d(0, 0, 1);

                //            Curve[] projectedCurve = Curve.ProjectToBrep(curve, currModel, direction, myDoc.ModelAbsoluteTolerance);

                //            myDoc.Objects.Add(projectedCurve[0], redAttribute);

                //            //cutterCurves.Add(myDoc.Objects.Add(projectedCurve[0], redAttribute));
                //        }

                //        myDoc.Views.Redraw();

                //        #endregion

                //        RhinoApp.KeyboardEvent += OnKeyboardEvent;
                //    }

                //}
                #endregion

                int count = 0;
                List<Curve> cutterCurves = new List<Curve>();
                RhinoApp.WriteLine("We need to cut your model for your end effector. Please select 6 points that best describe your cutter plane ---------------------------------------------------");


                while (count < 3)
                {
                    ObjRef pointRef;

                    var getSelectedPts = RhinoGet.GetOneObject("Please select your Point " + (count + 1) + ": ", false, ObjectType.Point, out pointRef);
                    RhinoApp.WriteLine("You selected a point at: " + pointRef.Point().Location.X + ", " + pointRef.Point().Location.Y + ", " + pointRef.Point().Location.Z);

                    if (pointRef != null)
                    {
                        Point3d tempPt = new Point3d(pointRef.Point().Location.X, pointRef.Point().Location.Y, pointRef.Point().Location.Z);

                        //Check if the selected point is in selectedPts
                        //1. If so, Get rid of the bounding box
                        //2. If not, store the selected point and display bounding box

                        if (selectedPts.Any(pt => pt.Equals(tempPt)))
                        {
                            RhinoApp.WriteLine("Please choose point that are NOT red");
                            continue;
                        }
                        selectedPts.Add(tempPt);

                        Guid tempPt_ID = pointRef.ObjectId;
                        myDoc.Objects.ModifyAttributes(pointRef, redAttribute, true);

                        myDoc.Views.Redraw();

                        count++;
                    }

                }

                #region Make all points to a plane, but only (1,0,0), (0,1,0), (0,0,1), so -------------->Commented
                ////Use the plane that has the least squared error
                //double x_diff = 0;
                //double y_diff = 0;
                //double z_diff = 0;
                //double x_total = 0;
                //double y_total = 0;
                //double z_total = 0;

                //List<double> x = new List<double>();
                //List<double> y = new List<double>();
                //List<double> z = new List<double>();

                //for (int i = 0; i < selectedPts.Count; i++)
                //{
                //    x_total += selectedPts[i].X;
                //    y_total += selectedPts[i].Y;
                //    z_total += selectedPts[i].Z;
                //    x.Add(selectedPts[i].X);
                //    y.Add(selectedPts[i].Y);
                //    z.Add(selectedPts[i].Z);
                //}

                //double x_mean = x_total / x.Count;
                //double y_mean = y_total / y.Count;
                //double z_mean = z_total / z.Count;

                //double x_std = Math.Sqrt(x.Sum(value => Math.Pow(value - x_mean, 2)) / selectedPts.Count);
                //double y_std = Math.Sqrt(y.Sum(value => Math.Pow(value - y_mean, 2)) / selectedPts.Count);
                //double z_std = Math.Sqrt(z.Sum(value => Math.Pow(value - z_mean, 2)) / selectedPts.Count);

                //if(z_std <= y_std && z_std <= x_std)
                //{
                //    selectedPts.SetAllZ(z_mean);
                //}
                //else if(x_std <= y_std && x_std <= z_std)
                //{
                //    selectedPts.SetAllX(x_mean);
                //}
                //else if(y_std <= x_std && y_std <= z_std)
                //{
                //    selectedPts.SetAllY(y_mean);
                //}
                #endregion

                for (int i = 0; i < selectedPts.Count - 1; i++)
                {
                    Line line = new Line(selectedPts[i], selectedPts[i + 1]);
                    cutterCurves.Add(line.ToNurbsCurve());
                }

                Line line1 = new Line(selectedPts[selectedPts.Count - 1], selectedPts[0]);
                cutterCurves.Add(line1.ToNurbsCurve());
                Curve[] cutterCurve = Curve.JoinCurves(cutterCurves);

                cutterPlane = new Plane();
                cutterCurve[0].TryGetPlane(out cutterPlane);
                

                Line rail = new Line(selectedPts[0], cutterPlane.Normal, 3);
                cutter = Brep.CreateFromSweep(rail.ToNurbsCurve(), cutterCurve[0], true, myDoc.ModelAbsoluteTolerance)[0];
                cutter = cutter.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
                //cutter.Flip();
                //myDoc.Objects.Add(cutterCurve[0]);

                Point3d center = cutter.GetBoundingBox(true).Center;
                Transform centerTrans = Transform.Scale(center, 2);
                

                cutter.Transform(centerTrans);

                #endregion

                //Kill all dots and duplicate brep, Show the original brep
                myDoc.Objects.Delete(dupObjID, true);
                myDoc.Objects.Show(currModelObjId, true);
                foreach (var ptsID in pts_normals)
                {
                    myDoc.Objects.Delete(ptsID, true);
                }

                return cutter;

            }
            cutterPlane = new Plane();
            return cutter;
        }

        public void OnKeyboardEvent(int key)
        {
            if (key == 13)
            {
                end_button_clicked = true;
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
            get { return new Guid("66711CB7-C973-4C22-BD6E-77A5867DF035"); }
        }
    }
}