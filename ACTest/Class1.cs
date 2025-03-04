using ACTest;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
        public double GetDistanceBetween2Points(Point3d p1, Point3d p2)
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
        //测试通用方法
        [CommandMethod("Cmd1")]
        public void Cmd1()
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;



            ////0303 向量计算复制
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
