using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;





public class MyBot : IChessBot
{


    public Move Think(Board board, Timer timer)
    {


        //double currentPos = EvaluatePosition(board);
        //Console.WriteLine(currentPos); //To check the strenght of the bot and to add features that will make him faster & better.
        // For this simple example, just return the first legal move.
        // You should implement your own evaluation and move selection logic here.
        //return moves[0];

        int searchDepth = 5;
        Move[] legalMoves = board.GetLegalMoves();

        
        

        double bestScore = double.MinValue;
        //Start with the first move
        Move bestMove = legalMoves[0];

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);

            double score = -NegaMax(searchDepth - 1, board, double.MinValue, double.MaxValue, false);

            board.UndoMove(move);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;


    }

    //https://www.chessprogramming.org/Negamax
    // This is a shorter, more comman way for mini-max.
    private double NegaMax(int depth, Board board, double alpha, double beta, bool maximizingPlayer)
    {
        if (depth == 0 || board.IsDraw() || board.IsInCheckmate())
        {
            // Base Case or Terminal Node.
            return Evaluate(board);
        }

        double maxScore = double.MinValue;
        double minScore = double.MaxValue;

        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);

            double score = -NegaMax(depth - 1, board, -beta, -alpha, !maximizingPlayer);

            board.UndoMove(move);

            if (score > maxScore)
            {
                maxScore = score;
            }

            alpha = Math.Max(alpha, score);

            if (alpha >= beta)
            {
                // Alpha-beta pruning.
                break;
            }
        }

        return maximizingPlayer ? maxScore : minScore;
    }





    public static int Evaluate(Board board)
    {
        int evaluation = 0;

        // Material evaluation
        evaluation += EvaluateMaterial(board);

        //Perspective to which side is going to play
        int perspective = (board.IsWhiteToMove) ? 1 : -1;

        return evaluation * perspective;
    }

    private static int EvaluateMaterial(Board board)
    {
        int evaluation = 0;

        // Piece values for evaluation
        const int pawnValue = 100;
        const int knightValue = 200;
        const int bishopValue = 300;
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

}

    





    

    





