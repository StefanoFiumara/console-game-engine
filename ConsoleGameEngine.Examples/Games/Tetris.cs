using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Graphics.Renderers;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Runner.Games;

// ReSharper disable once UnusedType.Global
public class Tetris : ConsoleGame
{
    private readonly Random _rng;
        
    private const float GameTick = 0.06f;
    private const float InputTick = 0.110f;

    private readonly List<Sprite> _tetrominos;
        
    private GameEntity _field;
    private GameEntity _currentPiece;
    private GameEntity _ghostPiece;
        
    private int _currentRotation;
        
    private float _gameTimer;
    private float _inputTimer;
    private int _level;

    private int Speed => 20 - _level;

    private int _speedCounter;

    private bool _inputHeld;
    private bool _forceDown;
    private bool _gameOver;
    
    private int _lineCount;
    private int _score;
        
    private List<int> _clearedLines;
    private int _droppedPieces;
        
    private readonly Vector _heldPiecePosition = new (5, 16);
    private Sprite _heldPiece;
    
    private List<Sprite> _randomizerBag;
        
    public Tetris() : base(new ConsoleRenderer(width: 32, height: 40, pixelSize: 16))
    {
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
            Color24.Cyan, 
            Color24.Green, 
            Color24.Red, 
            Color24.DarkMagenta, 
            Color24.Blue, 
            new Color24(255, 127, 0), // Orange 
            Color24.Yellow
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
        var result = _tetrominos.Select(t => new Sprite(t)).ToList();
        _rng.Shuffle(result);
        return result;
    }
        
    protected override bool Create(IRenderer renderer)
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
        _field = new GameEntity(Sprite.Create(fieldLayout));
        _field.Position = renderer.Bounds.Center - _field.Bounds.Size * 0.5f + 7 * Vector.Down;
            
        _clearedLines = new List<int>();

        SpawnNewPiece();

        return true;
    }

    protected override bool Update(float elapsedTime, IRenderer renderer, PlayerInput input)
    {
        if (input.IsKeyDown(KeyCode.Esc))
        {
            return false;
        }

        if (_gameOver)
        {
            renderer.Fill(' ');
            renderer.DrawString(renderer.Bounds.Center, "GAME OVER", alignment: TextAlignment.Centered);
            renderer.DrawString(renderer.Bounds.Center + 2 * Vector.Down, $"Score: {_score}", alignment: TextAlignment.Centered);
            renderer.DrawString(renderer.Bounds.Center + 4 * Vector.Down, $"Level: {_level}", alignment: TextAlignment.Centered);
            renderer.DrawString(renderer.Bounds.Center + 6 * Vector.Down, $"Lines: {_lineCount}", alignment: TextAlignment.Centered);
            renderer.DrawString(renderer.Bounds.Center + 8 * Vector.Down, $"Press ENTER to restart", alignment: TextAlignment.Centered);

            if (input.IsKeyDown(KeyCode.Enter))
            {
                return Create(renderer);
            }
            return true;
        }
            
        HandleInput(elapsedTime, input);
            
        // Ticks the game forward every GAME_TICK seconds.
        _gameTimer -= elapsedTime;
        if (_gameTimer <= 0f)
        {
            // TICK
            renderer.Fill(' ');
                
            _gameTimer = GameTick;
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
                
            DrawGhostPiece(renderer);
            DrawRotatedPiece(renderer, _currentPiece, _currentRotation);
            renderer.DrawObject(_field);
                
            DrawHud(renderer);
        }
            
        return true;
    }

    private void SpawnNewPiece(Sprite newPiece = null)
    {
        if (newPiece != null)
        {
            _currentPiece = new GameEntity(newPiece);
        }
        else
        {
            _currentPiece = new GameEntity(_randomizerBag[0]);
            _randomizerBag.RemoveAt(0);

            if (_randomizerBag.Count <= 7)
            {
                _randomizerBag.AddRange(GetRandomTetrominoBag());
            }
                
        }
            
        _currentPiece.Position = _field.Bounds.Center + Vector.Up * _field.Bounds.Height / 2 + Vector.Left * _currentPiece.Bounds.Width / 2;
        _ghostPiece = new GameEntity(new(_currentPiece.Sprite));
        _ghostPiece.Sprite.SetSpriteBackground(Color24.Black);
        _currentRotation = 0;
            
        if (!DoesPieceFit(_currentPiece, _currentRotation, _currentPiece.Position))
        {
            _gameOver = true;
        }
    }

    private void HandleInput(float elapsedTime, PlayerInput input)
    {
        // Hold queue
        if (input.IsKeyDown(KeyCode.Space))
        {
            var current = _currentPiece;
                
            SpawnNewPiece(_heldPiece);

            _heldPiece = new Sprite(current.Sprite);
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
            _inputTimer = InputTick;
            if (input.IsKeyHeld(KeyCode.Left)) MovePiece(_currentPiece, _currentRotation, Vector.Left);
            if (input.IsKeyHeld(KeyCode.Right)) MovePiece(_currentPiece, _currentRotation, Vector.Right);
            if (input.IsKeyHeld(KeyCode.Down))
            {
                MovePiece(_currentPiece, _currentRotation, Vector.Down);
            }
        }
    }

    private void DrawGhostPiece(IRenderer renderer)
    {
        _ghostPiece.Position = _currentPiece.Position;
        while (DoesPieceFit(_ghostPiece, _currentRotation, _ghostPiece.Position + Vector.Down))
        {
            MovePiece(_ghostPiece, _currentRotation, Vector.Down);
        }
            
        DrawRotatedPiece(renderer, _ghostPiece, _currentRotation);
    }

    private void HardDropPiece()
    {
        while (DoesPieceFit(_currentPiece, _currentRotation, _currentPiece.Position + Vector.Down))
        {
            MovePiece(_currentPiece, _currentRotation, Vector.Down);
        }
            
        LockPiece();
    }

    private void DrawHud(IRenderer renderer)
    {
        renderer.DrawString(Vector.Down + Vector.Right * renderer.Width / 2, "TETRIS", alignment: TextAlignment.Centered);
        renderer.DrawString(4 * Vector.Down, $"Score: {_score}");
        renderer.DrawString(6 * Vector.Down, $"Level: {_level}");
        renderer.DrawString(8 * Vector.Down, $"Lines: {_lineCount}");
            
        renderer.DrawString(new Vector(5, 15), $"HOLD");
        if (_heldPiece != null)
        {
            renderer.DrawSprite(_heldPiece, _heldPiecePosition);
        }
            
        renderer.DrawString(new Vector(23, 15), "NEXT");

        for (int i = 0; i < 4; i++)
        {
            renderer.DrawSprite(_randomizerBag[i], new Vector(23, 16 + 4 * i));
        }
    }

    private void CheckForClearedLines()
    {
        for (int y = 0; y < _currentPiece.Bounds.Height; y++)
        {
            bool isLine = true;
            for (int x = 1; x < _field.Bounds.Width - 1; x++)
            {
                var fieldRelativePosition = _currentPiece.Position + new Vector(x, y) - _field.Position;
                isLine &= _field.Sprite[x, (int)fieldRelativePosition.Y] == 'X';
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
                for (int x = 1; x < _field.Sprite.Width - 1; x++)
                {
                    for (int i = line; i > 0; i--)
                    {
                        var above = _field.Sprite[x, i - 1];
                        var aboveColor = _field.Sprite.GetFgColor(x, i - 1);
                        _field.Sprite[x, i] = above;

                        if (above == 'X')
                        {
                            _field.Sprite.SetFgColor(x, i, aboveColor);
                            _field.Sprite.SetBgColor(x, i, aboveColor);
                        }
                        else
                        {
                            _field.Sprite.SetFgColor(x, i, Color24.Black);
                            _field.Sprite.SetBgColor(x, i, Color24.Black);
                        }
                    }
                }
            }

            _clearedLines.Clear();
        }
    }

    private void LockPiece()
    {
        for (int x = 0; x < _currentPiece.Bounds.Width; x++)
        {
            for (int y = 0; y < _currentPiece.Bounds.Height; y++)
            {
                int pieceIndex = GetRotatedIndex(x, y, _currentRotation);
                var fieldRelativePosition = _currentPiece.Position + new Vector(x, y) - _field.Position;

                if (_currentPiece.Sprite[pieceIndex] == 'X')
                {
                    _field.Sprite[fieldRelativePosition] = 'X';
                    _field.Sprite.SetFgColor(fieldRelativePosition, _currentPiece.Sprite.GetFgColor(pieceIndex));
                    _field.Sprite.SetBgColor(fieldRelativePosition, _currentPiece.Sprite.GetFgColor(pieceIndex));
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

    private bool MovePiece(GameEntity piece, int rotation, Vector direction)
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

    private bool DoesPieceFit(GameEntity piece, int rotation, Vector newPosition)
    {
        for (int x = 0; x < piece.Bounds.Width; x++)
        {
            for (int y = 0; y < piece.Bounds.Height; y++)
            {
                int pieceIndex = GetRotatedIndex(x, y, rotation);
                var fieldRelativePosition = newPosition + new Vector(x, y) - _field.Position;

                if (piece.Sprite[pieceIndex] == 'X' && _field.Sprite[fieldRelativePosition] != ' ')
                {
                    return false;
                }
            }
        }
            
        return true;
    }

    private void DrawRotatedPiece(IRenderer renderer, GameEntity piece, int rotation)
    {
        for (var y = 0; y < piece.Bounds.Height; y++)
        {
            for (int x = 0; x < piece.Bounds.Width; x++)
            {
                var index = GetRotatedIndex(x, y, rotation);
                if (piece.Sprite[index] != ' ')
                {
                    renderer.Draw(
                        (int)piece.Position.X + x, 
                        (int)piece.Position.Y + y, 
                        piece.Sprite[index], 
                        piece.Sprite.GetFgColor(x, y),
                        piece.Sprite.GetBgColor(x, y));
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