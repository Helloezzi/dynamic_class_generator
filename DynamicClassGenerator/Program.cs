using DynamicClassGenerator;
using System;
using System.Reflection;
using InterfaceLib;
using System.Collections.Generic;

namespace TestApp
{


    class Test
    {
        List<string> Properties;

        string TestString
        {
            get
            {
                return Properties.Find(x => x == "Test");
            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // instance 생성
                DynamicClass dynamicClass = new DynamicClass();  

                // NameSpace 정의
                string nameSpace = "XyncStudio";
                dynamicClass.AddNameSpace(nameSpace);

                // 추가할 Using
                string[] imports = { "InterfaceLib", "System" , "System.Collections.Generic" };

                // 상속받을 Interface
                string[] references = { "IPosition", "IID" };

                // Class 생성
                dynamicClass.AddClass(nameSpace, "StepNode", TypeAttributes.Public, imports, references);

                // Method 추가
                string[] expressions = { };
                Parameter[] param = { };
                dynamicClass.AddMethod("StepNode", "SetPosition", System.CodeDom.MemberAttributes.Public | System.CodeDom.MemberAttributes.Final, "", expressions, param);

                // Variable
                // Double 타입 public 변수
                dynamicClass.AddVariable("StepNode", "width", Enumeration.VARIABLE_TYPE.Double.ToString(), MemberTypes.Field, System.CodeDom.MemberAttributes.Public);

                // Int32 static private변수
                dynamicClass.AddVariable("StepNode", "height", Enumeration.VARIABLE_TYPE.Int32.ToString(), MemberTypes.Field, System.CodeDom.MemberAttributes.Private | System.CodeDom.MemberAttributes.Static);

                // string const private 변수
                dynamicClass.AddVariable("StepNode", "nodeName", Enumeration.VARIABLE_TYPE.String.ToString(), MemberTypes.Field, System.CodeDom.MemberAttributes.Private);

                // 일반 적인 { get; set; } 형태
                dynamicClass.AddVariable("StepNode", "TestProperty", Enumeration.VARIABLE_TYPE.String.ToString(), MemberTypes.Property, System.CodeDom.MemberAttributes.Public | System.CodeDom.MemberAttributes.Final);

                // private 변수를 가지는 Get Set
                dynamicClass.AddVariable("StepNode", "TestProperty2", Enumeration.VARIABLE_TYPE.String.ToString(), MemberTypes.Property, System.CodeDom.MemberAttributes.Public | System.CodeDom.MemberAttributes.Final, false);

                dynamicClass.AddVariable("StepNode", "Id", "Guid", MemberTypes.Property, System.CodeDom.MemberAttributes.Public | System.CodeDom.MemberAttributes.Final);

                dynamicClass.AddVariable("StepNode", "Properties", "List<string>", MemberTypes.Field, System.CodeDom.MemberAttributes.Public);

                //
                string getMessage = string.Format($"Properties.Find(x => x == {'"'}Test{'"'});\n" + "int a = 0;");
                // private 변수를 가지는 Get Set
                dynamicClass.AddVariable("StepNode", "TestString", Enumeration.VARIABLE_TYPE.String.ToString(), MemberTypes.Property, System.CodeDom.MemberAttributes.Public | System.CodeDom.MemberAttributes.Final, getMessage);

                // Compile 할때 필요한 Dll 을 Parameter 로
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
