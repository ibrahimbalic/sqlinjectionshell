using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Web.Configuration;
using System.Data.SqlClient;

namespace WebApplication1
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        private DataTable dataTable = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            string query = "";

            SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["dbbagla"].ConnectionString);

            if (Request.QueryString["id"] != null)
            {
                query = "select * from haberler where id like '%" + Request.QueryString["id"].ToString() + "%'";
            }
            else if (Request.Form["id"] != null)
            {
                query = "select * from haberler where id like '%" +Request.Form["id"].ToString()+ "%'";
            }
            else
            {
                query = "select * from haberler";
            }

            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dataTable);
            conn.Close();
            da.Dispose();

            gridView.DataSource = dataTable;
            gridView.DataBind();

        }

        public void db()
        {
          

        }
    }
}