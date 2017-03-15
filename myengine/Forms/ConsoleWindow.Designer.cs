namespace MumApp1
{
	partial class ConsoleWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.consoleText = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// consoleText
			// 
			this.consoleText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.consoleText.BackColor = System.Drawing.Color.Black;
			this.consoleText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.consoleText.CausesValidation = false;
			this.consoleText.Font = new System.Drawing.Font("Consolas", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(238)));
			this.consoleText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.consoleText.HideSelection = false;
			this.consoleText.Location = new System.Drawing.Point(5, 1);
			this.consoleText.Name = "consoleText";
			this.consoleText.ReadOnly = true;
			this.consoleText.Size = new System.Drawing.Size(705, 356);
			this.consoleText.TabIndex = 0;
			this.consoleText.Text = "";
			// 
			// ConsoleWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(710, 358);
			this.Controls.Add(this.consoleText);
			this.Name = "ConsoleWindow";
			this.Text = "Přetáhní na me soubor";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox consoleText;
	}
}