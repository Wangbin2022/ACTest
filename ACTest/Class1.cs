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
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
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
                jppo.UserInputControls = UserInputControls.Accept3dCoordinates;
                //取动态坐标值
                PromptPointResult ppr = prompts.AcquirePoint(jppo);
                Point3d curPoint = ppr.Value;
                //矩阵变化
                if (curPoint != ppr.Value)
                {
                    Vector3d vector = jPointPre.GetVectorTo(curPoint);
                    jMt = Matrix3d.Displacement(vector);
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
                return SamplerStatus.NoChange;
            }
        }
        //测试通用方法
        [CommandMethod("Cmd1", CommandFlags.UsePickSet)]
        public void Cmd1()
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;


            //0309 DrawJig仿写移动命令 参考位移 必要性不大 没细看
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionResult psr = ed.SelectImplied();
            if (psr.Status != PromptStatus.OK)
            {
                psr = ed.GetSelection();
            }
            Point3d pointBase = new Point3d(0, 0, 0);
            PromptPointOptions ppo = new PromptPointOptions("\n 指定基点或[位移(D)]<位移>：");
            ppo.AllowNone = true;
            ppo.BasePoint = pointBase;
            ppo.UseBasePoint = true;
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status == PromptStatus.Cancel) return;
            if (ppr.Status == PromptStatus.OK) pointBase = ppr.Value;
            List<Entity> entList = new List<Entity>();
            ObjectId[] ids = psr.Value.GetObjectIds();
            entList = GetEntity(ids);
            //改原位图形底色
            LowColorEntity(entList, 211);
            //交互类要先声明
            MoveJig moveJig = new MoveJig(entList, pointBase);
            PromptResult pr = ed.Drag(moveJig);


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
