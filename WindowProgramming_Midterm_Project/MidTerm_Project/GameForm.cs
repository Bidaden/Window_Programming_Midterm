using MidTerm_Project.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MidTerm_Project
{
    public partial class GameForm : Form
    {
        private const int PLANE_OFFSET = 2;

        private int pix_x = 0;
        private int pix_y = 0;
        int shoot_p = 10; // vị trí của đạn khi được bắn ra khỏi máy bay
        int blood_p = 50; // vị trí hiển thị thanh HP của người chơi
        private Image[] bgrounds;
        int index = 0;
        //Image avatar = Resource.imgHeadSheep;     
        //Image boomImg = Resource.bomb4;          // mấy cái ảnh này thêm sau ,lấy cái đẹp hơn
        //Image shotImg = Resource.shotgun;
        //Image bloodImg = Resource.bloodbox;

        bool isDropGun = false;
        bool isDropBloodBox = false;
        public GameForm()
        {
            InitializeComponent();
            this.Size = new Size(420, 630);

        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            InitBackground();
        }
        public void InitBackground()
        {
            bgrounds = new Image[4];
            Random rd = new Random();
            index = rd.Next(0, 4);
            //bgrounds[0] = Resource.background1;
            //bgrounds[1] = Resource.background2;
            //bgrounds[2] = Resource.background3;   // lấy background đẹp saum cái này xấu vãi cứt
            //bgrounds[3] = Resource.background4;
        }
        ///<summary>    
        /// Hàm di chuyển BG
        ///</summary>
        /// <para name="e">para>Đối tượng đồ họa</param>


        public void BackMove(Graphics e) //  Dich chuyển ảnh theo timer để tránh khoảng trống
        {
            e = this.CreateGraphics();
            pix_y += PLANE_OFFSET;
            if (pix_y > 630)  // nếu BG trượt hết màn hình
            {
                pix_y = 0;  // reset điểm vẽ ảnh BG về 0 
            }
        }
        public abstract class DropItem //base class của DropItem 
        {
            protected Image itemImg;
            protected int item_y;
            bool isDrop = false;
            protected int dropSpeed = 5;
            protected bool isRestting = false;
            protected Random rnd = new Random();

            protected Rectangle getRect(int x, int y, int width, int height)
            {
                return new Rectangle(x, y, width, height);
            }

            protected async void MoveItem() // 
            {
                item_y += dropSpeed;

                if (item_y > 950 && !isRestting)
                {
                    isRestting = true;
                    item_y = 1000;

                    await Task.Delay(30000);

                    item_y = -100;
                    isDrop = false;
                    isRestting = false;
                }
            }
            protected bool ShouldDrop(int chance)
            {
                return new Random().Next(0, 20) == chance;
            }
            public abstract void Produce();

        }
        public class ShotGunItem : DropItem
        {
            public ShotGunItem()
            {
                itemImg = Resource.shotgun;
                item_y = 10;
            }

            public override void Produce()
            {
                Rectangle sgRect = GetRect(
                    rnd.Next(0, 350), item_y,
                    itemImg.Width, itemImg.Height);

                Rectangle mpRect = GetRect(
                    MyPlane.x, MyPlane.y,
                    MyPlane.myPlaneImg.Width, MyPlane.myPlaneImg.Height);

                if (ShouldDrop(2))
                {
                    isDrop = true;
                }

                if (isDrop && !MyPlane.isGetGun && sgRect.IntersectsWith(mpRect))
                {
                    MyPlane.isGetGun = true;
                    item_y = -100;
                }

                MoveItem();
            }
        }
        public class BloodBoxItem : DropItem
        {
            public BloodBoxItem()
            {
                itemImg = Resource.bloodbox;
                item_y = 50;
            }

            public override void Produce()
            {
                Rectangle bbRect = GetRect(
                    rnd.Next(0, 390), item_y,
                    itemImg.Width, itemImg.Height);

                Rectangle mpRect = GetRect(
                    MyPlane.x, MyPlane.y,
                    MyPlane.myPlaneImg.Width, MyPlane.myPlaneImg.Height);

                if (ShouldDrop(0))
                {
                    isDrop = true;
                }

                if (isDrop && !MyPlane.isGetBlood)
                {
                    if (bbRect.IntersectsWith(mpRect))
                    {
                        MyPlane.isGetBlood = true;
                        item_y = -100;
                    }
                }

                MoveItem();
            }
        }
    }
}
