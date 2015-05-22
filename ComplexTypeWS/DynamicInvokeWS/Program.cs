using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;

//Call a Web Service Without Adding a Web Reference
//http://www.codeproject.com/Articles/42278/Call-a-Web-Service-Without-Adding-a-Web-Reference

namespace DynamicInvokeWS
{
    class Program
    {
        static void Main(string[] args)
        {
            string wsUrl = @"http://localhost:3414/TestService.asmx";
            var silentResult = CallWebService(wsUrl, "TestService", "Silent", null);
            Console.WriteLine("Silent:{0}", JsonConvert.SerializeObject(silentResult));
            //null

            var helloWorldResult = CallWebService(wsUrl, "TestService", "HelloWorld", null);
            Console.WriteLine("HelloWorld:{0}", JsonConvert.SerializeObject(helloWorldResult));
            //Hello World

            var singleValueResult = CallWebService(wsUrl, "TestService", "SingleValue", new object[] { "亂馬客" });
            Console.WriteLine("SingleValue:{0}", JsonConvert.SerializeObject(singleValueResult));
            //Hello World, 亂馬客


            string simpleArrayArg = @"['RM', '亂馬客']";
            var SimpleArrayResult = CallWebService(wsUrl, "TestService", "SimpleArray", new object[] { simpleArrayArg });
            Console.WriteLine("SimpleArray:{0}", JsonConvert.SerializeObject(SimpleArrayResult));
            //Hello World, RM,亂馬客


            string simpleArg = @"{'Id':'999', 'Name':'亂馬客'}";
            var SimpleObjResult = CallWebService(wsUrl, "TestService", "SimpleObj", new object[] { simpleArg });
            Console.WriteLine("SimpleObj:{0}", JsonConvert.SerializeObject(SimpleObjResult));
            //Hello UserInfo, 999-亂馬客

            string depUsersArg = @"{'Id':'1', 'Name':'技術開發部', 'Users':[{'Id':'1', 'Name':'RM'},{'Id':'999', 'Name':'亂馬客'}]}";
            var depUsersObjResult = CallWebService(wsUrl, "TestService", "GetDepUsers", new object[] { depUsersArg });
            Console.WriteLine("GetDepUsers:{0}", JsonConvert.SerializeObject(depUsersObjResult));
            // 1, RM / 999, 亂馬客

            string usersArg = @"[{'Id':'1', 'Name':'RM'},{'Id':'999', 'Name':'亂馬客'}]";
            var getUsersResult = CallWebService(wsUrl, "TestService", "GetUsers", new object[] { usersArg });
            Console.WriteLine("GetUsers:{0}", JsonConvert.SerializeObject(getUsersResult));
            // 1, RM / 999, 亂馬客
            Console.ReadKey();
        }

        public static Object CallWebService(string webServiceAsmxUrl,
           string serviceName, string methodName, object[] args)
        {
            System.Net.WebClient client = new System.Net.WebClient();

            //Connect To the web service
            System.IO.Stream stream = client.OpenRead(webServiceAsmxUrl + "?wsdl");

            //Read the WSDL file describing a service.
            ServiceDescription description = ServiceDescription.Read(stream);

            //Load the DOM

            //--Initialize a service description importer.
            ServiceDescriptionImporter importer = new ServiceDescriptionImporter();

            //Use SOAP 1.2. (有些Java的WebService不Support 1.2，請設成空字串
            importer.ProtocolName = "Soap12"; 

            importer.AddServiceDescription(description, null, null);
            //--Generate a proxy client. 
            importer.Style = ServiceDescriptionImportStyle.Client;
            //--Generate properties to represent primitive values.

            importer.CodeGenerationOptions = System.Xml.Serialization.CodeGenerationOptions.GenerateProperties;

            //Initialize a Code-DOM tree into which we will import the service.
            CodeNamespace codenamespace = new CodeNamespace();
            CodeCompileUnit codeunit = new CodeCompileUnit();
            codeunit.Namespaces.Add(codenamespace);

            //Import the service into the Code-DOM tree. 
            //This creates proxy code that uses the service.

            ServiceDescriptionImportWarnings warning = importer.Import(codenamespace, codeunit);
            if (warning == 0)
            {

                //--Generate the proxy code
                CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

                //--Compile the assembly proxy with the 
                //  appropriate references
                string[] assemblyReferences = new string[]  {
                       "System.dll", 
                       "System.Web.Services.dll", 
                       "System.Web.dll", 
                       "System.Xml.dll", 
                       "System.Data.dll"};

                //--Add parameters
                CompilerParameters parms = new CompilerParameters(assemblyReferences);
                parms.GenerateInMemory = true; //(Thanks for this line nikolas)
                CompilerResults results = provider.CompileAssemblyFromDom(parms, codeunit);

                //--Check For Errors
                if (results.Errors.Count > 0)
                {

                    foreach (CompilerError oops in results.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine("========Compiler error============");
                        System.Diagnostics.Debug.WriteLine(oops.ErrorText);
                    }
                    throw new Exception("Compile Error Occured calling WebService.");
                }

                //--Finally, Invoke the web service method
                Object wsvcClass = results.CompiledAssembly.CreateInstance(serviceName);
                MethodInfo mi = wsvcClass.GetType().GetMethod(methodName);

                //這裡取出 WebService 的參數
                //判斷如果是物件或是Array的話，就透過JSON.NET來轉成WS要的物件
                ParameterInfo[] pInfos = mi.GetParameters();
                ArrayList newArgs = new ArrayList();
                int i = 0;
                foreach (ParameterInfo p in pInfos)
                {
                    Type pType = p.ParameterType;
                    if (pType.IsPrimitive || pType == typeof(string))
                    {
                        newArgs.Add(args[i]);
                    }
                    else{
                        //透過 JSON.NET 轉成物件
                        var argObj = JsonConvert.DeserializeObject((string)args[i], pType);
                        newArgs.Add(argObj);
                    }
                }

                return mi.Invoke(wsvcClass, newArgs.ToArray());

            }
            else
            {
                return null;
            }
        }
    }
}
