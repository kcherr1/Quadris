#pragma warning disable 1591
using System;
using System.Collections.Generic;

namespace Quadris {
  public enum CellState {
    EMPTY,
    OCCUPIED_PREVIOUSLY,
    OCCUPIED_ACTIVE_PIECE,
    COLLISION
  }

  public enum MoveDir {
    LEFT,
    RIGHT,
    DOWN,
    UP // Jacoby - Added enum to help hold piece in place with up button
  }

  public class GridCellInfo {
    public PieceColor Color { get; set; }
    public CellState State { get; set; }

    public GridCellInfo() {
      Reset();
    }

    public void Reset() {
      Color = PieceColor.NONE;
      State = CellState.EMPTY;
    }

    public void SetToActivePiece(Piece activePiece) {
      State = CellState.OCCUPIED_ACTIVE_PIECE;
      Color = activePiece.Color;
    }
  }

  public class Board {

   public GridCellInfo[,] Grid { get; private set; }
    public Piece ActivePiece { get; set; }

        // Jacoby: Added variable to check if piece is moveable.
        public List<Piece> Pieces = new List<Piece>();  //List of current and next piece...
        public int Score = 0;                           //Score
        public bool hold_lock = false;                  //Delays swap after one is made 
        public bool Piece_settled;                      //Checks to see if piece is settled
        public bool moveable = true;                    //Variable to control moveability of piece

    public Board() {
      Grid = new GridCellInfo[24, 10];
      for (int i = 0; i < Grid.GetLength(0); i++) {
        for (int j = 0; j < Grid.GetLength(1); j++) {
          Grid[i, j] = new GridCellInfo();
        }
      }
    }

    /// <summary>
    /// This method updates the grid and moves the active piece down
    /// </summary>
    /// Jacoby: Hold piece - checks to see if the piece is moveable before moving, up button changes moveable to false.
    public void Update() {
      if (ActivePieceCanMove(MoveDir.DOWN)) {
        if(moveable)
            ActivePiece.MoveDown();
        RefreshGridWithActivePiece();
      }
      else {
        SettlePiece();
        CheckForLine();
      }
    }

    private void RefreshGridWithActivePiece() {
      for (int r = 0; r < Grid.GetLength(0); r++) {
        for (int c = 0; c < Grid.GetLength(1); c++) {
          GridCellInfo cellInfo = Grid[r, c];
          if (cellInfo.State == CellState.OCCUPIED_ACTIVE_PIECE) {
            cellInfo.Reset();
          }
        }
      }
      for (int r = 0; r < ActivePiece.Layout.GetLength(0); r++) {
        for (int c = 0; c < ActivePiece.Layout.GetLength(1); c++) {
          if (ActivePiece.Layout[r, c]) {
            GridCellInfo cellInfo = GetCellInfo(r + ActivePiece.GridRow, c + ActivePiece.GridCol);
            cellInfo?.SetToActivePiece(ActivePiece);
          }
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="row">the row</param>
    /// <param name="col">the column</param>
    /// <returns>A GridCellInfo object</returns>
    public GridCellInfo GetCellInfo(int row, int col) {
      if (row < 0 || row >= Grid.GetLength(0) || col < 0 || col >= Grid.GetLength(1))
        return null;
      else
        return Grid[row, col];
    }

    public void MoveActivePieceLeft() {
      if (ActivePieceCanMove(MoveDir.LEFT)) {
        ActivePiece.MoveLeft();
        RefreshGridWithActivePiece();
      }
    }

    public void MoveActivePieceRight() {
      if (ActivePieceCanMove(MoveDir.RIGHT)) {
        ActivePiece.MoveRight();
        RefreshGridWithActivePiece();
      }
    }

    public void HoldActivePiece() {
      if (ActivePieceCanMove(MoveDir.RIGHT)) {
        ActivePiece.MoveRight();
        RefreshGridWithActivePiece();
      }
    }

    public void RotateActivePieceRight() {
      ActivePiece.RotateRight();
      if (CheckForOutOfBounds()) {
        ActivePiece.RotateLeft();
      }
    }

    public void RotateActivePieceLeft() {
      ActivePiece.RotateLeft();
      if (CheckForOutOfBounds()) {
        ActivePiece.RotateRight();
      }
    }

    public bool ActivePieceCanMove(MoveDir moveDir) {
      bool canMove = true;
      switch (moveDir) {
        case MoveDir.DOWN:
                    {

                        for (int c = 0; c < ActivePiece.Layout.GetLength(1); c++)
                        {
                            int lastRow = -1;
                            for (int r = 0; r < ActivePiece.Layout.GetLength(0); r++)
                            {
                                if (ActivePiece.Layout[r, c])
                                {
                                    lastRow = r;
                                }
                            }
                            if (lastRow == -1)
                            {
                                continue;
                            }
                            GridCellInfo cellInfo = GetCellInfo(ActivePiece.GridRow + lastRow + 1, ActivePiece.GridCol + c);
                            if (cellInfo == null || cellInfo.State == CellState.OCCUPIED_PREVIOUSLY)
                            {
                                canMove = false;
                                break;
                            }
                        }
                        break;
                    }
          
        case MoveDir.LEFT:
          for (int r = 0; r < ActivePiece.Layout.GetLength(0); r++) {
            int firstCol = -1;
            for (int c = 0; c < ActivePiece.Layout.GetLength(1); c++) {
              if (ActivePiece.Layout[r, c]) {
                firstCol = c;
                break;
              }
            }
            if (firstCol == -1) {
              continue;
            }
            GridCellInfo cellInfo = GetCellInfo(ActivePiece.GridRow + r, ActivePiece.GridCol + firstCol - 1);
            if (cellInfo == null || cellInfo.State == CellState.OCCUPIED_PREVIOUSLY) {
              canMove = false;
              break;
            }
          }
          break;

        case MoveDir.RIGHT:
          for (int r = 0; r < ActivePiece.Layout.GetLength(0); r++) {
            int lastCol = -1;
            for (int c = 0; c < ActivePiece.Layout.GetLength(1); c++) {
              if (ActivePiece.Layout[r, c]) {
                lastCol = c;
              }
            }
            if (lastCol == -1) {
              continue;
            }
            GridCellInfo cellInfo = GetCellInfo(ActivePiece.GridRow + r, ActivePiece.GridCol + lastCol + 1);
            if (cellInfo == null || cellInfo.State == CellState.OCCUPIED_PREVIOUSLY) {
              canMove = false;
              break;
            }
          }
          break;

        case MoveDir.UP:
                    { 
            canMove = false;
            break;
                        }
                    break;
      }
      return canMove;
    }

    private bool CheckForOutOfBounds() {
      for (int r = 0; r < ActivePiece.Layout.GetLength(0); r++) {
        for (int c = 0; c < ActivePiece.Layout.GetLength(1); c++) {
          if (ActivePiece.Layout[r, c]) {
            GridCellInfo cellInfo = GetCellInfo(r + ActivePiece.GridRow, c + ActivePiece.GridCol);
            if (cellInfo == null) {
              return true;
            }
          }
        }
      }
      return false;
    }

    public void SettlePiece() {
      for (int r = 0; r < Grid.GetLength(0); r++) {
        for (int c = 0; c < Grid.GetLength(1); c++) {
          GridCellInfo cellInfo = Grid[r, c];
          if (cellInfo.State == CellState.OCCUPIED_ACTIVE_PIECE) {
            cellInfo.State = CellState.OCCUPIED_PREVIOUSLY;
          }
        }
      } 
            // Piece is settled and active piece is set to the next piece. The next piece is a randomly generated piece.  Any hold lock is released.
            ActivePiece = Pieces[1];
            Pieces[0] = ActivePiece;
            Pieces[1] = Piece.GetRandPiece();
            hold_lock = false;
            Piece_settled = true;
        }

    // Jacoby: Score: 1 Q8: (Garrett Gresham) As a gamer I want to see the number of lines cleared in my game so I can have a tangential score.
    public void CheckForLine() {
      for (int curRow = 0; curRow < Grid.GetLength(0); curRow++) {
        bool allFilled = true;
        for (int col = 0; col < Grid.GetLength(1); col++) {
          if (GetCellInfo(curRow, col)?.State == CellState.EMPTY) {
            allFilled = false;
            break;
          }
        }
        if (allFilled) {
          for (int col = 0; col < Grid.GetLength(1); col++) {
            for (int dropRow = curRow; dropRow > 0; dropRow--) {
              Grid[dropRow, col] = Grid[dropRow - 1, col];
            }
          }
                    // Q8 Increments the score when a line is filled
                    Score++;
                    curRow--;
        }
      }
    }
  }
}
