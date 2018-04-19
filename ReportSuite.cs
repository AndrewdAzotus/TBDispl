
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TBDispl
{
    public partial class ReportSuite : Form
    {
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * 2016-03-23 - AjD - First version started                                                                *
         * 2016-03-24 - AjD - Completed beta version, demonstrated then genericised for use within any program     *
         * 2016-03-28 - AjD - Added useful tick boxes in field selection, only those fields selected will be       *
         *                    displayed in the data grid.                                                          *
         * 2016-03-29 - AjD - Added an Export button that exports the displayed datagrid to a CSV file. The user   *
         *                    is prompted for the location and filename. This location/file name is stored in an   *
         *                    xList table called 'Prof-<userid>' on the local database                             *
         * 2016-04-29 - AjD - When displaying filters based on existing filters to be included they were not being *
         *                    included in the filter window with ticks hence when a refresh was requested the      *
         *                    existing filters were not included. They are now ticked and, thus, included          *
         * 2016-05-02 - AjD - The filter control is now disabled unless a table column is selected                 *
         * 2016-05-03 - AjD - Version indicator added to form title bar                                            *
         * 2016-05-03 - AjD - selecting the range fld to add a new filter now selects all text for easy over-write *
         * 2016-05-09 - AjD - The filters box is now sorted in ascending order                                     *
         * 2016-05-09 - AjD - There was an error where nulls were being written to the filter list which caused    *
         *                    errors elsewhere.                                                                    *
         * 2016-09-01 - AjD - The cursor is now set to the hourglass whilst the user copies data from the grid to  *
         *                    the clipboards.                                                                      *
         * 2017-02-03 - AjD - If the column names do not match exactly [including case] then ARS crashed badly.    *
         *                    Column names are now checked for order and filter parameters and ignored if wrong.   *
         * 2017-03-03 - AjD - An error was found where if a table has spaces in the column names then the program  *
         *                    broke and the table of data was not displayed. This was corrected by adding brackets *
         *                    around column names.                                                                 *
         * 2017-03-03 - AjD - New functionality added allowing a list of reports to be passed in for the user to   *
         *                    select from and display. See 'Report Selector' for its change history.               *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
        private const String vrm = " v.2.01.005";

        private adFns Defns = new adFns();
        private String whoAmI;
        bool debug;

        private string sqlTxt;
        private SqlConnection sqlCxn;
        private SqlCommand sqlCmd;
        private SqlDataReader sqlRdr;

        // _ at the start of the variable name indicates these are initialised but should then remain constant.
        private String _dbName;
        private String _tblName;
        private Int32 _maxDisplayFilterChoices;
        private const String _ARSdbName = "AutomationReportingSuite";

        private xList xMyProfile;

        private Dictionary<String, Boolean> colListMstr;
        private SortedDictionary<string, Type> colsList;

        private List<string> colList;
        private SortedList<string, List<string>> colFltrs;
        private String colFltrName;
        private SortedList<string, string> colOrdering;
        private List<string> colOrders;

        private DataTable tblDetails;

        /// <summary>
        /// Designed to make a simple database/table display with user-save-able selection criteria.
        /// </summary>
        /// <param name="dbName">Name of Database to extract data from</param>
        /// <param name="tblName">Name of Table to extract data from</param>
        /// <param name="colSelected">[Optional] list of columns selected to display if not all columns</param>
        /// <param name="colOrderBy">[Optional] list of columns to sort on with Asc/Desc flag in the order to sort by</param>
        /// <param name="colFilters">[Optional] list of columns to filter on with sub-list of filter criteria</param>
        /// <param name="maxDisplayFilterChoices">[Optional] limit of filter criteria to display. Specify to prevent criteria list becoming too large when criteria are built from column data</param>
        public ReportSuite(string dbName, string tblName
                         , List<String> colSelected = null
                         , Dictionary<String, String> colOrderBy = null
                         , SortedList<String, List<String>> colFilters = null
                         , Int32 maxDisplayFilterChoices = 32
                         )
        {
            InitializeComponent();
            this.Text += vrm;

            whoAmI = System.Environment.UserName;
            xMyProfile = new xList("Prof-" + whoAmI, false, true, initValues: new string[] { "Name" }, dbName: _ARSdbName);
            debug = false && (whoAmI.ToUpper() == "XXB7CVH");

            _tblName = tblName;
            _dbName = dbName;
            _maxDisplayFilterChoices = maxDisplayFilterChoices;
            sqlCxn = new SqlConnection(Defns.cxn(_dbName));

            sqlCxn.Open();
            bldColumnList(colSelected);

            colFltrs = new SortedList<string, List<string>>();
            colOrdering = new SortedList<string, string>();
            colOrders = new List<string>();

            if (colOrderBy != null)
            {
                foreach (KeyValuePair<string, string> kvp in colOrderBy)
                {
                    colOrdering.Add(kvp.Key, kvp.Value);
                    colOrders.Add(kvp.Key);
                }
            }

            if (colFilters != null) colFltrs = colFilters;
            clbFltrs.Items.Clear();
            clbFltrs.Items.Add(adFns.LeftBrkt + "Select a column..." + adFns.RghtBrkt);

            // [+] load any saved queries
            this.cmbReportList.Items.Clear();
            this.cmbReportList.Items.Add(adFns.LeftBrkt + "Saved Queries" + adFns.RghtBrkt);
            sqlTxt = "Select * from AutomationReportingSuite.dbo.ReportList Where Owner=@W or AnyUser='True' and dbName=@D;"; //  and tblName=@T
            sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
            sqlCmd.Parameters.AddWithValue("@D", _dbName);
            //sqlCmd.Parameters.AddWithValue("@T", _tblName);
            sqlCmd.Parameters.AddWithValue("@W", whoAmI);
            sqlRdr = sqlCmd.ExecuteReader();
            while (sqlRdr.Read())
            {
                string qryName = sqlRdr["Descr"].ToString() + " " + adFns.LeftBrkt + sqlRdr["Owner"].ToString().Trim() + adFns.RghtBrkt;
                if (Convert.ToBoolean(sqlRdr["AnyUser"])) qryName += " [Public]";
                this.cmbReportList.Items.Add(qryName);
            }
            sqlRdr.Close();
            this.cmbReportList.SelectedIndex = 0;

            sqlCxn.Close();
        }

        private void ReportSuite_Load(object sender, EventArgs e)
        {
            if (colsList.Count > 0)
            {
                bldFldsList(-1);
                bldDisplayTable();
            }
            else throw (new Exception("ff")) ;
        }

        // Buttons <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAddFltr_Click(object sender, EventArgs e)
        {
            string rangeValue = txtRange.Text;
            string colName = colFltrName;
            if (colName.Contains("[")) colName = colName.Substring(0, colName.IndexOf(" ["));
            if (colName.Contains("{")) colName = colName.Substring(0, colName.IndexOf(" {"));

            if (colsList[colName] != typeof(int))
            {
                if (rangeValue.Substring(0, 1) != "'") rangeValue = "'" + rangeValue;
                if (rangeValue.Substring(rangeValue.Length - 1) != "'") rangeValue += "'";
            }
            if (clbFltrs.SelectedIndex < 1)
            {
                clbFltrs.Items.Add(rangeValue, true);
                if (colFltrs.ContainsKey(colName))
                {
                    colFltrs[colName].Add(rangeValue);
                }
                else
                {
                    colFltrs.Add(colName, new List<string> { rangeValue });
                }
            }
            else
            {
                clbFltrs.Items[clbFltrs.SelectedIndex] = rangeValue;
                colFltrs[colName][this.clbFltrs.SelectedIndex - 1] = rangeValue;
            }
            bldFldsList(colFltrName);

        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (this.dgvData.SelectedCells.Count == 0)
                this.dgvData.SelectAll();
            Clipboard.SetDataObject(this.dgvData.GetClipboardContent());
            Cursor.Current = Cursors.Default;
        }

        private void btnRangeStart_Click(object sender, EventArgs e)
        {
            String him = this.clbFltrs.SelectedItem.ToString();
            Int32 idx = colFltrs[colFltrName].IndexOf(him);
            colFltrs[colFltrName][idx] += " ..";
            this.clbFltrs.Items[this.clbFltrs.SelectedIndex] += " ..";
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            bldDisplayTable();
        }

        private void srtNone_CheckedChanged(object sender, EventArgs e)
        {
            if (this.srtNone.Focused)
            {
                string colName = colFltrName;
                if (colName.Contains("[")) colName = colName.Substring(0, colName.IndexOf(" ["));
                if (colName.Contains("{")) colName = colName.Substring(0, colName.IndexOf(" {"));
                if (colOrdering.ContainsKey(colName))
                {
                    colOrders.Remove(colName);
                    colOrdering.Remove(colName);
                }
                bldFldsList(colFltrName);
            }
        }

        // Control event related <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        private void cbxFullTableFltrs_CheckedChanged(object sender, EventArgs e)
        {
            bldFltrList(colFltrName);
        }

        private void clbFlds_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.srtAsc.Enabled = (this.clbFlds.SelectedIndex >= 0);
            this.srtDesc.Enabled = (this.clbFlds.SelectedIndex >= 0);
            this.srtNone.Enabled = (this.clbFlds.SelectedIndex >= 0);
            this.clbFltrs.Enabled = (this.clbFlds.SelectedIndex >= 0);
            if (this.clbFlds.Focused)
            {
                this.btnRangeStart.Enabled = false;
                string whoAmI = System.Environment.UserName.ToUpper(); // for debug stuff

                string colName = clbFlds.SelectedItem.ToString();
                if (colName.Contains("[")) colName = colName.Substring(0, colName.IndexOf(" ["));
                if (colName.Contains("{")) colName = colName.Substring(0, colName.IndexOf(" {"));
                colFltrName = colName;

                if (cbxColSel.CheckState == CheckState.Unchecked)
                {
                    bldFltrList(colName);
                }

                bool OnOff = (clbFlds.SelectedIndex >= 0);
                this.srtAsc.Enabled = OnOff;
                this.srtDesc.Enabled = OnOff;
                this.srtNone.Enabled = OnOff;
                if (colOrdering.ContainsKey(colName))
                {
                    switch (colOrdering[colName])
                    {
                        case "Asc":
                            this.srtAsc.Checked = true;
                            break;
                        case "Desc":
                            this.srtDesc.Checked = true;
                            break;
                    }
                }
                else
                {
                    this.srtNone.Checked = true;
                }
            }
        }

        private void clbRanges_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (clbFltrs.Focused && this.clbFltrs.SelectedIndex >= 0)
            {
                this.txtRange.Text = this.clbFltrs.SelectedItem.ToString();
                this.btnRangeStart.Enabled = (this.clbFltrs.SelectedIndex >= 0);
            }
        }

        private void txtRange_TextChanged(object sender, EventArgs e)
        {
            btnAddFltr.Enabled = (this.txtRange.Text.Length > 0);
        }

        // Miscellaneous <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        private void bldColumnList(List<string> colSelected)
        {
            colListMstr = new Dictionary<string, bool>();
            colsList = new SortedDictionary<string, Type>();

            if (_tblName.Length > 6 && _tblName.Substring(0, 6).ToLower() == "select")
            {
                sqlTxt = "Select Top 1 " + _tblName.Substring(7);
            }
            else
                sqlTxt = "Select Top 1 * from " + _tblName + ";";
            sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
            try
            {
                sqlRdr = sqlCmd.ExecuteReader();
            }
            catch (Exception)
            {
                Close();
                //throw new Exception("Invalid query or table");
            }

            if (sqlRdr != null)
            {
                for (int Lp1 = 0; Lp1 < sqlRdr.FieldCount; Lp1++)
                {
                    string colName = sqlRdr.GetName(Lp1).ToString();
                    if (colName.Contains("[")) colName = colName.Substring(0, colName.IndexOf(" ["));
                    if (colName.Contains("{")) colName = colName.Substring(0, colName.IndexOf(" {"));
                    Boolean displ = true;
                    if (colSelected != null && colSelected.Count > 0) displ = colSelected.Contains(colName);
                    colListMstr.Add(colName, displ);
                    colsList.Add(colName, sqlRdr.GetFieldType(Lp1)); // in case there are spaces or similar in the colname
                }
                sqlRdr.Close();
            }
        }

        private void bldDataGrid()
        {
            if (debug) MessageBox.Show("{5a}");
            tblDetails = new DataTable();
            colList.Clear();

            foreach (KeyValuePair<String, Boolean> colm in colListMstr)
            {
                string colName = colm.Key;
                if (debug) MessageBox.Show("{5b-" + colName + "}");

                if (colName.Contains("[")) colName = colName.Substring(0, colName.IndexOf(" ["));
                if (colName.Contains("{")) colName = colName.Substring(0, colName.IndexOf(" {"));
                string colXtra = "";
                //if (debug) MessageBox.Show("{5c-" + colName + "}");

                if (colFltrs.ContainsKey(colName) || colFltrs.ContainsKey("[" + colName + "]")) colXtra += " [Fltr]";
                if (colOrdering.ContainsKey(colName)) colXtra += " {" + colOrdering[colName] + "-" + (colOrders.IndexOf(colName) + 1).ToString() + "}";
                colList.Add(colName + colXtra);
                //if (debug) MessageBox.Show("{5d-" + colName + @"/" + " " + "}");

                if (colListMstr.ContainsKey(colName) && colListMstr[colName] && colsList.ContainsKey(colName)) tblDetails.Columns.Add(colName, colsList[colName]);
            }
            if (debug) MessageBox.Show("{5z}");
        }

        private void bldFldsList(int preSelected)
        {
            if (debug) MessageBox.Show("{4a}");
            colList = new List<string>();
            if (debug) MessageBox.Show("{4b}");
            bldDataGrid();
            if (debug) MessageBox.Show("{4c}");
            clbFlds.Items.Clear();
            if (debug) MessageBox.Show("{4d}");
            foreach (KeyValuePair<String, Boolean> colm in colListMstr)
            {
                string colXtra = "";
                if (colFltrs.ContainsKey(colm.Key) || colFltrs.ContainsKey("[" + colm.Key + "]")) colXtra += " [Fltr]";
                if (colOrdering.ContainsKey(colm.Key)) colXtra += " {" + colOrdering[colm.Key] + "-" + (colOrders.IndexOf(colm.Key) + 1).ToString() + "}";
                clbFlds.Items.Add(colm.Key + colXtra, colm.Value);
                if (debug) MessageBox.Show(colm.Key);
            }
            if (debug) MessageBox.Show("{4z}");
        }
        private void bldFldsList(String preSelected)
        {
            colList = new List<string>();
            bldDataGrid();
            clbFlds.Items.Clear();

            foreach (String colName in colList)
            {
                string colXtra = "";
                if (colFltrs.ContainsKey(colName) || colFltrs.ContainsKey("[" + colName + "]")) colXtra += " [Fltr]";
                if (colOrdering.ContainsKey(colName)) colXtra += " {" + colOrdering[colName] + "-" + (colOrders.IndexOf(colName) + 1).ToString() + "}";
                clbFlds.Items.Add(colName + colXtra, true);
                if ((colName.Contains(" ") ? colName.Substring(0, colName.IndexOf(" ")) : colName) == preSelected)
                    preSelected = colName;
            }
            clbFlds.SelectedItem = preSelected;
        }

        private void bldFltrList(String colName)
        {
            Cursor.Current = Cursors.WaitCursor;
            clbFltrs.Items.Clear();
            string colStuff = "[" + colName + "]";
            if (colsList[colName] == typeof(System.DateTime))
            {
                colStuff = "cast([" + colName + "] as date)";
            }
            sqlTxt = "Select Count(Distinct " + colStuff + ") as ctr from " + _tblName;
            if (cbxFullTableFltrs.Checked) sqlTxt += bldWhereClause();
            sqlTxt += ";";
            sqlCxn.Open();
            sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
            int rc = (Int32)sqlCmd.ExecuteScalar();
            if (!cbxFullTableFltrs.Checked && rc > _maxDisplayFilterChoices)
            {
                clbFltrs.Items.Add(adFns.LeftBrkt + "Too many choices to auto-list" + adFns.RghtBrkt);
                if (colFltrs.ContainsKey(colName))
                {
                    foreach (string colRange in colFltrs[colName])
                    {
                        if (colRange.Contains(" Between "))
                        {
                            clbFltrs.Items.Add(colRange.Substring(9, colRange.Length - 13) + " ...", true);
                        }
                        else
                            clbFltrs.Items.Add(colRange, true);
                    }
                }
                this.txtRange.Text = "";
            }
            else
            {
                sqlTxt = "Select Distinct " + colStuff + " as colm from " + _tblName;
                if (cbxFullTableFltrs.Checked) sqlTxt += bldWhereClause() + " and "; else sqlTxt += " Where ";
                sqlTxt += colStuff + " is not null;";
                sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                sqlRdr = sqlCmd.ExecuteReader();
                while (sqlRdr.Read())
                {
                    String fltrOption = sqlRdr["colm"].ToString();
                    Boolean chkd = false;
                    if (colFltrs.ContainsKey(colName))
                    {
                        chkd = colFltrs[colName].Contains("'" + sqlRdr["colm"] + "'") ||
                               colFltrs[colName].Contains("'" + sqlRdr["colm"] + " ..'") ||
                               colFltrs[colName].Contains("" + sqlRdr["colm"] + "") ||
                               colFltrs[colName].Contains("" + sqlRdr["colm"] + " ..");
                        if (colFltrs[colName].Contains("'" + sqlRdr["colm"] + "'")) { chkd = true; fltrOption = "'" + sqlRdr["colm"] + "'"; }
                        if (colFltrs[colName].Contains("'" + sqlRdr["colm"] + " ..'")) { chkd = true; fltrOption = "'" + sqlRdr["colm"] + " ..'"; }
                        if (colFltrs[colName].Contains("" + sqlRdr["colm"] + "")) { chkd = true; fltrOption = "" + sqlRdr["colm"] + ""; }
                        if (colFltrs[colName].Contains("" + sqlRdr["colm"] + " ..")) { chkd = true; fltrOption = "" + sqlRdr["colm"] + " .."; }
                    }
                    // clbFltrs.Items.Add(sqlRdr["colm"], chkd); // if a .ToString() is used then dates have times included AND I DON'T WANT THIS. So There!
                    if (fltrOption != "") clbFltrs.Items.Add(fltrOption, chkd);
                }
                sqlRdr.Close();
            }
            sqlCxn.Close();

            if (clbFltrs.Items.Count == 0) { clbFltrs.Items.Add(adFns.LeftBrkt + "No Values to Display" + adFns.RghtBrkt); }
            Cursor.Current = Cursors.Default;
        }

        // To Sort <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        private void bldDisplayTable()
        {
            if (debug) MessageBox.Show("{1}");
            bldDataGrid();
            tblDetails.Rows.Clear();
            sqlCxn.Open();
            Int32 timeWarn = 120;
            Int32 timeStop = 300;
            TimeSpan tspn = new TimeSpan();
            DateTime startedAt = DateTime.Now;

            //sqlTxt = "Select Distinct Count(*) from " + _tblName + " " + bldWhereClause() + ";";
            //sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
            //Int32 rc = (Int32)sqlCmd.ExecuteScalar();
            // if (rc < 128000 || (rc > 128000 && MessageBox.Show("This is going to take a frightfully long time", "Are you sure", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)) 
            {
                Cursor.Current = Cursors.WaitCursor;

                if (_tblName.Length > 6 && _tblName.Substring(0, 6).ToLower() == "select")
                {
                    sqlTxt = _tblName;
                    this.cbxColSel.Enabled = false;
                    this.btnSelAll.Enabled = false;
                    this.btnDesAll.Enabled = false;
                    this.clbFlds.Enabled = false;
                    this.clbFltrs.Enabled = false;
                    this.cbxFullTableFltrs.Enabled = false;
                    this.txtRange.Enabled = false;
                    this.btnRangeStart.Enabled = false;
                    this.btnAddFltr.Enabled = false;
                }
                else
                {
                    sqlTxt = "Select Distinct ";
                    foreach (String colm in colList)
                    {
                        string colName = colm;
                        if (colName.Contains("[")) colName = colName.Substring(0, colName.IndexOf(" ["));
                        if (colName.Contains("{")) colName = colName.Substring(0, colName.IndexOf(" {"));
                        //if (colListMstr.ContainsKey(colName) && colListMstr[colName])
                        //    sqlTxt += "[" + (colName.Contains(" ") ? colName.Substring(0, colName.IndexOf(" ")) : colName) + "], ";
                        if (colListMstr.ContainsKey(colName) && colListMstr[colName])
                            sqlTxt += "[" + colName + "], ";
                    }
                    string sqlTxt2 = "";
                    foreach (KeyValuePair<string, bool> col in colListMstr)
                    {
                        if (col.Value)
                            sqlTxt2 += "[" + col.Key + "], ";
                    }
                    if (debug) MessageBox.Show(sqlTxt + Environment.NewLine + Environment.NewLine + sqlTxt2);
                    sqlTxt = sqlTxt.Substring(0, sqlTxt.Length - 2) + " from " + _tblName;

                    sqlTxt += bldWhereClause();

                    if (colOrders.Count > 0)
                    { // to ensure the table is sorted by the columns in the correct order.
                        String OrderClause = " Order by";
                        foreach (string colOrder in colOrders)
                        {
                            if (colListMstr.ContainsKey(colOrder) && colListMstr[colOrder])
                            {
                                OrderClause += " [" + colOrder + "] " + colOrdering[colOrder] + ",";
                            }
                        }
                        if (OrderClause.Length > 9)
                        {
                            OrderClause = OrderClause.Substring(0, OrderClause.Length - 1);
                            sqlTxt += OrderClause;
                        }
                    }
                    sqlTxt += ";";
                }

                sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                sqlRdr = sqlCmd.ExecuteReader();
                while (sqlRdr.Read())
                {
                    DataRow dr = tblDetails.NewRow();
                    foreach (String colm in colList)
                    {
                        string colName = colm;
                        if (colName.Contains("[")) colName = colName.Substring(0, colName.IndexOf(" ["));
                        if (colName.Contains("{")) colName = colName.Substring(0, colName.IndexOf(" {"));
                        if (colListMstr[colName])
                        {
                            dr[colName] = sqlRdr[colName];
                        }
                    }
                    tblDetails.Rows.Add(dr);

                    tspn = DateTime.Now - startedAt;
                    if (tspn.TotalSeconds > timeWarn)
                    {
                        if (MessageBox.Show("Your query is taking a long time\n\nYou are advised that you should amend your search parameters to reduce the results.\n\nDo you wish to quit and try again?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            sqlCmd.Cancel();
                            break;
                        }
                        timeStop += (int)tspn.TotalSeconds;
                        timeWarn = timeStop + 12;
                    }
                    if (tspn.TotalSeconds > timeStop)
                    {
                        MessageBox.Show("This is taking too long");
                        sqlCmd.Cancel(); break;
                    }
                }
                sqlRdr.Close();
                this.lblGridSize.Text = tblDetails.Rows.Count + " rows found.";
                dgvData.DataSource = null;
                dgvData.DataSource = tblDetails;
                Cursor.Current = Cursors.Default;
            }
            sqlCxn.Close();
        }

        private String bldWhereClause()
        {
            string whereClause = "";

            if (colFltrs.Count > 0)
            {
                whereClause += " Where";
                foreach (KeyValuePair<string, List<string>> colSelect in colFltrs)
                {
                    string him = colSelect.Key;
                    whereClause += " (";
                    foreach (string rangeValue in colSelect.Value)
                    {
                        if (rangeValue.Length > 3 && rangeValue.Substring(rangeValue.Length - 3) == " ..")
                        { // this is deliberately not an elipsis as Word translates three '.' characters into a special single elipsis character.
                            whereClause += "[" + colSelect.Key + "] Between " + rangeValue.Substring(0, rangeValue.Length - 3) + " and ";
                        }
                        else
                        {
                            if (whereClause.Substring(whereClause.Length - 5) != " and ")
                            {
                                if (whereClause.Substring(whereClause.Length - 2) != " (")
                                {
                                    whereClause += " or ";
                                }
                                whereClause += "[" + colSelect.Key + "]";
                                if (!rangeValue.Contains(" Between "))
                                {
                                    whereClause += "=";
                                }
                            }
                            whereClause += rangeValue;
                        }
                    }
                    whereClause += ") and";
                }
            }
            if (whereClause.Length > 0) whereClause = whereClause.Substring(0, whereClause.Length - 4);

            return whereClause;
        }

        private void srtAsc_CheckedChanged(object sender, EventArgs e)
        {
            if (this.srtAsc.Focused) sortChanged("Asc");
        }

        private void srtDesc_CheckedChanged(object sender, EventArgs e)
        {
            if (this.srtDesc.Focused) sortChanged("Desc");
        }

        private void sortChanged(string order)
        {
            if (colFltrName != null)
            {
                string colName = colFltrName;
                if (colName.Contains("[")) colName = colName.Substring(0, colName.IndexOf(" ["));
                if (colName.Contains("{")) colName = colName.Substring(0, colName.IndexOf(" {"));
                if (colOrdering.ContainsKey(colName))
                    colOrdering[colName] = order;
                else
                {
                    colOrdering.Add(colName, order);
                    colOrders.Add(colName);
                }
                bldFldsList(colFltrName);
            }
        }

        private void clbFltrs_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (this.clbFltrs.Focused)
            {
                int him = clbFltrs.CheckedItems.Count + (e.NewValue == CheckState.Checked ? 1 : 0);
                int jim = e.Index;
                if (clbFltrs.CheckedItems.Count > 0 && e.NewValue == CheckState.Checked)
                {
                    if (e.Index >= clbFltrs.Items.IndexOf(clbFltrs.CheckedItems[clbFltrs.CheckedItems.Count - 1])) btnRangeStart.Enabled = false;
                    else btnRangeStart.Enabled = true;
                    btnRangeStart.Enabled = (e.Index < clbFltrs.Items.IndexOf(clbFltrs.CheckedItems[clbFltrs.CheckedItems.Count - 1])); // because I would sooner be super clever!
                }
                else this.btnRangeStart.Enabled = false;
                string colName = colFltrName;
                if (colName.Contains("[")) colName = colName.Substring(0, colName.IndexOf(" ["));
                if (colName.Contains("{")) colName = colName.Substring(0, colName.IndexOf(" {"));
                string rangeValue = this.clbFltrs.SelectedItem.ToString();
                if (colsList[colName] != typeof(int))
                {
                    if (rangeValue.Substring(0, 1) != "'") rangeValue = "'" + rangeValue;
                    if (rangeValue.Substring(rangeValue.Length - 1) != "'") rangeValue += "'";
                }

                if (e.NewValue == CheckState.Checked)
                {
                    if (colFltrs.ContainsKey(colName))
                    {
                        // cannot do it this way as they need to be in the same order as displayed, not the same order as the user clicked on them and a sorted list sorts 10 before 5!
                        // if (!colFltrs[colName].Contains(rangeValue)) colFltrs[colName].Add(rangeValue); 
                        colFltrs[colName].Clear();
                        foreach (object itm in this.clbFltrs.Items)
                        {
                            string itmValue = itm.ToString();
                            if (itmValue.Substring(0, 1) != "'") itmValue = "'" + itmValue;
                            if (itmValue.Substring(itmValue.Length - 1) != "'") itmValue += "'";
                            if (this.clbFltrs.Items.IndexOf(itm) == e.Index || this.clbFltrs.CheckedItems.Contains(itm))
                            {
                                colFltrs[colName].Add(itmValue);
                            }
                        }
                    }
                    else
                    {
                        colFltrs.Add(colName, new List<string> { rangeValue });
                        bldFldsList(this.clbFlds.SelectedIndex);
                    }
                }
                else
                {
                    if (this.clbFltrs.SelectedItem.ToString().Length > 3 && this.clbFltrs.SelectedItem.ToString().Substring(this.clbFltrs.SelectedItem.ToString().Length - 3) == " ..")
                        this.clbFltrs.SelectedItem = this.clbFltrs.SelectedItem.ToString().Substring(0, this.clbFltrs.SelectedItem.ToString().Length - 3);
                    if (colFltrs.ContainsKey(colName) && colFltrs[colName].Count > 1)
                    {
                        colFltrs[colName].Remove(rangeValue);
                    }
                    else
                    {
                        colFltrs.Remove(colName);
                    }
                    if (rangeValue.Length > 3 && rangeValue.Substring(rangeValue.Length - 3) == " ..") this.clbFltrs.Items[this.clbFltrs.SelectedIndex] = rangeValue.Substring(0, rangeValue.Length - 3);
                }
                bldFldsList(colFltrName);
                this.btnRefresh.Focus();
            }
        }

        private void btnSelAll_Click(object sender, EventArgs e)
        {
            for (int Lp1 = 0; Lp1 < clbFlds.Items.Count; Lp1++)
            {
                clbFlds.SetItemChecked(Lp1, true);
                String colName = clbFlds.Items[Lp1].ToString();
                if (colName.Contains("[")) colName = colName.Substring(0, colName.IndexOf(" ["));
                if (colName.Contains("{")) colName = colName.Substring(0, colName.IndexOf(" {"));
                colListMstr[colName] = true;
            }
            this.btnRefresh.Enabled = true;
        }

        private void btnDesAll_Click(object sender, EventArgs e)
        {
            for (int Lp1 = 0; Lp1 < clbFlds.Items.Count; Lp1++)
            {
                clbFlds.SetItemChecked(Lp1, false);
                String colName = clbFlds.Items[Lp1].ToString();
                if (colName.Contains("[")) colName = colName.Substring(0, colName.IndexOf(" ["));
                if (colName.Contains("{")) colName = colName.Substring(0, colName.IndexOf(" {"));
                colListMstr[colName] = false;
            }
            this.btnRefresh.Enabled = false;
        }

        private void cbxColSel_CheckedChanged(object sender, EventArgs e)
        {
            clbFlds.CheckOnClick = (cbxColSel.Checked);
            this.btnSelAll.Enabled = (cbxColSel.Checked);
            this.btnDesAll.Enabled = (cbxColSel.Checked);
            srtAsc.Enabled = !(cbxColSel.Checked);
            srtDesc.Enabled = !(cbxColSel.Checked);
            srtNone.Enabled = !(cbxColSel.Checked);
            clbFltrs.Enabled = (!cbxColSel.Checked);
            cbxFullTableFltrs.Enabled = !(cbxColSel.Checked);
            txtRange.Enabled = !(cbxColSel.Checked);
            btnAddFltr.Enabled = !(cbxColSel.Checked);
        }

        private void clbFlds_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (clbFlds.Focused)
            {
                if (!cbxColSel.Checked) e.NewValue = e.CurrentValue;
                else
                {
                    string colName = clbFlds.SelectedItem.ToString();
                    if (colName.Contains("[")) colName = colName.Substring(0, colName.IndexOf(" ["));
                    if (colName.Contains("{")) colName = colName.Substring(0, colName.IndexOf(" {"));
                    colListMstr[colName] = (e.NewValue == CheckState.Checked);
                }
                this.btnRefresh.Enabled = ((this.clbFlds.CheckedItems.Count > 0 && e.NewValue != CheckState.Unchecked) ||
                    (this.clbFlds.CheckedItems.Count == 0 && e.NewValue == CheckState.Checked)
                    || this.clbFlds.CheckedItems.Count > 1);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            String filePath = "";
            String fileName = "";
            String fileFull = "";

            fileFull = xMyProfile.Parm("ReportSuiteFile");

            SaveFileDialog fileSelect = new SaveFileDialog();
            fileSelect.Title = "Navigate to destination folder for export";
            if (Path.IsPathRooted(fileFull))
            {
                fileSelect.InitialDirectory = Path.GetDirectoryName(fileFull);
                fileSelect.FileName = Path.GetFileName(fileFull);
            }
            fileSelect.DefaultExt = "csv";
            if (fileSelect.ShowDialog() == DialogResult.OK)
            {
                filePath = Path.GetDirectoryName(fileSelect.FileName);
                fileName = Path.GetFileName(fileSelect.FileName);
                xMyProfile.Parm("ReportSuiteFile", fileSelect.FileName);
                xMyProfile.Save();
                fileFull = fileSelect.FileName;

                Cursor.Current = Cursors.WaitCursor;

                // copied [stolen] from http://www.codeproject.com/Tips/591034/Simplest-code-to-export-a-datatable-into-csv-forma
                var lines = new List<string>();

                String[] colNames = tblDetails.Columns.Cast<DataColumn>().Select(colm => colm.ColumnName).ToArray();
                // var hdr = String.Join(",", colNames);
                lines.Add(String.Join(",", colNames));

                var valLines = tblDetails.AsEnumerable().Select(rw => string.Join(",", rw.ItemArray));
                lines.AddRange(valLines);

                File.WriteAllLines(fileFull, lines);
                Cursor.Current = Cursors.Default;
            }
        }

        private void cmbReportList_SelectedIndexChanged(object sender, EventArgs e)
        {   // [+] load a query
            if (this.cmbReportList.SelectedIndex > 0)
            {
                String qryName = this.cmbReportList.Text;
                String qryOwner;
                Int32 idxReport = 0;
                String tblName = "";

                qryOwner = qryName.Substring(qryName.IndexOf(adFns.LeftBrkt) + 1);
                qryOwner = qryOwner.Substring(0, qryOwner.Length - 1);
                qryName = qryName.Substring(0, qryName.IndexOf(adFns.LeftBrkt) - 1);

                colListMstr.Clear();
                colOrders.Clear();
                colOrdering.Clear();
                colFltrs.Clear();

                sqlCxn.Open();
                sqlTxt = "Select * from AutomationReportingSuite.dbo.ReportList Where Descr=@D and Owner=@O and dbName=@DB;";
                sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                sqlCmd.Parameters.AddWithValue("@D", qryName);
                sqlCmd.Parameters.AddWithValue("@O", qryOwner);
                sqlCmd.Parameters.AddWithValue("@DB", _dbName);
                sqlRdr = sqlCmd.ExecuteReader();
                if (sqlRdr.Read())
                { // because, in theory, there will only ever and always be one row.
                    idxReport = (int)sqlRdr["Idx"];
                    tblName = sqlRdr["tblName"].ToString();
                }
                sqlRdr.Close();

                bldColumnList(null);

                sqlTxt = "Select * from [AutomationReportingSuite].[dbo].[SlctCols] where ptrReportList=@I";
                sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                sqlCmd.Parameters.AddWithValue("@I", idxReport);
                sqlRdr = sqlCmd.ExecuteReader();
                while (sqlRdr.Read())
                {
                    int orderDirection = Convert.ToInt32(sqlRdr["colOrdering"]);
                    if (orderDirection != 0)
                    {
                        colOrders.Add(sqlRdr["colName"].ToString());
                        colOrdering.Add(sqlRdr["colName"].ToString(), (orderDirection == 1 ? "Asc" : "Desc"));
                    }
                    int colSelection = Convert.ToInt32(sqlRdr["colShow"]);
                    if (colSelection != 0) colListMstr[sqlRdr["colName"].ToString()] = (colSelection > 0);
                }
                sqlRdr.Close();

                sqlTxt = "Select [colName],[colValues] From [AutomationReportingSuite].[dbo].[FltrCols] Inner join [AutomationReportingSuite].[dbo].[FltrFlds] on FltrFlds.ptrFltrCols = FltrCols.Idx Where ptrReportList=@I;";
                sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                sqlCmd.Parameters.AddWithValue("@I", idxReport);
                sqlRdr = sqlCmd.ExecuteReader();
                while (sqlRdr.Read())
                {
                    String fltrCol = sqlRdr["colName"].ToString();
                    String fltrFld = sqlRdr["colValues"].ToString();
                    if (colFltrs.ContainsKey(fltrCol))
                    {
                        colFltrs[fltrCol].Add(fltrFld);
                    }
                    else
                    {
                        colFltrs.Add(fltrCol, new List<string> { fltrFld });
                    }
                }
                sqlRdr.Close();

                sqlCxn.Close();

                bldFldsList(-1);
                bldDisplayTable();
            }
        }

        private void btnReportSave_Click(object sender, EventArgs e)
        {   // [+] When saving a query, call the 'delete_query' function and don't duplicate the delete process here.
            sqlCxn.Open();
            int rc = -1;
            int idxReportQry = -1;

            String saveBy = "";
            String saveAs = this.cmbReportList.Text; // This is fine if the user has typed something but if they've selected something??? We'll see...
            // saveAs is in the format: <query/report name> '<<'<userid>'>>' [Public]
            if (saveAs.Contains(" [Public]")) saveAs = saveAs.Substring(0, saveAs.IndexOf(" [Public]"));
            if (saveAs.Contains(adFns.LeftBrkt))
            {
                saveBy = saveAs.Substring(saveAs.IndexOf(adFns.LeftBrkt) + 1);
                saveBy = saveBy.Substring(0, saveBy.Length - 1);
                saveAs = saveAs.Substring(0, saveAs.IndexOf(adFns.LeftBrkt) - 1);
            }
            else
            {
                sqlTxt = "Select Count(*) from AutomationReportingSuite.dbo.ReportList Where Descr=@D and Owner=@W;";
                sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                sqlCmd.Parameters.AddWithValue("@D", saveAs);
                sqlCmd.Parameters.AddWithValue("@W", whoAmI);
                rc = Convert.ToInt32(sqlCmd.ExecuteScalar());
                if (rc > 0)
                {
                    MessageBox.Show("You already have a query report of this name, select another\n\nLook, just do it OK! I shouldn't have to tell you this!");
                }
            }

            if (saveBy != "" || (saveBy == "" && rc == 0))
            {
                sqlTxt = "Select Count(*) from [AutomationReportingSuite].dbo.ReportList Where Descr=@D and Owner=@W;";
                sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                sqlCmd.Parameters.AddWithValue("@D", saveAs);
                sqlCmd.Parameters.AddWithValue("@W", whoAmI);
                rc = Convert.ToInt32(sqlCmd.ExecuteScalar());
                // rc should only ever be 0 or 1. If 1 then only going to be updating the details of the report query
                if (rc == 0)
                {
                    sqlTxt = "Insert AutomationReportingSuite.dbo.ReportList (Descr,Owner,dbName,tblName) Output Inserted.Idx Values(@D,@O,@DB,@TBL);";
                    sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                    sqlCmd.Parameters.AddWithValue("@D", saveAs);
                    sqlCmd.Parameters.AddWithValue("@O", whoAmI);
                    sqlCmd.Parameters.AddWithValue("@DB", _dbName);
                    sqlCmd.Parameters.AddWithValue("@TBL", _tblName);
                    idxReportQry = (int)sqlCmd.ExecuteScalar();
                }
                else
                {
                    idxReportQry = Convert.ToInt32(this.cmbReportList.SelectedValue);
                    sqlTxt = "Select Idx from AutomationReportingSuite.dbo.ReportList Where Descr=@D;";
                    sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                    sqlCmd.Parameters.AddWithValue("@D", this.cmbReportList.Text);
                    idxReportQry = (int)sqlCmd.ExecuteScalar();
                }

                sqlTxt = "Select Count(*) from AutomationReportingSuite.dbo.SlctCols Where ptrReportList=@N;";
                sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                sqlCmd.Parameters.AddWithValue("@N", idxReportQry);
                if (Convert.ToInt32(sqlCmd.ExecuteScalar()) > 0)
                {
                    sqlTxt = "Delete from AutomationReportingSuite.dbo.SlctCols Where ptrReportList=@N;";
                    sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                    sqlCmd.Parameters.AddWithValue("@N", idxReportQry);
                    sqlCmd.ExecuteNonQuery();
                }
                foreach (KeyValuePair<String, Boolean> kvp in colListMstr)
                {
                    if ((colListMstr.ContainsValue(false) && kvp.Value) || colOrdering.ContainsKey(kvp.Key))
                    {
                        int SelectCode = 0;
                        if (colListMstr.ContainsValue(false))
                        {
                            SelectCode = (colListMstr[kvp.Key] ? 1 : -1);
                        }
                        int orderingCode = 0;
                        if (colOrdering.ContainsKey(kvp.Key))
                        {
                            switch (colOrdering[kvp.Key])
                            {
                                case "Asc": orderingCode = 1; break;
                                case "Desc": orderingCode = 1; break;
                                default: orderingCode = 0; break;
                            }
                        }

                        sqlTxt = "Insert AutomationReportingSuite.dbo.SlctCols (ptrReportList,colName,colShow,colOrdering) Values(@P,@N,@S,@O);";
                        sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                        sqlCmd.Parameters.AddWithValue("@P", idxReportQry);
                        sqlCmd.Parameters.AddWithValue("@N", kvp.Key);
                        sqlCmd.Parameters.AddWithValue("@S", SelectCode);
                        sqlCmd.Parameters.AddWithValue("@O", orderingCode);
                        sqlCmd.ExecuteNonQuery();
                    }
                }

                sqlTxt = "Select Count(*) from AutomationReportingSuite.dbo.FltrCols Where ptrReportList=@N;";
                sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                sqlCmd.Parameters.AddWithValue("@N", idxReportQry);
                if (Convert.ToInt32(sqlCmd.ExecuteScalar()) > 0)
                {
                    sqlTxt = "Delete from AutomationReportingSuite.dbo.FltrFlds Where ptrFltrCols in (Select Idx from AutomationReportingSuite.dbo.FltrCols where ptrReportList=@N);";
                    sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                    sqlCmd.Parameters.AddWithValue("@N", idxReportQry);
                    sqlCmd.ExecuteNonQuery();
                    sqlTxt = "Delete from AutomationReportingSuite.dbo.FltrCols Where ptrReportList=@N;";
                    sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                    sqlCmd.Parameters.AddWithValue("@N", idxReportQry);
                    sqlCmd.ExecuteNonQuery();
                }
                foreach (KeyValuePair<String, List<string>> fltrCol in colFltrs)
                {
                    int idxFltr = -1;
                    sqlTxt = "Insert AutomationReportingSuite.dbo.FltrCols (ptrReportList,colName) Output Inserted.Idx Values(@P,@N);";
                    sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                    sqlCmd.Parameters.AddWithValue("@P", idxReportQry);
                    sqlCmd.Parameters.AddWithValue("@N", fltrCol.Key);
                    idxFltr = (int)sqlCmd.ExecuteScalar();

                    foreach (string fltrvalue in fltrCol.Value)
                    {
                        sqlTxt = "Insert AutomationReportingSuite.dbo.FltrFlds (ptrFltrCols,colValues) Values(@P,@V);";
                        sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                        sqlCmd.Parameters.AddWithValue("@P", idxFltr);
                        sqlCmd.Parameters.AddWithValue("@V", fltrvalue);
                        sqlCmd.ExecuteNonQuery();
                    }
                }
                this.cmbReportList.Items.Add(saveAs + " " + adFns.LeftBrkt + saveBy + adFns.RghtBrkt);
            }
            sqlCxn.Close();
        }

        private void btnReportRemove_Click(object sender, EventArgs e)
        {   // [+] remove the query entirely
            if (this.cmbReportList.SelectedIndex > 0)
            {
                String qryName = this.cmbReportList.Text;
                String qryOwner;
                Int32 idxReport, rc;
                if (qryName.Contains("[Public]"))
                {
                    MessageBox.Show("Cannot delete public query reports :-p");
                }
                else
                {
                    qryOwner = qryName.Substring(qryName.IndexOf(adFns.LeftBrkt) + 1);
                    qryOwner = qryOwner.Substring(0, qryOwner.Length - 1);
                    qryName = qryName.Substring(0, qryName.IndexOf(adFns.LeftBrkt) - 1);
                    sqlCxn.Open();
                    sqlTxt = "Select Idx from AutomationReportingSuite.dbo.ReportList Where Descr=@D and Owner=@O;";
                    sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                    sqlCmd.Parameters.AddWithValue("@D", qryName);
                    sqlCmd.Parameters.AddWithValue("@O", qryOwner);
                    idxReport = (int)sqlCmd.ExecuteScalar();
                    sqlTxt = "Delete from AutomationReportingSuite.dbo.SlctCols where ptrReportList=@I;";
                    sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                    sqlCmd.Parameters.AddWithValue("@I", idxReport);
                    rc = (int)sqlCmd.ExecuteNonQuery();
                    sqlTxt = "Delete from AutomationReportingSuite.dbo.FltrFlds where ptrFltrCols in (Select Idx from AutomationReportingSuite.dbo.FltrCols where ptrReportList=@I);";
                    sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                    sqlCmd.Parameters.AddWithValue("@I", idxReport);
                    rc = (int)sqlCmd.ExecuteNonQuery();
                    sqlTxt = "Delete from AutomationReportingSuite.dbo.FltrCols where ptrReportList=@I;";
                    sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                    sqlCmd.Parameters.AddWithValue("@I", idxReport);
                    rc = (int)sqlCmd.ExecuteNonQuery();
                    sqlTxt = "Delete from AutomationReportingSuite.dbo.ReportList where Idx=@I;";
                    sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
                    sqlCmd.Parameters.AddWithValue("@I", idxReport);
                    rc = (int)sqlCmd.ExecuteNonQuery();
                    sqlCxn.Close();
                    if (rc > 0)
                    {
                        this.cmbReportList.Items.RemoveAt(this.cmbReportList.SelectedIndex);
                        this.cmbReportList.SelectedIndex = 0;
                    }
                }
            }
        }

        private void txtRange_Click(object sender, EventArgs e)
        {
            txtRange.SelectAll();
        }
    }

    class adFns
    {
        public static readonly Char LeftBrkt = (char)171; // <<
        public static readonly Char RghtBrkt = (char)187; // >>

        private string _dbName = null;

        public adFns(string dbName = null)
        {
            _dbName = dbName;
        }

        public string cxn(string databaseName = null)
        {
            //string cxn;

            //if (databaseName == null) { databaseName = _dbName; }
            //Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Environment", false);
            //cxn = "Server=" + key.GetValue("DBSERV").ToString() + ";" +
            //      "User=finali;" + "password=" + key.GetValue("MONARCH").ToString() + ";" +
            //      "Database=" + databaseName + ";";
            //cxn = "Server=" + key.GetValue("DBSERV").ToString() + ";" +
            //      "User=ars;" + "password=itchy&scratchy;" +
            //      "Database=" + databaseName + ";";
            //key.Close();
            return _cxn(databaseName);
        }
        public static String _cxn(String databaseName)
        {
            String cxn;

            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Environment", false);
            cxn = "Server=" + key.GetValue("DBSERV").ToString() + ";" +
                  "User=ars;" + "password=itchy&scratchy;" +
                  "Database=" + databaseName + ";";
            key.Close();
            return cxn;
        }

        // public datatable... ==> vardefns _defns=new vardefns(); then datatable fred = _defns.dtPopulateFromSQL("tblname");
        // public static datatable... ==> datatable fred = vardefns.dtPopulateFromSQL("tblname");
        // WHICH IS 'RIGHT'?

        /// <summary>
        ///   creates a data table based upon SQL [a table or query]
        /// </summary>
        /// <param name="fromTable"> either a table name or a full sql select statement</param>
        /// <param name="cxn">optional sql cxn - perhaps should not be optional!</param>
        /// <returns></returns>
        public DataTable dtPopulateFromSQL(string fromTable, SqlConnection cxn = null)
        {
            SqlConnection sqlCxn; bool sqlCxnOpened = false;
            SqlCommand sqlCmd;
            SqlDataReader sqlRdr;
            string sqlTxt;
            List<DataColumn> lstCols = new List<DataColumn>();
            DataTable tbl = new DataTable();

            if (cxn == null)
            {
                sqlCxn = new SqlConnection(adGrp._cxn("Osprey"));
            }
            else sqlCxn = cxn;
            if (sqlCxn.State == ConnectionState.Closed) { sqlCxn.Open(); sqlCxnOpened = true; }

            if ((fromTable + "      ").Substring(0, 6).ToUpper() == "SELECT")
                sqlTxt = fromTable;
            else
                sqlTxt = "Select * from " + fromTable + ";";
            sqlCmd = new SqlCommand(sqlTxt, sqlCxn);
            sqlRdr = sqlCmd.ExecuteReader();

            // build the internal data table based on the SQL table:
            DataTable dtSchema = sqlRdr.GetSchemaTable();
            if (dtSchema != null)
            {
                foreach (DataRow drow in dtSchema.Rows)
                {
                    string colName = Convert.ToString(drow["ColumnName"]);
                    DataColumn col = new DataColumn(colName, (Type)(drow["DataType"]));
                    col.Unique = (bool)drow["IsUnique"];
                    col.AllowDBNull = (bool)drow["AllowDBNull"];
                    col.AutoIncrement = (bool)drow["IsAutoIncrement"];
                    tbl.Columns.Add(col);
                    lstCols.Add(col);
                }
            }

            // read the actual table contents:
            while (sqlRdr.Read())
            {
                DataRow drow = tbl.NewRow();
                for (int Lp1 = 0; Lp1 < lstCols.Count; Lp1++)
                {
                    drow[((DataColumn)lstCols[Lp1])] = sqlRdr[Lp1];
                }
                // I would like to get this working but, for now, I need this code working; maybe later...
                //foreach (DataColumn colName in sqlRdr) {
                //    drow[((DataColumn)colName)] = sqlRdr[Convert.ToString(colName["ColumnName"])];
                //}
                tbl.Rows.Add(drow);
            }

            sqlRdr.Close();

            if (sqlCxnOpened) sqlCxn.Close();
            return tbl;
        }
    }
}