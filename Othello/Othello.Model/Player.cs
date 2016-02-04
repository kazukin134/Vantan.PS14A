namespace Othello.Model
{
    /// <summary>
    /// プレイヤー
    /// </summary>
    public enum Player
    {
        /// <summary>
        /// 黒プレイヤー
        /// </summary>
        Black = CellState.Black,

        /// <summary>
        /// 白プレイヤー
        /// </summary>
        White = CellState.White
    }

    public static class PlayerExtensions
    {
        public static CellState ToCellState(this Player player)
            => (CellState)player;

        public static Player GetOtherPlayer(this Player player)
            => player == Player.White ? Player.Black : Player.White;
    }
}