using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using GearCreation.Geometry;
using Rhino.DocObjects;
using System.Drawing;
using Rhino.Geometry.Intersect;
using Rhino.Input;
using System.Linq;
using Rhino.Runtime;
using static Rhino.DocObjects.PhysicallyBasedMaterial;

namespace GearCreation.Test
{
    public class AddGearTrain : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        /// 
        private Point3d customized_part_center_point = new Point3d(0, 0, 0);
        private Vector3d customized_part_gear_direction = new Vector3d(0, 0, 1);
        private Vector3d customized_part_gear_x_dir = new Vector3d(0, 0, 0);
        private int customized_part_teethNum = 30;
        private double customized_part_mod = 1.5;
        private double customized_part_pressure_angle = 20;
        private double Thickness = 5;
        private double customized_part_selfRotAngle = 0;
        private double customized_part_coneAngle = 90;
        private bool customized_part_movable = false;

        Point3d customized_part_location = new Point3d(0, 0, 0);
        private Brep currModel;
        private List<Point3d> surfacePts;
        private List<Point3d> selectedPts;
        private Guid currModelObjId;
        private RhinoDoc myDoc;
        ObjectAttributes solidAttribute, lightGuideAttribute, redAttribute, yellowAttribute, soluableAttribute;

        public AddGearTrain()
          : base("AddGearTrain", "GearTrain",
              "This component adds a gear train",
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
                #region 弄多个齿轮的代码

                //if (isPressed)
                //{
                //    Point3d first_center_point = new Point3d(0, 0, 0);
                //    Vector3d first_gear_direction = new Vector3d(0, 0, 1);
                //    Vector3d first_gear_x_dir = new Vector3d(0, 0, 0);
                //    int first_teethNum = 20;
                //    double first_mod = 1.5;
                //    double first_pressure_angle = 20;
                //    double first_Thickness = 5;
                //    double first_selfRotAngle = 0;
                //    bool first_movable = true;
                //    RhinoDoc myDoc = RhinoDoc.ActiveDoc;

                //    SpurGear first_gear = new SpurGear(first_center_point, first_gear_direction, first_gear_x_dir, first_teethNum, first_mod, first_pressure_angle, first_Thickness, first_selfRotAngle, first_movable);

                //    //first_gear.TipRadius + first_gear.RootRadius + 0.167 * 2
                //    Point3d second_center_point = new Point3d(first_gear.TipRadius + first_gear.RootRadius + 0.167 * 2, 0, 0);
                //    Vector3d second_gear_direction = new Vector3d(0, 0, 1);
                //    Vector3d second_gear_x_dir = new Vector3d(0, 0, 0);
                //    int second_teethNum = 20;
                //    double second_mod = 1.5;
                //    double second_pressure_angle = 20;
                //    double second_Thickness = 5;
                //    double second_selfRotAngle = 9;
                //    bool second_movable = true;
                //    SpurGear second_gear = new SpurGear(second_center_point, second_gear_direction, second_gear_x_dir, second_teethNum, second_mod, second_pressure_angle, second_Thickness, second_selfRotAngle, second_movable);
                //    //second_gear.Generate();


                //    myDoc.Objects.Add(first_gear.Model);
                //    myDoc.Objects.Add(second_gear.Model);
                //}
                #endregion

                //Customized gear
                customized_part_location = SelectGearExit();

                if (currModel != null)
                {
                    if (customized_part_location.Z == currModel.GetBoundingBox(true).Max.Z)
                        customized_part_center_point.Z = customized_part_location.Z - Thickness - 3;
                    else
                        customized_part_center_point.Z = customized_part_location.Z + Thickness + 3;
                    customized_part_center_point.X = customized_part_location.X;
                    customized_part_center_point.Y = customized_part_location.Y;
                }
                SpurGear customized_gear = new SpurGear(customized_part_center_point, customized_part_gear_direction, customized_part_gear_x_dir, customized_part_teethNum, customized_part_mod, customized_part_pressure_angle, Thickness, customized_part_selfRotAngle, customized_part_movable);

                Point3d end_gear_center_point = new Point3d(customized_part_location.X - 100, customized_part_location.Y, customized_part_center_point.Z);
                SpurGear end_gear = new SpurGear(end_gear_center_point, customized_part_gear_direction, customized_part_gear_x_dir, 20, customized_part_mod, customized_part_pressure_angle, Thickness, customized_part_selfRotAngle, customized_part_movable);


                //找中间的齿轮的中心点
                Point3d driven_gear_center_point = new Point3d(0, 0, 0);
                Vector3d driven_gear_gear_direction = new Vector3d(0, 0, 1);
                Vector3d driven_gear_gear_x_dir = new Vector3d(0, 0, 0);
                double driven_gear_mod = 1.5;
                double driven_gear_pressure_angle = 20;
                bool driven_gear_movable = false;

                double base_radius = customized_gear.RootRadius + 0.167 * 2;
                double driven_gear_outDiameter = 100 - customized_gear.RootRadius - end_gear.RootRadius - 0.167; //TODO: 把0.167减少可以让两个齿轮更加相近
                double driven_gear_pitchDiameter = driven_gear_outDiameter - 2 * driven_gear_mod;
                int driven_gear_numTeeth = (int)(driven_gear_pitchDiameter / driven_gear_mod);
                double driven_gear_selfRotAngle = 360 / driven_gear_numTeeth;
                //double driven_gear_selfRotAngle = 0;
                RhinoApp.WriteLine("Driven Gear teeth number: " + driven_gear_numTeeth.ToString());

                driven_gear_center_point.X = customized_part_location.X - base_radius - driven_gear_outDiameter / 2;

                driven_gear_center_point.Y = customized_part_location.Y;
                driven_gear_center_point.Z = customized_part_location.Z - Thickness - 3;
                SpurGear driven_gear = new SpurGear(driven_gear_center_point, driven_gear_gear_direction, driven_gear_gear_x_dir, driven_gear_numTeeth, driven_gear_mod, driven_gear_pressure_angle, Thickness, driven_gear_selfRotAngle, driven_gear_movable);

                //从0度到90度旋转，直到没有intersection为止
                //for (double i = 0.0; i < 91.0; i += 0.2)
                //{
                //    driven_gear.RotateGear(0.2);
                //    if (!Intersection.BrepBrep(driven_gear.Model, customized_gear.Model, myDoc.ModelAbsoluteTolerance, out Curve[] IntersectCurves, out Point3d[] IntersectPoints))
                //    {
                //        break;
                //    }
                //}
                //double stepAngle = 360.0 / driven_gear_numTeeth;
                //double boundary = 90.0 / stepAngle;
                //driven_gear.RotateGear(stepAngle / 2);
                driven_gear.RotateGear(7);
                //driven_gear.RotateGear(stepAngle/2 - (90 - stepAngle* (int)Math.Floor(boundary)));

                RhinoApp.WriteLine("Self Rotation Angle " + driven_gear.SelfRotAngle);
                RhinoApp.WriteLine("Tip Radius: " + driven_gear.TipRadius.ToString());
                RhinoApp.WriteLine("Calculated tip radius: " + driven_gear_outDiameter / 2);
                RhinoApp.WriteLine("Total Length between start and end" + (customized_gear.CenterPoint.X - end_gear.CenterPoint.X));

                //在gear旁边弄个box，看ipad里面的数据
                Point3d max = new Point3d(driven_gear_center_point.X - driven_gear_mod * driven_gear_numTeeth / 2 + 10, driven_gear_center_point.Y + 30, customized_part_location.Z);
                Point3d min = new Point3d(driven_gear_center_point.X - driven_gear_mod * driven_gear_numTeeth / 2 + 10 - 60, driven_gear_center_point.Y - 30, driven_gear.Model.GetBoundingBox(true).Min.Z - 3);
                BoundingBox motorExitBox = new BoundingBox(min, max);

                //Boolean difference 齿轮和box， 然后再Boolean difference 模型和box
                Brep motorExit = motorExitBox.ToBrep();
                currModel = Brep.CreateBooleanDifference(currModel, motorExit, myDoc.ModelAbsoluteTolerance, true)[0];
                motorExit = Brep.CreateBooleanDifference(motorExit, driven_gear.Model, myDoc.ModelAbsoluteTolerance, true)[0];

                //给gear们弄个boundingbox，但是比原boundingbox大5mm
                BoundingBox driven_gearBbox = driven_gear.Model.GetBoundingBox(true);
                max = new Point3d(driven_gearBbox.Max.X + 1, driven_gearBbox.Max.Y + 1, driven_gearBbox.Max.Z + 2);
                min = new Point3d(driven_gearBbox.Min.X - 1, driven_gearBbox.Min.Y - 1, driven_gearBbox.Min.Z - 3);
                driven_gearBbox = new BoundingBox(min, max);
                currModel = Brep.CreateBooleanDifference(currModel, driven_gearBbox.ToBrep(), myDoc.ModelAbsoluteTolerance, true)[0];

                BoundingBox customized_gearBbox = customized_gear.Model.GetBoundingBox(true);
                max = new Point3d(customized_gearBbox.Max.X + 1, customized_gearBbox.Max.Y + 1, customized_gearBbox.Max.Z + 2);
                min = new Point3d(customized_gearBbox.Min.X - 1, customized_gearBbox.Min.Y - 1, customized_gearBbox.Min.Z - 3);
                customized_gearBbox = new BoundingBox(min, max);
                currModel = Brep.CreateBooleanDifference(currModel, customized_gearBbox.ToBrep(), myDoc.ModelAbsoluteTolerance, true)[0];

                //把gearBbox和齿轮做boolean difference， 需要把gearBbox一分为二，各和齿轮做boolean difference，再join起来
                Brep gearBboxBrep = driven_gearBbox.ToBrep();
                Plane plane = new Plane(driven_gear.Model.GetBoundingBox(true).Center, new Point3d(0, 1, 0), new Point3d(0, 0, 1));
                PlaneSurface cutter = PlaneSurface.CreateThroughBox(plane, driven_gearBbox);
                Brep[] breps = Brep.CreateBooleanSplit(gearBboxBrep, cutter.ToBrep(), myDoc.ModelAbsoluteTolerance);
                breps[0] = Brep.CreateBooleanDifference(breps[0], driven_gear.Model, myDoc.ModelAbsoluteTolerance, true)[0];
                breps[1] = Brep.CreateBooleanDifference(breps[1], driven_gear.Model, myDoc.ModelAbsoluteTolerance, true)[0];
                gearBboxBrep = breps[0];
                if (gearBboxBrep.Join(breps[1], myDoc.ModelAbsoluteTolerance, false))
                    myDoc.Objects.Add(gearBboxBrep, soluableAttribute);
                else
                {
                    myDoc.Objects.Add(breps[0], soluableAttribute);
                    myDoc.Objects.Add(breps[1], soluableAttribute);
                }

                gearBboxBrep = customized_gearBbox.ToBrep();
                plane = new Plane(customized_gear.Model.GetBoundingBox(true).Center, new Point3d(0, 1, 0), new Point3d(0, 0, 1));
                cutter = PlaneSurface.CreateThroughBox(plane, customized_gearBbox);
                breps = Brep.CreateBooleanSplit(gearBboxBrep, cutter.ToBrep(), myDoc.ModelAbsoluteTolerance);
                breps[0] = Brep.CreateBooleanDifference(breps[0], customized_gear.Model, myDoc.ModelAbsoluteTolerance, true)[0];
                breps[1] = Brep.CreateBooleanDifference(breps[1], customized_gear.Model, myDoc.ModelAbsoluteTolerance, true)[0];
                gearBboxBrep = breps[0];
                if (gearBboxBrep.Join(breps[1], myDoc.ModelAbsoluteTolerance, false))
                    myDoc.Objects.Add(gearBboxBrep, soluableAttribute);
                else
                {
                    myDoc.Objects.Add(breps[0], soluableAttribute);
                    myDoc.Objects.Add(breps[1], soluableAttribute);
                }
                //customized gear加柱子
                customized_gear.Model = CreateTestGear(customized_gear, customized_gearBbox, out Brep solutablePipe);
                myDoc.Objects.Add(customized_gear.Model, solidAttribute);
                myDoc.Objects.Add(solutablePipe, soluableAttribute);

                //driven gear加柱子
                driven_gear.Model = CreateTestGear(driven_gear, driven_gearBbox, out solutablePipe);
                myDoc.Objects.Add(driven_gear.Model, solidAttribute);
                myDoc.Objects.Add(solutablePipe, soluableAttribute);





                myDoc.Objects.Delete(currModelObjId, true);
                currModelObjId = myDoc.Objects.Add(currModel, solidAttribute);
                myDoc.Objects.Add(motorExit, soluableAttribute);
                //myDoc.Objects.Add(end_gear.Model);
            }
        }

        private Brep CreateTestGear(SpurGear gear, BoundingBox gearBbox, out Brep solutablePipe)
        {
            //在gear中间加两个pipe， 连通到customized part location外面，大的pipe比小的pipe大0.7mm
            Point3d startPoint = new Point3d(gear.CenterPoint.X, gear.CenterPoint.Y, customized_part_location.Z + 20);
            Point3d endPoint = new Point3d(gear.CenterPoint.X, gear.CenterPoint.Y, gear.CenterPoint.Z - Thickness / 2 - 1.5);
            List<Point3d> points = new List<Point3d>();
            points.Add(startPoint);
            points.Add(endPoint);
            Curve actualPipeLine = Curve.CreateControlPointCurve(points, 1);
            Brep actualPipe = Brep.CreatePipe(actualPipeLine, 5, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];

            Point3d extensionPoint = new Point3d(gear.CenterPoint.X, gear.CenterPoint.Y, currModel.GetBoundingBox(true).Min.Z - 3);
            points.Add(extensionPoint);
            Curve soluablePipeLine = Curve.CreateControlPointCurve(points, 1);
            solutablePipe = Brep.CreateThickPipe(soluablePipeLine, 5, 5.7, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];

            //把pipe和模型做boolean difference
            currModel = Brep.CreateBooleanDifference(currModel, actualPipe, myDoc.ModelAbsoluteTolerance, true)[0];
            currModel = Brep.CreateBooleanDifference(currModel, solutablePipe, myDoc.ModelAbsoluteTolerance, true)[0];

            //加一个齿轮底下的垫片并和model连一起
            startPoint = new Point3d(gear.CenterPoint.X, gear.CenterPoint.Y, gear.Model.GetBoundingBox(true).Min.Z - 1);
            endPoint = new Point3d(gear.CenterPoint.X, gear.CenterPoint.Y, gearBbox.Min.Z - 0.3);
            points = new List<Point3d>();
            points.Add(startPoint);
            points.Add(endPoint);
            Curve gasketPipeLine = Curve.CreateControlPointCurve(points, 1);
            Brep gasketPipe = Brep.CreateThickPipe(gasketPipeLine, 5.7, 8, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];
            Brep[] breps = new Brep[2];
            breps[0] = currModel;
            breps[1] = gasketPipe;
            currModel = Brep.CreateBooleanUnion(breps, myDoc.ModelAbsoluteTolerance, false)[0];


            //把小pipe和gear加一起
            breps = new Brep[2];
            breps[0] = gear.Model;
            breps[1] = actualPipe;
            breps = Brep.CreateBooleanUnion(breps, myDoc.ModelAbsoluteTolerance, false);
            gear.Model = breps[0];

            return gear.Model;
        }

        private Point3d SelectGearExit()
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
                    return Point3d.Unset;


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
                Point3d customized_part_location;

                var getSelectedPts = RhinoGet.GetPoint("Please select points for parameter area", false, out customized_part_location);



                #endregion

                //Kill all dots and duplicate brep, Show the original brep
                myDoc.Objects.Delete(dupObjID, true);
                myDoc.Objects.Show(currModelObjId, true);
                foreach (var ptsID in pts_normals)
                {
                    myDoc.Objects.Delete(ptsID, true);
                }

                return customized_part_location;

            }
            return Point3d.Unset;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon
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
            get { return new Guid("61C638AB-5659-4FF2-8CCA-F441B646CCAB"); }
        }
    }
}