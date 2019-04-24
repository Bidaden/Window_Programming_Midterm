using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

// Suppress naming warnings for timer event handlers.
// These names (timer1_Tick, timer2_Tick) are hardwired in GameForm.Designer.cs
// and CANNOT be renamed — doing so would break the timer connection.
#pragma warning disable IDE1006

namespace Myplanegame
{
    public partial class GameForm : Form
    {
        private const int BG_SPEED = 2;

        // These change every frame — cannot be readonly
        private int pix_y = 0;
        private int shot_y = 10;
        private int blood_y = 50;
        private bool isDropGun = false;
        private bool isDropBox = false;
        private int bgIndex = 0;

        // readonly: assigned once in constructor, never reassigned
        private readonly Image[] bgrounds = new Image[4];
        private readonly Image avatarImg;
        private readonly Image boomImg;
        private readonly Image shotImg;
        private readonly Image bloodImg;
        private readonly MyPlane plane;

        public GameForm()
        {
            InitializeComponent();
            this.Size = new Size(420, 630);

            avatarImg = Resource.imgHeadSheep;
            boomImg = Resource.bomb4;
            shotImg = Resource.shotgun;
            bloodImg = Resource.bloodbox;
            plane = new MyPlane();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bgrounds[0] = Resource.background1;
            bgrounds[1] = Resource.background2;
            bgrounds[2] = Resource.background3;
            bgrounds[3] = Resource.background4;
            bgIndex = new Random().Next(0, 4);
        }

        private void DrawGame(Graphics g)
        {
            // --- Scrolling background ---
            pix_y += BG_SPEED;
            if (pix_y > 630) pix_y = 0;
            g.DrawImage(bgrounds[bgIndex], 0, pix_y, 420, 630);
            g.DrawImage(bgrounds[bgIndex], 0, pix_y - 630, 420, 630);

            // --- HUD ---
            g.DrawImage(avatarImg, 10, 10);
            g.DrawRectangle(new Pen(Color.Black), new Rectangle(10, 100, 100, 10));
            g.FillRectangle(Brushes.Red, 10, 101, plane.health, 9);
            g.DrawRectangle(new Pen(Color.Blue), new Rectangle(10, 120, 100, 10));
            g.FillRectangle(Brushes.Green, 11, 121, plane.score, 9);
            g.DrawString("Player: Bi", new Font("Arial", 9, FontStyle.Bold), Brushes.Yellow, new System.Drawing.Point(10, 140));
            g.DrawString("Score: " + plane.score, new Font("Arial", 9, FontStyle.Bold), Brushes.Yellow, new System.Drawing.Point(10, 160));

            // --- Player plane ---
            plane.Move();
            plane.Draw(g);

            // --- Player bullets ---
            MyBullet.ProduceMybul(plane);
            MyBullet.MoveMybul(g);
            MyBullet.IsHitEnemy(plane);

            // --- Enemy planes (EnemyPlane — sibling of MyPlane under Plane) ---
            EnemyPlane.ProduceFighter();
            EnemyPlane.FighterMove(g);

            // --- Enemy bullets ---
            EnemyBullet.ProduceEnbul(plane);
            EnemyBullet.MoveEnbul(g);
            EnemyBullet.HitPlane(plane);

            // --- Power-ups ---
            ProduceShotGun();
            ProduceBlood();

            if (isDropGun && !plane.isGetGun)
                g.DrawImage(shotImg, 200, shot_y);
            if (isDropBox && !plane.isGetBlood)
                g.DrawImage(bloodImg, 350, blood_y);
        }

        private void ProduceShotGun()
        {
            if (new Random().Next(0, 100) == 2) isDropGun = true;

            if (isDropGun && !plane.isGetGun)
            {
                var sgRect = new Rectangle(200, shot_y, shotImg.Width, shotImg.Height);
                if (sgRect.IntersectsWith(plane.GetBounds()))
                {
                    plane.isGetGun = true;
                    shot_y = -100;
                }
                shot_y += 5;
                if (shot_y > 950) shot_y = -100;
            }
        }

        private void ProduceBlood()
        {
            if (new Random().Next(0, 100) == 0) isDropBox = true;

            if (isDropBox && !plane.isGetBlood)
            {
                var bbRect = new Rectangle(350, blood_y, bloodImg.Width, bloodImg.Height);
                if (bbRect.IntersectsWith(plane.GetBounds()))
                {
                    plane.isGetBlood = true;
                    if (plane.health <= 90) plane.health += 10;
                    blood_y = -100;
                }
                blood_y += 5;
                if (blood_y > 950) blood_y = -100;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Bitmap buffer = new Bitmap(
                this.ClientRectangle.Width - 1,
                this.ClientRectangle.Height - 1);
            using (Graphics g = Graphics.FromImage(buffer))
                DrawGame(g);
            e.Graphics.DrawImage(buffer, 0, 0);
            base.OnPaint(e);
        }

        // These names MUST match GameForm.Designer.cs exactly — do not rename.
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            using (Graphics g = this.CreateGraphics())
            {
                for (int j = EnemyPlane.fighters.Count - 1; j >= 0; j--)
                {
                    if (EnemyPlane.fighters[j].flag)
                    {
                        g.DrawImage(boomImg, EnemyPlane.fighters[j].GetLoc());
                        new SoundPlayer(Resource.BOMB21).Play();
                        EnemyPlane.fighters.RemoveAt(j);
                    }
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            MyPlane.Keydown(e.KeyCode);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            MyPlane.Keyup(e.KeyCode);
            plane.ResetImage();   // public method — no direct access to protected 'image'
        }
    }
}

#pragma warning restore IDE1006