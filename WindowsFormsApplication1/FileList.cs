using System;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demiguise
{
    public partial class FileList : Form
    {
        OleDbConnection cnn;
        public bool buttonClicked;
        public String result;
        
        public FileList()
        {

            InitializeComponent();
            buttonClicked = true;
            result = null;
            cnn = new OleDbConnection();
            cnn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\Vijay Kumar Ravi\Documents\GitHub\dge_storage\WindowsFormsApplication1\client_db.mdb";
            OleDbCommand cmd = new OleDbCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT * FROM client_table;";
            cmd.Connection = cnn;
            cnn.Open();
            OleDbDataReader reader = cmd.ExecuteReader();
            using (cnn)
            {
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd.CommandText, cnn))
                {
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    var t = new DataTable();
                    adapter.Fill(t);

                    // Bind the table to the list box
                    listBox1.DisplayMember = "File Name";
                    listBox1.ValueMember = "File Name";
                    listBox1.DataSource = t;
                }
            }

           
        }
        
        private void FileList_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            result = listBox1.GetItemText(listBox1.SelectedItem);
            this.Close();
        }

        
    }
}
