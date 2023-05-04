﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ScreenShotComponent;
using System.Drawing.Imaging;

namespace Test
{
    public partial class FormMain : Form
    {
        private Image _image;
        private SaveFileDialog _dialog;
        private ScreenShot _screenShot;

        public FormMain()
        {
            InitializeComponent();
            _dialog = new SaveFileDialog();
            _dialog.Filter = "图片文件(*.jpg)|*.jpg";
            _screenShot = new ScreenShot();
            this.Hide();
        }

        private void 全屏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _image = _screenShot.GetFullScreenImage();
            this.Save(_image);
        }

        private void 区域ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _image = _screenShot.GetRangeImage();
            this.Save(_image);
        }

        private void 窗体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_screenShot.AfterCapture == null)
            {
                _screenShot.AfterCapture += new EventHandler<WindowScreenCaptureEventArgs>(ScreenShot_AfterWindowCapture);
            }
            _screenShot.OpenWindowImageCaptureForm();
        }

        private void Save(Image _image)
        {
            if (_image == null)
            {
                return;
            }
            if (_dialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = _dialog.FileName;
                _image.Save(fileName, ImageFormat.Jpeg);
            }
            _image = null;
        }

        private void ScreenShot_AfterWindowCapture(object o, WindowScreenCaptureEventArgs e)
        {
            _image = e.Image;
            this.Save(_image);
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
