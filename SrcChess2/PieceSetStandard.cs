using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Resources;

namespace SrcChess2 {
    /// <summary>
    /// Piece Set included in the assembly
    /// </summary>
    public class PieceSetStandard : PieceSet {
        /// <summary>Base Path of the resource</summary>
        private string  m_strBasePath;

        /// <summary>
        /// Class Ctor
        /// </summary>
        /// <param name="strName">      Piece set Name</param>
        /// <param name="strBasePath">  Base path in the assembly for this piece set</param>
        /// 
        private PieceSetStandard(string strName, string strBasePath) : base(strName) {
            m_strBasePath   = strBasePath;
        }

        /// <summary>
        /// Gets the pieces name as defined in the assembly
        /// </summary>
        /// <param name="ePiece">   Piece</param>
        /// <returns>
        /// Piece name
        /// </returns>
        protected static string NameFromChessPiece(ChessPiece ePiece) {
            string      strRetVal;

            switch (ePiece) {
            case ChessPiece.Black_Pawn:
                strRetVal   = "black pawn";
                break;
            case ChessPiece.Black_Rook:
                strRetVal   = "black rook";
                break;
            case ChessPiece.Black_Bishop:
                strRetVal   = "black bishop";
                break;
            case ChessPiece.Black_Knight:
                strRetVal   = "black knight";
                break;
            case ChessPiece.Black_Queen:
                strRetVal   = "black queen";
                break;
            case ChessPiece.Black_King:
                strRetVal   = "black king";
                break;
            case ChessPiece.Black_Chancellor:
                strRetVal = "black chancellor";
                break;
            case ChessPiece.Black_Archbishop:
                strRetVal = "black archbishop";
                break;
            case ChessPiece.Black_EmpoweredQueen:
                strRetVal = "black empoweredqueen";
                break;
            case ChessPiece.Black_Tiger:
                strRetVal = "Black tiger";
                break;
            case ChessPiece.Black_Elephant:
                strRetVal = "black elephant";
                break;
            case ChessPiece.Black_Ferz:
                strRetVal = "black ferz";
                break;
            case ChessPiece.Black_Wazir:
                strRetVal = "black wazir";
                break;
            case ChessPiece.Black_Amazon:
                strRetVal = "black amazon";
                break;
            case ChessPiece.Black_CrazyHorse:
                strRetVal = "black crazyhorse";
                break;
            case ChessPiece.Black_AmazonPawn:
                strRetVal = "black amazonpawn";
                break;
                case ChessPiece.Black_Gaja:
                    strRetVal = "black gaja";
                    break;
                case ChessPiece.Black_EmpoweredKnight:
                    strRetVal = "black empoweredknight";
                    break;
                case ChessPiece.Black_EmpoweredBishop:
                    strRetVal = "black empoweredbishop";
                    break;
                case ChessPiece.Black_EmpoweredRook:
                    strRetVal = "black empoweredrook";
                    break;
                case ChessPiece.Black_Zebra:
                    strRetVal = "black zebra";
                    break;
                case ChessPiece.Black_Camel:
                    strRetVal = "black camel";
                    break;
                case ChessPiece.Black_Unicorn:
                    strRetVal = "black unicorn";
                    break;

                case ChessPiece.Black_Lion:
                    strRetVal = "black lion";
                    break;
                    /*
                case ChessPiece.Black_Giraffe:
                    strRetVal = "black giraffe";
                    break;
                    */
                case ChessPiece.Black_Buffalo:
                    strRetVal = "black buffalo";
                    break;
                case ChessPiece.Black_Nemesis:
                    strRetVal = "black nemesis";
                    break;
                case ChessPiece.Black_DimensionalKnight:
                    strRetVal = "black dimensionalknight";
                    break;
                case ChessPiece.Black_DimensionalBishop:
                    strRetVal = "black dimensionalbishop";
                    break;
                case ChessPiece.Black_DimensionalRook:
                    strRetVal = "black dimensionalrook";
                    break;

                case ChessPiece.White_Reaper:
                    strRetVal = "white reaper";
                    break;
                case ChessPiece.White_Ghost:
                    strRetVal = "white ghost";
                    break;
                case ChessPiece.White_Lancer:
                    strRetVal = "white lancer";
                    break;
                case ChessPiece.White_ShogiHorse:
                    strRetVal = "white shogihorse";
                    break;
                case ChessPiece.White_GoldGeneral:
                    strRetVal = "white goldgeneral";
                    break;
                case ChessPiece.White_SilverGeneral:
                    strRetVal = "white silvergeneral";
                    break;
                case ChessPiece.White_Snake:
                    strRetVal = "white snake";
                    break;
                case ChessPiece.White_Hipo:
                    strRetVal = "white hipo";
                    break;
                case ChessPiece.White_Dragon:
                    strRetVal = "white dragon";
                    break;
                case ChessPiece.White_DragonHorse:
                    strRetVal = "white dragonhorse";
                    break;
                case ChessPiece.White_NemesisPawn:
                    strRetVal = "white nemesispawn";
                    break;




                case ChessPiece.White_Pawn:
                strRetVal   = "white pawn";
                break;
            case ChessPiece.White_Rook:
                strRetVal   = "white rook";
                break;
            case ChessPiece.White_Bishop:
                strRetVal   = "white bishop";
                break;
            case ChessPiece.White_Knight:
                strRetVal   = "white knight";
                break;
            case ChessPiece.White_Queen:
                strRetVal   = "white queen";
                break;
            case ChessPiece.White_King:
                strRetVal   = "white king";
                break;
            case ChessPiece.White_Chancellor:
                strRetVal = "white chancellor";
                break;
            case ChessPiece.White_Archbishop:
                strRetVal = "white archbishop";
                break;
            case ChessPiece.White_EmpoweredQueen:
                strRetVal = "white empoweredqueen";
                break;
            case ChessPiece.White_Tiger:
                strRetVal = "white tiger";
                break;
            case ChessPiece.White_Elephant:
                strRetVal = "white elephant";
                break;
            case ChessPiece.White_Ferz:
                strRetVal ="white ferz";
                break;
            case ChessPiece.White_Wazir:
                strRetVal = "white wazir";
                break;
            case ChessPiece.White_Amazon:
                strRetVal = "white amazon";
                break;
            case ChessPiece.White_CrazyHorse:
                strRetVal = "white crazyhorse";
                break;
            case ChessPiece.White_AmazonPawn:
                strRetVal = "white amazonpawn";
                break;
            case ChessPiece.White_Gaja:
                strRetVal = "white gaja";
                break;
            case ChessPiece.White_EmpoweredKnight:
                strRetVal = "white empoweredknight";
                break;
            case ChessPiece.White_EmpoweredBishop:
                strRetVal = "white empoweredbishop";
                break;
            case ChessPiece.White_EmpoweredRook:
                strRetVal = "white empoweredrook";
                break;
            case ChessPiece.White_DimensionalKnight:
                strRetVal = "white dimensionalknight";
                break;
            case ChessPiece.White_DimensionalBishop:
                strRetVal = "white dimensionalbishop";
                break;
            case ChessPiece.White_DimensionalRook:
                strRetVal = "white dimensionalrook";
                break;
                case ChessPiece.White_Zebra:
                    strRetVal = "white zebra";
                    break;
                case ChessPiece.White_Camel:
                    strRetVal = "white camel";
                    break;
                case ChessPiece.White_Unicorn:
                    strRetVal = "white unicorn";
                    break;
                case ChessPiece.White_Lion:
                    strRetVal = "white lion";
                    break;
                /* case ChessPiece.White_Giraffe:
                     strRetVal = "white giraffe";
                     break;
                     */
                case ChessPiece.White_Buffalo:
                    strRetVal = "white buffalo";
                    break;
                case ChessPiece.White_Nemesis:
                    strRetVal = "white nemesis";
                    break;
                case ChessPiece.Black_Reaper:
                    strRetVal = "black reaper";
                    break;
                case ChessPiece.Black_Ghost:
                    strRetVal = "black ghost";
                    break;
                case ChessPiece.Black_Lancer:
                    strRetVal = "black lancer";
                    break;
                case ChessPiece.Black_ShogiHorse:
                    strRetVal = "black shogihorse";
                    break;
                case ChessPiece.Black_GoldGeneral:
                    strRetVal = "black goldgeneral";
                    break;
                case ChessPiece.Black_SilverGeneral:
                    strRetVal = "black silvergeneral";
                    break;
                case ChessPiece.Black_Snake:
                    strRetVal = "black snake";
                    break;
                case ChessPiece.Black_Hipo:
                    strRetVal = "black hipo";
                    break;
                case ChessPiece.Black_Dragon:
                    strRetVal = "black dragon";
                    break;
                case ChessPiece.Black_DragonHorse:
                    strRetVal = "black dragonhorse";
                    break;
                case ChessPiece.Black_NemesisPawn:
                    strRetVal = "black nemesispawn";
                    break;
                default:
                strRetVal   = null;
                break;
            }
            return(strRetVal);
        }

        /// <summary>
        /// Load the specified piece from BAML
        /// </summary>
        /// <param name="ePiece">       Piece</param>
        protected override UserControl LoadPiece(ChessPiece ePiece) {
            UserControl userControlRetVal;
            Uri         uri;
            string      strUriName;

            strUriName          = "piecesets/" + m_strBasePath + "/" + NameFromChessPiece(ePiece) + ".xaml";
           // strUriName = "piecesets/leipzig/white rook.xaml";
            uri                 = new Uri(strUriName, UriKind.Relative);
            userControlRetVal   = App.LoadComponent(uri) as UserControl;
            return(userControlRetVal);
        }

        /// <summary>
        /// Load piece sets from resource
        /// </summary>
        /// <returns></returns>
        public static SortedList<string, PieceSet> LoadPieceSetFromResource() {
            SortedList<string, PieceSet>    arrRetVal;
            Assembly                        asm;
            string                          strResName;
            string                          strKeyName;
            string                          strPieceSetName;
            string[]                        arrPart;
            Stream                          streamResource;
            ResourceReader                  resReader;
            PieceSet                        pieceSet;
            
            arrRetVal       = new SortedList<string,PieceSet>(64);
            asm             = typeof(App).Assembly;
            strResName      = asm.GetName().Name + ".g.resources";
            streamResource  = asm.GetManifestResourceStream(strResName);
            try {
                resReader       = new System.Resources.ResourceReader(streamResource);
                streamResource  = null;
                using (resReader) {
                    foreach (DictionaryEntry  dictEntry in resReader.Cast<DictionaryEntry>()) {
                        strKeyName  = dictEntry.Key as string;
                        if (strKeyName != null) {
                            strKeyName  = strKeyName.ToLower();
                            if (strKeyName.StartsWith("piecesets/") && strKeyName.EndsWith(".baml")) {
                                arrPart = strKeyName.Split('/');
                                if (arrPart.Length == 3) {
                                    strPieceSetName = arrPart[1];
                                    if (!arrRetVal.ContainsKey(strPieceSetName)) {
                                        pieceSet    = new PieceSetStandard(strPieceSetName, strPieceSetName);
                                        arrRetVal.Add(strPieceSetName, pieceSet);
                                    }
                                }
                            }
                        }
                    }
                }
            } finally {
                if (streamResource != null) {
                    streamResource.Dispose();
                }
            }
            return(arrRetVal);
        }
    } // Class PieceSetStandard
} // Namespace
