using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using Microsoft.Win32;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace SrcChess2 {
    /// <summary>
    /// Defines a Chess Board Control
    /// </summary>
    public partial class ChessBoardControl : UserControl, SearchEngine.ITrace, IXmlSerializable {

        #region Inner Class
        
        /// <summary>
        /// Integer Point
        /// </summary>
        public struct IntPoint {
            /// <summary>
            /// Class Ctor
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            public  IntPoint(int x, int y) { X = x; Y = y; }
            /// <summary>X point</summary>
            public  int X;
            /// <summary>Y point</summary>
            public  int Y;
        }

        /// <summary>
        /// Interface implemented by the UI which show the move list.
        /// This interface is called by the chess control each time a move has to be updated.
        /// </summary>
        public interface IMoveListUI {
            /// <summary>Removes all move</summary>
            void        Reset(ChessBoard chessBoard);
            /// <summary>Append a move</summary>
            /// <param name="iPermCount">Permutation analyzed</param>
            /// <param name="iDepth">Depth of the search</param>
            /// <param name="iCacheHit">Nb of permutation found in cache</param>
            void        NewMoveDone(int iPermCount, int iDepth, int iCacheHit);
            /// <summary>Called when the position in the redo buffer has changed</summary>
            void        RedoPosChanged();
            /// <summary>Enable/disable the control</summary>
            bool        IsEnabled { get; set; }
            
        }
        
        /// <summary>
        /// Interface implemented by the UI which show the lost pieces.
        /// This interface is called each time the chess board need an update on the lost pieces UI.
        /// </summary>
        public interface IUpdateCmd {
            /// <summary>Update the lost pieces</summary>
            void        Update();
        }

        /// <summary>
        /// Show a piece moving from starting to ending point
        /// </summary>
        private class SyncFlash {
            /// <summary>Chess Board Control</summary>
            private ChessBoardControl           m_chessBoardControl;
            /// <summary>Solid Color Brush to flash</summary>
            private SolidColorBrush             m_brush;
            /// <summary>First Flash Color</summary>
            private Color                       m_colorStart;
            /// <summary>Second Flash Color</summary>
            private Color                       m_colorEnd;
            //System.Windows.Threading.Dispatcher
            DispatcherFrame                     m_dispatcherFrame;


            /// <summary>
            /// Class Ctor
            /// </summary>
            /// <param name="chessBoardControl">    Chess Board Control</param>
            /// <param name="brush">                Solid Color Brush to flash</param>
            /// <param name="colorStart">           First flashing color</param>
            /// <param name="colorEnd">             Second flashing color</param>
            public SyncFlash(ChessBoardControl chessBoardControl, SolidColorBrush brush, Color colorStart, Color colorEnd) {
                m_chessBoardControl = chessBoardControl;
                m_brush             = brush;
                m_colorStart        = colorStart;
                m_colorEnd          = colorEnd;
            }

            /// <summary>
            /// Flash the specified cell
            /// </summary>
            /// <param name="iCount">                   Flash count</param>
            /// <param name="dSec">                     Flash duration</param>
            /// <param name="eventHandlerTerminated">   Event handler to call when flash is finished</param>
            private void FlashCell(int iCount, double dSec, EventHandler eventHandlerTerminated) {
                ColorAnimation                  colorAnimation;
            
                colorAnimation                  = new ColorAnimation(m_colorStart, m_colorEnd, new Duration(TimeSpan.FromSeconds(dSec)));
                colorAnimation.AutoReverse      = true;
                colorAnimation.RepeatBehavior   = new RepeatBehavior(2);
                if (eventHandlerTerminated != null) {
                    colorAnimation.Completed   += new EventHandler(eventHandlerTerminated);
                }
                m_brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            }

            /// <summary>
            /// Show the move
            /// </summary>
            public void Flash() {
                m_chessBoardControl.IsEnabled       = false;
                FlashCell(4, 0.15, new EventHandler(FirstFlash_Completed));
                m_dispatcherFrame   = new DispatcherFrame();
                Dispatcher.PushFrame(m_dispatcherFrame);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void FirstFlash_Completed(object sender, EventArgs e) {
                m_chessBoardControl.IsEnabled   = true;
                m_dispatcherFrame.Continue      = false;

            }
        } // Class SyncFlash

        /// <summary>Event argument for the MoveSelected event</summary>
        public class MoveSelectedEventArgs : System.EventArgs {
            /// <summary>Move position</summary>
            public ChessBoard.MovePosS  Move;
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="move">     Move position</param>
            public                                      MoveSelectedEventArgs(ChessBoard.MovePosS move) { Move = move; }
        }

        /// <summary>Event argument for the QueryPiece event</summary>
        public class QueryPieceEventArgs : System.EventArgs {
            /// <summary>Position of the square</summary>
            public int                  Pos;
            /// <summary>Piece</summary>
            public ChessBoard.PieceE    Piece;
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="iPos">     Position of the square</param>
            /// <param name="ePiece">   Piece</param>
            public                                      QueryPieceEventArgs(int iPos, ChessBoard.PieceE ePiece) { Pos = iPos; Piece = ePiece; }
        }

        /// <summary>Event argument for the QueryPawnPromotionType event</summary>
        public class QueryPawnPromotionTypeEventArgs : System.EventArgs {
            /// <summary>Promotion type (Queen, Rook, Bishop, Knight or Pawn)</summary>
            public ChessBoard.MoveTypeE                 PawnPromotionType;
            /// <summary>Possible pawn promotions in the current context</summary>
            public ChessBoard.ValidPawnPromotionE       ValidPawnPromotion;
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="eValidPawnPromotion">  Possible pawn promotions in the current context</param>
            public                                      QueryPawnPromotionTypeEventArgs(ChessBoard.ValidPawnPromotionE eValidPawnPromotion) { ValidPawnPromotion = eValidPawnPromotion; PawnPromotionType = ChessBoard.MoveTypeE.Normal; }
        }
        #endregion

        #region Members
        /// <summary>Lite Cell Color property</summary>
        public static readonly DependencyProperty       LiteCellColorProperty;
        /// <summary>Dark Cell Color property</summary>
        public static readonly DependencyProperty       DarkCellColorProperty;
        /// <summary>White Pieces Color property</summary>
        public static readonly DependencyProperty       WhitePieceColorProperty;
        /// <summary>Black Pieces Color property</summary>
        public static readonly DependencyProperty       BlackPieceColorProperty;

        /// <summary>Delegate for the MoveSelected event</summary>
        public delegate void                            MoveSelectedEventHandler(object sender, MoveSelectedEventArgs e);
        /// <summary>Called when a user select a valid move to be done</summary>
        public event MoveSelectedEventHandler           MoveSelected;
        /// <summary>Delegate for the QueryPiece event</summary>
        public delegate void                            QueryPieceEventHandler(object sender, QueryPieceEventArgs e);
        /// <summary>Called when chess control in design mode need to know which piece to insert in the board</summary>
        public event QueryPieceEventHandler             QueryPiece;
        /// <summary>Delegate for the QueryPawnPromotionType event</summary>
        public delegate void                            QueryPawnPromotionTypeEventHandler(object sender, QueryPawnPromotionTypeEventArgs e);
        /// <summary>Called when chess control needs to know which type of pawn promotion must be done</summary>
        public event QueryPawnPromotionTypeEventHandler QueryPawnPromotionType;
        /// <summary>Called to refreshed the command state (menu, toolbar etc.)</summary>
        public event System.EventHandler                UpdateCmdState;

        /// <summary>Piece Set to use</summary>
        private PieceSet                                m_pieceSet;
        /// <summary>Board</summary>
        private ChessBoard                              m_board;
        /// <summary>Array of frames containing the chess piece</summary>
        private Border[]                                m_arrBorder;
        /// <summary>Array containing the current piece</summary>
        private ChessBoard.PieceE[]                     m_arrPiece;
        /// <summary>true to have white in the bottom of the screen, false to have black</summary>
        private bool                                    m_bWhiteInBottom = true;
        ///// <summary>Font use to draw coordinate on the side of the control</summary>
        //private Font                                  m_fontCoord;  TODO
        /// <summary>Currently selected cell</summary>
        private IntPoint                                m_ptSelectedCell;
        /// <summary>true to enable auto-selection</summary>
        private bool                                    m_bAutoSelection;
        /// <summary>User interface used to display the move list</summary>
        private IMoveListUI                             m_moveListUI;
        /// <summary>Time the last search was started</summary>
        private DateTime                                m_dateTimeStartSearching;
        /// <summary>Elapse time of the last search</summary>
        private TimeSpan                                m_timeSpanLastSearch;
        /// <summary>Timer for both player</summary>
        private GameTimer                               m_gameTimer;
        /// <summary>Name of the player playing white piece</summary>
        private string                                  m_strWhitePlayerName;
        /// <summary>Name of the player playing black piece</summary>
        private string                                  m_strBlackPlayerName;
        /// <summary>Type of player playing white piece</summary>
        private PgnParser.PlayerTypeE                   m_eWhitePlayerType;
        /// <summary>Type of player playing black piece</summary>
        private PgnParser.PlayerTypeE                   m_eBlackPlayerType;
        /// <summary>Not zero when board is flashing and reentrance can be a problem</summary>
        private int                                     m_iFlashing;
        #endregion

        #region Board creation

        /// <summary>
        /// Static Ctor
        /// </summary>
        static ChessBoardControl() {
            LiteCellColorProperty   = DependencyProperty.Register("LiteCellColor",
                                                                  typeof(Color),
                                                                  typeof(ChessBoardControl),
                                                               new FrameworkPropertyMetadata(Colors.Moccasin,
                                                                                             FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                             ColorInfoChanged));
            DarkCellColorProperty   = DependencyProperty.Register("DarkCellColor",
                                                               typeof(Color),
                                                               typeof(ChessBoardControl),
                                                               new FrameworkPropertyMetadata(Colors.SaddleBrown,
                                                                                             FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                             ColorInfoChanged));
            WhitePieceColorProperty   = DependencyProperty.Register("WhitePieceColor",
                                                               typeof(Color),
                                                               typeof(ChessBoardControl),
                                                               new FrameworkPropertyMetadata(Colors.White,
                                                                                             FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                             ColorInfoChanged));
            BlackPieceColorProperty   = DependencyProperty.Register("BlackPieceColor",
                                                                    typeof(Color),
                                                                    typeof(ChessBoardControl),
                                                                    new FrameworkPropertyMetadata(Colors.Black,
                                                                                                  FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                                  ColorInfoChanged));
        }

        /// <summary>
        /// Class Ctor
        /// </summary>
        public ChessBoardControl() {
            InitializeComponent();
            m_iFlashing                 = 0;
            m_board                     = new ChessBoard(this);
            if (!m_board.ReadBook("book.bin")) {
                m_board.ReadBookFromResource("SrcChess2.Book.bin");
            }
            m_ptSelectedCell            = new IntPoint(-1, -1);
            m_bAutoSelection            = true;
            m_gameTimer                 = new GameTimer();
            m_gameTimer.Enabled         = false;
            m_gameTimer.Reset(m_board.NextMoveColor);
            m_strWhitePlayerName        = "Player 1";
            m_strBlackPlayerName        = "Player 2";
            m_eWhitePlayerType          = PgnParser.PlayerTypeE.Human;
            m_eBlackPlayerType          = PgnParser.PlayerTypeE.Human;
            InitCell();
        }

        /// <summary>
        /// Returns the XML schema if any
        /// </summary>
        /// <returns>
        /// null
        /// </returns>
        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema() {
 	        return(null);
        }

        /// <summary>
        /// Deserialized the control from a XML reader
        /// </summary>
        /// <param name="reader">   Reader</param>
        void IXmlSerializable.ReadXml(XmlReader reader) {
            string      strWhitePlayerName;
            string      strBlackPlayerName;
            long        lWhiteTicksCount;
            long        lBlackTicksCount;

            if (reader.MoveToContent() != XmlNodeType.Element || reader.LocalName != "SrcChess2") {
                throw new SerializationException("Unknown format");
            } else if (reader.GetAttribute("Version") != "1.00") {
                throw new SerializationException("Unknown version");
            } else {
                strWhitePlayerName  = reader.GetAttribute("WhitePlayerName");
                strBlackPlayerName  = reader.GetAttribute("BlackPlayerName");
                lWhiteTicksCount    = Int64.Parse(reader.GetAttribute("WhiteTicksCount"));
                lBlackTicksCount    = Int64.Parse(reader.GetAttribute("BlackTicksCount"));
                reader.ReadStartElement();
                ((IXmlSerializable)m_board).ReadXml(reader);
                InitAfterLoad(strWhitePlayerName, strBlackPlayerName, lWhiteTicksCount, lBlackTicksCount);
                reader.ReadEndElement();
            }
        }

        /// <summary>
        /// Serialize the control into a XML writer
        /// </summary>
        /// <param name="writer">   XML writer</param>
        void IXmlSerializable.WriteXml(XmlWriter writer) {
            writer.WriteStartElement("SrcChess2");
            writer.WriteAttributeString("Version",         "1.00");
            writer.WriteAttributeString("WhitePlayerName", m_strWhitePlayerName);
            writer.WriteAttributeString("BlackPlayerName", m_strBlackPlayerName);
            writer.WriteAttributeString("WhiteTicksCount", m_gameTimer.WhitePlayTime.Ticks.ToString());
            writer.WriteAttributeString("BlackTicksCount", m_gameTimer.BlackPlayTime.Ticks.ToString());
            ((IXmlSerializable)m_board).WriteXml(writer);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Refresh the board color
        /// </summary>
        private void RefreshBoardColor() {
            int     iPos;
            Border  border;
            Brush   brushDark;
            Brush   brushLite;

            iPos        = 63;
            brushDark   = new SolidColorBrush(DarkCellColor); // m_colorInfo.m_colDarkCase);
            brushLite   = new SolidColorBrush(LiteCellColor); // m_colorInfo.m_colLiteCase);
            for (int y = 0; y < 8; y++) {
                for (int x = 0; x < 8; x++) {
                    border              = m_arrBorder[iPos];
                    border.Background   = (((x + y) & 1) == 0) ? brushLite : brushDark;
                    iPos--;
                }
            }
        }

        /// <summary>
        /// Initialize the cell
        /// </summary>
        private void InitCell() {
            int     iPos;
            Border  border;
            Brush   brushDark;
            Brush   brushLite;

            m_arrBorder = new Border[64];
            m_arrPiece  = new ChessBoard.PieceE[64];
            iPos        = 63;
            brushDark   = new SolidColorBrush(DarkCellColor);   // m_colorInfo.m_colDarkCase);
            brushLite   = new SolidColorBrush(LiteCellColor);   // m_colorInfo.m_colLiteCase);
            for (int y = 0; y < 8; y++) {
                for (int x = 0; x < 8; x++) {
                    border                  = new Border();
                    border.Name             = "Cell" + (iPos.ToString());
                    border.BorderThickness  = new Thickness(0);
                    border.Background       = (((x + y) & 1) == 0) ? brushLite : brushDark;
                    border.BorderBrush      = border.Background;
                    border.SetValue(Grid.ColumnProperty, x);
                    border.SetValue(Grid.RowProperty, y);
                    m_arrBorder[iPos]       = border;
                    m_arrPiece[iPos]        = ChessBoard.PieceE.None;
                    CellContainer.Children.Add(border);
                    iPos--;
                }
            }
           

        }

        /// <summary>
        /// Set the chess piece control
        /// </summary>
        /// <param name="iBoardPos">    Board position</param>
        /// <param name="ePiece">       Piece</param>
        private void SetPieceControl(int iBoardPos, ChessBoard.PieceE ePiece) {
            Border      border;
            UserControl userControlPiece;

            border              = m_arrBorder[iBoardPos];
            userControlPiece    = m_pieceSet[ePiece];
            if (userControlPiece != null) {
                userControlPiece.Margin  = (border.BorderThickness.Top == 0) ? new Thickness(3) : new Thickness(1);
            }
            m_arrPiece[iBoardPos]   = ePiece;
            border.Child            = userControlPiece;
        }

        /// <summary>
        /// Refresh the specified cell
        /// </summary>
        /// <param name="iBoardPos">    Board position</param>
        /// <param name="bFullRefresh"> true to refresh even if its the same piece</param>
        private void RefreshCell(int iBoardPos, bool bFullRefresh) {
            ChessBoard.PieceE   ePiece;

            if (m_board != null && m_pieceSet != null) {
                ePiece = m_board[iBoardPos];
                if (ePiece != m_arrPiece[iBoardPos] || bFullRefresh) {
                    SetPieceControl(iBoardPos, ePiece);
                }
            }
        }

        /// <summary>
        /// Refresh the specified cell
        /// </summary>
        /// <param name="iBoardPos">    Board position</param>
        private void RefreshCell(int iBoardPos) {
            RefreshCell(iBoardPos, false);  // bFullRefresh
        }

        /// <summary>
        /// Refresh the board
        /// </summary>
        /// <param name="bFullRefresh"> Refresh even if its the same piece</param>
        private void Refresh(bool bFullRefresh) {
            if (m_board != null && m_pieceSet != null) {
                for (int iBoardPos = 0; iBoardPos < 64; iBoardPos++) {
                    RefreshCell(iBoardPos, bFullRefresh);
                }
            }
        }

        /// <summary>
        /// Refresh the board
        /// </summary>
        public void Refresh() {
            Refresh(false); // bFullRefresh
        }

        /// <summary>
        /// Reset the board to the initial condition
        /// </summary>
        public void ResetBoard() {
            m_board.ResetBoard();
            SelectedCell    = new IntPoint(-1, -1);
            if (m_moveListUI != null) {
                m_moveListUI.Reset(m_board);
            }
            OnUpdateCmdState(System.EventArgs.Empty);
            m_gameTimer.Reset(m_board.NextMoveColor);
            m_gameTimer.Enabled = false;
            Refresh(false); // bForceRefresh
        }
        #endregion

        #region Properties
        /// <summary>
        /// Called when Image property changed
        /// </summary>
        private static void ColorInfoChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            ChessBoardControl   me;

            me = obj as ChessBoardControl;
            if (me != null && e.OldValue != e.NewValue) {
                me.RefreshBoardColor();
            }
        }

        /// <summary>
        /// Image displayed to the button
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        [Bindable(true)]
        [Category("Brushes")]
        [Description("Lite Cell Color")]
        public Color LiteCellColor {
            get {
                return ((Color)GetValue(LiteCellColorProperty));
            }
            set {
                SetValue(LiteCellColorProperty, value);
            }
        }

        /// <summary>
        /// Image displayed to the button
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        [Bindable(true)]
        [Category("Brushes")]
        [Description("Dark Cell Color")]
        public Color DarkCellColor {
            get {
                return ((Color)GetValue(DarkCellColorProperty));
            }
            set {
                SetValue(DarkCellColorProperty, value);
            }
        }

        /// <summary>
        /// Image displayed to the button
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        [Bindable(true)]
        [Category("Brushes")]
        [Description("White Pieces Color")]
        public Color WhitePieceColor {
            get {
                return ((Color)GetValue(WhitePieceColorProperty));
            }
            set {
                SetValue(WhitePieceColorProperty, value);
            }
        }

        /// <summary>
        /// Image displayed to the button
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        [Bindable(true)]
        [Category("Brushes")]
        [Description("Black Pieces Color")]
        public Color BlackPieceColor {
            get {
                return ((Color)GetValue(BlackPieceColorProperty));
            }
            set {
                SetValue(BlackPieceColorProperty, value);
            }
        }

        /// <summary>
        /// Current piece set
        /// </summary>
        public PieceSet PieceSet {
            get {
                return(m_pieceSet);
            }
            set {
                if (m_pieceSet != value) {
                    m_pieceSet = value;
                    Refresh(true);  // bForceRefresh
                }
            }
        }

        /// <summary>
        /// Current chess board
        /// </summary>
        public ChessBoard Board {
            get {
                return(m_board);
            }
            set {
                if (m_board != value) {
                    m_board = value;
                    Refresh(false); // bForceRefresh
                }
            }
        }

        /// <summary>
        /// User interface responsible to display move list.
        /// </summary>
        public IMoveListUI MoveListUI {
            get {
                return(m_moveListUI);
            }
            set {
                m_moveListUI = value;
                if (value != null) {
                    value.Reset(m_board);
                }
            }
        }

        /// <summary>
        /// Name of the player playing white piece
        /// </summary>
        public string WhitePlayerName {
            get {
                return(m_strWhitePlayerName);
            }
            set {
                m_strWhitePlayerName = value;
            }
        }

        /// <summary>
        /// Name of the player playing black piece
        /// </summary>
        public string BlackPlayerName {
            get {
                return(m_strBlackPlayerName);
            }
            set {
                m_strBlackPlayerName = value;
            }
        }

        /// <summary>
        /// Type of player playing white piece
        /// </summary>
        public PgnParser.PlayerTypeE WhitePlayerType {
            get {
                return(m_eWhitePlayerType);
            }
            set {
                m_eWhitePlayerType = value;
            }
        }

        /// <summary>
        /// Type of player playing black piece
        /// </summary>
        public PgnParser.PlayerTypeE BlackPlayerType {
            get {
                return(m_eBlackPlayerType);
            }
            set {
                m_eBlackPlayerType = value;
            }
        }

        /// <summary>
        /// Gets the chess board associated with the control
        /// </summary>
        public ChessBoard ChessBoard {
            get {
                return(m_board);
            }
        }

        /// <summary>
        /// Determine if the White are in the top or bottom of the draw board
        /// </summary>
        public bool WhiteInBottom {
            get {
                return(m_bWhiteInBottom);
            }
            set {
                if (value != m_bWhiteInBottom) {
                    m_bWhiteInBottom = value;
                    Refresh(false);  // bForceRefresh
                }
            }
        }

        /// <summary>
        /// Enable or disable the auto selection mode
        /// </summary>
        public bool AutoSelection {
            get {
                return(m_bAutoSelection);
            }
            set {
                m_bAutoSelection = value;
            }
        }

        /// <summary>
        /// Determine the board design mode
        /// </summary>
        public bool BoardDesignMode {
            get {
                return(m_board.DesignMode);
            }
            set {
                MessageBoxResult        eRes;
                ChessBoard.PlayerColorE eNextMoveColor;
                
                if (m_board.DesignMode != value) {
                    if (value) {
                        m_board.OpenDesignMode();
                        m_board.MovePosStack.Clear();
                        if (m_moveListUI != null) {
                            m_moveListUI.Reset(m_board);
                        }
                        m_gameTimer.Enabled = false;
                        OnUpdateCmdState(System.EventArgs.Empty);
                    } else {
                        eRes = MessageBox.Show("Is the next move to the white?", "SrcChess", MessageBoxButton.YesNo);
                        eNextMoveColor = (eRes == MessageBoxResult.Yes) ? ChessBoard.PlayerColorE.White : ChessBoard.PlayerColorE.Black;
                        if (m_board.CloseDesignMode(eNextMoveColor, (ChessBoard.BoardStateMaskE)0, 0 /*iEnPassant*/)) {
                            if (m_moveListUI != null) {
                                m_moveListUI.Reset(m_board);
                            }
                            m_gameTimer.Reset(m_board.NextMoveColor);
                            m_gameTimer.Enabled = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the number of move which can be undone
        /// </summary>
        public int UndoCount {
            get {
                return(m_board.MovePosStack.PositionInList + 1);
            }
        }

        /// <summary>
        /// Gets the number of move which can be redone
        /// </summary>
        public int RedoCount {
            get {
                int iCurPos;
                int iCount;
                
                iCurPos = m_board.MovePosStack.PositionInList;
                iCount  = m_board.MovePosStack.Count;
                return(iCount - iCurPos - 1);
            }
        }

        /// <summary>
        /// Current color to play
        /// </summary>
        public ChessBoard.PlayerColorE NextMoveColor {
            get {
                return(m_board.NextMoveColor);
            }
        }

        /// <summary>
        /// List of played moves
        /// </summary>
        private ChessBoard.MovePosS[] MoveList {
            get {
                ChessBoard.MovePosS[]   arrMoveList;
                int                     iMoveCount;
                
                iMoveCount  = m_board.MovePosStack.PositionInList + 1;
                arrMoveList = new ChessBoard.MovePosS[iMoveCount];
                if (iMoveCount != 0) {
                    m_board.MovePosStack.List.CopyTo(0, arrMoveList, 0, iMoveCount);
                }
                return(arrMoveList);
            }
        }

        /// <summary>
        /// Time use to find the last best move
        /// </summary>
        public TimeSpan LastFindBestMoveTimeSpan {
            get {
                return(m_timeSpanLastSearch);
            }
        }

        /// <summary>
        /// Game timer
        /// </summary>
        public GameTimer GameTimer {
            get {
                return(m_gameTimer);
            }
        }

        /// <summary>
        /// Set the cell selection  appearance
        /// </summary>
        /// <param name="ptCell"></param>
        /// <param name="bSelected"></param>
        private void SetCellSelectionState(IntPoint ptCell, bool bSelected) {
            Border  border;
            Control ctl;
            int     iPos;

            if (ptCell.X != -1 && ptCell.Y != -1) {
                iPos                    = ptCell.X + ptCell.Y * 8;
                border                  = m_arrBorder[iPos];
                border.BorderBrush      = (bSelected) ? Brushes.Black : border.Background;
                border.BorderThickness  = (bSelected) ? new Thickness(1) : new Thickness(0);
                ctl                     = border.Child as Control;
                if (ctl != null) {
                    ctl.Margin  = (bSelected) ? new Thickness(1) : new Thickness(3);
                }
            }
        }

        /// <summary>
        /// Currently selected case
        /// </summary>
        public IntPoint SelectedCell {
            get {
                return(m_ptSelectedCell);
            }
            set {
                SetCellSelectionState(m_ptSelectedCell, false);
                m_ptSelectedCell    = value;
                SetCellSelectionState(m_ptSelectedCell, true);
            }
        }

        /// <summary>
        /// true if a cell is selected
        /// </summary>
        public bool IsCellSelected {
            get {
                return(SelectedCell.X != -1 || SelectedCell.Y != -1);
            }
        }

        #endregion

        #region Events
        /// <summary>
        /// Trigger the UpdateCmdState event. Called when command state need to be reevaluated.
        /// </summary>
        /// <param name="e">    Event argument</param>
        protected void OnUpdateCmdState(System.EventArgs e) {
            if (UpdateCmdState != null) {
                UpdateCmdState(this, e);
            }
        }

        /// <summary>
        /// Trigger the MoveSelected event
        /// </summary>
        /// <param name="e">    Event arguments</param>
        protected virtual void OnMoveSelected(MoveSelectedEventArgs e) {
            if (MoveSelected != null) {
                MoveSelected(this, e);
            }
        }

        /// <summary>
        /// OnQueryPiece:       Trigger the QueryPiece event
        /// </summary>
        /// <param name="e">    Event arguments</param>
        protected virtual void OnQueryPiece(QueryPieceEventArgs e) {
            if (QueryPiece != null) {
                QueryPiece(this, e);
            }
        }

        /// <summary>
        /// OnQweryPawnPromotionType:   Trigger the QueryPawnPromotionType event
        /// </summary>
        /// <param name="e">            Event arguments</param>
        protected virtual void OnQueryPawnPromotionType(QueryPawnPromotionTypeEventArgs e) {
            if (QueryPawnPromotionType != null) {
                QueryPawnPromotionType(this, e);
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Save the current game into a file
        /// </summary>
        /// <param name="writer">   Binary writer</param>
        public virtual void SaveGame(BinaryWriter writer) {
            string                  strVersion;
            
            strVersion = "SRCBC095";
            writer.Write(strVersion);
            m_board.SaveBoard(writer);
            writer.Write(m_strWhitePlayerName);
            writer.Write(m_strBlackPlayerName);
            writer.Write(m_gameTimer.WhitePlayTime.Ticks);
            writer.Write(m_gameTimer.BlackPlayTime.Ticks);
        }

        /// <summary>
        /// Initialize the board control after a board has been loaded
        /// </summary>
        private void InitAfterLoad(string strWhitePlayerName, string strBlackPlayerName, long lWhiteTicks, long lBlackTicks) {
            if (m_moveListUI != null) {
                m_moveListUI.Reset(m_board);
            }
            OnUpdateCmdState(System.EventArgs.Empty);
            Refresh(false); // bForceRefresh
            m_strWhitePlayerName    = strWhitePlayerName;
            m_strBlackPlayerName    = strBlackPlayerName;
            m_gameTimer.ResetTo(m_board.NextMoveColor, lWhiteTicks, lBlackTicks);
            m_gameTimer.Enabled = true;
        }

        /// <summary>
        /// Load a game from a stream
        /// </summary>
        /// <param name="reader">   Binary reader</param>
        public virtual bool LoadGame(BinaryReader reader) {
            bool                        bRetVal;
            string                      strVersion;
            string                      strWhitePlayerName;
            string                      strBlackPlayerName;
            long                        lWhiteTicks;
            long                        lBlackTicks;
            
            strVersion = reader.ReadString();
            if (strVersion != "SRCBC095") {
                bRetVal = false;
            } else {
                bRetVal = m_board.LoadBoard(reader);
                if (bRetVal) {
                    strWhitePlayerName  = reader.ReadString();
                    strBlackPlayerName  = reader.ReadString();
                    lWhiteTicks         = reader.ReadInt64();
                    lBlackTicks         = reader.ReadInt64();
                    InitAfterLoad(strWhitePlayerName, strBlackPlayerName, lWhiteTicks, lBlackTicks);
                }
            }
            return(bRetVal);
        }

        /// <summary>
        /// Save the board content into a snapshot string.
        /// </summary>
        /// <returns>
        /// Snapshot
        /// </returns>
        public string TakeSnapshot() {
            StringBuilder       strbRetVal;
            XmlWriter           writer;
            XmlWriterSettings   xmlSettings;

            strbRetVal          = new StringBuilder(16384);
            xmlSettings         = new XmlWriterSettings();
            xmlSettings.Indent  = true;
            writer              = XmlWriter.Create(strbRetVal, xmlSettings);
            ((IXmlSerializable)this).WriteXml(writer);
            writer.Close();
            return(strbRetVal.ToString());
        }

        /// <summary>
        /// Restore the snapshot
        /// </summary>
        /// <param name="strSnapshot">  Snapshot</param>
        public void RestoreSnapshot(string strSnapshot) {
            TextReader  textReader;
            XmlReader   reader;

            textReader  = new StringReader(strSnapshot);
            reader      = XmlReader.Create(textReader);
            ((IXmlSerializable)this).ReadXml(reader);
        }

        /// <summary>
        /// Load a board from a file selected by the user.
        /// </summary>
        /// <returns>
        /// true if a new board has been read
        /// </returns>
         public bool LoadFromFile() {
            bool                    bRetVal = false;
            OpenFileDialog          openDlg;
            Stream                  stream;
            BinaryReader            reader;
            frmPgnGamePicker        frmPicker;
            int                     iIndex;
            string                  strSnapshot;
            
            openDlg = new OpenFileDialog();
            openDlg.AddExtension        = true;
            openDlg.CheckFileExists     = true;
            openDlg.CheckPathExists     = true;
            openDlg.DefaultExt          = "che";
            openDlg.Filter              = "Chess Files (*.che, *.pgn, *.cbsnp)|*.che;*.pgn;*.cbsnp";
            openDlg.Multiselect         = false;
            if (openDlg.ShowDialog() == true) {
                iIndex = openDlg.FileName.LastIndexOf('.');
                if (iIndex != -1 && openDlg.FileName.Substring(iIndex).ToLower() == ".cbsnp") {
                    try {
                        strSnapshot = System.IO.File.ReadAllText(openDlg.FileName);
                        RestoreSnapshot(strSnapshot);
                    } catch (System.Exception ex) {
                        MessageBox.Show(ex.Message);
                    }
                } else if (iIndex == -1 || openDlg.FileName.Substring(iIndex).ToLower() != ".pgn") {
                    try {
                        stream = openDlg.OpenFile();
                    } catch(System.Exception) {
                        MessageBox.Show("Unable to open the file - " + openDlg.FileName);
                        stream = null;
                    }
                    if (stream != null) {
                        try {
                            using (reader = new BinaryReader(stream)) {
                                if (!LoadGame(reader)) {
                                    MessageBox.Show("Bad file version - " + openDlg.FileName);
                                } else {
                                    bRetVal = true;
                                }
                            }
                        } catch(SystemException) {
                            MessageBox.Show("The file '" + openDlg.FileName + "' seems to be corrupted.");
                            ResetBoard();
                        }
                        stream.Dispose();
                        OnUpdateCmdState(System.EventArgs.Empty);
                    }
                } else {
                    frmPicker = new frmPgnGamePicker();
                    using (frmPicker) { 
                        if (frmPicker.InitForm(openDlg.FileName)) {
                            if (frmPicker.ShowDialog() == true) {
                                CreateGameFromMove(frmPicker.StartingChessBoard,
                                                   frmPicker.MoveList,
                                                   frmPicker.StartingColor,
                                                   frmPicker.WhitePlayerName,
                                                   frmPicker.BlackPlayerName,
                                                   frmPicker.WhitePlayerType,
                                                   frmPicker.BlackPlayerType,
                                                   frmPicker.WhiteTimer,
                                                   frmPicker.BlackTimer);
                                bRetVal = true;
                            }
                        }
                    }
                }
            }
            return(bRetVal);
        }

        /// <summary>
        /// Save a board to a file selected by the user
        /// </summary>
        public void SaveToFile() {
            SaveFileDialog  saveDlg;
            Stream          stream;
            
            saveDlg = new SaveFileDialog();
            saveDlg.AddExtension        = true;
            saveDlg.CheckPathExists     = true;
            saveDlg.DefaultExt          = "che";
            saveDlg.Filter              = "Chess Files (*.che)|*.che";
            saveDlg.OverwritePrompt     = true;
            if (saveDlg.ShowDialog() == true) {
                try {
                    stream = saveDlg.OpenFile();
                } catch(System.Exception) {
                    MessageBox.Show("Unable to open the file - " + saveDlg.FileName);
                    stream = null;
                }
                if (stream != null) {
                    try {
                        SaveGame(new BinaryWriter(stream));
                    } catch(SystemException) {
                        MessageBox.Show("Unable to write to the file '" + saveDlg.FileName + "'.");
                    }
                    stream.Dispose();
                }
            }
        }

        /// <summary>
        /// Save the board to a file selected by the user in PGN format
        /// </summary>
        public void SavePGNToFile() {
            SaveFileDialog      saveDlg;
            Stream              stream;
            StreamWriter        writer;
            MessageBoxResult    eResult;
            
            saveDlg = new SaveFileDialog();
            saveDlg.AddExtension        = true;
            saveDlg.CheckPathExists     = true;
            saveDlg.DefaultExt          = "pgn";
            saveDlg.Filter              = "PGN Chess Files (*.pgn)|*.pgn";
            saveDlg.OverwritePrompt     = true;
            if (saveDlg.ShowDialog() == true) {
                if (m_board.MovePosStack.PositionInList + 1 != m_board.MovePosStack.List.Count) {
                    eResult = MessageBox.Show("Do you want to save the undone moves?", "Saving to PGN File", MessageBoxButton.YesNoCancel);
                } else {
                    eResult = MessageBoxResult.Yes;
                }
                if (eResult != MessageBoxResult.Cancel) {
                    try {
                        stream = saveDlg.OpenFile();
                    } catch(System.Exception) {
                        MessageBox.Show("Unable to open the file - " + saveDlg.FileName);
                        stream = null;
                    }
                    if (stream != null) {
                        try {
                            using (writer = new StreamWriter(stream)) {
                                writer.Write(SaveGameToPGNText(eResult == MessageBoxResult.Yes));
                            }
                        } catch(SystemException) {
                            MessageBox.Show("Unable to write to the file '" + saveDlg.FileName + "'.");
                        }
                        stream.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Save the board to a file selected by the user in PGN format
        /// </summary>
        public void SaveSnapshot() {
            SaveFileDialog  saveDlg;
            string          strSnapshot;
            
            saveDlg                     = new SaveFileDialog();
            saveDlg.AddExtension        = true;
            saveDlg.CheckPathExists     = true;
            saveDlg.DefaultExt          = "pgn";
            saveDlg.Filter              = "Debugging Snapshot File (*.cbsnp)|*.cbsnp";
            saveDlg.OverwritePrompt     = true;
            if (saveDlg.ShowDialog() == true) {
                strSnapshot = TakeSnapshot();
                try {
                    System.IO.File.WriteAllText(saveDlg.FileName, strSnapshot, Encoding.Unicode);
                } catch(SystemException) {
                    MessageBox.Show("Unable to write to the file '" + saveDlg.FileName + "'.");
                }
            }
        }

        /// <summary>
        /// Create a new game using the specified list of moves
        /// </summary>
        /// <param name="chessBoardStarting">   Starting board or null if standard board</param>
        /// <param name="listMove">             List of moves</param>
        /// <param name="eNextMoveColor">       Color starting to play</param>
        /// <param name="strWhitePlayerName">   Name of the player playing white pieces</param>
        /// <param name="strBlackPlayerName">   Name of the player playing black pieces</param>
        /// <param name="eWhitePlayerType">     Type of player playing white pieces</param>
        /// <param name="eBlackPlayerType">     Type of player playing black pieces</param>
        /// <param name="spanPlayerWhite">      Timer for white</param>
        /// <param name="spanPlayerBlack">      Timer for black</param>
        public virtual void CreateGameFromMove(ChessBoard                   chessBoardStarting,
                                               List<ChessBoard.MovePosS>    listMove,
                                               ChessBoard.PlayerColorE      eNextMoveColor,
                                               string                       strWhitePlayerName,
                                               string                       strBlackPlayerName,
                                               PgnParser.PlayerTypeE        eWhitePlayerType,
                                               PgnParser.PlayerTypeE        eBlackPlayerType,
                                               TimeSpan                     spanPlayerWhite,
                                               TimeSpan                     spanPlayerBlack) {
            m_board.CreateGameFromMove(chessBoardStarting,
                                       listMove,
                                       eNextMoveColor);
            if (m_moveListUI != null) {
                m_moveListUI.Reset(m_board);
            }
            WhitePlayerName = strWhitePlayerName;
            BlackPlayerName = strBlackPlayerName;
            WhitePlayerType = eWhitePlayerType;
            BlackPlayerType = eBlackPlayerType;
            OnUpdateCmdState(System.EventArgs.Empty);
            m_gameTimer.ResetTo(m_board.NextMoveColor,
                                spanPlayerWhite.Ticks,
                                spanPlayerBlack.Ticks);
            m_gameTimer.Enabled = true;
            Refresh(false); // bForceRefresh
        }

        /// <summary>
        /// Creates a game from a PGN text paste by the user
        /// </summary>
        /// <returns>
        /// true if a new board has been loaded
        /// </returns>
        public bool CreateFromPGNText() {
            bool                bRetVal = false;
            frmCreatePgnGame    frm;
            
            frm = new frmCreatePgnGame();
            if (frm.ShowDialog() == true) {
                CreateGameFromMove(frm.StartingChessBoard,
                                    frm.MoveList,
                                    frm.StartingColor,
                                    frm.WhitePlayerName,
                                    frm.BlackPlayerName,
                                    frm.WhitePlayerType,
                                    frm.BlackPlayerType,
                                    frm.WhiteTimer,
                                    frm.BlackTimer);
                bRetVal = true;
            }
            return(bRetVal);
        }

        /// <summary>
        /// Creates a game from a PGN text paste by the user
        /// </summary>
        /// <returns>
        /// true if a new board has been loaded
        /// </returns>
         public string SaveGameToPGNText(bool bIncludeRedoMove) {
            string  strRetVal;
            
            strRetVal = PgnUtil.GetPGNFromBoard(m_board,
                                                bIncludeRedoMove,
                                                "SrcChess Game",
                                                "SrcChess Location",
                                                DateTime.Now.ToString("yyyy.MM.dd"),
                                                "1",
                                                WhitePlayerName,
                                                BlackPlayerName,
                                                WhitePlayerType,
                                                BlackPlayerType,
                                                m_gameTimer.WhitePlayTime,
                                                m_gameTimer.BlackPlayTime);
            return(strRetVal);
        }

        /// <summary>
        /// Create a book from files selected by the user
        /// </summary>
         public void CreateBookFromFiles() {
            OpenFileDialog          openDlg;
            SaveFileDialog          saveDlg;
            PgnParser               parser;
            Stream                  stream;
            TextReader              reader;
            Book                    book;
            string                  strText;
            List<int[]>             arrMoveList;
            int                     iSkip;
            int                     iTruncated;
            int                     iTotalSkip = 0;
            int                     iTotalTruncated = 0;
            int                     iTotalFiles = 0;
            int                     iBookEntries;
            bool                    bAbort = false;
            
            arrMoveList = new List<int[]>(8192);
            book        = new Book();
            openDlg = new OpenFileDialog();
            openDlg.AddExtension        = true;
            openDlg.CheckFileExists     = true;
            openDlg.CheckPathExists     = true;
            openDlg.DefaultExt          = "pgn";
            openDlg.Filter              = "Chess PGN Files (*.pgn)|*.pgn";
            openDlg.Multiselect         = true;
            if (openDlg.ShowDialog() == true) {
                foreach (string strFileName in openDlg.FileNames) {
                    try {
                        stream = File.OpenRead(strFileName);
                    } catch(System.Exception) {
                        MessageBox.Show("Unable to open the file - " + strFileName);
                        stream = null;
                    }
                    if (stream != null) {
                        reader  = new StreamReader(stream);
                        strText = reader.ReadToEnd();
                        parser  = new PgnParser(false);
                        try {
                            parser.Parse(strText, arrMoveList, out iSkip, out iTruncated);
                            iTotalSkip      += iSkip;
                            iTotalTruncated += iTruncated;
                            iTotalFiles++;
                        } catch(PgnParserException exc) {
                            MessageBox.Show("Error processing file '" + strFileName + "'\r\n" + exc.Message + "\r\n" + exc.CodeInError);
                            bAbort =  true;
                        }
                        stream.Close();
                    }
                    if (bAbort) {
                        break;
                    }
                }
                if (!bAbort) {
                    iBookEntries = book.CreateBookList(arrMoveList, 30, 10);
                    MessageBox.Show(iTotalFiles.ToString() + " PNG file(s) read. " + arrMoveList.Count.ToString() + " games processed. " + iTotalTruncated.ToString() + " truncated. " + iTotalSkip.ToString() + " skipped. " + iBookEntries.ToString() + " book entries defined.");
                    saveDlg = new SaveFileDialog();
                    saveDlg.AddExtension        = true;
                    saveDlg.CheckPathExists     = true;
                    saveDlg.DefaultExt          = "bin";
                    saveDlg.Filter              = "Chess Opening Book (*.bin)|*.bin";
                    saveDlg.OverwritePrompt     = true;
                    if (saveDlg.ShowDialog() == true) {
                        try {
                            book.SaveBookToFile(saveDlg.FileName);
                        } catch (System.Exception ex) {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set the piece in a case. Can only be used in design mode.
        /// </summary>
        public void SetCaseValue(int iPos, ChessBoard.PieceE ePiece) {

            if (BoardDesignMode) {
                m_board[iPos] = ePiece;
                RefreshCell(iPos);
            }
        }

        /// <summary>
        /// Find a move from the opening book
        /// </summary>
        /// <param name="searchMode">       Search mode</param>
        /// <param name="move">             Found move</param>
        /// <returns>
        /// true if succeed, false if no move found in book
        /// </returns>
        public bool FindBookMove(SearchEngine.SearchMode searchMode, out ChessBoard.MovePosS move) {
            bool                    bRetVal;
            ChessBoard.MovePosS[]   arrMoves;
            
            if (!m_board.StandardInitialBoard) {
                move.OriginalPiece  = ChessBoard.PieceE.None;
                move.StartPos       = 255;
                move.EndPos         = 255;
                move.Type           = ChessBoard.MoveTypeE.Normal;
                bRetVal             = false;
            } else {
                arrMoves = MoveList;
                bRetVal  = m_board.FindBookMove(searchMode, m_board.NextMoveColor, arrMoves, out move);
            }
            return(bRetVal);
        }

        /// <summary>
        /// Find the best move for a player using alpha-beta pruning or minmax search
        /// </summary>
        /// <param name="searchMode">       Search mode</param>
        /// <param name="chessBoard">       Chess board to use. Null to use the base one</param>
        /// <param name="moveBest">         Best move found</param>
        /// <param name="iPermCount">       Total permutation evaluated</param>
        /// <param name="iCacheHit">        Number of moves found in the translation table cache</param>
        /// <param name="iMaxDepth">        Maximum depth evaluated</param>
        /// <returns>
        /// true if a move has been found
        /// </returns>
        public bool FindBestMove(SearchEngine.SearchMode searchMode, ChessBoard chessBoard, out ChessBoard.MovePosS moveBest, out int iPermCount, out int iCacheHit, out int iMaxDepth) {
            bool    bRetVal;
            bool    bUseBook;
            
            bUseBook = ((searchMode.m_eOption & SearchEngine.SearchMode.OptionE.UseBook) != 0);
            if (bUseBook && FindBookMove(searchMode, out moveBest)) {
                iPermCount = -1;
                iCacheHit  = -1;
                iMaxDepth  = 0;
                bRetVal    = true;
            } else {
                if (chessBoard == null) {
                    chessBoard = m_board;
                }
                m_dateTimeStartSearching = DateTime.Now;
                bRetVal                  = chessBoard.FindBestMove(searchMode, m_board.NextMoveColor, out moveBest, out iPermCount, out iCacheHit, out iMaxDepth);
                m_timeSpanLastSearch     = DateTime.Now - m_dateTimeStartSearching;
            }
            return(bRetVal);
        }

        /// <summary>
        /// Cancel search
        /// </summary>
        public void CancelSearch() {
            m_board.CancelSearch();
        }

        /// <summary>
        /// Search trace
        /// </summary>
        /// <param name="iDepth">       Search depth</param>
        /// <param name="ePlayerColor"> Color who play</param>
        /// <param name="movePos">      Move position</param>
        /// <param name="iPts">         Points</param>
        void SearchEngine.ITrace.TraceSearch(int iDepth, ChessBoard.PlayerColorE ePlayerColor, ChessBoard.MovePosS movePos, int iPts) {
            //
        }

        /// <summary>
        /// Gets the position express in a human form
        /// </summary>
        /// <param name="ptStart">      Starting Position</param>
        /// <param name="ptEnd">        Ending position</param>
        /// <returns>
        /// Human form position
        /// </returns>
        static public string GetHumanPos(IntPoint ptStart, IntPoint ptEnd) {
            return(ChessBoard.GetHumanPos(ptStart.X + (ptStart.Y << 3)) + "-" + ChessBoard.GetHumanPos(ptEnd.X + (ptEnd.Y << 3)));
        }

        /// <summary>
        /// Gets the cell position from a mouse event
        /// </summary>
        /// <param name="e">        Mouse event argument</param>
        /// <param name="ptCell">   Resulting cell</param>
        /// <returns>
        /// true if succeed, false if mouse don't point to a cell
        /// </returns>
        public bool GetCellFromPoint(MouseEventArgs e, out IntPoint ptCell) {
            bool        bRetVal;
            Point       pt;
            int         iCol;
            int         iRow;
            double      dActualWidth;
            double      dActualHeight;

            pt  = e.GetPosition(CellContainer);
            dActualHeight   = CellContainer.ActualHeight;
            dActualWidth    = CellContainer.ActualWidth;
            iCol            = (int)(pt.X * 8 / dActualWidth);
            iRow            = (int)(pt.Y * 8 / dActualHeight);
            if (iCol >= 0 && iCol < 8 && iRow >= 0 && iRow < 8) {
                ptCell  = new IntPoint(7 - iCol, 7 - iRow);
                bRetVal = true;
            } else {
                ptCell  = new IntPoint(-1, -1);
                bRetVal = false;
            }
            return(bRetVal);
        }

        /// <summary>
        /// Flash the specified cell
        /// </summary>
        /// <param name="ptCell">   Cell to flash</param>
        public void FlashCell(IntPoint ptCell) {
            int             iCellPos;
            Border          border;
            Brush           brush;
            Color           colorStart;
            Color           colorEnd;
            SyncFlash       syncFlash;
            bool            bMoveListWasEnabled = true;
            
            m_iFlashing++;  // When flashing, a message loop is processed which can cause reentrance problem
            if (m_moveListUI != null) {
                bMoveListWasEnabled     = m_moveListUI.IsEnabled;
                //m_moveListUI.IsEnabled  = false;
            }
            try { 
                iCellPos = ptCell.X + ptCell.Y * 8;
                if (((ptCell.X + ptCell.Y) & 1) != 0) {
                    colorStart  = DarkCellColor;    // m_colorInfo.m_colDarkCase;
                    colorEnd    = LiteCellColor;    // m_colorInfo.m_colLiteCase;
                } else {
                    colorStart  = LiteCellColor;    // m_colorInfo.m_colLiteCase;
                    colorEnd    = DarkCellColor;    // m_colorInfo.m_colDarkCase;
                }
                border                          = m_arrBorder[iCellPos];
                brush                           = border.Background.Clone();
                border.Background               = brush;
                syncFlash                       = new SyncFlash(this, brush as SolidColorBrush, colorStart, colorEnd);
                syncFlash.Flash();
            } finally {
                m_iFlashing--;
                if (m_moveListUI != null) {
                    //m_moveListUI.IsEnabled = bMoveListWasEnabled;
                }
            }
        }

        /// <summary>
        /// Return true if board is flashing and we must not let the control be reentered
        /// </summary>
        public bool IsFlashing {
            get {
                return(m_iFlashing != 0);
            }
        }

        /// <summary>
        /// Flash the specified cell
        /// </summary>
        /// <param name="iStartPos">    Cell position</param>
        private void FlashCell(int iStartPos) {
            IntPoint    pt;

            pt  = new IntPoint(iStartPos & 7, iStartPos / 8);
            FlashCell(pt);
        }

        /// <summary>
        /// Get additional position to update when doing or undoing a special move
        /// </summary>
        /// <param name="movePos">      Position of the move</param>
        /// <returns>
        /// Array of position to undo
        /// </returns>
        private int[] GetPosToUpdate(ChessBoard.MovePosS movePos) {
            List<int>       arrRetVal = new List<int>(2);

            if ((movePos.Type & ChessBoard.MoveTypeE.MoveTypeMask) == ChessBoard.MoveTypeE.Castle) {
                switch(movePos.EndPos) {
                case 1:
                    arrRetVal.Add(0);
                    arrRetVal.Add(2);
                    break;
                case 5:
                    arrRetVal.Add(7);
                    arrRetVal.Add(4);
                    break;
                case 57:
                    arrRetVal.Add(56);
                    arrRetVal.Add(58);
                    break;
                case 61:
                    arrRetVal.Add(63);
                    arrRetVal.Add(60);
                    break;
                default:
                    MessageBox.Show("Oops!");
                    break;
                }
            } else if ((movePos.Type & ChessBoard.MoveTypeE.MoveTypeMask) == ChessBoard.MoveTypeE.EnPassant) {
                arrRetVal.Add((movePos.StartPos & 56) + (movePos.EndPos & 7));
            }
            return(arrRetVal.ToArray());
        }

        /// <summary>
        /// Show before move is done
        /// </summary>
        /// <param name="movePos">      Position of the move</param>
        /// <param name="bFlash">       true to flash the from and destination pieces</param>
        private void ShowBeforeMove(ChessBoard.MovePosS movePos, bool bFlash) {
            if (bFlash) {
                FlashCell(movePos.StartPos);
            }
        }

        /// <summary>
        /// Show after move is done
        /// </summary>
        /// <param name="movePos">      Position of the move</param>
        /// <param name="bFlash">       true to flash the from and destination pieces</param>
        private void ShowAfterMove(ChessBoard.MovePosS movePos, bool bFlash) {
            int[]       arrPosToUpdate;

            if (movePos.Type == ChessBoard.MoveTypeE.PieceEaten && (m_board[movePos.EndPos] == (ChessBoard.PieceE.Elephant | ChessBoard.PieceE.White) || m_board[movePos.EndPos] == (ChessBoard.PieceE.Elephant | ChessBoard.PieceE.Black)))
            {

                byte[] arr = ChessBoard.MaxPosElephant(movePos.StartPos, movePos.EndPos);
                

                RefreshCell(movePos.StartPos);
                RefreshCell(arr[0]);
                RefreshCell(arr[1]);
                RefreshCell(movePos.EndPos);
                if (bFlash)
                {
                    FlashCell(arr[0]);
                    FlashCell(arr[1]);
                    FlashCell(movePos.EndPos);
                }

            }
            else
            {
                RefreshCell(movePos.StartPos);
                RefreshCell(movePos.EndPos);
                if (bFlash)
                {
                    FlashCell(movePos.EndPos);
                }
            }
               



            //
            arrPosToUpdate = GetPosToUpdate(movePos);
            foreach (int iPos in arrPosToUpdate) {
                if (bFlash) {
                    FlashCell(iPos);
                }
                RefreshCell(iPos);
            }
        }

        /// <summary>
        /// Play the specified move
        /// </summary>
        /// <param name="movePos">      Position of the move</param>
        /// <param name="bFlash">       true to flash the from and destination pieces</param>
        /// <param name="iPermCount">   Permutation count</param>
        /// <param name="iDepth">       Maximum depth use to find the move</param>
        /// <param name="iCacheHit">    Number of permutation found in cache</param>
        /// <returns>
        /// NoRepeat, FiftyRuleRepeat, ThreeFoldRepeat, Tie, Check, Mate
        /// </returns>
        public ChessBoard.MoveResultE DoMove(ChessBoard.MovePosS movePos, bool bFlash, int iPermCount, int iDepth, int iCacheHit) {
            ChessBoard.MoveResultE eRetVal;
            // interessant
            if (movePos.Type == ChessBoard.MoveTypeE.PieceEaten && (m_board[movePos.StartPos] == (ChessBoard.PieceE.Tiger | ChessBoard.PieceE.White) || m_board[movePos.StartPos] == (ChessBoard.PieceE.Tiger | ChessBoard.PieceE.Black)))
            {
                movePos.StartPos = movePos.EndPos;
            }

            //elph forçar moviment i borrar els dos peces
            if (movePos.Type == ChessBoard.MoveTypeE.PieceEaten && (m_board[movePos.StartPos] == (ChessBoard.PieceE.Elephant | ChessBoard.PieceE.White) || m_board[movePos.StartPos] == (ChessBoard.PieceE.Elephant | ChessBoard.PieceE.Black)))
            {

                byte[] arr = ChessBoard.MaxPosElephant(movePos.StartPos, movePos.EndPos);
                movePos.EndPos = arr[2];
                //movePos.Type = MoveTypeE.Normal;
                
               // m_board[arr[0]] = ChessBoard.PieceE.None;
                 //m_board[arr[1]] = ChessBoard.PieceE.None;
                ChessBoard.m_pBoard[arr[0]] = ChessBoard.PieceE.None;
                ChessBoard.m_pBoard[arr[1]] = ChessBoard.PieceE.None;

            }

            if (m_iFlashing == 0) { 
                ShowBeforeMove(movePos, bFlash);
                eRetVal = m_board.DoMove(movePos);
                ShowAfterMove(movePos, bFlash);
                if (m_moveListUI != null) {
                    m_moveListUI.NewMoveDone(iPermCount, iDepth, iCacheHit);
                }
                OnUpdateCmdState(System.EventArgs.Empty);
                m_gameTimer.PlayerColor = m_board.NextMoveColor;
                m_gameTimer.Enabled     = (eRetVal == ChessBoard.MoveResultE.NoRepeat || eRetVal == ChessBoard.MoveResultE.Check);
            } else {
                eRetVal = SrcChess2.ChessBoard.MoveResultE.NoRepeat;
            }
            return(eRetVal);
        }

        /// <summary>
        /// Undo the last move
        /// </summary>
        /// <param name="bFlash">   true to flash the from and destination pieces</param>
        public void UndoMove(bool bFlash) {
            ChessBoard.MovePosS movePos;
            int[]               arrPosToUpdate;

            if (m_iFlashing == 0) { 
                movePos = m_board.MovePosStack.CurrentMove;
                if (bFlash) {
                    FlashCell(movePos.EndPos);
                }
                m_board.UndoMove();
                RefreshCell(movePos.EndPos);
                RefreshCell(movePos.StartPos);
                if (bFlash) {
                    FlashCell(movePos.StartPos);
                }
                arrPosToUpdate = GetPosToUpdate(movePos);
                Array.Reverse(arrPosToUpdate);
                foreach (int iPos in arrPosToUpdate) {
                    if (bFlash) {
                        FlashCell(iPos);
                    }
                    RefreshCell(iPos);
                }
                if (m_moveListUI != null) {
                    m_moveListUI.RedoPosChanged();
                }
                OnUpdateCmdState(System.EventArgs.Empty);
                m_gameTimer.PlayerColor = m_board.NextMoveColor;
                m_gameTimer.Enabled     = true;
            }
        }

        /// <summary>
        /// Redo the most recently undone move
        /// </summary>
        /// <param name="bFlash">   true to flash</param>
        /// <returns>
        /// NoRepeat, FiftyRuleRepeat, ThreeFoldRepeat, Check, Mate
        /// </returns>
        public ChessBoard.MoveResultE RedoMove(bool bFlash) {
            ChessBoard.MoveResultE  eRetVal;
            ChessBoard.MovePosS     movePos;

            if (m_iFlashing == 0) { 
                movePos = m_board.MovePosStack.NextMove;
                ShowBeforeMove(movePos, bFlash);
                eRetVal = m_board.RedoMove();
                ShowAfterMove(movePos, bFlash);
                if (m_moveListUI != null) {
                    m_moveListUI.RedoPosChanged();
                }
                OnUpdateCmdState(System.EventArgs.Empty);
                m_gameTimer.PlayerColor = m_board.NextMoveColor;
                m_gameTimer.Enabled = (eRetVal == ChessBoard.MoveResultE.NoRepeat || eRetVal == ChessBoard.MoveResultE.Check);
            } else {
                eRetVal = SrcChess2.ChessBoard.MoveResultE.NoRepeat;
            }
            return(eRetVal);
        }

        /// <summary>
        /// Select a move by index using undo/redo buffer to move
        /// </summary>
        /// <param name="iIndex">   Index of the move. Can be -1</param>
        /// <param name="bSucceed"> true if index in range</param>
        /// <returns>
        /// Repeat result
        /// </returns>
        public ChessBoard.MoveResultE SelectMove(int iIndex, out bool bSucceed) {
            ChessBoard.MoveResultE  eRetVal = ChessBoard.MoveResultE.NoRepeat;
            int                     iCurPos;
            int                     iCount;

            if (m_iFlashing == 0) { 
                iCurPos = m_board.MovePosStack.PositionInList;
                iCount  = m_board.MovePosStack.Count;
                if (iIndex >= -1 && iIndex < iCount) {
                    bSucceed = true;
                    if (iCurPos < iIndex) {
                        while (iCurPos != iIndex) {
                            eRetVal = RedoMove(false);
                            iCurPos++;
                        }
                    } else if (iCurPos > iIndex) {
                        while (iCurPos != iIndex) {
                            UndoMove(false);
                            iCurPos--;
                        }
                    }
                } else {
                    bSucceed = false;
                }
            } else {
                bSucceed = false;
            }
            return(eRetVal);
        }

        /// <summary>
        /// ShowHintMove:                   Show a hint on the next move to do
        /// </summary>
        /// <param name="movePos">          Move position</param>
        public void ShowHintMove(ChessBoard.MovePosS movePos) {
            ShowBeforeMove(movePos, true);
            m_board.DoMoveNoLog(movePos);
            ShowAfterMove(movePos, true);
            m_board.UndoMoveNoLog(movePos);
            ShowAfterMove(movePos, false);
        }

        /// <summary>
        /// ShowHint:                       Find and show a hint on the next move to do
        /// </summary>
        /// <param name="searchMode">       Search mode</param>
        /// <param name="movePos">          Move position found</param>
        /// <param name="iPermCount">       Permutation count</param>
        /// <param name="iCacheHit">        Cache hit</param>
        /// <returns>
        /// true if a hint has been shown
        /// </returns>
        public bool ShowHint(SearchEngine.SearchMode searchMode, out ChessBoard.MovePosS movePos, out int iPermCount, out int iCacheHit) {
            bool    bRetVal;
            int     iMaxDepth;
            
            if (FindBestMove(searchMode, null, out movePos, out iPermCount, out iCacheHit, out iMaxDepth)) {
                ShowHintMove(movePos);
                bRetVal = true;
            } else {
                bRetVal = false;
            }
            return(bRetVal);
        }

        /// <summary>
        /// Intercept Mouse click
        /// </summary>
        /// <param name="e">    Event Parameter</param>
        protected override void OnMouseDown(MouseButtonEventArgs e) {
            IntPoint                        pt;
            ChessBoard.MovePosS             tMove;
            ChessBoard.ValidPawnPromotionE  eValidPawnPromotion;
            QueryPieceEventArgs             eQueryPieceEventArgs;
            int                             iPos;
            ChessBoard.PieceE               ePiece;
            QueryPawnPromotionTypeEventArgs eventArg;
            bool                            bWhiteToMove;
            bool                            bWhitePiece;
            
            base.OnMouseDown(e);
            if (BoardDesignMode) {
                if (GetCellFromPoint(e, out pt)) {
                    iPos                 = pt.X + (pt.Y << 3);
                    eQueryPieceEventArgs = new QueryPieceEventArgs(iPos, ChessBoard[iPos]);
                    OnQueryPiece(eQueryPieceEventArgs);
                    ChessBoard[iPos] = eQueryPieceEventArgs.Piece;
                    RefreshCell(iPos);
                }
            } else if (AutoSelection) {
                if (GetCellFromPoint(e, out pt)) {
                    iPos = pt.X + (pt.Y << 3);
                    if (SelectedCell.X == -1 || SelectedCell.Y == -1) {
                        ePiece          = m_board[iPos];
                        bWhiteToMove    = (m_board.NextMoveColor == SrcChess2.ChessBoard.PlayerColorE.White);
                        bWhitePiece     = (ePiece & SrcChess2.ChessBoard.PieceE.Black) == 0;
                        if (ePiece != SrcChess2.ChessBoard.PieceE.None && bWhiteToMove == bWhitePiece) {
                            SelectedCell = pt;
                        } else {
                            System.Console.Beep();
                        }
                    } else {
                        if (SelectedCell.X == pt.X  && SelectedCell.Y == pt.Y) {
                            SelectedCell = new IntPoint(-1, -1);
                        } else {
                            tMove = ChessBoard.FindIfValid(m_board.NextMoveColor,
                                                            SelectedCell.X + (SelectedCell.Y << 3),
                                                            iPos);
                            if (tMove.StartPos != 255) {                                                           
                                eValidPawnPromotion = ChessBoard.FindValidPawnPromotion(m_board.NextMoveColor, 
                                                                                        SelectedCell.X + (SelectedCell.Y << 3),
                                                                                        iPos);
                                if (eValidPawnPromotion != ChessBoard.ValidPawnPromotionE.None) {
                                    eventArg = new QueryPawnPromotionTypeEventArgs(eValidPawnPromotion);
                                    OnQueryPawnPromotionType(eventArg);
                                    if (eventArg.PawnPromotionType == ChessBoard.MoveTypeE.Normal) {
                                        tMove.StartPos = 255;
                                    } else {
                                        tMove.Type &= ~ChessBoard.MoveTypeE.MoveTypeMask;
                                        tMove.Type |= eventArg.PawnPromotionType;
                                    }
                                }
                            }
                            SelectedCell = new IntPoint(-1, -1);
                            if (tMove.StartPos == 255) {
                                System.Console.Beep();
                            } else {
                                ePiece = m_board[iPos];
                               /* if (ePiece != SrcChess2.ChessBoard.PieceE.None && m_board[tMove.StartPos] == SrcChess2.ChessBoard.PieceE.Tiger)
                                {
                                    if(tMove.Type == SrcChess2.ChessBoard.MoveTypeE.PieceEaten)
                                        tMove.StartPos = tMove.EndPos;
                                    OnMoveSelected(new MoveSelectedEventArgs(tMove));

                                }
                                else*/
                                    OnMoveSelected(new MoveSelectedEventArgs(tMove));


                            }
                        }
                    }
                }
            }
        }
        #endregion
    
    } // Class ChessBoardControl
} // Namespace
