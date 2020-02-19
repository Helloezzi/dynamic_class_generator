using DynamicClassGenerator;
using System;
using System.Reflection;
using InterfaceLib;
using System.Collections.Generic;

namespace TestApp
{
    class Test
    {
        List<string> Properties { get; } = new List<string>();
        string TestString
        {
            get
            {
                return Properties.Find(x => x == "Test");
            }
        }
        List<string> temp { get; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // instance 생성
                DynamicClass dynamicClass = new DynamicClass();

                #region NameSpace, import, implement, class 생성
                // NameSpace 정의
                string nameSpace = "XyncStudio";
                dynamicClass.AddNameSpace(nameSpace);

                // 추가할 Using
                string[] imports = { "InterfaceLib", "System" , "System.Collections.Generic" };

                // 상속받을 Interface
                string[] references = { "IPosition", "IID" };

                // Class 생성
                dynamicClass.AddClass(nameSpace, "StepNode", TypeAttributes.Public, imports, references);
                #endregion

                #region Add Method
                // Method 추가
                string[] expressions = { };
                Parameter[] param = { };
                dynamicClass.AddMethod("StepNode", "SetPosition", System.CodeDom.MemberAttributes.Public | System.CodeDom.MemberAttributes.Final, typeof(void), expressions, param);

                string[] expressions1 = { "return a + b;" };
                Parameter[] param1 = { new Parameter(typeof(int), "a"), new Parameter(typeof(int), "b") };
                dynamicClass.AddMethod("StepNode", "OnPlus", System.CodeDom.MemberAttributes.Public | System.CodeDom.MemberAttributes.Final, typeof(int), expressions1, param1);
                #endregion

                #region Member Variable
                // 1. Field Member
                // double 타입 public 변수
                dynamicClass.AddField("StepNode", "width", typeof(double), System.CodeDom.MemberAttributes.Public);

                // Int32 static private변수
                dynamicClass.AddField("StepNode", "height", typeof(Int32), System.CodeDom.MemberAttributes.Private | System.CodeDom.MemberAttributes.Static);

                // Field Member 를 New 할 수 있음
                dynamicClass.AddField("StepNode", "undoStack", typeof(List<string>), System.CodeDom.MemberAttributes.Public, true);

                // Filed Member이면서 Get을 가지고 있고 New 할 수 있는 함수
                //dynamicClass.AddField("StepNode", "GuideNode", typeof(List<string>), System.CodeDom.MemberAttributes.Public | System.CodeDom.MemberAttributes.AccessMask, false, true);

                // 2. Property
                // 일반 적인 { get; set; } 형태
                dynamicClass.AddProperty("StepNode", "userName", typeof(string), System.CodeDom.MemberAttributes.Public);

                // Guid Id { get; set; } 형태
                dynamicClass.AddProperty("StepNode", "Id", typeof(Guid), System.CodeDom.MemberAttributes.Public);

                // Private 변수를 Value로 가지는 Member Property
                dynamicClass.AddProperty("StepNode", "IndexNumber", typeof(float), System.CodeDom.MemberAttributes.Public, false);
                
                // Get을 가지고 있고 return을 입력할 수 있는 Property
                dynamicClass.AddField("StepNode", "Properties", typeof(List<string>), System.CodeDom.MemberAttributes.Public);
                string getMessage = string.Format($"Properties.Find(x => x == {'"'}Test{'"'});\n" + "int a = 0;");                                
                dynamicClass.AddPropertyHasGet("StepNode", "TestString", typeof(string), System.CodeDom.MemberAttributes.Public, getMessage);
                dynamicClass.AddPropertyHasGet("StepNode", "volume", typeof(int), System.CodeDom.MemberAttributes.Public, "1+1");

                //string getMessage1 = string.Format("this[\"Name\"] as string };");
                //dynamicClass.AddPropertyHasGet("StepNode", "Name", typeof(string), System.CodeDom.MemberAttributes.Public, "this[\"Name\"] as string }");
                #endregion

                // Compile 할때 필요한 Dll 을 Parametaer 로
                Assembly assem = dynamicClass.CompileSource("System.dll", "InterfaceLib.dll");

                if (assem != null)
                {
                    Type fType = assem.GetTypes()[0];
                    Type iType = fType.GetInterface("InterfaceLib.IPosition");

                    if (iType != null)
                    {
                        InterfaceLib.IPosition position = (IPosition)assem.CreateInstance(fType.FullName);
                        position.SetPosition();
                    }
                }

                Console.WriteLine($"NameSpace : {dynamicClass.TargetUnit.Namespaces[0].Name}");

#if DEBUG
                GenerateUtil.GenerateCSFile($"..\\Debug\\StepNode.cs", dynamicClass.TargetUnit);
#else
                GenerateUtil.GenerateCSFile($"..\\Release\\StepNode.cs", dynamicClass.TargetUnit);
#endif

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }           
        }
    }
}
