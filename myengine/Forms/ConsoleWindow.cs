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
	public partial class ConsoleWindow : Form
	{

		public void AppendText(string msg)
		{
			if (Created)
			{
				if (InvokeRequired)
					Invoke((Action)(() => { AppendText(msg + "\n"); }));
				else
				{
					var scrollToBottom = consoleText.SelectionLength == 0 && consoleText.SelectionStart >= consoleText.TextLength - 1;
					consoleText.AppendText(msg);
					if(scrollToBottom)
					{
						consoleText.SelectionStart = consoleText.TextLength - 1;
						consoleText.ScrollToCaret();
					}
				}
			}
		}

		public event Action OnStart;
		public event Action OnExit;

		public ConsoleWindow()
		{
			InitializeComponent();
		}

		public void DoExit()
		{
			if (Created)
			{
				if (InvokeRequired)
					Invoke((Action)(DoExit));
				else
					Close();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			OnStart?.Invoke();
		}
		protected override void OnClosed(EventArgs e)
		{
			OnExit?.Invoke();
		}
	}
}
