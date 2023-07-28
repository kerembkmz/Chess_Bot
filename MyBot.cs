using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;





public class MyBot : IChessBot
{
    private int currentPlayedMoves = 0;
    private Dictionary<ulong, double> moveScores = new Dictionary<ulong, double>();

    public Move Think(Board board, Timer timer)
    {
        int searchDepth = 3; 
        Move[] legalMoves = board.GetLegalMoves();
        legalMoves = OrderMoves(legalMoves, board, currentPlayedMoves);
        
        double bestScore = double.MinValue;
        Move bestMove = legalMoves[0];

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);

            double score = -AlphaBeta(searchDepth - 1, board, double.MinValue, double.MaxValue);

            board.UndoMove(move);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        currentPlayedMoves += 2;
        return bestMove;
    }

    private double AlphaBeta(int depth, Board board, double alpha, double beta)
    {
        ulong hash = board.ZobristKey;
        if (moveScores.TryGetValue(hash, out double cachedScore)) 
        {
            return cachedScore;
        }

        if (depth == 0 || board.IsDraw() || board.IsInCheckmate())
        {
            return Evaluate(board);
        }

        Move[] legalMoves = board.GetLegalMoves();
        legalMoves = OrderMoves(legalMoves, board, currentPlayedMoves);

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);

            double score = -AlphaBeta(depth - 1, board, -beta, -alpha);

            board.UndoMove(move);

            if (score >= beta)
            {
                moveScores[hash] = beta;
                return beta;
            }

            if (score > alpha)
            {
                alpha = score;
            }
        }

        moveScores[hash] = alpha;
        return alpha;
    }

    private double EvaluateMove(Move move, Board board, int a)
    {
        int moveScore = 0;
        PieceType movePiece = move.MovePieceType;
        PieceType capturePiece = move.CapturePieceType;
        PieceType promotionPiece = move.PromotionPieceType;
        Square targetSquare = move.TargetSquare;

        int movePieceVal = (int)movePiece;
        int capturePieceVal = (int)capturePiece;
        int promotionPieceVal = (int)promotionPiece;

        // Debug statements to print move information
        Console.WriteLine($"Move: {move}, Move Piece: {movePiece}, Capture Piece: {capturePiece}, Promotion Piece: {promotionPiece}, Target Square: {targetSquare}");

        // Rule 1.
        // If opponent's piece is more valuable than ours, capture.
        if (capturePiece != PieceType.None)
        {
            moveScore += 10 * (capturePieceVal - movePieceVal);
        }

        // Rule 2.
        // Promoting is generally a good idea.
        if (move.IsPromotion)
        {
            moveScore += 5 * promotionPieceVal;
        }

        // Rule 3.
        // If our move's target is attacked by the opponent, then decrease the moveScore.
        if (board.SquareIsAttackedByOpponent(targetSquare))
        {
            moveScore -= (10 * movePieceVal);
        }

        // Rule 4: Encourage controlling the center by rewarding moves to center squares.
        const int centerValue = 20;
        if (IsInCenter(targetSquare))
        {
            moveScore += centerValue;
        }

        // Rule 5.
        // If it is a capture move and our our piece value is less than the opponents pieceval
        if (capturePiece != PieceType.None && movePieceVal <= capturePieceVal)
        {
            moveScore += 50; // Bonus to encourage capturing
        }

        // Rule 6
        // Bonus for pawn, knight, and bishop moves in the early game.
        if (!IsEndgame(a) && (movePiece == PieceType.Pawn || movePiece == PieceType.Rook)){
            moveScore += 50;
        }


        else if (!IsEndgame(a) && (movePiece == PieceType.Knight || movePiece == PieceType.Bishop)) {
            moveScore += 100;
        }

        else if (!IsEndgame(a) && (movePiece == PieceType.Queen))
        {
            moveScore -= 100;
        }

        // Rule 7:
        // Encourage castling to activate rooks and king safety.
        if (!IsEndgame(a) && move.IsCastles)
        {
            moveScore += 60;
        }

        // Debug statement to print move score
        Console.WriteLine($"Move Score: {moveScore}");

        return moveScore;
    }

    public Move[] OrderMoves(Move[] moves, Board board, int a)
    {
        // Order moves based on a scoring system
        Array.Sort(moves, (move1, move2) => EvaluateMove(move2, board, a).CompareTo(EvaluateMove(move1, board, a)));
        return moves;
    }

    private static bool IsEndgame(int a)
    {
        if (a < 15)
            return false;
        else
        {
            return true;
        }


    }

    // Helper method to check if a square is in the center of the board.
    private static bool IsInCenter(Square square)
    {
        int file = square.File;
        int rank = square.Rank;
        return (file >= 2 && file <= 5) && (rank >= 2 && rank <= 5);
    }





    public static int Evaluate(Board board)
    {
        int evaluation = 0;

        // Material evaluation
        evaluation += EvaluateMaterial(board);

        //Threats to bot's pieces.
        evaluation += CalculateThreat(board);

        //Encouragein controlling the center
        evaluation += ControlCenter(board);

        //Perspective to which side is going to play
        int perspective = (board.IsWhiteToMove) ? 1 : -1;

        if (board.IsInCheck())
        {
            // If the bot is in check, encourage it to find a way out.
            evaluation += 150;
        }

        if (board.IsInCheckmate())
        {
            // If the bot is in checkmate, encourage it to avoid such positions in the future.
            evaluation += 10000;
        }

        return evaluation * perspective;
    }

    private static int ControlCenter(Board board)
    {
        int centerValue = 20;

        // Encourage controlling center squares for the pieces
        int centerControlWhite = 0;
        int centerControlBlack = 0;

        // Squares in the center
        Square[] centerSquares = { new Square("d4"), new Square("e4"), new Square("d5"), new Square("e5") };

        // Evaluate control of center squares for white pieces
        foreach (Square square in centerSquares)
        {
            if (board.SquareIsAttackedByOpponent(square))
            {
                centerControlBlack++;
            }
            else if (board.GetPiece(square).IsWhite)
            {
                centerControlWhite++;
            }
        }

        // Evaluate control of center squares for black pieces
        foreach (Square square in centerSquares)
        {
            if (board.SquareIsAttackedByOpponent(square))
            {
                centerControlWhite++;
            }
            else if (!board.GetPiece(square).IsWhite)
            {
                centerControlBlack++;
            }
        }

        // Add the center control evaluation to the overall evaluation
        int evaluation = centerValue * (centerControlWhite - centerControlBlack);

        return evaluation;
    }

    private static int EvaluateMaterial(Board board)
    {
        int evaluation = 0;

        // Piece values for evaluation
        const int pawnValue = 100;
        const int knightValue = 300;
        const int bishopValue = 320;
        const int rookValue = 500;
        const int queenValue = 900;

        evaluation += board.GetPieceList(PieceType.Pawn, true).Count * pawnValue;
        evaluation -= board.GetPieceList(PieceType.Pawn, false).Count * pawnValue;
        evaluation += board.GetPieceList(PieceType.Knight, true).Count * knightValue;
        evaluation -= board.GetPieceList(PieceType.Knight, false).Count * knightValue;
        evaluation += board.GetPieceList(PieceType.Bishop, true).Count * bishopValue;
        evaluation -= board.GetPieceList(PieceType.Bishop, false).Count * bishopValue;
        evaluation += board.GetPieceList(PieceType.Rook, true).Count * rookValue;
        evaluation -= board.GetPieceList(PieceType.Rook, false).Count * rookValue;
        evaluation += board.GetPieceList(PieceType.Queen, true).Count * queenValue;
        evaluation -= board.GetPieceList(PieceType.Queen, false).Count * queenValue;

        

        return evaluation;
    }

    private static int CalculateThreat(Board board)
    {
        int threatScore = 0;
        PieceList[] allPieceLists = board.GetAllPieceLists();

        foreach (PieceList pieceList in allPieceLists)
        {
            foreach (Piece piece in pieceList)
            {
                if (piece.IsWhite == board.IsWhiteToMove)
                {
                    Square pieceSquare = piece.Square;

                    // Check if the current piece is under threat by the opponent
                    bool isUnderThreat = board.SquareIsAttackedByOpponent(pieceSquare);

                    // If the piece is under threat, apply a penalty based on its value
                    if (isUnderThreat)
                    {
                        int pieceValue = (int)piece.PieceType;
                        threatScore -= 10 * pieceValue;
                    }
                }
            }
        }

        return threatScore;
    }






}















