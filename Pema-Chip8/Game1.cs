#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using System.IO;

#endregion

namespace PemaChip8
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		public const int FPS = 150;

		public Chip8 Chip8 { get; set; }

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";	            
			graphics.IsFullScreen = false;
			graphics.PreferredBackBufferWidth = Chip8.PixelSize * Chip8.ScreenWidth;
			graphics.PreferredBackBufferHeight = Chip8.PixelSize * Chip8.ScreenHeight;
			TargetElapsedTime = TimeSpan.FromMilliseconds((double)((double)1000/(double)FPS));
			graphics.ApplyChanges();
		}
			
		protected override void Initialize()
		{
			base.Initialize();
		}
			
		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);

			Texture2D Blank = new Texture2D(GraphicsDevice, 1, 1);
			Blank.SetData<Color>(new Color[] { Color.Black });

			Chip8 = new Chip8(Blank);
			Chip8.LoadProgram(File.ReadAllBytes("Roms/BRIX"));
		}

		protected override void Update(GameTime gameTime)
		{
			Chip8.Update(gameTime);

			base.Update(gameTime);
		}
			
		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
			spriteBatch.Begin();

			Chip8.Draw(spriteBatch);
		
			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}

