using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Timers;
using System.Diagnostics;

namespace PemaChip8
{
	public class Chip8
	{
		//Constants
		public const int RamSize = 4096;
		public const int NumRegisters = 16;
		public const int ProgramCounterStartPos = 512;
		public const int StackSize = 16;
		public const int ScreenWidth = 64;
		public const int ScreenHeight = 32;
		public readonly byte[] HexChars = new byte[] 
		{              
			0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
			0x20, 0x60, 0x20, 0x20, 0x70, // 1
			0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
			0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
			0x90, 0x90, 0xF0, 0x10, 0x10, // 4
			0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
			0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
			0xF0, 0x10, 0x20, 0x40, 0x40, // 7
			0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
			0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
			0xF0, 0x90, 0xF0, 0x90, 0x90, // A
			0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
			0xF0, 0x80, 0x80, 0x80, 0xF0, // C
			0xE0, 0x90, 0x90, 0x90, 0xE0, // D
			0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
			0xF0, 0x80, 0xF0, 0x80, 0x80  // F
		};
		public const int PixelSize = 10;

		//Variables
		public bool[,] Screen { get; set; }
		public byte[] RAM { get; set; }
		public byte[] Registers { get; set; }
		public short IRegister { get; set; }
		public byte SoundRegister { get; set; }
		public byte DelayRegister { get; set; }
		public short ProgramCounter { get; set; }
		public Stack<short> Stack { get; set; }
		public Keys[] Input { get; set; }
		public Random Rand { get; set; }
		public bool ProgramLoaded { get; set; }
		public bool WaitingForKeyPress { get; set; }
		public int WaitingForKeyPressRegister { get; set; }

		public Texture2D PixelSprite { get; set; }

		public Chip8(Texture2D PixelSprite)
		{
			this.PixelSprite = PixelSprite;
			Reset();
		}

		public void Reset()
		{
			Screen = new bool[ScreenWidth, ScreenHeight];
			RAM = new byte[RamSize];
			Registers = new byte[NumRegisters];
			IRegister = 0;
			SoundRegister = 0;
			DelayRegister = 0;
			ProgramCounter = ProgramCounterStartPos;
			Stack = new Stack<short>(StackSize);
			Rand = new Random();
			ProgramLoaded = false;
			WaitingForKeyPress = false;

			//Load HexChars
			for (int i = 0; i < HexChars.Length; i++)
			{
				RAM[i] = HexChars[i];
			}

			//Set Input keys
			Input = new Keys[] 
			{	
				Keys.X,
				
				Keys.D1, Keys.D2, Keys.D3,
				Keys.Q,  Keys.W,  Keys.E,
				Keys.A,  Keys.S,  Keys.D,

				Keys.Z, Keys.C, Keys.D4, Keys.R, Keys.F, Keys.V
			};
		}

		public void LoadProgram(byte[] ROM)
		{
			Reset();

			for (int i = 0; i < ROM.Length; i++)
			{
				RAM[i + ProgramCounter] = ROM[i];
			}

			ProgramLoaded = true;
		}

		public void Step()
		{
			
			string Instruction = ((RAM[ProgramCounter] << 8) | (RAM[ProgramCounter+1] & 0xff)).ToHex();
			//Console.WriteLine(Instruction);

			if (WaitingForKeyPress)
			{
				KeyboardState State = Keyboard.GetState();
				for (int i = 0; i < Input.Length; i++)
				{
					if (State.IsKeyDown(Input[i]))
					{
						WaitingForKeyPress = false;
						Registers[WaitingForKeyPressRegister] = (byte)i;
						break;
					}
				}
			}
			else
			{
				switch (Instruction.Substring(0, 1))
				{
					case "0":
						switch (Instruction)
						{
						//CLS - Clear screen
							case "00E0":
								for (int i = 0; i < ScreenWidth; i++)
									for (int j = 0; j < ScreenHeight; j++)
										Screen[i, j] = false;
								break;

						//RET - Return from subroutine
							case "00EE":
								ProgramCounter = Stack.Pop();
								break;

							default:
								break;
						}
						break;

				//JP - Jump to location
					case "1":
						ProgramCounter = (short)(Instruction.Substring(1).ToShort()-2);
						break;

				//CALL - Call subroutine at location
					case "2":
						Stack.Push(ProgramCounter);
						ProgramCounter = (short)(Instruction.Substring(1).ToShort()-2);
						break;

				//SE - Skip next instruction if register X = byte
					case "3":
						if (Registers[Instruction.Substring(1, 1).ToInt()] == Instruction.Substring(2).ToByte())
						{
							ProgramCounter += 2;
						}
						break;
					
				//SNE - Skip next instruction if register X != byte
					case "4":
						if (Registers[Instruction.Substring(1, 1).ToInt()] != Instruction.Substring(2).ToByte())
						{
							ProgramCounter += 2;
						}
						break;
					
				//SE - Skip next instruction if register X = Y
					case "5":
						if (Registers[Instruction.Substring(1, 1).ToInt()] == Registers[Instruction.Substring(2, 1).ToInt()])
						{
							ProgramCounter += 2;
						}
						break;

				//LD - Put byte into register X
					case "6":
						Registers[Instruction.Substring(1, 1).ToInt()] = Instruction.Substring(2).ToByte();
						break;

				//ADD - Adds byte to register X
					case "7":
						Registers[Instruction.Substring(1, 1).ToInt()] += Instruction.Substring(2).ToByte();
						Registers[Instruction.Substring(1, 1).ToInt()] &= 0x00ff;
						break;

					case "8":
						switch (Instruction.Substring(3, 1))
						{
						//LD - Store register Y in X
							case "0":
								Registers[Instruction.Substring(1, 1).ToInt()] = Registers[Instruction.Substring(2, 1).ToInt()];
								break;

						//OR - Calculate X OR Y and store result in X
							case "1":
								Registers[Instruction.Substring(1, 1).ToInt()] = (byte)(Registers[Instruction.Substring(1, 1).ToInt()] | Registers[Instruction.Substring(2, 1).ToInt()]);
								break;
							
						//AND - Calculate X AND Y and store result in X
							case "2":
								Registers[Instruction.Substring(1, 1).ToInt()] = (byte)(Registers[Instruction.Substring(1, 1).ToInt()] & Registers[Instruction.Substring(2, 1).ToInt()]);
								break;
							
						//XOR - Calculate X XOR Y and store result in X
							case "3":
								Registers[Instruction.Substring(1, 1).ToInt()] = (byte)(Registers[Instruction.Substring(1, 1).ToInt()] ^ Registers[Instruction.Substring(2, 1).ToInt()]);
								break;

						//ADD - Calculate X + Y and store result in X, VF is carry
							case "4":
								if (Registers[Instruction.Substring(1, 1).ToInt()] + Registers[Instruction.Substring(2, 1).ToInt()] > 0xff)
									Registers[15] = 1;
								else
									Registers[15] = 0;
								Registers[Instruction.Substring(1, 1).ToInt()] += Registers[Instruction.Substring(2, 1).ToInt()];
								Registers[Instruction.Substring(1, 1).ToInt()] &= 0x00ff;
								break;

						//SUB - Calculate X - Y and store result in X, VF is borrow
							case "5":
								if (Registers[Instruction.Substring(2, 1).ToInt()] > Registers[Instruction.Substring(1, 1).ToInt()])
									Registers[15] = 0;
								else
									Registers[15] = 1;

								Registers[Instruction.Substring(1, 1).ToInt()] -= Registers[Instruction.Substring(2, 1).ToInt()];
								Registers[Instruction.Substring(1, 1).ToInt()] &= 0x00ff;
								break;

						//SHR 
							case "6":
								if (Registers[Instruction.Substring(2, 1).ToInt()].GetBit(7))
								{
									Registers[15] = 1;
								}
								else
									Registers[15] = 0;

								Registers[Instruction.Substring(1, 1).ToInt()] >>= 1;
								break;
							
						//SUBN
							case "7":
								if (Registers[Instruction.Substring(2, 1).ToInt()] > Registers[Instruction.Substring(1, 1).ToInt()])
									Registers[15] = 1;
								else
									Registers[15] = 0;

								Registers[Instruction.Substring(1, 1).ToInt()] = (byte)(Registers[Instruction.Substring(2, 1).ToInt()] - Registers[Instruction.Substring(1, 1).ToInt()]);
								Registers[Instruction.Substring(1, 1).ToInt()] &= 0x00ff;
								break;

						//SHL
							case "E":
								if (Registers[Instruction.Substring(2, 1).ToInt()].GetBit(0))
								{
									Registers[15] = 1;
								}
								else
									Registers[15] = 0;

								Registers[Instruction.Substring(1, 1).ToInt()] <<= 1;
								Registers[Instruction.Substring(1, 1).ToInt()] &= 0x00ff;
								break;

							default:
								break;
						}
						break;

				//SNE - Skip next instruction if register X != Y
					case "9":
						if (Registers[Instruction.Substring(1, 1).ToInt()] != Registers[Instruction.Substring(2, 1).ToInt()])
						{
							ProgramCounter += 2;
						}
						break;

				//LD - Set I to value
					case "A":
						IRegister = Instruction.Substring(1).ToShort();
						break;
					
				//JP - Jump to location + register 0
					case "B":
						ProgramCounter = (short)(Instruction.Substring(1).ToShort() + Registers[0]-2);
						break;
					
				//RND - Register X = random byte AND byte
					case "C":
						Registers[Instruction.Substring(1, 1).ToInt()] = (byte)((byte)Rand.Next(0, 256) & Instruction.Substring(2).ToByte());
						break;

				//DRW - Draw sprite starting at memory location I, at position (Register X, Y), set VF to 1 if the sprite collides with anything
					case "D":
						byte SpriteX = Registers[Instruction.Substring(1, 1).ToInt()];
						byte SpriteY = Registers[Instruction.Substring(2, 1).ToInt()];
						byte SpriteLength = Instruction.Substring(3, 1).ToByte();

						bool Collision = false;
						for (int Y = 0; Y < SpriteLength; Y++)
						{
							byte Sprite = RAM[IRegister + Y];
							for (int X = 0; X < 8; X++)
							{
								if (Sprite.GetBit(X))
								{
									if (DrawPixel(SpriteX + X, SpriteY + Y))
									{
										Collision = true;
									}
								}
							}
						}

						if (Collision)
							Registers[15] = 1;
						else
							Registers[15] = 0;
						break;
					
					case "E":
						switch (Instruction.Substring(2))
						{
						//SKP - Skip next instruction is key with value of register X is pressed
							case "9E":
								if (Keyboard.GetState().IsKeyDown(Input[Registers[Instruction.Substring(1, 1).ToInt()]]))
								{
									ProgramCounter += 2;
								}
								break;

						//SKNP - Skip next instruction is key with value of register X isn't pressed
							case "A1":
								if (!Keyboard.GetState().IsKeyDown(Input[Registers[Instruction.Substring(1, 1).ToInt()]]))
								{
									ProgramCounter += 2;
								}
								break;

							default:
								break;
						}
						break;
					
					case "F":
						switch (Instruction.Substring(2))
						{
						//LD - Set register X to delay timer value
							case "07":
								Registers[Instruction.Substring(1, 1).ToInt()] = DelayRegister;
								break;
							
						//LD - Wait for key press, store value in register X
							case "0A":
								WaitingForKeyPress = true;
								WaitingForKeyPressRegister = Instruction.Substring(1, 1).ToInt();
								break;
							
						//LD - Set delay timer to register x
							case "15":
								DelayRegister = Registers[Instruction.Substring(1, 1).ToInt()];
								break;
							
						//LD - Set sound timer to register X
							case "18":
								SoundRegister = Registers[Instruction.Substring(1, 1).ToInt()];
								break;
							
						//ADD - Set I to I + register X
							case "1E":
								if (IRegister + Registers[Instruction.Substring(1, 1).ToInt()] > 0xfff)
									Registers[15] = 1;
								else
									Registers[15] = 0;
								
								IRegister += Registers[Instruction.Substring(1, 1).ToInt()];
								IRegister &= 0x0fff;
								break;
							
						//LD - Set I equal to location of sprite for digit register X
							case "29":
								IRegister = (short)(Registers[Instruction.Substring(1, 1).ToInt()] * 5);
								break;
							
						//LD - Store BCD of register X in I, I+1, I+2
							case "33":
								string Number = Registers[Instruction.Substring(1, 1).ToInt()].ToString();
								if (Number.Length == 3)
								{
									RAM[IRegister] = byte.Parse(Number.Substring(0, 1));
									RAM[IRegister + 1] = byte.Parse(Number.Substring(1, 1));
									RAM[IRegister + 2] = byte.Parse(Number.Substring(2, 1));
								}
								if (Number.Length == 2)
								{
									RAM[IRegister] = 0;
									RAM[IRegister + 1] = byte.Parse(Number.Substring(0, 1));
									RAM[IRegister + 2] = byte.Parse(Number.Substring(1, 1));
								}
								if (Number.Length == 1)
								{
									RAM[IRegister] = 0;
									RAM[IRegister + 1] = 0;
									RAM[IRegister + 2] = byte.Parse(Number);
								}
								break;

						//LD - Store registers 0 through X in memory starting at location I register
							case "55":
								for (int i = 0; i <= Instruction.Substring(1, 1).ToInt(); i++)
									RAM[IRegister + i] = Registers[i];
								IRegister += (short)(Instruction.Substring(1, 1).ToInt() + 1);
								break;

						//LD - Read registers 0 through X from memory starting at location I register
							case "65":
								for (int i = 0; i <= Instruction.Substring(1, 1).ToInt(); i++)
									Registers[i] = RAM[IRegister + i];
								IRegister += (short)(Instruction.Substring(1, 1).ToInt() + 1);
								break;

							default:
								break;
						}
						break;

					default:
						break;
				}

				ProgramCounter += 2;

				if (DelayRegister > 0)
					DelayRegister--;
				if (SoundRegister > 0)
					SoundRegister--;
			}
		}

		public void Update(GameTime gameTime)
		{
			if (ProgramLoaded)
			{
				//var T = new Stopwatch();
				//T.Start();
				Step();
				//Console.WriteLine(T.Elapsed);
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			for (int i = 0; i < ScreenWidth; i++)
			{
				for (int j = 0; j < ScreenHeight; j++)
				{
					if (Screen[i, j])
						spriteBatch.Draw(PixelSprite, new Rectangle(i*PixelSize, j*PixelSize, PixelSize, PixelSize), Color.White);
				}
			}
		}

		public bool DrawPixel(int X, int Y)
		{
			if (X >= ScreenWidth)
			{
				X -= ScreenWidth;
			}
			else if (X < 0)
			{
				X += ScreenWidth;
			}

			if (Y >= ScreenHeight)
			{
				Y -= ScreenHeight;
			}
			else if (Y < 0)
			{
				Y += ScreenHeight;
			}

			bool Old = Screen[X, Y];
			Screen[X, Y] = Screen[X, Y] ^ true;

			if (Old == true && Screen[X, Y] == false)
				return true;
			else
				return false;
		}
	}
}

