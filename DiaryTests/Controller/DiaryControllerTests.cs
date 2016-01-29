using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Diary.Controller;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Diary.Controller.Tests
{
    [TestClass()]
    public class DiaryControllerTests
    {
        public static ILog Log;
        private const string entryPath = "..\\Entries.xml";
        
        private static void ConfigureLogging()
        {
            // Set logfile name and application name variables
            //GlobalContext.Properties["LogName"] = string.Format("{0}{1}", ConfigurationManager.AppSettings["LogPath"], Assembly.GetExecutingAssembly().GetName().Name + ".log");
            GlobalContext.Properties["Application"] = Assembly.GetExecutingAssembly().GetName().Name;
            GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;

            Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType); //Instantiate the logger
            Log.Info("Starting the testing application..");


        }


        [TestMethod()]
        public void DiaryControllerTest_Good()
        {
            ConfigureLogging();
            DiaryController dc = new DiaryController(Log, entryPath);
            if (dc == null)
                Assert.Fail();
            
            Assert.IsNotNull(dc.EntryFileFullName);
        }

        [TestMethod()]
        public void DiaryControllerTest_MissingEverything()
        {
            ConfigureLogging();
            try
            {
                DiaryController dc = new DiaryController(null, null);
            }
            catch (ArgumentNullException e)
            {
                Assert.IsTrue(e is ArgumentNullException);
            }

            catch (Exception e)
            {
                Assert.Fail("Exception was not caught.", e.Message);
            }
          
          

          
        }

        [TestMethod()]
        public void DiaryControllerTest_BadFile()
        {
            ConfigureLogging();
            //var entryPath = "Z:\\test\\notthere";
            try
            {
                DiaryController dc = new DiaryController(null, null);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is Exception);
            }


        }



        [TestMethod()]
        public void AddEntryTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteEntryTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateEntryTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetDiaryEventsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetDiaryEntriesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ArchiveFileTest()
        {
            Assert.Fail();
        }
    }
}
