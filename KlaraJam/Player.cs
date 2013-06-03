using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace KlaraJam
{
    public class Player
    {
        Game1 super;

        public Texture2D spriteSheet;
        public Texture2D texture_forcefield;
        Vector2 size = new Vector2(16,16);
        public Vector2 position;

        public bool shielded;
        public bool dead;

        float speed;
        int bulletsMax;
        int bullets;
        float bulletTimer;
        float bulletTimerFreq;

        bool rolling;
        int rollframe;
        short rolldir;
        int rollTimer;
        int rollTimerFreq;
        int time;

        int graceScore = 50;
        public bool drunken = false;

        public void init() {
            dead = false;
            shielded = true;
            speed = 3f;
            this.position = new Vector2(100,super.roomSize.Y/2);
            bulletsMax = 5;
            bulletTimerFreq = 16;
            rolling = false;
            rollframe = 1;
            rollTimerFreq = 5;
            time = 0;
        }

        public Player(Game1 super) {
            this.super = super;
            init();
        }

        void hit() {
            super.shake = 20;
            if(shielded) {
                shielded = false;
                super.sound_shieldbreak.Play(super.EffectVolume,0f,0f);
                super.killsSinceLeak = 0;
            } else {
                dead = true;
                super.lifetimes[super.Difficulty].Add(time);
                super.sound_explosion.Play(0.5f*super.EffectVolume, 0f, 0f);
                super.sound_death.Play(super.EffectVolume,0f,0f);
                Particle.Explosion(super, 300, position);
            }
        }

        public void Update() {
            time++;
            if(super.gps.IsConnected || (super.kbs.IsKeyDown(Keys.Enter) && !super.okbs.IsKeyDown(Keys.Enter))) {
                if(super.gps.Buttons.Start == ButtonState.Pressed || super.kbs.IsKeyDown(Keys.Enter)) {
                    super.restart();
                }
            }
            if(dead) return;
            //logic
            bulletTimer-=1;
            if(bulletTimer <= 0) {
                bullets = bulletsMax;
                bulletTimer = bulletTimerFreq;
            }

            if(rollTimer > 0) rollTimer-=1;
            if(rollTimer <= 0 && rolling) {
                rollframe += rolldir;
                if(rolldir == -1 && rollframe == 1) {
                    rolling = false;
                    rollframe = 1;
                }
                if(rolldir == 1 && rollframe == 5) {
                    rolling = false;
                    rollframe = 1;
                }
                rollTimer = rollTimerFreq;
            }

            for(int i = 0;i<super.bullets.Count;i++) {
                Bullet b = super.bullets[i];
                if(b.playerBullet) continue;
                if(!(
                    position.X-size.X/2 > b.position.X+b.size.X/2 ||
                    position.X+size.X/2 < b.position.X-b.size.X/2 ||
                    position.Y-size.Y/2 > b.position.Y+b.size.Y/2 ||
                    position.Y+size.Y/2 < b.position.Y-b.size.Y/2)) {
                    if(!rolling) {
                        hit();
                        super.bullets.Remove(b);
                    } else {
                        super.score += (int)(graceScore*super.multiplier);
                    }
                } } 

            for(int i = 0;i<super.enemies.Count;i++) {
                Enemy e = super.enemies[i];
                if(!(
                    position.X-size.X/2 > e.position.X+e.size.X/2 ||
                    position.X+size.X/2 < e.position.X-e.size.X/2 ||
                    position.Y-size.Y/2 > e.position.Y+e.size.Y/2 ||
                    position.Y+size.Y/2 < e.position.Y-e.size.Y/2)) {
                    if(!rolling) {
                        hit();
                        e.die();
                    } else {
                        super.score += (int)(graceScore*super.multiplier);
                    }
                } }

            for(int i = 0;i<super.asteroids.Count;i++) {
                Asteroid a = super.asteroids[i];
                if(!(
                    position.X-size.X/2 > a.position.X+a.size.X/2 ||
                    position.X+size.X/2 < a.position.X-a.size.X/2 ||
                    position.Y-size.Y/2 > a.position.Y+a.size.Y/2 ||
                    position.Y+size.Y/2 < a.position.Y-a.size.Y/2)) {
                    if(!rolling) {
                        hit();
                        super.asteroids.Remove(a);
                    } else {
                        super.score += (int)(graceScore*super.multiplier);
                    }
                } }

            
            //input
            Vector2 velocity = Vector2.Zero;
            if(super.gps.IsConnected) {
                velocity = new Vector2(
                    super.gps.ThumbSticks.Left.X*speed*(rolling?1.5f:1), 
                    -super.gps.ThumbSticks.Left.Y*speed*(rolling?1.5f:1));
            }

            if(super.kbs.IsKeyDown(Keys.Down)) { velocity = new Vector2(0, speed*(rolling?1.5f:1)); }
            if(super.kbs.IsKeyDown(Keys.Up)) { velocity = new Vector2(0, -speed*(rolling?1.5f:1)); }
            if(super.kbs.IsKeyDown(Keys.Left)) { velocity = new Vector2(-speed*(rolling?1.5f:1),0); }
            if(super.kbs.IsKeyDown(Keys.Right)) { velocity = new Vector2(speed*(rolling?1.5f:1),0); }

            position+=(drunken?-1:1)*velocity;

            if(position.X > super.roomSize.X || position.X < 0) {
                position.X-=(drunken?-1:1)*velocity.X;
            }
            if(position.Y > super.roomSize.Y || position.Y < 0) {
                position.Y-=(drunken?-1:1)*velocity.Y;
            }

            for(int i = 0;i<8;i++) {
                Particle p = new Particle(
                    super, 4, position-new Vector2(4,0), -velocity-new Vector2(1,1)+new Vector2((float)super.random.NextDouble(),(float)super.random.NextDouble()), new Vector2(2,2));
                p.color = i>4?new Color(1f,0f,0f):new Color(1f,1f,0f);
                super.particles.Add(p);
            }

            if(super.gps.IsConnected || super.kbs.IsKeyDown(Keys.Z)) {
                if(super.gps.Buttons.A == ButtonState.Pressed || super.kbs.IsKeyDown(Keys.Z)) {
                    if(bullets > 0 && !rolling) {
                        Bullet b = new Bullet(super, position, new Vector2(8,0), new Vector2(8,8));
                        b.texture = super.texture_bullet1;
                        b.playerBullet = true;
                        super.bullets.Add(b);
                        bullets-=1;
                        float pitch = (float)Math.Sin(MathHelper.ToRadians(5f*60*(time%60)));
                        super.sound_shot.Play(1.0f, pitch, 0f);
                    }
                }
            }

            if(super.gps.IsConnected || super.kbs.IsKeyDown(Keys.X)) {
                if((super.gps.Buttons.B == ButtonState.Pressed || super.kbs.IsKeyDown(Keys.X)) && !rolling) {
                    if(Math.Abs(super.gps.ThumbSticks.Left.Y) > 0.4 || super.kbs.IsKeyDown(Keys.X)) {
                        rolldir = Math.Sign(super.gps.ThumbSticks.Left.Y)==-1?(short)1:(short)-1;
                        rollframe = rolldir==-1?4:2;
                        rollTimer = rollTimerFreq;
                        rolling = true;
                        super.sound_roll.Play(super.EffectVolume,0f,0f);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            if(dead) return;
            if(shielded) {
                spriteBatch.Draw(
                    texture_forcefield,
                    Helpers.Scale(new Rectangle(
                        (int)(position.X-this.size.X/2),
                        (int)(position.Y-this.size.Y/2),
                        (int)size.X, (int)size.Y),
                        super.scale),
                    Color.White);
            }
            spriteBatch.Draw(
                spriteSheet,
                Helpers.Scale(new Rectangle(
                    (int)(position.X-this.size.X/2),
                    (int)(position.Y-this.size.Y/2),
                    (int)size.X, (int)size.Y),
                    super.scale),
                new Rectangle((int)size.X*(rollframe-1),0,16,16), Color.White);
        }
    }
}
