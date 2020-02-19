using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DynamicClassGenerator
{
    public class DynamicClass
    {
        public CodeCompileUnit TargetUnit { get; set; }

        public DynamicClass()
        {
            TargetUnit = new CodeCompileUnit();
        }

        public int AddNameSpace(string name)
        {
            if (!ExistNameSpace(name))
            {
                CodeNamespace cns = new CodeNamespace(name);
                return TargetUnit.Namespaces.Add(cns);
            }
            else
            {
                Console.WriteLine("Already exist NameSpace");
                return -1;
            }
        }

        private bool ExistNameSpace(string nameSpace)
        {
            foreach (CodeNamespace cns in TargetUnit.Namespaces)
            {
                if (cns.Name == nameSpace)
                {
                    return true;
                }
            }
            return false;
        }

        private CodeNamespace GetCodeNameSpace(string nameSpace)
        {
            foreach (CodeNamespace cns in TargetUnit.Namespaces)
            {
                if (cns.Name == nameSpace)
                {
                    return cns;
                }
            }
            return null;
        }

        public Assembly CompileSource(params string[] references)
        {
            try
            {
                //CodeDomProvider cpd = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } });
                CodeDomProvider cpd = new CSharpCodeProvider();
                ICodeGenerator codeGenerator = cpd.CreateGenerator();
                StringBuilder generateCode = new StringBuilder();
                StringWriter codeWriter = new StringWriter(generateCode);
                CodeGeneratorOptions options = new CodeGeneratorOptions
                {
                    BracingStyle = "C"
                };
                codeGenerator.GenerateCodeFromCompileUnit(TargetUnit, codeWriter, options);
                string thisCode = generateCode.ToString();

                CompilerParameters cp = new CompilerParameters();
                foreach (string refer in references)
                {
                    cp.ReferencedAssemblies.Add(refer);
                }                
                cp.GenerateExecutable = false;

                CompilerResults cr = cpd.CompileAssemblyFromSource(cp, thisCode);
                return cr.CompiledAssembly;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        /// <summary>
        /// import is using ...
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="import"></param>
        public void AddImport(string nameSpace, string import)
        {
            try
            {
                // NameSpace가 한개도 없으면 신규 추가
                if (!ExistNameSpace(nameSpace))
                {
                    AddNameSpace(nameSpace);                    
                }

                // NameSpace 가 이미 있을 경우
                CodeNamespace codeNameSpace = GetCodeNameSpace(nameSpace);
                codeNameSpace.Imports.Add(new CodeNamespaceImport(import));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="className"></param>
        /// <param name="type"></param>
        /// <param name="imports"></param>
        /// <param name="references"></param>
        public void AddClass(string nameSpace, string className, TypeAttributes type, string[] imports = null, string[] references = null)
        {
            // NameSpace가 없으면 신규 추가
            if (!ExistNameSpace(nameSpace))
            {
                AddNameSpace(nameSpace);
            }

            // NameSpace 가 이미 있을 경우
            CodeNamespace codeNameSpace = GetCodeNameSpace(nameSpace);

            // using 추가
            foreach(string import in imports)
            {
                codeNameSpace.Imports.Add(new CodeNamespaceImport(import));
            }

            CodeTypeDeclaration newClass = new CodeTypeDeclaration(className)
            {
                IsClass = true,
                TypeAttributes = type
            };

            // reference 추가
            foreach (string refer in references)
            {
                newClass.BaseTypes.Add(refer);                    
            }

            // 해당 nameSpace에 Class 추가
            codeNameSpace.Types.Add(newClass);
        }

        public void AddMethod(string className, string methodName, MemberAttributes memberAttributes, Type returnType, string[] expressions, Parameter[] parameters)
        {
            CodeTypeDeclaration ctd = GetClass(className);

            if (ctd == null)
                return;

            CodeMemberMethod method = new CodeMemberMethod
            {
                Attributes = memberAttributes,
                Name = methodName,
            };

            method.ReturnType = new CodeTypeReference(returnType);
            foreach (Parameter param in parameters)
            {
                method.Parameters.Add(new CodeParameterDeclarationExpression(param.Type, param.Name));
            }

            foreach(string exp in expressions)
            {
                method.Statements.Add(new CodeExpressionStatement(new CodeSnippetExpression(exp)));
            }

            ctd.Members.Add(method);
        }

        private CodeTypeDeclaration GetClass(string className)
        {
            foreach (CodeNamespace cns in TargetUnit.Namespaces)
            {
                CodeTypeDeclarationCollection coll = cns.Types;

                for (int i = 0; i < coll.Count; i++)
                {
                    if (coll[i].Name == className)
                    {
                        return coll[i];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Member Property 구현 Getter 만 구현 get 
        /// </summary>
        /// <param name="className"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="memberTypes"></param>
        /// <param name="memberAttributes"></param>
        /// <param name="getMessage"></param>
        public void AddPropertyHasGet(string className, string name, Type type, MemberAttributes memberAttributes, string getMessage)
        {
            CodeTypeDeclaration ctd = GetClass(className);

            if (ctd == null)
                return;            

            CodeMemberProperty member = new CodeMemberProperty()
            {
                Attributes = memberAttributes | MemberAttributes.Final,
                Name = name,
                HasGet = true,
                HasSet = false,
                Type = new CodeTypeReference(type),
            };
            member.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, getMessage)));
            ctd.Members.Add(member);
        }

        public void AddField(string className, string name, Type type, MemberAttributes memberAttributes, bool isCreate = false, bool hasGet = false)
        {
            CodeTypeDeclaration ctd = GetClass(className);

            if (ctd == null)
                return;

            string typeName = type.FullName.Replace(type.Namespace + ".", "");
            var cdProvider = CodeDomProvider.CreateProvider("CSharp");
            var reference = new CodeTypeReference(typeName);
            string csTypeName = cdProvider.GetTypeOutput(reference);

            CodeMemberField member = new CodeMemberField()
            {
                Attributes = memberAttributes | MemberAttributes.Final,
                Name = name,
                Type = new CodeTypeReference(typeName),
            };

            if (isCreate == true)
            {
                if (hasGet == true)
                {
                    member.Name += " { get; }" + $" = new {csTypeName}()";
                }
                else
                {
                    member.Name += "" + $" = new {csTypeName}()";
                }
                ctd.Members.Add(member);
            }
            else
            {
                if (hasGet)
                {
                    CodeMemberProperty codeMemberProperty = new CodeMemberProperty()
                    {
                        Attributes = memberAttributes | MemberAttributes.Final,
                        Name = name,
                        HasGet = false,
                        HasSet = false,
                        Type = new CodeTypeReference(typeName),
                    };

                    //codeMemberProperty.Name += " { get; }//";
                    ctd.Members.Add(codeMemberProperty);
                }
                else
                {
                    ctd.Members.Add(member);
                }
            }
            
        }

        public void AddProperty(string className, string name, Type type, MemberAttributes memberAttributes, bool hasGetSetDefault = true)
        {
            CodeTypeDeclaration ctd = GetClass(className);

            if (ctd == null)
                return;

            if (hasGetSetDefault == true)
            {
                CodeMemberField member = new CodeMemberField()
                {
                    Attributes = memberAttributes | MemberAttributes.Final,
                    Name = name,
                    Type = new CodeTypeReference(type),
                };
                member.Name += " { get; set; }//";
                ctd.Members.Add(member);
            }
            else
            {
                CodeMemberField mf = new CodeMemberField()
                {
                    Attributes = MemberAttributes.Private | MemberAttributes.Final,
                    Name = "_" + name.ToLower(),
                    Type = new CodeTypeReference(type)
                };

                ctd.Members.Add(mf);

                CodeMemberProperty member = new CodeMemberProperty()
                {
                    Attributes = memberAttributes | MemberAttributes.Final,
                    Name = name,
                    HasGet = true,
                    HasSet = true,
                    Type = new CodeTypeReference(type),
                };

                member.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + name.ToLower())));
                member.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + name.ToLower()), new CodePropertySetValueReferenceExpression()));
                ctd.Members.Add(member);
            }                
        }
    }
}

