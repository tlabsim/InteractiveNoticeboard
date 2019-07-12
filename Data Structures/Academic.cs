using System;
using System.Collections.Generic;
using System.Data;
using TLABS.Extensions;

namespace InteractiveNoticeboard.Data_Structures
{
    public class Batch
    {
        public int BatchID { get; set; }
        public string Degree { get; set; }
        public string Session { get; set; }
        public string BatchName
        {
            get
            {
                return string.Format("{0} {1} session", Degree, Session);
            }
        }
        DateTime? StartedOn { get; set; }
        public bool IsCurrent { get; set; }
        public string CurrentTerm { get; set; }

        public Batch()
        {
            BatchID = -1;
            Degree = string.Empty;
            Session = string.Empty;
            IsCurrent = false;
        }

        public static List<Batch> GetBatches(bool current_only = false)
        {
            List<Batch> batches = new List<Batch>();

            string query = string.Empty;
            if (current_only)
            {
                query = "SELECT * FROM batches WHERE is_current = true ORDER BY degree ASC, started_on DESC";
            }
            else
            {
                query = "SELECT * FROM batches ORDER BY degree ASC, started_on DESC";
            }

            using (DataTable dt = DB_Manager.DBClient.ExecuteAdapter(query))
            {
                if (dt != null)
                {
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        try
                        {
                            Batch batch = new Batch();

                            batch.BatchID = dt.Rows[r]["ID"].GetString().ToInt();
                            batch.Degree = dt.Rows[r]["degree"].GetString().Trim();
                            batch.Session = dt.Rows[r]["session"].GetString().Trim();
                            batch.IsCurrent = dt.Rows[r]["is_current"].GetString().ToBool(false);
                            batch.CurrentTerm = dt.Rows[r]["current_term"].GetString();
                            batch.StartedOn = dt.Rows[r]["started_on"].GetString().ToDateTime();
                            batches.Add(batch);

                            Console.WriteLine("{0}: {1}", batch.BatchID, batch.BatchName);
                        }
                        catch { }
                    }
                }
            }

            return batches;
        }
    }
}
