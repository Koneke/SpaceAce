using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace KlaraJam
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public enum Difficulties {
            Recruit,
            Veteran,
            General,
            SpaceAce
        }
        public Difficulties Difficulty;

        public enum GameStates {
            MainMenu,
            HelpMenu,
            OptionsMenu,
            Game
        }
        public GameStates GameState;

        int menu_choice = 0;
        string[] menu_choices = new string[]{"Start Game", "Help", "Options", "Quit"};
        string[] menu_help = new string[]{"Left stick to move", "A to shoot", "B to roll", "Start to restart", "Back to go back"};
        string[] menu_difficulties = new string[]{"Recruit", "Veteran", "General", "Space Ace"};
        int menu_switchTimer;
        int menu_switchTimerFreq = 0;

        int blackFadeTimer;
        int blackFadeTimerFreq = 30;
        bool fade;
        bool fadein;
        bool quitting = false;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Texture2D texture_background;
        public Texture2D texture_bullet1;
        public Texture2D texture_asteroid;
        public Texture2D texture_enemy;
        public Texture2D particle;
        public Texture2D menutex_help;
        public Texture2D menu_options;
        public Texture2D menu;
        public Texture2D dpad;
        int backgroundScroll;

        public SoundEffect sound_shot;
        public SoundEffect sound_explosion;
        public SoundEffect sound_hit;
        public SoundEffect sound_roll;
        public SoundEffect sound_shieldbreak;
        public SoundEffect sound_death;
        public SoundEffect sound_konami;
        public SoundEffect sound_boop;

        public SoundEffectInstance music;

        public float MusicVolume = 1.0f;
        public float EffectVolume = 0.6f;

        public float multiplier;
        public int score;
        public int highscore;
        public Dictionary<Difficulties, int> highscores;
        public Dictionary<Difficulties, List<int>> lifetimes;

        SpriteFont font;
        SpriteFont ingame;
        
        public Vector2 roomSize;
        public float scale = 6;

        public Player player;
        public List<Bullet> bullets;
        public List<Enemy> enemies;
        public List<Particle> particles;
        public List<Asteroid> asteroids;

        public int killsSinceLeak;
        public int killsToReg;

        int enemyTimer;
        int enemyTimerFreq;
        int asteroidTimer;
        int asteroidTimerFreq;

        public int shake;
        public Random random = new Random();

        public KeyboardState kbs;
        public KeyboardState okbs;
        public GamePadState gps;
        public GamePadState ogps;

        char[] secret = new char[10]{'0','0','0','0','0','0','0','0','0','0'};

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            GameState = GameStates.MainMenu;
        }

        protected override void Initialize()
        {
            roomSize = new Vector2(200,120);

            player = new Player(this);
            bullets = new List<Bullet>();
            enemies = new List<Enemy>();
            particles = new List<Particle>();
            asteroids = new List<Asteroid>();
            lifetimes = new Dictionary<Difficulties, List<int>>();
            lifetimes.Add(Difficulties.Recruit, new List<int>());
            lifetimes.Add(Difficulties.Veteran, new List<int>());
            lifetimes.Add(Difficulties.General, new List<int>());
            lifetimes.Add(Difficulties.SpaceAce, new List<int>());

            menu_choice = 0;
            menu_switchTimer = 0;
            menu_switchTimerFreq = 10;
            applyDifficulty();

            killsSinceLeak = 0;
            killsToReg = 7;
            highscore = 0;
            highscores = new Dictionary<Difficulties, int>();
            highscores.Add(Difficulties.Recruit, 0);
            highscores.Add(Difficulties.Veteran, 0);
            highscores.Add(Difficulties.General, 0);
            highscores.Add(Difficulties.SpaceAce, 0);

            Difficulty = Difficulties.General;

            fade = false;
            blackFadeTimer = 0;

            graphics.PreferredBackBufferWidth = (int)(roomSize.X*scale);
            graphics.PreferredBackBufferHeight = (int)(roomSize.Y*scale);
            graphics.ApplyChanges();

            this.IsMouseVisible = true;

            base.Initialize();
        }

        public void restart() {
            if(score > highscores[Difficulty]) highscores[Difficulty] = score;
            bullets.Clear();
            enemies.Clear();
            particles.Clear();
            asteroids.Clear();
            killsSinceLeak = 0;
            multiplier = 10;
            score = 0;
            player.init();
            applyDifficulty();
        }

        void applyDifficulty() {
            switch(Difficulty) {
                case Difficulties.Recruit:
                    enemyTimerFreq = 50;
                    asteroidTimerFreq = -1;
                    break;
                case Difficulties.Veteran:
                    enemyTimerFreq = 50;
                    asteroidTimerFreq = 90;
                    break;
                case Difficulties.General:
                case Difficulties.SpaceAce:
                    enemyTimerFreq = 30;
                    asteroidTimerFreq = 90;
                    break;
            }
            enemyTimer = enemyTimerFreq;
            asteroidTimer = asteroidTimerFreq;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            player.spriteSheet = Content.Load<Texture2D>("ship");
            player.texture_forcefield = Content.Load<Texture2D>("field");
            texture_bullet1 = Content.Load<Texture2D>("bullet");
            texture_asteroid = Content.Load<Texture2D>("asteroid");
            texture_background = Content.Load<Texture2D>("bg");
            texture_enemy = Content.Load<Texture2D>("enemy");
            particle = Content.Load<Texture2D>("particle");
            menutex_help = Content.Load<Texture2D>("helpmenu");
            menu_options = Content.Load<Texture2D>("options");
            menu = Content.Load<Texture2D>("menu");
            dpad = Content.Load<Texture2D>("dpad");

            sound_shot = Content.Load<SoundEffect>("shoot");
            sound_explosion = Content.Load<SoundEffect>("explosion");
            sound_hit = Content.Load<SoundEffect>("hit");
            sound_roll = Content.Load<SoundEffect>("roll");
            sound_shieldbreak = Content.Load<SoundEffect>("shieldbreak");
            sound_death = Content.Load<SoundEffect>("death");
            sound_konami = Content.Load<SoundEffect>("konamicode");
            sound_boop = Content.Load<SoundEffect>("boop");

            music = (Content.Load<SoundEffect>("KlaraJam")).CreateInstance();
            music.IsLooped = true;

            font = Content.Load<SpriteFont>("font");
            ingame = Content.Load<SpriteFont>("ingame");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            kbs = Keyboard.GetState();
            gps = GamePad.GetState(PlayerIndex.One);

            if(gps.IsConnected) {
                if(gps.DPad.Down == ButtonState.Pressed && !(ogps.DPad.Down == ButtonState.Pressed)) {
                    if(EffectVolume > 0f) { EffectVolume -= 0.1f; if(EffectVolume<0) EffectVolume=0f;} }
                if(gps.DPad.Up == ButtonState.Pressed && !(ogps.DPad.Up == ButtonState.Pressed)) {
                    if(EffectVolume < 1f) { EffectVolume += 0.1f; if(EffectVolume>1) EffectVolume=1f; } }
                if(gps.DPad.Left == ButtonState.Pressed && !(ogps.DPad.Left == ButtonState.Pressed)) {
                    if(MusicVolume > 0f) { MusicVolume -= 0.1f; if(MusicVolume<0) MusicVolume=0f; } }
                if(gps.DPad.Right == ButtonState.Pressed && !(ogps.DPad.Right == ButtonState.Pressed)) {
                    if(MusicVolume < 1f) { MusicVolume += 0.1f; if(MusicVolume>1) MusicVolume=1f; } }
            }

            if(kbs.IsKeyDown(Keys.NumPad2) && !(okbs.IsKeyDown(Keys.NumPad2))){
                if(EffectVolume > 0f) { EffectVolume -= 0.1f; if(EffectVolume<0) EffectVolume=0f;} }
            if(kbs.IsKeyDown(Keys.NumPad8) && !(okbs.IsKeyDown(Keys.NumPad8))){
                if(EffectVolume < 1f) { EffectVolume += 0.1f; if(EffectVolume>1) EffectVolume=1f; } }
            if(kbs.IsKeyDown(Keys.NumPad4) && !(okbs.IsKeyDown(Keys.NumPad4))){
                if(MusicVolume > 0f) { MusicVolume -= 0.1f; if(MusicVolume<0) MusicVolume=0f; } }
            if(kbs.IsKeyDown(Keys.NumPad6) && !(okbs.IsKeyDown(Keys.NumPad6))){
                if(MusicVolume < 1f) { MusicVolume += 0.1f; if(MusicVolume>1) MusicVolume=1f; } }

            if(fadein) {
                blackFadeTimer-=2; if(blackFadeTimer<=0) { blackFadeTimer=0; fadein = false;}}
            if(fade) blackFadeTimer++;
            if(blackFadeTimer<=blackFadeTimerFreq) {
                music.Volume = MusicVolume*((float)blackFadeTimerFreq-blackFadeTimer)/blackFadeTimerFreq;
            }

            switch(GameState) {
                case GameStates.MainMenu:
                    if(!(music.State == SoundState.Playing)) {
                        music.Play();
                    }
                    if(gps.Buttons.Back == ButtonState.Pressed && !(ogps.Buttons.Back == ButtonState.Pressed) || (kbs.IsKeyDown(Keys.Escape) && !okbs.IsKeyDown(Keys.Escape))) {
                        sound_shieldbreak.Play(EffectVolume,0f,0f);
                        fade = true;
                        blackFadeTimer = 0;
                        quitting = true;
                    }

                    if(blackFadeTimer >= blackFadeTimerFreq+(quitting?30:0) && fade) {
                        if(quitting) {
                            this.Exit();
                        } else {
                            GameState = GameStates.Game;
                            fade = false;
                            fadein = true;
                        }
                    }

                    menu_switchTimer--;
                    if(gps.IsConnected) {
                        if(gps.DPad.Up == ButtonState.Pressed && !(ogps.DPad.Up == ButtonState.Pressed)) {
                            for(int i = 1;i<10;i++) { secret[i-1] = secret[i]; }
                            secret[9] = 'u';
                        }
                        if(gps.DPad.Down == ButtonState.Pressed && !(ogps.DPad.Down == ButtonState.Pressed)) {
                            for(int i = 1;i<10;i++) { secret[i-1] = secret[i]; }
                            secret[9] = 'd';
                        }
                        if(gps.DPad.Left == ButtonState.Pressed && !(ogps.DPad.Left == ButtonState.Pressed)) {
                            for(int i = 1;i<10;i++) { secret[i-1] = secret[i]; }
                            secret[9] = 'l';
                        }
                        if(gps.DPad.Right == ButtonState.Pressed && !(ogps.DPad.Right == ButtonState.Pressed)) {
                            for(int i = 1;i<10;i++) { secret[i-1] = secret[i]; }
                            secret[9] = 'r';
                        }
                        if(gps.Buttons.A == ButtonState.Pressed && !(ogps.Buttons.A == ButtonState.Pressed)) {
                            for(int i = 1;i<10;i++) { secret[i-1] = secret[i]; }
                            secret[9] = 'a';
                        }
                        if(gps.Buttons.B == ButtonState.Pressed && !(ogps.Buttons.B == ButtonState.Pressed)) {
                            for(int i = 1;i<10;i++) { secret[i-1] = secret[i]; }
                            secret[9] = 'b';
                        }
                        if(gps.Buttons.Start == ButtonState.Pressed && !(ogps.Buttons.Start == ButtonState.Pressed)) {
                            for(int i = 1;i<10;i++) { secret[i-1] = secret[i]; }
                            secret[9] = 's';
                        }
                    }
                        if(
                            (gps.ThumbSticks.Left.Y > 0.5&&menu_switchTimer <= 0) ||
                            (kbs.IsKeyDown(Keys.Up)&&!(okbs.IsKeyDown(Keys.Up)))) {
                            menu_choice += 3;
                            menu_choice = menu_choice % 4;
                            menu_switchTimer = menu_switchTimerFreq;
                            sound_boop.Play(EffectVolume,0f,0f);
                        }
                        if(
                            (gps.ThumbSticks.Left.Y < -0.5&&menu_switchTimer <= 0) ||
                            (kbs.IsKeyDown(Keys.Down)&&!(okbs.IsKeyDown(Keys.Down)))) {
                            menu_choice += 1;
                            menu_choice = menu_choice % 4;
                            menu_switchTimer = menu_switchTimerFreq;
                            sound_boop.Play(EffectVolume,0f,0f);
                        }

                        if(!(ogps.Buttons.A == ButtonState.Pressed)) {
                            if(gps.Buttons.A == ButtonState.Pressed || (kbs.IsKeyDown(Keys.Z)&&!(okbs.IsKeyDown(Keys.Z)))) {
                                sound_hit.Play(EffectVolume,0f,0f);

                                if(menu_choices[menu_choice] == "Start Game") {
                                    if(new string(secret) == "uuddlrlrba") {
                                        player.drunken = !player.drunken;
                                        sound_konami.Play(EffectVolume,0f,0f);
                                    }
                                    fade = true;
                                    blackFadeTimer = 0;
                                    restart();
                                }
                                else if(menu_choices[menu_choice] == "Help") {
                                    GameState = GameStates.HelpMenu;
                                }
                                else if(menu_choices[menu_choice] == "Options") {
                                    GameState = GameStates.OptionsMenu;
                                    switch(Difficulty) {
                                        case Difficulties.Recruit:
                                            menu_choice = 0; break;
                                        case Difficulties.Veteran:
                                            menu_choice = 1; break;
                                        case Difficulties.General:
                                            menu_choice = 2; break;
                                        case Difficulties.SpaceAce:
                                            menu_choice = 3; break;
                                    }
                                }
                                else if(menu_choices[menu_choice] == "Quit") {
                                    quitting = true;
                                    fade = true;
                                    blackFadeTimer = 0;
                                    sound_shieldbreak.Play(EffectVolume,0f,0f);
                                }
                            }
                        }
                    break;
                case GameStates.HelpMenu:
                    if(gps.Buttons.A == ButtonState.Pressed && !(ogps.Buttons.A == ButtonState.Pressed) ||
                        gps.Buttons.B == ButtonState.Pressed && !(ogps.Buttons.B == ButtonState.Pressed) ||
                        (kbs.IsKeyDown(Keys.X)&&!(okbs.IsKeyDown(Keys.X))) ||
                        (kbs.IsKeyDown(Keys.Z)&&!(okbs.IsKeyDown(Keys.Z))) ||
                        (kbs.IsKeyDown(Keys.Escape)&&!(okbs.IsKeyDown(Keys.Escape)))
                        ) {
                        GameState = GameStates.MainMenu;
                        sound_hit.Play(EffectVolume,0f,0f);
                    }
                    break;
                case GameStates.OptionsMenu:
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                    gps.Buttons.B == ButtonState.Pressed ||
                    (kbs.IsKeyDown(Keys.Escape)&&!(okbs.IsKeyDown(Keys.Escape))) ||
                    (kbs.IsKeyDown(Keys.X)&&!(okbs.IsKeyDown(Keys.X)))
                        ){
                        GameState = GameStates.MainMenu;
                        menu_choice = 2;
                        sound_hit.Play(EffectVolume,0f,0f);
                    }

                    menu_switchTimer--;
                    if(
                        (gps.ThumbSticks.Left.Y > 0.5&&menu_switchTimer <= 0) ||
                        (kbs.IsKeyDown(Keys.Up)&&!(okbs.IsKeyDown(Keys.Up)))) {
                        menu_choice += 3;
                        menu_choice = menu_choice % 4;
                        menu_switchTimer = menu_switchTimerFreq;
                        sound_boop.Play(EffectVolume,0f,0f);
                    }

                    if(
                        (gps.ThumbSticks.Left.Y < -0.5&&menu_switchTimer <= 0) ||
                        (kbs.IsKeyDown(Keys.Down)&&!(okbs.IsKeyDown(Keys.Down)))) {
                        menu_choice += 1;
                        menu_choice = menu_choice % 4;
                        menu_switchTimer = menu_switchTimerFreq;
                        sound_boop.Play(EffectVolume,0f,0f);
                    }

                    if(!(ogps.Buttons.A == ButtonState.Pressed)) {
                        if(gps.Buttons.A == ButtonState.Pressed || (kbs.IsKeyDown(Keys.Z)&&!(okbs.IsKeyDown(Keys.Z)))) {
                            sound_hit.Play(EffectVolume,0f,0f);
                            switch(menu_choice) {
                                case 0:
                                    Difficulty = Difficulties.Recruit;
                                    applyDifficulty();
                                    break;
                                case 1:
                                    Difficulty = Difficulties.Veteran;
                                    applyDifficulty();
                                    break;
                                case 2:
                                    Difficulty = Difficulties.General;
                                    applyDifficulty();
                                    break;
                                case 3:
                                    Difficulty = Difficulties.SpaceAce;
                                    applyDifficulty();
                                    break;
                            }
                            menu_choice = 0;
                            GameState = GameStates.MainMenu;
                        }
                    }
                    break;
                case GameStates.Game:
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || kbs.IsKeyDown(Keys.Escape)) {
                        fade = true;
                        blackFadeTimer = 0;
                    }

                    if(fadein == false) {if(blackFadeTimer >= blackFadeTimerFreq+10) {
                            GameState = GameStates.MainMenu;
                            fade = false;
                            fadein = true;
                        }
                    }

                    if(blackFadeTimer >= blackFadeTimerFreq+30 && fade) {
                        this.Exit();
                    }

                    player.Update();
                    backgroundScroll += 3;
                    if(backgroundScroll >= roomSize.X) {
                        backgroundScroll = 0;
                    }

                    int difficultyMultiplier = 1;
                    switch(Difficulty) {
                        case Difficulties.Recruit: difficultyMultiplier = 1; break;
                        case Difficulties.Veteran: difficultyMultiplier = 2; break;
                        case Difficulties.General: difficultyMultiplier = 3; break;
                        case Difficulties.SpaceAce: difficultyMultiplier = 5; break;
                    }
                    if(!player.dead) {
                        multiplier = (10+killsSinceLeak*2)*difficultyMultiplier;
                    }

                    for(int i = 0;i<bullets.Count;i++) {
                        Bullet b = bullets[i];
                        b.position += b.velocity;
                        if((b.position.X > roomSize.X && b.velocity.X > 0) || (b.position.X < 0 && b.velocity.X < 0)) {
                            bullets.RemoveAt(i);
                        }
                    }

                    if(blackFadeTimer<=0) enemyTimer--;
                    if(enemyTimer <= 0) {
                        enemyTimer = enemyTimerFreq;
                        float angle = 325+(float)random.NextDouble()*135;
                        int r = 45;
                        angle = 180-r+(float)random.NextDouble()*(r*2);
                        Enemy e = new Enemy(this,
                            new Vector2(roomSize.X+60, (roomSize.Y/8)+(float)(random.NextDouble()*roomSize.Y)*((float)6/8)),
                            new Vector2(
                                (float)Math.Cos(MathHelper.ToRadians(angle)),
                                (float)Math.Sin(MathHelper.ToRadians(angle)))*3);
                        e.spriteSheet = texture_enemy;
                        enemies.Add(e);
                    }
                    for(int i = 0;i<enemies.Count;i++) {
                        Enemy e = enemies[i];
                        e.Update();
                    }

                    if(asteroidTimerFreq != -1) {
                        if(blackFadeTimer<=0) asteroidTimer--;
                        if(asteroidTimer <= 0) {
                            asteroidTimer = asteroidTimerFreq;
                            Asteroid a = new Asteroid(this,
                                new Vector2(roomSize.X+60, roomSize.Y/8+(float)(random.NextDouble()*roomSize.Y)*((float)6/8)),
                                new Vector2(-4, 0), new Vector2(16,16));
                            a.texture = texture_asteroid;
                            asteroids.Add(a);
                        }
                        for(int i = 0;i<asteroids.Count;i++) {
                            Asteroid a = asteroids[i];
                            a.Update();
                        }
                    }

                    for(int i = 0;i<particles.Count;i++) {
                        Particle p = particles[i];
                        p.Update();
                    }

                    if(shake > 0) shake-=1;

                    break;
            }

            okbs = kbs;
            ogps = gps;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Vector2 fms;
            switch(GameState) {
                case GameStates.MainMenu:
                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin(
                        SpriteSortMode.Deferred,
                        BlendState.NonPremultiplied,
                        SamplerState.PointClamp,
                        DepthStencilState.Default,
                        RasterizerState.CullNone);

                    spriteBatch.Draw(
                        menu,
                        Helpers.Scale(new Rectangle(0,0,(int)roomSize.X,(int)roomSize.Y), scale), Color.White);

                    for(int i = 0;i<4;i++) {
                        fms = font.MeasureString(menu_choices[i]);
                        fms*=6;
                        spriteBatch.DrawString(
                            font,
                            menu_choices[i],
                            new Vector2((roomSize.X*scale/2), roomSize.Y*scale/2)+new Vector2(-fms.X/2, -fms.Y*2+i*(fms.Y-46)+160)+new Vector2(1,1)*scale,
                            Color.Black,
                            0, Vector2.Zero, scale, SpriteEffects.None, 0);
                        spriteBatch.DrawString(
                            font,
                            menu_choices[i],
                            new Vector2((roomSize.X*scale/2), roomSize.Y*scale/2)+new Vector2(-fms.X/2, -fms.Y*2+i*(fms.Y-46)+160),
                            i==menu_choice?Color.White:Color.Red,
                            0, Vector2.Zero, scale, SpriteEffects.None, 0);
                    }
                    spriteBatch.End();
                    break;
                case GameStates.HelpMenu:
                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin(
                        SpriteSortMode.Deferred,
                        BlendState.NonPremultiplied,
                        SamplerState.PointClamp,
                        DepthStencilState.Default,
                        RasterizerState.CullNone);

                    spriteBatch.Draw(
                        menutex_help,
                        Helpers.Scale(new Rectangle(0,0,(int)roomSize.X,(int)roomSize.Y), scale), Color.White);

                    for(int i = 0;i<5;i++) {
                        fms = font.MeasureString(menu_help[i]);
                        fms*=6;
                        spriteBatch.DrawString(
                            font,
                            menu_help[i],
                            new Vector2((roomSize.X*scale/2), roomSize.Y*scale/2)+new Vector2(-fms.X/2, -fms.Y*2+i*(fms.Y-46)+40)+new Vector2(1,1)*scale,
                            Color.Black,
                            0, Vector2.Zero, scale, SpriteEffects.None, 0);
                        spriteBatch.DrawString(
                            font,
                            menu_help[i],
                            new Vector2((roomSize.X*scale/2), roomSize.Y*scale/2)+new Vector2(-fms.X/2, -fms.Y*2+i*(fms.Y-46)+40),
                            Color.White,
                            0, Vector2.Zero, scale, SpriteEffects.None, 0);
                    }
                    spriteBatch.End();
                    break;
                case GameStates.OptionsMenu:
                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin(
                        SpriteSortMode.Deferred,
                        BlendState.NonPremultiplied,
                        SamplerState.PointClamp,
                        DepthStencilState.Default,
                        RasterizerState.CullNone);

                    spriteBatch.Draw(
                        menu_options,
                        Helpers.Scale(new Rectangle(0,0,(int)roomSize.X,(int)roomSize.Y), scale), Color.White);

                    for(int i = 0;i<4;i++) {
                        fms = font.MeasureString(menu_difficulties[i]);
                        fms*=scale;
                        spriteBatch.DrawString(
                            font,
                            menu_difficulties[i],
                            new Vector2((roomSize.X*scale/2), roomSize.Y*scale/2)+new Vector2(-fms.X/2, -fms.Y*2+i*(fms.Y-46)+160)+new Vector2(1,1)*scale,
                            Color.Black,
                            0, Vector2.Zero, scale, SpriteEffects.None, 0);
                        spriteBatch.DrawString(
                            font,
                            menu_difficulties[i],
                            new Vector2((roomSize.X*scale/2), roomSize.Y*scale/2)+new Vector2(-fms.X/2, -fms.Y*2+i*(fms.Y-46)+160),
                            i==menu_choice?Color.White:Color.Red,
                            0, Vector2.Zero, scale, SpriteEffects.None, 0);
                    }

                    spriteBatch.Draw(
                        dpad,
                        Helpers.Scale(new Rectangle((int)roomSize.X/2-8, 16, 16, 16), scale), Color.White);

                    string str = "Music: "+Math.Round(MusicVolume*100)+"%";
                    fms = font.MeasureString(str);
                    spriteBatch.DrawString(
                        font,
                        str,
                        new Vector2(roomSize.X/2-fms.X/2-12, 16+fms.Y/8)*scale,
                        Color.White,
                        0, Vector2.Zero, scale/2, SpriteEffects.None, 0);
                    str = "Sound: "+Math.Round(EffectVolume*100)+"%";
                    fms = font.MeasureString(str);
                    spriteBatch.DrawString(
                        font,
                        str,
                        new Vector2(roomSize.X/2+12, 16+fms.Y/8)*scale,
                        Color.White,
                        0, Vector2.Zero, scale/2, SpriteEffects.None, 0);


                    spriteBatch.End();
                    break;
                case GameStates.Game:
                    RenderTarget2D rt = new RenderTarget2D(graphics.GraphicsDevice, (int)(roomSize.X*scale), (int)(roomSize.Y*scale));
                    graphics.GraphicsDevice.SetRenderTarget(rt);

                    spriteBatch.Begin(
                        SpriteSortMode.Deferred,
                        BlendState.NonPremultiplied,
                        SamplerState.PointClamp,
                        DepthStencilState.Default,
                        RasterizerState.CullNone);

                    spriteBatch.Draw(
                        texture_background,
                        Helpers.Scale(new Rectangle(-backgroundScroll,0,(int)roomSize.X,(int)roomSize.Y), scale),
                        Color.White); //background

                    spriteBatch.Draw(
                        texture_background,
                        Helpers.Scale(new Rectangle((int)roomSize.X-backgroundScroll,0,(int)roomSize.X,(int)roomSize.Y), scale),
                        Color.White); //background (again, for scroll seam)

                    foreach(Particle p in particles) {
                        p.Draw(spriteBatch);
                    }

                    player.Draw(spriteBatch);

                    foreach(Enemy e in enemies) {
                        e.Draw(spriteBatch);
                    }

                    foreach(Bullet b in bullets) {
                        b.Draw(spriteBatch);
                    }

                    foreach(Asteroid a in asteroids) {
                        a.Draw(spriteBatch);
                    }
                    
                    spriteBatch.End();

                    graphics.GraphicsDevice.SetRenderTarget(null);
                    Color[] data = new Color[(int)((roomSize.X*scale)*(roomSize.Y*scale))];
                    for(int x = 0;x<roomSize.X*scale;x++) {
                        data[x+(int)(roomSize.Y)] = new Color(1,0,0);
                    }
                    rt.GetData(data);
                    Texture2D render = new Texture2D(graphics.GraphicsDevice, (int)(roomSize.X*scale), (int)(roomSize.Y*scale));
                    render.SetData(data);

                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin(
                        SpriteSortMode.Deferred,
                        BlendState.NonPremultiplied,
                        SamplerState.PointClamp,
                        DepthStencilState.Default,
                        RasterizerState.CullNone);
                    spriteBatch.Draw(
                        render,
                        Helpers.Scale(new Rectangle(
                            (int)(-shake/2+(shake*random.NextDouble())),
                            (int)(-shake/2+(shake*random.NextDouble())),
                            (int)roomSize.X, (int)roomSize.Y), scale),
                        new Rectangle(0,0,(int)((roomSize.X*scale)/1), (int)((roomSize.Y*scale)/1)),
                        Color.White);
                    string stars = "";
                    switch(Difficulty) {
                        case Difficulties.Recruit: stars = "*";break;
                        case Difficulties.Veteran: stars = "**";break;
                        case Difficulties.General: stars = "***";break;
                        case Difficulties.SpaceAce: stars = "****";break;
                    }

                    str = "Score - "+(score).ToString()+" (high - "+highscores[Difficulty]+")\nMultiplier - "+(multiplier/10f).ToString().Replace(',','.');
                    str+="\nDifficulty - "+stars;
                    if(!player.shielded) {
                        str += "\nShield restored in "+(killsToReg-killsSinceLeak).ToString()+" kills.";
                    }

                    spriteBatch.DrawString(ingame,
                        str,
                        new Vector2(6,4), Color.White,
                        0, Vector2.Zero, scale/3, SpriteEffects.None, 0);
                    spriteBatch.End();

                    break;
            }

            spriteBatch.Begin();
            spriteBatch.Draw(particle, Helpers.Scale(
                new Rectangle(0,0,(int)roomSize.X,(int)roomSize.Y),scale),
                new Color(0f,0f,0f,(float)blackFadeTimer/blackFadeTimerFreq));
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public float framesToSec(float frames) {
            return (float)Math.Round(frames/60f,2);
        }
    }
}
