﻿
namespace SrcChess2 {
    /// <summary>Basic board evaluation function</summary>
    public class BoardEvaluationBasic : IBoardEvaluation {
        /// <summary>Value of each piece/color.</summary>
        static protected int[] s_piPiecePoint;

        /// <summary>
        /// Static constructor
        /// </summary>
        static BoardEvaluationBasic() {
            s_piPiecePoint = new int[128];  // changed
            s_piPiecePoint[(int)ChessBoard.PieceE.Pawn] = 100;
            s_piPiecePoint[(int)ChessBoard.PieceE.Rook] = 500;
            s_piPiecePoint[(int)ChessBoard.PieceE.Knight] = 300;
            s_piPiecePoint[(int)ChessBoard.PieceE.Bishop] = 325;
            s_piPiecePoint[(int)ChessBoard.PieceE.Queen] = 900;
            s_piPiecePoint[(int)ChessBoard.PieceE.Chancellor] = 850;
            s_piPiecePoint[(int)ChessBoard.PieceE.Archbishop] = 700;
            s_piPiecePoint[(int)ChessBoard.PieceE.EmpoweredQueen] = 400;
            s_piPiecePoint[(int)ChessBoard.PieceE.EmpoweredKnight] = 400;
            s_piPiecePoint[(int)ChessBoard.PieceE.EmpoweredBishop] = 425;
            s_piPiecePoint[(int)ChessBoard.PieceE.EmpoweredRook] = 600;
            s_piPiecePoint[(int)ChessBoard.PieceE.Tiger] = 350;
            s_piPiecePoint[(int)ChessBoard.PieceE.Elephant] = 525;
            s_piPiecePoint[(int)ChessBoard.PieceE.Amazon] = 1150;
            s_piPiecePoint[(int)ChessBoard.PieceE.Ferz] = 150;
            s_piPiecePoint[(int)ChessBoard.PieceE.Wazir] = 200;
            s_piPiecePoint[(int)ChessBoard.PieceE.CrazyHorse] = 150;
            s_piPiecePoint[(int)ChessBoard.PieceE.AmazonPawn] = 150;
            s_piPiecePoint[(int)ChessBoard.PieceE.Gaja] = 550;
            s_piPiecePoint[(int)ChessBoard.PieceE.Zebra] = 180;
            s_piPiecePoint[(int)ChessBoard.PieceE.Camel] = 200;
            s_piPiecePoint[(int)ChessBoard.PieceE.Giraffe] = 500; //???
            s_piPiecePoint[(int)ChessBoard.PieceE.Unicorn] = 550; //???
            s_piPiecePoint[(int)ChessBoard.PieceE.Lion] = 900; //
            s_piPiecePoint[(int)ChessBoard.PieceE.Buffalo] = 800; //???
            s_piPiecePoint[(int)ChessBoard.PieceE.Reaper] = 2000; //???
            s_piPiecePoint[(int)ChessBoard.PieceE.Lancer] = 150; //???
            s_piPiecePoint[(int)ChessBoard.PieceE.ShogiHorse] = 120; //???
            s_piPiecePoint[(int)ChessBoard.PieceE.GoldGeneral] = 300; //???
            s_piPiecePoint[(int)ChessBoard.PieceE.SilverGeneral] = 250; //???
            s_piPiecePoint[(int)ChessBoard.PieceE.Nemesis] = 900; //???
            s_piPiecePoint[(int)ChessBoard.PieceE.Snake] = 400; //???
            s_piPiecePoint[(int)ChessBoard.PieceE.Hipo] = 300; //???

            s_piPiecePoint[(int)ChessBoard.PieceE.King] = 1000000;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Pawn | ChessBoard.PieceE.Black)] = -100;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Rook | ChessBoard.PieceE.Black)] = -500;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Knight | ChessBoard.PieceE.Black)] = -300;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Bishop | ChessBoard.PieceE.Black)] = -325;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Queen | ChessBoard.PieceE.Black)] = -900;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Chancellor | ChessBoard.PieceE.Black)] = -850;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Archbishop | ChessBoard.PieceE.Black)] = -700;
            s_piPiecePoint[(int)(ChessBoard.PieceE.EmpoweredQueen | ChessBoard.PieceE.Black)] = -400;
            s_piPiecePoint[(int)(ChessBoard.PieceE.EmpoweredKnight | ChessBoard.PieceE.Black)] = -400;
            s_piPiecePoint[(int)(ChessBoard.PieceE.EmpoweredBishop | ChessBoard.PieceE.Black)] = -425;
            s_piPiecePoint[(int)(ChessBoard.PieceE.EmpoweredRook | ChessBoard.PieceE.Black)] = -600;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Tiger | ChessBoard.PieceE.Black)] = -350;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Elephant | ChessBoard.PieceE.Black)] = -525;
            s_piPiecePoint[(int)(ChessBoard.PieceE.King | ChessBoard.PieceE.Black)] = -1000000;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Amazon | ChessBoard.PieceE.Black)] = -1150;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Ferz | ChessBoard.PieceE.Black)] = -150;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Wazir | ChessBoard.PieceE.Black)] = -200;
            s_piPiecePoint[(int)(ChessBoard.PieceE.CrazyHorse | ChessBoard.PieceE.Black)] = -150;
            s_piPiecePoint[(int)(ChessBoard.PieceE.AmazonPawn | ChessBoard.PieceE.Black)] = -150;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Gaja | ChessBoard.PieceE.Black)] = -550;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Zebra | ChessBoard.PieceE.Black)] = -180;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Camel | ChessBoard.PieceE.Black)] = -200;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Giraffe | ChessBoard.PieceE.Black)] = -500;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Unicorn | ChessBoard.PieceE.Black)] = -550;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Lion | ChessBoard.PieceE.Black)] = -900;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Buffalo | ChessBoard.PieceE.Black)] = -800;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Reaper | ChessBoard.PieceE.Black)] = -2000;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Lancer | ChessBoard.PieceE.Black)] = -150;
            s_piPiecePoint[(int)(ChessBoard.PieceE.ShogiHorse | ChessBoard.PieceE.Black)] = -120;
            s_piPiecePoint[(int)(ChessBoard.PieceE.GoldGeneral | ChessBoard.PieceE.Black)] = -300;
            s_piPiecePoint[(int)(ChessBoard.PieceE.SilverGeneral | ChessBoard.PieceE.Black)] = -250;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Nemesis | ChessBoard.PieceE.Black)] = -900;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Snake | ChessBoard.PieceE.Black)] = -400;
            s_piPiecePoint[(int)(ChessBoard.PieceE.Hipo | ChessBoard.PieceE.Black)] = -300;

            // notes
            // horse dragon = wazir  bishop 525
            // Dragon = rook  ferz = 650
            
            // todo
            /**
             reset boaed info: castl
             bug epowered moviment i jaque
            bug animas
            shogui adapted
            midline invasion primera iteracio
    ***/
             
        }

        /// <summary>
        /// Name of the evaluation method
        /// </summary>
        public virtual string Name {
            get {
                return ("Basic");
            }
        }

        /// <summary>
        /// Evaluates a board. The number of point is greater than 0 if white is in advantage, less than 0 if black is.
        /// </summary>
        /// <param name="pBoard">           Board.</param>
        /// <param name="piPiecesCount">    Number of each pieces</param>
        /// <param name="posInfo">          Information about pieces position</param>
        /// <param name="iWhiteKingPos">    Position of the white king</param>
        /// <param name="iBlackKingPos">    Position of the black king</param>
        /// <param name="bWhiteCastle">     White has castled</param>
        /// <param name="bBlackCastle">     Black has castled</param>
        /// <param name="iMoveCountDelta">  Number of possible white move - Number of possible black move</param>
        /// <returns>
        /// Points
        /// </returns>
        public virtual int Points(ChessBoard.PieceE[] pBoard,
                                  int[] piPiecesCount,
                                  ChessBoard.PosInfoS posInfo,
                                  int iWhiteKingPos,
                                  int iBlackKingPos,
                                  bool bWhiteCastle,
                                  bool bBlackCastle,
                                  int iMoveCountDelta) {
            int iRetVal = 0;

            for (int iIndex = 0; iIndex < piPiecesCount.Length; iIndex++) {
                iRetVal += s_piPiecePoint[iIndex] * piPiecesCount[iIndex];
            }
            if (pBoard[12] == ChessBoard.PieceE.Pawn) {
                iRetVal -= 4;
            }
            if (pBoard[52] == (ChessBoard.PieceE.Pawn | ChessBoard.PieceE.Black)) {
                iRetVal += 4;
            }
            if (bWhiteCastle) {
                iRetVal += 10;
            }
            if (bBlackCastle) {
                iRetVal -= 10;
            }

            bool whiteKingMidelineInvasion = false;
            bool blackKingMidelineInvasion = false;

            for (int iIndex = 32; iIndex < 64; iIndex++)
            {
                if (pBoard[iIndex] == ChessBoard.PieceE.King)
                {
                    whiteKingMidelineInvasion = true;
                }
            }

            for (int iIndex = 0; iIndex < 32; iIndex++)
            {
                if (pBoard[iIndex] == ChessBoard.PieceE.King)
                {
                    blackKingMidelineInvasion = true;
                }
            }

            for (int iIndex = 32; iIndex < 64; iIndex++)
            {
                if (pBoard[iIndex] == ChessBoard.PieceE.King && !blackKingMidelineInvasion)
                {
                    iRetVal += 100000000;
                }
            }


           

            for (int iIndex = 0; iIndex < 32; iIndex++)
            {
                if (pBoard[iIndex] == (ChessBoard.PieceE.King | ChessBoard.PieceE.Black) && !whiteKingMidelineInvasion)
                {
                    iRetVal -= 100000000; 
                }
            }

            if (iWhiteKingPos / 8 == 1)
                iRetVal += 10;
            if (iWhiteKingPos / 8 == 2)
                iRetVal += 20;
            if (iWhiteKingPos / 8 == 3)
                iRetVal += 30;
            if (iBlackKingPos / 8 == 4)
                iRetVal -= 30;
            if (iBlackKingPos / 8 == 5)
                iRetVal -= 20;
            if (iBlackKingPos / 8 == 6)
                iRetVal -= 10;

            int bishopCount = 0;
            for (int iIndex = 0; iIndex < 64; iIndex++)
            {


                if (pBoard[iIndex] == ChessBoard.PieceE.Bishop)
                    bishopCount++;
                iRetVal += getPositionalValueForThisSquareWhite(pBoard, iIndex);
                iRetVal -= getPositionalValueForThisSquareBlack(pBoard, iIndex);


            }
            if (bishopCount == 2)
                iRetVal += 50;


            iRetVal += iMoveCountDelta;
            iRetVal += posInfo.m_iAttackedPieces * 2;
            //iRetVal += posInfo.m_iAttackedPos + posInfo.m_iAttackedPieces * 2 + posInfo.m_iPiecesDefending * 2;
            return (iRetVal);
        }


        public int getPositionalValueForThisSquareBlack(ChessBoard.PieceE[] pBoard, int i)
        {


            if (pBoard[i] == (ChessBoard.PieceE.Queen | ChessBoard.PieceE.Black))
                return queen[i];
            if (pBoard[i] == (ChessBoard.PieceE.Bishop | ChessBoard.PieceE.Black))
                return bishop[i];
            if (pBoard[i] == (ChessBoard.PieceE.Knight | ChessBoard.PieceE.Black))
                return knight[i];
            if (pBoard[i] == (ChessBoard.PieceE.Rook | ChessBoard.PieceE.Black))
                return rook[i];
            if (pBoard[i] == (ChessBoard.PieceE.Pawn | ChessBoard.PieceE.Black))
                return pawnMiddline[i];
            return 0;

        }

        public int getPositionalValueForThisSquareWhite(ChessBoard.PieceE[] pBoard, int i)
        {


            if (pBoard[i] == ChessBoard.PieceE.Queen)
                return queen[i];
            if (pBoard[i] == ChessBoard.PieceE.Bishop || pBoard[i] == ChessBoard.PieceE.Tiger)
                return bishop[i];
            if (pBoard[i] == ChessBoard.PieceE.Knight)
                return knight[i];
            if (pBoard[i] == ChessBoard.PieceE.Rook)
                return rook[i];
            if (pBoard[i] == ChessBoard.PieceE.Pawn)
                return pawnMiddline[i];
            return 0;

        }




       

        int[] queen = { // also archbishop and chancellor
        -20,-10,-10, -5, -5,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5,  5,  5,  5,  0,-10,
         -5,  0,  5,  5,  5,  5,  0, -5,
          0,  0,  5,  5,  5,  5,  0, -5,
        -10,  5,  5,  5,  5,  5,  0,-10,
        -10,  0,  5,  0,  0,  0,  0,-10,
        -20,-10,-10, -5, -5,-10,-10,-20
        };

        int[] king = { // 
  -30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-20,-30,-30,-40,-40,-30,-30,-20,
-10,-20,-20,-20,-20,-20,-20,-10,
 20, 20,  0,  0,  0,  0, 20, 20,
 20, 30, 10,  0,  0, 10, 30, 20
};

       
         int[] knight = { // 
-50,-40,-30,-30,-30,-30,-40,-50,
-40,-20,  0,  0,  0,  0,-20,-40,
-30,  0, 10, 15, 15, 10,  0,-30,
-30,  5, 15, 20, 20, 15,  5,-30,
-30,  0, 15, 20, 20, 15,  0,-30,
-30,  5, 10, 15, 15, 10,  5,-30,
-40,-20,  0,  5,  5,  0,-20,-40,
-50,-40,-30,-30,-30,-30,-40,-50,
};

         int[] bishop = { // also raja, tiger , elephant?, eQueen?, ferz
-20,-10,-10,-10,-10,-10,-10,-20,
-10,  0,  0,  0,  0,  0,  0,-10,
-10,  0,  5, 10, 10,  5,  0,-10,
-10,  5,  5, 10, 10,  5,  5,-10,
-10,  0, 10, 10, 10, 10,  0,-10,
-10, 10, 10, 10, 10, 10, 10,-10,
-10,  5,  0,  0,  0,  0,  5,-10,
-20,-10,-10,-10,-10,-10,-10,-20,
};

        int[] pawn = { // 
 0,  0,  0,  0,  0,  0,  0,  0,
50, 50, 50, 50, 50, 50, 50, 50,
10, 10, 20, 30, 30, 20, 10, 10,
 5,  5, 10, 25, 25, 10,  5,  5,
 0,  0,  0, 20, 20,  0,  0,  0,
 5, -5,-10,  0,  0,-10, -5,  5,
 5, 10, 10,-20,-20, 10, 10,  5,
 0,  0,  0,  0,  0,  0,  0,  0
};


        int[] pawnMiddline = { // 
 0,  0,  0,  0,  0,  0,  0,  0,
50, 50, 50, 50, 50, 50, 50, 50,
10, 10, 20, 30, 30, 20, 10, 10,
 5,  5, 10, 25, 25, 10,  5,  5,
 0,  0,  0, 20, 20,  0,  0,  0,
 5, 20, 20,  0,  0, 20, 20,  5,
 5, 10, 10,-20,-20, 10, 10,  5,
 0,  0,  0,  0,  0,  0,  0,  0
};

         int[] rook = { // 
  0,  0,  0,  0,  0,  0,  0,  0,
  5, 10, 10, 10, 10, 10, 10,  5,
 -5,  0,  0,  0,  0,  0,  0, -5,
 -5,  0,  0,  0,  0,  0,  0, -5,
 -5,  0,  0,  0,  0,  0,  0, -5,
 -5,  0,  0,  0,  0,  0,  0, -5,
 -5,  0,  0,  0,  0,  0,  0, -5,
  0,  0,  0,  5,  5,  0,  0,  0
};

        int[] kingMiddline = { // 
  -30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-40,
 30, 30, 30, 30, 30, 30, 30, 30,
 20, 20, 20, 20, 20, 20, 20, 10,
 10, 10, 10, 10, 10, 10, 10, 10,
  0,  0,  0,  0,  0,  0,  0,  0
};

        int[] elephant = { // 
   0,  0,  0,  0,  0,  0,  0,  0,
   5, 10, 10, 10, 10, 10, 10,  5,
  -5,  0,  5,  5,  5,  5,  0, -5,
  -5,  0,  5, 10, 10,  5,  0, -5,
  -5,  0,  5, 10, 10,  5,  0, -5,
  -5,  0,  5,  5,  5,  5,  0, -5,
  -5,  0,  0,  0,  0,  0,  0, -5,
 -10,-10,-10,-10,-10,-10,-10,-10
};





    } // Class BoardEvaluationBasic
} // Namespace
