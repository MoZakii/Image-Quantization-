using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        
        RGBPixel[,] ImageMatrix;
        List<KeyValuePair<double, int>> NewEdges;
        List<int>[] Clusters;
        RGBPixel[] Nodes = new RGBPixel[ImageOperations.Globals.distinct];
        int[] Colors;
        int k;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix , pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
            string K = TextBoxK.Text;
            k = System.Convert.ToInt32(K);
            long timeBefore = System.Environment.TickCount;
            Nodes = ImageOperations.FillGraph(ImageMatrix);
            NewEdges = ImageOperations.MST(ImageOperations.Globals.distinct, Nodes);
            long timeAfter = System.Environment.TickCount;

            ImageOperations.Globals.Time = timeAfter - timeBefore;
            MST_txt.Text = ImageOperations.Globals.sum.ToString();
            //k = ImageOperations.NumOfClusters(NewEdges);
            Distinct_txt.Text = ImageOperations.Globals.distinct.ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            
            long timeBefore = System.Environment.TickCount;
            Clusters = ImageOperations.Clustering(NewEdges, ImageOperations.Globals.distinct, k);
            
            Colors = ImageOperations.Pallete(Clusters, k, Nodes);
            ImageMatrix = ImageOperations.ImageQuantization(ImageMatrix,Colors,Nodes);
            long timeAfter = System.Environment.TickCount;

            ImageOperations.Globals.Time += timeAfter - timeBefore;
            TimeBoxS.Text = (ImageOperations.Globals.Time/1000).ToString();
            TimeBoxMS.Text = (ImageOperations.Globals.Time%1000).ToString();
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtWidth_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtHeight_TextChanged(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void TextBoxK_TextChanged(object sender, EventArgs e)
        {

        }
    }
}