using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.DocObjects;
using Rhino;
using Rhino.Geometry;
using System.Drawing;
using GearCreation.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Input;
using System.Linq;

namespace GearCreation.Test
{
    public class AddGearTrain2 : GH_Component
    {
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


        /// <summary>
        /// Initializes a new instance of the AddGearTrain2 class.
        /// </summary>
        public AddGearTrain2()
          : base("AddGearTrain2", "GearTrain2",
              "Description",
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
                #region 创造gear

                #region Customized gear
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

                #endregion

                Vector3d driven_gear_gear_direction = new Vector3d(0, 0, 1);
                Vector3d driven_gear_gear_x_dir = new Vector3d(0, 0, 0);
                double driven_gear_mod = 1.5;
                double driven_gear_pressure_angle = 20;
                bool driven_gear_movable = false;

                //double base_radius = customized_gear.RootRadius + 0.167 * 2; //base radius of the customized gear
                double base_radius = customized_gear.RootRadius; //base radius of the customized gear
                double driven_gear_outDiameter = 100 - customized_gear.RootRadius - end_gear.RootRadius; //TODO: 把0.167减少可以让两个齿轮更加相近



                #region First driven gear
                double first_driven_gear_outDiameter = driven_gear_outDiameter / 2;
                double first_driven_gear_pitchDiameter = first_driven_gear_outDiameter - 2 * driven_gear_mod;
                int first_driven_gear_numTeeth = (int)Math.Ceiling((double)(first_driven_gear_pitchDiameter / driven_gear_mod));
                Point3d first_driven_gear_center_point = new Point3d(0, 0, 0);
                first_driven_gear_center_point.X = customized_part_location.X - base_radius - first_driven_gear_outDiameter / 2 - 1.3;
                first_driven_gear_center_point.Y = customized_part_location.Y;
                first_driven_gear_center_point.Z = customized_part_location.Z - Thickness - 3;

                double first_driven_gear_selfRotAngle = -90;

                SpurGear first_driven_gear = new SpurGear(first_driven_gear_center_point, driven_gear_gear_direction, driven_gear_gear_x_dir, first_driven_gear_numTeeth, driven_gear_mod, driven_gear_pressure_angle, Thickness, first_driven_gear_selfRotAngle, driven_gear_movable);
                #endregion

                #region Second driven gear
                double second_driven_gear_outDiameter = driven_gear_outDiameter / 2;
                double second_driven_gear_pitchDiameter = second_driven_gear_outDiameter - 2 * driven_gear_mod;
                int second_driven_gear_numTeeth = (int)Math.Floor((double)second_driven_gear_pitchDiameter / driven_gear_mod);
                Point3d second_driven_gear_center_point = new Point3d(0, 0, 0);
                second_driven_gear_center_point.X = customized_part_location.X - base_radius - first_driven_gear.RootRadius * 2 - first_driven_gear.ToothDepth - second_driven_gear_outDiameter / 2 - 1.3;
                second_driven_gear_center_point.Y = customized_part_location.Y;
                second_driven_gear_center_point.Z = customized_part_location.Z - Thickness - 3;

                double second_driven_gear_selfRotAngle = -90;

                SpurGear second_driven_gear = new SpurGear(second_driven_gear_center_point, driven_gear_gear_direction, driven_gear_gear_x_dir, second_driven_gear_numTeeth, driven_gear_mod, driven_gear_pressure_angle, Thickness, second_driven_gear_selfRotAngle, driven_gear_movable);
                #endregion

                #endregion

                #region 马达入口
                Point3d max = new Point3d(second_driven_gear_center_point.X - driven_gear_mod * second_driven_gear_numTeeth / 2 + 5, second_driven_gear_center_point.Y + 30, customized_part_location.Z);
                Point3d min = new Point3d(second_driven_gear_center_point.X - driven_gear_mod * second_driven_gear_numTeeth / 2 + 5 - 60, second_driven_gear_center_point.Y - 30, second_driven_gear.Model.GetBoundingBox(true).Min.Z - 3);
                BoundingBox motorExitBox = new BoundingBox(min, max);

                //Boolean difference 齿轮和box， 然后再Boolean difference 模型和box
                Brep motorExit = motorExitBox.ToBrep();
                currModel = Brep.CreateBooleanDifference(currModel, motorExit, myDoc.ModelAbsoluteTolerance, false)[0];
                motorExit = Brep.CreateBooleanDifference(motorExit, second_driven_gear.Model, myDoc.ModelAbsoluteTolerance, false)[0];
                #endregion

                #region 齿轮Bounding box
                #region 创造Bounding box
                BoundingBox second_driven_gearBbox = second_driven_gear.Model.GetBoundingBox(true);
                max = new Point3d(second_driven_gearBbox.Max.X + 1, second_driven_gearBbox.Max.Y + 1, second_driven_gearBbox.Max.Z + 2);
                min = new Point3d(second_driven_gearBbox.Min.X - 1, second_driven_gearBbox.Min.Y - 1, second_driven_gearBbox.Min.Z - 3);
                second_driven_gearBbox = new BoundingBox(min, max);
                currModel = Brep.CreateBooleanDifference(currModel, second_driven_gearBbox.ToBrep(), myDoc.ModelAbsoluteTolerance, false)[0];


                BoundingBox first_driven_gearBbox = first_driven_gear.Model.GetBoundingBox(true);
                max = new Point3d(first_driven_gearBbox.Max.X + 1, first_driven_gearBbox.Max.Y + 1, first_driven_gearBbox.Max.Z + 2);
                min = new Point3d(first_driven_gearBbox.Min.X - 1, first_driven_gearBbox.Min.Y - 1, first_driven_gearBbox.Min.Z - 3);
                first_driven_gearBbox = new BoundingBox(min, max);
                currModel = Brep.CreateBooleanDifference(currModel, first_driven_gearBbox.ToBrep(), myDoc.ModelAbsoluteTolerance, false)[0];

                BoundingBox customized_gearBbox = customized_gear.Model.GetBoundingBox(true);
                max = new Point3d(customized_gearBbox.Max.X + 1, customized_gearBbox.Max.Y + 1, customized_gearBbox.Max.Z + 2);
                min = new Point3d(customized_gearBbox.Min.X - 1, customized_gearBbox.Min.Y - 1, customized_gearBbox.Min.Z - 3);
                customized_gearBbox = new BoundingBox(min, max);
                currModel = Brep.CreateBooleanDifference(currModel, customized_gearBbox.ToBrep(), myDoc.ModelAbsoluteTolerance, false)[0];
                #endregion

                #region 把gearBbox和齿轮做boolean difference， 需要把gearBbox一分为二，各和齿轮做boolean difference，再join起来
                Brep gearBboxBrep = second_driven_gearBbox.ToBrep();
                Plane plane = new Plane(second_driven_gear.Model.GetBoundingBox(true).Center, new Point3d(0, 1, 0), new Point3d(0, 0, 1));
                PlaneSurface cutter = PlaneSurface.CreateThroughBox(plane, second_driven_gearBbox);
                Brep[] breps = Brep.CreateBooleanSplit(gearBboxBrep, cutter.ToBrep(), myDoc.ModelAbsoluteTolerance);
                breps[0] = Brep.CreateBooleanDifference(breps[0], second_driven_gear.Model, myDoc.ModelAbsoluteTolerance, false)[0];
                breps[1] = Brep.CreateBooleanDifference(breps[1], second_driven_gear.Model, myDoc.ModelAbsoluteTolerance, false)[0];
                gearBboxBrep = breps[0];
                if (gearBboxBrep.Join(breps[1], myDoc.ModelAbsoluteTolerance, false))
                    myDoc.Objects.Add(gearBboxBrep, soluableAttribute);
                else
                {
                    myDoc.Objects.Add(breps[0], soluableAttribute);
                    myDoc.Objects.Add(breps[1], soluableAttribute);
                }

                gearBboxBrep = first_driven_gearBbox.ToBrep();
                plane = new Plane(first_driven_gear.Model.GetBoundingBox(true).Center, new Point3d(0, 1, 0), new Point3d(0, 0, 1));
                cutter = PlaneSurface.CreateThroughBox(plane, first_driven_gearBbox);
                breps = Brep.CreateBooleanSplit(gearBboxBrep, cutter.ToBrep(), myDoc.ModelAbsoluteTolerance);
                breps[0] = Brep.CreateBooleanDifference(breps[0], first_driven_gear.Model, myDoc.ModelAbsoluteTolerance, false)[0];
                breps[1] = Brep.CreateBooleanDifference(breps[1], first_driven_gear.Model, myDoc.ModelAbsoluteTolerance, false)[0];
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
                breps[0] = Brep.CreateBooleanDifference(breps[0], customized_gear.Model, myDoc.ModelAbsoluteTolerance, false)[0];
                breps[1] = Brep.CreateBooleanDifference(breps[1], customized_gear.Model, myDoc.ModelAbsoluteTolerance, false)[0];
                gearBboxBrep = breps[0];
                if (gearBboxBrep.Join(breps[1], myDoc.ModelAbsoluteTolerance, false))
                    myDoc.Objects.Add(gearBboxBrep, soluableAttribute);
                else
                {
                    myDoc.Objects.Add(breps[0], soluableAttribute);
                    myDoc.Objects.Add(breps[1], soluableAttribute);
                }
                #endregion
                #endregion

                #region 加柱子
                //customized gear加柱子
                customized_gear.Model = CreateTestGear(customized_gear, customized_gearBbox, out Brep solutablePipe);
                myDoc.Objects.Add(customized_gear.Model, solidAttribute);
                myDoc.Objects.Add(solutablePipe, soluableAttribute);

                //second driven gear加柱子
                second_driven_gear.Model = CreateTestGear(second_driven_gear, second_driven_gearBbox, out solutablePipe);
                myDoc.Objects.Add(second_driven_gear.Model, solidAttribute);
                myDoc.Objects.Add(solutablePipe, soluableAttribute);

                //first driven gear加柱子
                first_driven_gear.Model = CreateTestGear(first_driven_gear, first_driven_gearBbox, out solutablePipe);
                myDoc.Objects.Add(first_driven_gear.Model, solidAttribute);
                myDoc.Objects.Add(solutablePipe, soluableAttribute);
                #endregion

                myDoc.Objects.Delete(currModelObjId, true);
                currModelObjId = myDoc.Objects.Add(currModel, solidAttribute);
                myDoc.Objects.Add(motorExit, soluableAttribute);
            }
        }

        //TODO: 找出让齿轮错开的角度
        private double GetSelfRotAngle(SpurGear lastGear, int teethNum)
        {

            if (lastGear.NumTeeth < teethNum)
            {
                if (lastGear.NumTeeth % 20 == 0 && teethNum % 2 != 0)//完整
                {
                    return -90;
                }
                else if (lastGear.NumTeeth % 20 == 0 && teethNum % 2 == 0) //完整
                {
                    double stepAngle = 360.0 / teethNum;
                    double boundary = 90.0 / stepAngle;
                    int floorNum = (int)Math.Floor(boundary);
                    double leftoverAngle = 90 - stepAngle * floorNum;
                    return stepAngle / 2 - leftoverAngle;
                }
                else if (lastGear.NumTeeth % 2 != 0 && lastGear.NumTeeth % 20 != 0 && (lastGear.NumTeeth - lastGear.NumTeeth % 10 * 10) % 4 == 0) //完整
                {
                    double stepAngle = 360.0 / teethNum;
                    double boundary = 90.0 / stepAngle;
                    int floorNum = (int)Math.Floor(boundary);
                    double leftoverAngle = 90 - stepAngle * floorNum;
                    return stepAngle / 2 - leftoverAngle;
                }
                else if (lastGear.NumTeeth % 2 != 0 && lastGear.NumTeeth % 20 != 0 && (lastGear.NumTeeth - lastGear.NumTeeth % 10 * 10) % 4 != 0)//完整
                {
                    return -90;
                }
                else if (lastGear.NumTeeth % 20 == 0 && teethNum % 2 == 0)
                {
                    return -45;
                }
            }



            return -90 - lastGear.SelfRotAngle;
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
            currModel = Brep.CreateBooleanDifference(currModel, actualPipe, myDoc.ModelAbsoluteTolerance, false)[0];
            currModel = Brep.CreateBooleanDifference(currModel, solutablePipe, myDoc.ModelAbsoluteTolerance, false)[0];

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
            get { return new Guid("33BD59CC-8EA7-4BE2-B020-EE329A050D09"); }
        }
    }
}