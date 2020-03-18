using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;

namespace PNGCreator
{
	public partial class BackgroundForm : Form
	{
		private string fromPath;
		private string toPath;

		private string appName;
		private float Percent { 
			get {
				try
				{
					return ((tasksCompleted / tasks) * 100); 
				}
				catch
				{
					return 0;
				}
			}}
		private float tasks;
		private int tasksCompleted;

		public BackgroundForm()
		{
			InitializeComponent();
			string psc = Path.DirectorySeparatorChar.ToString();
			fromPath = $"{Directory.GetCurrentDirectory()}{psc}From{psc}";
			toPath = $"{Directory.GetCurrentDirectory()}{psc}To{psc}";

			appName = Text;
			Text = $"{appName}";

			if (!Directory.Exists(fromPath))
				Directory.CreateDirectory(fromPath);

			if (!Directory.Exists(toPath))
				Directory.CreateDirectory(toPath);
		}
		
		private void CreateButton_Click(object sender, EventArgs e)
		{
			tasks = tasksCompleted = 0;
			StartConverts();
		}

		async void StartConverts()
		{
			createButton.Enabled = false;
			Text = $"{appName} (Working...)";

			List<Task> convertTasks = new List<Task>();

			/*
			if (iPhone1_cb.Checked) convertTasks.Add( Task.Run(() => ConvertBatch(toPath, 2688, 1424, false, iPhone1_cb.Text)));
			if (iPhone2_cb.Checked) convertTasks.Add( Task.Run(() => ConvertBatch(toPath, 2436, 1125, false, iPhone2_cb.Text)));
			if (iPad1_cb.Checked) convertTasks.Add( Task.Run(() => ConvertBatch(toPath, 2732, 2048, false, iPad1_cb.Text)));
			if (iPad2_cb.Checked) convertTasks.Add( Task.Run(() => ConvertBatch(toPath, 2732, 2048, false, iPad2_cb.Text)));
			*/
			if (iPhone1_cb.Checked) convertTasks.Add( ConvertBatch(toPath, 2688, 1424, crop_cb.Checked, iPhone1_cb.Text));
			if (iPhone2_cb.Checked) convertTasks.Add(ConvertBatch(toPath, 2436, 1125, crop_cb.Checked, iPhone2_cb.Text));
			if (iPad1_cb.Checked) convertTasks.Add(ConvertBatch(toPath, 2732, 2048, crop_cb.Checked, iPad1_cb.Text));

			if (iPhone1_p_cb.Checked) convertTasks.Add(ConvertBatch(toPath, 1424, 2688, crop_cb.Checked, iPhone1_p_cb.Text));
			if (iPhone2_p_cb.Checked) convertTasks.Add(ConvertBatch(toPath, 1125, 2436, crop_cb.Checked, iPhone2_p_cb.Text));
			if (iPad1_p_cb.Checked) convertTasks.Add(ConvertBatch(toPath, 2048, 2732, crop_cb.Checked, iPad1_p_cb.Text));

			if (android1_cb.Checked) convertTasks.Add(ConvertBatch(toPath, 1024, 500, crop_cb.Checked, android1_cb.Text, true));
			if (android1_p_cb.Checked) convertTasks.Add(ConvertBatch(toPath, 500, 1024, crop_cb.Checked, android1_p_cb.Text, true));
			if (android2_cb.Checked) convertTasks.Add(ConvertBatch(toPath, 4096, 2304, crop_cb.Checked, android2_cb.Text));

			if (android3_cb.Checked) convertTasks.Add(ConvertBatch(toPath, 512, 512, crop_cb.Checked, android3_cb.Text));

			//foreach (var v in convertTasks) v.Start();
			//Task.WaitAll(convertTasks.ToArray());

			await Task.Delay(1000);
		
			createButton.Enabled = true;
		}

		async Task ConvertBatch(string path, int targetWidth, int targetHeight, bool crop, string finalName, bool removeAlpha = false)
		{
			Bitmap fromImage;
			Bitmap newImage = new Bitmap(targetWidth, targetHeight);

			int tasksHere = Directory.GetFiles(fromPath).Length;
			tasks += tasksHere;

			crop = true;

			for (int i = 0; i < tasksHere; i++)
			{
				if (crop)
				{
					//Method 1, crop
					using (FileStream fromStream = new FileStream(Directory.GetFiles(fromPath)[i], FileMode.Open))
					{
						fromImage = new Bitmap(Image.FromStream(fromStream), new Size(targetWidth, targetHeight));
					}
					fromImage.Save($"{toPath}{finalName}_{i}.png", (removeAlpha)? System.Drawing.Imaging.ImageFormat.Jpeg: System.Drawing.Imaging.ImageFormat.Png);
				}
				else
				{
					//Method 2, scale
					using (FileStream fromStream = new FileStream(Directory.GetFiles(fromPath)[i], FileMode.Open))
					{
						fromImage = new Bitmap(Image.FromStream(fromStream));
					}

					//with a ratio, a pixel will correspond to one of the ratio distance away
					float ratioX = (float)fromImage.Width  / targetWidth;
					float ratioY = (float)fromImage.Height / targetHeight;
					Color color;

					//set all pixels in _newText
					for (int x = 0; x < targetWidth; x++)
					{
						for (int y = 0; y < targetHeight; y++)
						{
							//there are no decimal pixels, so remove the decimals
							int posX = (int)(x * ratioX);
							int posY = (int)(y * ratioY);

							color = fromImage.GetPixel(posX, posY);
							newImage.SetPixel(x, y, color);
						}
					}
					newImage.Save($"{toPath}{finalName}_{i}.png", (removeAlpha) ? System.Drawing.Imaging.ImageFormat.Jpeg : System.Drawing.Imaging.ImageFormat.Png);

					//newImage.Save(toPath + $"{finalName}_{i}.png");
				}

				await Task.Delay(1);
				tasksCompleted++;
				Text = $"{appName} ({Percent.ToString("##0")}%)";
				await Task.Delay(10);
			}

			fromImage = null;
			newImage = null;

			GC.Collect();
		}
	}
}
