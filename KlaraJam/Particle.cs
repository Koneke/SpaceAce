using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KlaraJam
{
    public class Particle
    {
        Game1 super;

        public Texture2D texture;
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 size;

        public Color color;
        Color color1;
        Color color2;
        int fadetimer;
        int fadetime;

        int life;

        public Particle(Game1 super, int life, Vector2 position, Vector2 velocity, Vector2 size) {
            this.super = super;
            this.texture = super.particle;
            this.color = new Color(1f,1f,1f,1f);
            this.life = life;
            this.position = position;
            this.velocity = velocity;
            this.size = size;
            fadetime = -1;
        }

        public void setFade(Color color1, Color color2, int time) {
            this.color1 = color1;
            this.color2 = color2;
            this.fadetime = time;
            this.fadetimer = fadetime;
        }

        public void Update() {
            life--;
            if(life <= 0) {
                super.particles.Remove(this);
                return;
            }
            position+=velocity;
            if(fadetime != -1) {
                fadetimer--;
                color =
                    Helpers.Add(
                        Helpers.Scale(
                            color1, 1f-((float)fadetimer/fadetime)),
                        Helpers.Scale(
                            color2, 1f-((float)fadetimer/fadetime)));
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(
                texture,
                Helpers.Scale(new Rectangle(
                    (int)(position.X-size.X/2),
                    (int)(position.Y-size.Y/2),
                    (int)size.X, (int)size.Y), super.scale),
                color);
        }

        public static void Explosion(Game1 super, int amount, Vector2 position) {
            for(int r = 0;r<amount;r++) {
                float angle = (float)super.random.NextDouble()*360f;
                float speed = 4+(float)super.random.NextDouble();
                Particle p = 
                    new Particle(
                        super, 7+(int)(3*super.random.NextDouble()), position,
                        new Vector2(
                            (float)Math.Cos(MathHelper.ToRadians(angle))*speed,
                            (float)Math.Sin(MathHelper.ToRadians(angle))*speed),
                        new Vector2(2,2));
                p.color = new Color(255, super.random.NextDouble()>0.5?255:0, 0);
                super.particles.Add(p);
            }
        }
    }
}
