using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using System.IO;

namespace NewLoad
{
    public class Class1
    {
        private Action cmd1;
        public Class1()
        {
            Reload();
        }
        [CommandMethod("Reload1")]
        public void Reload()
        {
            var adapterFileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var targetFileInfo = Path.Combine(adapterFileInfo.DirectoryName, "ACTest.dll");
            var targetAssembly = Assembly.Load(File.ReadAllBytes(targetFileInfo));
            var targetType = targetAssembly.GetType("ACTest.Class1");
            var targetMethod = targetType.GetMethod("Cmd1");
            var targetObject = Activator.CreateInstance(targetType);
            cmd1 = () => targetMethod.Invoke(targetObject, null);
        }
        [CommandMethod("Cmd1")]
        public void Cmd1()
        {
            cmd1?.Invoke();
        }
    }
}
