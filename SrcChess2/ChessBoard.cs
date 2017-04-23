using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SrcChess2 {
    /// <summary>Implementation of the chess board without any user interface.</summary>
    public sealed class ChessBoard : IXmlSerializable {

        /// <summary>Player color (black and white)</summary>
        public enum PlayerColorE {
            /// <summary>White player</summary>
            White = 0,
            /// <summary>Black player</summary>
            Black = 1
        }

        /// <summary>Same as PieceE, but easier serialization.</summary>
        public enum SerPieceE : byte {
            /// <summary>No piece</summary>
            Empty = 0,
            /// <summary>Pawn</summary>
            WhitePawn = 1,
            /// <summary>Knight</summary>
            WhiteKnight = 2,
            /// <summary>Bishop</summary>
            WhiteBishop = 3,
            /// <summary>Rook</summary>
            WhiteRook = 4,
            /// <summary>Queen</summary>
            WhiteQueen = 5,
            /// <summary>King</summary>
            WhiteKing = 6,
            /// <summary>Not used</summary>
            NotUsed1 = 7,
            /// <summary>Not used</summary>
            NotUsed2 = 8,
            /// <summary>Pawn</summary>
            BlackPawn = 17,
            /// <summary>Knight</summary>
            BlackKnight = 18,
            /// <summary>Bishop</summary>
            BlackBishop = 19,
            /// <summary>Rook</summary>
            BlackRook = 20,
            /// <summary>Queen</summary>
            BlackQueen = 21,
            /// <summary>King</summary>
            BlackKing = 22,
            /// <summary>Not used</summary>
            NotUsed3 = 23,
        }

        /// <summary>Value of each piece on the board. Each piece is a combination of piece value and color (0 for white, 8 for black)</summary>
        [Flags]
        public enum PieceE : byte {
            /// <summary>No piece</summary>
            None = 0,
            /// <summary>Pawn</summary>
            Pawn = 1,
            /// <summary>Knight</summary>
            Knight = 2,
            /// <summary>Bishop</summary>
            Bishop = 3,
            /// <summary>Rook</summary>
            Rook = 4,
            /// <summary>Queen</summary>
            Queen = 5,
            /// <summary>King</summary>
            King = 6,
            /// <summary>Mask to find the piece</summary>
            PieceMask = 63,
            /// <summary>Piece is black</summary>
            Black = 64,
            /// <summary>White piece</summary>
            White = 0,

            // empowered queen
            EmpoweredQueen = 9,

            Chancellor = 10,

            Archbishop = 11,
            // Animals
            Tiger = 12,

            Elephant = 13,

            WildHorse = 14,

            //Empowered

            EmpoweredKnight = 15,
            EmpoweredBishop = 16,
            EmpoweredRook = 17,

            //  Amazons
            Amazon = 18,

            Ferz = 19,

            Wazir = 20,

            CrazyHorse = 21,

            AmazonPawn = 22,

            // Chaturanga

            Gaja = 23,

            // jungle

            Lion = 24,

            Zebra = 25,

            Camel = 26,

            Giraffe = 27,

            Unicorn = 28,

            Buffalo = 29,

            // nemesis
            Nemesis = 30,
            // Reaper
            Reaper = 31,

            Ghost = 32,
            // Shogi
            Lancer = 33,
            ShogiHorse = 34,
            GoldGeneral = 35,
            SilverGeneral = 36,

            Snake = 37,
            Hipo = 38,

            Raja = 39,

            // shogi
            Dragon = 40,
            DragonHorse = 41,

            NemesisPawn = 42,

            // 

            DimensionalBishop = 43,
            DimensionalRook = 44,
            DimensionalKnight = 45,




        }

        /// <summary>List of valid pawn promotion</summary>
        [Flags]
        public enum ValidPawnPromotionE {
            /// <summary>No valid promotion</summary>
            None = 0,
            /// <summary>Promotion to queen</summary>
            Queen = 1,
            /// <summary>Promotion to rook</summary>
            Rook = 2,
            /// <summary>Promotion to bishop</summary>
            Bishop = 4,
            /// <summary>Promotion to knight</summary>
            Knight = 8,
            /// <summary>Promotion to pawn</summary>
            Pawn = 16,
             /// <summary>Promotion to chancellor</summary>
            Chancellor = 32
        };

        /// <summary>Mask for board extra info</summary>
        [Flags]
        public enum BoardStateMaskE {
            /// <summary>0-63 to express the EnPassant possible position</summary>
            EnPassant = 63,
            /// <summary>black player is next to move</summary>
            BlackToMove = 64,
            /// <summary>white left castling is possible</summary>
            WLCastling = 128,
            /// <summary>white right castling is possible</summary>
            WRCastling = 256,
            /// <summary>black left castling is possible</summary>
            BLCastling = 512,
            /// <summary>black right castling is possible</summary>
            BRCastling = 1024,
            /// <summary>Mask use to save the number of times the board has been repeated</summary>
            BoardRepMask = 2048 + 4096 + 8192
        };

        /// <summary>Any repetition causing a draw?</summary>
        public enum RepeatResultE {
            /// <summary>No repetition found</summary>
            NoRepeat,
            /// <summary>3 times the same board</summary>
            ThreeFoldRepeat,
            /// <summary>50 times without moving a pawn or eating a piece</summary>
            FiftyRuleRepeat
        };

        /// <summary>Result of a move</summary>
        public enum MoveResultE {
            /// <summary>No repetition found</summary>
            NoRepeat,
            /// <summary>3 times the same board</summary>
            ThreeFoldRepeat,
            /// <summary>50 times without moving a pawn or eating a piece</summary>
            FiftyRuleRepeat,
            /// <summary>No more move for the next player</summary>
            TieNoMove,
            /// <summary>Not enough pieces to do a check mate</summary>
            TieNoMatePossible,
            /// <summary>Check</summary>
            Check,                      // 
            /// <summary>Checkmate</summary>
            Mate,
            /// <summary>Midline invasion</summary>
            Invasion,
            /// <summary>King of Hill</summary>
            KingOfHill
        }

        /// <summary>Type of possible move</summary>
        public enum MoveTypeE : byte {
            /// <summary>Normal move</summary>
            Normal = 0,
            /// <summary>Pawn which is promoted to a queen</summary>
            PawnPromotionToQueen = 1,
            /// <summary>Castling</summary>
            Castle = 2,
            /// <summary>Prise en passant</summary>
            EnPassant = 3,
            /// <summary>Pawn which is promoted to a rook</summary>
            PawnPromotionToRook = 4,
            /// <summary>Pawn which is promoted to a bishop</summary>
            PawnPromotionToBishop = 5,
            /// <summary>Pawn which is promoted to a knight</summary>
            PawnPromotionToKnight = 6,
            /// <summary>Pawn which is promoted to a pawn</summary>
            PawnPromotionToPawn = 7,
            /// <summary>Piece type mask</summary>
            PawnPromotionToChancellor = 8,
            /// <summary>Piece type mask</summary>
            MoveTypeMask = 15,
            /// <summary>The move eat a piece</summary>
            PieceEaten = 16,
            /// <summary>Move coming from book opening</summary>
            MoveFromBook = 32
        }

        /// <summary>Move description</summary>
        public struct MovePosS {
            /// <summary>Original piece if a piece has been eaten</summary>
            public PieceE OriginalPiece;
            /// <summary>Start position of the move (0-63)</summary>
            public byte StartPos;
            /// <summary>End position of the move (0-63)</summary>
            public byte EndPos;
            /// <summary>Type of move</summary>
            public MoveTypeE Type;
        }

        /// <summary>
        /// Position information. Positive value for white player, negative value for black player.
        /// All these informations are computed before the last move to improve performance.
        /// </summary>
        public struct PosInfoS {
            /// <summary>
            /// Class Ctor
            /// </summary>
            /// <param name="iAttackedPieces">  Number of pieces attacking this position</param>
            /// <param name="iPiecesDefending"> Number of pieces defending this position</param>
            public PosInfoS(int iAttackedPieces, int iPiecesDefending) { m_iAttackedPieces = iAttackedPieces; m_iPiecesDefending = iPiecesDefending; }
            /// <summary>Number of pieces being attacked by player's pieces</summary>
            public int m_iAttackedPieces;
            /// <summary>Number of pieces defending player's pieces</summary>
            public int m_iPiecesDefending;
        }

        /// <summary>NULL position info</summary>
        public static readonly ChessBoard.PosInfoS s_posInfoNull = new ChessBoard.PosInfoS(0, 0);
        /// <summary>Possible diagonal or linear moves for each board position</summary>
        static private int[][][] s_pppiCaseMoveDiagLine;
        /// <summary>Possible diagonal moves for each board position</summary>
        static private int[][][] s_pppiCaseMoveDiagonal;
        /// <summary>Possible linear moves for each board position</summary>
        static private int[][][] s_pppiCaseMoveLine;
        /// <summary>Possible knight moves for each board position</summary>
        static private int[][] s_ppiCaseMoveKnight;
        /// <summary>Possible king moves for each board position</summary>
        static private int[][] s_ppiCaseMoveKing;
        /// <summary>Possible board positions a black pawn can attack from each board position</summary>
        static private int[][] s_ppiCaseBlackPawnCanAttackFrom;
        /// <summary>Possible board positions a white pawn can attack from each board position</summary>
        static private int[][] s_ppiCaseWhitePawnCanAttackFrom;

        static private int[][][] s_pppiCaseMoveDiagKnight;

        static private int[][][] s_pppiCaseMoveLineKnight;

        static private int[][][] s_pppiCaseMoveTiger;

        static private int[][][] s_pppiCaseMoveElephant;
        static private int[][][] s_pppiCaseMoveDiagLineKnight;

        static private int[][] s_ppiCaseMoveFerz;

        static private int[][] s_ppiCaseMoveWazir;

        static private int[][] s_ppiCaseMoveCrazyHorse;

        static private int[][] s_ppiCaseWhiteAmazonPawnCanAttackFrom;
        static private int[][] s_ppiCaseBlackAmazonPawnCanAttackFrom;

        static private int[][] s_ppiCaseMoveWhiteGaja;
        static private int[][] s_ppiCaseMoveBlackGaja;

        /// <summary>Possible knight moves for each board position</summary>
        static private int[][] s_ppiCaseMoveLion;

        static private int[][] s_ppiCaseMoveZebra;

        static private int[][] s_ppiCaseMoveCamel;

        static private int[][] s_ppiCaseMoveGiraffe;

        static private int[][][] s_pppiCaseMoveUnicorn;

        static private int[][] s_ppiCaseMoveBuffalo;

        static private int[][][] s_pppiCaseMoveNemesis;

        static private int[][] s_ppiCaseMoveBlackReaper;
        static private int[][] s_ppiCaseMoveWhiteReaper;

        static private int[][] s_ppiCaseMoveGhost;


        // Shoghi 
        static private int[][] s_ppiCaseMoveWhiteGoldGeneral;
        static private int[][] s_ppiCaseMoveBlackGoldGeneral;

        static private int[][] s_ppiCaseMoveBlackShogiHorse;
        static private int[][] s_ppiCaseMoveWhiteShogiHorse;

        static private int[][] s_ppiCaseMoveWhiteSilverGeneral;
        static private int[][] s_ppiCaseMoveBlackSilverGeneral;

        static private int[][][] s_pppiCaseMoveWhiteLancer;
        static private int[][][] s_pppiCaseMoveBlackLancer;

        static private int[][][] s_pppiCaseMoveSnake;
        static private int[][][] s_pppiCaseMoveHipo;

        static private int[][] s_ppiCaseMoveRaja; // xaturanga king

        static private int[][][] s_pppiCaseMoveDragon;
        static private int[][][] s_pppiCaseMoveDragonHorse;

        static private int[][] s_ppiCaseWhiteNemesisPawnCanAttackFrom;
        static private int[][] s_ppiCaseBlackNemesisPawnCanAttackFrom;

        static private int[][][] s_pppiCaseMoveDiagonalDim;
     
        static private int[][][] s_pppiCaseMoveLineDim;
   
        static private int[][] s_ppiCaseMoveKnightDim;

        /// <summary>Chess board</summary>
        /// 63 62 61 60 59 58 57 56
        /// 55 54 53 52 51 50 49 48
        /// 47 46 45 44 43 42 41 40
        /// 39 38 37 36 35 34 33 32
        /// 31 30 29 28 27 26 25 24
        /// 23 22 21 20 19 18 17 16
        /// 15 14 13 12 11 10 9  8
        /// 7  6  5  4  3  2  1  0
        public PieceE[] m_pBoard;
        /// <summary>Position of the black king</summary>
        private int m_iBlackKingPos;
        /// <summary>Position of the white king</summary>
        private int m_iWhiteKingPos;
        /// <summary>Number of pieces of each kind/color</summary>
        private int[] m_piPiecesCount;
        /// <summary>Random number generator</summary>
        private Random m_rnd;
        /// <summary>Random number generator (repetitive, seed = 0)</summary>
        private Random m_rndRep;
        /// <summary>Number of time the right black rook has been moved. Used to determine if castle is possible</summary>
        private int m_iRBlackRookMoveCount;
        /// <summary>Number of time the left black rook has been moved. Used to determine if castle is possible</summary>
        private int m_iLBlackRookMoveCount;
        /// <summary>Number of time the black king has been moved. Used to determine if castle is possible</summary>
        private int m_iBlackKingMoveCount;
        /// <summary>Number of time the right white rook has been moved. Used to determine if castle is possible</summary>
        private int m_iRWhiteRookMoveCount;
        /// <summary>Number of time the left white rook has been moved. Used to determine if castle is possible</summary>
        private int m_iLWhiteRookMoveCount;
        /// <summary>Number of time the white king has been moved. Used to determine if castle is possible</summary>
        private int m_iWhiteKingMoveCount;
        /// <summary>White has castle if true</summary>
        private bool m_bWhiteCastle;
        /// <summary>Black has castle if true</summary>
        private bool m_bBlackCastle;
        /// <summary>Position behind the pawn which had just been moved from 2 positions</summary>
        private int m_iPossibleEnPassantAt;
        /// <summary>Stack of m_iPossibleEnPassantAt values</summary>
        private Stack<int> m_stackPossibleEnPassantAt;
        /// <summary>Opening book</summary>
        private Book m_book;
        /// <summary>Current zobrist key value. Probably unique for the current board position</summary>
        private Int64 m_i64ZobristKey;
        /// <summary>Object where to redirect the trace if any</summary>
        private SearchEngine.ITrace m_trace;
        /// <summary>Move history use to handle the fifty-move rule and the threefold repetition rule.</summary>
        private MoveHistory m_moveHistory;
        /// <summary>The board is in design mode if true</summary>
        private bool m_bDesignMode;
        /// <summary>Stack of moves since the initial board</summary>
        private MovePosStack m_moveStack;
        /// <summary>Color of the next move</summary>
        private PlayerColorE m_eNextMoveColor;
        /// <summary>true if the initial board is the standard one</summary>
        private bool m_bStdInitialBoard;
        /// <summary>Search engine using Alpha-Beta pruning</summary>
        private SearchEngineAlphaBeta m_searchEngineAlphaBeta;
        /// <summary>Search engine using Min-Max pruning</summary>
        private SearchEngineMinMax m_searchEngineMinMax;
        /// <summary>Information about pieces attack</summary>
        private PosInfoS m_posInfo;

        public static int m_dificultLevel;

        public static int m_victoryConditon;

        public static bool m_whiteCanCastle;
        public static bool m_blackCanCastle;

        // 
        public static double m_acumulat=0.0;
        public static double m_iteracions=0.0;

        /// <summary>
        /// Class static constructor. 
        /// Builds the list of possible moves for each piece type per position.
        /// Etablished the value of each type of piece for board evaluation.
        /// </summary>
        static ChessBoard() {
            List<int[]> arrMove;

            s_posInfoNull.m_iAttackedPieces = 0;
            s_posInfoNull.m_iPiecesDefending = 0;
            arrMove = new List<int[]>(4);
            s_pppiCaseMoveDiagLine = new int[64][][];
            s_pppiCaseMoveDiagonal = new int[64][][];
            s_pppiCaseMoveLine = new int[64][][];
            s_ppiCaseMoveKnight = new int[64][];
            s_ppiCaseMoveKing = new int[64][];
            s_ppiCaseWhitePawnCanAttackFrom = new int[64][];
            s_ppiCaseBlackPawnCanAttackFrom = new int[64][];
            s_pppiCaseMoveLineKnight = new int[64][][];
            s_pppiCaseMoveDiagKnight = new int[64][][];
            s_pppiCaseMoveTiger = new int[64][][];
            s_pppiCaseMoveElephant = new int[64][][];
            s_pppiCaseMoveDiagLineKnight = new int[64][][];
            s_ppiCaseMoveFerz = new int[64][];
            s_ppiCaseMoveWazir = new int[64][];
            s_ppiCaseMoveCrazyHorse = new int[64][];
            s_ppiCaseWhiteAmazonPawnCanAttackFrom = new int[64][];
            s_ppiCaseBlackAmazonPawnCanAttackFrom = new int[64][];

            s_ppiCaseMoveWhiteGaja = new int[64][];
            s_ppiCaseMoveBlackGaja = new int[64][];
            s_ppiCaseMoveLion = new int[64][];
            s_ppiCaseMoveZebra = new int[64][];
            s_ppiCaseMoveCamel = new int[64][];
            s_ppiCaseMoveGiraffe = new int[64][];

            s_ppiCaseMoveBuffalo = new int[64][];

            s_pppiCaseMoveUnicorn = new int[64][][];

            s_pppiCaseMoveNemesis = new int[64][][];

            s_ppiCaseMoveWhiteReaper = new int[64][];
            s_ppiCaseMoveBlackReaper = new int[64][];
            s_ppiCaseMoveGhost = new int[64][];

            s_ppiCaseMoveWhiteGoldGeneral = new int[64][];
            s_ppiCaseMoveBlackGoldGeneral = new int[64][];

            s_ppiCaseMoveWhiteSilverGeneral = new int[64][];
            s_ppiCaseMoveBlackSilverGeneral = new int[64][];

            s_ppiCaseMoveBlackShogiHorse = new int[64][];
            s_ppiCaseMoveWhiteShogiHorse = new int[64][];

            s_pppiCaseMoveBlackLancer = new int[64][][];
            s_pppiCaseMoveWhiteLancer = new int[64][][];

            s_pppiCaseMoveSnake = new int[64][][];
            s_pppiCaseMoveHipo = new int[64][][];

            s_ppiCaseMoveRaja = new int[64][];

            s_pppiCaseMoveDragon = new int[64][][];
            s_pppiCaseMoveDragonHorse = new int[64][][];

            s_ppiCaseWhiteNemesisPawnCanAttackFrom = new int[64][];
            s_ppiCaseBlackNemesisPawnCanAttackFrom = new int[64][];

            s_pppiCaseMoveDiagonalDim = new int[64][][];

            s_pppiCaseMoveLineDim = new int[64][][];

            s_ppiCaseMoveKnightDim = new int[64][];

            for (int iPos = 0; iPos < 64; iPos++) {
                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 0, -1, 1, 0, -1, 0, 1, 1, -1, 1, 0, 1, 1 }, true, false);
                s_pppiCaseMoveDiagLine[iPos] = arrMove.ToArray();
                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 1, 1, -1, 1, 1 }, true, false);
                s_pppiCaseMoveDiagonal[iPos] = arrMove.ToArray();
                FillMoves(iPos, arrMove, new int[] { -1, 0, 1, 0, 0, -1, 0, 1 }, true, false);
                s_pppiCaseMoveLine[iPos] = arrMove.ToArray();
                FillMoves(iPos, arrMove, new int[] { 1, 2, 1, -2, 2, -1, 2, 1, -1, 2, -1, -2, -2, -1, -2, 1 }, false, false);
                s_ppiCaseMoveKnight[iPos] = arrMove[0];
                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 0, -1, 1, 0, -1, 0, 1, 1, -1, 1, 0, 1, 1 }, false, false);
                s_ppiCaseMoveKing[iPos] = arrMove[0];
                FillMoves(iPos, arrMove, new int[] { -1, -1, 1, -1 }, false, false);
                s_ppiCaseWhitePawnCanAttackFrom[iPos] = arrMove[0];
                FillMoves(iPos, arrMove, new int[] { -1, 1, 1, 1 }, false, false);
                s_ppiCaseBlackPawnCanAttackFrom[iPos] = arrMove[0];


                // added
                // chancellor
                FillMoves(iPos, arrMove, new int[] { -1, 0, 1, 0, 0, -1, 0, 1 }, true, false);
                //FillMoves(iPos, arrMove, new int[] { 1, 2, 1, -2, 2, -1, 2, 1, -1, 2, -1, -2, -2, -1, -2, 1 }, false, true);
                FillMoves(iPos, arrMove, new int[] { 1, 2, 1, -2, 2, -1, 2, 1, -1, 2, -1, -2, -2, -1, -2, 1 }, true, true, 1);

                s_pppiCaseMoveLineKnight[iPos] = arrMove.ToArray();

                // archbishop
                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 1, 1, -1, 1, 1 }, true, false);
                // FillMoves(iPos, arrMove, new int[] { 1, 2, 1, -2, 2, -1, 2, 1, -1, 2, -1, -2, -2, -1, -2, 1 }, false, true);
                FillMoves(iPos, arrMove, new int[] { 1, 2, 1, -2, 2, -1, 2, 1, -1, 2, -1, -2, -2, -1, -2, 1 }, true, true, 1);
                s_pppiCaseMoveDiagKnight[iPos] = arrMove.ToArray();

                // Tiger

                //FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 1, 1, -1, 1, 1, -2, -2, -2, 2, 2, -2, 2, 2 ,0,0}, false, false);
                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 1, 1, -1, 1, 1 }, true, false, 2);
                s_pppiCaseMoveTiger[iPos] = arrMove.ToArray();

                // Elephant
                //FillMoves(iPos, arrMove, new int[] { -1, 0, 1, 0, 0, -1, 0, 1, -2, 0, 2, 0, 0, -2, 0, 2, -2, 0, 3, 0, 0, -3, 0, 3 }, false, false);
                FillMoves(iPos, arrMove, new int[] { -1, 0, 1, 0, 0, -1, 0, 1 }, true, false, 3);
                s_pppiCaseMoveElephant[iPos] = arrMove.ToArray();

                // Amazon
                //FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 0, -1, 1, 0, -1, 0, 1, 1, -1, 1, 0, 1, 1 }, true, false);
                //FillMoves(iPos, arrMove, new int[] { 1, 2, 1, -2, 2, -1, 2, 1, -1, 2, -1, -2, -2, -1, -2, 1 }, false, true);
                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 0, -1, 1, 0, -1, 0, 1, 1, -1, 1, 0, 1, 1 }, true, false);
                FillMoves(iPos, arrMove, new int[] { 1, 2, 1, -2, 2, -1, 2, 1, -1, 2, -1, -2, -2, -1, -2, 1 }, true, true, 1);





                s_pppiCaseMoveDiagLineKnight[iPos] = arrMove.ToArray();
                if (iPos == 32)
                    iPos = iPos; // debug
                // Ferz
                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 1, 1, -1, 1, 1 }, false, false);
                s_ppiCaseMoveFerz[iPos] = arrMove[0];

                // Wazir
                FillMoves(iPos, arrMove, new int[] { -1, 0, 1, 0, 0, -1, 0, 1 }, false, false);
                s_ppiCaseMoveWazir[iPos] = arrMove[0];

                // CrazyHorse
                FillMoves(iPos, arrMove, new int[] { 1, 2, -1, 2, -2, 1, 2, 1 }, false, false);
                s_ppiCaseMoveCrazyHorse[iPos] = arrMove[0];

                // amazon pawn
                FillMoves(iPos, arrMove, new int[] { -1, -1, 1, -1, 1, 0 }, false, false);
                s_ppiCaseWhiteAmazonPawnCanAttackFrom[iPos] = arrMove[0];
                FillMoves(iPos, arrMove, new int[] { -1, 1, 1, 1, -1, 0 }, false, false);
                s_ppiCaseBlackAmazonPawnCanAttackFrom[iPos] = arrMove[0];

                // Gaja
                //FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 1, 1, -1, 1, 1, 1, 0 }, true, false);
                //FillMoves(iPos, arrMove, new int[] { 2, 2, 2, -2, -2, -2, -2, 2, 0, 2, 0, -2, -2, 0, 2, 0 }, false, false);

                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 1, 1, -1, 1, 1, 0, 1, 2, 2, 2, -2, -2, -2, -2, 2, 0, 2, 0, -2, -2, 0, 2, 0 }, false, false);
                s_ppiCaseMoveWhiteGaja[iPos] = arrMove[0];

                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 1, 1, -1, 1, 1, 0, -1, 2, 2, 2, -2, -2, -2, -2, 2, 0, 2, 0, -2, -2, 0, 2, 0 }, false, false);
                s_ppiCaseMoveBlackGaja[iPos] = arrMove[0];

                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 0, -1, 1, 0, -1, 0, 1, 1, -1, 1, 0, 1, 1, 2, 2, 2, 1, 2, 0, 2, -1, 2, -2, -2, 2, -2, 1, -2, 0, -2, -1, -2, -2, -1, 2, 0, 2, 1, 2, -1, -2, 0, -2, 1, -2 }, false, false);
                s_ppiCaseMoveLion[iPos] = arrMove[0];

                FillMoves(iPos, arrMove, new int[] { 2, 3, 2, -3, 3, -2, 3, 2, -2, 3, -2, -3, -3, -2, -3, 2 }, false, false);
                s_ppiCaseMoveZebra[iPos] = arrMove[0];

                FillMoves(iPos, arrMove, new int[] { 1, 3, 1, -3, 3, -1, 3, 1, -1, 3, -1, -3, -3, -1, -3, 1 }, false, false);
                s_ppiCaseMoveCamel[iPos] = arrMove[0];

                // todo giraffe


                // unicorn
                FillMoves(iPos, arrMove, new int[] { 1, 2, 1, -2, 2, -1, 2, 1, -1, 2, -1, -2, -2, -1, -2, 1 }, true, false);
                s_pppiCaseMoveUnicorn[iPos] = arrMove.ToArray();

                // buffalo
                FillMoves(iPos, arrMove, new int[] { 1, 3, 1, -3, 3, -1, 3, 1, -1, 3, -1, -3, -3, -1, -3, 1, 2, 3, 2, -3, 3, -2, 3, 2, -2, 3, -2, -3, -3, -2, -3, 2, 1, 2, 1, -2, 2, -1, 2, 1, -1, 2, -1, -2, -2, -1, -2, 1 }, false, false);
                s_ppiCaseMoveBuffalo[iPos] = arrMove[0];

                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 0, -1, 1, 0, -1, 0, 1, 1, -1, 1, 0, 1, 1 }, true, false);
                s_pppiCaseMoveNemesis[iPos] = arrMove.ToArray();

                // 
                FillMoves2(iPos, arrMove, true, true);
                s_ppiCaseMoveWhiteReaper[iPos] = arrMove[0];

                FillMoves2(iPos, arrMove, true, false);
                s_ppiCaseMoveBlackReaper[iPos] = arrMove[0];

                FillMoves2(iPos, arrMove, false, false);
                s_ppiCaseMoveGhost[iPos] = arrMove[0];


                // Shoghi

                FillMoves(iPos, arrMove, new int[] { -1, 1, 1, 1, 1, -1, -1, -1, 0, 1, 1, 0, -1, 0 }, false, false);
                s_ppiCaseMoveWhiteGoldGeneral[iPos] = arrMove[0];

                FillMoves(iPos, arrMove, new int[] { -1, 1, 1, 1, 1, -1, -1, -1, 0, -1, 1, 0, -1, 0 }, false, false);
                s_ppiCaseMoveBlackGoldGeneral[iPos] = arrMove[0];


                FillMoves(iPos, arrMove, new int[] { -1, 1, 1, 1, 1, -1, -1, -1, 0, 1 }, false, false);
                s_ppiCaseMoveWhiteSilverGeneral[iPos] = arrMove[0];

                FillMoves(iPos, arrMove, new int[] { -1, 1, 1, 1, 1, -1, -1, -1, 0, -1 }, false, false);
                s_ppiCaseMoveBlackSilverGeneral[iPos] = arrMove[0];


                FillMoves(iPos, arrMove, new int[] { 1, 2, -1, 2 }, false, false);
                s_ppiCaseMoveWhiteShogiHorse[iPos] = arrMove[0];

                FillMoves(iPos, arrMove, new int[] { 1, -2, -1, -2 }, false, false);
                s_ppiCaseMoveBlackShogiHorse[iPos] = arrMove[0];

                FillMoves(iPos, arrMove, new int[] { 0, 1 }, true, false);
                s_pppiCaseMoveWhiteLancer[iPos] = arrMove.ToArray();

                FillMoves(iPos, arrMove, new int[] { 0, -1 }, true, false);
                s_pppiCaseMoveBlackLancer[iPos] = arrMove.ToArray();

                ///
                FillMoves(iPos, arrMove, new int[] { -1, -1, 1, -1, -1, 1, 1, 1 }, true, false, 2);
                s_pppiCaseMoveSnake[iPos] = arrMove.ToArray();


                FillMoves(iPos, arrMove, new int[] { -1, 0, 1, 0, 0, -1, 0, 1 }, true, false, 3);
                s_pppiCaseMoveHipo[iPos] = arrMove.ToArray();

                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 0, -1, 1, 0, -1, 0, 1, 1, -1, 1, 0, 1, 1, 1, 2, 1, -2, 2, -1, 2, 1, -1, 2, -1, -2, -2, -1, -2, 1 }, false, false);
                s_ppiCaseMoveRaja[iPos] = arrMove[0];

                // shogi
                // dragon
                FillMoves(iPos, arrMove, new int[] { -1, 0, 1, 0, 0, -1, 0, 1 }, true, false);
                //FillMoves(iPos, arrMove, new int[] { 1, 2, 1, -2, 2, -1, 2, 1, -1, 2, -1, -2, -2, -1, -2, 1 }, false, true);
                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 0, -1, 1, 0, -1, 0, 1, 1, -1, 1, 0, 1, 1 }, true, true, 1);

                s_pppiCaseMoveDragon[iPos] = arrMove.ToArray();

                // dragonHorse
                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 1, 1, -1, 1, 1 }, true, false);
                // FillMoves(iPos, arrMove, new int[] { 1, 2, 1, -2, 2, -1, 2, 1, -1, 2, -1, -2, -2, -1, -2, 1 }, false, true);
                FillMoves(iPos, arrMove, new int[] { -1, -1, -1, 0, -1, 1, 0, -1, 0, 1, 1, -1, 1, 0, 1, 1 }, true, true, 1);
                s_pppiCaseMoveDragonHorse[iPos] = arrMove.ToArray();

                // nemesis pawn
                FillMoves(iPos, arrMove, new int[] { -1, -1, 1, -1 }, false, false);
                s_ppiCaseWhiteNemesisPawnCanAttackFrom[iPos] = arrMove[0];
                FillMoves(iPos, arrMove, new int[] { -1, 1, 1, 1 }, false, false);
                s_ppiCaseBlackNemesisPawnCanAttackFrom[iPos] = arrMove[0];

               
                FillMovesDim(iPos, arrMove, new int[] { -1, -1, -1, 1, 1, -1, 1, 1 }, true, false,-1);
                s_pppiCaseMoveDiagonalDim[iPos] = arrMove.ToArray();
                if (iPos == 16)
                    iPos = 16;

                FillMoves(iPos, arrMove, new int[] { -1, 0, 1, 0, 0, -1, 0, 1 }, true, false);
                s_pppiCaseMoveLineDim[iPos] = arrMove.ToArray();


                FillMovesDim(iPos, arrMove, new int[] { 1, 2, 1, -2, 2, -1, 2, 1, -1, 2, -1, -2, -2, -1, -2, 1 }, false, false,1);
                s_ppiCaseMoveKnightDim[iPos] = arrMove[0];
                if (iPos == 48)
                    iPos = 48;

            }
        }

        static private void FillMoves2(int iStartPos, List<int[]> arrMove, bool isRep, bool isWhite)
        {

            List<int> arrMoveOnLine;

            arrMove.Clear();
            arrMoveOnLine = new List<int>(8);

            // default
            int startIndex = 0;
            int endIndex = 64;

            if (isRep && isWhite)
            {
                startIndex = 0;
                endIndex = 56;
            }
            if (isRep && !isWhite)
            {
                startIndex = 8;
                endIndex = 64;
            }

            for (int iIndex = startIndex; iIndex < endIndex; iIndex += 1)
            {
                if (iIndex != iStartPos)
                {
                    arrMoveOnLine.Add(iIndex);
                }
            }
            arrMove.Add(arrMoveOnLine.ToArray());
        }

        /// <summary>
        /// Fill the possible move array using the specified delta
        /// </summary>
        /// <param name="iStartPos">    Start position</param>
        /// <param name="arrMove">      Array of move to fill</param>
        /// <param name="arrDelta">     List of delta</param>
        /// <param name="bRepeat">      true to repeat, false to do it once</param>
        /// 
        static private void FillMoves(int iStartPos, List<int[]> arrMove, int[] arrDelta, bool bRepeat, bool concatArray)
        {
            FillMoves(iStartPos, arrMove, arrDelta, bRepeat, concatArray, -1);
        }

        static private void FillMoves(int iStartPos, List<int[]> arrMove, int[] arrDelta, bool bRepeat, bool concatArray, int limit) {
            int iColPos;
            int iRowPos;
            int iColIndex;
            int iRowIndex;
            int iColOfs;
            int iRowOfs;
            int iPosOfs;
            int iNewPos;
            int limitRep;
            List<int> arrMoveOnLine;
            if (!concatArray)
                arrMove.Clear();
            arrMoveOnLine = new List<int>(8);
            iColPos = iStartPos & 7;
            iRowPos = iStartPos >> 3;
            for (int iIndex = 0; iIndex < arrDelta.Length; iIndex += 2) {
                iColOfs = arrDelta[iIndex];
                iRowOfs = arrDelta[iIndex + 1];
                iPosOfs = iRowOfs * 8 + iColOfs;
                iColIndex = iColPos + iColOfs;
                iRowIndex = iRowPos + iRowOfs;
                iNewPos = iStartPos + iPosOfs;
                if (bRepeat) {
                    arrMoveOnLine.Clear();
                    limitRep = limit;
                    while (iColIndex >= 0 && iColIndex < 8 && iRowIndex >= 0 && iRowIndex < 8 && limitRep != 0) {
                        arrMoveOnLine.Add(iNewPos);
                        limitRep--;
                        if (bRepeat) {
                            iColIndex += iColOfs;
                            iRowIndex += iRowOfs;
                            iNewPos += iPosOfs;
                        } else {
                            iColIndex = -1;
                        }
                    }
                    if (arrMoveOnLine.Count != 0) {
                        arrMove.Add(arrMoveOnLine.ToArray());
                    }
                } else if (iColIndex >= 0 && iColIndex < 8 && iRowIndex >= 0 && iRowIndex < 8) {
                    arrMoveOnLine.Add(iNewPos);
                }
            }
            if (!bRepeat) {
                arrMove.Add(arrMoveOnLine.ToArray());
            }

            // just for debug
            if (!bRepeat && concatArray && iStartPos == 28)
            {
                int debug = 0;
            }
            /// <summary>Chess board</summary>
            /// 63 62 61 60 59 58 57 56
            /// 55 54 53 52 51 50 49 48
            /// 47 46 45 44 43 42 41 40
            /// 39 38 37 36 35 34 33 32
            /// 31 30 29 28 27 26 25 24
            /// 23 22 21 20 19 18 17 16
            /// 15 14 13 12 11 10 9  8
            /// 7  6  5  4  3  2  1  0

        }

        static private void FillMovesDim(int iStartPos, List<int[]> arrMove, int[] arrDelta, bool bRepeat, bool concatArray, int limit)
        {
            int iColPos;
            int iRowPos;
            int iColIndex;
            int iRowIndex;
            int iColOfs;
            int iRowOfs;
            int iPosOfs;
            int iNewPos;
            int limitRep;
            List<int> arrMoveOnLine;
            if (!concatArray)
                arrMove.Clear();
            arrMoveOnLine = new List<int>(8);
            iColPos = iStartPos & 7;
            iRowPos = iStartPos >> 3;
            for (int iIndex = 0; iIndex < arrDelta.Length; iIndex += 2)
            {
                iColOfs = arrDelta[iIndex];
                iRowOfs = arrDelta[iIndex + 1];
                iPosOfs = iRowOfs * 8 + iColOfs;
                iColIndex = iColPos + iColOfs;
                iRowIndex = iRowPos + iRowOfs;
                iNewPos = iStartPos + iPosOfs;

                // check first iteration and recalculate iNewPos if its necessary
                if (iColIndex >= 8)
                { // cross dimension left
                    iColIndex = iColIndex % 8;
                    iPosOfs = dimensionChangeLeft(iRowOfs, iColOfs);
                }
                else if (iColIndex < 0)
                {
                    iColIndex += 8;
                    iPosOfs = dimensionChangeRight(iRowOfs, iColOfs);
                }
                iNewPos = iStartPos + iPosOfs;



                // debug
                if (iStartPos == 16 && bRepeat)
                    iStartPos = 16;

                if (bRepeat)
                {
                    arrMoveOnLine.Clear();
                    limitRep = limit;
                    while (iRowIndex >= 0 && iRowIndex < 8 && limitRep != 0)
                    {
                        if (iNewPos >= 0 && iNewPos <= 63)
                            arrMoveOnLine.Add(iNewPos);
                        limitRep--;
                        if (bRepeat)
                        {
                            iColIndex += iColOfs;
                            iRowIndex += iRowOfs;
                            if (iColIndex >= 8) { // cross dimension left
                                iColIndex = iColIndex % 8;
                                iPosOfs = dimensionChangeLeft(iRowOfs, iColOfs);
                            }
                            else if (iColIndex < 0) {
                                iColIndex += 8;
                                iPosOfs = dimensionChangeRight(iRowOfs, iColOfs);
                            }
                            else
                                iPosOfs = iRowOfs * 8 + iColOfs;
                            iNewPos += iPosOfs;
                        }
                        else {
                            iColIndex = -1;
                        }
                    }  // end while
                    if (arrMoveOnLine.Count != 0)
                    {
                        arrMove.Add(arrMoveOnLine.ToArray());
                    }
                }
                else if (iRowIndex >= 0 && iRowIndex < 8)
                {
                    if(iColIndex >= 8)
                    {
                        iColIndex = iColIndex % 8;
                        iPosOfs = dimensionChangeLeft(iRowOfs, iColOfs);
                    } else if(iColIndex < 0)
                    {
                        iColIndex += 8;
                        iPosOfs = dimensionChangeRight(iRowOfs, iColOfs);
                    }
                    iNewPos = iStartPos + iPosOfs;
                    if(iNewPos>=0 && iNewPos <= 63)
                        arrMoveOnLine.Add(iNewPos);
                }
            }
            if (!bRepeat)
            {
                arrMove.Add(arrMoveOnLine.ToArray());
            }

            // just for debug
            if (!bRepeat && concatArray && iStartPos == 28)
            {
                int debug = 0;
            }
            /// <summary>Chess board</summary>
            /// 63 62 61 60 59 58 57 56
            /// 55 54 53 52 51 50 49 48
            /// 47 46 45 44 43 42 41 40
            /// 39 38 37 36 35 34 33 32
            /// 31 30 29 28 27 26 25 24
            /// 23 22 21 20 19 18 17 16
            /// 15 14 13 12 11 10 9  8
            /// 7  6  5  4  3  2  1  0

        }

        //1 -1

        static private int dimensionChangeLeft(int offRow, int offCol)
        {
            return offCol - 8 + offRow * 8;
        }
        // 16   1  -1
        static private int dimensionChangeRight(int offRow, int offCol)
        {
            return offCol + 8 + offRow * 8;
        }

        /// <summary>
        /// Class constructor. Build a board.
        /// </summary>
        private ChessBoard(SearchEngineAlphaBeta searchEngineAlphaBeta, SearchEngineMinMax searchEngineMinMax) {
            m_pBoard = new PieceE[64];
            m_book = new Book();
            m_piPiecesCount = new int[128]; // important set to 32
            m_rnd = new Random((int)DateTime.Now.Ticks);
            m_rndRep = new Random(0);
            m_stackPossibleEnPassantAt = new Stack<int>(256);
            m_trace = null;
            m_moveHistory = new MoveHistory();
            m_bDesignMode = false;
            m_moveStack = new MovePosStack();
            m_searchEngineAlphaBeta = searchEngineAlphaBeta;
            m_searchEngineMinMax = searchEngineMinMax;
            //ResetBoardMarti();
            //ResetBoardMartiVsClassic();


            //ResetBoardTest2();
            //ResetBoardTestAmazon();
            //ResetBoardTestElephant2();
            //ResetBoardTestEmpowered();
        //   ResetBoardTestInvasion();
            ResetBoardTestCheckMateElephant();
            //ResetBoardGeneric(0, 1, true, true, false, 0);
            //ResetBoardTestKing();
            //ResetBoard();
        }

        /// <summary>
        /// Class constructor. Build a board.
        /// </summary>
        public ChessBoard(SearchEngine.ITrace trace) : this(null, null) {
            m_trace = trace;
            m_searchEngineAlphaBeta = new SearchEngineAlphaBeta(trace, m_rnd, m_rndRep);
            m_searchEngineMinMax = new SearchEngineMinMax(trace, m_rnd, m_rndRep);

        }

        /// <summary>
        /// Class constructor. Build a board.
        /// </summary>
        public ChessBoard() : this((SearchEngine.ITrace)null) {
        }

        /// <summary>
        /// Class constructor. Use to create a new clone
        /// </summary>
        /// <param name="chessBoard">   Board to copy from</param>
        private ChessBoard(ChessBoard chessBoard) : this(chessBoard.m_searchEngineAlphaBeta, chessBoard.m_searchEngineMinMax) {
            CopyFrom(chessBoard);
        }

        /// <summary>
        /// Copy the state of the board from the specified one.
        /// </summary>
        /// <param name="chessBoard">   Board to copy from</param>
        public void CopyFrom(ChessBoard chessBoard) {
            chessBoard.m_pBoard.CopyTo(m_pBoard, 0);
            chessBoard.m_piPiecesCount.CopyTo(m_piPiecesCount, 0);
            m_stackPossibleEnPassantAt = new Stack<int>(chessBoard.m_stackPossibleEnPassantAt.ToArray());
            m_book = chessBoard.m_book;
            m_iBlackKingPos = chessBoard.m_iBlackKingPos;
            m_iWhiteKingPos = chessBoard.m_iWhiteKingPos;
            m_rnd = chessBoard.m_rnd;
            m_rndRep = chessBoard.m_rndRep;
            m_iRBlackRookMoveCount = chessBoard.m_iRBlackRookMoveCount;
            m_iLBlackRookMoveCount = chessBoard.m_iLBlackRookMoveCount;
            m_iBlackKingMoveCount = chessBoard.m_iBlackKingMoveCount;
            m_iRWhiteRookMoveCount = chessBoard.m_iRWhiteRookMoveCount;
            m_iLWhiteRookMoveCount = chessBoard.m_iLWhiteRookMoveCount;
            m_iWhiteKingMoveCount = chessBoard.m_iWhiteKingMoveCount;
            m_bWhiteCastle = chessBoard.m_bWhiteCastle;
            m_bBlackCastle = chessBoard.m_bBlackCastle;
            m_iPossibleEnPassantAt = chessBoard.m_iPossibleEnPassantAt;
            m_i64ZobristKey = chessBoard.m_i64ZobristKey;
            m_trace = chessBoard.m_trace;
            m_moveStack = chessBoard.m_moveStack.Clone();
            m_moveHistory = chessBoard.m_moveHistory.Clone();
            m_eNextMoveColor = chessBoard.m_eNextMoveColor;
        }

        /// <summary>
        /// Clone the current board
        /// </summary>
        /// <returns>
        /// New copy of the board
        /// </returns>
        public ChessBoard Clone() {
            return (new ChessBoard(this));
        }

        /// <summary>
        /// Returns the XML serialization schema
        /// </summary>
        /// <returns>
        /// null
        /// </returns>
        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema() {
            return (null);
        }

        /// <summary>
        /// Initialize the object using the specified XML reader
        /// </summary>
        /// <param name="reader">   XML reader</param>
        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader) {
            bool bIsEmpty;

            if (reader.MoveToContent() != XmlNodeType.Element || reader.LocalName != "Board") {
                throw new SerializationException("Unknown format");
            } else if (reader.GetAttribute("Version") != "1.00") {
                throw new SerializationException("Unknown version");
            } else {
                reader.ReadStartElement();
                reader.ReadStartElement("Pieces");
                for (int iIndex = 0; iIndex < m_pBoard.Length; iIndex++) {
                    m_pBoard[iIndex] = (PieceE)Enum.Parse(typeof(SerPieceE), reader.ReadElementString("Piece"));
                }
                reader.ReadEndElement();
                m_iBlackKingPos = Int32.Parse(reader.ReadElementString("BlackKingPosition"));
                m_iWhiteKingPos = Int32.Parse(reader.ReadElementString("WhiteKingPosition"));
                reader.ReadStartElement("PieceCount");
                for (int iIndex = 1; iIndex < m_piPiecesCount.Length - 1; iIndex++) {
                    m_piPiecesCount[iIndex] = Int32.Parse(reader.ReadElementString(((SerPieceE)iIndex).ToString()));
                }
                reader.ReadEndElement();
                m_iBlackKingMoveCount = Int32.Parse(reader.ReadElementString("BlackKingMoveCount"));
                m_iWhiteKingMoveCount = Int32.Parse(reader.ReadElementString("WhiteKingMoveCount"));
                m_iRBlackRookMoveCount = Int32.Parse(reader.ReadElementString("RBlackRookMoveCount"));
                m_iLBlackRookMoveCount = Int32.Parse(reader.ReadElementString("LBlackRookMoveCount"));
                m_iRWhiteRookMoveCount = Int32.Parse(reader.ReadElementString("RWhiteRookMoveCount"));
                m_iLWhiteRookMoveCount = Int32.Parse(reader.ReadElementString("LWhiteRookMoveCount"));
                m_bWhiteCastle = Boolean.Parse(reader.ReadElementString("WhiteCastle"));
                m_bBlackCastle = Boolean.Parse(reader.ReadElementString("BlackCastle"));
                m_iPossibleEnPassantAt = Int32.Parse(reader.ReadElementString("EnPassant"));
                m_stackPossibleEnPassantAt.Clear();
                reader.MoveToContent();
                bIsEmpty = reader.IsEmptyElement;
                reader.ReadStartElement("EnPassantStack");
                if (!bIsEmpty) {
                    while (reader.IsStartElement()) {
                        m_stackPossibleEnPassantAt.Push(Int32.Parse(reader.ReadElementString("EP")));
                    }
                    reader.ReadEndElement();
                }
                ((IXmlSerializable)m_moveStack).ReadXml(reader);
                m_i64ZobristKey = Int64.Parse(reader.ReadElementString("ZobristKey"));
                m_bDesignMode = Boolean.Parse(reader.ReadElementString("DesignMode"));
                m_eNextMoveColor = (PlayerColorE)Enum.Parse(typeof(PlayerColorE), reader.ReadElementString("NextMoveColor"));
                m_bStdInitialBoard = Boolean.Parse(reader.ReadElementString("StandardBoard"));
                ((IXmlSerializable)m_moveHistory).ReadXml(reader);
                reader.MoveToContent();
                m_posInfo.m_iAttackedPieces = Int32.Parse(reader.GetAttribute("AttackedPieces"));
                m_posInfo.m_iPiecesDefending = Int32.Parse(reader.GetAttribute("PiecesDefending"));
                reader.ReadStartElement("PositionInfo");
                reader.ReadEndElement();
            }
        }

        /// <summary>
        /// Save the object into the XML writer
        /// </summary>
        /// <param name="writer">   XML writer</param>
        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) {
            int[] piStack;

            writer.WriteStartElement("Board");
            writer.WriteAttributeString("Version", "1.00");

            writer.WriteStartElement("Pieces");
            foreach (PieceE ePiece in m_pBoard) {
                writer.WriteElementString("Piece", ((SerPieceE)ePiece).ToString());
            }
            writer.WriteEndElement();

            writer.WriteElementString("BlackKingPosition", m_iBlackKingPos.ToString());
            writer.WriteElementString("WhiteKingPosition", m_iWhiteKingPos.ToString());

            writer.WriteStartElement("PieceCount");
            for (int iIndex = 1; iIndex < m_piPiecesCount.Length - 1; iIndex++) {
                writer.WriteElementString(((SerPieceE)iIndex).ToString(), m_piPiecesCount[iIndex].ToString());
            }
            writer.WriteEndElement();

            writer.WriteElementString("BlackKingMoveCount", m_iBlackKingMoveCount.ToString());
            writer.WriteElementString("WhiteKingMoveCount", m_iWhiteKingMoveCount.ToString());
            writer.WriteElementString("RBlackRookMoveCount", m_iRBlackRookMoveCount.ToString());
            writer.WriteElementString("LBlackRookMoveCount", m_iLBlackRookMoveCount.ToString());
            writer.WriteElementString("RWhiteRookMoveCount", m_iRWhiteRookMoveCount.ToString());
            writer.WriteElementString("LWhiteRookMoveCount", m_iLWhiteRookMoveCount.ToString());
            writer.WriteElementString("WhiteCastle", m_bWhiteCastle.ToString());
            writer.WriteElementString("BlackCastle", m_bBlackCastle.ToString());
            writer.WriteElementString("EnPassant", m_iPossibleEnPassantAt.ToString());

            writer.WriteStartElement("EnPassantStack");
            piStack = m_stackPossibleEnPassantAt.ToArray();
            Array.Reverse(piStack);
            foreach (int iEnPassant in piStack) {
                writer.WriteElementString("EP", iEnPassant.ToString());
            }
            writer.WriteEndElement();

            ((IXmlSerializable)m_moveStack).WriteXml(writer);
            writer.WriteElementString("ZobristKey", m_i64ZobristKey.ToString());
            writer.WriteElementString("DesignMode", m_bDesignMode.ToString());
            writer.WriteElementString("NextMoveColor", m_eNextMoveColor.ToString());
            writer.WriteElementString("StandardBoard", m_bStdInitialBoard.ToString());
            ((IXmlSerializable)m_moveHistory).WriteXml(writer);
            writer.WriteStartElement("PositionInfo");
            writer.WriteAttributeString("AttackedPieces", m_posInfo.m_iAttackedPieces.ToString());
            writer.WriteAttributeString("PiecesDefending", m_posInfo.m_iPiecesDefending.ToString());
            writer.WriteEndElement();
        }

        /// <summary>
        /// Stack of all moves done since initial board
        /// </summary>
        public MovePosStack MovePosStack {
            get {
                return (m_moveStack);
            }
        }

        /// <summary>
        /// Get the move history which handle the fifty-move rule and the threefold repetition rule
        /// </summary>
        public MoveHistory MoveHistory {
            get {
                return (m_moveHistory);
            }
        }

        /// <summary>
        /// Compute extra information about the board
        /// </summary>
        /// <param name="ePlayerToMove">        Player color to move</param>
        /// <param name="bAddRepetitionInfo">   true to add board repetition information</param>
        /// <returns>
        /// Extra information about the board to discriminate between two boards with sames pieces but
        /// different setting.
        /// </returns>
        public BoardStateMaskE ComputeBoardExtraInfo(PlayerColorE ePlayerToMove, bool bAddRepetitionInfo) {
            BoardStateMaskE eRetVal;

            eRetVal = (BoardStateMaskE)m_iPossibleEnPassantAt;
            if (m_iWhiteKingMoveCount == 0) {
                if (m_iRWhiteRookMoveCount == 0) {
                    eRetVal |= BoardStateMaskE.WRCastling;
                }
                if (m_iLWhiteRookMoveCount == 0) {
                    eRetVal |= BoardStateMaskE.WLCastling;
                }
            }
            if (m_iBlackKingMoveCount == 0) {
                if (m_iRBlackRookMoveCount == 0) {
                    eRetVal |= BoardStateMaskE.BRCastling;
                }
                if (m_iLBlackRookMoveCount == 0) {
                    eRetVal |= BoardStateMaskE.BLCastling;
                }
            }
            if (ePlayerToMove == PlayerColorE.Black) {
                eRetVal |= BoardStateMaskE.BlackToMove;
            }
            if (bAddRepetitionInfo) {
                eRetVal = (BoardStateMaskE)((m_moveHistory.GetCurrentBoardCount(m_i64ZobristKey) & 7) << 11);
            }
            return (eRetVal);
        }

        /// <summary>
        /// Read the opening book from disk
        /// </summary>
        /// <param name="strFileName">  File name</param>
        /// <returns>
        /// true if succeed, false if not
        /// </returns>
        public bool ReadBook(string strFileName) {
            bool bRetVal;

            try {
                bRetVal = m_book.ReadBookFromFile(strFileName);
            } catch (Exception) {
                bRetVal = false;
            }
            return (bRetVal);
        }

        /// <summary>
        /// Read the opening book from disk
        /// </summary>
        /// <param name="strFileName">  File name</param>
        /// <returns>
        /// true if succeed, false if not
        /// </returns>
        public bool ReadBookFromResource(string strFileName) {
            bool bRetVal;

            try {
                bRetVal = m_book.ReadBookFromResource(strFileName);
            } catch (Exception) {
                bRetVal = false;
            }
            return (bRetVal);
        }

        /// <summary>
        /// Reset initial board info
        /// </summary>
        /// <param name="eNextMoveColor">   Next color moving</param>
        /// <param name="bInitialBoardStd"> true if its a standard board, false if coming from FEN or design mode</param>
        /// <param name="eMask">            Extra bord information</param>
        /// <param name="iEnPassant">       Position for en passant</param>
        /// 
        /// TODO castling com a parametre
        private void ResetInitialBoardInfo(PlayerColorE eNextMoveColor, bool bInitialBoardStd, BoardStateMaskE eMask, int iEnPassant) {
            PieceE ePiece;
            int iEnPassantCol;

            Array.Clear(m_piPiecesCount, 0, m_piPiecesCount.Length);
            for (int iIndex = 0; iIndex < 64; iIndex++) {



                ePiece = m_pBoard[iIndex];
                switch (ePiece) {
                    case PieceE.King | PieceE.White:
                        m_iWhiteKingPos = iIndex;
                        break;
                    case PieceE.King | PieceE.Black:
                        m_iBlackKingPos = iIndex;
                        break;
                }
                m_piPiecesCount[(int)ePiece]++;
            }
            if (iEnPassant != 0) {
                iEnPassantCol = (iEnPassant >> 3);
                if (iEnPassantCol != 2 && iEnPassantCol != 5) {
                    if (iEnPassantCol == 3) {   // Fixing old saved board which was keeping the en passant position at the position of the pawn instead of behind it
                        iEnPassant -= 8;
                    } else if (iEnPassantCol == 4) {
                        iEnPassant += 8;
                    } else {
                        iEnPassant = 0;
                    }
                }
            }
            m_iPossibleEnPassantAt = iEnPassant;
            m_iRBlackRookMoveCount = ((eMask & BoardStateMaskE.BRCastling) == BoardStateMaskE.BRCastling) ? 0 : 1;
            m_iLBlackRookMoveCount = ((eMask & BoardStateMaskE.BLCastling) == BoardStateMaskE.BLCastling) ? 0 : 1;
            m_iBlackKingMoveCount = 0;
            m_iRWhiteRookMoveCount = ((eMask & BoardStateMaskE.WRCastling) == BoardStateMaskE.WRCastling) ? 0 : 1;
            m_iLWhiteRookMoveCount = ((eMask & BoardStateMaskE.WLCastling) == BoardStateMaskE.WLCastling) ? 0 : 1;
            m_iWhiteKingMoveCount = 0;
            m_bWhiteCastle = true; //m_whiteCanCastle;
            m_bBlackCastle = true;// m_blackCanCastle;
            m_i64ZobristKey = ZobristKey.ComputeBoardZobristKey(m_pBoard);
            m_eNextMoveColor = eNextMoveColor;
            m_bDesignMode = false;
            m_bStdInitialBoard = bInitialBoardStd;
            m_moveHistory.Reset(m_pBoard, ComputeBoardExtraInfo(PlayerColorE.White, false));
            m_moveStack.Clear();
            m_stackPossibleEnPassantAt.Clear();
        }

        /// <summary>
        /// Reset the board to the initial configuration
        /// </summary>
        public void ResetBoard() {
            for (int iIndex = 0; iIndex < 64; iIndex++) {
                m_pBoard[iIndex] = PieceE.None;
            }
            for (int iIndex = 0; iIndex < 8; iIndex++) {
                m_pBoard[8 + iIndex] = PieceE.Pawn | PieceE.White;
                m_pBoard[48 + iIndex] = PieceE.Pawn | PieceE.Black;
            }
            m_pBoard[0] = PieceE.Rook | PieceE.White;
            m_pBoard[7 * 8] = PieceE.Rook | PieceE.Black;
            m_pBoard[7] = PieceE.Rook | PieceE.White;
            m_pBoard[7 * 8 + 7] = PieceE.Rook | PieceE.Black;
            m_pBoard[1] = PieceE.Knight | PieceE.White;
            m_pBoard[7 * 8 + 1] = PieceE.Knight | PieceE.Black;
            m_pBoard[6] = PieceE.Knight | PieceE.White;
            m_pBoard[7 * 8 + 6] = PieceE.Knight | PieceE.Black;
            m_pBoard[2] = PieceE.Bishop | PieceE.White;
            m_pBoard[7 * 8 + 2] = PieceE.Bishop | PieceE.Black;
            m_pBoard[5] = PieceE.Bishop | PieceE.White;
            m_pBoard[7 * 8 + 5] = PieceE.Bishop | PieceE.Black;
            m_pBoard[3] = PieceE.King | PieceE.White;
            m_pBoard[7 * 8 + 3] = PieceE.King | PieceE.Black;
            m_pBoard[4] = PieceE.Queen | PieceE.White;
            m_pBoard[7 * 8 + 4] = PieceE.Queen | PieceE.Black;
            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }

        /// <summary>
        /// Reset the board to the initial configuration
        /// </summary>
        public void ResetBoardMarti()
        {
            for (int iIndex = 0; iIndex < 64; iIndex++)
            {
                m_pBoard[iIndex] = PieceE.None;
            }
            for (int iIndex = 0; iIndex < 8; iIndex++)
            {
                m_pBoard[8 + iIndex] = PieceE.Pawn | PieceE.White;
                m_pBoard[48 + iIndex] = PieceE.Pawn | PieceE.Black;
            }
            m_pBoard[0] = PieceE.Chancellor | PieceE.White;
            m_pBoard[7 * 8] = PieceE.Chancellor | PieceE.Black;
            m_pBoard[7] = PieceE.Rook | PieceE.White;
            m_pBoard[7 * 8 + 7] = PieceE.Rook | PieceE.Black;
            m_pBoard[1] = PieceE.Knight | PieceE.White;
            m_pBoard[7 * 8 + 1] = PieceE.Knight | PieceE.Black;
            m_pBoard[6] = PieceE.Knight | PieceE.White;
            m_pBoard[7 * 8 + 6] = PieceE.Knight | PieceE.Black;
            m_pBoard[2] = PieceE.Bishop | PieceE.White;
            m_pBoard[7 * 8 + 2] = PieceE.Bishop | PieceE.Black;
            m_pBoard[5] = PieceE.Archbishop | PieceE.White;
            m_pBoard[7 * 8 + 5] = PieceE.Archbishop | PieceE.Black;
            m_pBoard[3] = PieceE.King | PieceE.White;
            m_pBoard[7 * 8 + 3] = PieceE.King | PieceE.Black;
            m_pBoard[4] = PieceE.EmpoweredQueen | PieceE.White;
            m_pBoard[7 * 8 + 4] = PieceE.EmpoweredQueen | PieceE.Black;
            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }

        public void ResetBoardMartiVsClassic()
        {
            for (int iIndex = 0; iIndex < 64; iIndex++)
            {
                m_pBoard[iIndex] = PieceE.None;
            }
            for (int iIndex = 0; iIndex < 8; iIndex++)
            {
                m_pBoard[8 + iIndex] = PieceE.Pawn | PieceE.White;
                m_pBoard[48 + iIndex] = PieceE.Pawn | PieceE.Black;
            }
            m_pBoard[0] = PieceE.Chancellor | PieceE.White;
            m_pBoard[7 * 8] = PieceE.Rook | PieceE.Black;
            m_pBoard[7] = PieceE.Rook | PieceE.White;
            m_pBoard[7 * 8 + 7] = PieceE.Rook | PieceE.Black;
            m_pBoard[1] = PieceE.Knight | PieceE.White;
            m_pBoard[7 * 8 + 1] = PieceE.Knight | PieceE.Black;
            m_pBoard[6] = PieceE.Knight | PieceE.White;
            m_pBoard[7 * 8 + 6] = PieceE.Knight | PieceE.Black;
            m_pBoard[2] = PieceE.Bishop | PieceE.White;
            m_pBoard[7 * 8 + 2] = PieceE.Bishop | PieceE.Black;
            m_pBoard[5] = PieceE.Archbishop | PieceE.White;
            m_pBoard[7 * 8 + 5] = PieceE.Bishop | PieceE.Black;
            m_pBoard[3] = PieceE.King | PieceE.White;
            m_pBoard[7 * 8 + 3] = PieceE.King | PieceE.Black;
            m_pBoard[4] = PieceE.EmpoweredQueen | PieceE.White;
            m_pBoard[7 * 8 + 4] = PieceE.Queen | PieceE.Black;
            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }




        /// <summary>
        /// Save the content of the board into the specified binary writer
        /// </summary>
        /// <param name="writer">   Binary writer</param>
        public void SaveBoard(BinaryWriter writer) {
            string strVersion;
            ChessBoard chessBoardInitial;
            MoveHistory.PackedBoard packetBoard;

            strVersion = "SRCBD095";
            writer.Write(strVersion);
            writer.Write(m_bStdInitialBoard);
            if (!m_bStdInitialBoard) {
                chessBoardInitial = Clone();
                chessBoardInitial.UndoAllMoves();
                packetBoard = MoveHistory.ComputePackedBoard(chessBoardInitial.m_pBoard, ComputeBoardExtraInfo(NextMoveColor, false));
                writer.Write(packetBoard.m_lVal1);
                writer.Write(packetBoard.m_lVal2);
                writer.Write(packetBoard.m_lVal3);
                writer.Write(packetBoard.m_lVal4);
                writer.Write((int)packetBoard.m_eInfo);
                writer.Write(m_iPossibleEnPassantAt);
            }
            m_moveStack.SaveToWriter(writer);
        }

        /// <summary>
        /// Load the content of the board into the specified stream
        /// </summary>
        /// <param name="reader">   Binary reader</param>
        public bool LoadBoard(BinaryReader reader) {
            bool bRetVal;
            MoveHistory.PackedBoard packetBoard;
            string strVersion;
            int iEnPassant;

            strVersion = reader.ReadString();
            if (strVersion != "SRCBD095") {
                bRetVal = false;
            } else {
                bRetVal = true;
                ResetBoard();
                m_bStdInitialBoard = reader.ReadBoolean();
                if (!m_bStdInitialBoard) {
                    packetBoard.m_lVal1 = reader.ReadInt64();
                    packetBoard.m_lVal2 = reader.ReadInt64();
                    packetBoard.m_lVal3 = reader.ReadInt64();
                    packetBoard.m_lVal4 = reader.ReadInt64();
                    packetBoard.m_eInfo = (BoardStateMaskE)reader.ReadInt32();
                    iEnPassant = reader.ReadInt32();
                    MoveHistory.UnpackBoard(packetBoard, m_pBoard);
                    m_eNextMoveColor = ((packetBoard.m_eInfo & BoardStateMaskE.BlackToMove) == BoardStateMaskE.BlackToMove) ? PlayerColorE.Black : PlayerColorE.White;
                    ResetInitialBoardInfo(m_eNextMoveColor, m_bStdInitialBoard, packetBoard.m_eInfo, iEnPassant);
                }
                m_moveStack.LoadFromReader(reader);
                for (int iIndex = 0; iIndex <= m_moveStack.PositionInList; iIndex++) {
                    DoMoveNoLog(m_moveStack.List[iIndex]);
                }
            }
            return (bRetVal);
        }

        /// <summary>
        /// Create a new game using the specified list of moves
        /// </summary>
        /// <param name="chessBoardStarting">   Starting board or null if standard board</param>
        /// <param name="listMove">             List of moves</param>
        /// <param name="eStartingColor">       Board starting color</param>
        public void CreateGameFromMove(ChessBoard chessBoardStarting, List<MovePosS> listMove, PlayerColorE eStartingColor) {
            BoardStateMaskE eMask;

            if (chessBoardStarting != null) {
                CopyFrom(chessBoardStarting);
                eMask = chessBoardStarting.ComputeBoardExtraInfo(PlayerColorE.White, false);
                ResetInitialBoardInfo(eStartingColor, false /*bInitialBoardStd*/, eMask, chessBoardStarting.m_iPossibleEnPassantAt);
            } else {
                ResetBoard();
            }
            foreach (MovePosS movePos in listMove) {
                DoMove(movePos);
            }
        }

        /// <summary>
        /// Determine if the board is in design mode
        /// </summary>
        public bool DesignMode {
            get {
                return (m_bDesignMode);
            }
        }

        /// <summary>
        /// Open the design mode
        /// </summary>
        public void OpenDesignMode() {
            m_bDesignMode = true;
        }

        /// <summary>
        /// Try to close the design mode.
        /// </summary>
        /// <param name="eNextMoveColor">   Color of the next move</param>
        /// <param name="eBoardMask">       Board extra information</param>
        /// <param name="iEnPassant">       Position of en passant or 0 if none</param>
        /// <returns>
        /// true if succeed, false if board is invalid
        /// </returns>
        public bool CloseDesignMode(PlayerColorE eNextMoveColor, BoardStateMaskE eBoardMask, int iEnPassant) {
            bool bRetVal;

            if (!m_bDesignMode) {
                bRetVal = true;
            } else {
                ResetInitialBoardInfo(eNextMoveColor, false, eBoardMask, iEnPassant);
                if (m_piPiecesCount[(int)(PieceE.King | PieceE.White)] == 1 &&
                    m_piPiecesCount[(int)(PieceE.King | PieceE.Black)] == 1) {
                    bRetVal = true;
                } else {
                    bRetVal = false;
                }
            }
            return (bRetVal);
        }

        /// <summary>
        /// true if the board is standard, false if initialized from design mode or FEN
        /// </summary>
        public bool StandardInitialBoard {
            get {
                return (m_bStdInitialBoard);
            }
        }

        /// <summary>
        /// Update the packed board representation and the value of the hash key representing the current board state.
        /// </summary>
        /// <param name="iPos1">        Position of the change</param>
        /// <param name="eNewPiece1">   New piece</param>
        private void UpdatePackedBoardAndZobristKey(int iPos1, PieceE eNewPiece1) {
            m_i64ZobristKey = ZobristKey.UpdateZobristKey(m_i64ZobristKey, iPos1, m_pBoard[iPos1], eNewPiece1);
            m_moveHistory.UpdateCurrentPackedBoard(iPos1, eNewPiece1);
        }

        /// <summary>
        /// Current Zobrist key value
        /// </summary>
        public long CurrentZobristKey {
            get {
                return (m_i64ZobristKey);
            }
        }

        /// <summary>
        /// Update the packed board representation and the value of the hash key representing the current board state. Use if two
        /// board positions are changed.
        /// </summary>
        /// <param name="iPos1">        Position of the change</param>
        /// <param name="eNewPiece1">   New piece</param>
        /// <param name="iPos2">        Position of the change</param>
        /// <param name="eNewPiece2">   New piece</param>
        private void UpdatePackedBoardAndZobristKey(int iPos1, PieceE eNewPiece1, int iPos2, PieceE eNewPiece2) {
            m_i64ZobristKey = ZobristKey.UpdateZobristKey(m_i64ZobristKey, iPos1, m_pBoard[iPos1], eNewPiece1, iPos2, m_pBoard[iPos2], eNewPiece2);
            m_moveHistory.UpdateCurrentPackedBoard(iPos1, eNewPiece1);
            m_moveHistory.UpdateCurrentPackedBoard(iPos2, eNewPiece2);
        }

        /// <summary>
        /// Next moving color
        /// </summary>
        public PlayerColorE NextMoveColor {
            get {
                return (m_eNextMoveColor);
            }
        }

        /// <summary>
        /// Current moving color
        /// </summary>
        public PlayerColorE CurrentMoveColor {
            get {
                return ((m_eNextMoveColor == PlayerColorE.White) ? PlayerColorE.Black : PlayerColorE.White);
            }
        }

        /// <summary>
        /// Get a piece at the specified position. Position 0 = Lower right (H1), 63 = Higher left (A8)
        /// </summary>
        public PieceE this[int iPos] {
            get {
                return (m_pBoard[iPos]);
            }
            set {
                if (m_bDesignMode) {
                    if (m_pBoard[iPos] != value) {
                        m_piPiecesCount[(int)m_pBoard[iPos]]--;
                        m_pBoard[iPos] = value;
                        m_piPiecesCount[(int)m_pBoard[iPos]]++;
                    }
                } else {
                    throw new NotSupportedException("Cannot be used if not in design mode");
                }
            }
        }

        /// <summary>
        /// Get the number of the specified piece which has been eated
        /// </summary>
        /// <param name="ePiece">   Piece and color</param>
        /// <returns>
        /// Count
        /// </returns>
        public int GetEatedPieceCount(PieceE ePiece) {
            int iRetVal;

            switch (ePiece & PieceE.PieceMask) {
                case PieceE.Pawn:
                    iRetVal = 8 - m_piPiecesCount[(int)ePiece];
                    break;
                case PieceE.Rook:
                case PieceE.Knight:
                case PieceE.Bishop:
                    iRetVal = 2 - m_piPiecesCount[(int)ePiece];
                    break;
                case PieceE.Queen:
                case PieceE.King:
                    iRetVal = 1 - m_piPiecesCount[(int)ePiece];
                    break;
                default:
                    iRetVal = 0;
                    break;
            }
            if (iRetVal < 0) {
                iRetVal = 0;
            }
            return (iRetVal);
        }

        /// <summary>
        /// Check the integrity of the board. Use for debugging.
        /// </summary>
        public void CheckIntegrity() {
            int[] piPiecesCount;
            int iBlackKingPos = -1;
            int iWhiteKingPos = -1;

            piPiecesCount = new int[16];
            for (int iIndex = 0; iIndex < 64; iIndex++) {
                piPiecesCount[(int)m_pBoard[iIndex]]++;
                if (m_pBoard[iIndex] == PieceE.King) {
                    iWhiteKingPos = iIndex;
                } else if (m_pBoard[iIndex] == (PieceE.King | PieceE.Black)) {
                    iBlackKingPos = iIndex;
                }
            }
            for (int iIndex = 1; iIndex < 16; iIndex++) {
                if (m_piPiecesCount[iIndex] != piPiecesCount[iIndex]) {
                    throw new ChessException("Piece count mismatch");
                }
            }
            if (iBlackKingPos != m_iBlackKingPos ||
                iWhiteKingPos != m_iWhiteKingPos) {
                throw new ChessException("King position mismatch");
            }
        }

        /// <summary>
        /// Do the move (without log)
        /// </summary>
        /// <param name="movePos">      Move to do</param>
        /// <returns>
        /// NoRepeat        No repetition
        /// ThreeFoldRepeat Three times the same board
        /// FiftyRuleRepeat Fifty moves without pawn move or piece eaten
        /// </returns>
        public RepeatResultE DoMoveNoLog(MovePosS movePos) {
            RepeatResultE eRetVal;
            PieceE ePiece;
            PieceE eOldPiece;
            int iEnPassantVictimPos;
            int iDelta;
            bool bPawnMoveOrPieceEaten;

            m_stackPossibleEnPassantAt.Push(m_iPossibleEnPassantAt);
            m_iPossibleEnPassantAt = 0;
            ePiece = m_pBoard[movePos.StartPos];
            bPawnMoveOrPieceEaten = ((ePiece & PieceE.PieceMask) == PieceE.Pawn) |
                                      ((movePos.Type & MoveTypeE.PieceEaten) == MoveTypeE.PieceEaten);
            switch (movePos.Type & MoveTypeE.MoveTypeMask) {
                case MoveTypeE.Castle:
                    UpdatePackedBoardAndZobristKey(movePos.EndPos, ePiece, movePos.StartPos, PieceE.None);
                    m_pBoard[movePos.EndPos] = ePiece;
                    m_pBoard[movePos.StartPos] = PieceE.None;
                    eOldPiece = PieceE.None;
                    if ((ePiece & PieceE.Black) != 0) {
                        if (movePos.EndPos == 57) {
                            UpdatePackedBoardAndZobristKey(58, m_pBoard[56], 56, PieceE.None);
                            m_pBoard[58] = m_pBoard[56];
                            m_pBoard[56] = PieceE.None;
                        } else {
                            UpdatePackedBoardAndZobristKey(60, m_pBoard[63], 63, PieceE.None);
                            m_pBoard[60] = m_pBoard[63];
                            m_pBoard[63] = PieceE.None;
                        }
                        m_bBlackCastle = true;
                        m_iBlackKingPos = movePos.EndPos;
                    } else {
                        if (movePos.EndPos == 1) {
                            UpdatePackedBoardAndZobristKey(2, m_pBoard[0], 0, PieceE.None);
                            m_pBoard[2] = m_pBoard[0];
                            m_pBoard[0] = PieceE.None;
                        } else {
                            UpdatePackedBoardAndZobristKey(4, m_pBoard[7], 7, PieceE.None);
                            m_pBoard[4] = m_pBoard[7];
                            m_pBoard[7] = PieceE.None;
                        }
                        m_bWhiteCastle = true;
                        m_iWhiteKingPos = movePos.EndPos;
                    }
                    break;
                case MoveTypeE.EnPassant:
                    UpdatePackedBoardAndZobristKey(movePos.EndPos, ePiece, movePos.StartPos, PieceE.None);
                    m_pBoard[movePos.EndPos] = ePiece;
                    m_pBoard[movePos.StartPos] = PieceE.None;
                    iEnPassantVictimPos = (movePos.StartPos & 56) + (movePos.EndPos & 7);
                    eOldPiece = m_pBoard[iEnPassantVictimPos];
                    UpdatePackedBoardAndZobristKey(iEnPassantVictimPos, PieceE.None);
                    m_pBoard[iEnPassantVictimPos] = PieceE.None;
                    m_piPiecesCount[(int)eOldPiece]--;
                    break;
                default:
                    // Normal
                    // PawnPromotionTo???
                    eOldPiece = m_pBoard[movePos.EndPos];   /// pete endpos 249
                    switch (movePos.Type & MoveTypeE.MoveTypeMask) {
                        case MoveTypeE.PawnPromotionToChancellor:
                            m_piPiecesCount[(int)ePiece]--;
                            ePiece = PieceE.Chancellor | (ePiece & PieceE.Black);
                            m_piPiecesCount[(int)ePiece]++;
                            break;
                        case MoveTypeE.PawnPromotionToQueen:
                            m_piPiecesCount[(int)ePiece]--;
                            ePiece = PieceE.Queen | (ePiece & PieceE.Black);
                            m_piPiecesCount[(int)ePiece]++;
                            break;
                        case MoveTypeE.PawnPromotionToRook:
                            m_piPiecesCount[(int)ePiece]--;
                            ePiece = PieceE.Rook | (ePiece & PieceE.Black);
                            m_piPiecesCount[(int)ePiece]++;
                            break;
                        case MoveTypeE.PawnPromotionToBishop:
                            m_piPiecesCount[(int)ePiece]--;
                            ePiece = PieceE.Bishop | (ePiece & PieceE.Black);
                            m_piPiecesCount[(int)ePiece]++;
                            break;
                        case MoveTypeE.PawnPromotionToKnight:
                            m_piPiecesCount[(int)ePiece]--;
                            ePiece = PieceE.Knight | (ePiece & PieceE.Black);
                            m_piPiecesCount[(int)ePiece]++;
                            break;
                        
                        case MoveTypeE.PawnPromotionToPawn:
                        default:
                            break;
                    }
                    UpdatePackedBoardAndZobristKey(movePos.EndPos, ePiece, movePos.StartPos, PieceE.None);
                    m_pBoard[movePos.EndPos] = ePiece;
                    m_pBoard[movePos.StartPos] = PieceE.None;
                    m_piPiecesCount[(int)eOldPiece]--;
                    switch (ePiece) {
                        case PieceE.King | PieceE.Black:
                            m_iBlackKingPos = movePos.EndPos;
                            if (movePos.StartPos == 59) {
                                m_iBlackKingMoveCount++;
                            }
                            break;
                        case PieceE.King | PieceE.White:
                            m_iWhiteKingPos = movePos.EndPos;
                            if (movePos.StartPos == 3) {
                                m_iWhiteKingMoveCount++;
                            }
                            break;
                        case PieceE.Rook | PieceE.Black:
                            if (movePos.StartPos == 56) {
                                m_iLBlackRookMoveCount++;
                            } else if (movePos.StartPos == 63) {
                                m_iRBlackRookMoveCount++;
                            }
                            break;
                        case PieceE.Rook | PieceE.White:
                            if (movePos.StartPos == 0) {
                                m_iLWhiteRookMoveCount++;
                            } else if (movePos.StartPos == 7) {
                                m_iRWhiteRookMoveCount++;
                            }
                            break;
                        case PieceE.Pawn | PieceE.White:
                        case PieceE.Pawn | PieceE.Black:
                            iDelta = movePos.StartPos - movePos.EndPos;
                            if (iDelta == -16 || iDelta == 16) {
                                m_iPossibleEnPassantAt = movePos.EndPos + (iDelta >> 1); // Position behind the pawn
                            }
                            break;
                    }
                    break;
            }
            m_moveHistory.UpdateCurrentPackedBoard(ComputeBoardExtraInfo(PlayerColorE.White, false));
            eRetVal = m_moveHistory.AddCurrentPackedBoard(m_i64ZobristKey, bPawnMoveOrPieceEaten);
            m_eNextMoveColor = (m_eNextMoveColor == PlayerColorE.White) ? PlayerColorE.Black : PlayerColorE.White;
            return (eRetVal);
        }

        /// <summary>
        /// Undo a move (without log)
        /// </summary>
        /// <param name="movePos">  Move to undo</param>
        public void UndoMoveNoLog(MovePosS movePos) {
            PieceE ePiece;
            PieceE eOriginalPiece;
            int iOldPiecePos;

            m_moveHistory.RemoveLastMove(m_i64ZobristKey);
            ePiece = m_pBoard[movePos.EndPos];
            switch (movePos.Type & MoveTypeE.MoveTypeMask) {
                case MoveTypeE.Castle:
                    UpdatePackedBoardAndZobristKey(movePos.StartPos, ePiece, movePos.EndPos, PieceE.None);
                    m_pBoard[movePos.StartPos] = ePiece;
                    m_pBoard[movePos.EndPos] = PieceE.None;
                    if ((ePiece & PieceE.Black) != 0) {
                        if (movePos.EndPos == 57) {
                            UpdatePackedBoardAndZobristKey(56, m_pBoard[58], 58, PieceE.None);
                            m_pBoard[56] = m_pBoard[58];
                            m_pBoard[58] = PieceE.None;
                        } else {
                            UpdatePackedBoardAndZobristKey(63, m_pBoard[60], 60, PieceE.None);
                            m_pBoard[63] = m_pBoard[60];
                            m_pBoard[60] = PieceE.None;
                        }
                        m_bBlackCastle = false;
                        m_iBlackKingPos = movePos.StartPos;
                    } else {
                        if (movePos.EndPos == 1) {
                            UpdatePackedBoardAndZobristKey(0, m_pBoard[2], 2, PieceE.None);
                            m_pBoard[0] = m_pBoard[2];
                            m_pBoard[2] = PieceE.None;
                        } else {
                            UpdatePackedBoardAndZobristKey(7, m_pBoard[4], 4, PieceE.None);
                            m_pBoard[7] = m_pBoard[4];
                            m_pBoard[4] = PieceE.None;
                        }
                        m_bWhiteCastle = false;
                        m_iWhiteKingPos = movePos.StartPos;
                    }
                    break;
                case MoveTypeE.EnPassant:
                    UpdatePackedBoardAndZobristKey(movePos.StartPos, ePiece, movePos.EndPos, PieceE.None);
                    m_pBoard[movePos.StartPos] = ePiece;
                    m_pBoard[movePos.EndPos] = PieceE.None;
                    eOriginalPiece = PieceE.Pawn | (((ePiece & PieceE.Black) == 0) ? PieceE.Black : PieceE.White);
                    iOldPiecePos = (movePos.StartPos & 56) + (movePos.EndPos & 7);
                    UpdatePackedBoardAndZobristKey(iOldPiecePos, eOriginalPiece);
                    m_pBoard[iOldPiecePos] = eOriginalPiece;
                    m_piPiecesCount[(int)eOriginalPiece]++;
                    break;
                default:
                    // Normal
                    // PawnPromotionTo???
                    eOriginalPiece = movePos.OriginalPiece;
                    switch (movePos.Type & MoveTypeE.MoveTypeMask) {
                        case MoveTypeE.PawnPromotionToChancellor:
                        case MoveTypeE.PawnPromotionToQueen:
                        case MoveTypeE.PawnPromotionToRook:
                        case MoveTypeE.PawnPromotionToBishop:
                        case MoveTypeE.PawnPromotionToKnight:
                            m_piPiecesCount[(int)ePiece]--;
                            ePiece = PieceE.Pawn | (ePiece & PieceE.Black);
                            m_piPiecesCount[(int)ePiece]++;
                            break;
                        case MoveTypeE.PawnPromotionToPawn:
                        default:
                            break;
                    }
                    UpdatePackedBoardAndZobristKey(movePos.StartPos, ePiece, movePos.EndPos, eOriginalPiece);
                    m_pBoard[movePos.StartPos] = ePiece;
                    m_pBoard[movePos.EndPos] = eOriginalPiece;
                    m_piPiecesCount[(int)eOriginalPiece]++;
                    switch (ePiece) {
                        case PieceE.King | PieceE.Black:
                            m_iBlackKingPos = movePos.StartPos;
                            if (movePos.StartPos == 59) {
                                m_iBlackKingMoveCount--;
                            }
                            break;
                        case PieceE.King:
                            m_iWhiteKingPos = movePos.StartPos;
                            if (movePos.StartPos == 3) {
                                m_iWhiteKingMoveCount--;
                            }
                            break;
                        case PieceE.Rook | PieceE.Black:
                            if (movePos.StartPos == 56) {
                                m_iLBlackRookMoveCount--;
                            } else if (movePos.StartPos == 63) {
                                m_iRBlackRookMoveCount--;
                            }
                            break;
                        case PieceE.Rook:
                            if (movePos.StartPos == 0) {
                                m_iLWhiteRookMoveCount--;
                            } else if (movePos.StartPos == 7) {
                                m_iRWhiteRookMoveCount--;
                            }
                            break;
                    }
                    break;
            }
            m_iPossibleEnPassantAt = m_stackPossibleEnPassantAt.Pop();
            m_eNextMoveColor = (m_eNextMoveColor == PlayerColorE.White) ? PlayerColorE.Black : PlayerColorE.White;
        }

        /// <summary>
        /// Check if there is enough pieces to make a check mate
        /// </summary>
        /// <returns>
        /// true            Yes
        /// false           No
        /// </returns>
        public bool IsEnoughPieceForCheckMate() {
            bool bRetVal;
            int iBigPieceCount;
            int iWhiteBishop;
            int iBlackBishop;
            int iWhiteKnight;
            int iBlackKnight;

            if (m_piPiecesCount[(int)(PieceE.Pawn | PieceE.White)] != 0 ||
                 m_piPiecesCount[(int)(PieceE.Pawn | PieceE.Black)] != 0) {
                bRetVal = true;
            } else {
                iBigPieceCount = m_piPiecesCount[(int)(PieceE.Queen | PieceE.White)] +
                                 m_piPiecesCount[(int)(PieceE.Queen | PieceE.Black)] +
                                 m_piPiecesCount[(int)(PieceE.Rook | PieceE.White)] +
                                 m_piPiecesCount[(int)(PieceE.Rook | PieceE.Black)];
                if (iBigPieceCount != 0) {
                    bRetVal = true;
                } else {
                    iWhiteBishop = m_piPiecesCount[(int)(PieceE.Bishop | PieceE.White)];
                    iBlackBishop = m_piPiecesCount[(int)(PieceE.Bishop | PieceE.Black)];
                    iWhiteKnight = m_piPiecesCount[(int)(PieceE.Knight | PieceE.White)];
                    iBlackKnight = m_piPiecesCount[(int)(PieceE.Knight | PieceE.Black)];
                    if ((iWhiteBishop + iWhiteKnight) >= 2 || (iBlackBishop + iBlackKnight) >= 2) {
                        // Two knights is typically impossible... but who knows!
                        bRetVal = true;
                    } else {
                        bRetVal = false;
                    }
                }
            }
            return (bRetVal);
        }

        /// <summary>
        /// Check if next move is possible.
        /// </summary>
        /// <returns>
        /// NoRepeat        Yes
        /// Check           Yes, but the user is currently in check
        /// Tie             No, no move for the user
        /// Mate            No, user is checkmate
        /// </returns>
        public MoveResultE CheckNextMove() {
            MoveResultE eRetVal;
            List<MovePosS> moveList;
            PlayerColorE ePlayerColor;

            ePlayerColor = NextMoveColor;
            moveList = EnumMoveList(ePlayerColor);
            // depending if victory condition is invasion or not
            if (m_victoryConditon == 1 && IsMidlineInvasion(ePlayerColor))
              eRetVal = MoveResultE.Invasion;
            
            else if (m_victoryConditon == 2 && IsKingOfHill(ePlayerColor))
                eRetVal = MoveResultE.KingOfHill;
            else if (IsCheck(ePlayerColor)) {
                eRetVal = (moveList.Count == 0) ? MoveResultE.Mate : MoveResultE.Check;
            } else {
                if (IsEnoughPieceForCheckMate()) {
                    eRetVal = (moveList.Count == 0) ? MoveResultE.TieNoMove : MoveResultE.NoRepeat;
                } else {
                    eRetVal = MoveResultE.TieNoMatePossible;
                }
            }
            return (eRetVal);
        }

        /// <summary>
        /// Do the move
        /// </summary>
        /// <param name="movePos">      Move to do</param>
        /// <returns>
        /// NoRepeat        No repetition
        /// ThreeFoldRepeat Three times the same board
        /// FiftyRuleRepeat Fifty moves without pawn move or piece eaten
        /// </returns>
        public MoveResultE DoMove(MovePosS movePos) {
            MoveResultE eRetVal;
            // interessant

            /* if (movePos.Type == MoveTypeE.PieceEaten && (m_pBoard[movePos.StartPos] == (PieceE.Tiger | PieceE.White ) || m_pBoard[movePos.StartPos] == (PieceE.Tiger | PieceE.Black)))
             {
                 movePos.StartPos = movePos.EndPos;
             }

             //elph forçar moviment i borrar els dos peces
             if (movePos.Type == MoveTypeE.PieceEaten && (m_pBoard[movePos.StartPos] == (PieceE.Elephant | PieceE.White) || m_pBoard[movePos.StartPos] == (PieceE.Elephant | PieceE.Black)))
             {

                 byte[] arr = MaxPosElephant(movePos.StartPos, movePos.EndPos);
                 movePos.EndPos = arr[2];
                 //movePos.Type = MoveTypeE.Normal;
                 m_pBoard[arr[0]] = PieceE.None;
                 m_pBoard[arr[1]] = PieceE.None;

             }*/


            switch (DoMoveNoLog(movePos)) {
                case RepeatResultE.ThreeFoldRepeat:
                    eRetVal = MoveResultE.ThreeFoldRepeat;
                    break;
                case RepeatResultE.FiftyRuleRepeat:
                    eRetVal = MoveResultE.FiftyRuleRepeat;
                    break;
                default:
                    eRetVal = CheckNextMove();
                    break;
            }
            m_moveStack.AddMove(movePos);
            return (eRetVal);
        }

        /// <summary>
        /// Undo a move
        /// </summary>
        public void UndoMove() {
            UndoMoveNoLog(m_moveStack.CurrentMove);
            m_moveStack.MoveToPrevious();
        }

        /// <summary>
        /// Redo a move
        /// </summary>
        /// <returns>
        /// NoRepeat        No repetition
        /// ThreeFoldRepeat Three times the same board
        /// FiftyRuleRepeat Fifty moves without pawn move or piece eaten
        /// </returns>
        public MoveResultE RedoMove() {
            MoveResultE eRetVal;

            switch (DoMoveNoLog(m_moveStack.NextMove)) {
                case RepeatResultE.ThreeFoldRepeat:
                    eRetVal = MoveResultE.ThreeFoldRepeat;
                    break;
                case RepeatResultE.FiftyRuleRepeat:
                    eRetVal = MoveResultE.FiftyRuleRepeat;
                    break;
                default:
                    eRetVal = CheckNextMove();
                    break;
            }
            m_moveStack.MoveToNext();
            return (eRetVal);
        }

        /// <summary>
        /// SetUndoRedoPosition:    Set the Undo/Redo position
        /// </summary>
        /// <param name="iPos">     New position</param>
        public void SetUndoRedoPosition(int iPos) {
            int iCurPos;

            iCurPos = m_moveStack.PositionInList;
            while (iCurPos > iPos) {
                UndoMove();
                iCurPos--;
            }
            while (iCurPos < iPos) {
                RedoMove();
                iCurPos++;
            }
        }

        /// <summary>
        /// Gets the number of white pieces on the board
        /// </summary>
        public int WhitePieceCount {
            get {
                int iRetVal = 0;

                for (int iIndex = 1; iIndex < 7; iIndex++) {
                    iRetVal += m_piPiecesCount[iIndex];
                }
                return (iRetVal);
            }
        }

        /// <summary>
        /// Gets the number of black pieces on the board
        /// </summary>
        public int BlackPieceCount {
            get {
                int iRetVal = 0;

                for (int iIndex = 9; iIndex < 15; iIndex++) {
                    iRetVal += m_piPiecesCount[iIndex];
                }
                return (iRetVal);
            }
        }

        /// <summary>
        /// Enumerates the attacking position using arrays of possible position and two possible enemy pieces
        /// </summary>
        /// <param name="arrAttackPos">     Array to fill with the attacking position. Can be null if only the count is wanted</param>
        /// <param name="ppiCaseMoveList">  Array of array of position.</param>
        /// <param name="ePiece1">          Piece which can possibly attack this position</param>
        /// <param name="ePiece2">          Piece which can possibly attack this position</param>
        /// <returns>
        /// Count of attacker
        /// </returns>
        private int EnumTheseAttackPos(List<byte> arrAttackPos, int[][] ppiCaseMoveList, PieceE ePiece1, PieceE ePiece2) {
            int iRetVal = 0;
            PieceE ePiece;

            foreach (int[] piMoveList in ppiCaseMoveList) {
                foreach (int iNewPos in piMoveList) {
                    ePiece = m_pBoard[iNewPos];
                    if (ePiece != PieceE.None) {
                        if (ePiece == ePiece1 ||
                            ePiece == ePiece2) {
                            iRetVal++;
                            if (arrAttackPos != null) {
                                arrAttackPos.Add((byte)iNewPos);
                            }
                        }
                        break;
                    }
                }
            }
            return (iRetVal);
        }

        /// <summary>
        /// Enumerates the attacking position using an array of possible position and one possible enemy piece
        /// </summary>
        /// <param name="arrAttackPos">     Array to fill with the attacking position. Can be null if only the count is wanted</param>
        /// <param name="piCaseMoveList">   Array of position.</param>
        /// <param name="ePiece">           Piece which can possibly attack this position</param>
        /// <returns>
        /// Count of attacker
        /// </returns>
        private int EnumTheseAttackPos(List<byte> arrAttackPos, int[] piCaseMoveList, PieceE ePiece) {
            int iRetVal = 0;


          //  var stopWatch = Stopwatch.StartNew();
            // todo optimize
           foreach (int iNewPos in piCaseMoveList) {
                if (m_pBoard[iNewPos] == ePiece) {
                    iRetVal++;
                    if (arrAttackPos != null) {
                        arrAttackPos.Add((byte)iNewPos);
                    }
                }
            }
           /*
            Parallel.ForEach(piCaseMoveList,
                iNewPos =>
                {
                    if (m_pBoard[iNewPos] == ePiece)
                    {
                        iRetVal++;
                        if (arrAttackPos != null)
                        {
                            arrAttackPos.Add((byte)iNewPos);
                        }
                    }

                });
                */


            //Console.WriteLine("EnumEmpowered:  check !!!");
//            System.Diagnostics.Debug.WriteLine("foreach loop execution time = {0} miliseconds\n", stopWatch.Elapsed.TotalMilliseconds);


            return (iRetVal);
        }

        private int EnumTheseAttackPosEmpowered(List<byte> arrAttackPos, int[][] ppiCaseMoveList, PieceE ePiece, PieceE eColor, int originalPos)
        {
            int iRetVal = 0;

            PieceE originalPiece = ePiece;

            foreach (int[] piCaseMoveList in ppiCaseMoveList)
            {
                foreach (int iNewPos in piCaseMoveList)
                {
                    if (m_pBoard[iNewPos] == ePiece)

                    {
                        // check adjacency
                        bool posIsInArray = false;


                        int[] arr = CheckAdjacent(iNewPos, 8);
                        if (ePiece == (PieceE.EmpoweredKnight | eColor))
                            ePiece = KnightAdjacent(arr, eColor == PieceE.White ? 0 : 1);
                        if (ePiece == (PieceE.EmpoweredBishop | eColor))
                            ePiece = BishopAdjacent(arr, eColor == PieceE.White ? 0 : 1);
                        if (ePiece == (PieceE.EmpoweredRook | eColor))
                            ePiece = RookAdjacent(arr, eColor == PieceE.White ? 0 : 1);

                  


                        // determine piece ad arry list
                        if (ePiece == PieceE.Amazon)
                        {


                            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveLine[originalPos], originalPiece, originalPiece);
                            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveDiagonal[originalPos], originalPiece, originalPiece);
                            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveKnight[originalPos], originalPiece);

                        } else if (ePiece == PieceE.Chancellor || ePiece == (PieceE.Chancellor | PieceE.Black))
                        {

                            //posIsInArray = containsInThisArray(s_pppiCaseMoveLineKnight[originalPos], iNewPos);

                            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveLine[originalPos], originalPiece, originalPiece);
                            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveKnight[originalPos], originalPiece);


                        }
                        else if (ePiece == PieceE.Queen || ePiece == (PieceE.Queen | PieceE.Black))
                        {
                            //   posIsInArray = containsInThisArray(s_pppiCaseMoveDiagLine[originalPos], iNewPos);
                            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveLine[originalPos], originalPiece, originalPiece);
                            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveDiagonal[originalPos], originalPiece, originalPiece);

                        }
                        else if (ePiece == PieceE.Archbishop|| ePiece == (PieceE.Archbishop| PieceE.Black))   //color !=1 ? PieceE.EmpoweredBishop :PieceE.EmpoweredBishop|PieceE.Black)
                        {
                            //     posIsInArray = containsInThisArray(s_pppiCaseMoveDiagKnight[originalPos], iNewPos);

                            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveDiagonal[originalPos], originalPiece, originalPiece);
                            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveKnight[originalPos], originalPiece);

                        }

                        else if (ePiece == PieceE.Knight)
                        {
                            //posIsInArray = containsInThisArray(s_ppiCaseMoveKnight[originalPos], iNewPos);
                            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveKnight[originalPos], originalPiece);
                        }
                        else if (ePiece == PieceE.Rook)
                        {
                            //posIsInArray = containsInThisArray(s_pppiCaseMoveLine[originalPos], iNewPos);
                            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveLine[originalPos], originalPiece, originalPiece);
                        }
                        else if (ePiece == PieceE.Bishop)
                        {
                            //posIsInArray = containsInThisArray(s_pppiCaseMoveDiagonal[originalPos], iNewPos);
                            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveDiagonal[originalPos], originalPiece, originalPiece);
                        }


                        if (iRetVal != 0)
                        {
                            //Console.WriteLine("EnumEmpowered: " + ePiece + " check !!!");
                            //iRetVal++;
                            if (arrAttackPos != null)
                            {
                                arrAttackPos.Add((byte)iNewPos);
                                //  Console.WriteLine("EnumEmpowered: " + ePiece + " check ");
                            }

                        }

                    }
                }
            }
            return (iRetVal);
        }

        private bool containsInThisArray(int[][] ppiCaseMoveList, int index)
        {
            foreach (int[] piMoveList in ppiCaseMoveList)
            {
                foreach (int iNewPos in piMoveList)
                {
                    if (iNewPos == index)
                        return true;
                }
            }
            return false;
        }

        private bool containsInThisArray(int[] piCaseMoveList, int index)
        {

            foreach (int iNewPos in piCaseMoveList)
            {
                if (iNewPos == index)
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Enumerates all position which can attack a given position
        /// </summary>
        /// <param name="ePlayerColor">     Position to check for black or white</param>
        /// <param name="iPos">             Position to check.</param>
        /// <param name="arrAttackPos">     Array to fill with the attacking position. Can be null if only the count is wanted</param>
        /// <returns>
        /// Count of attacker
        /// </returns>
        private int EnumAttackPos(PlayerColorE ePlayerColor, int iPos, List<byte> arrAttackPos) {
            int iRetVal;
            PieceE eColor;
            PieceE eEnemyQueen;
            PieceE eEnemyRook;
            PieceE eEnemyKing;
            PieceE eEnemyBishop;
            PieceE eEnemyKnight;
            PieceE eEnemyPawn;
            PieceE eEnemyChancellor;
            PieceE eEnemyArchbishop;
            PieceE eEnemyEmpoweredQueen;
            PieceE eEnemyEmpoweredKnight;
            PieceE eEnemyEmpoweredBishop;
            PieceE eEnemyEmpoweredRook;
            PieceE eEnemyTiger;
            PieceE eEnemyElephant;
            PieceE eEnemyAmazon;
            PieceE eEnemyFerz;
            PieceE eEnemyWazir;
            PieceE eEnemyCrazyHorse;
            PieceE eEnemyAmazonPawn;
            PieceE eEnemyGaja;
            PieceE eEnemyZebra;
            PieceE eEnemyCamel;
            PieceE eEnemyLion;
            PieceE eEnemyUnicorn;
            PieceE eEnemyBuffalo;
            PieceE eEnemyNemesis;
            PieceE eEnemyShogiHorse;
            PieceE eEnemyLancer;
            PieceE eEnemyGoldGeneral;
            PieceE eEnemySilverGeneral;
            PieceE eEnemySnake;
            PieceE eEnemyHipo;
            PieceE eEnemyDragon;
            PieceE eEnemyDragonHorse;
            PieceE eEnemyNemesisPawn;
            PieceE eEnemyDimensionalKnight;
            PieceE eEnemyDimensionalBishop;
            PieceE eEnemyDimensionalRook;

            eColor = (ePlayerColor == PlayerColorE.Black) ? PieceE.White : PieceE.Black;
            eEnemyQueen = PieceE.Queen | eColor;
            eEnemyRook = PieceE.Rook | eColor;
            eEnemyKing = PieceE.King | eColor;
            eEnemyBishop = PieceE.Bishop | eColor;
            eEnemyKnight = PieceE.Knight | eColor;
            eEnemyPawn = PieceE.Pawn | eColor;
            eEnemyChancellor = PieceE.Chancellor | eColor;
            eEnemyArchbishop = PieceE.Archbishop | eColor;
            eEnemyEmpoweredQueen = PieceE.EmpoweredQueen | eColor;
            eEnemyEmpoweredKnight = PieceE.EmpoweredKnight | eColor;
            eEnemyEmpoweredBishop = PieceE.EmpoweredBishop | eColor;
            eEnemyEmpoweredRook = PieceE.EmpoweredRook | eColor;
            eEnemyTiger = PieceE.Tiger | eColor;
            eEnemyElephant = PieceE.Elephant | eColor;
            eEnemyAmazon = PieceE.Amazon | eColor;
            eEnemyFerz = PieceE.Ferz | eColor;
            eEnemyWazir = PieceE.Wazir | eColor;
            eEnemyCrazyHorse = PieceE.CrazyHorse | eColor;
            eEnemyAmazonPawn = PieceE.AmazonPawn | eColor;
            eEnemyGaja = PieceE.Gaja | eColor;
            eEnemyZebra = PieceE.Zebra | eColor;
            eEnemyCamel = PieceE.Camel | eColor;
            eEnemyLion = PieceE.Lion | eColor;
            eEnemyUnicorn = PieceE.Unicorn | eColor;
            eEnemyBuffalo = PieceE.Buffalo | eColor;
            eEnemyNemesis = PieceE.Nemesis | eColor;
            eEnemyLancer = PieceE.Lancer | eColor;
            eEnemyShogiHorse = PieceE.ShogiHorse | eColor;
            eEnemyGoldGeneral = PieceE.GoldGeneral | eColor;
            eEnemySilverGeneral = PieceE.SilverGeneral | eColor;
            eEnemySnake = PieceE.Snake | eColor;
            eEnemyHipo = PieceE.Hipo | eColor;
            eEnemyDragon = PieceE.Dragon | eColor;
            eEnemyDragonHorse = PieceE.DragonHorse | eColor;
            eEnemyNemesisPawn = PieceE.NemesisPawn | eColor;

            eEnemyDimensionalKnight = PieceE.DimensionalKnight | eColor;
            eEnemyDimensionalBishop = PieceE.DimensionalBishop | eColor;
            eEnemyDimensionalRook = PieceE.DimensionalRook | eColor;


            // test

            //iRetVal = 0; /// delete
            //for (int i = 0; i < 100; i++)
            { 

            iRetVal = EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveDiagonal[iPos], eEnemyQueen, eEnemyBishop);  // bug pos 72 playing withElephant  // crash when playing anial vs animal bandom Fisher
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveLine[iPos], eEnemyQueen, eEnemyRook);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveKing[iPos], eEnemyKing);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveKnight[iPos], eEnemyKnight);
            iRetVal += EnumTheseAttackPos(arrAttackPos, (ePlayerColor == PlayerColorE.Black) ? s_ppiCaseWhitePawnCanAttackFrom[iPos] : s_ppiCaseBlackPawnCanAttackFrom[iPos], eEnemyPawn);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveLineKnight[iPos], eEnemyChancellor, eEnemyChancellor);  // review
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveDiagKnight[iPos], eEnemyArchbishop, eEnemyArchbishop);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveKing[iPos], eEnemyEmpoweredQueen);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveTiger[iPos], eEnemyTiger, eEnemyTiger);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveElephant[iPos], eEnemyElephant, eEnemyElephant);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveDiagLineKnight[iPos], eEnemyAmazon, eEnemyAmazon);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveFerz[iPos], eEnemyFerz);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveWazir[iPos], eEnemyWazir);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveCrazyHorse[iPos], eEnemyCrazyHorse);
            iRetVal += EnumTheseAttackPos(arrAttackPos, (ePlayerColor == PlayerColorE.Black) ? s_ppiCaseWhitePawnCanAttackFrom[iPos] : s_ppiCaseBlackPawnCanAttackFrom[iPos], eEnemyAmazonPawn);
            iRetVal += EnumTheseAttackPos(arrAttackPos, (ePlayerColor == PlayerColorE.White) ? s_ppiCaseMoveWhiteGaja[iPos] : s_ppiCaseMoveBlackGaja[iPos], eEnemyGaja);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveZebra[iPos], eEnemyZebra);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveCamel[iPos], eEnemyCamel);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveLion[iPos], eEnemyLion);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveUnicorn[iPos], eEnemyUnicorn, eEnemyUnicorn);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveBuffalo[iPos], eEnemyBuffalo);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveBlackShogiHorse[iPos], eEnemyShogiHorse);
            iRetVal += EnumTheseAttackPos(arrAttackPos, (ePlayerColor == PlayerColorE.White) ? s_ppiCaseMoveWhiteGoldGeneral[iPos] : s_ppiCaseMoveBlackGoldGeneral[iPos], eEnemyGoldGeneral);
            iRetVal += EnumTheseAttackPos(arrAttackPos, (ePlayerColor == PlayerColorE.White) ? s_ppiCaseMoveWhiteSilverGeneral[iPos] : s_ppiCaseMoveBlackSilverGeneral[iPos], eEnemySilverGeneral);
            iRetVal += EnumTheseAttackPos(arrAttackPos, (ePlayerColor == PlayerColorE.White) ? s_pppiCaseMoveWhiteLancer[iPos] : s_pppiCaseMoveBlackLancer[iPos], eEnemyLancer, eEnemyLancer);

            iRetVal += EnumTheseAttackPosEmpowered(arrAttackPos, s_pppiCaseMoveDiagLineKnight[iPos], eEnemyEmpoweredKnight, eColor, iPos);
            iRetVal += EnumTheseAttackPosEmpowered(arrAttackPos, s_pppiCaseMoveDiagLineKnight[iPos], eEnemyEmpoweredBishop, eColor, iPos);
            iRetVal += EnumTheseAttackPosEmpowered(arrAttackPos, s_pppiCaseMoveDiagLineKnight[iPos], eEnemyEmpoweredRook, eColor, iPos);

            //iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveDiagonal[iPos], eEnemyEmpoweredBishop, eEnemyEmpoweredBishop);  // bug pos 72 playing withElephant
            //iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveLine[iPos], eEnemyEmpoweredRook, eEnemyEmpoweredRook);
            //iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveKnight[iPos], eEnemyEmpoweredKnight);
            //  Console.WriteLine("enumThese:" + iPos);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveDiagonal[iPos], eEnemyNemesis, eEnemyNemesis);  // bug pos 72 playing withElephant
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveLine[iPos], eEnemyNemesis, eEnemyNemesis);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveSnake[iPos], eEnemySnake, eEnemySnake);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveHipo[iPos], eEnemyHipo, eEnemyHipo);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveDragon[iPos], eEnemyDragon, eEnemyDragon);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveDragonHorse[iPos], eEnemyDragonHorse, eEnemyDragonHorse);
            iRetVal += EnumTheseAttackPos(arrAttackPos, (ePlayerColor == PlayerColorE.Black) ?  s_ppiCaseWhiteNemesisPawnCanAttackFrom[iPos]: s_ppiCaseWhiteNemesisPawnCanAttackFrom[iPos], eEnemyNemesisPawn);

            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveDiagonalDim[iPos], eEnemyDimensionalBishop, eEnemyDimensionalBishop);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_pppiCaseMoveLineDim[iPos], eEnemyDimensionalRook, eEnemyDimensionalRook);
            iRetVal += EnumTheseAttackPos(arrAttackPos, s_ppiCaseMoveKnightDim[iPos], eEnemyDimensionalKnight);
                ///todo

            } 


            //if (iRetVal == 1  && iPos == 4)
            //  iRetVal = 1;

            return (iRetVal);
        }

        /// <summary>
        /// Determine if the specified king is attacked
        /// </summary>
        /// <param name="eColor">           King's color to check</param>
        /// <param name="iKingPos">         Position of the king</param>
        /// <returns>
        /// true if in check
        /// </returns>
        private bool IsCheck(PlayerColorE eColor, int iKingPos) {
           // if (iKingPos == 51)
             //   iKingPos = 59;
            bool isCheck_ = (EnumAttackPos(eColor, iKingPos, null) != 0);

           // if (isCheck_ && eColor == PlayerColorE.Black)
             //   Console.WriteLine("isCheck king pos: " + iKingPos);
            return isCheck_;
        }

        public bool IsMidlineInvasion2(PlayerColorE eColor) { 
          
            if (eColor == PlayerColorE.White && m_iWhiteKingPos >= 32 && m_iWhiteKingPos <= 63 )
                return true;
            if (eColor == PlayerColorE.Black && m_iBlackKingPos >= 0 && m_iBlackKingPos <= 31)
                return true;

            return false;
            }


        public bool IsMidlineInvasion(PlayerColorE eColor)
        {

            if ( m_iWhiteKingPos >= 32 && m_iWhiteKingPos <= 63)
                return true;
            if ( m_iBlackKingPos >= 0 && m_iBlackKingPos <= 31)
                return true;

            return false;
        }

        public bool IsKingOfHill(PlayerColorE eColor)
        {
            int kingPos = ((eColor == PlayerColorE.Black) ? m_iBlackKingPos : m_iWhiteKingPos);

            if (eColor == PlayerColorE.White && (m_iWhiteKingPos == 27 || m_iWhiteKingPos == 28 || m_iWhiteKingPos == 35 || m_iWhiteKingPos == 36))
                return true;
            if (eColor == PlayerColorE.Black && (m_iBlackKingPos == 27 || m_iBlackKingPos == 28 || m_iBlackKingPos == 35 || m_iBlackKingPos == 36))
                return true;

            return false;
        }



        /// <summary>
        /// Determine if the specified king is attacked
        /// </summary>
        /// <param name="eColor">           King's color to check</param>
        /// <returns>
        /// true if in check
        /// </returns>
        public bool IsCheck(PlayerColorE eColor) {

            //Console.WriteLine("black and white king pos: " +  m_iBlackKingPos +"  "+ m_iWhiteKingPos);

            bool chk = (IsCheck(eColor, (eColor == PlayerColorE.Black) ? m_iBlackKingPos : m_iWhiteKingPos));
            //if (chk)
              //  Console.WriteLine("check at:" + ((eColor == PlayerColorE.Black) ? m_iBlackKingPos : m_iWhiteKingPos));

            return chk;
        }


        /// <summary>
        /// Evaluates a board. The number of point is greater than 0 if white is in advantage, less than 0 if black is.
        /// </summary>
        /// <param name="searchMode">       Search mode</param>
        /// <param name="ePlayerToPlay">    Color of the player to play</param>
        /// <param name="iDepth">           Depth of the search</param>
        /// <param name="iMoveCountDelta">  White move count - Black move count</param>
        /// <param name="posInfoWhite">     Information about pieces attack</param>
        /// <param name="posInfoBlack">     Information about pieces attack</param>
        /// <returns>
        /// Number of points for the current board
        /// </returns>
        public int Points(SearchEngine.SearchMode searchMode, PlayerColorE ePlayerToPlay, int iDepth, int iMoveCountDelta, PosInfoS posInfoWhite, PosInfoS posInfoBlack) {
            int iRetVal;
            IBoardEvaluation boardEval;
            PosInfoS posInfoTmp;

            if (ePlayerToPlay == PlayerColorE.White) {
                boardEval = searchMode.m_boardEvaluationWhite;
                posInfoTmp = posInfoWhite;
            } else {
                boardEval = searchMode.m_boardEvaluationBlack;
                posInfoTmp.m_iAttackedPieces = -posInfoBlack.m_iAttackedPieces;
                posInfoTmp.m_iPiecesDefending = -posInfoBlack.m_iPiecesDefending;
            }
            iRetVal = boardEval.Points(m_pBoard, m_piPiecesCount, posInfoTmp, m_iWhiteKingPos, m_iBlackKingPos, m_bWhiteCastle, m_bBlackCastle, iMoveCountDelta);

            return (iRetVal);
        }

        /// <summary>
        /// Add a move to the move list if the move doesn't provokes the king to be attacked.
        /// </summary>
        /// <param name="ePlayerColor">     Color doing the the move</param>
        /// <param name="iStartPos">        Starting position</param>
        /// <param name="iEndPos">          Ending position</param>
        /// <param name="eType">            type of the move</param>
        /// <param name="arrMovePos">       List of move</param>
       /* private void AddIfNotCheck(PlayerColorE ePlayerColor, int iStartPos, int iEndPos, MoveTypeE eType, List<MovePosS> arrMovePos) {
            PieceE      eNewPiece;
            PieceE      eOldPiece;
            MovePosS    tMovePos;
            bool        bIsCheck;

            bool shouldbeTrue = false;

            eOldPiece           = m_pBoard[iEndPos];
            eNewPiece           = m_pBoard[iStartPos];
            m_pBoard[iEndPos]   = eNewPiece;
            m_pBoard[iStartPos] = PieceE.None;
            bIsCheck            = ((eNewPiece & PieceE.PieceMask) == PieceE.King) ? IsCheck(ePlayerColor, iEndPos) : IsCheck(ePlayerColor);
            m_pBoard[iStartPos] = m_pBoard[iEndPos];
            m_pBoard[iEndPos]   = eOldPiece;

            if (iStartPos == 17 && iEndPos == 0)
            {
                 shouldbeTrue = true;
            }


            if (!bIsCheck) {
                tMovePos.OriginalPiece  = m_pBoard[iEndPos];
                tMovePos.StartPos       = (byte)iStartPos;
                tMovePos.EndPos         = (byte)iEndPos;
                tMovePos.Type           = eType;
                if (m_pBoard[iEndPos] != PieceE.None || eType == MoveTypeE.EnPassant) {
                    tMovePos.Type |= MoveTypeE.PieceEaten;
                    m_posInfo.m_iAttackedPieces++;
                }



                if (arrMovePos != null) {
                    arrMovePos.Add(tMovePos);
                    if (shouldbeTrue)
                    {
                        string debug = "CORRECT";
                    }

                }
                else
                {
                    if (iStartPos == 17 && iEndPos == 0)
                    {
                        string debug = "ERROR";
                    }
                }
            }
        }*/

        private bool isCheckMulti(byte[] arrPos, PlayerColorE ePlayerColor)
        {

            bool bIsCheck = false;
            for (int i = 0; i < arrPos.Length; i++)
            {
               // if()
               
                bIsCheck |= ((m_pBoard[i] & PieceE.PieceMask) == PieceE.King) ? IsCheck(ePlayerColor, arrPos[i]) : IsCheck(ePlayerColor, arrPos[i]);
            }

            return bIsCheck;

        }


        private void AddIfNotCheck(PlayerColorE ePlayerColor, int iStartPos, int iEndPos, MoveTypeE eType, List<MovePosS> arrMovePos)
        {
            PieceE eNewPiece;
            PieceE eOldPiece;
            MovePosS tMovePos, tMovePos2;
            bool bIsCheck;



            eOldPiece = m_pBoard[iEndPos];
            eNewPiece = m_pBoard[iStartPos];

            if (eOldPiece == PieceE.Elephant)
            {
                if (false)//(dist > 2)
                {
                    return;
                }
            }

            // tigre mata
            bool restoreTigerMove = false;
            if(eNewPiece == PieceE.Tiger && m_pBoard[iEndPos]!= PieceE.None)
            {
                m_pBoard[iEndPos] = PieceE.None;
                m_pBoard[iStartPos] = eNewPiece;
                restoreTigerMove = true;
            }
            else {
               

                m_pBoard[iEndPos] = eNewPiece;
                m_pBoard[iStartPos] = PieceE.None;
            }


            if (eNewPiece != PieceE.Elephant)
            {
               bIsCheck = ((eNewPiece & PieceE.PieceMask) == PieceE.King) ? IsCheck(ePlayerColor, iEndPos) : IsCheck(ePlayerColor);
            }
            else {
                // team animals white bug below: TODO check
                byte[] elephantPos = MaxPosElephant((byte)iStartPos, (byte)iEndPos);
                bIsCheck = isCheckMulti(elephantPos, ePlayerColor); 
                //bIsCheck = false;
            }
            
           // bIsCheck = false;
            // restore pos
            if(restoreTigerMove) //(eNewPiece == PieceE.Tiger && m_pBoard[iEndPos] != PieceE.None)
            {
                m_pBoard[iStartPos] = eNewPiece;
                m_pBoard[iEndPos] = eOldPiece;
            }
            else  // restore case normal
            {
                m_pBoard[iStartPos] = m_pBoard[iEndPos];
                m_pBoard[iEndPos] = eOldPiece;
            }
            int esCheck;
            if (bIsCheck && PieceE.Tiger == m_pBoard[iStartPos])
            {
                esCheck = 0;  // tiger check
            }

           
            if ( iStartPos ==12 && iEndPos == 19 )
                esCheck = 0;
            if (iStartPos == 12 && iEndPos == 21)
                esCheck = 0;
            if (!bIsCheck)
            {
                if (eNewPiece == PieceE.Tiger && m_pBoard[iEndPos]!= PieceE.None)
                {

                    tMovePos.OriginalPiece = m_pBoard[iEndPos];  // important! 
                    tMovePos.StartPos = (byte)iStartPos;
                    tMovePos.EndPos = (byte)iEndPos;
                    tMovePos.Type = eType;// MoveTypeE.PieceEaten;

                    /*tMovePos2.OriginalPiece = m_pBoard[iEndPos];  // important! 
                    tMovePos2.StartPos = (byte)iEndPos;
                    tMovePos2.EndPos = (byte)iStartPos;
                    tMovePos2.Type = eType;// MoveTypeE.PieceEaten;
                    */
                }
                else if (false)//(eNewPiece == PieceE.Elephant)
                {

                    // same as normal
                    tMovePos.OriginalPiece = m_pBoard[iEndPos];
                    tMovePos.StartPos = (byte)iStartPos;
                    tMovePos.EndPos = (byte)iEndPos;
                    tMovePos.Type = eType;

                    byte[] elephantPos = MaxPosElephant((byte)iStartPos, (byte)iEndPos);

                    tMovePos.OriginalPiece = PieceE.None;
                    tMovePos.StartPos = (byte)iStartPos;
                    tMovePos.EndPos = (byte)elephantPos[1];
                    tMovePos.Type = eType;

                    tMovePos.OriginalPiece = PieceE.None;
                    tMovePos.StartPos = (byte)iStartPos;
                    tMovePos.EndPos = (byte)elephantPos[2];
                    tMovePos.Type = eType;

                }
                else
                {
                    tMovePos.OriginalPiece = m_pBoard[iEndPos];
                    tMovePos.StartPos = (byte)iStartPos;
                    tMovePos.EndPos = (byte)iEndPos;
                    tMovePos.Type = eType;
                }

                // refactor
                /*tMovePos2.OriginalPiece = m_pBoard[iEndPos];  // important! 
                tMovePos2.StartPos = (byte)iEndPos;
                tMovePos2.EndPos = (byte)iStartPos;
                tMovePos2.Type = eType;// MoveTypeE.PieceEaten;*/

                if (m_pBoard[iEndPos] != PieceE.None || eType == MoveTypeE.EnPassant)
                {
                    tMovePos.Type |= MoveTypeE.PieceEaten;
                    m_posInfo.m_iAttackedPieces++;
                }

                if (arrMovePos != null)
                {
                    arrMovePos.Add(tMovePos);
                  /*  if (eNewPiece == PieceE.Tiger && m_pBoard[iEndPos] != PieceE.None)
                    {
                        arrMovePos.Add(tMovePos2);
                    }*/

                }
            }
        }

        /// <summary>
        /// Add a pawn promotion series of moves to the move list if the move doesn't provokes the king to be attacked.
        /// </summary>
        /// <param name="ePlayerColor">     Color doing the the move</param>
        /// <param name="iStartPos">        Starting position</param>
        /// <param name="iEndPos">          Ending position</param>
        /// <param name="arrMovePos">       List of move</param>
        private void AddPawnPromotionIfNotCheck(PlayerColorE ePlayerColor, int iStartPos, int iEndPos, List<MovePosS> arrMovePos) {
            AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.PawnPromotionToChancellor, arrMovePos);
            AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.PawnPromotionToQueen, arrMovePos);
            AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.PawnPromotionToRook, arrMovePos);
            AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.PawnPromotionToBishop, arrMovePos);
            AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.PawnPromotionToKnight, arrMovePos);
            AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.PawnPromotionToPawn, arrMovePos);
        }



        /*private void Distancia()
        {
            int stx = pos - dis * dim - dis;
            Console.WriteLine(stx);
            for (int j = 0; j <= dis * 2; j++)
            {
                for (int i = stx; i <= stx + dis * 2; i++)
                {
                    int value = i + j * 8;
                    if (value < 0 || value > dim * dim)
                    { }
                    else
                        Console.WriteLine(i + j * 8);
                }
            }
        }*/

        private int Distance(int pos0, int pos1)
        {
            int dx = Math.Abs((pos0 % 8) - (pos1 % 8));
            int dy = Math.Abs((pos0 / 8) - (pos1 / 8));

            return Math.Max(dx, dy);

        }


        /// <summary>
        /// Add a move to the move list if the new position is empty or is an enemy
        /// </summary>
        /// <param name="ePlayerColor">     Color doing the the move</param>
        /// <param name="iStartPos">        Starting position</param>
        /// <param name="iEndPos">          Ending position</param>
        /// <param name="arrMovePos">       List of move</param>
        private bool AddMoveIfEnemyOrEmpty(PlayerColorE ePlayerColor, int iStartPos, int iEndPos, List<MovePosS> arrMovePos) {
            bool bRetVal;
            PieceE eOldPiece;
            bool shouldbeTrue = false;


            if (iStartPos == 17 && iEndPos == 0)
            {
                shouldbeTrue = true;
            }
            if (iStartPos == 17 && iEndPos == 9)
            {
                int shouldbeFalse = 0;
            }
            

            bRetVal = (m_pBoard[iEndPos] == PieceE.None);

            eOldPiece = m_pBoard[iEndPos];

            bool bIsNotGhost = (eOldPiece & PieceE.PieceMask) != PieceE.Ghost;
            bool bIsNotNemesis = (eOldPiece & PieceE.PieceMask) != PieceE.Nemesis;

            bool bIsNotImmobilized = !isAtSnakeRange(iStartPos, (int)ePlayerColor); // snake 

            bool bIsNotElephantAt2Distance = (eOldPiece & PieceE.PieceMask) != PieceE.Elephant   && (Distance(iStartPos,iEndPos)<=2);

            CheckAdjacent(iStartPos, 8);

            PieceE ePiece = m_pBoard[iStartPos];




            switch (ePiece & PieceE.PieceMask)
            {
                case PieceE.Reaper:

                    if ((bRetVal || ((eOldPiece & PieceE.Black) != 0) != (ePlayerColor == PlayerColorE.Black)) && (eOldPiece & PieceE.PieceMask) != PieceE.King && bIsNotNemesis && bIsNotGhost && bIsNotImmobilized)  // && Nemesis or ghost
                    {
                        AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.Normal, arrMovePos);
                        return (bRetVal);
                    }


                    break;
                case PieceE.Ghost:

                    if (bRetVal)
                    {
                        AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.Normal, arrMovePos);
                        return (bRetVal);
                    }
                    break;

                case PieceE.Nemesis:

                    if (bRetVal)
                    {
                        AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.Normal, arrMovePos);
                        return (bRetVal);
                    }

                    break;

                case PieceE.Elephant:


                    // Todo  Check if nemesis or ghost
                    AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.Normal, arrMovePos);
                    return (bRetVal);

                    break;

                    // the same as all pieces but can kill nemesis
                case PieceE.King:

                    if ((bRetVal || ((eOldPiece & PieceE.Black) != 0) != (ePlayerColor == PlayerColorE.Black)) && (eOldPiece & PieceE.PieceMask) != PieceE.Ghost)
                    {

                        AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.Normal, arrMovePos);
                    }
                    else {
                        m_posInfo.m_iPiecesDefending++;
                    }

                    break;
                case PieceE.Hipo:

                   
                    // no condition
                     AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.Normal, arrMovePos);
                    return (bRetVal);
                    //else {
                    //    m_posInfo.m_iPiecesDefending++;
                    //}

                    break;


                default:  // remaining pieces.

                    // Nemesis  // Ghost
                   /* if (bRetVal)  // && Nemesis or ghost
                    {
                        AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.Normal, arrMovePos);
                        return (bRetVal);
                    }/*
                  

                    // Sirlin Elephant
                   /* if ((m_pBoard[iStartPos] & PieceE.PieceMask) == PieceE.Elephant)
                    {
                        AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.Normal, arrMovePos);
                        return (bRetVal);
                    }*/

                    if ((bRetVal || ((eOldPiece & PieceE.Black) != 0) != (ePlayerColor == PlayerColorE.Black))&& (eOldPiece & PieceE.PieceMask) != PieceE.Ghost && bIsNotNemesis)
                    {
                   
                        AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.Normal, arrMovePos);
                    }
                    else {
                        m_posInfo.m_iPiecesDefending++;
                    }





                    break;
            }


           
            return (bRetVal);
        }

        private bool AddMoveIfOnlyEnemy(PlayerColorE ePlayerColor, int iStartPos, int iEndPos, List<MovePosS> arrMovePos)
        {
            bool bRetVal;
            PieceE eOldPiece;
            bool shouldbeTrue = false;


            bRetVal = (m_pBoard[iEndPos] != PieceE.None);
            eOldPiece = m_pBoard[iEndPos];
            if (bRetVal)
            {
                AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.Normal, arrMovePos);
            }
            else {
                m_posInfo.m_iPiecesDefending++;
            }
            return (bRetVal);
        }


        private bool AddMoveIfEnemyElephant(PlayerColorE ePlayerColor, int iStartPos, int iEndPos, int iMaxEndPos, List<MovePosS> arrMovePos)
        {
            bool bRetVal;
            PieceE eOldPiece;
            bool shouldbeTrue = false;


            bRetVal = (m_pBoard[iEndPos] == PieceE.None);
            eOldPiece = m_pBoard[iEndPos];
            if (((eOldPiece & PieceE.Black) != 0) != (ePlayerColor == PlayerColorE.Black))
            {
                if (bRetVal || ((eOldPiece & PieceE.Black) != 0) != (ePlayerColor == PlayerColorE.Black))
                {
                    // todo change to addIfnotCheckElephant
                    AddIfNotCheck(ePlayerColor, iStartPos, iMaxEndPos, MoveTypeE.Normal, arrMovePos);
                }
                else {
                    m_posInfo.m_iPiecesDefending++;
                }

            }
            return (bRetVal);
        }

        private bool AddMoveIfEnemyTiger(PlayerColorE ePlayerColor, int iStartPos, int iEndPos, List<MovePosS> arrMovePos)
        {
            bool bRetVal;
            PieceE eOldPiece;

            bRetVal = (m_pBoard[iEndPos] == PieceE.None);
            eOldPiece = m_pBoard[iEndPos];

            if (bRetVal)
            {
                AddIfNotCheck(ePlayerColor, iStartPos, iEndPos, MoveTypeE.Normal, arrMovePos);
            } else if (((eOldPiece & PieceE.Black) != 0) != (ePlayerColor == PlayerColorE.Black))
            {

                AddIfNotCheck(ePlayerColor, iStartPos, iStartPos, MoveTypeE.Normal, arrMovePos);

            }
            else {
                m_posInfo.m_iPiecesDefending++;
            }

            return (bRetVal);

        }














        /// <summary>
        /// Enumerates the castling move
        /// </summary>
        /// <param name="ePlayerColor"> Color doing the the move</param>
        /// <param name="arrMovePos">   List of move</param>
        private void EnumCastleMove(PlayerColorE ePlayerColor, List<MovePosS> arrMovePos) {
            if (ePlayerColor == PlayerColorE.Black) {
                if (!m_bBlackCastle) {
                    if (m_iBlackKingMoveCount == 0) {
                        if (m_iLBlackRookMoveCount == 0 &&
                            m_pBoard[57] == PieceE.None &&
                            m_pBoard[58] == PieceE.None) {
                            if (EnumAttackPos(ePlayerColor, 58, null) == 0 &&
                                EnumAttackPos(ePlayerColor, 59, null) == 0) {
                                AddIfNotCheck(ePlayerColor, 59, 57, MoveTypeE.Castle, arrMovePos);
                            }
                        }
                        if (m_iRBlackRookMoveCount == 0 &&
                            m_pBoard[60] == PieceE.None &&
                            m_pBoard[61] == PieceE.None &&
                            m_pBoard[62] == PieceE.None) {
                            if (EnumAttackPos(ePlayerColor, 59, null) == 0 &&
                                EnumAttackPos(ePlayerColor, 60, null) == 0) {
                                AddIfNotCheck(ePlayerColor, 59, 61, MoveTypeE.Castle, arrMovePos);
                            }
                        }
                    }
                }
            } else {
                if (!m_bWhiteCastle) {
                    if (m_iWhiteKingMoveCount == 0) {
                        if (m_iLWhiteRookMoveCount == 0 &&
                            m_pBoard[1] == PieceE.None &&
                            m_pBoard[2] == PieceE.None) {
                            if (EnumAttackPos(ePlayerColor, 2, null) == 0 &&
                                EnumAttackPos(ePlayerColor, 3, null) == 0) {
                                AddIfNotCheck(ePlayerColor, 3, 1, MoveTypeE.Castle, arrMovePos);
                            }
                        }
                        if (m_iRWhiteRookMoveCount == 0 &&
                            m_pBoard[4] == PieceE.None &&
                            m_pBoard[5] == PieceE.None &&
                            m_pBoard[6] == PieceE.None) {
                            if (EnumAttackPos(ePlayerColor, 3, null) == 0 &&
                                EnumAttackPos(ePlayerColor, 4, null) == 0) {
                                AddIfNotCheck(ePlayerColor, 3, 5, MoveTypeE.Castle, arrMovePos);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates the move a specified pawn can do
        /// </summary>
        /// <param name="ePlayerColor">     Color doing the the move</param>
        /// <param name="iStartPos">        Pawn position</param>
        /// <param name="arrMovePos">       List of move</param>
        private void EnumPawnMove(PlayerColorE ePlayerColor, int iStartPos, List<MovePosS> arrMovePos) {
            int iDir;
            int iNewPos;
            int iNewColPos;
            int iRowPos;
            bool bCanMove2Case;

            iRowPos = (iStartPos >> 3);
            bCanMove2Case = (ePlayerColor == PlayerColorE.Black) ? (iRowPos == 6) : (iRowPos == 1);
            iDir = (ePlayerColor == PlayerColorE.Black) ? -8 : 8;
            iNewPos = iStartPos + iDir;
            if (iNewPos >= 0 && iNewPos < 64) {
                if (m_pBoard[iNewPos] == PieceE.None) {
                    iRowPos = (iNewPos >> 3);
                    if (iRowPos == 0 || iRowPos == 7) {
                        AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos, arrMovePos);
                    } else {
                        AddIfNotCheck(ePlayerColor, iStartPos, iNewPos, MoveTypeE.Normal, arrMovePos);
                    }
                    if (bCanMove2Case && m_pBoard[iNewPos + iDir] == PieceE.None) {
                        AddIfNotCheck(ePlayerColor, iStartPos, iNewPos + iDir, MoveTypeE.Normal, arrMovePos);
                    }
                }
            }
            iNewPos = iStartPos + iDir;
            if (iNewPos >= 0 && iNewPos < 64) {
                iNewColPos = iNewPos & 7;
                iRowPos = (iNewPos >> 3);
                if (iNewColPos != 0 && m_pBoard[iNewPos - 1] != PieceE.None && (m_pBoard[iNewPos - 1] & PieceE.PieceMask) != PieceE.Ghost && (m_pBoard[iNewPos - 1] & PieceE.PieceMask) != PieceE.Nemesis)
                {
                    if (((m_pBoard[iNewPos - 1] & PieceE.Black) == 0) == (ePlayerColor == PlayerColorE.Black)) {
                        if (iRowPos == 0 || iRowPos == 7) {
                            AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos - 1, arrMovePos);
                        } else {
                            AddIfNotCheck(ePlayerColor, iStartPos, iNewPos - 1, MoveTypeE.Normal, arrMovePos);
                        }
                    } else {
                        m_posInfo.m_iPiecesDefending++;
                    }
                }
                if (iNewColPos != 7 && m_pBoard[iNewPos + 1] != PieceE.None && (m_pBoard[iNewPos + 1] & PieceE.PieceMask) != PieceE.Ghost && (m_pBoard[iNewPos + 1] & PieceE.PieceMask) != PieceE.Nemesis)
                {
                    if (((m_pBoard[iNewPos + 1] & PieceE.Black) == 0) == (ePlayerColor == PlayerColorE.Black)) {
                        if (iRowPos == 0 || iRowPos == 7) {
                            AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos + 1, arrMovePos);
                        } else {
                            AddIfNotCheck(ePlayerColor, iStartPos, iNewPos + 1, MoveTypeE.Normal, arrMovePos);
                        }
                    } else {
                        m_posInfo.m_iPiecesDefending++;
                    }
                }
            }
        }

       

        private int isLeftRight(int start, int end)
        {
            if (start % 8 > end % 8)
                return -1;
            else if (start % 8 == end % 8)
                return 0;
            else
                return 1;
        }

        private void EnumNemesisPawnMove(PlayerColorE ePlayerColor, int iStartPos, List<MovePosS> arrMovePos)
        {
            int iDir;
            int iNewPos;
            int iNewColPos;
            int iRowPos;
            bool bCanMove2Case;

            int enemyKingPos=0;
            if (ePlayerColor == PlayerColorE.White)
                enemyKingPos = FindEnemyKingPosition(PlayerColorE.Black);
            if (ePlayerColor == PlayerColorE.Black)
                enemyKingPos = FindEnemyKingPosition(PlayerColorE.White);


            iRowPos = (iStartPos >> 3);
            bCanMove2Case = (ePlayerColor == PlayerColorE.Black) ? (iRowPos == 6) : (iRowPos == 1);
            iDir = (ePlayerColor == PlayerColorE.Black) ? -8 : 8;
            iNewPos = iStartPos + iDir;
            if (iNewPos >= 0 && iNewPos < 64)
            {
                if (m_pBoard[iNewPos] == PieceE.None)
                {
                    iRowPos = (iNewPos >> 3);
                    if (iRowPos == 0 || iRowPos == 7)
                    {
                        AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos, arrMovePos);
                    }
                    else {
                        AddIfNotCheck(ePlayerColor, iStartPos, iNewPos, MoveTypeE.Normal, arrMovePos);
                    }
                    if (bCanMove2Case && m_pBoard[iNewPos + iDir] == PieceE.None)
                    {
                        AddIfNotCheck(ePlayerColor, iStartPos, iNewPos + iDir, MoveTypeE.Normal, arrMovePos);
                    }
                }
            }
            iNewPos = iStartPos + iDir;
            if (iNewPos >= 0 && iNewPos < 64)
            {
                iNewColPos = iNewPos & 7;
                iRowPos = (iNewPos >> 3);
                if (iNewColPos != 0 && m_pBoard[iNewPos - 1] != PieceE.None && (m_pBoard[iNewPos - 1] & PieceE.PieceMask) != PieceE.Ghost && (m_pBoard[iNewPos - 1] & PieceE.PieceMask) != PieceE.Nemesis)
                {
                    if (((m_pBoard[iNewPos - 1] & PieceE.Black) == 0) == (ePlayerColor == PlayerColorE.Black))
                    {
                        if (iRowPos == 0 || iRowPos == 7)
                        {
                            AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos - 1, arrMovePos);
                        }
                        else {
                            AddIfNotCheck(ePlayerColor, iStartPos, iNewPos - 1, MoveTypeE.Normal, arrMovePos);
                        }
                    }
                    else {
                        m_posInfo.m_iPiecesDefending++;
                    }
                }
                if (iNewColPos != 7 && m_pBoard[iNewPos + 1] != PieceE.None && (m_pBoard[iNewPos + 1] & PieceE.PieceMask) != PieceE.Ghost && (m_pBoard[iNewPos + 1] & PieceE.PieceMask) != PieceE.Nemesis)
                {
                    if (((m_pBoard[iNewPos + 1] & PieceE.Black) == 0) == (ePlayerColor == PlayerColorE.Black))
                    {
                        if (iRowPos == 0 || iRowPos == 7)
                        {
                            AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos + 1, arrMovePos);
                        }
                        else {
                            AddIfNotCheck(ePlayerColor, iStartPos, iNewPos + 1, MoveTypeE.Normal, arrMovePos);
                        }
                    }
                    else {
                        m_posInfo.m_iPiecesDefending++;
                    }
                }
            }



            if ((ePlayerColor == PlayerColorE.Black) ? (iStartPos % 8 > enemyKingPos % 8) : (iStartPos % 8 < enemyKingPos % 8)) // optimize 
            {



                //case up left

                iRowPos = (iStartPos >> 3);
                bCanMove2Case = (ePlayerColor == PlayerColorE.Black) ? (iRowPos == 6) : (iRowPos == 1);
                iDir = (ePlayerColor == PlayerColorE.Black) ? -9 : 9;
                iNewPos = iStartPos + iDir;
                if (iNewPos >= 0 && iNewPos < 64)
                {
                    if (m_pBoard[iNewPos] == PieceE.None)
                    {
                        iRowPos = (iNewPos >> 3);
                        if (iRowPos == 0 || iRowPos == 7)
                        {
                            AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos, arrMovePos);
                        }
                        else {
                            AddIfNotCheck(ePlayerColor, iStartPos, iNewPos, MoveTypeE.Normal, arrMovePos);
                        }

                    }
                }

                //case left

                iRowPos = (iStartPos >> 3);
                bCanMove2Case = (ePlayerColor == PlayerColorE.Black) ? (iRowPos == 6) : (iRowPos == 1);
                iDir = (ePlayerColor == PlayerColorE.Black) ? -1 : 1;
                iNewPos = iStartPos + iDir;
                if (iNewPos >= 0 && iNewPos < 64)
                {
                    if (m_pBoard[iNewPos] == PieceE.None)
                    {
                        iRowPos = (iNewPos >> 3);
                        if (iRowPos == 0 || iRowPos == 7)
                        {
                            AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos, arrMovePos);
                        }
                        else {
                            AddIfNotCheck(ePlayerColor, iStartPos, iNewPos, MoveTypeE.Normal, arrMovePos);
                        }

                    }
                }


            }
            //case up right
           if ((ePlayerColor == PlayerColorE.Black) ? (iStartPos % 8 < enemyKingPos % 8) : (iStartPos % 8 > enemyKingPos % 8)) // optimize
            {
                iRowPos = (iStartPos >> 3);

                iDir = (ePlayerColor == PlayerColorE.Black) ? -7 : 7;
                iNewPos = iStartPos + iDir;
                if (iNewPos >= 0 && iNewPos < 64)
                {
                    if (m_pBoard[iNewPos] == PieceE.None)
                    {
                        iRowPos = (iNewPos >> 3);
                        if (iRowPos == 0 || iRowPos == 7)
                        {
                            AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos, arrMovePos);
                        }
                        else {
                            AddIfNotCheck(ePlayerColor, iStartPos, iNewPos, MoveTypeE.Normal, arrMovePos);
                        }

                    }
                }

                //case right

                iRowPos = (iStartPos >> 3);

                iDir = (ePlayerColor == PlayerColorE.Black) ? 1 : -1;
                iNewPos = iStartPos + iDir;
                if (iNewPos >= 0 && iNewPos < 64)
                {
                    if (m_pBoard[iNewPos] == PieceE.None)
                    {
                        iRowPos = (iNewPos >> 3);
                        if (iRowPos == 0 || iRowPos == 7)
                        {
                            AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos, arrMovePos);
                        }
                        else {
                            AddIfNotCheck(ePlayerColor, iStartPos, iNewPos, MoveTypeE.Normal, arrMovePos);
                        }

                    }
                }
            } // end right and right up

            
        }

        //find enemy pos

        private int FindEnemyKingPosition(PlayerColorE ePlayerColor)
        {
            int pos = 0;
            for (byte i = 0; i < 64; i++) {
                if (m_pBoard[i] ==  (PieceE.King | (ePlayerColor == PlayerColorE.White ? PieceE.White : PieceE.Black)))
                    return i;
            }
               
            return pos;
        }


        /*private void EnumNemesisPawnMove(PlayerColorE ePlayerColor, int iStartPos, List<MovePosS> arrMovePos)
        {
            int iDir;
            int iNewPos;
            int iNewColPos;
            int iRowPos;
            bool bCanMove2Case;
            
            iRowPos = (iStartPos >> 3);
            bCanMove2Case = (ePlayerColor == PlayerColorE.Black) ? (iRowPos == 6) : (iRowPos == 1);
            iDir = (ePlayerColor == PlayerColorE.Black) ? -8 : 8;
            iNewPos = iStartPos + iDir;
            if (iNewPos >= 0 && iNewPos < 64)
            {
                if (m_pBoard[iNewPos] == PieceE.None)
                {
                    iRowPos = (iNewPos >> 3);
                    if (iRowPos == 0 || iRowPos == 7)
                    {
                        AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos, arrMovePos);
                    }
                    else {
                        AddIfNotCheck(ePlayerColor, iStartPos, iNewPos, MoveTypeE.Normal, arrMovePos);
                    }
                    if (bCanMove2Case && m_pBoard[iNewPos + iDir] == PieceE.None)
                    {
                        AddIfNotCheck(ePlayerColor, iStartPos, iNewPos + iDir, MoveTypeE.Normal, arrMovePos);
                    }
                }
            }
            iNewPos = iStartPos + iDir;
            if (iNewPos >= 0 && iNewPos < 64)
            {
                iNewColPos = iNewPos & 7;
                iRowPos = (iNewPos >> 3);
                if (iNewColPos != 0 && m_pBoard[iNewPos - 1] != PieceE.None && m_pBoard[iNewPos - 1] != PieceE.Ghost && m_pBoard[iNewPos - 1] != PieceE.Nemesis)
                {
                    if (((m_pBoard[iNewPos - 1] & PieceE.Black) == 0) == (ePlayerColor == PlayerColorE.Black))
                    {
                        if (iRowPos == 0 || iRowPos == 7)
                        {
                            AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos - 1, arrMovePos);
                        }
                        else {
                            AddIfNotCheck(ePlayerColor, iStartPos, iNewPos - 1, MoveTypeE.Normal, arrMovePos);
                        }
                    }
                    else {
                        m_posInfo.m_iPiecesDefending++;
                    }
                }
                if (iNewColPos != 7 && m_pBoard[iNewPos + 1] != PieceE.None && m_pBoard[iNewPos + 1] != PieceE.Ghost && m_pBoard[iNewPos + 1] != PieceE.Nemesis)
                {
                    if (((m_pBoard[iNewPos + 1] & PieceE.Black) == 0) == (ePlayerColor == PlayerColorE.Black))
                    {
                        if (iRowPos == 0 || iRowPos == 7)
                        {
                            AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos + 1, arrMovePos);
                        }
                        else {
                            AddIfNotCheck(ePlayerColor, iStartPos, iNewPos + 1, MoveTypeE.Normal, arrMovePos);
                        }
                    }
                    else {
                        m_posInfo.m_iPiecesDefending++;
                    }
                }
            }


            //case left or right


        }*/


        /// <summary>
        /// Enumerates the move a specified pawn can do
        /// </summary>
        /// <param name="ePlayerColor">     Color doing the the move</param>
        /// <param name="iStartPos">        Pawn position</param>
        /// <param name="arrMovePos">       List of move</param>
        private void EnumAmazonPawnMove(PlayerColorE ePlayerColor, int iStartPos, List<MovePosS> arrMovePos)
        {
            int iDir;
            int iNewPos;
            int iNewColPos;
            int iRowPos;
            bool bCanMove2Case;

            iRowPos = (iStartPos >> 3);
            bCanMove2Case = (ePlayerColor == PlayerColorE.Black) ? (iRowPos == 6) : (iRowPos == 1);
            iDir = (ePlayerColor == PlayerColorE.Black) ? -8 : 8;
            iNewPos = iStartPos + iDir;
            if (iNewPos >= 0 && iNewPos < 64)
            {
                if (m_pBoard[iNewPos] == PieceE.None)
                {
                    iRowPos = (iNewPos >> 3);
                    if (iRowPos == 0 || iRowPos == 7)
                    {
                        AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos, arrMovePos);
                    }
                    else {
                        AddIfNotCheck(ePlayerColor, iStartPos, iNewPos, MoveTypeE.Normal, arrMovePos);
                    }
                    if (bCanMove2Case && m_pBoard[iNewPos + iDir] == PieceE.None)
                    {
                        AddIfNotCheck(ePlayerColor, iStartPos, iNewPos + iDir, MoveTypeE.Normal, arrMovePos);
                    }
                }
            }
            iNewPos = iStartPos + iDir;
            if (iNewPos >= 0 && iNewPos < 64)
            {
                iNewColPos = iNewPos & 7;
                iRowPos = (iNewPos >> 3);
                if (iNewColPos != 0 && m_pBoard[iNewPos - 1] != PieceE.None)
                {
                    if (((m_pBoard[iNewPos - 1] & PieceE.Black) == 0) == (ePlayerColor == PlayerColorE.Black))
                    {
                        if (iRowPos == 0 || iRowPos == 7)
                        {
                            AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos - 1, arrMovePos);
                        }
                        else {
                            AddIfNotCheck(ePlayerColor, iStartPos, iNewPos - 1, MoveTypeE.Normal, arrMovePos);
                        }
                    }
                    else {
                        m_posInfo.m_iPiecesDefending++;
                    }
                }
                if (iNewColPos != 7 && m_pBoard[iNewPos + 1] != PieceE.None)
                {
                    if (((m_pBoard[iNewPos + 1] & PieceE.Black) == 0) == (ePlayerColor == PlayerColorE.Black))
                    {
                        if (iRowPos == 0 || iRowPos == 7)
                        {
                            AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos + 1, arrMovePos);
                        }
                        else {
                            AddIfNotCheck(ePlayerColor, iStartPos, iNewPos + 1, MoveTypeE.Normal, arrMovePos);
                        }
                    }
                    else {
                        m_posInfo.m_iPiecesDefending++;
                    }
                }
                if (iNewColPos != 7 && m_pBoard[iNewPos] != PieceE.None)
                {
                    if (((m_pBoard[iNewPos] & PieceE.Black) == 0) == (ePlayerColor == PlayerColorE.Black))
                    {
                        if (iRowPos == 0 || iRowPos == 7)
                        {
                            AddPawnPromotionIfNotCheck(ePlayerColor, iStartPos, iNewPos, arrMovePos);
                        }
                        else {
                            AddIfNotCheck(ePlayerColor, iStartPos, iNewPos, MoveTypeE.Normal, arrMovePos);
                        }
                    }
                    else {
                        m_posInfo.m_iPiecesDefending++;
                    }
                }

            }
        }

        /// <summary>
        /// Enumerates the en passant move
        /// </summary>
        /// <param name="ePlayerColor">     Color doing the the move</param>
        /// <param name="arrMovePos">       List of move</param>
        private void EnumEnPassant(PlayerColorE ePlayerColor, List<MovePosS> arrMovePos) {
            int iColPos;
            PieceE eAttackingPawn;
            PieceE ePawnInDanger;
            int iPosBehindPawn;
            int iPosPawnInDanger;

            if (m_iPossibleEnPassantAt != 0) {
                iPosBehindPawn = m_iPossibleEnPassantAt;
                if (ePlayerColor == PlayerColorE.White) {
                    iPosPawnInDanger = iPosBehindPawn - 8;
                    eAttackingPawn = PieceE.Pawn | PieceE.White;
                } else {
                    iPosPawnInDanger = iPosBehindPawn + 8;
                    eAttackingPawn = PieceE.Pawn | PieceE.Black;
                }
                ePawnInDanger = m_pBoard[iPosPawnInDanger];
                // Check if there is an attacking pawn at the left
                iColPos = iPosPawnInDanger & 7;
                if (iColPos > 0 && m_pBoard[iPosPawnInDanger - 1] == eAttackingPawn) {
                    m_pBoard[iPosPawnInDanger] = PieceE.None;
                    AddIfNotCheck(ePlayerColor,
                                  iPosPawnInDanger - 1,
                                  iPosBehindPawn,
                                  MoveTypeE.EnPassant,
                                  arrMovePos);
                    m_pBoard[iPosPawnInDanger] = ePawnInDanger;
                }
                if (iColPos < 7 && m_pBoard[iPosPawnInDanger + 1] == eAttackingPawn) {
                    m_pBoard[iPosPawnInDanger] = PieceE.None;
                    AddIfNotCheck(ePlayerColor,
                                  iPosPawnInDanger + 1,
                                  iPosBehindPawn,
                                  MoveTypeE.EnPassant,
                                  arrMovePos);
                    m_pBoard[iPosPawnInDanger] = ePawnInDanger;
                }
            }
        }

        /// <summary>
        /// Enumerates the move a specified piece can do using the pre-compute move array
        /// </summary>
        /// <param name="ePlayerColor">             Color doing the the move</param>
        /// <param name="iStartPos">                Starting position</param>
        /// <param name="ppiMoveListForThisCase">   Array of array of possible moves</param>
        /// <param name="arrMovePos">               List of move</param>
        private void EnumFromArray(PlayerColorE ePlayerColor, int iStartPos, int[][] ppiMoveListForThisCase, List<MovePosS> arrMovePos, PieceE pieceE) {

            int testCounter = 0;
            foreach (int[] piMovePosForThisDiag in ppiMoveListForThisCase) {
                foreach (int iNewPos in piMovePosForThisDiag) {
                    if (iStartPos == 17 && iNewPos == 0)
                    {

                        testCounter++;

                    }
                    if (iStartPos == 17 && iNewPos == 34)
                    {

                        testCounter++;

                    }
                    if (iStartPos == 17 && iNewPos == 32)
                    {

                        testCounter++;

                    }
                    if (iStartPos == 17 && iNewPos == 27)
                    {

                        testCounter++;

                    }

                    if (iStartPos == 17 && iNewPos == 2)
                    {

                        testCounter++;

                    }

                    if (iStartPos == 17 && iNewPos == 11)
                    {
                        testCounter++;
                    }
                    if (pieceE.Equals(PieceE.Pawn)) {
                        int hello = 0;
                    }
                    if (false)//(pieceE.Equals(PieceE.Tiger))
                    {
                        if (!AddMoveIfEnemyTiger(ePlayerColor, iStartPos, iNewPos, arrMovePos))
                        {
                            break;
                        }
                    }
                    else {
                        if (!AddMoveIfEnemyOrEmpty(ePlayerColor, iStartPos, iNewPos, arrMovePos))
                        {
                            break;
                        }
                    }
                }
            }
            if (iStartPos == 17) {
                int prova = 0;
            }


        }
        /// <summary>Chess board</summary>
        /// 63 62 61 60 59 58 57 56
        /// 55 54 53 52 51 50 49 48
        /// 47 46 45 44 43 42 41 40
        /// 39 38 37 36 35 34 33 32
        /// 31 30 29 28 27 26 25 24
        /// 23 22 21 20 19 18 17 16
        /// 15 14 13 12 11 10 9  8
        /// 7  6  5  4  3  2  1  0


        /// <summary>
        /// Enumerates the move a specified piece can do using the pre-compute move array
        /// </summary>
        /// <param name="ePlayerColor">             Color doing the the move</param>
        /// <param name="iStartPos">                Starting position</param>
        /// <param name="piMoveListForThisCase">    Array of possible moves</param>
        /// <param name="arrMovePos">               List of move</param>
        private void EnumFromArray(PlayerColorE ePlayerColor, int iStartPos, int[] piMoveListForThisCase, List<MovePosS> arrMovePos, PieceE pieceE) {
            foreach (int iNewPos in piMoveListForThisCase) {
                //if (false)//(pieceE == PieceE.Tiger)
                  //  AddMoveIfEnemyTiger(ePlayerColor, iStartPos, iNewPos, arrMovePos);
                //else
                AddMoveIfEnemyOrEmpty(ePlayerColor, iStartPos, iNewPos, arrMovePos);
            }
        }

        /// <summary>
        /// Enumerates all the possible moves for a player
        /// </summary>
        /// <param name="ePlayerColor">             Color doing the the move</param>
        /// <param name="bMoveList">                true to returns a MoveList</param>
        /// <param name="posInfo">                  Structure to fill with pieces information</param>
        /// <returns>
        /// List of possible moves or null
        /// </returns>
        public List<MovePosS> EnumMoveList(PlayerColorE ePlayerColor, bool bMoveList, out PosInfoS posInfo) {
            PieceE ePiece;
            List<MovePosS> arrMovePos;
            bool bBlackToMove;

            m_posInfo.m_iAttackedPieces = 0;
            m_posInfo.m_iPiecesDefending = 0;
            arrMovePos = (bMoveList) ? new List<MovePosS>(256) : null;
            bBlackToMove = (ePlayerColor == PlayerColorE.Black);
            var stopWatch = Stopwatch.StartNew();

            

           //Parallel.For(0, 64, iIndex => 
             //  { 
            for (int iIndex = 0; iIndex < 64; iIndex++) {
                ePiece = m_pBoard[iIndex];
                int j;
                if (bBlackToMove)
                     j = 0;

                if (ePiece != PieceE.None && ((ePiece & PieceE.Black) != 0) == bBlackToMove) {

                    if (bBlackToMove)
                        j = 0;

                    if ((ePiece & PieceE.PieceMask) == (PieceE.EmpoweredKnight))
                    {

                        int[] arr = CheckAdjacent(iIndex, 8);
                        ePiece = KnightAdjacent(arr,(int)ePlayerColor); 
                       
                    }
 

                    if ((ePiece & PieceE.PieceMask) == PieceE.EmpoweredBishop)
                    {

                        int[] arr = CheckAdjacent(iIndex, 8);
                        ePiece = BishopAdjacent(arr, (int)ePlayerColor); // review
                    }

                    if ((ePiece & PieceE.PieceMask) == PieceE.EmpoweredRook)
                    {

                        int[] arr = CheckAdjacent(iIndex, 8);
                        ePiece = RookAdjacent(arr, (int)ePlayerColor); // review
                    }


                    switch (ePiece & PieceE.PieceMask) {
                        case PieceE.Pawn:
                            EnumPawnMove(ePlayerColor, iIndex, arrMovePos);
                            break;
                        case PieceE.NemesisPawn:
                            EnumNemesisPawnMove(ePlayerColor, iIndex, arrMovePos);
                            break;
                        case PieceE.Knight:
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveKnight[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Bishop:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveDiagonal[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Rook:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveLine[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Queen:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveDiagLine[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.King:
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveKing[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Chancellor:  // split
                                                 //EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveLineKnight[iIndex], arrMovePos);
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveKnight[iIndex], arrMovePos, ePiece);
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveLine[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Archbishop:
                            //  EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveDiagKnight[iIndex], arrMovePos);
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveKnight[iIndex], arrMovePos, ePiece);
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveDiagonal[iIndex], arrMovePos, ePiece);
                            break;
                        // empowered queen
                        case PieceE.EmpoweredQueen:
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveKing[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Tiger:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveTiger[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Elephant:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveElephant[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Amazon:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveDiagLineKnight[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Ferz:
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveFerz[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Wazir:
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveWazir[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.CrazyHorse:
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveCrazyHorse[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.AmazonPawn:
                            EnumAmazonPawnMove(ePlayerColor, iIndex, arrMovePos);
                            break;
                        case PieceE.Gaja:
                            EnumFromArray(ePlayerColor, iIndex, ePlayerColor == 0 ? s_ppiCaseMoveWhiteGaja[iIndex] : s_ppiCaseMoveBlackGaja[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.EmpoweredBishop:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveDiagonal[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.EmpoweredRook:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveLine[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.EmpoweredKnight:
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveKnight[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Camel:
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveCamel[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Zebra:
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveZebra[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Lion:
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveLion[iIndex], arrMovePos, ePiece);
                            break;
                            
                        case PieceE.Unicorn:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveUnicorn[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Buffalo:
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveBuffalo[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Nemesis:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveNemesis[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Reaper:

                            EnumFromArray(ePlayerColor, iIndex, ePlayerColor == 0 ? s_ppiCaseMoveWhiteReaper[iIndex] : s_ppiCaseMoveBlackReaper[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Ghost:
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveGhost[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.GoldGeneral:
                            EnumFromArray(ePlayerColor, iIndex, ePlayerColor == 0 ? s_ppiCaseMoveWhiteGoldGeneral[iIndex] : s_ppiCaseMoveBlackGoldGeneral[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.SilverGeneral:
                            EnumFromArray(ePlayerColor, iIndex, ePlayerColor == 0 ? s_ppiCaseMoveWhiteSilverGeneral[iIndex] : s_ppiCaseMoveBlackSilverGeneral[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.ShogiHorse:
                            EnumFromArray(ePlayerColor, iIndex, ePlayerColor == 0 ? s_ppiCaseMoveWhiteShogiHorse[iIndex] : s_ppiCaseMoveBlackShogiHorse[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Lancer:
                            EnumFromArray(ePlayerColor, iIndex, ePlayerColor == 0 ? s_pppiCaseMoveWhiteLancer[iIndex] : s_pppiCaseMoveBlackLancer[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Snake:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveSnake[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Hipo:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveHipo[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.Dragon:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveDragon[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.DragonHorse:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveDragonHorse[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.DimensionalBishop:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveDiagonalDim[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.DimensionalKnight:
                            EnumFromArray(ePlayerColor, iIndex, s_ppiCaseMoveKnightDim[iIndex], arrMovePos, ePiece);
                            break;
                        case PieceE.DimensionalRook:
                            EnumFromArray(ePlayerColor, iIndex, s_pppiCaseMoveLineDim[iIndex], arrMovePos, ePiece);
                            break;



                    }
                }
            }


            m_acumulat += stopWatch.Elapsed.TotalMilliseconds;
            m_iteracions += 1;

            double resultat = m_acumulat / m_iteracions;


            //System.Diagnostics.Debug.WriteLine("foreach loop mitjana acumulada loop = {0} miliseconds\n", resultat);
            // 0.12 sense 
            EnumCastleMove(ePlayerColor, arrMovePos);
            EnumEnPassant(ePlayerColor, arrMovePos);
            posInfo = m_posInfo;
            return (arrMovePos);
        }

        /// <summary>
        /// Enumerates all the possible moves for a player
        /// </summary>
        /// <param name="ePlayerColor">             Color doing the the move</param>
        /// <returns>
        /// List of possible moves
        /// </returns>
        public List<MovePosS> EnumMoveList(PlayerColorE ePlayerColor) {
            PosInfoS posInfo;

            return (EnumMoveList(ePlayerColor, true, out posInfo));
        }

        /// <summary>
        /// Enumerates all the possible moves for a player
        /// </summary>
        /// <param name="ePlayerColor">             Color doing the the move</param>
        /// <param name="posInfo">                  Structure to fill with pieces information</param>
        public void ComputePiecesCoverage(PlayerColorE ePlayerColor, out PosInfoS posInfo) {
            EnumMoveList(ePlayerColor, false, out posInfo);
        }

        /// <summary>
        /// Cancel search
        /// </summary>
        public void CancelSearch() {
            m_searchEngineAlphaBeta.CancelSearch();
            m_searchEngineMinMax.CancelSearch();
        }

        /// <summary>
        /// Find the best move for a player using alpha-beta pruning or minmax search
        /// </summary>
        /// <param name="searchMode">       Search mode</param>
        /// <param name="ePlayerColor">     Color doing the move</param>
        /// <param name="moveBest">         Best move found</param>
        /// <param name="iPermCount">       Total permutation evaluated</param>
        /// <param name="iCacheHit">        Number of moves found in the translation table cache</param>
        /// <param name="iMaxDepth">        Maximum depth reached</param>
        /// <returns>
        /// true if a move has been found
        /// </returns>
        public bool FindBestMove(SearchEngine.SearchMode searchMode, PlayerColorE ePlayerColor, out MovePosS moveBest, out int iPermCount, out int iCacheHit, out int iMaxDepth) {
            bool bRetVal;
            bool bUseAlphaBeta;
            SearchEngine searchEngine;

            bUseAlphaBeta = ((searchMode.m_eOption & SearchEngine.SearchMode.OptionE.UseAlphaBeta) != 0);
            searchEngine = bUseAlphaBeta ? (SearchEngine)m_searchEngineAlphaBeta : (SearchEngine)m_searchEngineMinMax;
            bRetVal = searchEngine.FindBestMove(this,
                                                        searchMode,
                                                        ePlayerColor,
                                                        out moveBest,
                                                        out iPermCount,
                                                        out iCacheHit,
                                                        out iMaxDepth);
            return (bRetVal);
        }

        /// <summary>
        /// Find type of pawn promotion are valid for the specified starting/ending position
        /// </summary>
        /// <param name="ePlayerColor">     Color doing the move</param>
        /// <param name="iStartPos">        Position to start</param>
        /// <param name="iEndPos">          Ending position</param>
        /// <returns>
        /// None or a combination of Queen, Rook, Bishop, Knight and Pawn
        /// </returns>
        public ValidPawnPromotionE FindValidPawnPromotion(PlayerColorE ePlayerColor, int iStartPos, int iEndPos) {
            ValidPawnPromotionE eRetVal = ValidPawnPromotionE.None;
            List<MovePosS> moveList;

            moveList = EnumMoveList(ePlayerColor);
            foreach (MovePosS move in moveList) {
                if (move.StartPos == iStartPos && move.EndPos == iEndPos) {
                    switch (move.Type & MoveTypeE.MoveTypeMask) {
                        case MoveTypeE.PawnPromotionToChancellor:
                            eRetVal |= ValidPawnPromotionE.Chancellor;
                            break;
                        case MoveTypeE.PawnPromotionToQueen:
                            eRetVal |= ValidPawnPromotionE.Queen;
                            break;
                        case MoveTypeE.PawnPromotionToRook:
                            eRetVal |= ValidPawnPromotionE.Rook;
                            break;
                        case MoveTypeE.PawnPromotionToBishop:
                            eRetVal |= ValidPawnPromotionE.Bishop;
                            break;
                        case MoveTypeE.PawnPromotionToKnight:
                            eRetVal |= ValidPawnPromotionE.Knight;
                            break;
                        case MoveTypeE.PawnPromotionToPawn:
                            eRetVal |= ValidPawnPromotionE.Pawn;
                            break;
                        default:
                            break;
                    }
                }
            }
            return (eRetVal);
        }

        /// <summary>
        /// Find a move from the valid move list
        /// </summary>
        /// <param name="ePlayerColor">     Color doing the move</param>
        /// <param name="iStartPos">        Position to start</param>
        /// <param name="iEndPos">          Ending position</param>
        /// <returns>
        /// Move or -1
        /// </returns>
        public MovePosS FindIfValid(PlayerColorE ePlayerColor, int iStartPos, int iEndPos) {
            MovePosS tMoveRetVal;
            List<MovePosS> moveList;
            int iIndex;

            /// <summary>Chess board</summary>
            /// 63 62 61 60 59 58 57 56
            /// 55 54 53 52 51 50 49 48
            /// 47 46 45 44 43 42 41 40
            /// 39 38 37 36 35 34 33 32
            /// 31 30 29 28 27 26 25 24
            /// 23 22 21 20 19 18 17 16
            /// 15 14 13 12 11 10 9  8
            /// 7  6  5  4  3  2  1  0


            moveList = EnumMoveList(ePlayerColor);
            iIndex = moveList.FindIndex(x => x.StartPos == iStartPos && x.EndPos == iEndPos);
            if (iIndex == -1) {
                tMoveRetVal.StartPos = 255;
                tMoveRetVal.EndPos = 255;
                tMoveRetVal.OriginalPiece = PieceE.None;
                tMoveRetVal.Type = MoveTypeE.Normal;
            } else {
                tMoveRetVal = moveList[iIndex];
            }
            return (tMoveRetVal);
        }

        /// <summary>
        /// Find a move from the opening book
        /// </summary>
        /// <param name="searchMode">       Search mode</param>
        /// <param name="ePlayerColor">     Color doing the move</param>
        /// <param name="arrPrevMove">      Previous move</param>
        /// <param name="move">             Found move</param>
        /// <returns>
        /// true if succeed, false if no move found in book
        /// </returns>
        public bool FindBookMove(SearchEngine.SearchMode searchMode, PlayerColorE ePlayerColor, MovePosS[] arrPrevMove, out ChessBoard.MovePosS move) {
            bool bRetVal;
            int iMove;
            Random rnd;

            if (searchMode.m_eRandomMode == SearchEngine.SearchMode.RandomModeE.Off) {
                rnd = null;
            } else if (searchMode.m_eRandomMode == SearchEngine.SearchMode.RandomModeE.OnRepetitive) {
                rnd = m_rndRep;
            } else {
                rnd = m_rnd;
            }
            move.OriginalPiece = PieceE.None;
            move.StartPos = 255;
            move.EndPos = 255;
            move.Type = ChessBoard.MoveTypeE.Normal;
            iMove = m_book.FindMoveInBook(arrPrevMove, rnd);
            if (iMove == -1) {
                bRetVal = false;
            } else {
                move = FindIfValid(ePlayerColor, iMove & 255, iMove >> 8);
                move.Type |= MoveTypeE.MoveFromBook;
                bRetVal = (move.StartPos != 255);
            }
            return (bRetVal);
        }

        /// <summary>
        /// Undo all the specified move starting with the last move
        /// </summary>
        public void UndoAllMoves() {
            while (m_moveStack.PositionInList != -1) {
                UndoMove();
            }
        }

        /// <summary>
        /// Gets the position express in a human form
        /// </summary>
        /// <param name="iPos">     Position</param>
        /// <returns>
        /// Human form position
        /// </returns>
        static public string GetHumanPos(int iPos) {
            string strRetVal;
            int iColPos;
            int iRowPos;

            iColPos = 7 - (iPos & 7);
            iRowPos = iPos >> 3;
            strRetVal = ((Char)(iColPos + 'A')).ToString() + ((Char)(iRowPos + '1')).ToString();
            return (strRetVal);
        }

        /// <summary>
        /// Gets the position express in a human form
        /// </summary>
        /// <param name="move">     Move</param>
        /// <returns>
        /// Human form position
        /// </returns>
        static public string GetHumanPos(ChessBoard.MovePosS move) {
            string strRetVal;

            strRetVal = GetHumanPos(move.StartPos);
            strRetVal += ((move.Type & MoveTypeE.PieceEaten) == MoveTypeE.PieceEaten) ? "x" : "-";
            strRetVal += GetHumanPos(move.EndPos);

            if ((move.Type & ChessBoard.MoveTypeE.MoveFromBook) == ChessBoard.MoveTypeE.MoveFromBook) {
                strRetVal = "(" + strRetVal + ")";
            }
            switch (move.Type & ChessBoard.MoveTypeE.MoveTypeMask) {
                case ChessBoard.MoveTypeE.PawnPromotionToQueen:
                    strRetVal += "=Q";
                    break;
                case ChessBoard.MoveTypeE.PawnPromotionToRook:
                    strRetVal += "=R";
                    break;
                case ChessBoard.MoveTypeE.PawnPromotionToBishop:
                    strRetVal += "=B";
                    break;
                case ChessBoard.MoveTypeE.PawnPromotionToKnight:
                    strRetVal += "=N";
                    break;
                case ChessBoard.MoveTypeE.PawnPromotionToPawn:
                    strRetVal += "=P";
                    break;
                default:
                    break;
            }
            return (strRetVal);
        }

        //added functions

        private PieceE[] SetTeam(int index)
        {

            //classic
            if (index == 0)
                return TeamClassic();
            //chaturanga
            if (index == 1)
                return TeamChaturanga();
            //capablanca
            if (index == 2)
                return TeamCapablanca();
            // nemesis
            if (index == 3)
                return TeamNemesis();
            //Reaper
            if (index == 4)
                return TeamReaper();
            if (index == 5)
                return TeamEmpowered();
            if (index == 6)
                return TeamAnimals();
            if (index == 7)
                return TeamAmazon();
            if (index == 8)
                return TeamShogi();
            if (index == 9)
                return TeamShogiAdapted();
            if (index == 10)
                return TeamDimensional();


            // default
            return TeamClassic();
        }


        /// <summary>
        /// Reset the board to the initial configuration
        /// </summary>
        public void ResetBoardGeneric(int indexW, int indexB, Boolean teamShogi1, Boolean teamShogi2, bool randomfischer, int dificult, int victoryCondition)
        {
            // set difficult
            m_dificultLevel = dificult;
            m_victoryConditon = victoryCondition;
            m_whiteCanCastle = (indexW == 0 ? true : false);
            m_blackCanCastle = (indexB == 0 ? true : false);

            PieceE[] teamW = SetTeam(indexW);
            PieceE[] teamB = SetTeam(indexB);
            Random rnd = new Random();
            if (randomfischer)
            {
                rnd.Shuffle(teamW);
                rnd.Shuffle(teamB);
                //TODO disable castle
            }

            PieceE pawnT1, pawnT2;


            if (indexW == 3)
                pawnT1 = PieceE.NemesisPawn;
            else
                pawnT1 = PieceE.Pawn;

            if (indexB == 3)
                pawnT2 = PieceE.NemesisPawn;
            else
                pawnT2 = PieceE.Pawn;

            for (int iIndex = 0; iIndex < 64; iIndex++)
            {
                m_pBoard[iIndex] = PieceE.None;
            }


            int blackPawnLineUp = 48;
            int whitePawnLineUp = 8;

            if (teamShogi1)
            {

                whitePawnLineUp = 16;
                m_pBoard[9] = PieceE.Rook | PieceE.White;
                m_pBoard[14] = PieceE.Bishop | PieceE.White;
                m_pBoard[11] = PieceE.GoldGeneral | PieceE.White;
                m_pBoard[12] = PieceE.SilverGeneral | PieceE.White;

            } 
            if (teamShogi2)
                blackPawnLineUp = 40;

            for (int iIndex = 0; iIndex < 8; iIndex++)
            {
                m_pBoard[whitePawnLineUp + iIndex] = pawnT1 | PieceE.White;
                m_pBoard[blackPawnLineUp + iIndex] = pawnT2 | PieceE.Black;
            }
            m_pBoard[0] = teamW[0] | PieceE.White;
            m_pBoard[7 * 8] = teamB[0] | PieceE.Black;
            m_pBoard[7] = teamW[7] | PieceE.White;
            m_pBoard[7 * 8 + 7] = teamB[7] | PieceE.Black;
            m_pBoard[1] = teamW[1] | PieceE.White;
            m_pBoard[7 * 8 + 1] = teamB[1] | PieceE.Black;
            m_pBoard[6] = teamW[6] | PieceE.White;

            m_pBoard[7 * 8 + 6] = teamB[6] | PieceE.Black;
            m_pBoard[2] = teamW[2] | PieceE.White;
            m_pBoard[7 * 8 + 2] = teamB[2] | PieceE.Black;
            m_pBoard[5] = teamW[5] | PieceE.White;
            m_pBoard[7 * 8 + 5] = teamB[5] | PieceE.Black;
            m_pBoard[3] = teamW[3] | PieceE.White;
            m_pBoard[7 * 8 + 3] = teamB[3] | PieceE.Black;
            m_pBoard[4] = teamW[4] | PieceE.White;
            m_pBoard[7 * 8 + 4] = teamB[4] | PieceE.Black;




            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }

        /// <summary>Chess board</summary>
        /// 63 62 61 60 59 58 57 56
        /// 55 54 53 52 51 50 49 48
        /// 47 46 45 44 43 42 41 40
        /// 39 38 37 36 35 34 33 32
        /// 31 30 29 28 27 26 25 24
        /// 23 22 21 20 19 18 17 16
        /// 15 14 13 12 11 10 9  8
        /// 7  6  5  4  3  2  1  0

        public void ResetBoardTest()
        {
            
            m_pBoard[5] = PieceE.King | PieceE.White;
            m_pBoard[12] = PieceE.Tiger | PieceE.White;

            m_pBoard[19] = PieceE.Pawn | PieceE.Black;
            m_pBoard[61] = PieceE.Queen | PieceE.Black;
            m_pBoard[21] = PieceE.Pawn | PieceE.Black;
            m_pBoard[30] = PieceE.Pawn | PieceE.Black;
            m_pBoard[63] = PieceE.King | PieceE.Black;

            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }

        public void ResetBoardTest2()
        {

            m_pBoard[5] = PieceE.King | PieceE.White;
            m_pBoard[56] = PieceE.Tiger | PieceE.Black;

            m_pBoard[19] = PieceE.Pawn | PieceE.Black;
            m_pBoard[42] = PieceE.Queen | PieceE.White;
            m_pBoard[21] = PieceE.Pawn | PieceE.Black;
            m_pBoard[30] = PieceE.Pawn | PieceE.Black;
            m_pBoard[63] = PieceE.King | PieceE.Black;

            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }

        public void ResetBoardTestCheckMate()
        {

            m_pBoard[56] = PieceE.King | PieceE.Black;
            m_pBoard[48] = PieceE.Pawn | PieceE.Black;
            m_pBoard[49] = PieceE.Pawn | PieceE.Black;
            m_pBoard[50] = PieceE.Pawn | PieceE.Black;
            m_pBoard[0] = PieceE.King | PieceE.White;
            m_pBoard[7] = PieceE.Queen | PieceE.White;

            
            ResetInitialBoardInfo(PlayerColorE.White,
                                true /*Standard board*/,
                                BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                0 /*iEnPassant*/);
        }

        public void ResetBoardTestCheckMateElephant()
        {

            m_pBoard[56] = PieceE.King | PieceE.Black;
            m_pBoard[8] = PieceE.Pawn | PieceE.White;
            m_pBoard[9] = PieceE.Pawn | PieceE.White;
            m_pBoard[10] = PieceE.Pawn | PieceE.White;
            m_pBoard[0] = PieceE.King | PieceE.White;
            m_pBoard[24] = PieceE.Elephant | PieceE.White;
            m_pBoard[63] = PieceE.Queen | PieceE.Black;

            ResetInitialBoardInfo(PlayerColorE.Black,
                                true /*Standard board*/,
                                BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                0 /*iEnPassant*/);
        }



        public void ResetBoardTestCheckMate2()
        {

            m_pBoard[56] = PieceE.King | PieceE.Black;
            m_pBoard[48] = PieceE.Pawn | PieceE.Black;
            m_pBoard[49] = PieceE.Pawn | PieceE.Black;
            m_pBoard[50] = PieceE.Pawn | PieceE.Black;
            m_pBoard[51] = PieceE.Pawn | PieceE.Black;
            m_pBoard[52] = PieceE.Pawn | PieceE.Black;
            m_pBoard[7] = PieceE.Queen | PieceE.White;


            m_pBoard[11] = PieceE.Queen | PieceE.Black;
            m_pBoard[8] = PieceE.Pawn | PieceE.White;
            m_pBoard[9] = PieceE.Pawn | PieceE.White;
            m_pBoard[10] = PieceE.Pawn | PieceE.White;

            m_pBoard[0] = PieceE.King | PieceE.White;




            

            //mirar perque depend d'aquesta funcio
            ResetInitialBoardInfo(PlayerColorE.White,
                                true /*Standard board*/,
                                BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                0 /*iEnPassant*/);
        }


        public void ResetBoardTestInvasion() // el que no va
        {

            m_pBoard[24] = PieceE.King | PieceE.White;

            m_pBoard[33] = PieceE.Pawn | PieceE.White;

            //m_pBoard[56] = PieceE.Tiger | PieceE.Black;


            m_pBoard[19] = PieceE.Pawn | PieceE.Black;
            // m_pBoard[42] = PieceE.Queen | PieceE.White;
            m_pBoard[21] = PieceE.Pawn | PieceE.Black;
            m_pBoard[30] = PieceE.Pawn | PieceE.Black;
            m_pBoard[39] = PieceE.King | PieceE.Black;

            ResetInitialBoardInfo(PlayerColorE.Black,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }


        public void ResetBoardTestInvasion2()
        {

            m_pBoard[24] = PieceE.King | PieceE.White;

            m_pBoard[33] = PieceE.Pawn | PieceE.White;

            //m_pBoard[56] = PieceE.Tiger | PieceE.Black;


            m_pBoard[35] = PieceE.Pawn | PieceE.Black;
           // m_pBoard[42] = PieceE.Queen | PieceE.White;
            m_pBoard[37] = PieceE.Pawn | PieceE.Black;
            m_pBoard[46] = PieceE.Pawn | PieceE.Black;
            m_pBoard[39] = PieceE.King | PieceE.Black;

            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }

        public void ResetBoardTestInvasion3()
        {

            m_pBoard[24] = PieceE.King | PieceE.White;

            m_pBoard[8] = PieceE.Pawn | PieceE.White;
            m_pBoard[9] = PieceE.Pawn | PieceE.White;
            m_pBoard[10] = PieceE.Pawn | PieceE.White;
            m_pBoard[0] = PieceE.Queen | PieceE.White;
            m_pBoard[1] = PieceE.Bishop | PieceE.White;

            


            m_pBoard[55] = PieceE.Pawn | PieceE.Black;
            // m_pBoard[42] = PieceE.Queen | PieceE.White;
            m_pBoard[54] = PieceE.Pawn | PieceE.Black;
            m_pBoard[53] = PieceE.Pawn | PieceE.Black;
            m_pBoard[39] = PieceE.King | PieceE.Black;

            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }



        public void ResetBoardTestElephant()
        {

            m_pBoard[5] = PieceE.King | PieceE.White;
            m_pBoard[24] = PieceE.Elephant | PieceE.White;

            m_pBoard[8] = PieceE.Pawn | PieceE.Black;
           // m_pBoard[61] = PieceE.Queen | PieceE.Black;
            m_pBoard[16] = PieceE.Pawn | PieceE.Black;
            m_pBoard[0] = PieceE.Pawn | PieceE.Black;
            //m_pBoard[21] = PieceE.Pawn | PieceE.Black;
            //m_pBoard[30] = PieceE.Pawn | PieceE.Black;
            m_pBoard[63] = PieceE.King | PieceE.Black;

            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }

        public void ResetBoardTestElephantKill()
        {

            m_pBoard[5] = PieceE.King | PieceE.White;
            m_pBoard[24] = PieceE.Elephant | PieceE.White;
            m_pBoard[32] = PieceE.Queen | PieceE.Black;

            m_pBoard[8] = PieceE.Pawn | PieceE.Black;
            // m_pBoard[61] = PieceE.Queen | PieceE.Black;
            m_pBoard[16] = PieceE.Pawn | PieceE.Black;
            m_pBoard[0] = PieceE.Pawn | PieceE.Black;
            //m_pBoard[21] = PieceE.Pawn | PieceE.Black;
            //m_pBoard[30] = PieceE.Pawn | PieceE.Black;
            m_pBoard[63] = PieceE.King | PieceE.Black;

            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }


        public void ResetBoardTestEmpowered()
        {
            m_pBoard[5] = PieceE.King | PieceE.White;
            m_pBoard[2] = PieceE.Queen | PieceE.White;
            m_pBoard[9] = PieceE.EmpoweredKnight | PieceE.White;
            m_pBoard[4] = PieceE.EmpoweredRook | PieceE.White;
            m_pBoard[56] = PieceE.Pawn | PieceE.Black;
            m_pBoard[55] = PieceE.Pawn | PieceE.Black;
            m_pBoard[54] = PieceE.Pawn | PieceE.Black;
            m_pBoard[63] = PieceE.Rook | PieceE.White;

           // m_pBoard[61] = PieceE.EmpoweredKnight | PieceE.Black;
            //m_pBoard[62] = PieceE.EmpoweredRook | PieceE.Black;
            m_pBoard[63] = PieceE.Queen | PieceE.Black;
            m_pBoard[59] = PieceE.King | PieceE.Black;

            m_pBoard[0] = PieceE.Pawn ;
            m_pBoard[24] = PieceE.Pawn ;
            m_pBoard[7] = PieceE.Queen | PieceE.White;
            m_pBoard[1] = PieceE.Pawn | PieceE.White;
        }

        public void ResetBoardTestElephant2()
        {

            m_pBoard[5] = PieceE.King | PieceE.White;
            m_pBoard[63] = PieceE.King | PieceE.Black;

            m_pBoard[24] = PieceE.Elephant | PieceE.White;
            m_pBoard[48] = PieceE.Pawn | PieceE.White;
            m_pBoard[40] = PieceE.Pawn | PieceE.Black;
            m_pBoard[32] = PieceE.Queen | PieceE.Black;



            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }

        public void ResetBoardTestElephant3()
        {

            m_pBoard[5] = PieceE.King | PieceE.White;
            m_pBoard[63] = PieceE.King | PieceE.Black;

            m_pBoard[56] = PieceE.Elephant | PieceE.Black;
            m_pBoard[48] = PieceE.Pawn | PieceE.Black;
            m_pBoard[40] = PieceE.Pawn | PieceE.White;
            m_pBoard[32] = PieceE.Queen | PieceE.White;



            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }

        public void ResetBoardTestKing()
        {

            m_pBoard[47] = PieceE.King | PieceE.White;
            m_pBoard[63] = PieceE.King | PieceE.Black;
            m_pBoard[38] = PieceE.Queen | PieceE.White;

            m_pBoard[11] = PieceE.Pawn | PieceE.Black;
            m_pBoard[12] = PieceE.Pawn | PieceE.White;
            m_pBoard[13] = PieceE.Pawn | PieceE.Black;




            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }

        /// <summary>Chess board</summary>
        /// 63 62 61 60 59 58 57 56
        /// 55 54 53 52 51 50 49 48
        /// 47 46 45 44 43 42 41 40
        /// 39 38 37 36 35 34 33 32
        /// 31 30 29 28 27 26 25 24
        /// 23 22 21 20 19 18 17 16
        /// 15 14 13 12 11 10 9  8
        /// 7  6  5  4  3  2  1  0


        public void ResetBoardTestAmazon()
        {

            m_pBoard[0] = PieceE.King | PieceE.White;
            m_pBoard[63] = PieceE.King | PieceE.Black;
            m_pBoard[28] = PieceE.Amazon | PieceE.White;

            m_pBoard[43] = PieceE.Pawn | PieceE.White;
            m_pBoard[45] = PieceE.Pawn | PieceE.White;
            m_pBoard[13] = PieceE.Pawn | PieceE.Black;




            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }

        /// <summary>Chess board</summary>
        /// 63 62 61 60 59 58 57 56
        /// 55 54 53 52 51 50 49 48
        /// 47 46 45 44 43 42 41 40
        /// 39 38 37 36 35 34 33 32
        /// 31 30 29 28 27 26 25 24
        /// 23 22 21 20 19 18 17 16
        /// 15 14 13 12 11 10 9  8
        /// 7  6  5  4  3  2  1  0


        public void ResetBoardTestTiger()
        {

            m_pBoard[0] = PieceE.King | PieceE.White;
            m_pBoard[63] = PieceE.King | PieceE.Black;
            m_pBoard[35] = PieceE.Tiger | PieceE.White;

            m_pBoard[11] = PieceE.Pawn | PieceE.Black;
            m_pBoard[12] = PieceE.Pawn | PieceE.White;
            m_pBoard[13] = PieceE.Pawn | PieceE.Black;




            ResetInitialBoardInfo(PlayerColorE.White,
                                  true /*Standard board*/,
                                  BoardStateMaskE.BLCastling | BoardStateMaskE.BRCastling | BoardStateMaskE.WLCastling | BoardStateMaskE.WRCastling,
                                  0 /*iEnPassant*/);
        }






        public PieceE[] TeamClassic()
        {

            PieceE[] teamC = new PieceE[8];
            teamC[0] = PieceE.Rook;
            teamC[1] = PieceE.Knight;
            teamC[2] = PieceE.Bishop;
            teamC[3] = PieceE.King;
            teamC[4] = PieceE.Queen;
            teamC[5] = PieceE.Bishop;
            teamC[6] = PieceE.Knight;
            teamC[7] = PieceE.Rook;

            return teamC;

        }

        public PieceE[] TeamCapablanca()
        {

            PieceE[] teamC = new PieceE[8];
            teamC[0] = PieceE.Chancellor;
            teamC[1] = PieceE.Knight;
            teamC[2] = PieceE.Bishop;
            teamC[3] = PieceE.King;
            teamC[4] = PieceE.Ferz;
            teamC[5] = PieceE.Archbishop;
            teamC[6] = PieceE.Knight;
            teamC[7] = PieceE.Rook;

            return teamC;

        }

        public PieceE[] TeamAnimals()
        {

            PieceE[] teamC = new PieceE[8];
            teamC[0] = PieceE.Elephant;
            teamC[1] = PieceE.Knight;
            teamC[2] = PieceE.Tiger;
            teamC[3] = PieceE.King;
            teamC[4] = PieceE.Chancellor;
            teamC[5] = PieceE.Tiger;
            teamC[6] = PieceE.Knight;
            teamC[7] = PieceE.Elephant;

            return teamC;

        }

        public PieceE[] TeamEmpowered()
        {

            PieceE[] teamC = new PieceE[8];
            teamC[0] = PieceE.EmpoweredRook;
            teamC[1] = PieceE.EmpoweredKnight;
            teamC[2] = PieceE.EmpoweredBishop;
            teamC[3] = PieceE.King;
            teamC[4] = PieceE.EmpoweredQueen;
            teamC[5] = PieceE.EmpoweredBishop;
            teamC[6] = PieceE.EmpoweredKnight;
            teamC[7] = PieceE.EmpoweredRook;

            return teamC;

        }


        public PieceE[] TeamAmazon()
        {

            PieceE[] team = new PieceE[8];
            team[0] = PieceE.Wazir;
            team[1] = PieceE.Knight;// ancient PieceE.Unicorn; 
            team[2] = PieceE.Ferz;
            team[3] = PieceE.King;
            team[4] = PieceE.Amazon;
            team[5] = PieceE.Ferz;
            team[6] = PieceE.Knight;
            team[7] = PieceE.Wazir;

            return team;

        }

        public PieceE[] TeamChaturanga()
        {

            PieceE[] team = new PieceE[8];
            team[0] = PieceE.Rook;
            team[1] = PieceE.Knight;
            team[2] = PieceE.Gaja;
            team[3] = PieceE.King;
            team[4] = PieceE.EmpoweredQueen;
            team[5] = PieceE.Gaja;
            team[6] = PieceE.Knight;
            team[7] = PieceE.Rook;

            return team;

        }

        public PieceE[] TeamJungle()
        {

            PieceE[] team = new PieceE[8];
            team[0] = PieceE.Hipo;
            team[1] = PieceE.Zebra;
            team[2] = PieceE.Snake;
            team[3] = PieceE.King;
            team[4] = PieceE.Lion;
            team[5] = PieceE.Snake;
            team[6] = PieceE.Zebra;
            team[7] = PieceE.Hipo;

            return team;

        }

        public PieceE[] TeamReaper()
        {

            PieceE[] team = new PieceE[8];
            team[0] = PieceE.Ghost;
            team[1] = PieceE.Knight;
            team[2] = PieceE.Bishop;
            team[3] = PieceE.King;
            team[4] = PieceE.Reaper;
            team[5] = PieceE.Bishop;
            team[6] = PieceE.Knight;
            team[7] = PieceE.Ghost;

            return team;

        }


        public PieceE[] TeamNemesis()
        {

            PieceE[] team = new PieceE[8];
            team[0] = PieceE.Rook;
            team[1] = PieceE.Knight;
            team[2] = PieceE.Bishop;
            team[3] = PieceE.King;
            team[4] = PieceE.Nemesis;
            team[5] = PieceE.Bishop;
            team[6] = PieceE.Knight;
            team[7] = PieceE.Rook;

            return team;

        }

        public PieceE[] TeamShogi()
        {

            PieceE[] team = new PieceE[8];
            team[0] = PieceE.Lancer;
            team[1] = PieceE.ShogiHorse;
            team[2] = PieceE.SilverGeneral;
            team[3] = PieceE.King;
            team[4] = PieceE.GoldGeneral;
            team[5] = PieceE.SilverGeneral;
            team[6] = PieceE.ShogiHorse;
            team[7] = PieceE.Lancer;

          //  team[9] = PieceE.Rook;

            //team[14] = PieceE.Bishop;


            return team;

        }

        public PieceE[] TeamShogiAdapted()
        {

            PieceE[] team = new PieceE[8];
            team[0] = PieceE.Dragon;
            team[1] = PieceE.ShogiHorse;
            team[2] = PieceE.DragonHorse;
            team[3] = PieceE.King;
            team[4] = PieceE.GoldGeneral;
            team[5] = PieceE.DragonHorse;
            team[6] = PieceE.ShogiHorse;
            team[7] = PieceE.Dragon;

            //  team[9] = PieceE.Rook;

            //team[14] = PieceE.Bishop;


            return team;

        }

        public PieceE[] TeamDimensional()
        {

            PieceE[] team = new PieceE[8];
            team[0] = PieceE.DimensionalRook;
            team[1] = PieceE.DimensionalKnight;
            team[2] = PieceE.DimensionalBishop;
            team[3] = PieceE.King;
            team[4] = PieceE.EmpoweredQueen;
            team[5] = PieceE.DimensionalBishop;
            team[6] = PieceE.DimensionalKnight;
            team[7] = PieceE.DimensionalRook;

            //  team[9] = PieceE.Rook;

            //team[14] = PieceE.Bishop;


            return team;

        }









        // todo refactor
        /* public void CheckAdjacent(int p, int dim)
         {

             int u, d, l, r;
             if (p % dim != 0)
             {
                 r = p - 1;
                 Console.WriteLine(r);
             }
             if (p % dim != dim)
             {
                 l = p + 1;
                 Console.WriteLine(l);
             }
             if (p / dim != 0)
             {
                 d = p - dim;
                 Console.WriteLine(d);
             }
             if (p / dim != dim - 1)
             {
                 u = p + dim;
                 Console.WriteLine(u);
             }

         }*/


        // returns -1 if no possible
        public int[] CheckAdjacent(int p, int dim)
        {

            int[] arr = new int[4];

            int u, d, l, r;

            if (p / dim != dim - 1)
            {
                u = p + dim;
                //Console.WriteLine(u);
                arr[0] = u;
            }
            else {
                arr[0] = -1;
            }

            if (p % dim != dim-1)
            {
                l = p + 1;
                //Console.WriteLine(l);
                arr[1] = l;
            }
            else {
                arr[1] = -1;
            }

            if (p / dim != 0)
            {
                d = p - dim;
                //Console.WriteLine(d);
                arr[2] = d;
            }
            else {
                arr[2] = -1;
            }



            if (p % dim != 0)
            {
                r = p - 1;
               // Console.WriteLine(r);
                arr[3] = r;
            }
            else {
                arr[3] = -1;
            }
            return arr;

        }

        private bool thisIndexExistInBoard(int index)
        {
            if ((index >= 0) && (index <= 63))
                return true;
            else
                return false;

        }

        private bool isAtSnakeRange(int pos, int team)
        {
            for(int i = pos-9; i< pos-7; i++)
            {
                if (thisIndexExistInBoard(i))
                {
                    if (m_pBoard[i] == PieceE.Snake)
                        return true;
                }
            }

            if (thisIndexExistInBoard(pos-1))
            {
                if (m_pBoard[pos-1] == PieceE.Snake)
                    return true;
            }

            if (thisIndexExistInBoard(pos + 1))
            {
                if (m_pBoard[pos + 1] == PieceE.Snake)
                    return true;
            }


            for (int i = pos +7; i < pos +9; i++)
            {
                if (thisIndexExistInBoard(i))
                {
                    if (m_pBoard[i] == PieceE.Snake)
                        return true;
                }
            }

            // if nothing found
            return false;


        }


        private PieceE KnightAdjacent(int[] arr, int color)
        {
            
            bool biFound = false;
            bool roFound = false;

           for (int i = 0; i < 4; i++)
            {
                if (arr[i] != -1)
                {
                    if (m_pBoard[arr[i]] == ( color !=1 ? PieceE.EmpoweredBishop :PieceE.EmpoweredBishop|PieceE.Black))
                        biFound = true;
                    if (m_pBoard[arr[i]] == (color != 1 ? PieceE.EmpoweredRook : PieceE.EmpoweredRook | PieceE.Black))
                        roFound = true;
                }
            }

            if (biFound && roFound)
                return PieceE.Amazon;

            if (!biFound && roFound)
                return PieceE.Chancellor;
            if (biFound && !roFound)
                return PieceE.Archbishop;
            // otherwise
            return PieceE.Knight;

        }


        private PieceE BishopAdjacent(int[] arr, int color)
        {

            bool knFound = false;
            bool roFound = false;

            for (int i = 0; i < 4; i++)
            {
                if (arr[i] != -1)
                {
                    if (m_pBoard[arr[i]] == (color != 1 ? PieceE.EmpoweredKnight : PieceE.EmpoweredKnight | PieceE.Black))
                        knFound = true;
                    if (m_pBoard[arr[i]] == (color != 1 ? PieceE.EmpoweredRook : PieceE.EmpoweredRook | PieceE.Black))
                        roFound = true;
                }
            }

            if (knFound && roFound)
                return PieceE.Amazon;

            if (!knFound && roFound)
                return PieceE.Queen;
            if (knFound && !roFound)
                return PieceE.Archbishop;
            // otherwise
            return PieceE.Bishop;

        }

        private PieceE RookAdjacent(int[] arr, int color)
        {

            bool biFound = false;
            bool knFound = false;

            for (int i = 0; i < 4; i++)
            {
                if (arr[i] != -1)
                {
                    if (m_pBoard[arr[i]] == (color != 1 ? PieceE.EmpoweredBishop : PieceE.EmpoweredBishop | PieceE.Black))
                        biFound = true;
                    if (m_pBoard[arr[i]] == (color != 1 ? PieceE.EmpoweredKnight : PieceE.EmpoweredKnight | PieceE.Black))
                        knFound = true;
                }
            }

            if (biFound && knFound)
                return PieceE.Amazon;

            if (!biFound &&knFound)
                return PieceE.Chancellor;
            if (biFound && !knFound)
                return PieceE.Queen;
            // otherwise
            return PieceE.Rook;

        }



        // endPos nomes serveix per esbrinar la direcció cap on es mou
        public byte[] MaxPosElephant(byte startPos, byte endPos)
        {
            //Console.WriteLine
            byte dim = 8;
            byte[] arrayPos;

           

            if (endPos >= startPos + 8)
            { // up

                int rowIndex = startPos / 8;
                int lenght;
                if (rowIndex <= 4)
                    lenght = 3;
                else
                    lenght = 7 - rowIndex;

                arrayPos = new byte[lenght];

                for (int i = 0; i < lenght; i++)
                    arrayPos[i] = (byte)(startPos +dim *(i + 1));

               // arrayPos = new byte[3];
                //arrayPos[0] =(byte) (startPos + dim);
                //arrayPos[1] = (byte)(startPos + dim * 2);
                //arrayPos[2] = (byte)(startPos + dim * 3);

               
            }
            else if (endPos > startPos)
            {  // left
                

                int colIndex = startPos % 8;
                int lenght;
                if (colIndex <= 4)
                    lenght = 3;
                else
                    lenght = 7 - colIndex;

                arrayPos = new byte[lenght];

                for (int i = 0; i < lenght; i++)
                    arrayPos[i] = (byte)(startPos +  i + 1);


            }
            else if (endPos <= startPos - 8)
            { // down
                int lenght; // = (startPos - endPos) / 8;

                if (startPos / 8 >= 3)
                    lenght = 3;
                else
                    lenght = startPos / 8;

                arrayPos = new byte[lenght];
               
                for (int i = 0; i < lenght; i++)
                    arrayPos[i] = (byte)(startPos -dim*(i+1));
               
            }
            else { // right

                int lenght;
                if (startPos % 8 >= 3)
                    lenght = 3;
                else
                    lenght = startPos % 8;

               
                arrayPos = new byte[lenght];

                for (int i = 0; i < lenght; i++)
                    arrayPos[i] = (byte)(startPos - (i + 1));

                /**arrayPos = new byte[3];
                
                arrayPos[1] = (byte)(startPos - 2);
                arrayPos[2] = (byte)(startPos - 3);*/
            }

            return arrayPos;

        }

        public byte[] MaxPosHipo(byte startPos, byte endPos)
        {
            //Console.WriteLine
            byte dim = 8;
            byte[] arrayPos = new byte[4];

            if (endPos >= startPos + 8)
            { // up
                arrayPos[0] = (byte)(startPos + dim);
                arrayPos[1] = (byte)(startPos + dim * 2);
                arrayPos[2] = (byte)(startPos + dim * 3);
                arrayPos[3] = (byte)(startPos + dim * 4);

            }
            else if (endPos > startPos)
            {  // left
                arrayPos[0] = (byte)(startPos + 1);
                arrayPos[1] = (byte)(startPos + 2);
                arrayPos[2] = (byte)(startPos + 3);
                arrayPos[3] = (byte)(startPos + 4);

            }
            else if (endPos <= startPos - 8)
            { // down

                arrayPos[0] = (byte)(startPos - dim);
                arrayPos[1] = (byte)(startPos - dim * 2);
                arrayPos[2] = (byte)(startPos - dim * 3);
                arrayPos[3] = (byte)(startPos - dim * 4);

            }
            else { // right
                arrayPos[0] = (byte)(startPos - 1);
                arrayPos[1] = (byte)(startPos - 2);
                arrayPos[2] = (byte)(startPos - 3);
                arrayPos[3] = (byte)(startPos - 4);
            }

            return arrayPos;

        }



        private int MinLinePos(int pos)
        {
            int dim = 8;
            return(pos / dim * dim);
        }

        private int MaxLinePos(int pos)
        {
            int dim = 8;
            return(pos / dim * dim + dim - 1);
        }


        private int MaxYPos(int pos)
        {
            int dim = 8;
            return (pos % 8 + (8 * 7));
        }

        private int MinYPos(int pos)
        {
            int dim = 8;
            return (pos % dim);
        }

    









    } // Class ChessBoard

    /// <summary>Chess exception</summary>
    [Serializable]
    public class ChessException : System.Exception {
        /// <summary>
        /// Class constructor
        /// </summary>
        public ChessException() : base() {
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="strError"> Error</param>
        public ChessException(string strError) : base(strError) {
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="strError"> Error</param>
        /// <param name="ex">       Inner exception</param>
        public ChessException(string strError, Exception ex) : base(strError, ex) {
        }

        /// <summary>
        /// Serialization Ctor
        /// </summary>
        /// <param name="info">     Serialization info</param>
        /// <param name="context">  Streaming context</param>
        protected ChessException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    } // Class ChessException

} // Namespace
