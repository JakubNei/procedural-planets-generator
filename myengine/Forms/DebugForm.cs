using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyEngine
{
	public partial class DebugForm : Form
	{
		protected override bool ShowWithoutActivation => true;

		public DebugForm()
		{
			InitializeComponent();
			InitializeDataWatchers();
		}

		private void listView2_MouseClick(object sender, MouseEventArgs e)
		{
			var senderList = (ListView)sender;
			if (senderList.SelectedItems.Count == 1 && IsInBound(e.Location, senderList.SelectedItems[0].Bounds))
			{
				var item = senderList.SelectedItems[0];
				var cvar = item.Tag as CVar;
				cvar?.Toogle();
			}
		}

		private static bool IsInBound(Point location, Rectangle bound)
		{
			return (bound.Y <= location.Y &&
					bound.Y + bound.Height >= location.Y &&
					bound.X <= location.X &&
					bound.X + bound.Width >= location.X);
		}




		DictionaryWatcher<string, string> stringValuesWatcher;
		DictionaryWatcher<string, CVar, string> cvarValuesWatcher;

		void InitializeDataWatchers()
		{
			{
				var items = listView1.Items;
				stringValuesWatcher = new DictionaryWatcher<string, string>();
				stringValuesWatcher.OnAdded += (key, item) => items.Add(new ListViewItem(new string[] { key, item }) { Tag = key });
				stringValuesWatcher.OnUpdated += (key, item) => items.OfType<ListViewItem>().First(i => (string)i.Tag == key).SubItems[1].Text = item;
				stringValuesWatcher.OnRemoved += (key, item) => items.Remove(items.OfType<ListViewItem>().First(i => (string)i.Tag == key));
			}
			{
				var items = listView2.Items;
				cvarValuesWatcher = new DictionaryWatcher<string, CVar, string>();
				cvarValuesWatcher.comparisonValueSelector = (item) => item.Bool.ToString();
				cvarValuesWatcher.OnAdded += (key, item) =>
				{
					items.Add(new ListViewItem(new string[] {
								item.toogleKey == OpenTK.Input.Key.Unknown ? string.Empty : item.toogleKey.ToString(),
								item.name,
								item.Bool.ToString()
							})
					{ Tag = item });
				};
				cvarValuesWatcher.OnUpdated += (key, item) =>
				{
					var subItems = items.OfType<ListViewItem>().First(i => i.Tag == item).SubItems;
					subItems[0].Text = item.toogleKey.ToString();
					subItems[2].Text = item.Bool.ToString();
				};
				cvarValuesWatcher.OnRemoved += (key, item) => items.Remove(items.OfType<ListViewItem>().First(i => i.Tag == item));
			}
		}


		public void UpdateBy(IDictionary<string, string> stringValues, IDictionary<string, CVar> nameToCVar)
		{
			stringValuesWatcher.UpdateBy(stringValues);
			cvarValuesWatcher.UpdateBy(nameToCVar);
		}


	}

}
