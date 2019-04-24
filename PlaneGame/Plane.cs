using System.Drawing;

namespace Myplanegame
{
   
    public abstract class Plane : GameObject
    {
        public int health;

        public int X => x;
        public int Y => y;

        protected Plane(int startX, int startY, int startHealth)
            : base(startX, startY)
        {
            health = startHealth;
        }

        // Default hitbox = 80x100 (player size).
        // EnemyPlane overrides this to its own sprite size.
        public override Rectangle GetBounds()
        {
            return new Rectangle(x, y, 80, 100);
        }

        // VIRTUAL — both children override but call base first
        public virtual void TakeDamage(int amount)
        {
            health -= amount;
        }

        public abstract override void Draw(Graphics g);
        public abstract override void Move();
    }
}