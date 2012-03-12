using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using ProtoBuf;
using ProtoBuf.Meta;
using System.Reflection;
using log4net;

namespace MyUtilities
{
   
    public static class SerializationHelper
    {
        private static ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

        public static bool TryXmlSerialize<T>(T sourceObject, out string result) where T : new()
        {
            result = null;
            if (sourceObject == null)
                return false;

            System.Xml.Serialization.XmlSerializer XmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            MemoryStream targetStream = new MemoryStream();
            try
            {
                XmlSerializer.Serialize(targetStream, sourceObject);
                byte[] r = targetStream.GetBuffer();
                byte[] r2 = new byte[r.Length];
                r.CopyTo(r2, 0);
                result = r2.ToDefaultEncodedString();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                targetStream.Dispose();
            }

        }

        public static bool TryXmlDeSerialize<T>(string sourceObject, out T obj) where T : new()
        {
            obj = default(T);
            if (String.IsNullOrEmpty(sourceObject))
                return false;

            XmlSerializer ser = new XmlSerializer(typeof(T));
            TextReader tr = new StringReader(sourceObject);
            try
            {
                obj = (T)ser.Deserialize(tr);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                tr.Dispose();
            }

        }


        public static bool TryBinaryDeSerialize<T>(byte[] sourceObject, out T obj)
        {
            obj = default(T);

            if (sourceObject == null)
                return false;
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream sourceStream = new MemoryStream(sourceObject);

            try
            {
                obj = (T)formatter.Deserialize(sourceStream);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                sourceStream.Dispose();
            }
        }


        public static bool TryBinarySerialize<T>(T sourceObject, IDictionary<string, string> headers, out byte[] result)
        {
            result = null;
            if (sourceObject == null)
                return false;
            BinaryFormatter formatter = new BinaryFormatter();

            MemoryStream targetStream = new MemoryStream();
            try
            {
                List<Header> lHeaders = new List<Header>();
                if (headers != null)
                    foreach (var header in headers)
                        lHeaders.Add(new Header(header.Key, header.Value));

                formatter.Serialize(targetStream, sourceObject, lHeaders.ToArray());
                byte[] r = targetStream.GetBuffer();
                result = new byte[r.Length];
                r.CopyTo(result, 0);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                targetStream.Dispose();
            }
        }

        public static bool TryBinarySerialize<T>(T sourceObject, out byte[] result)
        {
            return TryBinarySerialize(sourceObject, null, out result);
        }

        public static bool TryProtoBufSerialize<T>(T sourceObject, out byte[] result)
        {
            result = null;
            if (sourceObject == null)
                return false;
            try
            {
                using (MemoryStream targetStream = new MemoryStream())
                {
                    var header = new MessageHeader()
                    {
                        Guid = Guid.NewGuid(),
                        TypeName = typeof(T).AssemblyQualifiedName
                    };
                    Serializer.SerializeWithLengthPrefix<MessageHeader>(targetStream, header, PrefixStyle.Base128);
                    Serializer.SerializeWithLengthPrefix<T>(targetStream, sourceObject, PrefixStyle.Base128);
                    //Serializer.Serialize<T>(targetStream, sourceObject);
                    //Serializer.NonGeneric.Serialize(targetStream, sourceObject);
                    //Serializer.SerializeWithLengthPrefix<T>(targetStream, sourceObject, PrefixStyle.Base128);
                    byte[] r = targetStream.GetBuffer();
                    result = new byte[r.Length];
                    r.CopyTo(result, 0);
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Debug("Problem protobuf serializing message.", ex);
                return false;
            }
        }

        public static bool TryProtoBufDeserialize<T>(byte[] bytes, out T obj)
        {
            obj = default(T);

            if (bytes == null)
                return false;

            try
            {
                using (MemoryStream sourceStream = new MemoryStream(bytes))
                {
                    MessageHeader header;
                    header = Serializer.DeserializeWithLengthPrefix<MessageHeader>(sourceStream, PrefixStyle.Base128);
                    if (!typeCache.ContainsKey(header.TypeName))
                        typeCache[header.TypeName] = Type.GetType(header.TypeName);
                    MethodInfo m = typeof(Serializer).GetMethod("DeserializeWithLengthPrefix", new Type[] {typeof(Stream), 
                        typeof(PrefixStyle)}).MakeGenericMethod(typeCache[header.TypeName]);
                    obj = (T)m.Invoke(null, new object[] { sourceStream, PrefixStyle.Base128 });
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        public static bool TryDataContractSerialize<T>(T data, out byte[] result)
        {
            result = null;
            try
            {
                var ser = new DataContractSerializer(typeof(T));
                MemoryStream targetStream = new MemoryStream();
                ser.WriteObject(targetStream, data);
                byte[] r = targetStream.GetBuffer();
                result = new byte[r.Length];
                r.CopyTo(result, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryDataContractDeserialize<T>(byte[] sourceObject, out T result)
        {
            result = default(T);
            try
            {
                var ser = new DataContractSerializer(typeof(T));
                MemoryStream sourceStream = new MemoryStream(sourceObject);
                result = (T)ser.ReadObject(sourceStream);
                return true;
            }
            catch
            {
                return false;
            }
        }



        [ProtoContract]
        class MessageHeader
        {
            public MessageHeader() { }

            [ProtoMember(1, IsRequired = true)]
            public Guid Guid { get; set; }

            //[ProtoIgnore]
            //public Type Type { get; set; }
            private string typeName;
            [ProtoMember(2, IsRequired = true)]
            public string TypeName
            {
                get { return typeName; }
                set { typeName = value; }
            }
        }

    }
}


