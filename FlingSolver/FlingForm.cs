using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace FlingSolver
{
    public partial class FlingForm : Form
    {
        public FlingForm()
        {
            InitializeComponent();

            _boardTile = new PictureBox[8, 7];

            int tileSize = 50;

            int py = 50;
            int ox = 10;

            int px = ox;

            for (int y = 0; y < _boardTile.GetLength(0); ++y)
            {
                for (int x = 0; x < _boardTile.GetLength(1); ++x)
                {
                    _boardTile[y, x] = new PictureBox();
                    _boardTile[y, x].Location = new Point(px, py);
                    px += tileSize + 2;
                }

                px = ox;
                py += tileSize + 2;
            }

            foreach (var tile in _boardTile)
            {
                tile.Width = tile.Height = tileSize;
                tile.BorderStyle = BorderStyle.FixedSingle;
                tile.BackColor = _emptyTileColor;
                tile.BackgroundImageLayout = ImageLayout.Center;
                tile.Click += new EventHandler(OnTileClick);
                Controls.Add(tile);
            }
        }

        void OnTileClick(object sender, EventArgs e)
        {
            PictureBox tile = sender as PictureBox;
            if (tile == null)
                return;

            if (tile.BackColor == _occupiedTileColor)
                tile.BackColor = _emptyTileColor;
            else
                tile.BackColor = _occupiedTileColor;
        }

        ulong GetBoard()
        {
            ulong board = 0;

            for (int y = 0; y < Board.Height; ++y)
            {
                for (int x = 0; x < Board.Width; ++x)
                {
                    Board.Set(ref board, x, y, _boardTile[y, x].BackColor == _occupiedTileColor);
                }
            }

            return board;
        }

        void OnSolveClick(object sender, EventArgs e)
        {
            ulong board = GetBoard();

            var moves = FlingSolver.Solve(board);

            if (moves == null)
            {
                MessageBox.Show("Unsolvable.", "FlingSolver", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            Thread solutionThread = new Thread(new ParameterizedThreadStart(ShowSolution));
            solutionThread.IsBackground = true;
            solutionThread.Start(Tuple.Create(board, moves));
        }

        Image ImageFromDirection(Direction d)
        {
            switch (d)
            {
                case Direction.Up:
                    return Resources.Up;

                case Direction.Right:
                    return Resources.Right;

                case Direction.Down:
                    return Resources.Down;

                case Direction.Left:
                    return Resources.Left;
            }

            return null;
        }

        void ShowSolution(object odata)
        {
            var data = odata as Tuple<ulong, List<Move>>;
            if (data == null)
                return;
            
            ulong board = data.Item1;
            List<Move> moves = data.Item2;

            foreach (var move in moves)
            {
                Invoke(new Action(() =>
                {
                    _boardTile[move.UnitLocation.Y, move.UnitLocation.X].BackColor = _activeTileColor;
                    _boardTile[move.UnitLocation.Y, move.UnitLocation.X].BackgroundImage = ImageFromDirection(move.Direction);
                }));

                Thread.Sleep(1000);
                
                board = FlingSolver.MakeMove(board, move);

                Invoke(new Action(() =>
                {
                    for (int y = 0; y < _boardTile.GetLength(0); ++y)
                    {
                        for (int x = 0; x < _boardTile.GetLength(1); ++x)
                        {
                            _boardTile[y, x].BackgroundImage = null;
                            _boardTile[y, x].BackColor = Board.IsSet(board, x, y) ? _occupiedTileColor : _emptyTileColor;
                        }
                    }
                }));

                Thread.Sleep(500);
            }

            Invoke(new Action(() =>
            {
                for (int y = 0; y < _boardTile.GetLength(0); ++y)
                {
                    for (int x = 0; x < _boardTile.GetLength(1); ++x)
                    {
                        _boardTile[y, x].BackgroundImage = null;
                        _boardTile[y, x].BackColor = _emptyTileColor;
                    }
                }
            }));
        }

        readonly Color _emptyTileColor = Color.FromArgb(250, 250, 250);
        readonly Color _occupiedTileColor = Color.RoyalBlue;
        readonly Color _activeTileColor = Color.Yellow;

        PictureBox[,] _boardTile;
    }

    class FlingSolver
    {
        static public List<Move> Solve(ulong board)
        {
            List<Move> moves = new List<Move>();
            if (Solve(board, moves))
            {
                moves.Reverse();
                return moves;
            }

            return null;
        }

        static bool Solve(ulong board, List<Move> moves)
        {
            if (IsSolved(board))
                return true;

            var possibleMoves = GetValidMoves(board);
            if (!possibleMoves.Any())
                return false;

            foreach (var possibleMove in possibleMoves)
            {
                ulong newBoard = MakeMove(board, possibleMove);

                if (Solve(newBoard, moves))
                {
                    moves.Add(possibleMove);
                    return true;
                }
            }

            return false;
        }

        // Assumes at least one unit on the board.
        static bool IsSolved(ulong board)
        {
            return (board & (board - 1)) == 0;
        }

        static IEnumerable<Move> GetValidMoves(ulong board)
        {
            for (int x = 0; x < Board.Width; ++x)
            {
                ulong ymask = 1UL << x;
                for (int y = 0; y < Board.Height; ++y)
                {
                    ulong ynmask = ymask << Board.Width;

                    if (((board & ymask) != 0) && (board & ynmask) == 0)
                    {
                        ulong ysmask = ynmask << Board.Width;
                        for (int yscan = y + 2; yscan < Board.Height; ++yscan, ysmask <<= Board.Width)
                        {
                            if ((board & ysmask) != 0)
                            {
                                yield return new Move
                                {
                                    UnitLocation = Location.Create(x, y),
                                    Direction = Direction.Down
                                };

                                yield return new Move
                                {
                                    UnitLocation = Location.Create(x, yscan),
                                    Direction = Direction.Up
                                };

                                y = yscan;
                                ynmask = ysmask;
                                break;
                            }
                        }
                    }

                    ymask = ynmask;
                }
            }

            for (int y = 0; y < Board.Height; ++y)
            {
                ulong xmask = 1UL << (y * Board.Width);
                for (int x = 0; x < Board.Width; ++x)
                {
                    ulong xnmask = xmask << 1;

                    if (((board & xmask) != 0) && (board & xnmask) == 0)
                    {
                        ulong xsmask = xnmask << 1;
                        for (int xscan = x + 2; xscan < Board.Width; ++xscan, xsmask <<= 1)
                        {
                            if ((board & xsmask) != 0)
                            {
                                yield return new Move
                                {
                                    UnitLocation = Location.Create(x, y),
                                    Direction = Direction.Right
                                };

                                yield return new Move
                                {
                                    UnitLocation = Location.Create(xscan, y),
                                    Direction = Direction.Left
                                };

                                x = xscan;
                                xnmask = xsmask;
                                break;
                            }
                        }
                    }

                    xmask = xnmask;
                }
            }
        }

        public static ulong MakeMove(ulong board, Move move)
        {
            ulong newBoard = board;

            var currLoc = move.UnitLocation;

            Board.Set(ref newBoard, currLoc.X, currLoc.Y, false);

            switch (move.Direction)
            {
                case Direction.Up:
                    for (int y = currLoc.Y - 1; y >= 0; --y)
                    {
                        if (Board.IsSet(board, currLoc.X, y))
                        {
                            Board.Set(ref newBoard, currLoc.X, y + 1, true);
                            return MakeMove(newBoard, new Move
                            {
                                UnitLocation = Location.Create(currLoc.X, y),
                                Direction = Direction.Up
                            });
                        }
                    }

                    break;

                case Direction.Down:
                    for (int y = currLoc.Y + 1; y <= Board.Height - 1; ++y)
                    {
                        if (Board.IsSet(board, currLoc.X, y))
                        {
                            Board.Set(ref newBoard, currLoc.X, y - 1, true);
                            return MakeMove(newBoard, new Move
                            {
                                UnitLocation = Location.Create(currLoc.X, y),
                                Direction = Direction.Down
                            });
                        }
                    }

                    break;
                
                case Direction.Left:
                    for (int x = currLoc.X - 1; x >= 0; --x)
                    {
                        if (Board.IsSet(board, x, currLoc.Y))
                        {
                            Board.Set(ref newBoard, x + 1, currLoc.Y, true);
                            return MakeMove(newBoard, new Move
                            {
                                UnitLocation = Location.Create(x, currLoc.Y),
                                Direction = Direction.Left
                            });
                        }
                    }

                    break;

                case Direction.Right:
                    for (int x = currLoc.X + 1; x <= Board.Width - 1; ++x)
                    {
                        if (Board.IsSet(board, x, currLoc.Y))
                        {
                            Board.Set(ref newBoard, x - 1, currLoc.Y, true);
                            return MakeMove(newBoard, new Move
                            {
                                UnitLocation = Location.Create(x, currLoc.Y),
                                Direction = Direction.Right
                            });
                        }
                    }

                    break;
            }

            return newBoard;
        }
    }

    static class Board // 7x8
    {
        public static bool IsSet(ulong board, int x, int y)
        {
            return (board & Bitmasks[y, x]) != 0;
        }

        public static void Set(ref ulong board, int x, int y, bool set)
        {
            if (set)
                board |= Bitmasks[y, x];
            else
                board &= ~Bitmasks[y, x];
        }

        public const int Width = 7;
        public const int Height = 8;

        static ulong GetBitmask(int x, int y)
        {
            return 1UL << (y * Width + x);
        }

        static ulong[,] Bitmasks;
        static Board()
        {
            Bitmasks = new ulong[Height, Width];
            for (int x = 0; x < Width; ++x)
                for (int y = 0; y < Height; ++y)
                    Bitmasks[y, x] = GetBitmask(x, y);
        }
    }

    class Move
    {
        public Location UnitLocation;
        public Direction Direction;
    }

    struct Location : IEquatable<Location>
    {
        public static Location Create(int x, int y)
        {
            return new Location { X = x, Y = y };
        }

        public bool Equals(Location other)
        {
            return other.X == X
                && other.Y == Y;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Location loc = (Location)obj;
            return X == loc.X
                && Y == loc.Y;
        }

        public override int GetHashCode()
        {
            return (X << 16) | Y;
        }

        public static bool operator==(Location lhs, Location rhs)
        {
            if (object.ReferenceEquals(lhs, null) != object.ReferenceEquals(rhs, null))
                return true;

            return object.ReferenceEquals(lhs, null)
                || lhs.Equals(rhs);
        }

        public static bool operator !=(Location lhs, Location rhs)
        {
            return !(lhs == rhs);
        }

        public int X;
        public int Y;
    }

    enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}
