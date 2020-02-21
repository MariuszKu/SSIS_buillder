using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PackGen
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Building Package...");

            // Create a new SSIS Package
            var package = new Package();
            TaskHost taskHost;

            // Add a Connection Manager to the Package, of type, OLEDB 
            var connMgrOleDb = package.Connections.Add("OLEDB");

            var connectionString = new StringBuilder();

            connectionString.Append("Provider=SQLOLEDB.1;");
            connectionString.Append("Integrated Security=SSPI;Initial Catalog=");
            connectionString.Append("DB");
            connectionString.Append(";Data Source=");
            connectionString.Append(".");
            connectionString.Append(";");

            connMgrOleDb.ConnectionString = connectionString.ToString();
            connMgrOleDb.Name = "My OLE DB Connection";
            connMgrOleDb.Description = "OLE DB connection";
            int i = 0;

            package.Variables.Add("ChangeDate", false, "User", "2019-21-01");

            using (SqlConnection con = new SqlConnection("Data Source=.; Initial Catalog=DB;Integrated Security=SSPI;"))
            { 
                con.Open();

                using (SqlCommand command = new SqlCommand("select * from ##list", con))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        var a = package.Executables.Add("STOCK:SQLTask");
                        taskHost = a as TaskHost;
                        i++;
                        // Set required properties
                        taskHost.Properties["Connection"].SetValue(taskHost, connMgrOleDb.ID);
                        taskHost.Properties["SqlStatementSource"].SetValue(taskHost, reader.GetString(1));
                        
                            // Add variable to hold parameter value
                        
                        
                        Console.WriteLine(taskHost.InnerObject.GetType().ToString());
                        
                        ExecuteSQLTask task = taskHost.InnerObject as ExecuteSQLTask;
                        task.ParameterBindings.Add();
                        IDTSParameterBinding parameterBinding = task.ParameterBindings.GetBinding(0);
                        parameterBinding.DtsVariableName = "User::ChangeDate";
                        parameterBinding.ParameterDirection = ParameterDirections.Input;
                        parameterBinding.DataType = 7;
                        parameterBinding.ParameterName = "0";

                        taskHost.Name = reader.GetString(2);
                    }
                }

                using (SqlCommand command = new SqlCommand("select * from ##dep2", con))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        PrecedenceConstraint pcFileTasks = package.PrecedenceConstraints.Add(package.Executables[reader.GetString(1)], package.Executables[reader.GetString(0)]);
                    }
                }
            }


            var app = new Application();



            Console.WriteLine("Saving Package...");
            app.SaveToXml(@"C:\Users\cognos\Documents\SSIS Exports\test.dtsx", package, null);

        }
    }
}
