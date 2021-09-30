using System;
using System.Collections.Generic;
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
        
        private const float GAME_TICK = 0.05f;
        private const float INPUT_TICK = 0.06f;

        private readonly List<Sprite> _tetrominos;
        
        private Sprite _field;
        private Sprite _currentPiece;
        private Sprite _ghostPiece;
        
        private int _currentRotation;
        
        private float _gameTimer;
        private float _inputTimer;
        private int _level;

        private int Speed => 10 - _level;

        private int _speedCounter;
        private bool _forceDown;
        private bool _gameOver;

        private int _lineCount;
        private int _score;
        
        private List<int> _clearedLines;
        

        public Tetris()
        {
            InitConsole(32, 60, 16);

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

        protected override bool Create()
        {
            _level = 0;
            
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

        private void SpawnNewPiece()
        {
            _currentPiece = _tetrominos[_rng.Next(0, 7)];
            _currentPiece.Position = _field.Bounds.Center + Vector.Up * _field.Height / 2 + Vector.Left * _currentPiece.Width / 2;
            _ghostPiece = new Sprite(_currentPiece);
            _ghostPiece.SetSpriteBackground(ConsoleColor.Black);
            _currentRotation = 0;
        }

        protected override bool Update(float elapsedTime, KeyboardInput input)
        {
            if (input.IsKeyDown(KeyCode.Esc))
            {
                return false;
            }

            if (_gameOver)
            {
                // TODO: Draw game over screen
                // TODO: Restart Button
            }
            
            if (input.IsKeyDown(KeyCode.Z)) RotatePiece(-90);
            else if (input.IsKeyDown(KeyCode.X)) RotatePiece(90);

            _inputTimer -= elapsedTime;
            if (_inputTimer <= 0)
            {
                _inputTimer = INPUT_TICK;
                if (input.IsKeyHeld(KeyCode.Left))  MovePiece(_currentPiece, _currentRotation, Vector.Left);
                if (input.IsKeyHeld(KeyCode.Right)) MovePiece(_currentPiece, _currentRotation, Vector.Right);
                if (input.IsKeyHeld(KeyCode.Down))  MovePiece(_currentPiece, _currentRotation, Vector.Down);
            }
            
            if (input.IsKeyDown(KeyCode.Up)) HardDropPiece();
            
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
                        
                        if (!DoesPieceFit(_currentPiece, _currentRotation, _currentPiece.Position))
                        {
                            _gameOver = true;
                            return false;
                        }
                    }

                    _speedCounter = 0;
                    _forceDown = false;
                }
                
                DrawString(3 * Vector.Down, $"Score: {_score}");
                DrawString(5 * Vector.Down, $"Lines: {_lineCount}");
                
                DrawGhostPiece();
                DrawRotatedSprite(_currentPiece, _currentRotation);
                DrawSprite(_field);
            }
            
            return true;
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

        private void CheckLines()
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
                            _field.SetGlyph(x, i, above);

                            if (above == 'X')
                            {
                                _field.SetFgColor(x, i, ConsoleColor.DarkGray);
                                _field.SetBgColor(x, i, ConsoleColor.DarkGray);
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
                        _field.SetFgColor(fieldRelativePosition, ConsoleColor.DarkGray);
                        _field.SetBgColor(fieldRelativePosition, ConsoleColor.DarkGray);
                    }
                }
            }

            _score += 25;
            _speedCounter = 0;
            CheckLines();
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
    }
}