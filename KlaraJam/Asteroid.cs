using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KlaraJam
{
    public class Asteroid
    {
        Game1 super;

        public Texture2D texture;
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 size;
        float rotation;

        public Asteroid(Game1 super, Vector2 position, Vector2 velocity, Vector2 size) {
            this.super = super;
            this.position = position;
            this.velocity = velocity;
            this.size = size;
        }

        public void Update() {
            rotation += MathHelper.ToRadians(6);
            position+=velocity;
            if(position.X < -100) {
                super.asteroids.Remove(this);
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(
                texture,
                Helpers.Scale(new Rectangle(
                    (int)(position.X-size.X/2),
                    (int)(position.Y-size.Y/2),
                    (int)size.X, (int)size.Y), super.scale), null, Color.White, rotation, size*0.5f, SpriteEffects.None, 0);
        }
    }
}
