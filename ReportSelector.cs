
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TBDispl
{
    public partial class ReportSelector : Form
    {
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * 2017-03-03 - AjD - First version made live.                                                             *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        string whoAmI;
        private xList xMyProfile;
        string _DBName = "Common";
        Dictionary<string, List<string>> _lstReports;
        Dictionary<string, string> _lstReportTitles = new Dictionary<string, string>();
        List<TabPage> listTabs;
        List<ListBox> listLsts;

        /// <summary>
        /// pass in a dictionary where the parms are as follows:
        /// 1] the title of the tab or Database name
        ///    if the 2nd parm [item list] is null.
        /// 2] list of entries to appear in the tab,
        ///    optionally this list may be:
        ///    "{display-name}actual-report"
        ///    which may be an SQL statement.
        ///    
        /// NOTE: This does NOT check for the validity or existence of any tables or queries.
        /// </summary>
        /// <param name="ReportsList"></param>
        public ReportSelector(Dictionary<string, List<string>> ReportsList)
        {
            InitializeComponent();
            // these next two lines are commented out until
            // the code to turn them on when a report is actually
            // selected in any of the dynamically created tab
            // pages is added.
            this.btnDisplay.Enabled = false;
            this.btnExport.Enabled = false;

            _lstReports = ReportsList;
        }

        private void ReportSelector_Load(object sender, EventArgs e)
        {
            listTabs = new List<TabPage>();
            listLsts = new List<ListBox>();

            foreach (KeyValuePair<string, List<string>> newTabs in _lstReports)
            {
                if (newTabs.Value == null)
                {
                    _DBName = newTabs.Key;
                }
                else
                {
                    listTabs.Add(new TabPage());
                    listLsts.Add(new ListBox());
                    int idxTab = listTabs.Count - 1;

                    this.tabReportLists.Controls.Add(listTabs[idxTab]);

                    listTabs[idxTab].Controls.Add(listLsts[idxTab]);
                    listTabs[idxTab].Location = new System.Drawing.Point(4, 22);
                    listTabs[idxTab].Name = "tabReports" + idxTab.ToString();
                    listTabs[idxTab].Padding = tabPage1.Padding; // new System.Windows.Forms.Padding(3);
                    listTabs[idxTab].Size = tabPage1.Size; // new System.Drawing.Size(303, 313);
                    listTabs[idxTab].TabIndex = idxTab;
                    listTabs[idxTab].Text = newTabs.Key;
                    listTabs[idxTab].UseVisualStyleBackColor = true;

                    listLsts[idxTab].Anchor = (
                        (System.Windows.Forms.AnchorStyles)
                            (System.Windows.Forms.AnchorStyles.Top |
                             System.Windows.Forms.AnchorStyles.Bottom |
                             System.Windows.Forms.AnchorStyles.Left |
                             System.Windows.Forms.AnchorStyles.Right)
                        );
                    listLsts[idxTab].FormattingEnabled = true;
                    listLsts[idxTab].Location = new System.Drawing.Point(6, 6);
                    listLsts[idxTab].Name = "listBox";
                    int x = listTabs[idxTab].Size.Width - 12;
                    int y = listTabs[idxTab].Size.Height - 17;
                    listLsts[idxTab].Size = new System.Drawing.Size(x, y);
                    listLsts[idxTab].TabIndex = 0;
                    listLsts[idxTab].SelectedIndexChanged += new System.EventHandler(this.listBox_SelectedIndexChanged);

                    foreach (string item in newTabs.Value)
                    {
                        string itemText;
                        if (item.Substring(0, 1) == "{")
                        {
                            itemText = item.Substring(1, item.IndexOf("}") - 1);
                            _lstReportTitles.Add(idxTab.ToString("000") + "-" + itemText, item.Substring(item.IndexOf("}") + 1));
                        }
                        else
                        {
                            itemText = item;
                        }
                        listLsts[idxTab].Items.Add(itemText);
                    }
                }
            }

            whoAmI = System.Environment.UserName;
            xMyProfile = new xList("Prof-" + whoAmI, false, true, initValues: new string[] { "Name" }, dbName: _DBName);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDisplay_Click(object sender, EventArgs e)
        {
            if (((ListBox)tabReportLists.SelectedTab.Controls["listBox"]).SelectedIndex > 0)
            {
                string chosenReport = ((ListBox)tabReportLists.SelectedTab.Controls["listBox"]).SelectedItem.ToString();
                string idxTab = (tabReportLists.SelectedIndex - 1).ToString("000") + "-";
                if (_lstReportTitles.ContainsKey(idxTab + chosenReport))
                {
                    chosenReport = _lstReportTitles[idxTab + chosenReport];
                }
                ReportSuite dspyRpt = new ReportSuite(_DBName, chosenReport, null, null, null, 128);
                try
                {
                    dspyRpt.Show();
                }
                catch (Exception)
                {
                    MessageBox.Show("Report does not exist - please report to Application Maintenance.");
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)tabReportLists.SelectedTab.Controls["listBox"];
            if (lb.SelectedIndex > 0)
            {
                string chosenReport = lb.SelectedItem.ToString();
                string actualReport = chosenReport;
                string idxTab = (tabReportLists.SelectedIndex - 1).ToString("000") + "-";
                if (_lstReportTitles.ContainsKey(idxTab + chosenReport))
                {
                    actualReport = _lstReportTitles[idxTab + chosenReport];
                }

                string fileFull = xMyProfile.Parm("ReportSuiteFile");

                SaveFileDialog fileSelect = new SaveFileDialog();
                fileSelect.Title = "Navigate to destination folder for export";
                if (Path.IsPathRooted(fileFull))
                {
                    fileSelect.InitialDirectory = Path.GetDirectoryName(fileFull);
                    fileSelect.FileName = chosenReport;
                }
                fileSelect.DefaultExt = "csv";
                if (fileSelect.ShowDialog() == DialogResult.OK)
                {
                    // commented out as the filename is dependent on the report chosen and this will change a lot.
                    //xMyProfile.Parm("ReportSuiteFile", fileSelect.FileName);
                    //xMyProfile.Save();
                    fileFull = fileSelect.FileName;

                    Cursor.Current = Cursors.WaitCursor;

                    if (actualReport.Length < 6 || actualReport.Substring(0, 6).ToLower() != "select")
                    {
                        actualReport = "Select * from " + actualReport + ";";
                    }

                    adFns Defns = new adFns();
                    SqlConnection sqlCxn = new SqlConnection(Defns.cxn(_DBName));
                    sqlCxn.Open();
                    try
                    {
                        SqlCommand sqlCmd = new SqlCommand(actualReport, sqlCxn);
                        SqlDataReader sqlRdr = sqlCmd.ExecuteReader();
                        StreamWriter csvFile = new StreamWriter(Path.Combine(fileFull));

                        var csvColNames = Enumerable.Range(0, sqlRdr.FieldCount).Select(sqlRdr.GetName).ToList();
                        string csvRow = "";
                        foreach (string colTitle in csvColNames)
                        {
                            csvRow += (colTitle.Contains(" ") ? "\"" + colTitle + "\"" : colTitle) + ",";
                        }
                        csvRow = csvRow.Substring(0, csvRow.Length - 1);
                        csvFile.WriteLine(csvRow);

                        while (sqlRdr.Read())
                        {
                            csvRow = "";
                            for (int Lp1 = 0; Lp1 < sqlRdr.FieldCount; ++Lp1)
                            {
                                string value = sqlRdr[Lp1].ToString();
                                if (value.Contains(","))
                                    value = "\"" + value + "\"";
                                csvRow += value.Replace(Environment.NewLine, " ") + ",";
                            }
                            csvRow = csvRow.Substring(0, csvRow.Length - 1);
                            csvFile.WriteLine(csvRow);
                        }

                        sqlRdr.Close();

                        csvFile.Close();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Data Set either broken or does not exist", "File not created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    sqlCxn.Close();

                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool btnEnabled = ((ListBox)sender).SelectedIndex >= 0;
            this.btnDisplay.Enabled = btnEnabled;
            this.btnExport.Enabled = btnEnabled;
        }

        private void tabReportLists_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool btnEnabled = ((ListBox)(this.tabReportLists.SelectedTab.Controls["listBox"])).SelectedIndex >= 0;
            this.btnDisplay.Enabled = btnEnabled;
            this.btnExport.Enabled = btnEnabled;
        }
    }
}