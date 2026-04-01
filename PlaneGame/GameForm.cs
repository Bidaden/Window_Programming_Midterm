using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Media;
using System.Windows.Forms;

#pragma warning disable IDE1006  // timer1_Tick / timer2_Tick locked by Designer.cs

namespace Myplanegame
{
    public partial class GameForm : Form
    {
        // ── Virtual canvas — game always renders at this size, then scales ──
        private const int BASE_W = 420;
        private const int BASE_H = 630;
        private const int BG_SPEED = 2;

        // ── Score file — saved next to the .exe ──
        private static readonly string SCORE_FILE = "highscores.txt";
        private const int MAX_SCORES = 5;   // keep top 5 runs

        // ── Game states ──
        private enum GameState { Menu, Playing, GameOver, ScoreBoard }
        private GameState state = GameState.Menu;

        // ── Rendering ──
        private int pix_y = 0;
        private int bgIndex = 0;

        // ── Power-ups ──
        private int shot_y = 10;
        private int blood_y = 50;
        private bool isDropGun = false;
        private bool isDropBox = false;

        // ── Game-over menu: which button is hovered ──
        private int hoveredButton = -1;   // -1 = none, 0/1/2 = button index

        // ── Assets (readonly: set once in constructor) ──
        private readonly Image[] bgrounds = new Image[4];
        private readonly Image avatarImg;
        private readonly Image boomImg;
        private readonly Image shotImg;
        private readonly Image bloodImg;
        private readonly MyPlane plane;

        // ── Fonts ──
        private readonly Font titleFont = new Font("Arial", 36, FontStyle.Bold);
        private readonly Font subFont = new Font("Arial", 14, FontStyle.Regular);
        private readonly Font hintFont = new Font("Arial", 11, FontStyle.Italic);
        private readonly Font btnFont = new Font("Arial", 13, FontStyle.Bold);
        private readonly Font scoreFont = new Font("Arial", 12, FontStyle.Regular);
        private readonly Font bigFont = new Font("Arial", 28, FontStyle.Bold);

        // ── Menu buttons ──
        private readonly Rectangle menuPlayBtn = new Rectangle(110, 400, 200, 44);  // Play (normal window)
        private readonly Rectangle menuFullscreenBtn = new Rectangle(110, 458, 200, 44);  // Play fullscreen

        // ── Button rectangles for GameOver menu (in virtual canvas coords) ──
        private readonly Rectangle[] gameOverBtns = new Rectangle[]
        {
            new Rectangle(110, 320, 200, 44),   // 0 = Play Again
            new Rectangle(110, 378, 200, 44),   // 1 = Check My Score
            new Rectangle(110, 436, 200, 44),   // 2 = Exit
        };
        private readonly string[] gameOverLabels = { "Play Again", "Check My Score", "Exit" };

        // ── Back button on ScoreBoard ──
        private readonly Rectangle backBtn = new Rectangle(110, 530, 200, 44);

        // ── Hovered button on Menu ──
        private int hoveredMenuButton = -1;   // 0 = Play, 1 = Fullscreen

        public GameForm()
        {
            InitializeComponent();
            this.Size = new Size(BASE_W, BASE_H);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            avatarImg = Resource.imgHeadSheep;
            boomImg = Resource.bomb4;
            shotImg = Resource.shotgun;
            bloodImg = Resource.bloodbox;
            plane = new MyPlane();

            this.MouseClick += OnMouseClick;
            this.MouseMove += OnMouseMove;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bgrounds[0] = Resource.background1;
            bgrounds[1] = Resource.background2;
            bgrounds[2] = Resource.background3;
            bgrounds[3] = Resource.background4;
            bgIndex = new Random().Next(0, 4);
        }

        //  SCORE FILE HELPERS

        private void SaveScore(int score)
        {
            // Read existing scores
            int[] scores = LoadScores();

            // Append new score and sort descending, keep top MAX_SCORES
            int[] all = new int[scores.Length + 1];
            scores.CopyTo(all, 0);
            all[scores.Length] = score;
            Array.Sort(all);
            Array.Reverse(all);   // descending

            int keep = Math.Min(all.Length, MAX_SCORES);
            string[] lines = new string[keep];
            for (int i = 0; i < keep; i++)
                lines[i] = all[i].ToString();

            File.WriteAllLines(SCORE_FILE, lines);
        }

        private int[] LoadScores()
        {
            if (!File.Exists(SCORE_FILE)) return new int[0];
            string[] lines = File.ReadAllLines(SCORE_FILE);
            int[] result = new int[lines.Length];
            for (int i = 0; i < lines.Length; i++)
                int.TryParse(lines[i].Trim(), out result[i]);
            return result;
        }

        private int LoadHighScore()
        {
            int[] scores = LoadScores();
            return scores.Length > 0 ? scores[0] : 0;
        }

        //  INPUT — mouse click and hover

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (state != GameState.GameOver && state != GameState.ScoreBoard) return;

            // Convert real mouse position → virtual canvas position
            Point vp = ToVirtual(e.Location);

            if (state == GameState.Menu)
            {
                hoveredMenuButton = -1;
                if (menuPlayBtn.Contains(vp)) hoveredMenuButton = 0;
                else if (menuFullscreenBtn.Contains(vp)) hoveredMenuButton = 1;
            }
            else if (state == GameState.GameOver)
            {
                hoveredButton = -1;
                for (int i = 0; i < gameOverBtns.Length; i++)
                    if (gameOverBtns[i].Contains(vp)) { hoveredButton = i; break; }
            }
            else if (state == GameState.ScoreBoard)
            {
                hoveredButton = backBtn.Contains(vp) ? 0 : -1;
            }

            this.Invalidate();
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            Point vp = ToVirtual(e.Location);

            switch (state)
            {
                case GameState.Menu:
                    if (menuPlayBtn.Contains(vp))
                    {
                        // Play at normal window size
                        state = GameState.Playing;
                    }
                    else if (menuFullscreenBtn.Contains(vp))
                    {
                        // Play at 70% of screen size
                        state = GameState.Playing;
                        int w = (int)(Screen.PrimaryScreen.Bounds.Width * 0.7f);
                        int h = (int)(Screen.PrimaryScreen.Bounds.Height * 0.7f);
                        this.FormBorderStyle = FormBorderStyle.FixedSingle;
                        this.WindowState = FormWindowState.Normal;
                        this.Size = new Size(w, h);
                    }
                    break;

                case GameState.Playing:
                    // In-game click = toggle fullscreen only
                    if (this.WindowState == FormWindowState.Maximized)
                        ExitFullscreen();
                    else
                        GoFullscreen();
                    break;

                case GameState.GameOver:
                    if (gameOverBtns[0].Contains(vp))       // Play Again
                    {
                        ResetGame();
                        state = GameState.Playing;
                    }
                    else if (gameOverBtns[1].Contains(vp))  // Check My Score
                    {
                        state = GameState.ScoreBoard;
                    }
                    else if (gameOverBtns[2].Contains(vp))  // Exit
                    {
                        Application.Exit();
                    }
                    break;

                case GameState.ScoreBoard:
                    if (backBtn.Contains(vp))
                        state = GameState.GameOver;
                    break;
            }
        }

        // Convert a real screen point → virtual 420x630 canvas point
        private Point ToVirtual(Point screen)
        {
            float scaleX = (float)this.ClientSize.Width / BASE_W;
            float scaleY = (float)this.ClientSize.Height / BASE_H;
            return new Point((int)(screen.X / scaleX), (int)(screen.Y / scaleY));
        }

        //  FULLSCREEN

        private void GoFullscreen()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }

        private void ExitFullscreen()
        {
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Size = new Size(BASE_W, BASE_H);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && this.WindowState == FormWindowState.Maximized)
                ExitFullscreen();
            base.OnKeyDown(e);
        }

        //  GAME RESET

        private void ResetGame()
        {
            // Reset plane state
            MyPlane.isGameOver = false;
            plane.health = 100;
            plane.score = 0;
            plane.isGetGun = false;
            plane.isGetBlood = false;
            plane.ResetImage();

            // Clear all game objects
            MyBullet.mybulList.Clear();
            EnemyBullet.enbullist.Clear();
            EnemyPlane.fighters.Clear();

            // Reset power-up positions
            shot_y = 10;
            blood_y = 50;
            isDropGun = false;
            isDropBox = false;
            pix_y = 0;

            hoveredButton = -1;
        }

        //  PAINT — draw into 420x630 canvas, stretch to window

        protected override void OnPaint(PaintEventArgs e)
        {
            using (Bitmap canvas = new Bitmap(BASE_W, BASE_H))
            using (Graphics g = Graphics.FromImage(canvas))
            {
                switch (state)
                {
                    case GameState.Menu: DrawMenu(g); break;
                    case GameState.Playing: DrawGame(g); break;
                    case GameState.GameOver: DrawGameOver(g); break;
                    case GameState.ScoreBoard: DrawScoreBoard(g); break;
                }

                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.DrawImage(canvas,
                    new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height),
                    new Rectangle(0, 0, BASE_W, BASE_H),
                    GraphicsUnit.Pixel);
            }
            base.OnPaint(e);
        }

        //  MENU SCREEN

        private void DrawMenu(Graphics g)
        {
            g.DrawImage(bgrounds[bgIndex], 0, 0, BASE_W, BASE_H);

            using (SolidBrush overlay = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                g.FillRectangle(overlay, 0, 0, BASE_W, BASE_H);

            // Title
            DrawCentered(g, "PLANE GAME", titleFont, Brushes.White, 130);
            DrawCentered(g, "by Bi", subFont, Brushes.LightGray, 195);

            // High score display
            int hi = LoadHighScore();
            string hiText = hi > 0 ? $"Best: {hi}" : "No record yet";
            DrawCentered(g, hiText, hintFont, Brushes.Gold, 228);

            g.DrawLine(new Pen(Color.White, 1), 80, 258, 340, 258);

            // Controls
            string[] controls = {
                "W A S D  —  Move",
                "J        —  Shoot",
                "Click    —  Fullscreen / toggle",
                "Escape   —  Exit fullscreen"
            };
            float cy = 272;
            foreach (string line in controls)
            {
                SizeF sz = g.MeasureString(line, hintFont);
                g.DrawString(line, hintFont, Brushes.LightYellow,
                    (BASE_W - sz.Width) / 2f, cy);
                cy += 26;
            }

            // Buttons
            DrawButton(g, menuPlayBtn, "▶  Play", hoveredMenuButton == 0);
            DrawButton(g, menuFullscreenBtn, "⛶  Expand ", hoveredMenuButton == 1);
        }

        //  GAME OVER SCREEN

        private void DrawGameOver(Graphics g)
        {
            // Draw last game frame dimmed as background
            g.DrawImage(bgrounds[bgIndex], 0, 0, BASE_W, BASE_H);
            using (SolidBrush overlay = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                g.FillRectangle(overlay, 0, 0, BASE_W, BASE_H);

            // "GAME OVER"
            DrawCentered(g, "GAME OVER", bigFont, Brushes.Red, 120);

            // Score this round + high score
            int hi = LoadHighScore();
            DrawCentered(g, $"Your score: {plane.score}", subFont, Brushes.White, 190);
            DrawCentered(g, $"Best score: {hi}", subFont, Brushes.Gold, 218);

            // New high score badge
            if (plane.score >= hi && plane.score > 0)
                DrawCentered(g, "★ New High Score! ★", hintFont, Brushes.Yellow, 248);

            // Buttons
            DrawButton(g, gameOverBtns[0], gameOverLabels[0], hoveredButton == 0);
            DrawButton(g, gameOverBtns[1], gameOverLabels[1], hoveredButton == 1);
            DrawButton(g, gameOverBtns[2], gameOverLabels[2], hoveredButton == 2);
        }

        // ══════════════════════════════════════════════════════════
        //  SCORE BOARD SCREEN
        // ══════════════════════════════════════════════════════════

        private void DrawScoreBoard(Graphics g)
        {
            g.DrawImage(bgrounds[bgIndex], 0, 0, BASE_W, BASE_H);
            using (SolidBrush overlay = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                g.FillRectangle(overlay, 0, 0, BASE_W, BASE_H);

            DrawCentered(g, "HIGH SCORES", bigFont, Brushes.Gold, 80);
            g.DrawLine(new Pen(Color.Gold, 1), 80, 140, 340, 140);

            int[] scores = LoadScores();

            if (scores.Length == 0)
            {
                DrawCentered(g, "No scores yet. Play to record!", scoreFont, Brushes.LightGray, 200);
            }
            else
            {
                string[] medals = { "🥇", "🥈", "🥉", "4.", "5." };
                float sy = 160;
                for (int i = 0; i < scores.Length; i++)
                {
                    bool isTop = (i == 0);
                    Brush color = isTop ? Brushes.Gold : Brushes.White;
                    string line = $"  {medals[i]}  {scores[i]} pts";
                    DrawCentered(g, line, isTop ? btnFont : scoreFont, color, sy);
                    sy += 38;
                }
            }

            // Back button
            DrawButton(g, backBtn, "← Back", hoveredButton == 0);
        }

        // ══════════════════════════════════════════════════════════
        //  GAME SCREEN
        // ══════════════════════════════════════════════════════════

        private void DrawGame(Graphics g)
        {
            // Check if game just ended this frame — save score immediately
            if (MyPlane.isGameOver)
            {
                SaveScore(plane.score);
                state = GameState.GameOver;
                return;
            }

            pix_y += BG_SPEED;
            if (pix_y > BASE_H) pix_y = 0;
            g.DrawImage(bgrounds[bgIndex], 0, pix_y, BASE_W, BASE_H);
            g.DrawImage(bgrounds[bgIndex], 0, pix_y - BASE_H, BASE_W, BASE_H);

            g.DrawImage(avatarImg, 10, 10);
            g.DrawRectangle(new Pen(Color.Black), new Rectangle(10, 100, 100, 10));
            g.FillRectangle(Brushes.Red, 10, 101, plane.health, 9);
            g.DrawRectangle(new Pen(Color.Blue), new Rectangle(10, 120, 100, 10));
            g.FillRectangle(Brushes.Green, 11, 121, plane.score, 9);
            g.DrawString("Player: Bi", new Font("Arial", 9, FontStyle.Bold), Brushes.Yellow, new System.Drawing.Point(10, 140));
            g.DrawString("Score: " + plane.score, new Font("Arial", 9, FontStyle.Bold), Brushes.Yellow, new System.Drawing.Point(10, 160));

            plane.Move();
            plane.Draw(g);

            MyBullet.ProduceMybul(plane);
            MyBullet.MoveMybul(g);
            MyBullet.IsHitEnemy(plane);

            EnemyPlane.ProduceFighter();
            EnemyPlane.FighterMove(g);

            EnemyBullet.ProduceEnbul(plane);
            EnemyBullet.MoveEnbul(g);
            EnemyBullet.HitPlane(plane);

            ProduceShotGun();
            ProduceBlood();

            if (isDropGun && !plane.isGetGun) g.DrawImage(shotImg, 200, shot_y);
            if (isDropBox && !plane.isGetBlood) g.DrawImage(bloodImg, 350, blood_y);
        }

        private void ProduceShotGun()
        {
            if (new Random().Next(0, 100) == 2) isDropGun = true;
            if (isDropGun && !plane.isGetGun)
            {
                var r = new Rectangle(200, shot_y, shotImg.Width, shotImg.Height);
                if (r.IntersectsWith(plane.GetBounds())) { plane.isGetGun = true; shot_y = -100; }
                shot_y += 5;
                if (shot_y > 950) shot_y = -100;
            }
        }

        private void ProduceBlood()
        {
            if (new Random().Next(0, 100) == 0) isDropBox = true;
            if (isDropBox && !plane.isGetBlood)
            {
                var r = new Rectangle(350, blood_y, bloodImg.Width, bloodImg.Height);
                if (r.IntersectsWith(plane.GetBounds()))
                {
                    plane.isGetBlood = true;
                    if (plane.health <= 90) plane.health += 10;
                    blood_y = -100;
                }
                blood_y += 5;
                if (blood_y > 950) blood_y = -100;
            }
        }

        // ══════════════════════════════════════════════════════════
        //  DRAW HELPERS
        // ══════════════════════════════════════════════════════════

        private void DrawCentered(Graphics g, string text, Font font, Brush brush, float y)
        {
            SizeF sz = g.MeasureString(text, font);
            g.DrawString(text, font, brush, (BASE_W - sz.Width) / 2f, y);
        }

        private void DrawButton(Graphics g, Rectangle rect, string label, bool hovered)
        {
            // Background
            Color bg = hovered ? Color.FromArgb(220, 70, 130, 200) : Color.FromArgb(180, 30, 30, 60);
            using (SolidBrush br = new SolidBrush(bg))
                g.FillRectangle(br, rect);

            // Border
            Pen border = hovered ? new Pen(Color.Cyan, 2) : new Pen(Color.Gray, 1);
            g.DrawRectangle(border, rect);

            // Label
            SizeF sz = g.MeasureString(label, btnFont);
            float tx = rect.X + (rect.Width - sz.Width) / 2f;
            float ty = rect.Y + (rect.Height - sz.Height) / 2f;
            g.DrawString(label, btnFont, hovered ? Brushes.Cyan : Brushes.White, tx, ty);
        }

        // ══════════════════════════════════════════════════════════
        //  TIMERS
        // ══════════════════════════════════════════════════════════

        private void timer1_Tick(object sender, EventArgs e) => this.Invalidate();

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (state != GameState.Playing) return;
            float scaleX = (float)this.ClientSize.Width / BASE_W;
            float scaleY = (float)this.ClientSize.Height / BASE_H;
            using (Graphics g = this.CreateGraphics())
            {
                for (int j = EnemyPlane.fighters.Count - 1; j >= 0; j--)
                {
                    if (EnemyPlane.fighters[j].flag)
                    {
                        int ex = (int)(EnemyPlane.fighters[j].GetLoc().X * scaleX);
                        int ey = (int)(EnemyPlane.fighters[j].GetLoc().Y * scaleY);
                        g.DrawImage(boomImg, ex, ey);
                        new SoundPlayer(Resource.BOMB21).Play();
                        EnemyPlane.fighters.RemoveAt(j);
                    }
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e) => MyPlane.Keydown(e.KeyCode);

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            MyPlane.Keyup(e.KeyCode);
            plane.ResetImage();
        }
    }
}

#pragma warning restore IDE1006