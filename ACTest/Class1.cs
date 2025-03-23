using ACTest;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using AcadApp = Autodesk.AutoCAD.Windows;
using Wnd = System.Windows.Forms;
using System.IO;

//声明命令存储位置，加快执行查找速度
[assembly: CommandClass(typeof(Class1))]

namespace ACTest
{
    public class Class1
    {
        //封装的事务处理类
        public ObjectId AddEntityToModelSpace(Database db, Entity ent)
        {
            ObjectId objectId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                objectId = btr.AppendEntity(ent);
                trans.AddNewlyCreatedDBObject(ent, true);
                trans.Commit();
            }
            return objectId;
        }
        public ObjectId[] AddEntityToModelSpace(Database db, params Entity[] ent)
        {
            ObjectId[] objectId = new ObjectId[ent.Length];
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                for (int i = 0; i < ent.Length; i++)
                {
                    objectId[i] = btr.AppendEntity(ent[i]);
                    trans.AddNewlyCreatedDBObject(ent[i], true);
                }
                trans.Commit();
            }
            return objectId;
        }
        public List<ObjectId> AddEntityToModelSpace(Database db, List<Entity> ent)
        {
            List<ObjectId> objectIds = new List<ObjectId>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                foreach (var item in ent)
                {
                    btr.AppendEntity(item);
                    objectIds.Add(item.ObjectId);
                    trans.AddNewlyCreatedDBObject(item, true);
                }
                trans.Commit();
            }
            return objectIds;
        }
        public ObjectId AddLineToModelSpace(Database db, Point3d startPoint, Point3d endPoint)
        {
            return AddEntityToModelSpace(db, new Line(startPoint, endPoint));
        }
        public ObjectId AddLineToModelSpace(Database db, Point3d startPoint, Double length, Double degree)
        {

            double X = startPoint.X + length * Math.Cos(DegreeToAngle(degree));
            double Y = startPoint.Y + length * Math.Sin(DegreeToAngle(degree));
            Point3d endPoint = new Point3d(X, Y, 0);
            return AddEntityToModelSpace(db, new Line(startPoint, endPoint));
        }
        public double DegreeToAngle(Double degree)
        {
            return degree * Math.PI / 180;
        }
        public double AngleToDegree(Double angle)
        {
            return angle * 180 / Math.PI;
        }
        public ObjectId AddArcToModelSpace(Database db, Point3d center, Double radius, Double startDegree, double endDegree)
        {
            return AddEntityToModelSpace(db, new Arc(center, radius, DegreeToAngle(startDegree), DegreeToAngle(endDegree)));
        }
        private bool IsOnOneLine(Point3d p1, Point3d p2, Point3d p3)
        {
            Vector3d v21 = p2.GetVectorTo(p1);
            Vector3d v23 = p2.GetVectorTo(p2);
            if (v21.GetAngleTo(v23) == 0 || v21.GetAngleTo(v23) == Math.PI)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public ObjectId AddArcToModelSpace(Database db, Point3d startPoint, Point3d pointOnArc, Point3d endPoint)
        {
            if (IsOnOneLine(pointOnArc, startPoint, endPoint))
            {
                return ObjectId.Null;
            }
            CircularArc3d arc3D = new CircularArc3d(startPoint, pointOnArc, endPoint);
            Arc arc = new Arc(arc3D.Center, arc3D.Radius, GetAngleToAxisX(arc3D.Center, startPoint), GetAngleToAxisX(arc3D.Center, endPoint));
            return AddEntityToModelSpace(db, arc);
        }
        public double GetAngleToAxisX(Point3d p1, Point3d p2)
        {
            Vector3d temp = new Vector3d(1, 0, 0);
            Vector3d VsToe = p1.GetVectorTo(p2);
            return VsToe.Y > 0 ? temp.GetAngleTo(VsToe) : -temp.GetAngleTo(VsToe);
        }
        public ObjectId AddArcToModelSpace(Database db, Point3d centerPoint, Point3d startPoint, double degree)
        {
            double radius = GetDistanceBetween2Points(centerPoint, startPoint);
            //Get起点角度
            double startAngle = GetAngleToAxisX(centerPoint, startPoint);
            Arc arc = new Arc(centerPoint, radius, startAngle, startAngle + DegreeToAngle(degree));
            return AddEntityToModelSpace(db, arc);
        }
        public static double GetDistanceBetween2Points(Point3d p1, Point3d p2)
        {
            return Math.Abs(Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y) + (p1.Z - p2.Z) * (p1.Z - p2.Z)));
        }
        public ObjectId AddCircleToModelSpace(Point3d centerPoint, double radius)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            return AddEntityToModelSpace(db, new Circle(centerPoint, new Vector3d(0, 0, 1), radius));
        }
        public ObjectId AddCircleToModelSpace(Point3d p1, Point3d p2)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Point3d centerPoint = GetCenterPointBy2Points(p1, p2);
            double radius = GetDistanceBetween2Points(p1, centerPoint);
            return AddCircleToModelSpace(centerPoint, radius);

        }
        public ObjectId AddCircleToModelSpace(Point3d p1, Point3d p2, Point3d p3)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            if (IsOnOneLine(p1, p2, p3))
            {
                return ObjectId.Null;
            }
            CircularArc3d arc3D = new CircularArc3d(p1, p2, p3);
            return AddCircleToModelSpace(arc3D.Center, arc3D.Radius);
        }
        private Point3d GetCenterPointBy2Points(Point3d p1, Point3d p2)
        {
            return new Point3d((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2, (p1.Z + p2.Z) / 2);
        }
        public ObjectId AddRectToModelSpace(Database db, Point2d pt1, Point2d pt2)
        {
            Polyline polyline = new Polyline();
            Point2d p1 = new Point2d(Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y));
            Point2d p2 = new Point2d(Math.Max(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y));
            Point2d p3 = new Point2d(Math.Max(pt1.X, pt2.X), Math.Max(pt1.Y, pt2.Y));
            Point2d p4 = new Point2d(Math.Min(pt1.X, pt2.X), Math.Max(pt1.Y, pt2.Y));
            polyline.AddVertexAt(0, p1, 0, 0, 0);
            polyline.AddVertexAt(1, p2, 0, 0, 0);
            polyline.AddVertexAt(2, p3, 0, 0, 0);
            polyline.AddVertexAt(3, p4, 0, 0, 0);
            polyline.Closed = true;
            return AddEntityToModelSpace(db, polyline);
        }
        public ObjectId HatchEntity(string patternName, double scale, double degree, ObjectId entId)
        {
            ////写下句免总是要传db的值
            Database db = HostApplicationServices.WorkingDatabase;
            ObjectId hatchId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Hatch hatch = new Hatch();
                hatch.PatternScale = scale;
                hatch.SetHatchPattern(HatchPatternType.PreDefined, patternName);
                hatch.BackgroundColor = Color.FromRgb(127, 12, 12);
                //hatch.ColorIndex = hatchColorIndex;
                hatch.ColorIndex = 1;
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                hatchId = btr.AppendEntity(hatch);
                trans.AddNewlyCreatedDBObject(hatch, true);
                //下面的还不能提前声明
                hatch.PatternAngle = DegreeToAngle(degree);
                hatch.Associative = true;
                //设置关联图形和方式
                ObjectIdCollection ids = new ObjectIdCollection();
                ids.Add(entId);
                hatch.AppendLoop(HatchLoopTypes.Outermost, ids);
                hatch.EvaluateHatch(true);
                trans.Commit();
            }
            return hatchId;
        }
        //0303 改颜色
        public void ChangeEntityColor(ObjectId cId, short colorIndex)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                Entity ent1 = cId.GetObject(OpenMode.ForWrite) as Entity;
                ent1.ColorIndex = colorIndex;
                trans.Commit();
            }
        }
        public void ChangeEntityColor(Entity ent, short colorIndex)
        {
            //在调整前务必要检验图形是否是新建的
            if (ent.IsNewObject)
            {
                ent.ColorIndex = colorIndex;
            }
            else { ChangeEntityColor(ent.Id, colorIndex); }
        }
        public void MoveEntity(ObjectId entId, Point3d sourcePoint, Point3d targetPoint)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                Entity ent1 = entId.GetObject(OpenMode.ForWrite) as Entity;
                Vector3d v = sourcePoint.GetVectorTo(targetPoint);
                Matrix3d mt = Matrix3d.Displacement(v);
                ent1.TransformBy(mt);
                trans.Commit();
            }
        }
        public void MoveEntity(Entity ent, Point3d sourcePoint, Point3d targetPoint)
        {
            //在调整前务必要检验图形是否是新建的
            if (ent.IsNewObject)
            {
                Vector3d v = sourcePoint.GetVectorTo(targetPoint);
                Matrix3d mt = Matrix3d.Displacement(v);
                ent.TransformBy(mt);
            }
            else { MoveEntity(ent.Id, sourcePoint, targetPoint); }
        }
        public Entity CopyEntity(ObjectId entId, Point3d sourcePoint, Point3d targetPoint)
        {
            Entity entNew = null;
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                Entity ent1 = entId.GetObject(OpenMode.ForWrite) as Entity;
                Vector3d v = sourcePoint.GetVectorTo(targetPoint);
                Matrix3d mt = Matrix3d.Displacement(v);
                entNew = ent1.GetTransformedCopy(mt);
                trans.Commit();
            }
            return entNew;
        }
        public Entity CopyEntity(Entity ent, Point3d sourcePoint, Point3d targetPoint)
        {
            Entity entNew = null;
            //在调整前务必要检验图形是否是新建的
            if (ent.IsNewObject)
            {
                Vector3d v = sourcePoint.GetVectorTo(targetPoint);
                Matrix3d mt = Matrix3d.Displacement(v);
                entNew = ent.GetTransformedCopy(mt);
            }
            else { entNew = CopyEntity(ent.Id, sourcePoint, targetPoint); }
            return entNew;
        }
        //旋转只考虑中心旋转的情况
        public void RotateEntity(ObjectId entId, Point3d center, double degree)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                Entity ent1 = entId.GetObject(OpenMode.ForWrite) as Entity;
                Matrix3d mt = Matrix3d.Rotation(DegreeToAngle(degree), Vector3d.ZAxis, center);
                ent1.TransformBy(mt);
                trans.Commit();
            }
        }
        public void RotateEntity(Entity ent, Point3d center, double degree)
        {
            //在调整前务必要检验图形是否是新建的
            if (ent.IsNewObject)
            {
                Matrix3d mt = Matrix3d.Rotation(DegreeToAngle(degree), Vector3d.ZAxis, center);
                ent.TransformBy(mt);
            }
            else { RotateEntity(ent.Id, center, degree); }
        }
        public Entity MirrorEntity(ObjectId entId, Point3d p1, Point3d p2, bool isDeleteSource)
        {
            Entity entR;
            //计算镜像变换矩阵
            Matrix3d mt = Matrix3d.Mirroring(new Line3d(p1, p2));
            //Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = entId.Database.TransactionManager.StartTransaction())
            {
                Entity ent = trans.GetObject(entId, OpenMode.ForWrite) as Entity;
                entR = ent.GetTransformedCopy(mt);
                //是否移动还是复制？
                if (isDeleteSource)
                {
                    ent.Erase();
                    //不能直接赋值给新的
                    //entR = ent;
                }
                trans.Commit();
            }
            return entR;
        }
        public Entity MirrorEntity(Entity ent, Point3d p1, Point3d p2, bool isDeleteSource)
        {
            Entity entR;
            if (ent.IsNewObject)
            {
                //计算镜像变换矩阵
                Matrix3d mt = Matrix3d.Mirroring(new Line3d(p1, p2));
                entR = ent.GetTransformedCopy(mt);
            }
            else
            {
                entR = MirrorEntity(ent.ObjectId, p1, p2, isDeleteSource);
            }
            return entR;
        }
        public void ScaleEntity(ObjectId entId, Point3d basePoint, double factor)
        {
            Matrix3d mt = Matrix3d.Scaling(factor, basePoint);
            using (Transaction trans = entId.Database.TransactionManager.StartTransaction())
            {
                Entity ent = entId.GetObject(OpenMode.ForWrite) as Entity;
                ent.TransformBy(mt);
                trans.Commit();
            }
        }
        public void ScaleEntity(Entity ent, Point3d basePoint, double factor)
        {
            if (ent.IsNewObject)
            {
                Matrix3d mt = Matrix3d.Scaling(factor, basePoint);
                ent.TransformBy(mt);
            }
            else { ScaleEntity(ent.ObjectId, basePoint, factor); }
        }
        public void DeleteEntiy(ObjectId entId)
        {
            using (Transaction trans = entId.Database.TransactionManager.StartTransaction())
            {
                Entity ent = entId.GetObject(OpenMode.ForWrite) as Entity;
                ent.Erase();
                trans.Commit();
            }
        }
        public void DeleteEntiy(Entity ent)
        {
            DeleteEntiy(ent.ObjectId);
        }
        public PromptPointResult GetPoint(PromptPointOptions ppo)
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            ppo.AllowNone = true;
            return ed.GetPoint(ppo);

        }
        public PromptPointResult GetPoint(Editor editor, string promptStr)
        {
            PromptPointOptions ppo = new PromptPointOptions(promptStr);
            //允许回车和空格废弃
            ppo.AllowNone = true;
            return GetPoint(ppo);
        }
        public PromptPointResult GetPoint(Editor editor, string promptStr, Point3d pointBase, params string[] keyWord)
        {
            PromptPointOptions ppo = new PromptPointOptions(promptStr);
            //允许回车和空格废弃
            ppo.AllowNone = true;
            for (int i = 0; i < keyWord.Length; i++)
            {
                ppo.Keywords.Add(keyWord[i]);
            }
            //取消默认的显示选项
            ppo.AppendKeywordsToMessage = false;
            ppo.BasePoint = pointBase;
            ppo.UseBasePoint = true;
            return GetPoint(ppo);
        }
        private Point3d GetLineStartPoint(ObjectId lineId)
        {
            Point3d startPoint = new Point3d();
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Line line = lineId.GetObject(OpenMode.ForRead) as Line;
                startPoint = line.StartPoint;
                trans.Commit();
            }
            return startPoint;
        }
        public class CircleJig : EntityJig
        {
            private double jRadius;
            public CircleJig(Point3d center) : base(new Circle())
            {
                (Entity as Circle).Center = center;
            }
            //图形更新，自带事务处理无需声明
            protected override bool Update()
            {
                if (jRadius > 0)
                {
                    (Entity as Circle).Radius = jRadius;
                }
                return true;
            }
            //移动鼠标时改变属性 ed.Drag联动
            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                JigPromptPointOptions jppo = new JigPromptPointOptions("\n 请指定圆上的一个点：[放弃（U）]");
                //空格键控制
                char space = (char)32;
                jppo.Keywords.Add("U");
                jppo.Keywords.Add(space.ToString());
                jppo.UserInputControls = UserInputControls.Accept3dCoordinates;
                jppo.Cursor = CursorType.RubberBand;
                jppo.BasePoint = (Entity as Circle).Center;
                jppo.UseBasePoint = true;
                PromptPointResult ppr = prompts.AcquirePoint(jppo);
                jRadius = GetDistanceBetween2Points(ppr.Value, (Entity as Circle).Center);
                return SamplerStatus.NoChange;
            }
            public Entity GetEntity()
            {
                return Entity;
            }
        }
        private void ChangeColor(SelectionSet sSet)
        {
            ObjectId[] ids = sSet.GetObjectIds();
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //不是新建不必设置块表
                for (int i = 0; i < sSet.Count; i++)
                {
                    Entity entity = ids[i].GetObject(OpenMode.ForWrite) as Entity;
                    entity.ColorIndex = 1;
                }
                trans.Commit();
            }
        }
        private void ChangeColor(List<ObjectId> ids)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //不是新建不必设置块表
                for (int i = 0; i < ids.Count; i++)
                {
                    Entity entity = ids[i].GetObject(OpenMode.ForWrite) as Entity;
                    entity.ColorIndex = 1;
                }
                trans.Commit();
            }
        }
        private List<Point3d> getSelectPoint(SelectionSet sSet)
        {
            List<Point3d> points = new List<Point3d>();
            ObjectId[] ids = sSet.GetObjectIds();
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //不是新建不必设置块表
                for (int i = 0; i < sSet.Count; i++)
                {
                    Entity entity = ids[i].GetObject(OpenMode.ForRead) as Entity;
                    Point3d center = (entity as Circle).Center;
                    double radius = (entity as Circle).Radius;
                    points.Add(new Point3d(center.X + radius, center.Y + radius, center.Z));
                }
                trans.Commit();
            }
            return points;
        }
        private List<Entity> GetEntity(ObjectId[] ids)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            List<Entity> entList = new List<Entity>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId item in ids)
                {
                    Entity entity = item.GetObject(OpenMode.ForRead) as Entity;
                    entList.Add(entity);
                }
                trans.Commit();
            }
            return entList;
        }
        private void LowColorEntity(List<Entity> entList, byte colorNum)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                foreach (Entity item in entList)
                {
                    Entity entity = item.ObjectId.GetObject(OpenMode.ForWrite) as Entity;
                    entity.ColorIndex = colorNum;
                }
                trans.Commit();
            }
        }
        public class MoveJig : DrawJig
        {
            Database db = HostApplicationServices.WorkingDatabase;
            private List<Entity> jEntList = new List<Entity>();
            private Point3d jPointBase;
            private Point3d jPointPre;
            //默认矩阵向量平移无变化
            Matrix3d jMt = Matrix3d.Displacement(new Vector3d(0, 0, 0));
            public MoveJig(List<Entity> entList, Point3d pointBase)
            {
                jEntList = entList;
                jPointBase = pointBase;
                jPointPre = pointBase;
            }
            protected override bool WorldDraw(Autodesk.AutoCAD.GraphicsInterface.WorldDraw draw)
            {
                foreach (var item in jEntList)
                {
                    draw.Geometry.Draw(item);
                }
                return true;
            }
            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                //声明提示类
                JigPromptPointOptions jppo = new JigPromptPointOptions("\n 指定第二点或<使用第一点作为位移>:");
                jppo.Cursor = CursorType.RubberBand;
                jppo.BasePoint = jPointBase;
                jppo.UseBasePoint = true;
                jppo.Keywords.Add(" ");
                jppo.AppendKeywordsToMessage = false;
                jppo.UserInputControls = UserInputControls.Accept3dCoordinates;
                //取动态坐标值
                PromptPointResult ppr = prompts.AcquirePoint(jppo);
                Point3d curPoint = ppr.Value;
                //矩阵变化
                if (curPoint != jPointPre)
                {
                    Vector3d vector = jPointPre.GetVectorTo(curPoint);
                    jMt = Matrix3d.Displacement(vector);
                    foreach (var item in jEntList)
                    {
                        item.TransformBy(jMt);
                    }
                    //using (Transaction trans = db.TransactionManager.StartTransaction())
                    //{
                    //    foreach (var item in jEntList)
                    //    {
                    //        Entity entity = item.ObjectId.GetObject(OpenMode.ForWrite) as Entity;
                    //        item.TransformBy(jMt);
                    //    }
                    //    trans.Commit();
                    //}
                }
                jPointPre = curPoint;
                //if (ppr.Status == PromptStatus.Cancel)
                //{
                //    return SamplerStatus.Cancel;
                //}
                //else 
                return SamplerStatus.NoChange;
            }
            public List<Entity> GetEntity()
            {
                return jEntList;
            }
        }
        private List<Entity> CopyEntity(List<Entity> entList, Matrix3d mt)
        {
            List<Entity> entListCopy = new List<Entity>();
            foreach (var item in entList)
            {
                entListCopy.Add(item.GetTransformedCopy(mt));
            }
            return entListCopy;
        }
        private void DeleteEntitys(Entity[] ents)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                foreach (Entity item in ents)
                {
                    Entity entity = item.ObjectId.GetObject(OpenMode.ForWrite) as Entity;
                    entity.Erase();
                }
                trans.Commit();
            }
        }
        public struct TextSpecialSymbol
        {
            public static readonly string Degree = @"\U+00B0";      //角度(°)
            public static readonly string Tolerance = @"\U+00B1";   //公差（±）
            public static readonly string Diameter = @"\U+00D8";    //直径（Ø）
            public static readonly string Angle = @"\U+2220";       //角度（∠）
            public static readonly string AlmostEqual = @"\U+2248"; //约等于（≈）
            public static readonly string LineBoundary = @"\U+E100";    // 边界线
            public static readonly string LineCenter = @"\U+2104";  // 中心线
            public static readonly string Delta = @"\U+0394";       // 增量(Δ)
            public static readonly string ElectricalPhase = @"\U+0278"; // 电相位(φ)
            public static readonly string LineFlow = @"\U+E101";    // 流线
            public static readonly string Identity = @"\U+2261";    // 标识
            public static readonly string InitialLength = @"\U+E200";   // 初始长度
            public static readonly string LineMonument = @"\U+E102";    // 界碑线
            public static readonly string Notequal = @"\U+2260";    // 不相等(≠)
            public static readonly string Ohm = @"\U+2126";     // 欧姆
            public static readonly string Omega = @"\U+03A9";   // 欧米加(Ω)
            public static readonly string LinePlate = @"\U+214A";   // 地界线
            public static readonly string Subscript2 = @"\U+2082";  // 下标2
            public static readonly string Square = @"\U+00B2";      // 平方
            public static readonly string Cube = @"\U+00B3";        // 立方
            public static readonly string Overline = @"%%o";    // 单行文字上划线
            public static readonly string Underline = @"%%u";   // 单行文字下划线
            public static readonly string Alpha = @"\U+03B1";   // 希腊字母(α)
            public static readonly string Belta = @"\U+03B2";   //希腊字母（β）
            public static readonly string Gamma = @"\U+03B3";   //希腊字母（γ ）
            public static readonly string Theta = @"\U+03B8";   //希腊字母（θ ）
            public static readonly string SteelBar1 = @"\U+0082";   // 一级钢筋符号
            public static readonly string SteelBar2 = @"\U+0083";   // 二级钢筋符号
            public static readonly string SteelBar3 = @"\U+0084";   // 三级钢筋符号
            public static readonly string SteelBar4 = @"\U+0085";   // 四级钢筋符号
        }
        public struct MTextStackType
        {
            public static readonly string Horizental = "/"; //水平堆叠
            public static readonly string Italic = "#";  //斜分堆叠
            public static readonly string Tolerance = "^"; //容差堆叠
        }
        public void PickMText() //多行文字转字符串没成功
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptEntityResult per = ed.GetEntity("\n 请选择多行文字：");
            if (per.Status == PromptStatus.OK) return;
            Entity ent;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                ent = per.ObjectId.GetObject(OpenMode.ForRead) as Entity;
                MText mt = (MText)ent;
                string text = mt.Contents;
                MessageBox.Show(text);
            }

            //ed.WriteMessage(text);
        }
        private Entity GetEntity(Database db, ObjectId entId)
        {
            Entity ent;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                ent = entId.GetObject(OpenMode.ForRead) as Entity;
            }
            return ent;
        }
        private List<Point3d> GetBaseLineDivPoints(Line line, double divDist)
        {
            List<Point3d> points = new List<Point3d>();
            int divNum = (int)(line.Length / divDist);
            Point3d startPoint = line.StartPoint;
            double angle = line.Angle;
            for (int i = 0; i < divNum + 1; i++)
            {
                points.Add(PolarPoint(startPoint, divDist * i, angle));
            }
            if (divDist * divNum != line.Length)
            {
                points.Add(line.EndPoint);
            }
            return points;
        }
        private List<Point3d> GetBaseLineDivPoints(Line line, int divNum)
        {
            return GetBaseLineDivPoints(line, line.Length / divNum);
        }
        private Point3d PolarPoint(Point3d startPoint, double dist, double angle)
        {
            //double X = startPoint.X + dist * Math.Cos(AngleToDegree(angle));
            //double Y = startPoint.Y + dist * Math.Sin(AngleToDegree(angle));
            double X = startPoint.X + dist * Math.Cos((angle));
            double Y = startPoint.Y + dist * Math.Sin((angle));
            return new Point3d(X, Y, 0);
        }
        public List<ObjectId> AddEntityReturnList(Database db, Entity[] ent)
        {
            List<ObjectId> ids = new List<ObjectId>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                //foreach (var entity in ent)
                //{
                //    ids.Add(btr.AppendEntity(entity));
                //    trans.AddNewlyCreatedDBObject(entity, true);
                //}
                for (int i = 0; i < ent.Length; i++)
                {
                    ids.Add(btr.AppendEntity(ent[i]));
                    trans.AddNewlyCreatedDBObject(ent[i], true);
                }
                //为什么直线生成的多个点只显示第一个且无法捕捉后面的？？
                trans.Commit();
            }
            return ids;
        }
        private List<Line> GetDivLines(Point3d[] points, double angle, double length)
        {
            List<Line> lines = new List<Line>();
            foreach (var item in points)
            {
                lines.Add(new Line(item, PolarPoint(item, length, angle)));
            }
            return lines;
        }
        private List<Line> ModifyDivLines(Line[] lines, Entity ent)
        {
            List<Line> divLines = new List<Line>();
            foreach (var line in lines)
            {
                Point3dCollection insertPoints = new Point3dCollection();
                line.IntersectWith(ent, Intersect.ExtendThis, insertPoints, IntPtr.Zero, IntPtr.Zero);
                if (insertPoints.Count > 0)
                {
                    line.EndPoint = insertPoints[0];
                    divLines.Add(line);
                }
            }
            return divLines;
        }
        private List<DBText> GetDivDimTexts(Line[] lines, double height, double dist)
        {
            List<DBText> texts = new List<DBText>();
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                foreach (var item in lines)
                {
                    DBText text = new DBText();
                    //text.TextString = item.Length.ToString();
                    if (item.Angle >= 0 && item.Angle <= Math.PI)
                    {
                        text.TextString = string.Format("{0:N}", item.Length);
                        text.Rotation = item.Angle;
                        text.HorizontalMode = TextHorizontalMode.TextRight;
                    }
                    else
                    {
                        text.TextString = "-" + string.Format("{0:N}", item.Length);
                        text.Rotation = item.Angle + Math.PI;
                        text.HorizontalMode = TextHorizontalMode.TextLeft;
                    }
                    text.Position = PolarPoint(item.StartPoint, dist, item.Angle);
                    text.Height = height;
                    text.VerticalMode = TextVerticalMode.TextVerticalMid;
                    text.AlignmentPoint = text.Position;
                    texts.Add(text);
                }

                trans.Commit();
            }
            return texts;
        }
        private AddLayerResult AddLayer(Database db, string layerName)
        {
            AddLayerResult res = new AddLayerResult();
            try
            {
                SymbolUtilityServices.ValidateSymbolName(layerName, false);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                res.status = AddLayerStatus.IllegalLayerName;
                return res;
            }
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (!lt.Has(layerName))
                {
                    LayerTableRecord ltr = new LayerTableRecord();
                    ltr.Name = layerName;
                    lt.UpgradeOpen();
                    res.value = lt.Add(ltr);
                    lt.DowngradeOpen();
                    trans.AddNewlyCreatedDBObject(ltr, true);
                    trans.Commit();
                    res.status = AddLayerStatus.AddLayerOK;
                    res.layerName = layerName;
                }
                else
                {
                    MessageBox.Show("图层已存在");
                    res.status = AddLayerStatus.LayerNameExist;
                }
            }
            return res;
        }
        private enum AddLayerStatus
        {
            AddLayerOK,
            IllegalLayerName,
            LayerNameExist
        }
        private struct AddLayerResult
        {
            public AddLayerStatus status;
            public string layerName;
            public ObjectId value;
            //ObjectId value = ObjectId.Null;
        }
        public ChangePropertyStatus ChangeLayerColor(Database db, string layerName, short colorIndex)
        {
            ChangePropertyStatus status;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (lt.Has(layerName))
                {
                    LayerTableRecord ltr = lt[layerName].GetObject(OpenMode.ForWrite) as LayerTableRecord;
                    ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
                    status = ChangePropertyStatus.ChangeDone;
                }
                else
                {
                    MessageBox.Show("图层不存在");
                    status = ChangePropertyStatus.NotExist;
                }
                trans.Commit();
            }
            return status;
        }
        public bool ChangeLayerLock(Database db, string layerName)
        {
            //ChangePropertyStatus status;
            bool status;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (lt.Has(layerName))
                {
                    LayerTableRecord ltr = lt[layerName].GetObject(OpenMode.ForWrite) as LayerTableRecord;
                    ltr.IsOff = true;
                    ltr.IsFrozen = true;
                    ltr.IsLocked = true;
                    //ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
                    //status = ChangePropertyStatus.ChangeDone;
                    status = true;
                }
                else
                {
                    MessageBox.Show("图层不存在");
                    //status = ChangePropertyStatus.NotExist;
                    status = false;
                }
                trans.Commit();
            }
            return status;
        }
        public bool ChangeLayerLineWeight(Database db, string layerName, LineWeight lineWeight)
        {
            bool status;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (lt.Has(layerName))
                {
                    LayerTableRecord ltr = lt[layerName].GetObject(OpenMode.ForWrite) as LayerTableRecord;
                    ltr.LineWeight = lineWeight;
                    status = true;
                }
                else
                {
                    MessageBox.Show("图层不存在");
                    status = false;
                }
                trans.Commit();
            }
            return status;
        }
        public enum ChangePropertyStatus
        {
            ChangeDone,
            NotExist,
        }
        public bool SetCurrentLayer(Database db, string layerName)
        {
            bool isSetOK = false;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (lt.Has(layerName))
                {
                    ObjectId layerId = lt[layerName];
                    if (db.Clayer != layerId)
                    {
                        db.Clayer = layerId;
                        isSetOK = true;
                    }
                }
                else
                {
                    MessageBox.Show("图层不存在");
                    isSetOK = false;
                }
                trans.Commit();
            }
            return isSetOK;
        }
        public List<LayerTableRecord> GetAllLayers(Database db)
        {
            List<LayerTableRecord> layerList = new List<LayerTableRecord>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (ObjectId item in lt)
                {
                    LayerTableRecord ltr = item.GetObject(OpenMode.ForRead) as LayerTableRecord;
                    layerList.Add(ltr);
                }
                trans.Commit();
            }
            return layerList;
        }
        public List<string> GetAllLayerNames(Database db)
        {
            List<string> layerList = new List<string>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                //要先对图层内容检查才能后续
                lt.GenerateUsageData();
                foreach (ObjectId item in lt)
                {
                    LayerTableRecord ltr = item.GetObject(OpenMode.ForRead) as LayerTableRecord;
                    layerList.Add(ltr.Name);
                }
                trans.Commit();
            }
            return layerList;
        }
        public bool DeleteLayer(Database db, string layerName)
        {
            if (layerName == "0" || layerName == "Defpoints") return false;
            bool canBeDelete = false;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                lt.GenerateUsageData();
                if (lt.Has(layerName))
                {
                    LayerTableRecord ltr = lt[layerName].GetObject(OpenMode.ForWrite) as LayerTableRecord;
                    //ObjectId layerId = lt[layerName];
                    if (!ltr.IsUsed && db.Clayer != lt[layerName])
                    {
                        ltr.Erase();
                        canBeDelete = true;
                    }
                }
                else
                {
                    MessageBox.Show("图层不存在");
                    canBeDelete = true;
                }
                trans.Commit();
            }
            return canBeDelete;
        }
        public bool DeleteLayer(Database db, string layerName, bool forceDelete)
        {
            if (layerName == "0" || layerName == "Defpoints") return false;
            bool canBeDelete = false;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                lt.GenerateUsageData();
                if (lt.Has(layerName))
                {
                    LayerTableRecord ltr = lt[layerName].GetObject(OpenMode.ForWrite) as LayerTableRecord;
                    //ObjectId layerId = lt[layerName];
                    if (forceDelete)
                    {
                        if (ltr.IsUsed)
                        {
                            DeleteAllEntityInLayer(ltr);
                        }
                        if (db.Clayer == lt[layerName])
                        {
                            db.Clayer = lt["0"];
                        }
                        ltr.Erase();
                        canBeDelete = true;
                    }
                    else
                    {
                        if (!ltr.IsUsed && db.Clayer != lt[layerName])
                        {
                            ltr.Erase();
                            canBeDelete = true;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("图层不存在");
                    canBeDelete = true;
                }
                trans.Commit();
            }
            return canBeDelete;
        }
        public void DeleteAllEntityInLayer(LayerTableRecord ltr)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            TypedValue[] value = new TypedValue[]
            {
                new TypedValue((int)DxfCode.LayerName,ltr.Name)
            };
            SelectionFilter filter = new SelectionFilter(value);
            PromptSelectionResult psr = ed.SelectAll(filter);
            if (psr.Status == PromptStatus.OK)
            {
                ObjectId[] ids = psr.Value.GetObjectIds();
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    //BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    //BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite) as BlockTableRecord;
                    //LayerTable lt = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                    //lt.GenerateUsageData();
                    foreach (ObjectId item in ids)
                    {
                        Entity ent = item.GetObject(OpenMode.ForWrite) as Entity;
                        ent.Erase();
                    }
                    trans.Commit();
                }
            }
        }
        public void DeleteVoidLayer(Database db)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                lt.GenerateUsageData();
                foreach (ObjectId item in lt)
                {
                    LayerTableRecord ltr = item.GetObject(OpenMode.ForWrite) as LayerTableRecord;
                    if (!ltr.IsUsed)
                    {
                        ltr.Erase();
                    }
                }
                trans.Commit();
            }
        }
        private void AddTextStyle(Database db, string textStyleName)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                TextStyleTable tst = trans.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                if (!tst.Has(textStyleName))
                {
                    TextStyleTableRecord tstr = new TextStyleTableRecord();
                    tstr.Name = textStyleName;
                    tst.UpgradeOpen();
                    tst.Add(tstr);
                    trans.AddNewlyCreatedDBObject(tstr, true);
                    tst.DowngradeOpen();
                }
                trans.Commit();
            }
        }
        static ObjectId GetArrowObjectId(string arrow, string newArrName)
        {
            ObjectId arrObjId = ObjectId.Null;
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            string oldArrName = Autodesk.AutoCAD.ApplicationServices.Application.GetSystemVariable(arrow) as string;
            Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable(arrow, newArrName);
            if (oldArrName.Length != 0)
                Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable(arrow, oldArrName);
            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                arrObjId = bt[newArrName];
                tr.Commit();
            }
            return arrObjId;
        }
        private ObjectId AddDimStyle(Database db, string dimStyleName)
        {
            ObjectId dimStyleId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                DimStyleTable dst = trans.GetObject(db.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;
                if (!dst.Has(dimStyleName))
                {
                    DimStyleTableRecord dstr = new DimStyleTableRecord();
                    dstr.Name = dimStyleName;
                    dstr.Dimsah = true;
                    //ObjectId id1 = GetArrowObjectId("DIMBLK1", "_DOT");
                    //ObjectId id2 = GetArrowObjectId("DIMBLK2", "_CLOSED");
                    ObjectId id1 = GetArrowObjectId("DIMBLK1", "_ARCHTICK");
                    ObjectId id2 = GetArrowObjectId("DIMBLK1", "_ARCHTICK");
                    dstr.Dimblk1 = id1;
                    dstr.Dimblk2 = id2;
                    dstr.Dimclrd = Color.FromColorIndex(ColorMethod.ByAci, 2);
                    dst.UpgradeOpen();
                    dimStyleId = dst.Add(dstr);
                    trans.AddNewlyCreatedDBObject(dstr, true);
                    dst.DowngradeOpen();
                }
                trans.Commit();
            }
            return dimStyleId;
        }
        private ObjectId LoadLineType(Database db, string lineTypeName)
        {
            ObjectId lineTypeId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LinetypeTable ltt = trans.GetObject(db.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                if (!ltt.Has(lineTypeName))
                {
                    //载入线型表
                    db.LoadLineTypeFile(lineTypeName, "acadiso.lin");
                    lineTypeId = ltt[lineTypeName];
                }
                trans.Commit();
            }
            return lineTypeId;
        }
        public class BlockTool
        {
            /// <summary>
            /// 更新块参照的属性
            /// </summary>
            /// <param name="BlockRefId">块参照的ObjectId</param>
            /// <param name="attrNameValues">属性字典</param>
            public void UpdateBlockAttr(ObjectId BlockRefId, Dictionary<string, string> attrNameValues)
            {
                using (Transaction trans = BlockRefId.Database.TransactionManager.StartTransaction())
                {
                    if (BlockRefId != ObjectId.Null)
                    {
                        BlockReference br = (BlockReference)BlockRefId.GetObject(OpenMode.ForRead);
                        foreach (ObjectId item in br.AttributeCollection)
                        {
                            AttributeReference attRef = (AttributeReference)item.GetObject(OpenMode.ForRead);
                            //判断属性字典中是否包含要更改的属性值
                            if (attrNameValues.ContainsKey(attRef.Tag.ToString()))
                            {
                                attRef.UpgradeOpen();
                                attRef.TextString = attrNameValues[attRef.Tag.ToString()].ToString();
                                attRef.DowngradeOpen();
                            }
                        }
                    }
                    trans.Commit();
                }
            }
            public ObjectId InsertBlockReference(Database db, ObjectId BlockRecordId, Point3d position, double rotation, Scale3d scale)
            {
                ObjectId blkRefId = ObjectId.Null;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (bt.Has(BlockRecordId))
                    {
                        BlockReference br = new BlockReference(position, BlockRecordId);
                        br.Rotation = rotation;
                        br.ScaleFactors = scale;
                        BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                        blkRefId = btr.AppendEntity(br);
                        trans.AddNewlyCreatedDBObject(br, true);
                    }
                    trans.Commit();
                }
                return blkRefId;
            }
            public ObjectId InsertBlockReference(Database db, ObjectId BlockRecordId, Point3d position)
            {
                ObjectId blkRefId = ObjectId.Null;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (bt.Has(BlockRecordId))
                    {
                        BlockReference br = new BlockReference(position, BlockRecordId);
                        BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                        blkRefId = btr.AppendEntity(br);
                        trans.AddNewlyCreatedDBObject(br, true);
                    }
                    trans.Commit();
                }
                return blkRefId;
            }
            public ObjectId InsertAttrBlockReference(Database db, ObjectId BlockRecordId, Point3d position)
            {
                ObjectId blkRefId = ObjectId.Null;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    if (bt.Has(BlockRecordId))
                    {
                        BlockReference br = new BlockReference(position, BlockRecordId);
                        blkRefId = btr.AppendEntity(br);
                        trans.AddNewlyCreatedDBObject(br, true);
                    }
                    trans.Commit();
                }
                return blkRefId;
            }
            public ObjectId InsertAttrBlockReference(Database db, ObjectId BlockRecordId, Point3d position, double rotation, Scale3d scale)
            {
                ObjectId blkRefId = ObjectId.Null;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    if (bt.Has(BlockRecordId))
                    {
                        BlockReference br = new BlockReference(position, BlockRecordId);
                        br.Rotation = rotation;
                        br.ScaleFactors = scale;
                        blkRefId = btr.AppendEntity(br);
                        //添加属性定义
                        BlockTableRecord blockRecord = BlockRecordId.GetObject(OpenMode.ForRead) as BlockTableRecord;
                        if (blockRecord.HasAttributeDefinitions)
                        {
                            foreach (ObjectId item in blockRecord)
                            {
                                DBObject obj = item.GetObject(OpenMode.ForRead);
                                if (obj is AttributeDefinition)
                                {
                                    //声明属性参照
                                    AttributeReference attrRef = new AttributeReference();
                                    attrRef.SetAttributeFromBlock((AttributeDefinition)obj, br.BlockTransform);
                                    br.AttributeCollection.AppendAttribute(attrRef);
                                    trans.AddNewlyCreatedDBObject(attrRef, true);
                                }
                            }
                        }
                        trans.AddNewlyCreatedDBObject(br, true);
                    }
                    trans.Commit();
                }
                return blkRefId;
            }
            public ObjectId AddBlockTableRecord(Database db, string btrName, List<Entity> ents)
            {
                ObjectId btrId = ObjectId.Null;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (!bt.Has(btrName))
                    {
                        BlockTableRecord btr = new BlockTableRecord();
                        btr.Name = btrName;
                        for (int i = 0; i < ents.Count; i++)
                        {
                            btr.AppendEntity(ents[i]);
                            //trans.AddNewlyCreatedDBObject(ents[i], true);
                        }
                        bt.UpgradeOpen();
                        bt.Add(btr);
                        trans.AddNewlyCreatedDBObject(btr, true);
                        bt.DowngradeOpen();
                    }
                    btrId = bt[btrName];
                    trans.Commit();
                }
                return btrId;
            }
        }
        public static class MyBlockTableRecord
        {
            //普通块定义
            private static string blockName = "block";
            private static List<Entity> blockEnts = new List<Entity>();
            private static ObjectId blockId = ObjectId.Null;
            public static string BlockName { get => blockName; set => blockName = value; }
            public static List<Entity> BlockEnts
            {
                get
                {
                    Circle circle = new Circle(Point3d.Origin, Vector3d.ZAxis, 10);
                    Line line1 = new Line(new Point3d(-5, 0, 0), new Point3d(5, 0, 0));
                    Line line2 = new Line(new Point3d(0, 5, 0), new Point3d(0, -5, 0));
                    blockEnts.Add(circle);
                    blockEnts.Add(line1);
                    blockEnts.Add(line2);
                    return MyBlockTableRecord.blockEnts;
                }
            }
            public static ObjectId BlockId { get => blockId; set => blockId = value; }
            //属性块定义
            private static string attrBlockName = "attrBlock";
            private static List<Entity> attrBlockEnts = new List<Entity>();
            private static ObjectId attrBlockId = ObjectId.Null;
            public static string AttrBlockName { get => attrBlockName; set => attrBlockName = value; }
            public static List<Entity> AttrBlockEnts
            {
                get
                {
                    Circle circle = new Circle(Point3d.Origin, Vector3d.ZAxis, 10);
                    Line line1 = new Line(new Point3d(-5, 0, 0), new Point3d(5, 0, 0));
                    Line line2 = new Line(new Point3d(0, 5, 0), new Point3d(0, -5, 0));
                    //加属性定义
                    AttributeDefinition attr = new AttributeDefinition();
                    attr.Position = new Point3d(12, 2, 0);
                    attr.Tag = "编号";
                    attr.Prompt = "天线编号";
                    attr.TextString = "attr-05";
                    attr.Height = 3;
                    AttributeDefinition attr2 = new AttributeDefinition();
                    attr2.Position = new Point3d(12, -8, 0);
                    attr2.Tag = "功率";
                    attr2.Prompt = "功率大小";
                    attr2.TextString = "10kW";
                    attr2.Height = 3;
                    attrBlockEnts.Add(circle);
                    attrBlockEnts.Add(line1);
                    attrBlockEnts.Add(line2);
                    attrBlockEnts.Add(attr);
                    attrBlockEnts.Add(attr2);
                    return MyBlockTableRecord.attrBlockEnts;
                }
            }
            public static ObjectId AttrBlockId { get => attrBlockId; set => attrBlockId = value; }
        }
        public struct TxtData
        {
            public string blockName;
            public string LayerName;
            public Point3d position;
            public Dictionary<string, string> attrs;
        }
        private int TransData(string[] contents, out List<TxtData> datas)
        {
            datas = new List<TxtData>();
            //根据返回行数判断出错位置 
            int row = -1;
            TxtData data = new TxtData();
            for (int i = 0; i < contents.Length; i++)
            {
                string[] con = contents[i].Split(new char[] { ',' });
                data.blockName = con[0];
                data.LayerName = con[1];
                double X, Y, Z;
                if (!double.TryParse(con[2], out X) || double.TryParse(con[3], out Y) || double.TryParse(con[4], out Z))
                {
                    row = i;
                    break;
                }
                data.position = new Point3d(X, Y, Z);
                //向字典写入值，要先声明内存
                data.attrs = new Dictionary<string, string>();
                data.attrs.Add("ZS", con[5]);
                data.attrs.Add("XS", con[6]);
                datas.Add(data);
            }
            return row;
        }
        //测试通用方法
        [CommandMethod("Cmd1", CommandFlags.UsePickSet)]
        public void Cmd1()
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;

            ////0323 Excel新建和交互
            ////0323 读取文本并整理，添加块实例
            //Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //Wnd.OpenFileDialog openFile = new Wnd.OpenFileDialog();
            //openFile.Filter = "文本文件(*.txt)|*.txt";
            //Wnd.DialogResult openRes = openFile.ShowDialog();
            //if (openRes == Wnd.DialogResult.OK)
            //{
            //    string[] contents = File.ReadAllLines(openFile.FileName);
            //    List<TxtData> datas = new List<TxtData>();
            //    int row = TransData(contents, out datas);
            //    if (row < 0)
            //    {
            //        //生成块InsertAttrBlockReference
            //        //判断图层表是否存在，生成图层/
            //        //向块表添加块
            //    }
            //    else ed.WriteMessage($"外部数据文件在第{row + 1}行出错，请检查");
            //}
            //0323 保存图形信息到txt
            //Wnd.SaveFileDialog saveFileDialog = new Wnd.SaveFileDialog();
            //string[] contents = new string[] { "111", "222" };
            ////替换数组即可
            //File.WriteAllLines(saveFileDialog.FileName,contents);
            //0323 CAD保存文件的窗口调用
            //AcadApp.SaveFileDialog saveFileDialog=new AcadApp.SaveFileDialog();
            //Wnd.SaveFileDialog saveFileDialog = new Wnd.SaveFileDialog();
            //saveFileDialog.Title = "保存图形数据";
            //saveFileDialog.Filter = "文本文件(*.txt)|*.txt";
            //string str = db.Filename;//获取文件fullname
            //saveFileDialog.InitialDirectory = Path.GetDirectoryName(db.Filename);
            //saveFileDialog.FileName=Path.GetFileNameWithoutExtension(db.Filename);
            //0323 CAD图元信息统计保存到表格 
            //一个读取地形图高程点属性块转换表格的实例
            //建立一个Struct储存要保存的对象，图块名，层名称，XYZ值，属性块的Key和Value
            //扩展Table类TableEx内建立表
            ////0323 表格新建
            //Table table = new Table();
            ////表行、列数（含通长标题）
            //table.SetSize(10, 5);
            //table.SetColumnWidth(25);
            //table.SetRowHeight(11);
            //table.Position= new Point3d(0, 100, 0);
            //table.Cells[0, 0].TextString = "材料统计表";
            //table.Cells[0, 0].TextHeight = 4;
            //Color color = Color.FromColorIndex(ColorMethod.ByAci, 2);
            //table.Cells[0, 0].ContentColor= color;
            //table.Columns[0].Width = 100;
            //using (Transaction tr = db.TransactionManager.StartTransaction())
            //{
            //    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            //    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            //    btr.AppendEntity(table);
            //    tr.AddNewlyCreatedDBObject(table, true);
            //    tr.Commit();
            //}
            ////例程结束
            //0322 修改属性块的属性      
            //0322 插入属性块
            //Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //PromptEntityResult per = ed.GetEntity("选择块");
            //if (per.Status == PromptStatus.OK)
            //{
            //    using (Transaction tr = db.TransactionManager.StartTransaction())
            //    {
            //        //属性块
            //        BlockReference br = per.ObjectId.GetObject(OpenMode.ForRead) as BlockReference;
            //    }
            //}
            //BlockTool blockTool = new BlockTool();
            //MyBlockTableRecord.AttrBlockId = blockTool.AddBlockTableRecord(db, MyBlockTableRecord.AttrBlockName, MyBlockTableRecord.AttrBlockEnts);
            //blockTool.InsertAttrBlockReference(db, MyBlockTableRecord.AttrBlockId, new Point3d(10, 10, 0), Math.PI / 4, new Scale3d(1, 1, 1));
            //例程结束
            ////0322 属性块定义
            ////BlockTool blockTool = new BlockTool();
            ////MyBlockTableRecord.AttrBlockId = blockTool.AddBlockTableRecord(db,MyBlockTableRecord.AttrBlockName,MyBlockTableRecord.AttrBlockEnts);
            //using (Transaction tr = db.TransactionManager.StartTransaction())
            //{
            //    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            //    foreach (var item in bt)
            //    {
            //        if (item==MyBlockTableRecord.AttrBlockId)
            //        {
            //            BlockTableRecord btr = (BlockTableRecord)item.GetObject(OpenMode.ForRead);
            //            foreach (var item1 in btr)
            //            {
            //                if (item1.GetObject(OpenMode.ForRead) is AttributeDefinition)
            //                {
            //                    AttributeDefinition attr = item1.GetObject(OpenMode.ForRead) as AttributeDefinition;
            //                }
            //            }
            //        }
            //    }
            //    tr.Commit();
            //}
            ////例程结束
            ////0321 插入块操作
            //BlockTool blockTool = new BlockTool();
            ////MyBlockTableRecord.BlockId = blockTool.InsertBlockReference(db, MyBlockTableRecord.BlockId, new Point3d(100, 100, 0));
            //Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            //using (Transaction tr = db.TransactionManager.StartTransaction())
            //{
            //    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            //    // 打开模型空间
            //    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            //    // 查找名为“111”的块定义
            //    BlockTable blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            //    BlockTableRecord blockTableRecord = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
            //    BlockTableRecord blockDef = (BlockTableRecord)tr.GetObject(blockTable["111"], OpenMode.ForRead);
            //    if (blockDef != null)
            //    {
            //        MyBlockTableRecord.BlockId = blockTool.InsertBlockReference(db, blockDef.ObjectId, new Point3d(100, 100, 0), Math.PI / 4, new Scale3d(2.0));
            //        //MyBlockTableRecord.BlockId = blockTool.InsertBlockReference(db, blockDef.ObjectId, new Point3d(100, 100, 0));
            //        //// 创建块引用
            //        //BlockReference blockRef = new BlockReference(new Point3d(10, 10, 0), blockDef.ObjectId);
            //        //// 将块引用添加到模型空间
            //        //btr.AppendEntity(blockRef);
            //        //tr.AddNewlyCreatedDBObject(blockRef, true);
            //        doc.Editor.WriteMessage("块 '111' 已插入到点 (10, 10, 0)。\n");
            //    }
            //    else
            //    {
            //        doc.Editor.WriteMessage("未找到名为 '111' 的块定义。\n");
            //    }
            //    // 提交事务
            //    tr.Commit();
            //}
            ////例程结束
            ////0321 新建块操作
            //BlockTool blockTool = new BlockTool();
            ////MyBlockTableRecord.BlockId= blockTool.AddBlckTableRecord(db, MyBlockTableRecord.BlockName, MyBlockTableRecord.BlockEnts);
            //Circle circle = new Circle(Point3d.Origin, Vector3d.ZAxis, 10);
            //Line line1 = new Line(new Point3d(-5, 0, 0), new Point3d(5, 0, 0));
            //Line line2 = new Line(new Point3d(0, 5, 0), new Point3d(0, -5, 0));
            //List<Entity> entities =new List<Entity>();
            //entities.Add(circle);
            //entities.Add(line1);
            //entities.Add(line2);
            //MyBlockTableRecord.BlockId= blockTool.AddBlckTableRecord(db, "111", entities);
            ////using (Transaction trans = db.TransactionManager.StartTransaction())
            ////{
            ////    BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            ////    trans.Commit();
            ////}
            ////例程结束
            ////0318 线型操作 保存在acad.lin（英制）acadiso.lin（公制）中
            //ObjectId linetypeId = LoadLineType(db, "CENTER");
            //////设置图层线型
            ////using (Transaction trans = db.TransactionManager.StartTransaction())
            ////{
            ////    LayerTable lt = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
            ////    if (lt.Has("0")) 
            ////    {
            ////        LayerTableRecord ltr = lt["0"].GetObject(OpenMode.ForWrite) as LayerTableRecord;
            ////        ltr.LinetypeObjectId = objectId;
            ////    }
            ////    trans.Commit();
            ////}
            //////设置单个元素线型
            //ObjectId cirId = AddCircleToModelSpace(new Point3d(0, 0, 0), 100);
            //using (Transaction trans = db.TransactionManager.StartTransaction())
            //{
            //    Circle circle = cirId.GetObject(OpenMode.ForWrite) as Circle;
            //    circle.Linetype = "CENTER";
            //    //circle.LinetypeId = linetypeId;
            //    circle.ColorIndex = 2;
            //    trans.Commit();
            //}
            ////设置注释线型，没啥用吧..有样式替代的问题 db.SetDimstyleData(dstr);
            ////例程结束
            //0317 注释样式DIMSTY
            //AddDimStyle(db, "111");
            //遍历样式
            //using (Transaction trans = db.TransactionManager.StartTransaction())
            //{
            //    StringBuilder sb = new StringBuilder();
            //    DimStyleTable dst = trans.GetObject(db.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;
            //    foreach (ObjectId item in dst)
            //    {
            //        DimStyleTableRecord dstr = item.GetObject(OpenMode.ForRead) as DimStyleTableRecord;
            //        sb.Append(dstr.Name + "\n");
            //    }
            //    MessageBox.Show(sb.ToString());
            //}
            //0317 文字样式ST
            //AddTextStyle(db, "111");
            //0316 删除图层
            //DeleteLayer(db, "111");
            //强制删除方法可以不考虑这层有无图元，是否当前图层
            //DeleteLayer(db, "111", true);
            //DeleteVoidLayer(db);
            //0316 图层操作 
            //ChangeLayerColor(db, "111", 2);
            //ChangeLayerLock(db, "111");
            //ChangeLayerLineWeight(db, "111", LineWeight.LineWeight013);
            //改图层可能目标被锁、冻、藏，需要判断；还要判断是否已经是当前
            //SetCurrentLayer(db, "111");
            //List<string> lns= GetAllLayerNames(db);
            //StringBuilder sb = new StringBuilder();
            //foreach (string layer in lns) 
            //{ 
            //    sb.Append(layer+"\n");
            //}
            //MessageBox.Show(sb.ToString()+"\n"+lns.Count);
            //新建图层
            //AddLayer(db, "111");
            //例程结束
            ////0315 直径标注
            //DiametricDimension dDim= new DiametricDimension();
            //dDim.ChordPoint= new Point3d(10, 10, 0);
            //dDim.FarChordPoint = new Point3d(50, 10, 0);
            //dDim.LeaderLength = 10;
            //AddEntityToModelSpace(db, dDim);
            ////例程结束
            ////0315 半径标注
            //RadialDimension rDim= new RadialDimension();
            //rDim.Center = new Point3d(10, 10, 0);
            //rDim.ChordPoint= new Point3d(20, 30, 0);
            //rDim.LeaderLength = 10;
            //AddEntityToModelSpace(db, rDim);
            ////例程结束
            ////0315 弧长标注
            ////ArcDimension arcDim = new ArcDimension(new Point3d(10, 10, 0), new Point3d(20, 10, 0), new Point3d(20, 20, 0), new Point3d(25, 10, 0),"<>",db.Dimstyle);
            //Arc arc = new Arc();
            //arc.Center = new Point3d(10, 10, 0);
            //arc.Radius = 50;
            //arc.StartAngle = 0;
            //arc.EndAngle = Math.PI * 0.25;
            //ArcDimension arcDim = new ArcDimension(arc.Center, arc.StartPoint, arc.EndPoint, new Point3d(arc.EndPoint.X + 5, arc.EndPoint.Y, arc.EndPoint.Z), "<>", db.Dimstyle);
            //AddEntityToModelSpace(db, arcDim);
            ////例程结束
            ////0315 角度标注
            //LineAngularDimension2 iDim = new LineAngularDimension2();
            //iDim.XLine1Start = new Point3d(100, 100, 0);
            //iDim.XLine1End = new Point3d(200, 100, 0);
            //iDim.XLine2Start = new Point3d(100, 150, 0);
            //iDim.XLine2End = new Point3d(200, 300, 0);
            //iDim.ArcPoint=new Point3d(200,300,0);
            //AddEntityToModelSpace(db,iDim);
            ////例程结束
            ////0315 对齐标注,跟线性标注很类似？？
            //AlignedDimension aDim = new AlignedDimension();
            //Point3d p1 = new Point3d(100, 100, 0);
            //Point3d p2 = new Point3d(200, 200, 0);
            //aDim.XLine1Point = p1;
            //aDim.XLine2Point = p2;
            //aDim.DimLinePoint= new Point3d(300, 150, 0);
            //AddEntityToModelSpace(db,aDim);
            ////例程结束
            ////0315 线性标注
            ////线性标注
            //Point3d p1 = new Point3d(100, 100, 0);
            //Point3d p2 = new Point3d(200, 200, 0);
            //Line line = new Line(p1, p2);
            //RotatedDimension rotateDim = new RotatedDimension();
            //rotateDim.XLine1Point = p1;
            //rotateDim.XLine2Point = p2;
            //rotateDim.DimLinePoint = new Point3d(300, 150, 0);
            //////垂直与线标注
            ////rotateDim.Rotation = line.Angle;
            ////rotateDim.TextRotation = Math.PI * 0.5;
            ////注释+文字替换
            //rotateDim.DimensionText = "<>米";
            ////箭头大小
            //rotateDim.Dimasz = 10;
            //rotateDim.Rotation = p1.GetVectorTo(p2).GetAngleTo(Vector3d.XAxis);
            //Vector2d v = new Vector2d();
            //AddEntityToModelSpace(db, rotateDim);
            //double num = rotateDim.Measurement;
            //MessageBox.Show(num.ToString());
            ////例程结束
            ////0315 实现自动采样线功能
            //Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            //PromptEntityResult perBase = ed.GetEntity("\n 请选择基准线");
            //if (perBase.Status != PromptStatus.OK) return;
            //PromptEntityResult perCurve = ed.GetEntity("\n 请选择采样线");
            //if (perCurve.Status != PromptStatus.OK) return;
            ////获取两线实体对象
            //Entity baseEntity = GetEntity(db, perBase.ObjectId);
            //Entity curve = GetEntity(db, perCurve.ObjectId);
            //if (baseEntity is Line)
            //{
            //    Line baseLine = baseEntity as Line;
            //    List<Point3d> divPoints = GetBaseLineDivPoints(baseLine, 11);
            //    ////尝试生成节点测试
            //    //List<DBPoint> dBPoints = new List<DBPoint>();
            //    //foreach (var item in divPoints)
            //    //{
            //    //    dBPoints.Add(new DBPoint(item));
            //    //}
            //    //AddEntityReturnList(db, dBPoints.ToArray());
            //    ////在节点生成线测试
            //    //List<Line> divLines = GetDivLines(divPoints.ToArray(), baseLine.Angle + Math.PI * 0.5, 100);
            //    //AddEntityReturnList(db, divLines.ToArray());
            //    List<Line> divLines = GetDivLines(divPoints.ToArray(), baseLine.Angle + Math.PI * 0.5, 100);
            //    divLines = ModifyDivLines(divLines.ToArray(), curve);
            //    //文字处理
            //    List<DBText> dimTexts = GetDivDimTexts(divLines.ToArray(), 15, -10);
            //    AddEntityReturnList(db, divLines.ToArray());
            //    AddEntityReturnList(db, dimTexts.ToArray());
            //}
            //else
            //{
            //    ed.WriteMessage("\n 基准线必须为直线");
            //}
            ////例程结束
            ////PickMText();
            ////0313 多行文字
            //MText mText = new MText();
            //mText.Location = new Point3d(50, 50, 0);
            ////mText.Contents = "Hello,World.\n C# is 12";
            ////双行带横线
            ////mText.Contents = "\\A1;%%P30\\H0.5x;\\SH7/P7;";
            //string t1 = "%%P30";
            //mText.Contents = $"\\A1;{t1}\\H0.5x;\\SH7/{t1};";
            ////双行无横线
            ////mText.Contents = "\\A1;%%P30 \\H0.5x;\\SH7^P7; ";
            ////斜线
            ////mText.Contents = "\\A1;%%P30 \\H0.5x;\\SH7#P7; ";
            //mText.Width = 30;
            //AddEntityToModelSpace(db, mText);
            ////例程结束
            ////0312 文字注释
            ////生成一系列注释线
            //Line[] lines = new Line[10];
            //Point3d[] pt1 = new Point3d[10];
            //Point3d[] pt2 = new Point3d[10];
            //for (int i = 0; i < lines.Length; i++)
            //{
            //    pt1[i] = new Point3d(50, 50 + 20 * i, 0);
            //    pt2[i] = new Point3d(150, 50 + 20 * i, 0);
            //    lines[i] = new Line(pt1[i], pt2[i]);
            //}
            //AddEntityToModelSpace(db, lines);
            ////创建单行文字
            //DBText text0 = new DBText();
            //text0.Position = new Point3d(50, 50, 0);
            //text0.TextString = "Hello,World."+ TextSpecialSymbol.Cube;
            //text0.Height = 100;
            //text0.ColorIndex = 2;
            //text0.Rotation = Math.PI * 0.5;
            ////text0.IsMirroredInX = true;
            ////TextAlign会把字体恢复到默认高度
            //text0.HorizontalMode = TextHorizontalMode.TextAlign;
            ////默认的对齐点是原点
            //text0.AlignmentPoint = text0.Position;
            //text0.VerticalMode = TextVerticalMode.TextBottom;
            //AddEntityToModelSpace(db, text0);
            ////例程结束
            //Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //ed.WriteMessage(TextSpecialSymbol.Cube);
            ////0309 DrawJig仿写移动命令 参考位移 必要性不大 没细看
            //Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //PromptSelectionResult psr = ed.SelectImplied();
            //if (psr.Status != PromptStatus.OK)
            //{
            //    psr = ed.GetSelection();
            //}
            //if (psr.Status != PromptStatus.OK) return;
            //Point3d pointBase = new Point3d(0, 0, 0);
            //PromptPointOptions ppo = new PromptPointOptions("\n 指定基点或[位移(D)]<位移>：");
            //ppo.AllowNone = true;
            ////ppo.BasePoint = pointBase;
            ////ppo.UseBasePoint = true;
            //PromptPointResult ppr = ed.GetPoint(ppo);
            //if (ppr.Status == PromptStatus.Cancel) return;
            //if (ppr.Status == PromptStatus.OK) pointBase = ppr.Value;
            ////获取图形对象
            //List<Entity> entList = new List<Entity>();
            //ObjectId[] ids = psr.Value.GetObjectIds();
            //entList = GetEntity(ids);
            ////复制新对象，传递给MoveJig类的对象
            //Matrix3d mt = Matrix3d.Displacement(new Vector3d(0, 0, 0));
            //List<Entity> entListCopy = CopyEntity(entList, mt);
            ////为什么cancel不成功要另外写copy2？
            //List<Entity> entListCopy2 = CopyEntity(entList, mt);
            ////改原位图形底色
            //LowColorEntity(entList, 211);
            ////交互类要先声明
            //MoveJig moveJig = new MoveJig(entListCopy, pointBase);
            //PromptResult pr = ed.Drag(moveJig);
            ////确认后处理
            //if (pr.Status == PromptStatus.OK)
            //{
            //    List<Entity> ents = moveJig.GetEntity();
            //    AddEntityToModelSpace(db, ents.ToArray());
            //    DeleteEntitys(entList.ToArray());
            //}
            //if (pr.Status == PromptStatus.Cancel)
            //{
            //    AddEntityToModelSpace(db, entListCopy2.ToArray());
            //    DeleteEntitys(entList.ToArray());
            //}
            ////响应中间空格
            //if (pr.Status == PromptStatus.Keyword && pr.StringResult == " ")
            //{
            //    //Vector3d vector = Point3d.Origin.GetVectorTo(pointBase);
            //    //mt = Matrix3d.Displacement(vector);
            //    foreach (var item in entListCopy2)
            //    {
            //        MoveEntity(item, Point3d.Origin, pointBase);
            //    }
            //    AddEntityToModelSpace(db, entListCopy2.ToArray());
            //    DeleteEntitys(entList.ToArray());
            //}
            ////例程结束
            ////0309 先选择后处理的办法 选择集处理
            //Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ////先操作再选择的写法SelectImplied
            ////需要顶部属性加入 CommandFlags.UsePickSet
            //PromptSelectionResult psr = ed.SelectImplied();
            //ed.WriteMessage("OK");
            ////例程结束
            //按点选择,程序有问题不细究了
            //TypedValue[] values = new TypedValue[] { new TypedValue((int)DxfCode.Start, "circle") };
            //SelectionFilter filter = new SelectionFilter(values);
            //PromptSelectionResult psr = ed.GetSelection(filter);
            //List<ObjectId> ids = new List<ObjectId>();
            //if (psr.Status == PromptStatus.OK)
            //{
            //    SelectionSet sSet = psr.Value;
            //    List<Point3d> points = getSelectPoint(sSet);
            //    for (int i = 0; i < points.Count; i++)
            //    {
            //        PromptSelectionResult ss1 = ed.SelectCrossingWindow(points.ElementAt(i), points.ElementAt(i));
            //        ids.AddRange(ss1.Value.GetObjectIds());
            //    }
            //}
            //ChangeColor(ids);
            ////psr = ed.SelectCrossingWindow(pt1, pt2);
            ////过滤器处理
            //TypedValue[] values = new TypedValue[] { new TypedValue((int)DxfCode.Start, "circle") };
            ////DxfCode是内置元素的代码对照表，8是图层名称
            ////可用(setq ent (entsel)) (setq ent_id(car ent))(setq ent_data(entget ent_id))
            ////等同以上(entget(car(entsel)))取对象内部属性dicttionary值，等同revit lookup
            //SelectionFilter filter = new SelectionFilter(values);
            //PromptSelectionResult psr = ed.GetSelection(filter);
            //if (psr.Status == PromptStatus.OK)
            //{
            //    SelectionSet selectionSet = psr.Value;
            //    ChangeColor(selectionSet);
            //}
            ////例程结束
            ////选择所有图形
            ////PromptSelectionResult psr = ed.SelectAll();
            ////ed.SelectLast();//选择最后绘制的实体
            ////ed.SelectImplied();//选择已选的实体
            ////ed.SelectPrevious();//选择上次选中的实体
            ////ed.SelectWindow();//选择2点窗口中的实体
            ////ed.SelectCrossingWindow();//选择2点窗口碰到的实体
            //PromptSelectionResult psr = ed.GetSelection();
            //if (psr.Status == PromptStatus.OK)
            //{
            //    SelectionSet selectionSet = psr.Value;
            //    ChangeColor(selectionSet);
            //}
            ////例程结束
            ////0308 拖拽类EntityJig
            //Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //Point3d center = new Point3d();
            ////double radius = 0;
            //PromptPointResult ppr = GetPoint(ed, "\n 请指定圆心：");
            //if (ppr.Status == PromptStatus.OK)
            //{
            //    center = ppr.Value;
            //}
            //CircleJig jCircle = new CircleJig(center);
            ////PromptPointResult pr = ed.Drag(jCircle) as PromptPointResult;
            ////if (pr.Status == PromptStatus.OK)
            ////{
            ////    Point3d pt = pr.Value;
            ////    AddCircleToModelSpace(center, GetDistanceBetween2Points(center, pt));
            ////}
            ////下面写法等同以上效果
            //PromptResult pr = ed.Drag(jCircle);
            //if (pr.Status == PromptStatus.OK)
            //{
            //    AddEntityToModelSpace(db, jCircle.GetEntity());
            //}
            ////例程结束
            ////取半径，需要ENtityJig替代
            //PromptDistanceOptions pdo = new PromptDistanceOptions("\n 请指定圆上的一个点：");
            //pdo.BasePoint = center;
            //pdo.UseBasePoint = true;
            ////下面这句顺序很重要，否则取半径需两个点？
            //PromptDoubleResult pdr = ed.GetDistance(pdo);
            //if (pdr.Status == PromptStatus.OK)
            //{
            //    radius = pdr.Value;
            //}
            //AddCircleToModelSpace(center, radius);
            //以上Jig替代
            ////0307 仿系统直线,连续绘制，退回和封闭OK
            //Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ////声明直线集合对象，保存过程生成便于回溯
            //List<ObjectId> lineList = new List<ObjectId>();
            ////声明默认起点
            //Point3d pointStart = new Point3d(0, 0, 0);
            //Point3d pointPre = new Point3d(0, 0, 0);
            //PromptPointResult ppr = GetPoint2(ed, "\n input 1st point");
            //if (ppr.Status == PromptStatus.Cancel) return;
            //if (ppr.Status == PromptStatus.None) pointPre = pointStart;
            //if (ppr.Status == PromptStatus.OK)
            //{
            //    pointStart = ppr.Value;
            //    pointPre = pointStart;
            //}
            ////循环退出条件
            //bool isContinue = true;
            //while (isContinue)
            //{
            //    if (lineList.Count >= 2)
            //    {
            //        ppr = GetPoint(ed, "\n 指定下一点或[闭合(C)/放弃（U）]", pointPre, new string[] { "C", "U" });
            //    }
            //    else
            //    {
            //        ppr = GetPoint(ed, "\n 指定下一点或[放弃（U）]", pointPre, new string[] { "U" });
            //    }
            //    Point3d pointNext;
            //    if (ppr.Status == PromptStatus.Cancel) return;
            //    if (ppr.Status == PromptStatus.None) return;
            //    if (ppr.Status == PromptStatus.OK)
            //    {
            //        pointNext = ppr.Value;
            //        lineList.Add(AddLineToModelSpace(db, pointPre, pointNext));
            //        pointPre = pointNext;
            //    }
            //    if (ppr.Status == PromptStatus.Keyword)
            //    {
            //        switch (ppr.StringResult)
            //        {
            //            case "U":
            //                //如果为空要重置条件
            //                if (lineList.Count < 0)
            //                {
            //                    pointStart = new Point3d(0, 0, 0);
            //                    pointPre = new Point3d(0, 0, 0);
            //                    ppr = GetPoint2(ed, "\n input 1st point");
            //                    if (ppr.Status == PromptStatus.Cancel) return;
            //                    if (ppr.Status == PromptStatus.None) pointPre = pointStart;
            //                    if (ppr.Status == PromptStatus.OK)
            //                    {
            //                        pointStart = ppr.Value;
            //                        pointPre = pointStart;
            //                    }
            //                }
            //                else if (lineList.Count > 0)
            //                {
            //                    int count = lineList.Count;
            //                    ObjectId id = lineList.ElementAt(count - 1);
            //                    pointPre = GetLineStartPoint(id);
            //                    lineList.RemoveAt(count - 1);
            //                    DeleteEntiy(id);
            //                }
            //                break;
            //            case "C":
            //                lineList.Add(AddLineToModelSpace(db, pointPre, pointStart));
            //                isContinue = false;
            //                break;
            //            //default:
            //            //    break;
            //        }
            //    }
            //}
            ////例程结束
            ////0307 非模态直接开启机制不允许绘图命令，开模态就没问题
            //Window1 window1 = new Window1(db);
            //window1.ShowDialog();
            ////可能因为线程，测试没通过
            ////0306 用户交互
            ////Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //PromptPointOptions ppo = new PromptPointOptions("input point");
            //ppo.AllowNone = true;
            //PromptPointResult ppr = GetPoint(ppo);
            //Point3d p1 = new Point3d(0, 0, 0);
            //Point3d p2 = new Point3d();
            //if (ppr.Status == PromptStatus.Cancel) return;
            //if (ppr.Status == PromptStatus.OK) p1=ppr.Value;
            //ppo.Message = "2nd Point input.";
            //ppo.BasePoint = p1;
            //ppo.UseBasePoint= true;
            //ppr = GetPoint(ppo);
            //if (ppr.Status == PromptStatus.Cancel) return;
            //if (ppr.Status == PromptStatus.None) return;
            //if (ppr.Status == PromptStatus.OK) p2 = ppr.Value;
            //AddLineToModelSpace(db, p1, p2);
            ////例程结束
            //PromptPointOptions ppo = new PromptPointOptions("input point");
            //
            ////basePoint自动捕捉一条虚线
            //ppo.BasePoint = new Point3d(100, 100, 0);
            //ppo.UseBasePoint = true;
            //PromptPointResult ppr = ed.GetPoint(ppo);
            //打开文件OK
            //ed.GetFileNameForOpen(new PromptOpenFileOptions("input something"));
            //ed.GetFileNameForOpen("input something");
            //PromptStatus.枚举有7种
            ////简易直线
            //PromptPointResult ppr = ed.GetPoint("input point");
            //if (ppr.Status == PromptStatus.OK)
            //{
            //    Point3d point1 = ppr.Value;
            //    ppr = ed.GetPoint("请选择第二个点");
            //    if (ppr.Status == PromptStatus.OK)
            //    {
            //        Point3d point2 = ppr.Value;
            //        AddLineToModelSpace(db, point1, point2);
            //    }
            //}
            //倒角和延申CAD二开不支持？？打断，偏移方法也没有讲，应该原理近似
            ////0304 缩放测试
            //Circle c1 = new Circle(new Point3d(100, 100, 0), Vector3d.ZAxis, 100);
            //AddEntityToModelSpace(db, c1);
            //ScaleEntity(c1.ObjectId, new Point3d(100, 0, 0), 2);
            ////例程结束
            ////0304 镜像测试
            //Circle c1 = new Circle(new Point3d(100, 100, 0), Vector3d.ZAxis, 100);
            //ObjectId objectId = AddEntityToModelSpace(db, c1);
            //Entity ent = MirrorEntity(objectId, new Point3d(100, 0, 0), new Point3d(200, 0, 0),true);
            //AddEntityToModelSpace(db, ent);
            ////例程结束
            ////0303 向量计算复制.注意生成新对象的句子可以写到方法体中避免内部重复
            //Circle c1 = new Circle(new Point3d(100, 100, 0), new Vector3d(0, 0, 1), 100);
            //Point3d p1 = new Point3d(100, 100, 0);
            //Point3d p2 = new Point3d(200, 300, 0);
            ////为什么下句不能用.ObjectId的方式点出来？？
            //Circle c2 = CopyEntity(c1, p1, p2) as Circle;
            //AddEntityToModelSpace(db, c1);
            //AddEntityToModelSpace(db, c2);
            ////例程结束
            ////0303 向量计算移动
            //Circle c1 = new Circle(new Point3d(100, 100, 0), new Vector3d(0, 0, 1), 100);
            //Circle c2 = new Circle(new Point3d(100, 100, 0), new Vector3d(0, 0, 1), 100);
            ////移动相对点
            //Point3d p1 = new Point3d(100, 100, 0);
            //Point3d p2 = new Point3d(200, 300, 0);
            ////获取移动举例
            //Vector3d v = p1.GetVectorTo(p2);
            //AddEntityToModelSpace(db, c1);
            ////变换移动c2
            //Matrix3d mt = Matrix3d.Displacement(v);
            //c2.TransformBy(mt);
            //AddEntityToModelSpace(db, c2);
            ////例程结束
            ////0301 图案填充 hatch直接继承entity
            ////主要步骤 创建对象，设置类型和填充名词等，添加到空间，设置关链边界，最后提交事务
            //ObjectId oid = AddCircleToModelSpace(new Point3d(100, 100, 0), 100);
            //ObjectIdCollection ids = new ObjectIdCollection();
            //ids.Add(oid);
            ////using (Transaction trans = db.TransactionManager.StartTransaction())
            ////{
            ////    Hatch hatch = new Hatch();
            ////    hatch.PatternScale = 10;
            ////    hatch.SetHatchPattern(HatchPatternType.PreDefined, "ANGLE");
            ////    BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
            ////    BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            ////    btr.AppendEntity(hatch);
            ////    trans.AddNewlyCreatedDBObject(hatch, true);
            ////    //下面的还不能提前声明
            ////    hatch.PatternAngle = Math.PI / 4;
            ////    hatch.Associative = true;
            ////    //设置关联图形和方式
            ////    hatch.AppendLoop(HatchLoopTypes.Outermost, ids);
            ////    hatch.EvaluateHatch(true);
            ////    trans.Commit();
            ////}
            //HatchEntity("ANGLE", 5, 45, oid);
            ////例程结束
            ////0301 外部数据导入
            //string fileName = @"D:\users\mhzy\Desktop\test.txt";
            //string[] contents = File.ReadAllLines(fileName);
            //List<List<string>> list = new List<List<string>>();
            //for (int i = 0; i < contents.Length; i++)
            //{
            //    string[] cont = contents[i].Split(new char[] { ' ' });
            //    List<string> subList = new List<string>();
            //    for (int j = 0; j < cont.Length; j++)
            //    {
            //        subList.Add(cont[j]);
            //    }
            //    list.Add(subList);
            //}
            ////开始多段线
            //Polyline polyline = new Polyline();
            //for (int i = 0; i < list.Count; i++)
            //{
            //    double x, y;
            //    bool bx = double.TryParse(list[i][0], out x);
            //    bool by = double.TryParse(list[i][1], out y);
            //    if (bx == false || by == false)
            //    {
            //        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //        ed.WriteMessage("外部文件读取出错");
            //        return;
            //    }
            //    polyline.AddVertexAt(i, new Point2d(x, y), 0, 0, 0 );                
            //}
            //AddEntityToModelSpace(db, polyline);
            ////例程结束
            //0301 生成矩形测试
            //AddRectToModelSpace(db, new Point2d(0, 0), new Point2d(100, 100));
            //生成多边形略，需要参数圆心、半径、边数。判断边数>3后建立点数组，循环定义值
            ////0301 生成多段线测试
            ////轻量（优化）多段线，重量（二维）多段线，三维多段线
            ////HasBulges凸度（圆弧相关）在多段线中，一个顶点与下一个顶点形成圆弧角度1/4的正切值表示0=直线，1=半圆
            ////五个参数，点序号，起始点，凸度，起点和终点宽度
            //Polyline polyline =new Polyline();
            ////Point2d p1 = new Point2d(100, 100);
            ////Point2d p2 = new Point2d(200, 100);
            ////Point2d p3 = new Point2d(200, 200);
            ////polyline.AddVertexAt(0, p1, 0, 0, 0);
            ////polyline.AddVertexAt(1, p2, 0, 0, 0);
            ////polyline.AddVertexAt(2, p3, 0, 0, 0);
            ////polyline.Closed=true;
            ////AddEntityToModelSpace(db, polyline);
            //Point2d p1 = new Point2d(100, 100);
            //Point2d p2 = new Point2d(500, 100);
            //Point2d p3 = new Point2d(500, 200);
            //Point2d p4 = new Point2d(100, 200);
            //polyline.AddVertexAt(0,p1,0,0,0);
            //polyline.AddVertexAt(1,p2,-1,0,0);
            //polyline.AddVertexAt(2,p3,0,0,0);
            //polyline.AddVertexAt(3,p4,1,0,0);
            //polyline.Closed = true;
            //AddEntityToModelSpace(db, polyline);
            ////例程结束
            ////0301 生成圆测试
            //Database db = Application.DocumentManager.MdiActiveDocument.Database;
            ////Circle c1 = new Circle();
            ////c1.Center = new Point3d(50, 50, 0);
            ////c1.Radius = 50;
            ////Circle c2 = new Circle(new Point3d(100, 100, 0), new Vector3d(0, 0, 1),50);
            ////AddEntityToModelSpace(db, c1);
            ////AddEntityToModelSpace(db, c2);
            //AddCircleToModelSpace(db, new Point3d(100, 100, 0), 100);
            ////例程结束
            //////0301 生成圆弧测试
            //Database db = Application.DocumentManager.MdiActiveDocument.Database;
            ////Arc arc1 = new Arc();
            ////arc1.Center = new Point3d(0, 0, 0);
            ////arc1.StartAngle = -Math.PI / 4;
            ////arc1.EndAngle = Math.PI / 4;
            ////arc1.Radius = 100;
            //////double startAngle = 15;
            //////Arc arc2 = new Arc(new Point3d(50, 50, 0), 20, AngleToDegree(startAngle), 90);
            //////Arc arc3 = new Arc(new Point3d(100, 100, 0), new Vector3d(0, 0, 1), 20, Math.PI / 4, Math.PI / 2);          
            //////AddEntityToModelSpace(db, arc2);
            //////AddArcToModelSpace(db, new Point3d(50, 50, 0), 20, 15, 90);
            ////Point3d startPoint = new Point3d(100, 100, 0);
            ////Point3d endPoint = new Point3d(200, 200, 0);
            ////Point3d pointOnArc = new Point3d(150, 100, 0);
            ////CircularArc3d arc3D = new CircularArc3d(startPoint, pointOnArc, endPoint);
            ////double radius = arc3D.Radius;
            ////Point3d center = arc3D.Center;
            //////取起点到圆心连线向量与正向求角度
            ////Vector3d c2S = center.GetVectorTo(startPoint);
            ////Vector3d c2E = center.GetVectorTo(endPoint);
            ////Vector3d VectorX = new Vector3d(1, 0, 0);
            //////要判断向量正副值取起终点
            ////double startAngle = c2S.Y > 0 ? VectorX.GetAngleTo(c2S) : -VectorX.GetAngleTo(c2S);
            ////double endAngle = c2E.Y > 0 ? VectorX.GetAngleTo(c2E) : -VectorX.GetAngleTo(c2E);
            ////Arc arc = new Arc(center, radius, startAngle, endAngle);
            ////AddEntityToModelSpace(db, arc);
            ////圆心，起点，终点角度生成圆弧
            //AddArcToModelSpace(db, new Point3d(50, 50, 0), new Point3d(150, 50, 0), 90);
            ////例程结束
            //0228 生成直线的几种方法.OK
            //Line line1 = new Line();
            //Point3d startPoint = new Point3d(0, 0, 0);
            //Point3d endPoint = new Point3d(200, 200, 0);
            //line1.StartPoint = startPoint;
            //line1.EndPoint = endPoint;
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            //Database db = doc.Database;
            ////AddEntityToModelSpace(db, line1);
            ////等同上句，需要类和方法都转为静态且方法参数前加this,没啥用
            ////db.AddEntityToModelSpace(line1);
            ////Line line2 = new Line(new Point3d(0, 100, 0), new Point3d(100, 100, 0));
            ////List<Entity> lines = new List<Entity>();
            ////lines.Add(line1); 
            ////lines.Add(line2);
            ////AddEntityToModelSpace(db, lines);
            //AddLineToModelSpace(db, new Point3d(200,200,0),200,60);
            ////例程结束
            ////0227 调用MakeArrow方法生成箭头。ok
            //Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //////获取点选并输出
            //var positionOption = new PromptPointOptions("请输入坐标点");
            //var positionResult = ed.GetPoint(positionOption);
            //if (positionResult.Status == PromptStatus.OK)
            //{
            //    ed.WriteMessage($"坐标点：（{positionResult.Value.X}，{positionResult.Value.Y}，{positionResult.Value.Z}）");
            //}
            //////获取角度弧度，点选两点，取与X轴正向夹角
            //var angleOption = new PromptAngleOptions("\n请指定角度：");
            //var angleResult = ed.GetAngle(angleOption);
            //if (angleResult.Status == PromptStatus.OK)
            //{
            //    ed.WriteMessage($"弧度值：{angleResult.Value}");
            //}
            ////调用MakeArrow方法生成箭头
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            //Database db = doc.Database;
            //using (Transaction trans = db.TransactionManager.StartTransaction())
            //{
            //    var arrow = MakeArrow();
            //    //坐标变换
            //    var mtx = Matrix3d.Displacement(positionResult.Value - Point3d.Origin) *
            //        Matrix3d.Rotation(angleResult.Value - Math.PI / 2, Vector3d.ZAxis, Point3d.Origin);
            //    arrow.TransformBy(mtx);
            //    //读块表
            //    BlockTable blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            //    //找模型空间并写入块表
            //    BlockTableRecord modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            //    modelSpace.AppendEntity(arrow);
            //    //事务也要加入
            //    trans.AddNewlyCreatedDBObject(arrow, true);
            //    trans.Commit();
            //}
            ////例程结束
        }
        //[CommandMethod("TestDemo")]
        //public void TestDemo()
        //{
        //    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        //    ed.WriteMessage("Hello,AucoCAD");
        //}
        private Polyline MakeArrow()
        {
            var entity = new Polyline();
            entity.AddVertexAt(0, Point2d.Origin + Vector2d.YAxis * 0.2, 0, 0, 0);
            entity.AddVertexAt(0, Point2d.Origin + Vector2d.XAxis * 0.2, 0, 0, 0);
            entity.AddVertexAt(0, Point2d.Origin + Vector2d.XAxis * -0.2, 0, 0, 0);
            entity.Closed = true;
            return entity;
        }
    }
}
