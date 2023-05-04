/*
 * 作者：赵忠印
 * 时间：2010-9-3 11:13:08
 * 描述：捕捉窗体的辅助Form
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using ScreenShotComponent.Properties;
using System.IO;

namespace ScreenShotComponent
{
    internal partial class WindowCaptureForm : Form
    {
        private IntPtr _windowHandle;
        private Image _image;
        private Point _windowLocation;
        private Size _windowSize;

        /// <summary>
        /// 捕捉窗体的辅助Form
        /// </summary>
        public WindowCaptureForm()
        {
            InitializeComponent();
            _windowHandle = IntPtr.Zero;
            this.pictureBox1.MouseDown += new MouseEventHandler(pictureBox1_MouseDown);
        }

        //鼠标按下事件
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Stream stream = new MemoryStream(Resources.windowfi);
            this.Cursor = new Cursor(stream);
            this.pictureBox1.Image = Resources._null;
            this.pictureBox1.MouseMove += new MouseEventHandler(pictureBox1_MouseMove);
            this.pictureBox1.MouseUp += new MouseEventHandler(pictureBox1_MouseUp);
        }

        //鼠标抬起事件
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            this.pictureBox1.Image = global::ScreenShotComponent.Properties.Resources.mouse;
            Cursor = Cursors.Default;
            this.pictureBox1.MouseMove -= pictureBox1_MouseMove;
            this.pictureBox1.MouseUp -= pictureBox1_MouseUp;
            if (_windowHandle != IntPtr.Zero)
            {
                WindowsAPI.ShowInvertRectTracker(_windowHandle);
                this.GetScreenImage();
                if (this.AfterCaptureScreen != null)
                {
                    WindowScreenCaptureEventArgs args = new WindowScreenCaptureEventArgs();
                    args.Image = _image;
                    this.AfterCaptureScreen(null, args);
                }
            }
            else
            {
                this.ShowWindowInfo(_windowHandle);//清除信息
            }
        }

        //移动鼠标事件
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (Cursor.Position.X >= this.Location.X
                && Cursor.Position.Y >= this.Location.Y
                && Cursor.Position.X <= this.Location.X + this.Width
                && Cursor.Position.Y <= this.Location.Y + this.Height)//本窗体不再截图范围之内
            {
                if (_windowHandle != IntPtr.Zero)
                {
                    WindowsAPI.ShowInvertRectTracker(_windowHandle);
                    _windowHandle = IntPtr.Zero;
                }
                return;
            }
            IntPtr windowHandle = WindowsAPI.ChildWindowFromPoint(Cursor.Position);

            if (windowHandle != _windowHandle)
            {
                WindowsAPI.ShowInvertRectTracker(_windowHandle);
                _windowHandle = windowHandle;
                WindowsAPI.ShowInvertRectTracker(_windowHandle);
                this.ShowWindowInfo(_windowHandle);
            }
        }

        //显示窗体信息
        private void ShowWindowInfo(IntPtr _windowHandle)
        {
            if (_windowHandle == IntPtr.Zero)
            {
                this.textBoxLocation.Text =
                    this.textBoxName.Text =
                    this.textBoxSize.Text = string.Empty;
            }
            else
            {
                this.textBoxName.Text = WindowsAPI.GetWindowText(_windowHandle);
                Rectangle rect = WindowsAPI.GetWindowRect(_windowHandle);
                this.textBoxSize.Text = rect.Width.ToString() + "*" + rect.Height.ToString();
                this.textBoxLocation.Text = rect.X.ToString() + "," + rect.Y.ToString();
                _windowLocation = rect.Location;
                _windowSize = rect.Size;
            }
        }

        //截图
        private void GetScreenImage()
        {
            WindowsAPI.SetForegroundWindow(_windowHandle);
            _image = new Bitmap(_windowSize.Width, _windowSize.Height);
            Graphics g = Graphics.FromImage(_image);
            Thread.Sleep(300);
            g.CopyFromScreen(_windowLocation, new Point(0, 0), new Size(_windowSize.Width, _windowSize.Height));
            WindowsAPI.SetForegroundWindow(this.Handle);
        }

        #region 公共成员

        /// <summary>
        /// 获取截图
        /// </summary>
        public Image ScreenImage
        {
            get
            {
                return _image;
            }
        }

        /// <summary>
        /// 截取屏幕后触发
        /// </summary>
        public EventHandler<WindowScreenCaptureEventArgs> AfterCaptureScreen;

        #endregion
    }

    /// <summary>
    /// 捕获屏幕事件参数
    /// </summary>
    public class WindowScreenCaptureEventArgs : EventArgs
    {
        /// <summary>
        /// 截图
        /// </summary>
        public Image Image { get; set; }
    }
}
