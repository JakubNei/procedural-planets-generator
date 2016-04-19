using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace MyEngine.Panels
{
    public partial class DebugValuesTable : Form
    {

        DataTable dt;


        public DebugValuesTable()
        {
            InitializeComponent();

            DataSet ds = new DataSet();
            dt = new DataTable("MyTable");
            dt.Columns.Add(new DataColumn("name", typeof(string)));
            dt.Columns.Add(new DataColumn("value", typeof(string)));
            ds.Tables.Add(dt);

            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = dt.TableName;

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }


        Dictionary<string, DataRow> debugValueToDataRow = new Dictionary<string, DataRow>();
        private void timer1_Tick(object sender, EventArgs e)
        {

            Dictionary<string, string> data_copy;
            lock (Debug.stringValues)
            {
                data_copy = Debug.stringValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            foreach (var kvp in data_copy)
            {
                DataRow dr;
                if(debugValueToDataRow.TryGetValue(kvp.Key, out dr) == false)
                {
                    dr = dt.NewRow();
                    dr["name"] = kvp.Key;
                    dt.Rows.Add(dr);
                    debugValueToDataRow[kvp.Key] = dr;
                }
                dr["value"] = kvp.Value;
            }
        }
    }
}

