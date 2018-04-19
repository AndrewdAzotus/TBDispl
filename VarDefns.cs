
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TBDispl
{
    class adGrp
    {
        public static readonly Char LeftBrkt = (char)171; // <<
        public static readonly Char RghtBrkt = (char)187; // >>

        private string _dbName = null;

        public adGrp(string dbName = null)
        {
            _dbName = dbName;
        }

        public string cxn(string databaseName = null)
        {
            return _cxn(databaseName == null ? _dbName : databaseName);
        }
        public static string _cxn(string databaseName)
        {
            if (databaseName.ToString() == "") return "";
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Environment", false);
            string cxn = "Server=" + key.GetValue("DBSERV").ToString() + ";" +
                         "User=ars;password=itchy&scratchy;" +
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

            if (cxn == null) {
                sqlCxn = new SqlConnection(adGrp._cxn("Osprey"));
                //sqlCxn.Open();
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
            if (dtSchema != null) {
                foreach (DataRow drow in dtSchema.Rows) {
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
            while (sqlRdr.Read()) {
                DataRow drow = tbl.NewRow();
                for (int Lp1 = 0; Lp1 < lstCols.Count; Lp1++) {
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

    public partial class Shrd : Form
    {
        public static void fillCombo(xList fldData, ComboBox frmField, int chosenIdx = 0, bool useTitle = true, bool inclParmData = false)
        {
            int selIdx = 0;
            if (useTitle) frmField.Items.Add(fldData.Title); else chosenIdx--;
            //if (fldData.hasSubList) {
            //    frmField.SelectedIndexChanged += Combo_SelectedIndexChanged;
            //    fldSubFld.Add(frmField.Name, fldData.SubList);
            //    fldSubData.Add(frmField.Name, fldData);
            //}

            for (int Lp1 = 1; Lp1 <= fldData.MaxIdx; Lp1++) {
                string thing = fldData.Descr(Lp1);
                if (fldData.Idx(Lp1).Equals(chosenIdx)) selIdx = Lp1;
                if (inclParmData) thing += " - " + fldData.Parm(Lp1) + "[" + fldData.Idx(Lp1) + "]";
                frmField.Items.Add(thing);
            }

            //foreach (string province in fldData) {
            //    frmField.Items.Add(province);
            //}

            frmField.SelectedIndex = selIdx;
        }
    }

    public class xList : IEnumerable<string>
    {
        /// <summary>
        /// This class encapsulates a simple way to manage multiple small tables in a single [SQL] table.
        /// Data Defn:
        ///  r  Idx         Auto Increment int column, (column is optional in database defn)
        ///  r  ListType    List Entry Type, all items in a list will have the same list type, the 
        ///                 list of lists is ListType=0
        ///  r  ListIdx     List index pointer within the list
        ///  r  EntryDescr  Main list entry
        ///  o  EntryParm   text Parameter associated with list entry
        ///  o  EntryValue  numeric Parameter associated with list entry
        ///  o  EntryWhen   datetime Parameter associated with list entry
        ///  o  EntryFlag   byte value Parameter associated with list entry (0-255 or -128-127)
        ///     
        /// creator:
        ///   - xList Title
        ///   - autocreate [if xList does not exist]
        ///   - init values [for entrydescr, for auto-creation]
        ///   - dbName [where the xList actually is!]
        ///   
        /// this[...]:
        ///   - sets/returns the entrydescr value
        ///   
        /// Title
        ///   - sets/gets the title of the xlist, equivalent to this[0] and defaults to xlist name if this[0] does not exist
        ///   
        /// hasSubList
        ///   - is there a combobox list dependency     } this is used so that when this xlist is assigned to a combobox
        ///                                             } when this combobox is updated the compbo box that holds the 
        /// SubList                                     } sublist dependency may updated at the same time. e.g. if
        ///   - the name of the combolist dependency    } salutation is changed [Mr, Mrs. etc.] then gender may be altered too
        ///   
        /// MaxIdx
        ///   - the highest xList idx currently in use
        ///   
        /// Descr / Parm / [set]Value / [set]When / [set]Flag
        ///   - gets/sets values for the row
        ///   
        /// contains
        ///   - does the xlist hold the description
        ///   
        /// indexof
        ///   - returns the index in the xlist of the descr value
        ///   
        /// to write:
        ///   - add [entry with options value/parm etc. and write to database table]
        ///   - append to xlist but NOT write to database table, i.e. temp adds for duration of pgm run
        ///   - save [to db table]
        ///   - sort [by field, e.g. parm, value, datetime etc.]
        ///     . this will require reading the xlist info into a DataTable, but this can be developed in parallel with existing lists
        ///     
        /// What to do about:
        ///   - instant updates to sql
        ///   - listidx of appended values, i.e. values added locally within memory only
        ///   
        /// To access an xList from a pointer field from another table:
        /// 
        /// Select EntryDescr, (,[entryParm]) (,[entryValue]) (,[entryWhen]) (,[entryFlag])
        ///   From xList where ListType = (Select ListIdx from xList where ListType=0 and EntryDescr='[xlistName]');
        /// 
        /// </summary>
        //private const Char LeftBrkt = (char)171; // <<
        //private const Char RghtBrkt = (char)187; // >>
        adGrp defns = new adGrp("Common");

        private SqlConnection SQLCxn;
        private SqlCommand SQLCmd;
        private SqlDataReader SQLRdr;
        private string sqlTxt;

        private string xTitle = "";
        private string xSubList = "";
        private Boolean xAutoSave = false;

        private List<int> xIdx = new List<int>();
        private List<string> xDescr = new List<string>();
        private List<string> xParm = new List<string>();
        private List<int> xValue = new List<int>();
        //private List<DateTime> xWhen = new List<DateTime>();
        //private List<sbyte> xFlag = new List<sbyte>();
        private List<bool> xUpdtd = new List<bool>();

        private DataSet xTable;
        private DataTable xTbl;
        private Dictionary<int, int> intIdx = new Dictionary<int, int>();

        private int xlType = 0;
        private int xlCount = 0;
        private int xlMaxIdx = 0;

        public enum Flds
        {
            Idx, Descr, Parm, Value, When, Flag
        }
        private Dictionary<Flds, string> xListOrder = new Dictionary<Flds, string>();
        private enum intFlds
        {
            ListIdx, Descr, Parm, Value, When, Flag, Updated
        }
        private Dictionary<intFlds, string> xListFlds = new Dictionary<intFlds, string>();


        public xList(string xListName = ""
                   , bool autoSave = false
                   , bool autoCreate = false
                   , string initTitle = ""
                   , string[] initValues = null
                   , Flds orderBy = Flds.Idx
                   , string dbName = "Common"
                   , string andWhere = ""
                   )
        {
            xAutoSave = autoSave;

            initStuff();
            DataRow xRow;

            SQLCxn = new SqlConnection(defns.cxn(dbName));

            SQLCxn.Open();
            if (xListName != "") {
                sqlTxt = "Select ListIdx,EntryParm from xList where ListType=0 and EntryDescr=@xLN;";
                SQLCmd = new SqlCommand(sqlTxt, SQLCxn);
                SQLCmd.Parameters.AddWithValue("@xLN", xListName);
                SQLRdr = SQLCmd.ExecuteReader();
                if (SQLRdr.Read()) {
                    xlType = (int)SQLRdr[0];
                    xSubList = SQLRdr[1].ToString();
                    SQLRdr.Close();
                }
                else { // auto-create here if requested...
                    SQLRdr.Close();
                    if (autoCreate) {
                        int maxIdx = 0;
                        sqlTxt = "Select Coalesce(Max(ListIdx),0) as MaxIdx from dbo.xList where ListType=0;";
                        SQLCmd = new SqlCommand(sqlTxt, SQLCxn);
                        //try {
                        //Int32.TryParse(SQLCmd.ExecuteScalar().ToString(), out maxIdx);
                        //maxIdx++;
                        maxIdx = (int)SQLCmd.ExecuteScalar() + 1;
                        //}
                        //catch (Exception exc) {
                        //    string bert = exc.GetType().ToString() + "-" + exc.Message;
                        //}
                        sqlTxt = "Insert xList (ListType,ListIdx,EntryDescr) values(0,@XI,@XD);";
                        SQLCmd = new SqlCommand(sqlTxt, SQLCxn);
                        SQLCmd.Parameters.AddWithValue("@XI", maxIdx);
                        SQLCmd.Parameters.AddWithValue("@XD", xListName);
                        if (Convert.ToInt32(SQLCmd.ExecuteNonQuery()) > 0) {
                            xlType = maxIdx;
                            if (initTitle != "") {
                                sqlTxt = "Insert xList (ListType,ListIdx,EntryDescr) values(@XT,@XI,@XD);";
                                SQLCmd = new SqlCommand(sqlTxt, SQLCxn);
                                SQLCmd.Parameters.AddWithValue("@XT", maxIdx);
                                SQLCmd.Parameters.AddWithValue("@XI", 0);
                                SQLCmd.Parameters.AddWithValue("@XD", initTitle);
                                if (Convert.ToInt32(SQLCmd.ExecuteNonQuery()) == 0) {
                                    string bert = "was unable to add a new row...";
                                    bert += "to xlist during autocreate";
                                }
                            }
                            if (!initValues.Equals(null)) { // && initValues.Length > 0) {
                                int Ctr = 1;
                                foreach (string newDesc in initValues) {
                                    sqlTxt = "Insert xList (ListType,ListIdx,EntryDescr) values(@XT,@XI,@XD);";
                                    SQLCmd = new SqlCommand(sqlTxt, SQLCxn);
                                    SQLCmd.Parameters.AddWithValue("@XT", maxIdx);
                                    SQLCmd.Parameters.AddWithValue("@XI", Ctr++);
                                    SQLCmd.Parameters.AddWithValue("@XD", newDesc);
                                    if (Convert.ToInt32(SQLCmd.ExecuteNonQuery()) == 0) {
                                        string bert = "was unable to add a new row...";
                                        bert += "to xlist during autocreate";
                                        MessageBox.Show(bert, "Uh, HELP! - Don't know what to do!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    }
                                }
                            }
                        }
                    }
                }
                // SQLRdr.Close(); // needs to be above as it needs to be at the beginning of the 'else' clause as the cxn is reused!
            }
            if (xSubList != "") {
                SQLCmd = new SqlCommand("Select Count(Idx) from xList where ListType=0 and EntryDescr=@ed;", SQLCxn);
                SQLCmd.Parameters.AddWithValue("@ed", xSubList);
                if (Convert.ToInt32(SQLCmd.ExecuteScalar()) == 0) xSubList = "";
            }
            sqlTxt = "Select * from xList as tblPri where ListType=@xLT";
            if (andWhere != "") { sqlTxt += " and " + andWhere; }
            sqlTxt += " Order By " + xListOrder[orderBy];
            sqlTxt += ";";
            SQLCmd = new SqlCommand(sqlTxt, SQLCxn);
            SQLCmd.Parameters.AddWithValue("@xLT", xlType);
            SQLRdr = SQLCmd.ExecuteReader();
            int Lp1 = 0;
            while (SQLRdr.Read()) {
                // old list style
                int idx = Convert.ToInt32(SQLRdr["ListIdx"]); // can NEVER be null
                if (xTitle == "" && idx != 0) {
                    xTitle = (xListName == "" ? "xList Index" : xListName);// adGrp.LeftBrkt + (xListName == "" ? "xList Index" : xListName) + adGrp.RghtBrkt;
                    xRow = xTbl.NewRow();
                    xRow["ListIdx"] = 0;
                    xRow["EntryDescr"] = xTitle;
                    xTbl.Rows.Add(xRow);
                    //xTbl.Rows[0]["EntryDescr"] = Autumn.LeftBrkt + SQLRdr["EntryDescr"].ToString() + Autumn.RghtBrkt;
                }
                //else 
                {
                    xIdx.Add(idx);
                    string txt = SQLRdr["EntryDescr"].ToString();
                    if (idx == 0) { xTitle = txt; } // adGrp.LeftBrkt + txt + adGrp.RghtBrkt; }
                    xDescr.Add(txt); // used in the inumerator...
                    //   xParm.Add(SQLRdr["EntryParm"] is DBNull ? "" : SQLRdr["EntryParm"].ToString());
                    //    xValue.Add(SQLRdr["EntryValue"] is DBNull ? 0 : Convert.ToInt32(SQLRdr["EntryValue"]));
                    //xWhen.Add(SQLRdr["EntryWhen"] is DBNull ? DateTime.MinValue : Convert.ToDateTime(SQLRdr["EntryWhen"]));
                    //xFlag.Add(SQLRdr["EntryFlag"] is DBNull ? (sbyte)0 : Convert.ToSByte(SQLRdr["EntryFlag"]));
                    // xUpdtd.Add(false);
                    xlCount++;

                    // new dataset/datatable style
                    xRow = xTbl.NewRow();
                    xRow["Idx"] = SQLRdr["Idx"];
                    xRow["ListIdx"] = SQLRdr["ListIdx"];
                    xRow["EntryDescr"] = SQLRdr["EntryDescr"];
                    xRow["EntryParm"] = SQLRdr["EntryParm"];
                    xRow["EntryValue"] = SQLRdr["EntryValue"];
                    xRow["EntryWhen"] = SQLRdr["EntryWhen"];
                    xRow["EntryFlag"] = SQLRdr["EntryFlag"];
                    xRow["Updated"] = false;
                    xTbl.Rows.Add(xRow);
                    intIdx.Add(Convert.ToInt32(SQLRdr["ListIdx"]), Lp1++);
                }
            }
            SQLRdr.Close();

            sqlTxt = "Select Max(ListIdx) from xList as tblPri where ListType=@xLT";
            SQLCmd = new SqlCommand(sqlTxt, SQLCxn);
            SQLCmd.Parameters.AddWithValue("@xLT", xlType);
            xlMaxIdx = Convert.ToInt32(SQLCmd.ExecuteScalar());

            SQLCxn.Close();
            if (xTitle == "") {
                xTitle = xListName; // adGrp.LeftBrkt + xListName + adGrp.RghtBrkt;
                xRow = xTbl.NewRow();
                xRow["ListIdx"] = 0;
                xRow["EntryDescr"] = xListName;
                xTbl.Rows.Add(xRow);
            }
        }

        /* --- sets and gets --- */
        //public string Title { get { return xTitle; } set { xTitle = value; } }
        /// <summary>
        /// Title is hold an xList title which although held as part of the list in the table is handled differently.
        /// When fetched [via get] is returned surrounded with special characters &#171; and &#187; [ascii characters 171 and 187]. These
        /// do not need to be specified when setting the title since they
        /// will be removed. The title is never stored in the database with the special character delimiters.
        /// </summary>
        public string Title
        {
            get
            {
                return adGrp.LeftBrkt + xTbl.Rows[0][xListFlds[intFlds.Descr]].ToString() + adGrp.RghtBrkt;
                //return xTbl.Rows[0][xListFlds[intFlds.Descr]].ToString();
            }
            set
            {
                //xTitle = value;
                if (value.Substring(0, 1).Equals(adGrp.LeftBrkt)) value = value.Substring(1);
                if (value.Substring(value.Length - 1).Equals(adGrp.RghtBrkt)) value = value.Substring(0, value.Length - 1);
                xTbl.Rows[0][xListFlds[intFlds.Descr]] = value;
                xTbl.Rows[0][xListFlds[intFlds.Updated]] = true;
            }
        }
        public bool hasSubList { get { return (xSubList != ""); } } // functionality not used here at UPS ... yet!
        public string SubList { get { return xSubList; } } // functionality not used here at UPS ... yet!
        public int MaxIdx { get { return xlMaxIdx; } }
        public int Count { get { return xTbl.Rows.Count - 1; } } // this is the xTbl count less one because the xList Title is included in the xTbl and is not actually part of the row data.

        public string this[int idx]
        {
            get
            {
                //return xDescr[idx];
                return xTbl.Rows[idx][xListFlds[intFlds.Descr]].ToString();
            }
            set
            {
                xTbl.Rows[idx][xListFlds[intFlds.Descr]] = value;
                xTbl.Rows[idx][xListFlds[intFlds.Updated]] = true;
                xDescr[idx] = value;
                xUpdtd[idx] = true;
            }
        }
        public object this[int idx, Flds fld]
        {
            get
            {
                return xTbl.Rows[idx][xListOrder[fld]];
            }
            set
            {
                xTbl.Rows[idx][xListOrder[fld]] = value;
                xTbl.Rows[idx][xListFlds[intFlds.Updated]] = true;
            }
        }

        public int Idx(int idx)
        {
            return Convert.ToInt32(xTbl.Rows[idx][xListFlds[intFlds.ListIdx]]);
        }
        public string Descr(int idx, string setDescr = null)
        {
            int xIdx = idx;
            //xIdx = intIdx[idx];

            if (setDescr != null) { xTbl.Rows[xIdx][xListFlds[intFlds.Descr]] = setDescr; xTbl.Rows[xIdx][xListFlds[intFlds.Updated]] = true; }

            string bert = ""; try {
                bert = xTbl.Rows[xIdx][xListFlds[intFlds.Descr]].ToString();
            }
            catch (Exception exc) {
                string jim = exc.GetType().ToString() + " - " + exc.Message;

            }
            return bert;
        }
        public string Parm(int idx, string setParm = null)
        {
            if (setParm != null) {
                xTbl.Rows[idx][xListFlds[intFlds.Parm]] = setParm;
                xTbl.Rows[idx][xListFlds[intFlds.Updated]] = true;
            }
            return xTbl.Rows[idx][xListFlds[intFlds.Parm]].ToString();
        }
        public String Parm(String Descr, string setParm = null)
        {
            String rc = "";
            int idx = indexOf(Descr);
            if (idx > 0) {
                rc = Parm(idx, setParm);
            }
            else if (idx < 0 || setParm != null) {
                if (Add(Descr, parm: setParm)) rc = setParm;
            }
            return rc;
        }
        public int Value(int idx)
        {
            // idx--; // because xlist is 1 based [because xlist=0 is xlist title] and the list box is 0 based
            // int fred= xValue[idx];
            int rtn;
            int.TryParse(xTbl.Rows[idx][xListFlds[intFlds.Value]].ToString(), out rtn);
            return rtn;
        }
        public void setValue(int idx, int setValue)
        {
            //xValue[idx] = setValue;
            xTbl.Rows[idx][xListFlds[intFlds.Value]] = setValue;
            xTbl.Rows[idx][xListFlds[intFlds.Updated]] = true;
        }
        public DateTime When(int idx)
        {
            DateTime rc;
            if (xTbl.Rows[idx][xListFlds[intFlds.When]] == System.DBNull.Value) {
                rc = DateTime.MinValue;
            }
            else rc = Convert.ToDateTime(xTbl.Rows[idx][xListFlds[intFlds.When]]);
            return rc;
        }
        public void setWhen(int idx, DateTime setWhen) // ideally, this would default to System.DateTime.MinValue, but System.DateTime.MinValue is not a compile time constant and, thus, cannot be used...
        {
            xTbl.Rows[idx][xListFlds[intFlds.When]] = setWhen;
            xTbl.Rows[idx][xListFlds[intFlds.Updated]] = true;
        }

        public byte Flag(int idx)
        {
            byte rc = 0;
            if (idx > 0) {
                if (xTbl.Rows[idx][xListFlds[intFlds.Flag]] == System.DBNull.Value) { rc = 0; }
                else rc = Convert.ToByte(xTbl.Rows[idx][xListFlds[intFlds.Flag]]);
            }
            return rc;
        }
        public byte Flag(string entryDescr)
        {
            return Flag(indexOf(entryDescr));
        }
        public void setFlag(int idx, byte valueFlag = 0)
        {
            xTbl.Rows[idx][xListFlds[intFlds.Flag]] = valueFlag;
            entryUpdate(idx);
            //xTbl.Rows[idx][xListFlds[intFlds.Updated]] = true;
        }
        public void setFlag(string entryDescr, byte valueFlag = 0)
        {
            setFlag(indexOf(entryDescr), valueFlag);
        }

        /* --- list functions --- */
        public bool contains(string Descr)
        {
            return (indexOf(Descr) >= 0);
        }

        public int indexOf(string Descr)
        {
            int idx = xDescr.IndexOf(Descr);
            return (idx >= 0 ? xIdx[idx] : idx);
        }

        /* --- storage [loads and saves] functions --- */
        public bool Add(string descr, string parm = null, int? value = null, DateTime? when = null, byte? flag = null)
        {
            bool rc = false;

            SQLCmd = new SqlCommand();
            string sqlTxt1 = "Insert xList (ListType,ListIdx,EntryDescr";
            string sqlTxt2 = ") Values(@LT,@LI,@ED";
            SQLCmd.Parameters.AddWithValue("@LT", xlType);
            SQLCmd.Parameters.AddWithValue("@LI", ++xlMaxIdx);
            SQLCmd.Parameters.AddWithValue("@ED", descr);
            if (parm != null) { sqlTxt1 += ",EntryParm"; sqlTxt2 += ",@EP"; SQLCmd.Parameters.AddWithValue("@EP", parm); }
            if (value.HasValue) { sqlTxt1 += ",EntryValue"; sqlTxt2 += ",@EV"; SQLCmd.Parameters.AddWithValue("@EV", value); }
            if (when.HasValue) { sqlTxt1 += ",EntryWhen"; sqlTxt2 += ",@EW"; SQLCmd.Parameters.AddWithValue("@EW", when); }
            if (flag.HasValue) { sqlTxt1 += ",EntryFlag"; sqlTxt2 += ",@EF"; SQLCmd.Parameters.AddWithValue("@EF", flag); }
            sqlTxt = sqlTxt1 + sqlTxt2 + ");";

            SQLCmd.Connection = SQLCxn;
            SQLCmd.CommandText = sqlTxt;
            SQLCxn.Open();
            rc = (SQLCmd.ExecuteNonQuery() == 1);
            SQLCxn.Close();
            if (rc) {
                xlCount++;
                Append(xlMaxIdx, descr, parm, value, when, flag);
            }
            return rc;
        }
        public void Append(string descr, string parm = null, int? value = null, DateTime? when = null, byte? flag = null)
        {
            Append(++xlMaxIdx, descr, parm, value, when, flag);
        }
        private void Append(int listIdx, string descr, string parm = null, int? value = null, DateTime? when = null, byte? flag = null)
        {
            DataRow xRow;
            xRow = xTbl.NewRow();
            xRow["ListIdx"] = listIdx;
            xRow["EntryDescr"] = descr;
            xRow["EntryParm"] = parm;
            if (value == null) xRow["EntryValue"] = System.DBNull.Value; else xRow["EntryValue"] = value;
            if (when == null) xRow["EntryWhen"] = System.DBNull.Value; else xRow["EntryWhen"] = when;
            if (flag == null) xRow["EntryFlag"] = System.DBNull.Value; else xRow["EntryFlag"] = flag;
            xTbl.Rows.Add(xRow);
            intIdx.Add(listIdx, listIdx);
        }
        public bool Save(bool addAppended = false)
        {
            int rc = 0;
            DataRow[] xRows = xTbl.Select(xListFlds[intFlds.Updated].ToString() + "=true");
            if (xRows.Length > 0) {
                SQLCxn.Open();
                foreach (DataRow xRow in xRows) {
                    bool newEntry = false;
                    string sqlTxt1, sqlTxt2;
                    if (xRow["Idx"] == System.DBNull.Value) {
                        newEntry = true;
                        sqlTxt1 = "Insert xList (ListType,ListIdx,EntryDescr";
                        sqlTxt2 = ") Values(@LT,@EL,@ED";
                    }
                    else {
                        sqlTxt1 = "Update xList set ListIdx=@EL,EntryDescr=@ED";
                        sqlTxt2 = "";
                    }
                    SQLCmd = new SqlCommand();
                    foreach (intFlds colName in Enum.GetValues(typeof(Flds))) { // yeah, I know, these are of different types, but intFlds contains 'Updated' which does not need to be written to the xList table.
                        object contents = xRow[xListFlds[colName]]; // .ToString();
                        if (contents != System.DBNull.Value) {
                            if (!sqlTxt1.Contains(colName.ToString())) {
                                if (newEntry) {
                                    sqlTxt1 += ",Entry" + colName;
                                    sqlTxt2 += ",@E" + colName.ToString().Substring(0, 1);
                                }
                                else {
                                    sqlTxt1 += ",Entry" + colName.ToString() + "=@E" + colName.ToString().Substring(0, 1);
                                }
                            }
                            SQLCmd.Parameters.AddWithValue("@E" + colName.ToString().Substring(0, 1), contents);
                        }
                    }
                    int fred = (int)xRow[0];
                    if (newEntry) SQLCmd.Parameters.AddWithValue("@LT", xlType);
                    else SQLCmd.Parameters.AddWithValue("@Idx", xRow[0]);
                    SQLCmd.Connection = SQLCxn;
                    SQLCmd.CommandText = sqlTxt1 + sqlTxt2 + (newEntry ? ")" : " Where Idx=@Idx") + ";";
                    try {
                        if (SQLCmd.ExecuteNonQuery() != 1) // inserted/updated a row...
                            rc++;
                    }
                    catch (Exception exc) {
                        string bert = exc.Message;
                        MessageBox.Show("xList Update faied\n\n" + bert, xTitle);
                    }
                }
                SQLCxn.Close();
            }

            return (rc == 0);
        }
        private void entryUpdate(int idx)
        {
            if (xAutoSave) {
                // this is not the best way to do this, but it'll do for now!
                xTbl.Rows[idx][xListFlds[intFlds.Updated]] = true;
                Save();
            }
            else {
                xTbl.Rows[idx][xListFlds[intFlds.Updated]] = true;
            }
        }

        /* --- providing C# functionality --- */
        /* --- foreach functional stuff --- */
        public IEnumerator<string> GetEnumerator()
        {
            return xDescr.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return xDescr.GetEnumerator();
        }

        /* --- internal hidden functions --- */
        private void initStuff()
        {
            xTbl = new DataTable("xList");
            //DataColumn xData;
            //DataRow xEntry;

            xTbl.Columns.Add(new DataColumn("Idx", System.Type.GetType("System.Int32")));
            xTbl.Columns.Add(new DataColumn("ListIdx", System.Type.GetType("System.Int32")));
            xTbl.Columns.Add(new DataColumn("EntryDescr", System.Type.GetType("System.String")));
            xTbl.Columns.Add(new DataColumn("EntryParm", System.Type.GetType("System.String")));
            xTbl.Columns.Add(new DataColumn("EntryValue", System.Type.GetType("System.Int32")));
            xTbl.Columns.Add(new DataColumn("EntryWhen", System.Type.GetType("System.DateTime")));
            xTbl.Columns.Add(new DataColumn("EntryFlag", System.Type.GetType("System.Byte")));
            xTbl.Columns.Add(new DataColumn("Updated", System.Type.GetType("System.Boolean")));

            xTable = new DataSet("xList");
            xTable.Tables.Add(xTbl);

            xListOrder.Add(Flds.Idx, "ListIdx");
            xListOrder.Add(Flds.Descr, "EntryDescr");
            xListOrder.Add(Flds.Parm, "EntryParm");
            xListOrder.Add(Flds.Value, "EntryValue");
            xListOrder.Add(Flds.When, "EntryWhen");
            xListOrder.Add(Flds.Flag, "EntryFlag");

            xListFlds.Add(intFlds.ListIdx, "ListIdx");
            xListFlds.Add(intFlds.Descr, "EntryDescr");
            xListFlds.Add(intFlds.Parm, "EntryParm");
            xListFlds.Add(intFlds.Value, "EntryValue");
            xListFlds.Add(intFlds.When, "EntryWhen");
            xListFlds.Add(intFlds.Flag, "EntryFlag");
            xListFlds.Add(intFlds.Updated, "Updated");
        }
    }
}
