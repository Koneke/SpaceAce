using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KlaraJam
{
    public class Bullet
    {
        Game1 super;

        public Texture2D texture;
        public Boolean playerBullet;
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 size;

        public Bullet(Game1 super, Vector2 position, Vector2 velocity, Vector2 size) {
            this.super = super;
            this.position = position;
            this.velocity = velocity;
            this.size = size;
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(
                texture,
                Helpers.Scale(new Rectangle(
                    (int)(position.X-size.X/2),
                    (int)(position.Y-size.Y/2),
                    (int)size.X, (int)size.Y), super.scale),
                Color.White);
        }
    }
}
