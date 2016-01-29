using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Diary.Model
{
    [Serializable()]
    public class DiaryEntry
    {
        public int Id { get; set; }
        public string EntryEvent { get; set; }
        public string EntryTxt { get; set; }
        public string UserName { get; set; }

        private DateTime _entryDt;
        public DateTime EntryDt
        {
            get {return _entryDt.ToLocalTime(); }
            set { _entryDt = value.ToUniversalTime(); }
        }

       

        
    }
    

    
}
