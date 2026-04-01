using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Myplanegame
{
 
    public class MyBullet : Bullet
    {
        private const int BULLET_SPEED = 3;
        private static int fireCounter = 0;
        private const double PI = Math.PI;

        public int Angle { get; private set; }

        // readonly: the list object never changes (items inside can change)
        public static readonly List<MyBullet> mybulList = new List<MyBullet>();

        // Constructor: base(bx, by, 18) → Bullet(bx,by,18) → GameObject(bx,by)
        public MyBullet(int bx, int by, int angle)
            : base(bx, by, BULLET_SPEED)
        {
            Angle = angle;
            SetImageAndOffset(angle);
        }

        private void SetImageAndOffset(int angle)
        {
            switch (angle)
            {
                case 0: image = Resource.bul02; y -= 17; break;
                case 30: image = Resource.bul02_30; x += 12; y -= 12; break;
                case 60: image = Resource.bul02_60; x += 2; y -= 17; break;
                case 120: image = Resource.bul02_120; x -= 35; y -= 12; break;
                case 150: image = Resource.bul02_150; x -= 20; y -= 12; break;
            }
        }

        // OVERRIDE Move() — player bullets go UPWARD (y decreases)
        // EnemyBullet.Move() goes DOWNWARD (y increases) — polymorphism
        public override void Move()
        {
            switch (Angle)
            {
                case 0: y -= speed; break;
                case 60: x += (int)(speed / 2.0); y -= (int)(speed * Math.Cos(PI / 6)); break;
                case 30: x += (int)(speed * Math.Cos(PI / 6)); y -= (int)(speed / 2.0); break;
                case 120: x -= (int)(speed / 2.0); y -= (int)(speed * Math.Cos(PI / 6)); break;
                case 150: x -= (int)(speed * Math.Cos(PI / 6)); y -= (int)(speed / 2.0); break;
            }
            if (IsOutOfBounds()) isActive = false;
        }

        // --- Static helpers ---

        public static void ProduceMybul(MyPlane plane)
        {
            if (MyPlane.isGameOver) return;

            fireCounter++;
            if (fireCounter < 20) return;   // fire once every 10 ticks
            fireCounter = 0;

            mybulList.Add(new MyBullet(plane.X + 13, plane.Y - 10, 0));

            if (plane.isGetGun)
            {
                mybulList.Add(new MyBullet(plane.X + 7, plane.Y - 8, 30));
                mybulList.Add(new MyBullet(plane.X + 30, plane.Y - 12, 120));
            }
        }

        public static void MoveMybul(Graphics g)
        {
            for (int i = mybulList.Count - 1; i >= 0; i--)
            {
                mybulList[i].Move();
                if (!mybulList[i].isActive)
                    mybulList.RemoveAt(i);
                else
                    mybulList[i].Draw(g);
            }
        }

        public static void IsHitEnemy(MyPlane plane)
        {
            for (int i = mybulList.Count - 1; i >= 0; i--)
            {
                for (int j = EnemyPlane.fighters.Count - 1; j >= 0; j--)
                {
                    if (mybulList[i].CollidesWith(EnemyPlane.fighters[j]))
                    {
                        mybulList.RemoveAt(i);
                        EnemyPlane.fighters[j].TakeDamage(1);
                        if (plane.score < 100) plane.score += 1;
                        break;
                    }
                    else if (plane.CollidesWith(EnemyPlane.fighters[j]))
                    {
                        EnemyPlane.fighters[j].TakeDamage(1);
                        if (plane.score < 100) plane.score += 1;
                    }
                }
            }
        }
    }
}