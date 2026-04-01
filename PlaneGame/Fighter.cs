using System;
using System.Collections.Generic;
using System.Drawing;

namespace Myplanegame
{
    
    public class EnemyPlane : Plane
    {
        public bool flag = false;

        private const int ENEMY_SPEED = 2;

        // readonly: the list object never changes (items inside can change)
        public static readonly List<EnemyPlane> fighters = new List<EnemyPlane>();

        // Constructor: base(startX, 0, 1) → Plane → GameObject
        public EnemyPlane(int startX, int colorIndex) : base(startX, 0, 1)
        {
            switch (colorIndex)
            {
                case 0: image = Resource.fighterRed; break;
                case 1: image = Resource.fighterGreen; break;
                case 2: image = Resource.fighterYellow; break;
                default: image = Resource.fighterRed; break;
            }
        }

        public Point GetLoc() => new Point(x, y);

        // OVERRIDE GetBounds() — enemy sprites are 65x45, not 80x100
        public override Rectangle GetBounds()
        {
            return new Rectangle(x, y, 65, 45);
        }

        // OVERRIDE TakeDamage() from Plane
        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);    // Plane: health -= amount
            if (health <= 0) flag = true;
        }

        // OVERRIDE Draw() from Plane / GameObject
        public override void Draw(Graphics g)
        {
            g.DrawImage(image, x, y);
        }

        // OVERRIDE Move() from Plane / GameObject
        // Enemy planes fall straight down — no keyboard
        public override void Move()
        {
            y += ENEMY_SPEED;
            if (y > 650) isActive = false;
        }

        // --- Static helpers ---

        public static void ProduceFighter()
        {
            Random rng = new Random();
            if (rng.Next(18) == 0)
                fighters.Add(new EnemyPlane(rng.Next(0, 350), rng.Next(0, 3)));
        }

        public static void FighterMove(Graphics g)
        {
            for (int i = fighters.Count - 1; i >= 0; i--)
            {
                fighters[i].Move();
                if (!fighters[i].isActive)
                    fighters.RemoveAt(i);
                else
                    fighters[i].Draw(g);
            }
        }
    }
}