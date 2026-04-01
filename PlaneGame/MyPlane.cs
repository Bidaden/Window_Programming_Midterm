using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Myplanegame
{
  
    public class MyPlane : Plane
    {
        public int score = 0;
        public bool isGetGun = false;
        public bool isGetBlood = false;
        public static bool isGameOver = false;

        private const int SPEED = 4;

        // readonly: assigned once in constructor, never reassigned
        private readonly Image imgNormal;
        private readonly Image imgLeft;
        private readonly Image imgRight;
        private readonly Image imgGameOver;

        // readonly: the list object never changes (items inside can change)
        private static readonly List<Keys> heldKeys = new List<Keys>();

        // Constructor: base(180, 530, 100) → Plane(180,530,100) → GameObject(180,530)
        public MyPlane() : base(180, 530, 100)
        {
            imgNormal = Resource.plane;
            imgLeft = Resource.plane;
            imgRight = Resource.plane;
            imgGameOver = Resource.gameover;
            image = imgNormal;    // 'image' inherited from GameObject
        }

        // Public method so GameForm can reset the tilt image
        // without touching the protected 'image' field directly
        public void ResetImage()
        {
            image = imgNormal;
        }

        // --- Keyboard input ---
        public static void Keydown(Keys key)
        {
            if (!heldKeys.Contains(key)) heldKeys.Add(key);
        }

        public static void Keyup(Keys key)
        {
            heldKeys.Remove(key);
        }

        public static bool IsKeyDown(Keys key)
        {
            return heldKeys.Contains(key);
        }

        // OVERRIDE TakeDamage() from Plane
        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);        // Plane: health -= amount
            if (score > 0) score -= amount;
            if (health <= 0) isGameOver = true;
        }

        // OVERRIDE Draw() from Plane / GameObject
        public override void Draw(Graphics g)
        {
            if (health > 0)
            {
                g.DrawImage(image, x, y, 80, 100);
            }
            else
            {
                isGameOver = true;
                g.DrawImage(imgNormal, 0, -300);
                g.DrawImage(imgGameOver, 10, 260);
            }
        }

        // OVERRIDE Move() from Plane / GameObject
        public override void Move()
        {
            if (isGameOver) return;

            if (IsKeyDown(Keys.A)) { image = imgLeft; x = Math.Max(5, x - SPEED); }
            if (IsKeyDown(Keys.D)) { image = imgRight; x = Math.Min(370, x + SPEED); }
            if (IsKeyDown(Keys.W)) { y = Math.Max(5, y - SPEED); }
            if (IsKeyDown(Keys.S)) { y = Math.Min(530, y + SPEED); }
        }
    }
}