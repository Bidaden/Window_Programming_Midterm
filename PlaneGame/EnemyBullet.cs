using System;
using System.Collections.Generic;
using System.Drawing;

namespace Myplanegame
{
    public class EnemyBullet : Bullet
    {
        private readonly double slope;

        // readonly: the list object never changes (items inside can change)
        public static readonly List<EnemyBullet> enbullist = new List<EnemyBullet>();

        // Constructor: base(ex, ey, speed) → Bullet → GameObject
        public EnemyBullet(int ex, int ey, int bulletSpeed, int playerX, int playerY)
            : base(ex, ey, bulletSpeed)
        {
            image = Resource.en_bul01;
            double dy = playerY - ey;
            slope = (dy != 0) ? (1.0 * (playerX - ex) / dy) : 0;
        }

        // OVERRIDE Move() — enemy bullets go DOWNWARD (y increases)
        public override void Move()
        {
            y += speed;
            x += (int)(speed * slope);
            if (IsOutOfBounds()) isActive = false;
        }

        public override Rectangle GetBounds()
        {
            return new Rectangle(x, y, 6, 6);
        }

        // --- Static helpers ---

        public static void ProduceEnbul(MyPlane plane)
        {
            Random rng = new Random();
            for (int i = 0; i < EnemyPlane.fighters.Count; i++)
            {
                if (rng.Next(0, 80) == 0)
                {
                    enbullist.Add(new EnemyBullet(
                        EnemyPlane.fighters[i].X + 15,
                        EnemyPlane.fighters[i].Y + 30,
                        rng.Next(3, 7),
                        plane.X, plane.Y));
                }
            }
        }

        public static void MoveEnbul(Graphics g)
        {
            for (int i = enbullist.Count - 1; i >= 0; i--)
            {
                enbullist[i].Move();
                if (!enbullist[i].isActive)
                    enbullist.RemoveAt(i);
                else
                    enbullist[i].Draw(g);
            }
        }

        public static void HitPlane(MyPlane plane)
        {
            for (int i = enbullist.Count - 1; i >= 0; i--)
            {
                if (enbullist[i].CollidesWith(plane))
                {
                    enbullist.RemoveAt(i);
                    plane.TakeDamage(1);
                }
            }
        }
    }
}