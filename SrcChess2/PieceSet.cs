using System.Windows.Controls;

namespace SrcChess2 {
    /// <summary>
    /// Defines a set of chess pieces
    /// </summary>
    public abstract class PieceSet {

        /// <summary>
        /// List of standard pieces
        /// </summary>
        protected enum ChessPiece {
            /// <summary>No Piece</summary>
            None            = -1,
            /// <summary>Black Pawn</summary>
            Black_Pawn      = 0,
            /// <summary>Black Rook</summary>
            Black_Rook      = 1,
            /// <summary>Black Bishop</summary>
            Black_Bishop    = 2,
            /// <summary>Black Knight</summary>
            Black_Knight    = 3,
            /// <summary>Black Queen</summary>
            Black_Queen     = 4,
            /// <summary>Black King</summary>
            Black_King      = 5,
            /// <summary>White Pawn</summary>
            White_Pawn      = 6,
            /// <summary>White Rook</summary>
            White_Rook      = 7,
            /// <summary>White Bishop</summary>
            White_Bishop    = 8,
            /// <summary>White Knight</summary>
            White_Knight    = 9,
            /// <summary>White Queen</summary>
            White_Queen     = 10,
            /// <summary>White King</summary>
            White_King      = 11,
            // new pieces
            Black_Chancellor = 12,

            Black_Archbishop = 13,

            Black_EmpoweredQueen = 14,

            White_Chancellor = 15,

            White_Archbishop = 16,

            White_EmpoweredQueen = 17,

            White_Tiger = 18,


            White_Elephant = 19,


            Black_Tiger = 20,


            Black_Elephant = 21,

            Black_Amazon =22,

            Black_Ferz =23,

            Black_Wazir = 24,

            Black_CrazyHorse = 25,

            White_Amazon = 26,

            White_Ferz = 27,

            White_Wazir = 28,

            White_CrazyHorse = 29,

            Black_AmazonPawn = 30,

            White_AmazonPawn = 31,

            Black_Gaja = 32,

            White_Gaja = 33,

            
            Black_EmpoweredKnight = 34,
            White_EmpoweredKnight = 35,

            Black_EmpoweredBishop = 36,
            White_EmpoweredBishop = 37,

            Black_EmpoweredRook = 38,
            White_EmpoweredRook = 39,

            Black_Zebra = 40,
            White_Zebra = 41,

            Black_Camel = 42,
            White_Camel = 43,

            Black_Unicorn = 46,
            White_Unicorn = 47,

            Black_Lion = 44,
            White_Lion = 45,

            Black_Buffalo = 48,
            White_Buffalo = 49,

            Black_Nemesis= 50,
            White_Nemesis = 51,

            Black_Reaper = 52,
            White_Reaper = 53,

            Black_Ghost = 54,
            White_Ghost = 55,

            Black_Lancer = 56,
            White_Lancer = 57,

            Black_ShogiHorse = 58,
            White_ShogiHorse = 59,

            Black_SilverGeneral = 60,
            White_SilverGeneral = 61,

            Black_GoldGeneral = 62,
            White_GoldGeneral = 63,

            Black_Snake = 64,
            White_Snake = 65,

            Black_Hipo = 66,
            White_Hipo = 67,

            Black_Dragon = 68,
            White_Dragon = 69,

            Black_DragonHorse = 70,
            White_DragonHorse = 71,

            Black_NemesisPawn = 72,
            White_NemesisPawn = 73,

            Black_DimensionalKnight = 74,
            White_DimensionalKnight = 75,

            Black_DimensionalBishop = 76,
            White_DimensionalBishop = 77,

            Black_DimensionalRook = 78,
            White_DimensionalRook = 79,

            Black_Raja = 80,
            White_Raja = 81,

            //Black_Giraffe = 46,
            //White_Giraffe = 47,

        };

        
        /// <summary>Name of the piece set</summary>
        public      string              Name { get; private set; }

        /// <summary>
        /// Class Ctor
        /// </summary>
        /// <param name="strName">  Piece set Name</param>
        protected PieceSet(string strName) {
            Name                = strName;
        }

        /// <summary>
        /// Transform a ChessBoard piece into a ChessPiece enum
        /// </summary>
        /// <param name="ePiece"></param>
        /// <returns></returns>
        private static ChessPiece GetChessPieceFromPiece(ChessBoard.PieceE ePiece) {
            ChessPiece  eRetVal;

            switch (ePiece) {
            case ChessBoard.PieceE.Pawn | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Pawn;
                break;
            case ChessBoard.PieceE.Knight | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Knight;
                break;
            case ChessBoard.PieceE.Bishop | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Bishop;
                break;
            case ChessBoard.PieceE.Rook | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Rook;
                break;
            case ChessBoard.PieceE.Queen | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Queen;
                break;
            case ChessBoard.PieceE.King | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_King;
                break;
            case ChessBoard.PieceE.Chancellor | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Chancellor;
                break;
            case ChessBoard.PieceE.Archbishop | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Archbishop;
                break;
            case ChessBoard.PieceE.EmpoweredQueen | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_EmpoweredQueen;
                break;
            case ChessBoard.PieceE.Tiger | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Tiger;
                break;
            case ChessBoard.PieceE.Elephant | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Elephant;
                break;
            case ChessBoard.PieceE.Amazon | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Amazon;
                break;
            case ChessBoard.PieceE.Ferz | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Ferz;
                break;
            case ChessBoard.PieceE.Wazir | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Wazir;
                break;
            case ChessBoard.PieceE.CrazyHorse | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_CrazyHorse;
                break;
            case ChessBoard.PieceE.AmazonPawn | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_AmazonPawn;
                break;
            case ChessBoard.PieceE.Gaja | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Gaja;
                break;
            case ChessBoard.PieceE.Zebra | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Zebra;
                break;
            case ChessBoard.PieceE.Camel | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Camel;
                break;
            case ChessBoard.PieceE.Unicorn | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Unicorn;
                break;
            case ChessBoard.PieceE.Lion | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Lion;
                break;
            case ChessBoard.PieceE.Buffalo | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Buffalo;
                break;
            case ChessBoard.PieceE.Nemesis | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Nemesis;
                break;
            case ChessBoard.PieceE.Reaper | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Reaper;
                break;
            case ChessBoard.PieceE.Ghost | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Ghost;
                break;
            case ChessBoard.PieceE.Lancer | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Lancer;
                break;
            case ChessBoard.PieceE.ShogiHorse | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_ShogiHorse;
                break;
            case ChessBoard.PieceE.GoldGeneral | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_GoldGeneral;
                break;
            case ChessBoard.PieceE.SilverGeneral | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_SilverGeneral;
                break;
            case ChessBoard.PieceE.Snake| ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Snake;
                break;
            case ChessBoard.PieceE.Hipo | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Hipo;
                break;
            case ChessBoard.PieceE.Dragon | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Dragon;
                break;
            case ChessBoard.PieceE.DragonHorse | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_DragonHorse;
                break;
            case ChessBoard.PieceE.NemesisPawn | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_NemesisPawn;
                break;


                case ChessBoard.PieceE.Pawn | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Pawn;
                break;
            case ChessBoard.PieceE.Knight | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Knight;
                break;
            case ChessBoard.PieceE.Bishop | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Bishop;
                break;
            case ChessBoard.PieceE.Rook | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Rook;
                break;
            case ChessBoard.PieceE.Queen | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Queen;
                break;
            case ChessBoard.PieceE.King | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_King;
                break;
            case ChessBoard.PieceE.Chancellor | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Chancellor;
                break;
            case ChessBoard.PieceE.Archbishop | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Archbishop;
                break;
            case ChessBoard.PieceE.EmpoweredQueen | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_EmpoweredQueen;
                break;
            case ChessBoard.PieceE.Tiger | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Tiger;
                break;
            case ChessBoard.PieceE.Elephant | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Elephant;
                break;
            case ChessBoard.PieceE.Amazon | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Amazon;
                break;
            case ChessBoard.PieceE.Ferz | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.White_Ferz;
                break;
            case ChessBoard.PieceE.Wazir | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Wazir;
                break;
            case ChessBoard.PieceE.CrazyHorse | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_CrazyHorse;
                break;
            case ChessBoard.PieceE.AmazonPawn | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_AmazonPawn;
                break;
            case ChessBoard.PieceE.Gaja | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Gaja;
                break;
            case ChessBoard.PieceE.Zebra | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Zebra;
                break;
            case ChessBoard.PieceE.Camel | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Camel;
                break;
            case ChessBoard.PieceE.EmpoweredKnight | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_EmpoweredKnight;
                break;
            case ChessBoard.PieceE.EmpoweredKnight | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_EmpoweredKnight;
                break;
            case ChessBoard.PieceE.EmpoweredBishop | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_EmpoweredBishop;
                break;
            case ChessBoard.PieceE.EmpoweredBishop | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_EmpoweredBishop;
                break;
            case ChessBoard.PieceE.EmpoweredRook | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_EmpoweredRook;
                break;
            case ChessBoard.PieceE.EmpoweredRook | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_EmpoweredRook;
                break;
            case ChessBoard.PieceE.DimensionalKnight | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_DimensionalKnight;
                break;
            case ChessBoard.PieceE.DimensionalKnight | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_DimensionalKnight;
                break;
            case ChessBoard.PieceE.DimensionalBishop | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_DimensionalBishop;
                break;
            case ChessBoard.PieceE.Raja | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Raja;
                break;
            case ChessBoard.PieceE.DimensionalBishop | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_DimensionalBishop;
                break;
            case ChessBoard.PieceE.DimensionalRook | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_DimensionalRook;
                break;
            case ChessBoard.PieceE.DimensionalRook | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_DimensionalRook;
                break;
            case ChessBoard.PieceE.Raja | ChessBoard.PieceE.White:
                eRetVal = ChessPiece.White_Raja;
                break;
            case ChessBoard.PieceE.Unicorn | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Unicorn;
                break;
            case ChessBoard.PieceE.Lion | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Lion;
                break;
            case ChessBoard.PieceE.Buffalo | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Buffalo;
                break;
            case ChessBoard.PieceE.Nemesis | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Nemesis;
                break;
            case ChessBoard.PieceE.Reaper | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Reaper;
                break;
            case ChessBoard.PieceE.Ghost | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Ghost;
                break;
            case ChessBoard.PieceE.Lancer | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Lancer;
                break;
            case ChessBoard.PieceE.ShogiHorse | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_ShogiHorse;
                break;
            case ChessBoard.PieceE.GoldGeneral | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_GoldGeneral;
                break;
            case ChessBoard.PieceE.SilverGeneral | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_SilverGeneral;
                break;
            case ChessBoard.PieceE.Snake | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Snake;
                break;
            case ChessBoard.PieceE.Hipo | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Hipo;
                break;
            case ChessBoard.PieceE.DragonHorse | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_DragonHorse;
                break;
            case ChessBoard.PieceE.Dragon | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_Dragon;
                break;
            case ChessBoard.PieceE.NemesisPawn | ChessBoard.PieceE.Black:
                eRetVal = ChessPiece.Black_NemesisPawn;
                break;


                default:
                eRetVal = ChessPiece.None;
                break;
            }
            return(eRetVal);
        }

        /// <summary>
        /// Load a new piece
        /// </summary>
        /// <param name="eChessPiece">  Chess Piece</param>
        protected abstract UserControl LoadPiece(ChessPiece eChessPiece);

        /// <summary>
        /// Gets the specified piece
        /// </summary>
        /// <param name="ePiece"></param>
        /// <returns>
        /// User control expressing the piece
        /// </returns>
        public UserControl this[ChessBoard.PieceE ePiece] {
            get {
                UserControl userControlRetVal;
                ChessPiece  eChessPiece;

                eChessPiece         = GetChessPieceFromPiece(ePiece);
                userControlRetVal   = (eChessPiece == ChessPiece.None) ? null : LoadPiece(eChessPiece);
                return(userControlRetVal);
            }
        }
    } // Class PieceSet
} // Namespace
