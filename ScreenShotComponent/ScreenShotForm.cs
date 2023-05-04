/*
 * 作者：赵忠印
 * 时间：2010-9-1 17:19:41
 * 描述：截图矩形框Form
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace ScreenShotComponent
{
    internal partial class ScreenShotForm : Form
    {
        private Image _image;//截取的图片数据

        private readonly int CON_MIN_WIDTH = 200;//最小高度
        private readonly int CON_MIN_HEIGHT = 100;//最小长度
        //
        //拖拽相关
        //
        private int _oldX, _oldY;//鼠标移动前坐标
        private int _newX, _newY;//鼠标移动后坐标
        private int _distanceX, _distanceY;//鼠标移动的位移

        public ScreenShotForm()
        {
            InitializeComponent();
            this.InitForm();
        }

        //初始化工作
        private void InitForm()
        {
            this.WindowState = FormWindowState.Normal;
            this.Opacity = 0.4;
            //
            //鼠标形状
            //
            this.Cursor = Cursors.SizeAll;
            this.panelMidLeft.Cursor = this.panelMidRight.Cursor = Cursors.SizeWE;
            this.panelUpMid.Cursor = this.panelDownMid.Cursor = Cursors.SizeNS;
            this.panelUpLeft.Cursor = this.panelDownRight.Cursor = Cursors.SizeNWSE;
            this.panelUpRight.Cursor = this.panelDownLeft.Cursor = Cursors.SizeNESW;
            //
            //添加拖拽大小事件
            //
            this.panelDownLeft.MouseDown += new MouseEventHandler(panel_MouseDown);
            this.panelDownMid.MouseDown += new MouseEventHandler(panel_MouseDown);
            this.panelDownRight.MouseDown += new MouseEventHandler(panel_MouseDown);
            this.panelMidLeft.MouseDown += new MouseEventHandler(panel_MouseDown);
            this.panelMidRight.MouseDown += new MouseEventHandler(panel_MouseDown);
            this.panelUpLeft.MouseDown += new MouseEventHandler(panel_MouseDown);
            this.panelUpMid.MouseDown += new MouseEventHandler(panel_MouseDown);
            this.panelUpRight.MouseDown += new MouseEventHandler(panel_MouseDown);
            //
            //添加拖拽位置事件
            //
            this.MouseDown += new MouseEventHandler(screenShotForm_MouseDown);
            //
            //其他事件
            //
            this.DoubleClick += delegate(object o, EventArgs e) { this.GetScreenImage(); };
            this.LocationChanged += delegate(object o, EventArgs e) { this.ViewInfor(); };
            this.SizeChanged += delegate(object o, EventArgs e) { this.ViewInfor(); };
            this.KeyDown += delegate(object o,KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Abort;
                }
            };
            this.MouseClick += new MouseEventHandler(ScreenShotForm_MouseClick);
        }

        #region 拖拽大小相关

        private void panel_MouseDown(object sender, MouseEventArgs e)
        {
            _oldX = Cursor.Position.X;
            _oldY = Cursor.Position.Y;

            this.panelDownLeft.MouseMove += new MouseEventHandler(panel_MouseMove);
            this.panelDownMid.MouseMove += new MouseEventHandler(panel_MouseMove);
            this.panelDownRight.MouseMove += new MouseEventHandler(panel_MouseMove);
            this.panelMidLeft.MouseMove += new MouseEventHandler(panel_MouseMove);
            this.panelMidRight.MouseMove += new MouseEventHandler(panel_MouseMove);
            this.panelUpLeft.MouseMove += new MouseEventHandler(panel_MouseMove);
            this.panelUpMid.MouseMove += new MouseEventHandler(panel_MouseMove);
            this.panelUpRight.MouseMove += new MouseEventHandler(panel_MouseMove);

            this.panelDownLeft.MouseUp += new MouseEventHandler(panel_MouseUp);
            this.panelDownMid.MouseUp += new MouseEventHandler(panel_MouseUp);
            this.panelDownRight.MouseUp += new MouseEventHandler(panel_MouseUp);
            this.panelMidLeft.MouseUp += new MouseEventHandler(panel_MouseUp);
            this.panelMidRight.MouseUp += new MouseEventHandler(panel_MouseUp);
            this.panelUpLeft.MouseUp += new MouseEventHandler(panel_MouseUp);
            this.panelUpMid.MouseUp += new MouseEventHandler(panel_MouseUp);
            this.panelUpRight.MouseUp += new MouseEventHandler(panel_MouseUp);
        }

        private void panel_MouseUp(object sender, MouseEventArgs e)
        {
            this.panelDownLeft.MouseMove -= new MouseEventHandler(panel_MouseMove);
            this.panelDownMid.MouseMove -= new MouseEventHandler(panel_MouseMove);
            this.panelDownRight.MouseMove -= new MouseEventHandler(panel_MouseMove);
            this.panelMidLeft.MouseMove -= new MouseEventHandler(panel_MouseMove);
            this.panelMidRight.MouseMove -= new MouseEventHandler(panel_MouseMove);
            this.panelUpLeft.MouseMove -= new MouseEventHandler(panel_MouseMove);
            this.panelUpMid.MouseMove -= new MouseEventHandler(panel_MouseMove);
            this.panelUpRight.MouseMove -= new MouseEventHandler(panel_MouseMove);

            this.panelDownLeft.MouseUp -= new MouseEventHandler(panel_MouseUp);
            this.panelDownMid.MouseUp -= new MouseEventHandler(panel_MouseUp);
            this.panelDownRight.MouseUp -= new MouseEventHandler(panel_MouseUp);
            this.panelMidLeft.MouseUp -= new MouseEventHandler(panel_MouseUp);
            this.panelMidRight.MouseUp -= new MouseEventHandler(panel_MouseUp);
            this.panelUpLeft.MouseUp -= new MouseEventHandler(panel_MouseUp);
            this.panelUpMid.MouseUp -= new MouseEventHandler(panel_MouseUp);
            this.panelUpRight.MouseUp -= new MouseEventHandler(panel_MouseUp);
        }

        private void panel_MouseMove(object sender, MouseEventArgs e)
        {
            _newX = Cursor.Position.X;
            _newY = Cursor.Position.Y;

            _distanceX = _newX - _oldX;
            _distanceY = _newY - _oldY;

            Panel temp = sender as Panel;
            this.Drag(temp, _distanceX, _distanceY);
            _oldX = _newX;
            _oldY = _newY;
        }

        //拖拽大小实现
        private void Drag(Panel dotPanel, int distanceX, int distanceY)
        {
            if (dotPanel == this.panelMidLeft)//改变横向位置和宽度
            {
                if (this.Width - distanceX < CON_MIN_WIDTH)
                {
                    return;
                }
                this.Location = new Point(this.Location.X + distanceX, this.Location.Y);
                this.Size = new Size(this.Width - distanceX, this.Height);
            }
            else if (dotPanel == this.panelMidRight)//改变宽度
            {
                if (this.Width + distanceX < CON_MIN_WIDTH)
                {
                    return;
                }
                this.Size = new Size(this.Width + distanceX, this.Height);
            }
            else if (dotPanel == this.panelUpMid)//改变纵向位置和大小
            {
                if (this.Height - distanceY < CON_MIN_HEIGHT)
                {
                    return;
                }
                this.Location = new Point(this.Location.X, this.Location.Y + distanceY);
                this.Size = new Size(this.Width, this.Height - distanceY);
            }
            else if (dotPanel == this.panelDownMid)//改变大小
            {
                if (this.Height + distanceY < CON_MIN_HEIGHT)
                {
                    return;
                }
                this.Size = new Size(this.Width, this.Height + distanceY);
            }
            else if (dotPanel == this.panelUpLeft)//改变横向、纵向位置和大小
            {
                if (this.Width - distanceX < CON_MIN_WIDTH)
                {
                    distanceX = 0;
                }
                if (this.Height - _distanceY < CON_MIN_HEIGHT)
                {
                    distanceY = 0;
                }
                this.Location = new Point(this.Location.X + distanceX, this.Location.Y + distanceY);
                this.Size = new Size(this.Width - distanceX, this.Height - distanceY);
            }

            else if (dotPanel == this.panelUpRight)//改变纵向方向、改变大小
            {
                if (this.Width + distanceX < CON_MIN_WIDTH)
                {
                    distanceX = 0;
                }
                if (this.Height - distanceY < CON_MIN_HEIGHT)
                {
                    distanceY = 0;
                }
                this.Location = new Point(this.Location.X, this.Location.Y + distanceY);
                this.Size = new Size(this.Width + distanceX, this.Height - distanceY);
            }
            else if (dotPanel == this.panelDownLeft)//改变横向方向和大小
            {
                if (this.Width - distanceX < CON_MIN_WIDTH)
                {
                    distanceX = 0;
                }
                if (this.Height + distanceY < CON_MIN_HEIGHT)
                {
                    distanceY = 0;
                }
                this.Location = new Point(this.Location.X + distanceX, this.Location.Y);
                this.Size = new Size(this.Width - distanceX, this.Height + distanceY);
            }
            else if (dotPanel == this.panelDownRight)//改变大小
            {
                if (this.Width + distanceX < CON_MIN_WIDTH)
                {
                    distanceX = 0;
                }
                if (this.Height + _distanceY < CON_MIN_HEIGHT)
                {
                    distanceY = 0;
                }
                this.Size = new Size(this.Width + distanceX, this.Height + distanceY);
            }
        }

        #endregion

        #region 拖拽位置实现

        //鼠标按下事件
        private void screenShotForm_MouseDown(object sender, MouseEventArgs e)
        {
            this.MouseUp += new MouseEventHandler(screenShotForm_MouseUp);
            this.MouseMove += new MouseEventHandler(screenShotForm_MouseMove);
            _oldX = Cursor.Position.X;
            _oldY = Cursor.Position.Y;
        }

        //鼠标移动事件
        private void screenShotForm_MouseMove(object sender, MouseEventArgs e)
        {
            _newX = Cursor.Position.X;
            _newY = Cursor.Position.Y;

            _distanceX = _newX - _oldX;
            _distanceY = _newY - _oldY;

            this.Location = new Point(this.Location.X + _distanceX, this.Location.Y + _distanceY);

            _oldX = _newX;
            _oldY = _newY;
        }

        //鼠标Up事件
        private void screenShotForm_MouseUp(object sender, MouseEventArgs e)
        {
            this.MouseMove -= screenShotForm_MouseMove;
            this.MouseUp -= screenShotForm_MouseUp;
        }

        #endregion

        #region 公共方法、属性

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

        #endregion

        //绘制边框
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ControlPaint.DrawBorder(e.Graphics, new Rectangle(0, 0, this.Width, this.Height), Color.Red, 2, ButtonBorderStyle.Dotted,
                Color.Red, 2, ButtonBorderStyle.Dotted, Color.Red, 2, ButtonBorderStyle.Dotted, Color.Red, 2, ButtonBorderStyle.Dotted);
        }

        //双击获取截图
        private void GetScreenImage()
        {
            _image = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(_image);
            g.CopyFromScreen(this.Location, new Point(0, 0), new Size(this.Width, this.Height));
            this.DialogResult = DialogResult.OK;
        }

        //显示截图位置和大小
        private void ViewInfor()
        {
            this.labelHeight.Text = this.Height.ToString();
            this.labelWidth.Text = this.Width.ToString();
            this.labelX.Text = this.Location.X.ToString();
            this.labelY.Text = this.Location.Y.ToString();
        }

        //点击右键关闭窗体
        private void ScreenShotForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.DialogResult = DialogResult.Abort;
            }
        }
    }
}
