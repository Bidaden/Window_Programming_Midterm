using System.Drawing;

namespace Myplanegame
{
  
    public abstract class GameObject
    {
        protected int x;
        protected int y;
        protected Image image;      // not readonly — subclasses swap images at runtime
        public bool isActive = true;

        protected GameObject(int startX, int startY)
        {
            x = startX;
            y = startY;
        }

        public abstract void Draw(Graphics g);
        public abstract void Move();

        public virtual Rectangle GetBounds()
        {
            if (image != null)
                return new Rectangle(x, y, image.Width, image.Height);
            return new Rectangle(x, y, 0, 0);
        }

        public bool CollidesWith(GameObject other)
        {
            return this.GetBounds().IntersectsWith(other.GetBounds());
        }
    }
}