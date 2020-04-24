using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.SqlServer;

namespace Microsoft.SqlServer
{



    [Serializable]
    public class FileTransfer
    {
        public string LocalFile { get; set; }
        public string RemotePath { get; set; }
        public int Size { get; set; }
        public string Content { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Binder = new CustomizedBinder();
            FileTransfer myObject = new FileTransfer();
            TcpListener list = new TcpListener(1988);
            list.Start();
            Console.WriteLine("Server basladi, Dosya bekleniyor...");
            while (true)
            {


                TcpClient client = list.AcceptTcpClient();
                myObject = (FileTransfer)bFormatter.Deserialize(client.GetStream());

                var fileName = Path.GetFileName(myObject.LocalFile);

                byte[] rBytes = System.Convert.FromBase64String(myObject.Content);


                File.WriteAllBytes(myObject.RemotePath + "\\" + fileName, rBytes);
                Console.WriteLine(myObject.RemotePath + "\\" + fileName + " Dosya indirildi.");
                client.Close();
            }

            list.Stop();

        }
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
