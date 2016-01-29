using System.Linq;
using Diary.Controller;
using Diary.Model;
using Diary.Properties;
using log4net;
using System;
using System.Configuration;
using System.Reflection;
using System.Windows.Forms;


namespace Diary
{
    public partial class Form1 : Form
    {
        private NotifyIcon _trayIcon;
        private ContextMenu _trayMenu;
        private readonly DiaryController _dc;
        private readonly ILog _log;
        static readonly Timer SaveTimer = new Timer();
        private readonly bool _hideUserNameColumn = true;

        private string _filterText;

        public Form1(ILog log, DiaryController dc, int autoSaveInMinutes, bool hideUserNameColumn = true)
        {
            _log = log; //program level log file
            _dc = dc; //program level diary controller
            _hideUserNameColumn = hideUserNameColumn;
            InitializeComponent();
            ConfigureSysTray();
            ConfigureAutoSave(autoSaveInMinutes);
            ConfigureAutoComplete();
            dataGridView1.AutoGenerateColumns = false;
            LoadData();
        }

        private void ConfigureAutoComplete()
        {
            var autoComplete = new AutoCompleteStringCollection();
            autoComplete.AddRange(_dc.EntryEvents.ToArray());

            txtEventName.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtEventName.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtEventName.AutoCompleteCustomSource = autoComplete;

        }
        
        private void ConfigureAutoSave(int saveIntervalinMinutes)
        {
            _log.Info(string.Format("AutoSave Interval = {0}", saveIntervalinMinutes));
            SaveTimer.Interval = (saveIntervalinMinutes * 60 * 1000);
            /* Adds the event and the event handler for the method that will process the timer event to the timer. */
            SaveTimer.Tick += TimerEventProcessor;
            SaveTimer.Start();
        }

        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {

            if (string.IsNullOrEmpty(txtEntry.Text))
                return;

            _log.Info("AutoSaveing Entry");
            //Save current entry
            SaveEntry(txtEntry.Text, txtEventName.Text);
        }

        private void SaveEntry(string msg, string eventType = "")
        {
            _log.Info(string.Format("Saving Entry = {0} EventType = {1}", msg, eventType));
            if (!string.IsNullOrEmpty(msg))
            {
                _dc.AddEntry(msg, eventType);
            }

            ClearOldText();
        }

        private void ClearOldText()
        {
            txtEntry.Text = "";
            //reload form
            LoadData(_filterText);
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveEntry(txtEntry.Text, txtEventName.Text);
        }
        
        private void LoadData(string filter = "")
        {
            //dataGridView1.DataSource = null;
            //dataGridView1.Refresh();
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            dataGridView1.DataSource = _dc.GetDiaryEntries(filter);
            AddColumns();
            //dataGridView1.Refresh();
        }


        private void AddColumns()
        {

            DataGridViewColumn col = new DataGridViewTextBoxColumn();
            col.Width = 35;
            col.DataPropertyName = "Id";
            col.HeaderText = @"Id";
            dataGridView1.Columns.Add(col);

            DataGridViewColumn col2 = new DataGridViewTextBoxColumn();
            col2.Width = 120;
            col2.DataPropertyName = "EntryDt";
            col2.HeaderText = @"EntryDt";
            dataGridView1.Columns.Add(col2);

            DataGridViewColumn col3 = new DataGridViewTextBoxColumn();
            col3.Width = 80;
            col3.DataPropertyName = "EntryEvent";
            col3.HeaderText = @"EntryEvent";
            dataGridView1.Columns.Add(col3);

            DataGridViewColumn col4 = new DataGridViewTextBoxColumn();
            col4.MinimumWidth = (_hideUserNameColumn) ? 200 : 120;
            col4.DataPropertyName = "EntryTxt";
            col4.HeaderText = @"EntryTxt";
            dataGridView1.Columns.Add(col4);

            if (!_hideUserNameColumn)
            {
                DataGridViewColumn col5 = new DataGridViewTextBoxColumn();
                col5.Width = 80;
                col5.DataPropertyName = "UserName";
                col5.HeaderText = @"UserName";
                dataGridView1.Columns.Add(col5);
            }

        }


        #region systray

        private void ConfigureSysTray()
        {
            // Create a simple tray menu with only one item.
            _trayMenu = new ContextMenu();
            _trayMenu.MenuItems.Add("E&xit", OnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            _trayIcon = new NotifyIcon
            {
                Text = Assembly.GetExecutingAssembly().GetName().Name,
                Icon = Resources.AppIcon,
                ContextMenu = _trayMenu,
                Visible = true
            };

            // Add menu to tray icon and show it.

            _trayIcon.DoubleClick += trayIcon_DoubleClick;
            _trayIcon.Click += trayIcon_Click;
        }

        private void trayIcon_Click(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void trayIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowForm();
        }

        #endregion

        private void ShowForm()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
            BringToFront();
            Focus();
            txtEntry.Focus();
        }
        
        private void HideForm()
        {
            Hide();
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }
        
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveEntry(txtEntry.Text, txtEventName.Text);
            Application.Exit();
        }

     
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool exitonClose;
            if (!bool.TryParse(ConfigurationManager.AppSettings["ExitOnClose"], out exitonClose)) return;
            if (e.CloseReason != CloseReason.UserClosing) return;
            if (exitonClose) return;

            e.Cancel = true; //I'm sorry Dave, I'm afraid I can't do that.
            HideForm();
        }

        private void clearAllToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var dialogResult = MessageBox.Show(Resources.MSG_ArchiveFile, Resources.M_ArchiveQuestionTitle, MessageBoxButtons.YesNo);
            switch (dialogResult)
            {
                case DialogResult.Yes:
                    _dc.ArchiveFile();
                    LoadData(_filterText);
                    break;
                case DialogResult.No:
                    break;
            }
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            _filterText = txtEventName.Text;

            LoadData(_filterText);
        }
        
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //var editedCell = this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
            //var newValue = editedCell.Value.ToString();

            var row = dataGridView1.Rows[e.RowIndex];
            var de = row.DataBoundItem as DiaryEntry;
                if (de != null)
                {
                  _dc.UpdateEntry(de);
                }
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadData(_filterText);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var box = new AboutBox();
            box.ShowDialog();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != Keys.Escape) return base.ProcessCmdKey(ref msg, keyData);
            Hide();
            return true;
        }

     
    }
}