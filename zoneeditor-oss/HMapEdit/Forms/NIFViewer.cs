using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HMapEdit.Forms
{
	public partial class NIFViewer : Form
	{
		public NIFViewer()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			nifList.SelectedIndexChanged += NifList_SelectedIndexChanged;
			nifList.Columns.Add(new ColumnHeader { Name = "FileName", Text = "File name" });
			nifList.Columns.Add(new ColumnHeader { Name = "ID", Text = "ID" });
			nifList.Columns.Add(new ColumnHeader { Name = "Group", Text = "Group" });
			nifList.Items.AddRange(Objects.NIFs.Values.Select(n => new ListViewItem(new[] { n.FileName, n.ID.ToString(), n.Group })).ToArray());
		}

		private void NifList_SelectedIndexChanged(object sender, EventArgs e)
		{
			nifRenderer.nif = null;
			if (nifList.SelectedItems.Count != 1)
				return;
			var selected = nifList.SelectedItems[0].Text;
			nifRenderer.nif = Objects.NIFs.Values.FirstOrDefault(n => n.FileName == selected)?.Model;
		}
	}
}
