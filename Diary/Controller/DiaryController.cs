using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;
using System.Xml.Linq;
using Diary.Model;
using log4net;

namespace Diary.Controller
{
    public class DiaryController
    {
        //if you need to add a prop to diaryentries, add, updated, loadlist needs to be updated.

        #region props
        private readonly ILog _log;

        private static XDocument _xmlDoc;
        
        public string EntryFileFullName { get; set; }

        public string SortDir { get; set; }

        public IEnumerable<string> EntryEvents { get; private set; } 
        
        #endregion

        public DiaryController(ILog log, string pathToEntries, string sortDir = "desc")
        {
            
            if(log == null)
                    throw new ArgumentNullException("log", "ILog is null");

            _log = log;
            _xmlDoc = GetEntryFile(pathToEntries);
            SortDir = sortDir;
            EntryEvents = GetDiaryEvents();

        }

        public void AddEntry(string entrytxt, string eventName= "")
        {
            //make sure file and dir still exist
            //XDocument xmlDoc = GetEntryFile(EntryFileFullName);

            List<DiaryEntry> items = LoadList();
            int max = items.Count == 0 ? 0 : items.Max(i => i.Id);

            int newId = ++max;

            var xElement = _xmlDoc.Element("Entries");
            if (xElement != null)
                xElement.Add(
                    new XElement("DiaryEntry", new XElement("Id", newId.ToString(CultureInfo.InvariantCulture)),
                        new XElement("EntryDt", DateTime.UtcNow),
                        new XElement("EntryEvent", eventName),
                        new XElement("EntryTxt", entrytxt),
                        new XElement("UserName",GetUser())
                        ));

            _xmlDoc.Save(EntryFileFullName);

            EntryEvents = GetDiaryEvents();
        }

        public void DeleteEntry(DiaryEntry entry)
        {
            //delete 
            //entry.Id
            _xmlDoc.Root.Elements().Where(e => e.Attribute("id").Value.Equals(entry.Id.ToString())).Select(e => e).Single().Remove();
            _xmlDoc.Save(EntryFileFullName);
        }

        public void UpdateEntry(DiaryEntry entry)
        {
            //find original
            var items = from item in _xmlDoc.Descendants("DiaryEntry")
                let xElement = item.Element("Id")
                where xElement != null && xElement.Value == entry.Id.ToString()
                        select item;

            foreach (XElement itemElement in items)
            {
                itemElement.SetElementValue("EntryEvent", entry.EntryEvent);
                itemElement.SetElementValue("EntryTxt", entry.EntryTxt);
                itemElement.SetElementValue("EntryDt", entry.EntryDt);
                itemElement.SetElementValue("UserName", entry.UserName);
            }
            

            //update with this one.


            _xmlDoc.Save(EntryFileFullName);
        }
        
        public IEnumerable<string> GetDiaryEvents()
        {
            //EntryEvents
            IEnumerable<string> entries = from e in _xmlDoc.Descendants("DiaryEntry") 
                                          let xElement = e.Element("EntryEvent") 
                                          where xElement != null 
                                          select (xElement.Value);

            return entries.Distinct();
        }

        public BindingList<DiaryEntry> GetDiaryEntries(string eventTypeFilter = "")
        {
            
            string sort = string.IsNullOrEmpty(SortDir) ? "desc" : SortDir.ToLower();
            List<DiaryEntry> entries = new List<DiaryEntry>();
            switch (sort)
            {
                case "desc":
                    entries = LoadList(eventTypeFilter).OrderByDescending(x => x.EntryDt).ToList();
                    break;
                case "asc":
                    entries =  LoadList(eventTypeFilter).OrderBy(x => x.EntryDt).ToList();
                    break;
                default:
                    entries =  LoadList(eventTypeFilter).OrderBy(x => x.Id).ToList();
                    break;
            }

            var bindinglist = new BindingList<DiaryEntry>(entries);

            return bindinglist;
        }

        public void ArchiveFile()
        {
            string datepart = String.Format("{0:yyyy-MM-dd_hh-mm-ss}", DateTime.Now.ToUniversalTime());

            string newFileName = string.Format("{0}-{1}", Path.GetFileNameWithoutExtension(EntryFileFullName), datepart);

            string outFileName = EntryFileFullName.Replace(Path.GetFileNameWithoutExtension(EntryFileFullName), newFileName);
            
            try
            {
                _log.Info(string.Format("Moving current File - {0} to new archive File - {1}", EntryFileFullName, outFileName));
                File.Move(EntryFileFullName, outFileName);
            }
            catch (Exception)
            {
                _log.Error(string.Format("ERROR - Moving current File - {0} to new archive File - {1}", EntryFileFullName, outFileName));
                throw;
            }
            

            //generate new file
            _xmlDoc = GetEntryFile(EntryFileFullName);
        }
        
        private XDocument GetEntryFile(string pathToEntries)
        {
            _log.Info(string.Format("Configuring Entries File {0}.", pathToEntries));

            EntryFileFullName = string.IsNullOrEmpty(pathToEntries)
                ? Application.ExecutablePath.Replace(Assembly.GetExecutingAssembly().GetName().Name + ".EXE", "") +
                  "Entries.xml"
                : pathToEntries;

            _log.Info(string.Format("Checking Entries File at {0}.", EntryFileFullName));
            //check if path exists
            var fi = new FileInfo(Path.GetFullPath(EntryFileFullName));
            if (fi.Exists)
            {
                _log.Info(string.Format("File already Exists {0}.", EntryFileFullName));
                return XDocument.Load(EntryFileFullName);
            }

            // Determine whether the directory exists.
            string path = Path.GetDirectoryName(EntryFileFullName);
            if (string.IsNullOrEmpty(path))
            {
                _log.Error(string.Format("Problem creating Path for the Entries file {0}.", path));
                MessageBox.Show(string.Format("Problem creating Path for the Entries file. {0}", EntryFileFullName));
            }
            else //path isnt blank
            {
                if (Directory.Exists(path))
                {
                    _log.Info(string.Format("Path already exists - {0}", path));
                }
                else
                {
                    // Try to create the directory.
                    try
                    {
                        Directory.CreateDirectory(path);
                        _log.Info(string.Format("The directory was created successfully at {0}", path));
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(string.Format("Problem creating Path for the Entries file. {0}", EntryFileFullName));
                        throw;
                    }
                    
                   
                }
            }

            CreateFile(EntryFileFullName);
            return XDocument.Load(EntryFileFullName);
        }

        private void CreateFile(string filePath)
        {
            _log.Info(string.Format("Creating File - {0}", filePath));
            new XDocument(new XElement("Entries")).Save(filePath);
        }

        private List<DiaryEntry> LoadList(string eventTypeFilter = "")
        {
            _log.Info(string.Format("Loading Entries - {0}", _xmlDoc.Descendants("DiaryEntry").Count()));

            IEnumerable<DiaryEntry> entries;

            //TODO: improve this so the filter doesnt have to be in the if statement

            if (string.IsNullOrEmpty(eventTypeFilter))
            {

                entries = from e in _xmlDoc.Descendants("DiaryEntry")
                    let id = e.Element("Id") where id != null
                    let entryDt = e.Element("EntryDt") where entryDt != null
                    let entryEvent = e.Element("EntryEvent") where entryEvent != null
                    let entryTxt = e.Element("EntryTxt") where entryTxt != null
                    let userName = e.Element("UserName") where userName != null
                    select (new DiaryEntry
                                {
                                    Id = Convert.ToInt32(id.Value),
                                    EntryDt = Convert.ToDateTime(entryDt.Value).ToLocalTime(),
                                    EntryEvent = entryEvent.Value,
                                    EntryTxt = entryTxt.Value,
                                    UserName = userName.Value
                                });

            }
            else
            {
                _log.Info(string.Format("Loading Entries with filter - {0}", eventTypeFilter));
                entries = from e in _xmlDoc.Descendants("DiaryEntry")
                    let xElement1 = e.Element("EntryEvent") where xElement1 != null
                    where xElement1.Value.ToLower() == eventTypeFilter.ToLower()
                    let id = e.Element("Id") where id != null
                    let entryDt = e.Element("EntryDt") where entryDt != null
                    let entryEvent = e.Element("EntryEvent") where entryEvent != null
                    let entryTxt = e.Element("EntryTxt") where entryTxt != null
                    let userName = e.Element("UserName") where userName != null
                    select (new DiaryEntry
                                {
                                    Id = Convert.ToInt32(id.Value),
                                    EntryDt = Convert.ToDateTime(entryDt.Value).ToLocalTime(),
                                    EntryEvent = entryEvent.Value,
                                    EntryTxt = entryTxt.Value,
                                    UserName = userName.Value
                                });

            }




            return entries.ToList();
        }

        private static string GetUser()
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            return windowsIdentity != null ? windowsIdentity.Name : string.Empty;
        }
    }
}