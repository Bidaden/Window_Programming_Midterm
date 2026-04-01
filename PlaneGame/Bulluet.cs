using System.Drawing;

namespace Myplanegame
{
   
    public abstract class Bullet : GameObject
    {
        protected readonly int speed;

        protected Bullet(int startX, int startY, int bulletSpeed)
            : base(startX, startY)
        {
            speed = bulletSpeed;
        }

        // SHARED — both bullet types draw the same way
        public override void Draw(Graphics g)
        {
            if (image != null)
                g.DrawImage(image, x, y);
        }

        // SHARED — same boundary check for both bullet types
        protected bool IsOutOfBounds()
        {
            return y < 0 || y > 700 || x < 0 || x > 420;
        }

        // ABSTRACT — MyBullet goes UP, EnemyBullet goes DOWN
        public abstract override void Move();

        public override Rectangle GetBounds()
        {
            return new Rectangle(x, y, 8, 10);
        }
    }
}