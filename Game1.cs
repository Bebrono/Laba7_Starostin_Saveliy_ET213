using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using static System.Formats.Asn1.AsnWriter;


namespace monotest
{

    public class GameObjectManager
    {
        private List<GameObject> _gameObjects; 
        private List<MovingObject> _movingObjects; 
        private List<Wall> _walls; 
        private Player _player;                   

        public GameObjectManager()
        {
            _gameObjects = new List<GameObject>();
            _movingObjects = new List<MovingObject>();
            _walls = new List<Wall>();
        }

        public void AddStaticObject(GameObject staticObject)
        {
            _gameObjects.Add(staticObject);
        }

        public void AddMovingObject(MovingObject movingObject)
        {
            _movingObjects.Add(movingObject);
        }

        public void SetPlayer(Player player)
        {
            _player = player;
        }
        public void AddWall(Wall wall)
        {
            _walls.Add(wall);
        }

        public void Update(GameTime gameTime)
        {
            foreach (var obj in _movingObjects)
            {
                obj.Update(gameTime, _walls);
            }

            _player?.Update(gameTime, _walls);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var obj in _gameObjects)
            {
                obj.Draw(spriteBatch);
            }

            foreach (var obj in _movingObjects)
            {
                obj.Draw(spriteBatch);
            }

            foreach (var obj in _walls)
            {
                obj.Draw(spriteBatch);
            }

            _player?.Draw(spriteBatch);
        }
    }
    public class GameObject
    {
        protected Texture2D _texture;
        protected Vector2 _position; 
        protected Vector2 _size;    
        protected float _layer;     

        public GameObject(Texture2D texture, Vector2 position, Vector2 size, float layer = 0f)
        {
            _texture = texture;
            _position = position;
            _size = size;
            _layer = layer;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _texture,                     
                new Rectangle(                
                    (int)_position.X,
                    (int)_position.Y,
                    (int)_size.X,
                    (int)_size.Y),
                null,               
                Color.White,                 
                0f,                           
                Vector2.Zero,                 
                SpriteEffects.None,           
                _layer                        
            );
        }
    }


    public class MovingObject : GameObject
    {
        protected Vector2 _velocity;
        private Random _random;

        public MovingObject(Texture2D texture, Vector2 position, Vector2 velocity, Vector2 size)
            : base(texture, position, size)
        {
            _velocity = velocity;
            _random = new Random(); 
        }

        public void Update(GameTime gameTime, List<Wall> walls)
        {
            Vector2 newPosition = _position + _velocity;

            Rectangle newBounds = new Rectangle((int)newPosition.X, (int)newPosition.Y, (int)_size.X, (int)_size.Y);
            foreach (var wall in walls)
            {
                if (wall.IsColliding(newBounds))
                {
                    float randomAngle = (float)(_random.NextDouble() * Math.PI * 2);
                    _velocity = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * _velocity.Length();

                    return;
                }
            }

            _position = newPosition;
        }
    }



    public class Player : MovingObject
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;

        public Player(Texture2D texture, Vector2 position, Vector2 size)
            : base(texture, position, Vector2.Zero, size)
        {
        }

        public void Update(GameTime gameTime, List<Wall> walls)
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            Vector2 movement = Vector2.Zero;

            if (_currentKeyboardState.IsKeyDown(Keys.W) || _currentKeyboardState.IsKeyDown(Keys.Up))
                movement.Y -= 1;
            if (_currentKeyboardState.IsKeyDown(Keys.S) || _currentKeyboardState.IsKeyDown(Keys.Down))
                movement.Y += 1;
            if (_currentKeyboardState.IsKeyDown(Keys.A) || _currentKeyboardState.IsKeyDown(Keys.Left))
                movement.X -= 1;
            if (_currentKeyboardState.IsKeyDown(Keys.D) || _currentKeyboardState.IsKeyDown(Keys.Right))
                movement.X += 1;

            if (movement.Length() > 0)
                movement.Normalize();

            Vector2 newPosition = _position + movement * 5f;

            Rectangle newBounds = new Rectangle((int)newPosition.X, (int)newPosition.Y, (int)_size.X, (int)_size.Y);
            foreach (var wall in walls)
            {
                if (wall.IsColliding(newBounds))
                {
                    return;
                }
            }

            _position = newPosition;
        }
    }
    public class Wall : GameObject
    {
        public Rectangle Bounds => new Rectangle(
            (int)_position.X,
            (int)_position.Y,
            (int)(_size.X),
            (int)(_size.Y));

        public Wall(Texture2D texture, Vector2 position, Vector2 size)
            : base(texture, position, size)
        {
        }

        public bool IsColliding(Rectangle otherBounds)
        {
            return Bounds.Intersects(otherBounds);
        }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private List<GameObject> _staticObjects;
        private List<MovingObject> _movingObjects;
        private GameObjectManager _gameObjectManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();

            _gameObjectManager = new GameObjectManager();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);


            // TODO: use this.Content to load your game content here\

            Texture2D cupepTexture = Content.Load<Texture2D>("chupep");
            Texture2D cheliabinskTexture = Content.Load<Texture2D>("cheliabinsk");
            Texture2D sobakaTexture = Content.Load<Texture2D>("sobaka");
            Texture2D playerTexture = Content.Load<Texture2D>("player");
            Texture2D oselTexture = Content.Load<Texture2D>("osel");

            _staticObjects = new List<GameObject>();
            _movingObjects = new List<MovingObject>();

            Vector2 size = new Vector2(120, 120);

            Vector2 startPosition = new Vector2(100, 100);
            float spacing = 500;
            
            startPosition = new Vector2(0, 0);
            spacing = 120;
            for (int i = 0; i < 16; i++)
            {
                Vector2 position = new Vector2(startPosition.X + i * spacing, startPosition.Y);
                _gameObjectManager.AddWall(new Wall(oselTexture, position, size));
            }
            startPosition = new Vector2(0, 960);
            spacing = 120;
            for (int j = 0; j < 16; j++)
            {
                Vector2 position = new Vector2(startPosition.X + j * spacing, startPosition.Y);
                _gameObjectManager.AddWall(new Wall(oselTexture, position, size));
            }
            startPosition = new Vector2(0, 120);
            spacing = 120;
            for (int j = 0; j < 7; j++)
            {
                Vector2 position = new Vector2(startPosition.X, startPosition.Y + j * spacing);
                _gameObjectManager.AddWall(new Wall(oselTexture, position, size));
            }
            startPosition = new Vector2(1800, 120);
            spacing = 120;
            for (int j = 0; j < 7; j++)
            {
                Vector2 position = new Vector2(startPosition.X, startPosition.Y + j * spacing);
                _gameObjectManager.AddWall(new Wall(oselTexture, position, size));
            }


            _gameObjectManager.AddWall(new Wall(oselTexture, new Vector2(345, 678), size));
            _gameObjectManager.AddWall(new Wall(oselTexture, new Vector2(450, 500), size));
            _gameObjectManager.AddWall(new Wall(oselTexture, new Vector2(1444, 555), size));
            _gameObjectManager.AddWall(new Wall(oselTexture, new Vector2(1000, 500), size));
            _gameObjectManager.AddMovingObject(new MovingObject(sobakaTexture, new Vector2(130, 130), new Vector2(0, 3), size));
            _gameObjectManager.AddMovingObject(new MovingObject(sobakaTexture, new Vector2(200, 200), new Vector2(4, 0), size));
            _gameObjectManager.AddMovingObject(new MovingObject(sobakaTexture, new Vector2(300, 300), new Vector2(2, 2), size));
            _gameObjectManager.AddMovingObject(new MovingObject(sobakaTexture, new Vector2(700, 700), new Vector2(-10, -10), size));

            _gameObjectManager.SetPlayer(new Player(playerTexture, new Vector2(800, 800), size));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            _gameObjectManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            foreach (var obj in _staticObjects)
            {
                obj.Draw(_spriteBatch);
            }

            _gameObjectManager.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
