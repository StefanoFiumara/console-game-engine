using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Runner.Games
{
    // ReSharper disable once UnusedType.Global
    public class Tetris : ConsoleGameEngineBase
    {
        protected override string Name => "Tetris";

        private readonly Random _rng;
        
        private const float GAME_TICK = 0.06f;
        private const float INPUT_TICK = 0.087f;

        private readonly List<Sprite> _tetrominos;
        
        private Sprite _field;
        private Sprite _currentPiece;
        private Sprite _ghostPiece;
        
        private int _currentRotation;
        
        private float _gameTimer;
        private float _inputTimer;
        private int _level;

        private int Speed => 20 - _level;

        private int _speedCounter;

        private bool _inputHeld;
        private bool _forceDown;
        private bool _gameOver;

        private bool _lockGray = true;
        
        private int _lineCount;
        private int _score;
        
        private List<int> _clearedLines;
        private int _droppedPieces;

        private Sprite _heldPiece;

        private List<Sprite> _randomizerBag;
        
        public Tetris()
        {
            InitConsole(32, 40, 16);

            _rng = new Random();

            var shapes = new string[7];
            
            shapes[0] += "    " + '\n';
            shapes[0] += "XXXX" + '\n';
            shapes[0] += "    " + '\n';
            shapes[0] += "    " + '\n';

            shapes[1] += "    " + '\n';
            shapes[1] += " XX " + '\n';
            shapes[1] += "XX  " + '\n';
            shapes[1] += "    " + '\n';

            shapes[2] += "    " + '\n';
            shapes[2] += " XX " + '\n';
            shapes[2] += "  XX" + '\n';
            shapes[2] += "    " + '\n';

            shapes[3] += "    " + '\n';
            shapes[3] += "  X " + '\n';
            shapes[3] += " XXX" + '\n';
            shapes[3] += "    " + '\n';

            shapes[4] += "    " + '\n';
            shapes[4] += " X  " + '\n';
            shapes[4] += " XXX" + '\n';
            shapes[4] += "    " + '\n';

            shapes[5] += "    " + '\n';
            shapes[5] += "  X " + '\n';
            shapes[5] += "XXX " + '\n';
            shapes[5] += "    " + '\n';
            
            shapes[6] += "    " + '\n';
            shapes[6] += " XX " + '\n';
            shapes[6] += " XX " + '\n';
            shapes[6] += "    " + '\n';

            var colors = new[]
            {
                ConsoleColor.Cyan, 
                ConsoleColor.Green, 
                ConsoleColor.Red, 
                ConsoleColor.Magenta, 
                ConsoleColor.Blue, 
                ConsoleColor.DarkYellow, 
                ConsoleColor.Yellow
            };
            
            _tetrominos = new List<Sprite>();
            
            for (var i = 0; i < shapes.Length; i++)
            {
                var shape = shapes[i];
                var sprite = new Sprite(shape, colors[i], colors[i]);
                _tetrominos.Add(sprite);
            }
            
        }

        private List<Sprite> GetRandomTetrominoBag()
        {
            var result = _tetrominos.Select(tetromino => new Sprite(tetromino)).ToList();

            result.Shuffle(_rng);
            return result;
        }
        
        protected override bool Create()
        {
            _gameOver = false;
            _level = 0;
            _lineCount = 0;
            _score = 0;

            _randomizerBag = GetRandomTetrominoBag();
            _randomizerBag.AddRange(GetRandomTetrominoBag());
            
            //TODO: Figure out circular buffer for next piece queue
            
            var fieldLayout = string.Empty;
            for (int i = 0; i < 21; i++)
            {
                fieldLayout += "#          #\n";
            }
            fieldLayout += "############\n";
            _field = new Sprite(fieldLayout);
            _field.Position = ScreenRect.Center - _field.Bounds.Size * 0.5f + 7 * Vector.Down;
            
            _clearedLines = new List<int>();

            SpawnNewPiece();

            return true;
        }

        protected override bool Update(float elapsedTime, KeyboardInput input)
        {
            if (input.IsKeyDown(KeyCode.Esc))
            {
                return false;
            }

            if (_gameOver)
            {
                Fill(ScreenRect, ' ');
                DrawString(ScreenRect.Center, "GAME OVER", centered: true);
                DrawString(ScreenRect.Center + 2 * Vector.Down, $"Score: {_score}", centered: true);
                DrawString(ScreenRect.Center + 4 * Vector.Down, $"Level: {_level}", centered: true);
                DrawString(ScreenRect.Center + 6 * Vector.Down, $"Lines: {_lineCount}", centered: true);
                DrawString(ScreenRect.Center + 8 * Vector.Down, $"Press ENTER to restart", centered: true);

                if (input.IsKeyDown(KeyCode.Enter))
                {
                    return Create();
                }
                return true;
            }
            
            HandleInput(elapsedTime, input);
            
            // Ticks the game forward every GAME_TICK seconds.
            _gameTimer -= elapsedTime;
            if (_gameTimer <= 0f)
            {
                // TICK
                Fill(ScreenRect, ' ');
                
                _gameTimer = GAME_TICK;
                _speedCounter++;
                _forceDown = _speedCounter >= Speed;

                RemoveClearedLines();
                
                if (_forceDown)
                {
                    if(!MovePiece(_currentPiece, _currentRotation, Vector.Down))
                    {
                        LockPiece();
                    }

                    _speedCounter = 0;
                    _forceDown = false;
                }
                
                DrawGhostPiece();
                DrawRotatedSprite(_currentPiece, _currentRotation);
                DrawSprite(_field);
                
                DrawHud();
            }
            
            return true;
        }

        private void SpawnNewPiece(Sprite newPiece = null)
        {
            if (newPiece != null)
            {
                _currentPiece = newPiece;
            }
            else
            {
                _currentPiece = _randomizerBag[0];
                _randomizerBag.RemoveAt(0);

                if (_randomizerBag.Count <= 7)
                {
                    _randomizerBag.AddRange(GetRandomTetrominoBag());
                }
                
            }
            
            _currentPiece.Position = _field.Bounds.Center + Vector.Up * _field.Height / 2 + Vector.Left * _currentPiece.Width / 2;
            _ghostPiece = new Sprite(_currentPiece);
            _ghostPiece.SetSpriteBackground(ConsoleColor.Black);
            _currentRotation = 0;
            
            if (!DoesPieceFit(_currentPiece, _currentRotation, _currentPiece.Position))
            {
                _gameOver = true;
            }
        }

        private void HandleInput(float elapsedTime, KeyboardInput input)
        {
            // Hold queue
            if (input.IsKeyDown(KeyCode.Space))
            {
                var current = _currentPiece;
                
                SpawnNewPiece(_heldPiece);

                _heldPiece = new Sprite(current);
            }
            
            // Rotations
            if (input.IsKeyDown(KeyCode.Z)) RotatePiece(-90);
            else if (input.IsKeyDown(KeyCode.X)) RotatePiece(90);
            if (input.IsKeyDown(KeyCode.Up)) HardDropPiece();


            // Input timer for Left/Right/Down inputs
            if (input.IsKeyDown(KeyCode.Left) ||
                input.IsKeyDown(KeyCode.Right) ||
                input.IsKeyDown(KeyCode.Down))
            {
                _inputHeld = true;
                _inputTimer = 0;
            }

            if (input.IsKeyUp(KeyCode.Left) ||
                input.IsKeyUp(KeyCode.Right) ||
                input.IsKeyUp(KeyCode.Down))
            {
                _inputHeld = false;
                _inputTimer = 0;
            }

            if (_inputHeld) _inputTimer -= elapsedTime;

            _inputTimer -= elapsedTime;
            if (_inputHeld && _inputTimer <= 0)
            {
                _inputTimer = INPUT_TICK;
                if (input.IsKeyHeld(KeyCode.Left)) MovePiece(_currentPiece, _currentRotation, Vector.Left);
                if (input.IsKeyHeld(KeyCode.Right)) MovePiece(_currentPiece, _currentRotation, Vector.Right);
                if (input.IsKeyHeld(KeyCode.Down))
                {
                    MovePiece(_currentPiece, _currentRotation, Vector.Down);
                }
            }
        }

        private void DrawGhostPiece()
        {
            _ghostPiece.Position = _currentPiece.Position;
            while (DoesPieceFit(_ghostPiece, _currentRotation, _ghostPiece.Position + Vector.Down))
            {
                MovePiece(_ghostPiece, _currentRotation, Vector.Down);
            }
            
            DrawRotatedSprite(_ghostPiece, _currentRotation);
        }

        private void HardDropPiece()
        {
            while (DoesPieceFit(_currentPiece, _currentRotation, _currentPiece.Position + Vector.Down))
            {
                MovePiece(_currentPiece, _currentRotation, Vector.Down);
            }
            
            LockPiece();
        }

        private void DrawHud()
        {
            DrawString(Vector.Down + Vector.Right * ScreenWidth / 2, "TETRIS", centered: true);
            DrawString(4 * Vector.Down, $"Score: {_score}");
            DrawString(6 * Vector.Down, $"Level: {_level}");
            DrawString(8 * Vector.Down, $"Lines: {_lineCount}");
            
            DrawString(new Vector(5, 15), $"HOLD");
            if (_heldPiece != null)
            {
                _heldPiece.Position = new Vector(5, 16);
                DrawSprite(_heldPiece);
            }
            
            DrawString(new Vector(23, 15), "NEXT");

            for (int i = 0; i < 4; i++)
            {
                var nextPiece = _randomizerBag[i];
                nextPiece.Position = new Vector(23, 16 + 4 * i);
                DrawSprite(nextPiece);

            }
        }

        private void CheckForClearedLines()
        {
            for (int y = 0; y < _currentPiece.Height; y++)
            {
                bool isLine = true;
                for (int x = 1; x < _field.Width - 1; x++)
                {
                    var fieldRelativePosition = _currentPiece.Position + new Vector(x, y) - _field.Position;
                    isLine &= _field.GetGlyph(x, (int)fieldRelativePosition.Y) == 'X';
                }

                if (isLine)
                {
                    _clearedLines.Add((int)(_currentPiece.Position.Y + y - _field.Position.Y));
                }
            }

            if (_clearedLines.Count != 0)
            {
                _lineCount += _clearedLines.Count;
                _score += (1 << _clearedLines.Count) * 100;

                if (_lineCount % 10 == 0)
                {
                    _level++;
                }
            }
        }

        private void RemoveClearedLines()
        {
            if (_clearedLines.Count != 0)
            {
                foreach (var line in _clearedLines)
                {
                    for (int x = 1; x < _field.Width - 1; x++)
                    {
                        for (int i = line; i > 0; i--)
                        {
                            var above = _field.GetGlyph(x, i - 1);
                            var aboveColor = _field.GetFgColor(x, i - 1);
                            _field.SetGlyph(x, i, above);

                            if (above == 'X')
                            {
                                _field.SetFgColor(x, i, _lockGray ? ConsoleColor.DarkGray : aboveColor);
                                _field.SetBgColor(x, i, _lockGray ? ConsoleColor.DarkGray : aboveColor);
                            }
                            else
                            {
                                _field.SetFgColor(x, i, ConsoleColor.Black);
                                _field.SetBgColor(x, i, ConsoleColor.Black);
                            }
                        }
                    }
                }

                _clearedLines.Clear();
            }
        }

        private void LockPiece()
        {
            for (int x = 0; x < _currentPiece.Width; x++)
            {
                for (int y = 0; y < _currentPiece.Height; y++)
                {
                    int pieceIndex = GetRotatedIndex(x, y, _currentRotation);
                    var fieldRelativePosition = _currentPiece.Position + new Vector(x, y) - _field.Position;

                    if (_currentPiece.GetGlyph(pieceIndex) == 'X')
                    {
                        _field.SetGlyph(fieldRelativePosition, 'X');
                        _field.SetFgColor(fieldRelativePosition, _lockGray ? ConsoleColor.DarkGray : _currentPiece.GetFgColor(pieceIndex));
                        _field.SetBgColor(fieldRelativePosition, _lockGray ? ConsoleColor.DarkGray : _currentPiece.GetFgColor(pieceIndex));
                    }
                }
            }

            _score += 25;
            _speedCounter = 0;
            _droppedPieces++;

            if (_droppedPieces % 15 == 0)
            {
                _level++;
            }
            CheckForClearedLines();
            SpawnNewPiece();
        }

        private bool MovePiece(Sprite piece, int rotation, Vector direction)
        {
            if (DoesPieceFit(piece, rotation, piece.Position + direction))
            {
                piece.Position += direction;
                return true;
            }

            return false;
        }

        private void RotatePiece(int rotation)
        {
            void WrapRotation()
            {
                if (_currentRotation >= 360) _currentRotation = 0;
                if (_currentRotation < 0) _currentRotation = 270;
            }

            _currentRotation += rotation;
            WrapRotation();

            if (!DoesPieceFit(_currentPiece, _currentRotation, _currentPiece.Position))
            {
                // If the piece does not fit in its original position, check if it's possible to kick it in some direction before undoing the rotation
                if (!MovePiece(_currentPiece, _currentRotation, Vector.Left) && 
                    !MovePiece(_currentPiece, _currentRotation, Vector.Right) && 
                    !MovePiece(_currentPiece, _currentRotation, Vector.Down) && 
                    !MovePiece(_currentPiece, _currentRotation, Vector.Up))
                {
                    // If it does not fit anywhere, undo the rotation.
                    _currentRotation -= rotation;
                    WrapRotation();
                }
            }

            if (!DoesPieceFit(_currentPiece, _currentRotation, _currentPiece.Position + Vector.Down))
            {
                // Gives the player some room to play with before their piece locks into place
                _speedCounter -= 2;
            }
        }

        private bool DoesPieceFit(Sprite tetromino, int rotation, Vector newPosition)
        {
            for (int x = 0; x < tetromino.Width; x++)
            {
                for (int y = 0; y < tetromino.Height; y++)
                {
                    int pieceIndex = GetRotatedIndex(x, y, rotation);
                    var fieldRelativePosition = newPosition + new Vector(x, y) - _field.Position;

                    if (tetromino.GetGlyph(pieceIndex) == 'X' && _field.GetGlyph(fieldRelativePosition) != ' ')
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        private void DrawRotatedSprite(Sprite sprite, int rotation)
        {
            for (var y = 0; y < sprite.Height; y++)
            {
                for (int x = 0; x < sprite.Width; x++)
                {
                    var index = GetRotatedIndex(x, y, rotation);
                    if (sprite.GetGlyph(index) != ' ')
                    {
                        Draw(
                            (int)sprite.Position.X + x, 
                            (int)sprite.Position.Y + y, 
                            sprite.GetGlyph(index), 
                            sprite.GetFgColor(x, y),
                            sprite.GetBgColor(x, y));
                    }
                }
            }
        }

        private int GetRotatedIndex(int x, int y, int rotation)
        {
            switch (rotation)
            {
                case 90:
                    return 12 + y - (x * 4);
                case 180:
                    return 15 - (y * 4) - x;
                case 270:
                    return 3 - y + (x * 4);
                default:
                    return y * 4 + x;
            }
        }
    }
}