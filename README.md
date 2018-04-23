# TBDispl
View a table with save-able filtering and ordering profiles

Call it like this in its most basic form:

      {
            Assembly dllAsm;
            Type typeAsm;

            string pathFull = $@"\\{srvrName}\{Path}\TBDispl.dll");

            try
            {
                List<object> parms = new List<object>();
                parms.Add($"{Database}");
                parms.Add($"{TablName or View}");
                parms.Add(null);
                parms.Add(null);
                parms.Add(null);
                parms.Add(128);

                dllAsm = Assembly.LoadFile(pathFull);
                typeAsm = dllAsm.GetType("TBDispl.ReportSuite");
                MethodInfo method = typeAsm.GetMethod("Show", new Type[] { });
                if (method == null) MessageBox.Show("Method TBDispl ReportSuite Show not found.");

                object obj = Activator.CreateInstance(typeAsm, parms.ToArray());
                method.Invoke(obj, null);
            }
            catch (Exception exc)
            {
                string bert = exc.ToString();
                MessageBox.Show(bert, "Oops");
            }
        }
