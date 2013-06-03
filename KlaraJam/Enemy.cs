using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KlaraJam
{
    public class Enemy
    {
        Game1 super;

        public Texture2D spriteSheet;
        public Vector2 size = new Vector2(16,16);
        public Vector2 position;
        public Vector2 velocity;
        public int health;

        float bulletTimer;
        float bulletTimerFreq;
        int bullets;
        int bulletsMax;

        public Enemy(Game1 super, Vector2 position, Vector2 velocity) {
            this.super = super;
            this.position = position;
            this.velocity = velocity;
            health = 2;
            bulletTimerFreq = 45;
            bulletTimer = bulletTimerFreq;
            bullets = 0;
            bulletsMax = 1;
        }
        
        public void die() {
            super.shake = 5;
            super.sound_explosion.Play(super.EffectVolume*0.7f, 0f, 0f);

            Particle.Explosion(super, 100, position);
            super.killsSinceLeak++;

            if(super.killsSinceLeak >= super.killsToReg) {
                if(!super.player.shielded) {
                    super.player.shielded = true;
                }
            }

            super.score += (int)(50*super.multiplier);
            super.enemies.Remove(this);
        }

        public void Update() {
            for(int i = 0;i<8;i++) {
                Particle p = new Particle(
                    super, 4, position+new Vector2(4,0), -velocity-new Vector2(1,1)+new Vector2((float)super.random.NextDouble()*3,(float)super.random.NextDouble()), new Vector2(2,2));
                p.color = i>4?new Color(1f,0f,0f):new Color(1f,1f,0f);
                super.particles.Add(p);
            }

            for(int i = 0;i<super.bullets.Count;i++) {
                Bullet b = super.bullets[i];
                if(!b.playerBullet) continue;
                if(!(
                    position.X-size.X/2 > b.position.X+b.size.X/2 ||
                    position.X+size.X/2 < b.position.X-b.size.X/2 ||
                    position.Y-size.Y/2 > b.position.Y+b.size.Y/2 ||
                    position.Y+size.Y/2 < b.position.Y-b.size.Y/2)) {
                    super.bullets.RemoveAt(i);
                    health-=1;
                    if(health <= 0) {
                        die();
                    } else {
                        super.sound_hit.Play(super.EffectVolume*0.4f, 0f, 0f);
                        float angle = (float)super.random.NextDouble()*360f;
                        Particle p = 
                            new Particle(
                                super, 10, position,
                                new Vector2(
                                    (float)Math.Cos(angle)*5,
                                    (float)Math.Sin(angle*5)),
                                new Vector2(2,2));
                        p.color = new Color(255, super.random.NextDouble()>0.5?255:0, 0);
                        super.particles.Add(p);
                    }
                }
            }
            if((position+velocity).Y < 0 || (position+velocity).Y > super.roomSize.Y) {
                velocity.Y*=-1;
            }
            this.position += this.velocity;

            if(position.X < -100) {
                super.killsSinceLeak = 0;
                super.enemies.Remove(this);
            }

            if(super.Difficulty == Game1.Difficulties.SpaceAce) {
                bulletTimer--;
                if(bulletTimer <= 0) {
                    Bullet b = new Bullet(
                        super, position, new Vector2(-8, 0), new Vector2(8,8));
                    b.texture = super.texture_bullet1;
                    super.bullets.Add(b);
                    bullets++;
                    super.sound_shot.Play(super.EffectVolume,0f,0f);
                    if(bullets >= bulletsMax) {
                        bulletTimer = bulletTimerFreq;
                        bullets = 0;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(
                spriteSheet,
                Helpers.Scale(new Rectangle(
                    (int)(position.X-this.size.X/2),
                    (int)(position.Y-this.size.Y/2),
                    (int)size.X, (int)size.Y),
                    super.scale),
                new Rectangle(0,0,16,16), Color.White);
        }
    }
}
