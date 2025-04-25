using ACTest;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

//声明命令存储位置，加快执行查找速度
[assembly: CommandClass(typeof(DgnPurge))]
namespace ACTest
{
    public class DgnPurge
    {
        public struct ads_name
        {
            public IntPtr a;
            public IntPtr b;
        }
        private const string dgnLsDefName = "DGNLSDEF";
        private const string dgnLsDictName = "ACAD_DGNLINESTYLECOMP";
        [DllImport("acdb19.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int acdbHandEnt(string h, ref ads_name n);

        [CommandMethod("Dgnpurge", CommandFlags.UsePickSet)]
        public void PurgeDgnLinetypes()
        {
            Document mdiActiveDocument = Application.DocumentManager.MdiActiveDocument;
            Database database = mdiActiveDocument.Database;
            Editor editor = mdiActiveDocument.Editor;
            using (OpenCloseTransaction tr = ((Autodesk.AutoCAD.DatabaseServices.TransactionManager)mdiActiveDocument.TransactionManager).StartOpenCloseTransaction())
            {
                ObjectIdCollection ids = CollectComplexLinetypeIds(database, (Transaction)tr);
                int count1 = ids.Count;
                ObjectIdCollection tokeep = PurgeLinetypesReferencedNotByAnonBlocks(database, (Transaction)tr, ids);
                ObjectIdCollection nodtoremove = CollectStrokeIds(database, (Transaction)tr);
                int count2 = nodtoremove.Count;
                PurgeStrokesReferencedByLinetypes((Transaction)tr, tokeep, nodtoremove);
                int num1 = 0;
                foreach (ObjectId objectId in nodtoremove)
                {
                    try
                    {
                        DBObject dbObject = ((Transaction)tr).GetObject(objectId, (OpenMode)1);
                        dbObject.Erase();
                        if (((RXObject)dbObject).GetRXClass().Name.Equals("AcDbLSSymbolComponent"))
                            EraseReferencedAnonBlocks((Transaction)tr, dbObject);
                        ++num1;
                    }
                    catch (System.Exception ex)
                    {
              //          editor.WriteMessage("\nUnable to erase stroke ({0}): {1}", new object[2]
              //          {
              //(object) ((ObjectId) ref objectId).ObjectClass.Name,
              //(object) ex.Message
              //          });
                    }
                }
                int num2 = 0;
                foreach (ObjectId objectId in ids)
                {
                    try
                    {
                        ((Transaction)tr).GetObject(objectId, (OpenMode)1).Erase();
                        ++num2;
                    }
                    catch (System.Exception ex)
                    {
              //          editor.WriteMessage("\nUnable to erase linetype ({0}): {1}", new object[2]
              //          {
              //(object) ((ObjectId) ref objectId).ObjectClass.Name,
              //(object) ex.Message
              //          });
                    }
                }
                DBDictionary dbDictionary1 = (DBDictionary)((Transaction)tr).GetObject(database.NamedObjectsDictionaryId, (OpenMode)0);
                editor.WriteMessage("\nPurged {0} unreferenced complex linetype records (of {1}).", new object[2]
                {
          (object) num2,
          (object) count1
                });
                editor.WriteMessage("\nPurged {0} unreferenced strokes (of {1}).", new object[2]
                {
          (object) num1,
          (object) count2
                });
                if (dbDictionary1.Contains("ACAD_DGNLINESTYLECOMP"))
                {
                    DBDictionary dbDictionary2 = (DBDictionary)((Transaction)tr).GetObject((ObjectId)dbDictionary1["ACAD_DGNLINESTYLECOMP"], (OpenMode)0);
                    if (dbDictionary2.Count == 0)
                    {
                        ((DBObject)dbDictionary2).UpgradeOpen();
                        ((DBObject)dbDictionary2).Erase();
                        editor.WriteMessage("\nRemoved the empty DGN linetype stroke dictionary.");
                    }
                }
              ((Transaction)tr).Commit();
            }
            //kimi改后
            //Document mdiActiveDocument = Application.DocumentManager.MdiActiveDocument;
            //Database database = mdiActiveDocument.Database;
            //Editor editor = mdiActiveDocument.Editor;
            //using (OpenCloseTransaction tr = (OpenCloseTransaction)mdiActiveDocument.TransactionManager.StartOpenCloseTransaction())
            //{
            //    try
            //    {
            //        ObjectIdCollection complexLinetypeIds = CollectComplexLinetypeIds(database, tr);
            //        int complexLinetypeCount = complexLinetypeIds.Count;
            //        ObjectIdCollection toKeep = PurgeLinetypesReferencedNotByAnonBlocks(database, tr, complexLinetypeIds);
            //        ObjectIdCollection strokeIds = CollectStrokeIds(database, tr);
            //        int strokeCount = strokeIds.Count;
            //        PurgeStrokesReferencedByLinetypes(tr, toKeep, strokeIds);
            //        int purgedStrokes = 0;
            //        foreach (ObjectId strokeId in strokeIds)
            //        {
            //            try
            //            {
            //                DBObject obj = tr.GetObject(strokeId, OpenMode.ForWrite);
            //                obj.Erase();
            //                if (((RXObject)obj).GetRXClass().Name.Equals("AcDbLSSymbolComponent"))
            //                {
            //                    EraseReferencedAnonBlocks(tr, obj);
            //                }
            //                purgedStrokes++;
            //            }
            //            catch (System.Exception ex)
            //            {
            //                //editor.WriteMessage("\nUnable to erase stroke ({0}): {1}", obj.ObjectClass.Name, ex.Message);
            //            }
            //        }
            //        int purgedLinetypes = 0;
            //        foreach (ObjectId linetypeId in complexLinetypeIds)
            //        {
            //            try
            //            {
            //                DBObject obj = tr.GetObject(linetypeId, OpenMode.ForWrite);
            //                obj.Erase();
            //                purgedLinetypes++;
            //            }
            //            catch (System.Exception ex)
            //            {
            //                //editor.WriteMessage("\nUnable to erase linetype ({0}): {1}", obj.ObjectClass.Name, ex.Message);
            //            }
            //        }
            //        DBDictionary namedObjectsDict = (DBDictionary)tr.GetObject(database.NamedObjectsDictionaryId, OpenMode.ForRead);
            //        if (namedObjectsDict.Contains("ACAD_DGNLINESTYLECOMP"))
            //        {
            //            DBDictionary dgnLineStyleCompDict = (DBDictionary)tr.GetObject((ObjectId)namedObjectsDict["ACAD_DGNLINESTYLECOMP"], OpenMode.ForRead);
            //            if (dgnLineStyleCompDict.Count == 0)
            //            {
            //                dgnLineStyleCompDict.UpgradeOpen();
            //                dgnLineStyleCompDict.Erase();
            //                editor.WriteMessage("\nRemoved the empty DGN linetype stroke dictionary.");
            //            }
            //        }
            //        editor.WriteMessage("\nPurged {0} unreferenced complex linetype records (of {1}).", purgedLinetypes, complexLinetypeCount);
            //        editor.WriteMessage("\nPurged {0} unreferenced strokes (of {1}).", purgedStrokes, strokeCount);
            //        tr.Commit();
            //    }
            //    catch (System.Exception ex)
            //    {
            //        editor.WriteMessage("\nAn error occurred during the purge operation: {0}", ex.Message);
            //    }
            //}
            //kimi改前bak
            //Document mdiActiveDocument = Application.DocumentManager.MdiActiveDocument;
            //Database database = mdiActiveDocument.Database;
            //Editor editor = mdiActiveDocument.Editor;
            //OpenCloseTransaction val = ((Autodesk.AutoCAD.ApplicationServices.TransactionManager)mdiActiveDocument.TransactionManager).StartOpenCloseTransaction();
            //try
            //{
            //    ObjectIdCollection val2 = CollectComplexLinetypeIds(database, (Transaction)(object)val);
            //    int count = val2.Count;
            //    ObjectIdCollection tokeep = PurgeLinetypesReferencedNotByAnonBlocks(database, (Transaction)(object)val, val2);
            //    ObjectIdCollection val3 = CollectStrokeIds(database, (Transaction)(object)val);
            //    int count2 = val3.Count;
            //    PurgeStrokesReferencedByLinetypes((Transaction)(object)val, tokeep, val3);
            //    int num = 0;
            //    foreach (ObjectId item in val3)
            //    {
            //        ObjectId val4 = item;
            //        try
            //        {
            //            DBObject @object = ((Transaction)val).GetObject(val4, (OpenMode)1);
            //            @object.Erase();
            //            if (((RXObject)@object).GetRXClass().Name.Equals("AcDbLSSymbolComponent"))
            //            {
            //                EraseReferencedAnonBlocks((Transaction)(object)val, @object);
            //            }
            //            num++;
            //        }
            //        catch (System.Exception ex)
            //        {
            //            //editor.WriteMessage("\nUnable to erase stroke ({0}): {1}", new object[2]
            //            //{
            //            //((ObjectId)(ref val4)).ObjectClass.Name,
            //            //ex.Message
            //            //});
            //        }
            //    }
            //    int num2 = 0;
            //    foreach (ObjectId item2 in val2)
            //    {
            //        ObjectId val5 = item2;
            //        try
            //        {
            //            DBObject object2 = ((Transaction)val).GetObject(val5, (OpenMode)1);
            //            object2.Erase();
            //            num2++;
            //        }
            //        catch (System.Exception ex2)
            //        {
            //            //editor.WriteMessage("\nUnable to erase linetype ({0}): {1}", new object[2]
            //            //{
            //            //((ObjectId)(ref val5)).ObjectClass.Name,
            //            //ex2.Message
            //            //});
            //        }
            //    }
            //    DBDictionary val6 = (DBDictionary)((Transaction)val).GetObject(database.NamedObjectsDictionaryId, (OpenMode)0);
            //    editor.WriteMessage("\nPurged {0} unreferenced complex linetype records (of {1}).", new object[2] { num2, count });
            //    editor.WriteMessage("\nPurged {0} unreferenced strokes (of {1}).", new object[2] { num, count2 });
            //    if (val6.Contains("ACAD_DGNLINESTYLECOMP"))
            //    {
            //        DBDictionary val7 = (DBDictionary)((Transaction)val).GetObject((ObjectId)val6["ACAD_DGNLINESTYLECOMP"], (OpenMode)0);
            //        if (val7.Count == 0)
            //        {
            //            ((DBObject)val7).UpgradeOpen();
            //            ((DBObject)val7).Erase();
            //            editor.WriteMessage("\nRemoved the empty DGN linetype stroke dictionary.");
            //        }
            //    }
            //((Transaction)val).Commit();
            //}
            //finally
            //{
            //    ((IDisposable)val)?.Dispose();
            //}
        }
        private static ObjectIdCollection CollectComplexLinetypeIds(Database db, Transaction tr)
        {
            //从数据库中收集具有复杂线型定义（DGNLSDEF）的线型（Linetype）对象的 ObjectId
            ObjectIdCollection objectIdCollection = new ObjectIdCollection();
            foreach (ObjectId objectId in (SymbolTable)tr.GetObject(db.LinetypeTableId, (OpenMode)0))
            {
                DBObject dbObject = tr.GetObject(objectId, (OpenMode)0);
                if (dbObject.ExtensionDictionary != ObjectId.Null && ((DBDictionary)tr.GetObject(dbObject.ExtensionDictionary, (OpenMode)0)).Contains("DGNLSDEF"))
                    objectIdCollection.Add(objectId);
            }
            return objectIdCollection;
            //kimi改后
            //ObjectIdCollection val = new ObjectIdCollection();
            //LinetypeTable linetypeTable = (LinetypeTable)tr.GetObject(db.LinetypeTableId, OpenMode.ForRead);
            //using (SymbolTableEnumerator enumerator = linetypeTable.GetEnumerator())
            //{
            //    while (enumerator.MoveNext())
            //    {
            //        ObjectId current = enumerator.Current;
            //        DBObject obj = tr.GetObject(current, OpenMode.ForRead);

            //        if (obj.ExtensionDictionary != ObjectId.Null)
            //        {
            //            DBDictionary extDict = (DBDictionary)tr.GetObject(obj.ExtensionDictionary, OpenMode.ForRead);

            //            if (extDict.Contains("DGNLSDEF"))
            //            {
            //                val.Add(current);
            //            }
            //        }
            //    }
            //}
            //return val;
            //kimi改前bak
            //ObjectIdCollection val = new ObjectIdCollection();
            //LinetypeTable val2 = (LinetypeTable)tr.GetObject(db.LinetypeTableId, (OpenMode)0);
            //SymbolTableEnumerator enumerator = ((SymbolTable)val2).GetEnumerator();
            //try
            //{
            //    while (enumerator.MoveNext())
            //    {
            //        ObjectId current = enumerator.Current;
            //        DBObject @object = tr.GetObject(current, (OpenMode)0);
            //        if (@object.ExtensionDictionary != ObjectId.Null)
            //        {
            //            DBDictionary val3 = (DBDictionary)tr.GetObject(@object.ExtensionDictionary, (OpenMode)0);
            //            if (val3.Contains("DGNLSDEF"))
            //            {
            //                val.Add(current);
            //            }
            //        }
            //    }
            //    return val;
            //}
            //finally
            //{
            //    ((IDisposable)enumerator)?.Dispose();
            //}
        }
        private static ObjectIdCollection CollectStrokeIds(Database db, Transaction tr)
        {
            //从数据库中收集与 ACAD_DGNLINESTYLECOMP 相关的 ObjectId
            ObjectIdCollection objectIdCollection = new ObjectIdCollection();
            DBDictionary dbDictionary = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, (OpenMode)0);
            if (dbDictionary.Contains("ACAD_DGNLINESTYLECOMP"))
            {
                foreach (DBDictionaryEntry dbDictionaryEntry in (DBDictionary)tr.GetObject((ObjectId)dbDictionary["ACAD_DGNLINESTYLECOMP"], (OpenMode)0))
                    objectIdCollection.Add(((DBDictionaryEntry) dbDictionaryEntry).Value);
            }
            return objectIdCollection;
            //kimi改后
            //ObjectIdCollection val = new ObjectIdCollection();
            //DBDictionary namedObjectsDict = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
            //if (namedObjectsDict.Contains("ACAD_DGNLINESTYLECOMP"))
            //{
            //    DBDictionary lineStyleDict = (DBDictionary)tr.GetObject((ObjectId)namedObjectsDict["ACAD_DGNLINESTYLECOMP"], OpenMode.ForRead);
            //    using (DbDictionaryEnumerator enumerator = lineStyleDict.GetEnumerator())
            //    {
            //        while (enumerator.MoveNext())
            //        {
            //            val.Add(enumerator.Current.Value);
            //        }
            //    }
            //}
            //return val;
            //ObjectIdCollection objectIdCollection = new ObjectIdCollection();
            //DBDictionary dbDictionary = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, (OpenMode)0);
            //if (dbDictionary.Contains("ACAD_DGNLINESTYLECOMP"))
            //{
            //    foreach (DBDictionaryEntry dbDictionaryEntry in (DBDictionary)tr.GetObject((ObjectId)dbDictionary["ACAD_DGNLINESTYLECOMP"], (OpenMode)0))
            //        objectIdCollection.Add(((DBDictionaryEntry)ref dbDictionaryEntry).Value);
            //}
            //return objectIdCollection;
            //kimi改前bak
            //ObjectIdCollection val = new ObjectIdCollection();
            //DBDictionary val2 = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, (OpenMode)0);
            //if (val2.Contains("ACAD_DGNLINESTYLECOMP"))
            //{
            //    DBDictionary val3 = (DBDictionary)tr.GetObject((ObjectId)val2["ACAD_DGNLINESTYLECOMP"], (OpenMode)0);
            //    DbDictionaryEnumerator enumerator = val3.GetEnumerator();
            //    try
            //    {
            //        while (enumerator.MoveNext())
            //        {
            //            DBDictionaryEntry current = enumerator.Current;
            //            //val.Add(((DBDictionaryEntry)(ref current)).Value);
            //            val.Add(((DBDictionaryEntry)(current)).Value);
            //        }
            //    }
            //    finally
            //    {
            //        ((IDisposable)enumerator)?.Dispose();
            //    }
            //}
            //return val;
        }
        private static ObjectIdCollection PurgeLinetypesReferencedNotByAnonBlocks(Database db, Transaction tr, ObjectIdCollection ids)
        {
            //清理数据库中未被匿名块（Anonymous Blocks）引用的线型（Linetype）对象
            ObjectIdCollection objectIdCollection = new ObjectIdCollection();
            Handle handseed = db.Handseed;
            long num = ((Handle)handseed).Value;
            DgnPurge.ads_name n = new DgnPurge.ads_name();
            for (long index = 1; index < num; ++index)
            {
                if (DgnPurge.acdbHandEnt(Convert.ToString(index, 16), ref n) == 5100)
                {
                    ObjectId objectId;
                    // ISSUE: explicit constructor call
                    //((ObjectId)ref objectId).\u002Ector(n.a);
                    objectId = new ObjectId(n.a);
                    Entity entity = tr.GetObject(objectId, (OpenMode)0, true) as Entity;
                    if (entity != null && !entity.IsErased && ids.Contains(entity.LinetypeId))
                    {
                        BlockTableRecord blockTableRecord = (BlockTableRecord)tr.GetObject(((DBObject)entity).OwnerId, (OpenMode)0);
                        if (!((SymbolTableRecord)blockTableRecord).Name.StartsWith("*") || ((SymbolTableRecord)blockTableRecord).Name.ToUpper() == BlockTableRecord.ModelSpace || ((SymbolTableRecord)blockTableRecord).Name.ToUpper().StartsWith(BlockTableRecord.PaperSpace))
                        {
                            ids.Remove(entity.LinetypeId);
                            objectIdCollection.Add(entity.LinetypeId);
                        }
                    }
                }
            }
            return objectIdCollection;
            //kimi改后
            //ObjectIdCollection val = new ObjectIdCollection();
            //Handle handseed = db.Handseed;
            //long value = handseed.Value;
            //ObjectId val2;
            //ads_name n = default(ads_name);
            //// 用于存储需要移除的 ObjectId
            //ObjectIdCollection toRemove = new ObjectIdCollection();
            //for (long num = 1L; num < value; num++)
            //{
            //    string h = Convert.ToString(num, 16);
            //    int num2 = acdbHandEnt(h, ref n);
            //    if (num2 != 5100)
            //    {
            //        continue;
            //    }
            //    val2 = new ObjectId(n.a);
            //    DBObject obj = tr.GetObject(val2, OpenMode.ForRead, true);
            //    Entity val3 = obj as Entity;
            //    if (val3 != null && !val3.IsErased && ids.Contains(val3.LinetypeId))
            //    {
            //        BlockTableRecord val4 = (BlockTableRecord)tr.GetObject(val3.OwnerId, OpenMode.ForRead);
            //        if (!((SymbolTableRecord)val4).Name.StartsWith("*") || ((SymbolTableRecord)val4).Name.ToUpper() == BlockTableRecord.ModelSpace || ((SymbolTableRecord)val4).Name.ToUpper().StartsWith(BlockTableRecord.PaperSpace.ToUpper()))
            //        {
            //            toRemove.Add(val3.LinetypeId);
            //            val.Add(val3.LinetypeId);
            //        }
            //    }
            //}
            //// 从 ids 中移除需要移除的 ObjectId
            //foreach (ObjectId id in toRemove)
            //{
            //    ids.Remove(id);
            //}
            //return val;
            //kimi改前bak
            //ObjectIdCollection val = new ObjectIdCollection();
            //Handle handseed = db.Handseed;
            ////long value = ((Handle)(ref handseed)).Value;
            //long value = ((Handle)(handseed)).Value;
            //ads_name n = default(ads_name);
            //ObjectId val2 = default(ObjectId);
            //for (long num = 1L; num < value; num++)
            //{
            //    string h = Convert.ToString(num, 16);
            //    int num2 = acdbHandEnt(h, ref n);
            //    if (num2 != 5100)
            //    {
            //        continue;
            //    }
            //    //((ObjectId)(ref val2))..ctor(n.a);
            //    DBObject @object = tr.GetObject(val2, (OpenMode)0, true);
            //    Entity val3 = (Entity)(object)((@object is Entity) ? @object : null);
            //    if ((DisposableWrapper)(object)val3 != (DisposableWrapper)null && !((DBObject)val3).IsErased && ids.Contains(val3.LinetypeId))
            //    {
            //        BlockTableRecord val4 = (BlockTableRecord)tr.GetObject(((DBObject)val3).OwnerId, (OpenMode)0);
            //        if (!((SymbolTableRecord)val4).Name.StartsWith("*") || ((SymbolTableRecord)val4).Name.ToUpper() == BlockTableRecord.ModelSpace || ((SymbolTableRecord)val4).Name.ToUpper().StartsWith(BlockTableRecord.PaperSpace))
            //        {
            //            ids.Remove(val3.LinetypeId);
            //            val.Add(val3.LinetypeId);
            //        }
            //    }
            //}
            //return val;
        }
        private static void PurgeStrokesReferencedByLinetypes(Transaction tr, ObjectIdCollection tokeep, ObjectIdCollection nodtoremove)
        {
            //清理与线型（Linetype）相关的笔画（Stroke）引用。 
            foreach (ObjectId id in tokeep)
            {
                DgnPurge.PurgeStrokesReferencedByObject(tr, nodtoremove, id);
            }
            //kimi改后
            //if (tr == null || tokeep == null || nodtoremove == null)
            //{
            //    throw new ArgumentNullException("One or more input parameters are null.");
            //}

            //foreach (ObjectId item in tokeep)
            //{
            //    try
            //    {
            //        PurgeStrokesReferencedByObject(tr, nodtoremove, item);
            //    }
            //    catch (System.Exception ex)
            //    {
            //        // 处理异常，例如记录日志
            //        //Console.WriteLine($"Error processing ObjectId {item}: {ex.Message}");
            //    }
            //}
            //kimi改前bak
            //foreach (ObjectId item in tokeep)
            //{
            //    PurgeStrokesReferencedByObject(tr, nodtoremove, item);
            //}
        }
        private static void PurgeStrokesReferencedByObject(Transaction tr, ObjectIdCollection nodIds, ObjectId id)
        {
            //递归地清理与指定对象相关的笔画（Stroke）引用。
            DBObject dbObject1 = tr.GetObject(id, (OpenMode)0);
            if (dbObject1.ExtensionDictionary != ObjectId.Null)
            {
                DBDictionary dbDictionary = (DBDictionary)tr.GetObject(dbObject1.ExtensionDictionary, (OpenMode)0);
                if (!dbDictionary.Contains("DGNLSDEF"))
                    return;
                DBObject dbObject2 = tr.GetObject(dbDictionary.GetAt("DGNLSDEF"), (OpenMode)0);
                ReferenceFiler referenceFiler = new ReferenceFiler();
                dbObject2.DwgOut((DwgFiler)referenceFiler);
                foreach (ObjectId hardPointerId in referenceFiler.HardPointerIds)
                {
                    if (nodIds.Contains(hardPointerId))
                        nodIds.Remove(hardPointerId);
                    DgnPurge.PurgeStrokesReferencedByObject(tr, nodIds, hardPointerId);
                }
            }
            else
            {
                if (!((RXObject)dbObject1).GetRXClass().Name.Equals("AcDbLSCompoundComponent") && !((RXObject)dbObject1).GetRXClass().Name.Equals("AcDbLSPointComponent"))
                    return;
                ReferenceFiler referenceFiler = new ReferenceFiler();
                dbObject1.DwgOut((DwgFiler)referenceFiler);
                foreach (ObjectId hardPointerId in referenceFiler.HardPointerIds)
                {
                    if (nodIds.Contains(hardPointerId))
                        nodIds.Remove(hardPointerId);
                    DgnPurge.PurgeStrokesReferencedByObject(tr, nodIds, hardPointerId);
                }
            }
            //kimi改后
            //try
            //{
            //    DBObject obj = tr.GetObject(id, OpenMode.ForRead);

            //    if (obj.ExtensionDictionary != ObjectId.Null)
            //    {
            //        DBDictionary extDict = (DBDictionary)tr.GetObject(obj.ExtensionDictionary, OpenMode.ForRead);

            //        if (extDict.Contains("DGNLSDEF"))
            //        {
            //            DBObject dgnlsdefObj = tr.GetObject(extDict.GetAt("DGNLSDEF"), OpenMode.ForRead);

            //            ReferenceFiler referenceFiler = new ReferenceFiler();
            //            dgnlsdefObj.DwgOut((DwgFiler)(object)referenceFiler);

            //            foreach (ObjectId hardPointerId in referenceFiler.HardPointerIds)
            //            {
            //                if (nodIds.Contains(hardPointerId))
            //                {
            //                    nodIds.Remove(hardPointerId);
            //                }
            //                PurgeStrokesReferencedByObject(tr, nodIds, hardPointerId);
            //            }
            //        }
            //    }
            //    if (!((RXObject)obj).GetRXClass().Name.Equals("AcDbLSCompoundComponent") && !((RXObject)obj).GetRXClass().Name.Equals("AcDbLSPointComponent"))
            //    {
            //        return;
            //    }
            //    ReferenceFiler referenceFiler2 = new ReferenceFiler();
            //    obj.DwgOut((DwgFiler)(object)referenceFiler2);
            //    foreach (ObjectId hardPointerId2 in referenceFiler2.HardPointerIds)
            //    {
            //        if (nodIds.Contains(hardPointerId2))
            //        {
            //            nodIds.Remove(hardPointerId2);
            //        }
            //        PurgeStrokesReferencedByObject(tr, nodIds, hardPointerId2);
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    // 处理异常，例如记录日志
            //    //Console.WriteLine($"Error processing ObjectId {id}: {ex.Message}");
            //}
            //kimi改前bak
            //DBObject @object = tr.GetObject(id, (OpenMode)0);
            //if (@object.ExtensionDictionary != ObjectId.Null)
            //{
            //    DBDictionary val = (DBDictionary)tr.GetObject(@object.ExtensionDictionary, (OpenMode)0);
            //    if (!val.Contains("DGNLSDEF"))
            //    {
            //        return;
            //    }
            //    DBObject object2 = tr.GetObject(val.GetAt("DGNLSDEF"), (OpenMode)0);
            //    ReferenceFiler referenceFiler = new ReferenceFiler();
            //    object2.DwgOut((DwgFiler)(object)referenceFiler);
            //    {
            //        foreach (ObjectId hardPointerId in referenceFiler.HardPointerIds)
            //        {
            //            if (nodIds.Contains(hardPointerId))
            //            {
            //                nodIds.Remove(hardPointerId);
            //            }
            //            PurgeStrokesReferencedByObject(tr, nodIds, hardPointerId);
            //        }
            //        return;
            //    }
            //}
            //if (!((RXObject)@object).GetRXClass().Name.Equals("AcDbLSCompoundComponent") && !((RXObject)@object).GetRXClass().Name.Equals("AcDbLSPointComponent"))
            //{
            //    return;
            //}
            //ReferenceFiler referenceFiler2 = new ReferenceFiler();
            //@object.DwgOut((DwgFiler)(object)referenceFiler2);
            //foreach (ObjectId hardPointerId2 in referenceFiler2.HardPointerIds)
            //{
            //    if (nodIds.Contains(hardPointerId2))
            //    {
            //        nodIds.Remove(hardPointerId2);
            //    }
            //    PurgeStrokesReferencedByObject(tr, nodIds, hardPointerId2);
            //}
        }
        private static void EraseReferencedAnonBlocks(Transaction tr, DBObject obj)
        {
            //删除（Erase）数据库中被指定对象引用的匿名块（Anonymous Blocks）
            ReferenceFiler referenceFiler = new ReferenceFiler();
            obj.DwgOut((DwgFiler)referenceFiler);
            foreach (ObjectId hardPointerId in referenceFiler.HardPointerIds)
            {
                BlockTableRecord blockTableRecord = tr.GetObject(hardPointerId, (OpenMode)0) as BlockTableRecord;
                if (blockTableRecord != null && blockTableRecord.IsAnonymous)
                {
                    ((DBObject)blockTableRecord).UpgradeOpen();
                    ((DBObject)blockTableRecord).Erase();
                }
            }
            //kimi改后
            //ReferenceFiler referenceFiler = new ReferenceFiler();
            //obj.DwgOut((DwgFiler)(object)referenceFiler);
            //foreach (ObjectId hardPointerId in referenceFiler.HardPointerIds)
            //{
            //    try
            //    {
            //        DBObject objectRef = tr.GetObject(hardPointerId, OpenMode.ForRead);
            //        BlockTableRecord blockTableRecord = objectRef as BlockTableRecord;

            //        if (blockTableRecord != null && blockTableRecord.IsAnonymous)
            //        {
            //            blockTableRecord.UpgradeOpen();
            //            blockTableRecord.Erase();
            //        }
            //    }
            //    catch (System.Exception ex)
            //    {
            //        // 处理异常，例如记录日志
            //        //Console.WriteLine($"Error processing ObjectId {hardPointerId}: {ex.Message}");
            //    }
            //}
            //kimi改前bak
            //ReferenceFiler referenceFiler = new ReferenceFiler();
            //obj.DwgOut((DwgFiler)(object)referenceFiler);
            //foreach (ObjectId hardPointerId in referenceFiler.HardPointerIds)
            //{
            //    DBObject @object = tr.GetObject(hardPointerId, (OpenMode)0);
            //    BlockTableRecord val2 = (BlockTableRecord)(object)((@object is BlockTableRecord) ? @object : null);
            //    if ((DisposableWrapper)(object)val2 != (DisposableWrapper)null && val2.IsAnonymous)
            //    {
            //        ((DBObject)val2).UpgradeOpen();
            //        ((DBObject)val2).Erase();
            //    }
            //}
        }
        //public void Dgnpurge()
        //{
        //    //Database db = Application.DocumentManager.MdiActiveDocument.Database;
        //    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        //    ed.WriteMessage("New202503");

        //}
        public class ReferenceFiler : DwgFiler
        {
            public ObjectIdCollection HardPointerIds;
            public ObjectIdCollection SoftPointerIds;
            public ObjectIdCollection HardOwnershipIds;
            public ObjectIdCollection SoftOwnershipIds;
            public override ErrorStatus FilerStatus
            {
                get
                {
                    return (ErrorStatus)0;
                }
                set
                {
                }
            }
            public override FilerType FilerType => (FilerType)7;
            public override long Position => 0L;
            public ReferenceFiler()
            {
                HardPointerIds = new ObjectIdCollection();
                SoftPointerIds = new ObjectIdCollection();
                HardOwnershipIds = new ObjectIdCollection();
                SoftOwnershipIds = new ObjectIdCollection();
            }
            public override IntPtr ReadAddress()
            {
                return default(IntPtr);
            }
            public override byte[] ReadBinaryChunk()
            {
                return null;
            }
            public override bool ReadBoolean()
            {
                return true;
            }
            public override byte ReadByte()
            {
                return 0;
            }
            public override void ReadBytes(byte[] value)
            {
            }
            public override double ReadDouble()
            {
                return 0.0;
            }
            public override Handle ReadHandle()
            {
                return default(Handle);
            }
            public override ObjectId ReadHardOwnershipId()
            {
                return ObjectId.Null;
            }
            public override ObjectId ReadHardPointerId()
            {
                return ObjectId.Null;
            }
            public override short ReadInt16()
            {
                return 0;
            }
            public override int ReadInt32()
            {
                return 0;
            }
            public override long ReadInt64()
            {
                return 0L;
            }
            public override Point2d ReadPoint2d()
            {
                return default(Point2d);
            }
            public override Point3d ReadPoint3d()
            {
                return default(Point3d);
            }
            public override Scale3d ReadScale3d()
            {
                return default(Scale3d);
            }
            public override ObjectId ReadSoftOwnershipId()
            {
                return ObjectId.Null;
            }
            public override ObjectId ReadSoftPointerId()
            {
                return ObjectId.Null;
            }
            public override string ReadString()
            {
                return null;
            }
            public override ushort ReadUInt16()
            {
                return 0;
            }
            public override uint ReadUInt32()
            {
                return 0u;
            }
            public override ulong ReadUInt64()
            {
                return 0uL;
            }
            public override Vector2d ReadVector2d()
            {
                return default(Vector2d);
            }
            public override Vector3d ReadVector3d()
            {
                return default(Vector3d);
            }
            public override void ResetFilerStatus()
            {
            }
            public override void Seek(long offset, int method)
            {
            }
            public override void WriteAddress(IntPtr value)
            {
            }
            public override void WriteBinaryChunk(byte[] chunk)
            {
            }
            public override void WriteBoolean(bool value)
            {
            }
            public override void WriteByte(byte value)
            {
            }
            public override void WriteBytes(byte[] value)
            {
            }
            public override void WriteDouble(double value)
            {
            }
            public override void WriteHandle(Handle handle)
            {
            }
            public override void WriteInt16(short value)
            {
            }
            public override void WriteInt32(int value)
            {
            }
            public override void WriteInt64(long value)
            {
            }
            public override void WritePoint2d(Point2d value)
            {
            }
            public override void WritePoint3d(Point3d value)
            {
            }
            public override void WriteScale3d(Scale3d value)
            {
            }
            public override void WriteString(string value)
            {
            }
            public override void WriteUInt16(ushort value)
            {
            }
            public override void WriteUInt32(uint value)
            {
            }
            public override void WriteUInt64(ulong value)
            {
            }
            public override void WriteVector2d(Vector2d value)
            {
            }
            public override void WriteVector3d(Vector3d value)
            {
            }
            public override void WriteHardOwnershipId(ObjectId value)
            {

                HardOwnershipIds.Add(value);
            }
            public override void WriteHardPointerId(ObjectId value)
            {

                HardPointerIds.Add(value);
            }
            public override void WriteSoftOwnershipId(ObjectId value)
            {
                SoftOwnershipIds.Add(value);
            }
            public override void WriteSoftPointerId(ObjectId value)
            {
                SoftPointerIds.Add(value);
            }
            public void reset()
            {
                HardPointerIds.Clear();
                SoftPointerIds.Clear();
                HardOwnershipIds.Clear();
                SoftOwnershipIds.Clear();
            }
        }
    }

}
