using System;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace Diary.Extensions
{
    public static class Serializer
    {
        #region Functions
        /// <summary>
        /// This function writes the serialized XML to the file name passed in.
        /// </summary>
        /// <typeparam name="T">The object type to serialize.</typeparam>
        /// <param name="t">The instance of the object.</param>
        /// <param name="outFilename">The file name. It can be a full path.</param>
        public static void SerializeToXML<T>(T t, String outFilename, XmlSerializerNamespaces inNameSpaces = null)
        {
            if (outFilename.Contains(@"\"))
            {
                String[] dirs = outFilename.Split(@"\".ToCharArray());

                for (int i = 0; i < dirs.Length - 1; i++)
                {

                    if (!Directory.Exists(dirs[i]))
                        Directory.CreateDirectory(dirs[i]);
                }
            }
            XmlSerializerNamespaces ns = inNameSpaces;
            if (ns == null)
            {
                ns = new XmlSerializerNamespaces();
                ns.Add("", "");
            }
            XmlSerializer serializer = new XmlSerializer(t.GetType());
            TextWriter textWriter = (TextWriter)new StreamWriter(outFilename);
            serializer.Serialize(textWriter, t, ns);
            textWriter.Close();
        }

        /// <summary>
        /// This function returns the serialized XML as a string
        /// </summary>
        /// <typeparam name="T">The object type to serialize.</typeparam>
        /// <param name="t">The instance of the object.</param>
        /// <param name="outString">The string that will be passed the XML.</param>
        public static String SerializeToXML<T>(T t, XmlSerializerNamespaces inNameSpaces = null)
        {
            XmlSerializerNamespaces ns = inNameSpaces;
            if (ns == null)
            {
                ns = new XmlSerializerNamespaces();
                ns.Add("", "");
            }
            XmlSerializer serializer = new XmlSerializer(t.GetType());
            TextWriter textWriter = (TextWriter)new StringWriter();
            serializer.Serialize(textWriter, t, ns);
            return textWriter.ToString();
        }

        /// <summary>
        /// This function deserializes the XML file passed in.
        /// </summary>
        /// <typeparam name="T">The object type to serialize.</typeparam>
        /// <param name="inFilename">The file or full path to the file.</param>
        /// <returns>The object that was deserialized from xml.</returns>
        public static T DeserializeFromXML<T>(String inFilename)
        {
            // Wait 1 second if file doesn't exist, in case we are waiting on a
            // separate thread and beat it here.
            if (!File.Exists(inFilename))
                Thread.Sleep(1000);

            // File should exist by now.
            if (File.Exists(inFilename))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(T));
                TextReader textReader = (TextReader)new StreamReader(inFilename);
                XmlTextReader reader = new XmlTextReader(textReader);
                try { reader.Read(); }
                catch { }
                var retVal = (T)deserializer.Deserialize(reader);
                textReader.Close();
                return retVal;
            }
            else
            {
                throw new FileNotFoundException(inFilename);
            }
        }

        /// <summary>
        /// This function deserializes the XML string passed in.
        /// </summary>
        /// <typeparam name="T">The object type to serialize.</typeparam>
        /// <param name="inString">The string containing the XML.</param>
        /// <returns>The object that was deserialized from xml.</returns>
        public static T DeserializeFromXML<T>(ref String inString)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T));
            TextReader textReader = (TextReader)new StringReader(inString);
            T retVal = (T)deserializer.Deserialize(textReader);
            textReader.Close();
            return retVal;
        }
        #endregion
    }
}
