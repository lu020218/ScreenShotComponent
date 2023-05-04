/*
 * 作者：赵忠印
 * 时间：2010-9-2 11:23:09
 * 描述：截图组件
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace ScreenShotComponent
{
    public class ScreenShot
    {
        private ScreenShotForm _rectForm;//区域截图框
        private WindowCaptureForm _windowCaptureForm;//捕获窗体图框

        public ScreenShot()
        {
            _windowCaptureForm = new WindowCaptureForm();
            _windowCaptureForm.AfterCaptureScreen += new EventHandler<WindowScreenCaptureEventArgs>(WindowCapture_AfterCapture);
        }

        #region 公共方法、事件

        /// <summary>
        /// 让用户选择区域截图，如果用户按下Esc键或用鼠标右键点击截图区域，则取消截图，返回null
        /// </summary>
        /// <returns>截图</returns>
        public Image GetRangeImage()
        {
            if (_rectForm == null)
            {
                _rectForm = new ScreenShotForm();
            }
            if (_rectForm.Visible == false)
            {
                if (_rectForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return _rectForm.ScreenImage;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取全屏截图
        /// </summary>
        /// <returns>截图</returns>
        public Image GetFullScreenImage()
        {
            Image image = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(image);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), Screen.PrimaryScreen.Bounds.Size);
            return image;
        }

        /// <summary>
        /// 截取窗体
        /// </summary>
        public void OpenWindowImageCaptureForm()
        {
            if (_windowCaptureForm.Visible == false)
            {
                _windowCaptureForm.ShowDialog();
            }
        }

        /// <summary>
        /// 截取窗体后触发
        /// </summary>
        public EventHandler<WindowScreenCaptureEventArgs> AfterCapture;

        #endregion

        private void WindowCapture_AfterCapture(object o, WindowScreenCaptureEventArgs e)
        {
            if (AfterCapture != null)
            {
                AfterCapture(o, e);
            }
        }
    }
}
