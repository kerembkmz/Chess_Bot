# Chess_Bot

## Features
1. Move Order.
	1. If the piece to capture is more valuable encourage capture.
	2. Promoting is encouraged.
	3. If the move's intented square is attacked, discourage the move.
	4. Encourage controlling the center squares (4 squares)
	5. Encourage captures even if material is lost if center control is increased up to a score.
2. Evaluation.
	1. Material evaluation.
	2. Center control score.
	3. Threat score is added to evaluation. (Bot's pieces that are under attack with their corresponding material scores)
3. Search.
	1. Currently uses NegaMax search with alpha-beta pruning with depth 5. 
	2. Move order is satisfied after every move. 
	3. Inside search, base case/terminal node is used for SearchAllCaptures method.
4. SearchAllCaptures Method.
	1. Mini-max search only for possible captures.

Chess AI for Sebastian Lague's Chess Challange in C#. I will be updating my bot.cs file only, the original github page is : https://github.com/SebLague/Chess-Challenge