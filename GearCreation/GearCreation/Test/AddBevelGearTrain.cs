using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.DocObjects;
using Rhino;
using Rhino.Geometry;
using System.Drawing;
using Rhino.Geometry.Intersect;
using Rhino.Input;
using System.Linq;
using GearCreation.Geometry;

namespace GearCreation.Test
{
    public class AddBevelGearTrain : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AddBevelGearTrain class.
        /// </summary>
        /// 

        private Point3d customized_part_center_point = new Point3d(0, 0, 0);
        private Vector3d customized_part_gear_direction = new Vector3d(-1, 0, 0);
        private Vector3d customized_part_gear_x_dir = new Vector3d(0, 0, 0);
        private int customized_part_teethNum = 30;
        private double customized_part_mod = 1.5;
        private double customized_part_pressure_angle = 20;
        private double Thickness = 5;
        private double customized_part_selfRotAngle = 0;
        private double customized_part_coneAngle = 90;
        private bool movable = false;

        Point3d customized_part_location = new Point3d(0, 0, 0);
        private Brep currModel;
        private List<Point3d> surfacePts;
        private List<Point3d> selectedPts;
        private Guid currModelObjId;
        private RhinoDoc myDoc;
        ObjectAttributes solidAttribute, lightGuideAttribute, redAttribute, yellowAttribute, soluableAttribute;

        public AddBevelGearTrain()
          : base("AddBevelGearTrain", "GearTrain1*1",
              "This component adds a bevel gear testing set",
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
                customized_part_location = SelectGearExit();

                #region Customized gear
                if (currModel != null)
                {
                    customized_part_center_point.Z = customized_part_location.Z;
                    customized_part_center_point.Y = customized_part_location.Y;

                    if (customized_part_location.X == currModel.GetBoundingBox(true).Max.X)
                        customized_part_center_point.X = customized_part_location.X - Thickness - 3;
                    else
                        customized_part_center_point.X = customized_part_location.X + Thickness + 3;
                }

                BevelGear customized_gear = new BevelGear(customized_part_center_point, customized_part_gear_direction, customized_part_gear_x_dir, customized_part_teethNum, customized_part_mod, customized_part_pressure_angle, Thickness, customized_part_selfRotAngle, customized_part_coneAngle, movable);

                #endregion

                #region Driven gear
                Vector3d driven_gear_gear_direction = new Vector3d(0, 0, 1);
                Vector3d driven_gear_gear_x_dir = new Vector3d(0, 0, 0);
                double driven_gear_mod = 1.5;
                double driven_gear_pressure_angle = 20;
                bool driven_gear_movable = false;
                int driven_gear_numTeeth = 30;
                double driven_gear_coneAngle = 90;
                double driven_gear_selfRotAngle = 360 / driven_gear_numTeeth / 2;

                Point3d driven_gear_center_point = new Point3d(0, 0, 0);

                double driven_gear_pitchDiameter = driven_gear_mod * driven_gear_numTeeth;
                double driven_gear_outDiameter = driven_gear_pitchDiameter + 2 * driven_gear_mod;

                driven_gear_center_point.X = customized_gear.Model.GetBoundingBox(true).Max.X - driven_gear_outDiameter / 2;
                driven_gear_center_point.Y = customized_part_location.Y;//TODO： 最好用customized gear的center.Y，需要在Bevel Gear的transform那一部分把center给transorm
                driven_gear_center_point.Z = customized_gear.Model.GetBoundingBox(true).Min.Z;

                BevelGear driven_gear = new BevelGear(driven_gear_center_point, driven_gear_gear_direction, driven_gear_gear_x_dir, driven_gear_numTeeth, driven_gear_mod, driven_gear_pressure_angle, Thickness, driven_gear_selfRotAngle, driven_gear_coneAngle, movable);

                #endregion
                #endregion

                #region 马达入口
                Point3d max = new Point3d(driven_gear_center_point.X - driven_gear_mod * driven_gear_numTeeth / 2 + 5, driven_gear_center_point.Y + 30, customized_part_location.Z);
                Point3d min = new Point3d(currModel.GetBoundingBox(true).Min.X - 5, driven_gear_center_point.Y - 30, driven_gear.Model.GetBoundingBox(true).Min.Z - 3);
                BoundingBox motorExitBox = new BoundingBox(min, max);

                //Boolean difference 齿轮和box， 然后再Boolean difference 模型和box
                Brep motorExit = motorExitBox.ToBrep();
                currModel = Brep.CreateBooleanDifference(currModel, motorExit, myDoc.ModelAbsoluteTolerance, false)[0];
                motorExit = Brep.CreateBooleanDifference(motorExit, driven_gear.Model, myDoc.ModelAbsoluteTolerance, false)[0];
                #endregion

                #region 齿轮Bounding box
                #region 创造Bounding box
                BoundingBox driven_gearBbox = driven_gear.Model.GetBoundingBox(true);
                max = new Point3d(driven_gearBbox.Max.X + 1, driven_gearBbox.Max.Y + 1, driven_gearBbox.Max.Z + 2);
                min = new Point3d(driven_gearBbox.Min.X - 1, driven_gearBbox.Min.Y - 1, driven_gearBbox.Min.Z - 3);
                driven_gearBbox = new BoundingBox(min, max);
                currModel = Brep.CreateBooleanDifference(currModel, driven_gearBbox.ToBrep(), myDoc.ModelAbsoluteTolerance, false)[0];

                BoundingBox customized_gearBbox = customized_gear.Model.GetBoundingBox(true);
                max = new Point3d(customized_gearBbox.Max.X + 3, customized_gearBbox.Max.Y + 1, customized_gearBbox.Max.Z + 1);
                min = new Point3d(customized_gearBbox.Min.X - 2, customized_gearBbox.Min.Y - 1, customized_gearBbox.Min.Z - 1);
                customized_gearBbox = new BoundingBox(min, max);
                currModel = Brep.CreateBooleanDifference(currModel, customized_gearBbox.ToBrep(), myDoc.ModelAbsoluteTolerance, false)[0];
                #endregion

                #region 把gearBbox和齿轮做boolean difference， 需要把gearBbox一分为二，各和齿轮做boolean difference，再join起来
                Brep gearBboxBrep = driven_gearBbox.ToBrep();
                Plane plane = new Plane(driven_gear.Model.GetBoundingBox(true).Center, new Point3d(0, (driven_gear.Model.GetBoundingBox(true).Max.Y - driven_gear.Model.GetBoundingBox(true).Min.Y) / 2, 0), new Point3d(0, 0, (driven_gear.Model.GetBoundingBox(true).Max.Z - driven_gear.Model.GetBoundingBox(true).Min.Z) / 2));
                PlaneSurface cutter = PlaneSurface.CreateThroughBox(plane, driven_gearBbox);
                Brep[] breps = Brep.CreateBooleanSplit(gearBboxBrep, cutter.ToBrep(), myDoc.ModelAbsoluteTolerance);
                breps[0] = Brep.CreateBooleanDifference(breps[0], driven_gear.Model, myDoc.ModelAbsoluteTolerance, false)[0];
                breps[1] = Brep.CreateBooleanDifference(breps[1], driven_gear.Model, myDoc.ModelAbsoluteTolerance, false)[0];
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

                //driven gear加柱子
                driven_gear.Model = CreateTestGear(driven_gear, driven_gearBbox, out solutablePipe);
                myDoc.Objects.Add(driven_gear.Model, solidAttribute);
                myDoc.Objects.Add(solutablePipe, soluableAttribute);

                #endregion

                myDoc.Objects.Delete(currModelObjId, true);
                currModelObjId = myDoc.Objects.Add(currModel, solidAttribute);
                myDoc.Objects.Add(motorExit, soluableAttribute);
            }
        }

        private Brep CreateTestGear(BevelGear gear, BoundingBox gearBbox, out Brep solutablePipe)
        {
            if (gear.Direction == new Vector3d(0, 0, 1))
            {
                //在gear中间加两个pipe， 连通到customized part location外面，大的pipe比小的pipe大0.7mm
                Point3d startPoint = new Point3d(gear.CenterPoint.X, gear.CenterPoint.Y, customized_part_location.Z + 20);
                Point3d endPoint = new Point3d(gear.CenterPoint.X, gear.CenterPoint.Y, gear.CenterPoint.Z - Thickness / 2 - 3);
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

                //加一个齿轮上面的垫片并和model连一起
                startPoint = new Point3d(gear.CenterPoint.X, gear.CenterPoint.Y, gear.Model.GetBoundingBox(true).Max.Z + 1);
                endPoint = new Point3d(gear.CenterPoint.X, gear.CenterPoint.Y, gearBbox.Max.Z + 0.3);
                points = new List<Point3d>();
                points.Add(startPoint);
                points.Add(endPoint);
                gasketPipeLine = Curve.CreateControlPointCurve(points, 1);
                gasketPipe = Brep.CreateThickPipe(gasketPipeLine, 5.7, 8, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];
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
                return gear.Model;
            }
            else if (gear.Direction == new Vector3d(-1, 0, 0))
            {
                //在gear中间加两个pipe， 连通到customized part location外面，大的pipe比小的pipe大0.7mm
                Point3d startPoint = new Point3d(customized_part_location.X + 20, gear.CenterPoint.Y, gear.CenterPoint.Z);
                Point3d endPoint = new Point3d(gear.CenterPoint.X - Thickness / 2 - 6, gear.CenterPoint.Y, gear.CenterPoint.Z);
                List<Point3d> points = new List<Point3d>();
                points.Add(startPoint);
                points.Add(endPoint);
                Curve actualPipeLine = Curve.CreateControlPointCurve(points, 1);
                Brep actualPipe = Brep.CreatePipe(actualPipeLine, 5, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];

                Point3d extensionPoint = new Point3d(currModel.GetBoundingBox(true).Min.X - 3, gear.CenterPoint.Y, gear.CenterPoint.Z);
                points.Add(extensionPoint);
                Curve soluablePipeLine = Curve.CreateControlPointCurve(points, 1);
                solutablePipe = Brep.CreateThickPipe(soluablePipeLine, 5, 5.7, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];

                //把pipe和模型做boolean difference
                currModel = Brep.CreateBooleanDifference(currModel, actualPipe, myDoc.ModelAbsoluteTolerance, false)[0];
                currModel = Brep.CreateBooleanDifference(currModel, solutablePipe, myDoc.ModelAbsoluteTolerance, false)[0];

                //加一个齿轮底下的垫片并和model连一起
                startPoint = new Point3d(gear.Model.GetBoundingBox(true).Max.X + 1, gear.CenterPoint.Y, gear.CenterPoint.Z);
                endPoint = new Point3d(gearBbox.Max.X + 0.3, gear.CenterPoint.Y, gear.CenterPoint.Z);
                points = new List<Point3d>();
                points.Add(startPoint);
                points.Add(endPoint);
                Curve gasketPipeLine = Curve.CreateControlPointCurve(points, 1);
                Brep gasketPipe = Brep.CreateThickPipe(gasketPipeLine, 5.7, 8, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];
                Brep[] breps = new Brep[2];
                breps[0] = currModel;
                breps[1] = gasketPipe;
                currModel = Brep.CreateBooleanUnion(breps, myDoc.ModelAbsoluteTolerance, false)[0];

                //加一个齿轮上面的垫片和model连一起
                startPoint = new Point3d(gear.Model.GetBoundingBox(true).Min.X - 1, gear.CenterPoint.Y, gear.CenterPoint.Z);
                endPoint = new Point3d(gearBbox.Min.X - 0.3, gear.CenterPoint.Y, gear.CenterPoint.Z);
                points = new List<Point3d>();
                points.Add(startPoint);
                points.Add(endPoint);
                gasketPipeLine = Curve.CreateControlPointCurve(points, 1);
                gasketPipe = Brep.CreateThickPipe(gasketPipeLine, 5.7, 8, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];
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
                return gear.Model;
            }



            solutablePipe = new Brep();
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

                for (int i = 0; i < h; i += 5)
                {
                    Point3d Origin = new Point3d(w / 2, l / 2, i);
                    Point3d xPoint = new Point3d(boundingBox.Max.X + offset, l / 2, i);
                    Point3d yPoint = new Point3d(w / 2, boundingBox.Max.Y + offset, i);

                    Plane plane = new Plane(Origin, xPoint, yPoint);
                    PlaneSurface planeSurface = PlaneSurface.CreateThroughBox(plane, boundingBox);

                    Intersection.BrepSurface(currModel, planeSurface, myDoc.ModelAbsoluteTolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints);

                    //Create Points on the Curve
                    if (intersectionCurves != null)
                    {
                        if (intersectionCurves.Length != 0)
                        {
                            foreach (Curve curve in intersectionCurves)
                            {
                                double[] curveParams = curve.DivideByLength(2, true, out Point3d[] points);
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
            get { return new Guid("73FA0134-A286-4049-8235-3D0115D83711"); }
        }
    }
}