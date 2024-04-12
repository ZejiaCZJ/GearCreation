using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GearCreation.Geometry;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Input;
using Rhino.Runtime;

namespace GearCreation.Test
{
    public class GearCreation : GH_Component
    {
        private Point3d customized_part_location;
        private Point3d center_point = new Point3d(0, 0, 0);
        private Vector3d gear_direction = new Vector3d(0, 0, 1);
        private Vector3d gear_x_dir = new Vector3d(0, 0, 0);
        private int teethNum = 30;
        private double mod = 1.5;
        private double pressure_angle = 20;
        private double Thickness = 5;
        private double selfRotAngle = 0;
        private double coneAngle = 90;
        private bool movable = false;

        private Brep currModel;
        private List<Point3d> surfacePts;
        private List<Point3d> selectedPts;
        private Guid currModelObjId;
        private RhinoDoc myDoc;
        ObjectAttributes solidAttribute, lightGuideAttribute, redAttribute, yellowAttribute, soluableAttribute;


        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GearCreation()
          : base("AddGear", "Gear",
            "This component adds a gear train with 2 spur gear",
            "GearCreation", "Test")
        {
            myDoc = RhinoDoc.ActiveDoc;
            surfacePts = new List<Point3d>();
            selectedPts = new List<Point3d>();

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
                selectGearExit();

                if (currModel != null)
                {
                    if (customized_part_location.Z == currModel.GetBoundingBox(true).Max.Z)
                        center_point.Z = customized_part_location.Z - Thickness - 3;
                    else
                        center_point.Z = customized_part_location.Z + Thickness + 3;
                    center_point.X = customized_part_location.X;
                    center_point.Y = customized_part_location.Y;

                    SpurGear gear = new SpurGear(center_point, gear_direction, gear_x_dir, teethNum, mod, pressure_angle, Thickness, selfRotAngle, movable);

                    //在gear旁边弄个box，看model里面的数据
                    Point3d max = new Point3d(center_point.X - mod * teethNum / 2 + 10, center_point.Y + 20, customized_part_location.Z);
                    Point3d min = new Point3d(center_point.X - mod * teethNum / 2 + 10 - 40, center_point.Y - 20, gear.Model.GetBoundingBox(true).Min.Z - 3);
                    BoundingBox motorExitBox = new BoundingBox(min, max);

                    //Boolean difference 齿轮和box， 然后再Boolean difference 模型和box
                    Brep motorExit = motorExitBox.ToBrep();
                    currModel = Brep.CreateBooleanDifference(currModel, motorExit, myDoc.ModelAbsoluteTolerance, true)[0];
                    motorExit = Brep.CreateBooleanDifference(motorExit, gear.Model, myDoc.ModelAbsoluteTolerance, true)[0];



                    //给gear弄个boundingbox，但是比原boundingbox大5mm
                    BoundingBox gearBbox = gear.Model.GetBoundingBox(true);
                    max = new Point3d(gearBbox.Max.X + 1, gearBbox.Max.Y + 1, gearBbox.Max.Z + 2);
                    min = new Point3d(gearBbox.Min.X - 1, gearBbox.Min.Y - 1, gearBbox.Min.Z - 3);
                    gearBbox = new BoundingBox(min, max);
                    currModel = Brep.CreateBooleanDifference(currModel, gearBbox.ToBrep(), myDoc.ModelAbsoluteTolerance, true)[0];

                    //把gearBbox和齿轮做boolean difference， 需要把gearBbox一分为二，各和齿轮做boolean difference，再join起来
                    Brep gearBboxBrep = gearBbox.ToBrep();
                    Plane plane = new Plane(gear.Model.GetBoundingBox(true).Center, new Point3d(0, 1, 0), new Point3d(0, 0, 1));
                    PlaneSurface cutter = PlaneSurface.CreateThroughBox(plane, gearBbox);
                    Brep[] breps = Brep.CreateBooleanSplit(gearBboxBrep, cutter.ToBrep(), myDoc.ModelAbsoluteTolerance);
                    breps[0] = Brep.CreateBooleanDifference(breps[0], gear.Model, myDoc.ModelAbsoluteTolerance, true)[0];
                    breps[1] = Brep.CreateBooleanDifference(breps[1], gear.Model, myDoc.ModelAbsoluteTolerance, true)[0];
                    gearBboxBrep = breps[0];
                    if (gearBboxBrep.Join(breps[1], myDoc.ModelAbsoluteTolerance, false))
                        myDoc.Objects.Add(gearBboxBrep, soluableAttribute);
                    else
                    {
                        myDoc.Objects.Add(breps[0], soluableAttribute);
                        myDoc.Objects.Add(breps[1], soluableAttribute);
                    }

                    //在gear中间加两个pipe， 连通到customized part location外面，大的pipe比小的pipe大0.7mm
                    Point3d startPoint = new Point3d(center_point.X, center_point.Y, customized_part_location.Z + 20);
                    Point3d endPoint = new Point3d(center_point.X, center_point.Y, center_point.Z - Thickness / 2 - 1.5);
                    List<Point3d> points = new List<Point3d>();
                    points.Add(startPoint);
                    points.Add(endPoint);
                    Curve actualPipeLine = Curve.CreateControlPointCurve(points, 1);
                    Brep actualPipe = Brep.CreatePipe(actualPipeLine, 5, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];

                    Point3d extensionPoint = new Point3d(center_point.X, center_point.Y, currModel.GetBoundingBox(true).Min.Z - 3);
                    points.Add(extensionPoint);
                    Curve soluablePipeLine = Curve.CreateControlPointCurve(points, 1);
                    Brep solutablePipe = Brep.CreateThickPipe(soluablePipeLine, 5, 5.7, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];

                    //把pipe和模型做boolean difference
                    currModel = Brep.CreateBooleanDifference(currModel, actualPipe, myDoc.ModelAbsoluteTolerance, true)[0];
                    currModel = Brep.CreateBooleanDifference(currModel, solutablePipe, myDoc.ModelAbsoluteTolerance, true)[0];

                    //加一个齿轮底下的垫片并和model连一起
                    startPoint = new Point3d(center_point.X, center_point.Y, gear.Model.GetBoundingBox(true).Min.Z - 1);
                    endPoint = new Point3d(center_point.X, center_point.Y, gearBbox.Min.Z - 0.3);
                    points = new List<Point3d>();
                    points.Add(startPoint);
                    points.Add(endPoint);
                    Curve gasketPipeLine = Curve.CreateControlPointCurve(points, 1);
                    Brep gasketPipe = Brep.CreateThickPipe(gasketPipeLine, 5.7, 8, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];
                    breps = new Brep[2];
                    breps[0] = currModel;
                    breps[1] = gasketPipe;
                    currModel = Brep.CreateBooleanUnion(breps, myDoc.ModelAbsoluteTolerance, false)[0];


                    //把小pipe和gear加一起
                    breps = new Brep[2];
                    breps[0] = gear.Model;
                    breps[1] = actualPipe;
                    breps = Brep.CreateBooleanUnion(breps, myDoc.ModelAbsoluteTolerance, false);
                    gear.Model = breps[0];

                    myDoc.Objects.Delete(currModelObjId, true);
                    myDoc.Objects.Add(gear.Model, solidAttribute);
                    myDoc.Objects.Add(motorExit, soluableAttribute);
                    myDoc.Objects.Add(solutablePipe, soluableAttribute);

                    currModelObjId = myDoc.Objects.Add(currModel, solidAttribute);

                }
            }
        }


        private void selectGearExit()
        {
            ObjRef objSel_ref1;



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
                    return;


                // Find the candidate positions to place dots
                BoundingBox meshBox = currModel.GetBoundingBox(true);
                double w = meshBox.Max.X - meshBox.Min.X;
                double l = meshBox.Max.Y - meshBox.Min.Y;
                double h = meshBox.Max.Z - meshBox.Min.Z;
                double offset = 5;

                //For each point in x-y plane
                for (int i = 31; i < w - 30; i++)
                {
                    for (int j = 31; j < l - 30; j++)
                    {
                        // Create a nurbs curve of a straight line in respect to z axis with same x,y coordinate
                        Point3d ptInSpaceOri = new Point3d(i + meshBox.Min.X, j + meshBox.Min.Y, meshBox.Min.Z - offset);
                        Point3d ptInSpaceEnd = new Point3d(i + meshBox.Min.X, j + meshBox.Min.Y, meshBox.Max.Z + offset);

                        Line ray_ln = new Line(ptInSpaceOri, ptInSpaceEnd);
                        Curve ray_crv = ray_ln.ToNurbsCurve();

                        Curve[] overlapCrvs;
                        Point3d[] overlapPts;



                        // Get the 3D points of the intersection between the line and the surface of the current model
                        Intersection.CurveBrep(ray_crv, currModel, myDoc.ModelAbsoluteTolerance, out overlapCrvs, out overlapPts);

                        // Store the candidate positions
                        if (overlapPts != null)
                        {
                            if (overlapPts.Count() != 0)
                            {
                                foreach (Point3d p in overlapPts)
                                {
                                    surfacePts.Add(p);
                                }
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

                #region ask the user to select a point


                var getSelectedPts = RhinoGet.GetPoint("Please select points for parameter area", false, out customized_part_location);



                #endregion

                //Kill all dots and duplicate brep, Show the original brep
                myDoc.Objects.Delete(dupObjID, true);
                myDoc.Objects.Show(currModelObjId, true);
                foreach (var ptsID in pts_normals)
                {
                    myDoc.Objects.Delete(ptsID, true);
                }



            }

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5517bcfe-919c-48db-85f8-a4b5205d40ae");
    }
}