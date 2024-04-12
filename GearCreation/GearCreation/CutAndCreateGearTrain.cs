using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.DocObjects;
using Rhino;
using Rhino.Geometry;
using GearCreation.Geometry;
using System.Drawing;
using Rhino.Input;
using Rhino.Geometry.Intersect;
using static Rhino.DocObjects.PhysicallyBasedMaterial;

namespace GearCreation
{
    public class CutAndCreateGearTrain : GH_Component
    {
        private Essentials original_essentials;
        private Brep currModel;
        private Guid currModelObjId;
        private Brep cutter;
        private RhinoDoc myDoc;
        private double module = 1.5;
        private double pressure_angle = 20;
        private double thickness = 5;
        ObjectAttributes solidAttribute, lightGuideAttribute, redAttribute, yellowAttribute, soluableAttribute;


        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public CutAndCreateGearTrain()
          : base("Cut&Create", "Cut&Create",
              "This component cut the main model with the cutter, then create a gear train that connects to the bottom of the main model",
              "GearCreation", "Main")
        {
            myDoc = RhinoDoc.ActiveDoc;
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
            pManager.AddGenericParameter("Button", "B", "Button to create gear", GH_ParamAccess.item);
            pManager.AddGenericParameter("Essentials", "E", "The essential that contains: main model, cutter, cutter plane, cutter thickness, cutted models", GH_ParamAccess.item);
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
                return;
            if (!DA.GetData(1, ref original_essentials))
                return;

            if(isPressed)
            {
                Essentials essentials = new Essentials();

                #region Load the main model and cutter
                if (original_essentials.Cutter == new Guid())
                    return;

                essentials = original_essentials.Duplicate(original_essentials);

                //Copy cutter
                ObjRef cutterObj = new ObjRef(essentials.Cutter);
                cutter = cutterObj.Brep();

                //Copy main model
                currModelObjId = essentials.MainModel;
                ObjRef currObj = new ObjRef(currModelObjId);
                currModel = currObj.Brep();
                #endregion

                #region Cut the main model
                Brep[] cuttedBrep = Brep.CreateBooleanDifference(currModel, cutter, myDoc.ModelAbsoluteTolerance, false);
                myDoc.Objects.Delete(essentials.Cutter, true);
                myDoc.Objects.Delete(essentials.MainModel, true);
                List<Guid> cuttedBrepObjId = new List<Guid>();
                foreach (var brep in cuttedBrep)
                {
                    Guid guid = myDoc.Objects.Add(brep);
                    cuttedBrepObjId.Add(guid);
                }
                myDoc.Views.Redraw();
                #endregion

                #region Ask which brep is the end effector and create gear train
                ObjRef endEffector_ref;
                var rc = RhinoGet.GetOneObject("Select the end effector: ", false, ObjectType.Brep, out endEffector_ref);
                if (rc == Rhino.Commands.Result.Success)
                {
                    #region Get end effector and main model
                    Guid mainModelObjId;
                    Brep mainModel;
                    Guid endEffectorObjId;
                    Brep endEffector;

                    endEffectorObjId = endEffector_ref.ObjectId;
                    endEffector = endEffector_ref.Brep();

                    if(endEffectorObjId == cuttedBrepObjId[0])
                    {
                        mainModelObjId = cuttedBrepObjId[1];
                        mainModel = cuttedBrep[1];
                    }
                    else
                    {
                        mainModelObjId = cuttedBrepObjId[0];
                        mainModel = cuttedBrep[0];
                    }
                    #endregion

                    #region Create Gear train

                    #region Calculate the point of the end effector that extend the pipe to the main model
                    Point3d pointConnection1 = new Point3d(0,0,0);
                    if (Intersection.BrepBrep(endEffector, cutter, myDoc.ModelAbsoluteTolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints))
                    {
                        if(intersectionCurves.Length == 0)
                        {
                            RhinoApp.WriteLine("No intersection found");
                            return;
                        }
                        Brep[] intersectionSurface = Brep.CreatePlanarBreps(intersectionCurves, myDoc.ModelAbsoluteTolerance);
                        //myDoc.Objects.Add(intersectionSurface[0]);
                        pointConnection1 = intersectionSurface[0].GetBoundingBox(true).Center;
                        //myDoc.Objects.AddPoint(pointConnection1);
                    }
                    else
                    {
                        RhinoApp.WriteLine("The point of the end effector that extend the pipe to the main model cannot be found. Restart and Try again.");
                        return;
                    }

                    #endregion

                    #region Calculate the direction of the end gear that needs to be facing.
                    Vector3d end_gear_dir = essentials.CutterPlane.Normal;
                    double distance = pointConnection1.DistanceTo(mainModel.GetBoundingBox(true).Center); 
                    Line endEffector_rail = new Line(pointConnection1, end_gear_dir, distance);
                    if (!cuttedBrep[0].IsPointInside(endEffector_rail.To, myDoc.ModelAbsoluteTolerance, true)) // TODO: We need to make sure if the main model is closed and manifold. Perhaps write a function to fix the original model in the first place.
                    {
                        end_gear_dir.Reverse();
                        endEffector_rail = new Line(pointConnection1, end_gear_dir, distance);
                    }
                    myDoc.Objects.Add(endEffector_rail.ToNurbsCurve());
                    #endregion

                    #region Calculate the cone angle of the end bevel gear and its driven gear
                    double end_gear_coneAngle = ConeAngle(new Vector3d(0, 0, 1), end_gear_dir);
                    #endregion

                    #region if cone angle is greater than 90, use a special set of bevel gear ---------> To be implemented

                    #endregion

                    #region if cone angle is smaller than 90, go ahead and make the gear train
                    #region end gear
                    //Create the gear
                    Point3d end_gear_centerPoint = endEffector_rail.To;
                    Vector3d end_gear_Direction = end_gear_dir;
                    
                    Vector3d end_gear_xDir = new Vector3d(0, 0, 0);
                    int end_gear_teethNum = 20;
                    double end_gear_selfRotAngle = 0;
                    BevelGear end_gear = new BevelGear(end_gear_centerPoint, end_gear_Direction, end_gear_xDir, end_gear_teethNum, module, pressure_angle, thickness, end_gear_selfRotAngle, 90, false);
                    myDoc.Objects.Add(end_gear.Model);

                    #region Create the pipe that connects the end gear and end effector --> Half finish, need to implement gaskets
                    Brep actualPipe = new Brep();
                    Brep solutablePipe = new Brep();
                    if (endEffector_rail.Extend(0, 5))
                    {
                        actualPipe = Brep.CreatePipe(endEffector_rail.ToNurbsCurve(), 3, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];
                    }

                    if (endEffector_rail.Extend(0, 4))
                    {
                        solutablePipe = Brep.CreateThickPipe(endEffector_rail.ToNurbsCurve(), 3, 3.7, false, PipeCapMode.Flat, true, myDoc.ModelAbsoluteTolerance, myDoc.ModelAngleToleranceRadians)[0];
                    }

                    //mainModel = Brep.CreateBooleanDifference(mainModel, actualPipe, myDoc.ModelAbsoluteTolerance, false)[0];
                    //mainModel = Brep.CreateBooleanDifference(mainModel, solutablePipe, myDoc.ModelAbsoluteTolerance, false)[0];

                    Brep[] breps = new Brep[2];
                    breps[0] = end_gear.Model;
                    breps[1] = actualPipe;

                    breps = Brep.CreateBooleanUnion(breps, myDoc.ModelAbsoluteTolerance, false);
                    myDoc.Objects.Add(breps[0]);
                    #endregion

                    #endregion

                    /*
                    #region driven gear of end gear
                    //Calculate the vector that is perpendicular to the end gear facing direction
                    Vector3d orthogonal = new Vector3d();
                    orthogonal.PerpendicularTo(end_gear_Direction);
                    while (orthogonal.Z == end_gear_Direction.Z || orthogonal.Z >= end_gear_Direction.Z)
                        orthogonal.Rotate(Math.PI/2, end_gear_Direction);
                    
                    //Get the line that is along the xy axis that has the same direction of the end gear facing direction
                    Line rail1 = new Line(end_gear_centerPoint, orthogonal, end_gear.Model.GetBoundingBox(true).Min.Z); //A line that is along the xy axis that has the same direction of the end gear facing direction
                    Line rail2 = new Line(end_gear_centerPoint, end_gear_Direction, 100); // A line that has the direction of the end gear facing direction
                    Line rail3 = new Line(end_gear_centerPoint, new Vector3d(rail2.Direction.X, rail2.Direction.Y, 0));// A line that is paralle to the end gear and through the end gear's center point

                    //Move rail3 to minimum z of the end gear and extend it from the end
                    double length = end_gear_centerPoint.Z - end_gear.Model.GetBoundingBox(true).Min.Z; 
                    Transform transform = Transform.Translation(new Vector3d(0,0,-length));
                    rail3.Transform(transform);
                    rail3.Extend(100,100);

                    //myDoc.Objects.Add(rail1.ToNurbsCurve());
                    //myDoc.Objects.Add(rail3.ToNurbsCurve());


                    //Calculate the intersection point of rail1 and rail3
                    if (Intersection.LineLine(rail1, rail3, out double a, out double b))
                    {
                        RhinoApp.WriteLine("No intersection found when creating the driven gear of end gear");
                        return;
                    }

                    Point3d intersectPoint = rail3.PointAt(b);
                    //myDoc.Objects.AddPoint(intersectPoint);

                    //Use the intersection point to get the Center Point of the driven gear of end gear


                    //Create the driven gear
                    #endregion
                    */



                    #endregion

                    #endregion

                }

                #endregion





                //if(currModel.GetBoundingBox(true).Center.DistanceTo(new Point3d(0,0,0)) > cuttedBrep[1].GetBoundingBox(true).Center.DistanceTo(curr))


                //测试齿轮cone angle
                //Vector3d baseVector = new Vector3d(0, 0, 1);

                //Plane plane = new Plane(new Point3d(0, 0, 0), baseVector);
                //myDoc.Objects.Add(new PlaneSurface(essentials.CutterPlane, new Interval(0, 10), new Interval(0, 10)));
                //myDoc.Objects.Add(new PlaneSurface(plane, new Interval(0, 10), new Interval(0, 10)));
                //RhinoApp.WriteLine("Base has normal: " + plane.Normal.X + ", " + plane.Normal.Y + ", " + plane.Normal.Z);
                //RhinoApp.WriteLine("Plane has normal: " + end_gear_dir.X + ", " + end_gear_dir.Y + ", " + end_gear_dir.Z);
                //RhinoApp.WriteLine("The angle between the plane and the base is: " + ConeAngle(baseVector, end_gear_dir));

                //测试齿轮面向的方向

                //测试齿轮
                //Point3d customized_part_center_point = new Point3d(0, 0, 0);
                //Vector3d customized_part_gear_x_dir = new Vector3d(0, 0, 0);
                //int customized_part_teethNum = 30;
                //double customized_part_mod = 1.5;
                //double customized_part_pressure_angle = 20;
                //double Thickness = 5;
                //double customized_part_coneAngle = ConeAngle(new Vector3d(0, 0, 1), end_gear_dir);


                //BevelGear customized_gear = new BevelGear(customized_part_center_point, end_gear_dir, customized_part_gear_x_dir, customized_part_teethNum, customized_part_mod, customized_part_pressure_angle, Thickness, 0, customized_part_coneAngle, false);

                //BevelGear second_gear = new BevelGear(customized_part_center_point, new Vector3d(0, 0, 1), customized_part_gear_x_dir, customized_part_teethNum, customized_part_mod, customized_part_pressure_angle, Thickness, 0, customized_part_coneAngle, false);

                //myDoc.Objects.Add(customized_gear.Model);
                //myDoc.Objects.Add(second_gear.Model);

            }




        }

        private Vector3d Direction(Point3d main_center, Point3d endEffector_center, Vector3d normal)
        {
            


            return new Vector3d(0, 0, 0);
        }



        private double ConeAngle(Vector3d baseVector, Vector3d customizedPlaneNormal)
        {
            double dotProduct = Vector3d.Multiply(baseVector, customizedPlaneNormal);
            double lengthA = baseVector.Length;
            double lengthB = customizedPlaneNormal.Length;
            double cosTheta = dotProduct / (lengthA * lengthB);
            double theta = Math.Acos(cosTheta);
            double angle = 180 - theta * (180 / Math.PI);

            return angle;
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
            get { return new Guid("8F7D4FF7-E481-405A-ACCB-EA91074D10C9"); }
        }
    }
}