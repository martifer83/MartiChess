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
