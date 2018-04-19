
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBDispl
{
    class EditSkeltoDocs
    {
        /* Development History
         * v.0.1 2015-11-16, Andrew J. Davis, Initial version. Opens tables and Documents
         * v.0.2 2015-11-17, Andrew J. Davis, Updates skeleton and fills fields then writes builtDocs
         * v.0.3 2015-11-18, Andrew J. Davis, Implemented tables and Primary record processing
         * v.1.0 2015-11-18, Andrew J. Davis, first Release
         * v.1.1 2015-11-19, Andrew J. Davis, update of rtf code processing to only check for control characters and @"/"
         * 
         * Future Development:
         * - set up a currency format, e.g. [[$AMT]]
         * - and a justifying format, e.g. [[#,##0.00-Value]]
         * - and a date-time format, e.g. [[#dd-mmm-yyyy#When]], 
         *   although this is harder as there are lots of differnet date formats
         * - gender based field processing, i.e. [[ <m> | <f> | <other> ]]
         */
        string _SklPath;
        string _DocPath;
        string _dbName = "Common";
        SqlConnection _sqlCxn;

        StringBuilder _rtfDocument = new StringBuilder(131072);
        /// <summary>
        /// Initialise the necessary 'stuff' for the document builder.
        /// </summary>
        /// <param name="SklPath">Required - full path to location of skeletons</param>
        /// <param name="DocPath">Required - full path to location where built documents will be placed</param>
        /// <param name="dbCxn">[Optional - if a database connection is not passed, then one will be created.</param>
        /// <param name="dbName">[Optional - Default = 'Common' database]</param>
        public EditSkeltoDocs(string SklPath, string DocPath, string dbName = null, SqlConnection dbCxn = null)
        {
            if (!Directory.Exists(SklPath))
                throw new System.ArgumentException("Skeleton Path must exist.", "Skel To Doc Builder");
            _SklPath = SklPath;

            if (!Directory.Exists(SklPath))
                throw new System.ArgumentException("Document Path to build documents to must exist.", "Skel To Doc Builder");
            _DocPath = DocPath;
            if (dbName != null) { _dbName = dbName; }
            if (dbCxn == null) { _sqlCxn = new SqlConnection(adGrp._cxn(_dbName)); }
        }

        ~EditSkeltoDocs()
        {
            _rtfDocument = null;
        }

        /// <summary>
        /// Build a document from a skeleton file by inserting data from an SQL Command into fields such as [[FirstName]].
        /// Documents are in rtf format to ensure: application [word processor] independence, interface [language] independence.
        /// i.e. with an rtf, the end user may use M$ Word, OpenOffice or LibreOffice Writer, or any other editor.
        ///              also the interface may be written in C#, as it is here, however a PHP version is also available.
        /// </summary>
        /// <param name="rtfSkeleton">name of skeleton to use when building document. Later versions will proffer a file selection window if this field is empty.</param>
        /// <param name="sqlMainRows">SQL command to return one or more records from database defined in the constructor</param>
        /// <param name="colClientRef">[Optional] columns to use when creating folder within 'DocPath'; Defaults to storing all docs in DocPath;</param>
        /// <param name="sqlTblRows">[Optional, defaults to same sql statement as MainRows</param>
        /// <param name="sqlPrimaryRows">[Optional] some fields are defined as [[^FirstName]] the '^' indicates the data should come from this parent record.</param>
        /// <param name="sqlLinkToPrimary">[Optional if PrimaryRows Specified] defines a 1 to 1 relationship between MainRows and Primary Rows. This is simply a where clause without the where and may be specified as part of the PrimaryRows sql statement</param>
        public void Build(string rtfSkeleton
                        , string sqlMainRows
                        , string colClientRef = ""
                        , string sqlTblRows = ""
                        , string sqlPrimaryRows = ""
                        , string sqlLinkToPrimary = ""
                        , string sqlLinkToMain = ""
                         )
        {
            bool inTbl = false;
            bool ifFlg = false;

            // notes: If, for example, building an invoice for a company, the invoice rows would be passed in
            // as sqlMainRows and the company information would be passed in as PrimaryRows with LinkToPrimary
            // containing the Id[x] field and LinkToMain containing the column in the mainrows [invoice rows]
            // that contains the Id[x] of the company.

            List<string> fldDataCols = new List<string>();
            SqlCommand sqlCmd;
            SqlDataReader sqlRdr;

            SqlConnection _priCxn = null;
            SqlCommand priCmd = null;
            SqlDataReader priRdr = null;
            List<string> priDataCols = new List<string>();

            SqlConnection _tblCxn = null;
            SqlCommand tblCmd = null;
            SqlDataReader tblRdr = null;
            List<string> tblDataCols = new List<string>();
            if (sqlTblRows.Equals("")) sqlTblRows = sqlMainRows;

            _sqlCxn.Open();
            sqlCmd = new SqlCommand(sqlMainRows, _sqlCxn);
            sqlRdr = sqlCmd.ExecuteReader();
            // get columns names of SQL data passed in:
            for (int Lp1 = 0; Lp1 < sqlRdr.FieldCount; Lp1++) {
                fldDataCols.Add(sqlRdr.GetName(Lp1).ToLower());
            }

            int LpDoc = 0;
            while (sqlRdr.Read()) {
                _rtfDocument.Append(File.ReadAllText(Path.Combine(_SklPath, rtfSkeleton)));
                string rtfBuiltDoc = DateTime.Now.ToString("hhmmss") + "-" + rtfSkeleton;
                rtfBuiltDoc = (++LpDoc).ToString() + "] " + rtfSkeleton; // [DBG]

                // SO THIS IS FOR SEARCHING ONLY. stringBuilder does not have indexof etc. so use THIS string for finding the [[flds]] and then do the replace in the stringbuilder.
                string rtfCpy = _rtfDocument.ToString(); // if this is modified, esp. on large documents then memory management will quickly grow out of control and fall apart.

                //int jim = 0;
                //Stopwatch sw1 = new Stopwatch();
                //sw1.Start();
                //for (int Lp1 = 0; Lp1 < 100000; Lp1++) { jim = _rtfDocument.ToString().IndexOf("]]"); }
                //sw1.Stop();
                //TimeSpan ts1 = sw1.Elapsed;

                //Stopwatch sw2 = new Stopwatch();
                //sw2.Start();
                //for (int Lp1 = 0; Lp1 < 100000; Lp1++) { jim = rtfCpy.IndexOf("]]"); }
                //sw2.Stop();
                //TimeSpan ts2 = sw2.Elapsed;
               
                int ptr1 = rtfCpy.IndexOf("[["); int ptr2 = 0; int ptr3 = 0;
                string fldName = "", fldFrom = "", fldDest = "", rtfBit = "";
                bool replaceAll;
                
                while (ptr1 > 0) {
                    replaceAll = false;
                    ptr2 = rtfCpy.IndexOf("]]");
                    ptr3 = rtfCpy.IndexOf("[[", ptr1 + 2);
                    while (ptr3 >= 0 && ptr3 < ptr2) {
                        ptr1 = ptr3;
                        ptr3 = rtfCpy.IndexOf("[[", ptr1 + 2);
                    }
                    fldFrom = rtfCpy.Substring(ptr1, ptr2 - ptr1 + 2);
                    fldName = rtfCpy.Substring(ptr1 + 2, ptr2 - ptr1 - 2);

                    rtfBit = "";
                    fldDest = null;
                    while (fldName.Contains("}{")) {
                        int ptr4 = fldName.IndexOf("}{");
                        int ptr5 = fldName.IndexOf(" ") + 1;
                        while (ptr5 < fldName.Length && (fldName.Substring(ptr5, 1).All(char.IsControl) || fldName.Substring(ptr5, 1).Equals(@"\"))) {
                            ptr5 = fldName.IndexOf(" ", ptr5 + 1) + 1;
                        }
                        rtfBit += fldName.Substring(ptr4, ptr5 - ptr4);
                        fldName = fldName.Substring(0, ptr4) + fldName.Substring(ptr5);
                    }
                    fldName = fldName.ToLower();
                    replaceAll = false;

                    if (fldName.Substring(0, 1).Equals("-") && fldName.IndexOf("-", 1) > 0) {
                        switch (fldName.Substring(0, fldName.IndexOf("-", 1) + 1)) { // special fields
                            case "-today-":
                                fldDest = DateTime.Today.ToShortDateString();
                                replaceAll = true;
                                break;
                            case "-if-":
                                int ptr = 0;
                                string[] parmFlds = { "", "", "" };
                                for (int Lp1 = 4; Lp1 < fldName.Length; Lp1++) { // the initialise for Lp1 needs to be at the end of -if- for true flexibility...
                                    if (Lp1 + 1 < fldName.Length && ">>==<<!=".Contains(fldName.Substring(Lp1, 2))) {
                                        parmFlds[++ptr] = fldName.Substring(Lp1, 2);
                                        ptr++; Lp1 += 2;
                                    }
                                    // the following conditional is only required when the second parm is a null string, e.g. [[-if-[[fld]]==]]
                                    if (Lp1 < fldName.Length) { parmFlds[ptr] += fldName.Substring(Lp1, 1); }
                                }
                                switch (parmFlds[1]) {
                                    case "!=":
                                        ifFlg = (string.Compare(parmFlds[0], parmFlds[2]) != 0);
                                        break;
                                    case "==":
                                        ifFlg = (string.Compare(parmFlds[0], parmFlds[2]) == 0);
                                        break;
                                    case ">=":
                                        ifFlg = (string.Compare(parmFlds[0], parmFlds[2]) >= 0);
                                        break;
                                    case ">>":
                                        ifFlg = (string.Compare(parmFlds[0], parmFlds[2]) > 0);
                                        break;
                                    case "<=":
                                        ifFlg = (string.Compare(parmFlds[0], parmFlds[2]) <= 0);
                                        break;
                                    case "<<":
                                        ifFlg = (string.Compare(parmFlds[0], parmFlds[2]) < 0);
                                        break;
                                }
                                fldDest = "";
                                break;
                            case "-true-":
                                if (ifFlg) {
                                    fldDest = "";
                                }
                                else {
                                    int ptr4 = rtfCpy.ToLower().IndexOf("[[-trueend-]]") + "[[-trueend-]]".Length;
                                    _rtfDocument.Remove(ptr1, ptr4 - ptr1);
                                    fldDest = "";
                                }
                                break;
                            case "-trueend-":
                                fldDest = "";
                                break;
                            case "-false-":
                                if (ifFlg) {
                                    int ptr4 = rtfCpy.ToLower().IndexOf("[[-falseend-]]") + "[[-falseend-]]".Length;
                                    _rtfDocument.Remove(ptr1, ptr4 - ptr1);
                                }
                                else {
                                    fldDest = "";
                                }
                                break;
                            case "-falseend-": // falseend is really processed as part of false so just clear the field
                                fldDest = "";
                                break;
                            case "-tbl-": // initialise table procesing...
                                inTbl = true;
                                _tblCxn = new SqlConnection(adGrp._cxn(_dbName));
                                _tblCxn.Open();
                                tblCmd = new SqlCommand(sqlTblRows, _tblCxn);
                                tblRdr = tblCmd.ExecuteReader();
                                for (int Lp1 = 0; Lp1 < tblRdr.FieldCount; Lp1++) {
                                    tblDataCols.Add(tblRdr.GetName(Lp1).ToLower());
                                }
                                fldDest = "";
                                break;
                            case "-tblreset-":
                                tblRdr.Close(); // and... from the top!
                                tblRdr = tblCmd.ExecuteReader();
                                fldDest = "";
                                break;
                            case "-tblend-":
                                inTbl = false; // tidy up...
                                tblRdr.Close();
                                _tblCxn.Close();
                                fldDest = "";
                                break;
                            case "-tblrow-":
                                if (tblRdr.Read()) {
                                    // duplicate the row
                                    int ptr4 = rtfCpy.ToLower().IndexOf("[[-tblrowend-]]") + "[[-tblrowend-]]".Length;
                                    string txtRow = rtfCpy.Substring(ptr1, ptr4 - ptr1);
                                    _rtfDocument.Insert(ptr4, txtRow);
                                }
                                else {
                                    int ptr4 = rtfCpy.ToLower().IndexOf("[[-tblrowend-]]") + "[[-tblrowend-]]".Length;
                                    _rtfDocument.Remove(ptr1, ptr4 - ptr1);
                                }
                                fldDest = "";
                                break;
                            case "-tblrowend-":
                                fldDest = "";
                                break;
                            default:
                                // check for gender field here as [[- <male> | <female> | <unspecified> -]]
                                // if it is done here then it will preclude including [[gender]] processing in tbls...
                                // if gender processing is not required in tbls then that's OK.
                                // cannot think if gender processing would be useful in tbls,
                                // or if gender processing is even needed here at UPS
                                if (fldName.Contains("|") && tblDataCols.Contains("Gender")) {
                                    // deal with Gender based fields: [[ <male> | <female> | <unspecified> ]]
                                }
                                break;
                        }
                    }
                    else if (fldName.Substring(0, 1).Equals("^")) {
                        if (sqlPrimaryRows != "" && sqlLinkToPrimary != "" && sqlLinkToMain != "") {
                            if (_priCxn == null) {
                                _priCxn = new SqlConnection(adGrp._cxn(_dbName));
                                _priCxn.Open();
                            }
                            if (priCmd == null) {
                                if (sqlPrimaryRows.Substring(sqlPrimaryRows.Length) == ";") sqlPrimaryRows = sqlPrimaryRows.Substring(0, sqlPrimaryRows.Length - 1);
                                priCmd = new SqlCommand(sqlPrimaryRows + (sqlLinkToPrimary == "" ? "" : " where " + sqlLinkToPrimary + "=@lnk;"), _priCxn);
                                priCmd.Parameters.AddWithValue("@lnk", sqlRdr[sqlLinkToMain].ToString());
                            }
                            if (priRdr == null) {
                                priRdr = priCmd.ExecuteReader();
                                for (int Lp1 = 0; Lp1 < priRdr.FieldCount; Lp1++) {
                                    priDataCols.Add(priRdr.GetName(Lp1).ToLower());
                                }
                                priRdr.Read();
                            }
                            if (priRdr.IsClosed) {
                                if (sqlPrimaryRows.Substring(sqlPrimaryRows.Length) == ";") sqlPrimaryRows = sqlPrimaryRows.Substring(0, sqlPrimaryRows.Length - 1);
                                priCmd = new SqlCommand(sqlPrimaryRows + (sqlLinkToPrimary == "" ? "" : " where " + sqlLinkToPrimary + "=@lnk;"), _priCxn);
                                priCmd.Parameters.AddWithValue("@lnk", sqlRdr[sqlLinkToMain].ToString());
                                priRdr = priCmd.ExecuteReader();
                                priRdr.Read();
                            }
                            if (priDataCols.Contains(fldName.Substring(1))) {
                                fldDest = priRdr[fldName.Substring(1)].ToString();
                            }
                        }
                    }
                    else { // standard table fields
                        if (inTbl) {
                            if (fldName.Contains("|") && tblDataCols.Contains("Gender")) {
                                // gender processing here or as a special field???
                                // deal with Gender based fields: [[ <male> | <female> | <unspecified> ]]
                            }
                            if (tblDataCols.Contains(fldName)) fldDest = tblRdr[fldName].ToString();
                        }
                        else {
                            if (fldName.Contains("|") && fldDataCols.Contains("Gender")) {
                                // gender processing here or as a special field???
                                // deal with Gender based fields: [[ <male> | <female> | <unspecified> ]]
                            }
                            if (fldDataCols.Contains(fldName)) fldDest = sqlRdr[fldName].ToString();
                        }
                    }
                    if (fldDest == null) { fldDest = "[![" + fldName + "]!]"; replaceAll = true; }
                    fldDest += rtfBit;
                    if ((replaceAll && !rtfBit.Equals("")) || inTbl) replaceAll = false; // an || inTbl in the conditional???

                    if (replaceAll) { _rtfDocument.Replace(fldFrom, fldDest); }
                    else { _rtfDocument.Replace(fldFrom, fldDest, ptr1, fldFrom.Length); }
                    File.WriteAllText(Path.Combine(_DocPath, rtfBuiltDoc), _rtfDocument.ToString()); // [[DBG]]
                    rtfCpy = null; // to minimise the impact on memory usage...
                    rtfCpy = _rtfDocument.ToString(); // if [string rtfCpy] is modified, esp. on large documents then memory management will quickly grow out of control.
                    ptr1 = rtfCpy.IndexOf("[[");
                }
                File.WriteAllText(Path.Combine(_DocPath, rtfBuiltDoc), _rtfDocument.ToString());
                _rtfDocument.Clear();
                if (priRdr != null && !priRdr.IsClosed) { priRdr.Close(); }
            }
            if (_priCxn != null) _priCxn.Close();
            sqlRdr.Close();
            _sqlCxn.Close();
        }
    }
}
