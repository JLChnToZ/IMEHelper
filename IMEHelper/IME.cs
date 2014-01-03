using System;
using Microsoft.Xna.Framework;

namespace JLChnToZ.IMEHelper {
    /// <summary>
    /// 整合 IME 到 XNA Framework
    /// </summary>
    public class IMEHandler {
        private IMENativeWindow _nativeWnd;

        /// <summary>
        /// 建置函數, 必須在 initialize() 時呼叫
        /// </summary>
        /// <param name="game">遊戲物件</param>
        /// <param name="showDefaultIMEWindow">是否顯示預設的 IME 視窗</param>
        public IMEHandler(Game game, bool showDefaultIMEWindow = false) {
            this.GameInstance = game;
            _nativeWnd = new IMENativeWindow(game, showDefaultIMEWindow);
            _nativeWnd.onCandidatesReceived += (s, e) => { if (onCandidatesReceived != null) onCandidatesReceived(s, e); };
            _nativeWnd.onCompositionReceived += (s, e) => { if (onCompositionReceived != null) onCompositionReceived(s, e); };
            _nativeWnd.onResultReceived += (s, e) => { if (onResultReceived != null) onResultReceived(s, e); };
        }

        /// <summary>
        /// 當候選字介面有更動時會呼叫
        /// </summary>
        public event EventHandler onCandidatesReceived;

        /// <summary>
        /// 當合成字串有更動時呼叫
        /// </summary>
        public event EventHandler onCompositionReceived;

        /// <summary>
        /// 當有輸出時呼叫
        /// </summary>
        public event EventHandler<IMEResultEventArgs> onResultReceived;

        /// <summary>
        /// 取得所有候選字
        /// </summary>
        public string[] Candidates { get { return _nativeWnd._candidates; } }

        /// <summary>
        /// 取得候選字頁面的選項總數最大值
        /// </summary>
        public uint CandidatesPageSize { get { return _nativeWnd._pgSize; } }

        /// <summary>
        /// 取得當前候選字頁面第一個字的索引值
        /// </summary>
        public uint CandidatesPageStart { get { return _nativeWnd._pgStart; } }

        /// <summary>
        /// 取得正在選擇的候選字索引值
        /// </summary>
        public uint CandidatesSelection { get { return _nativeWnd._pgSel; } }

        /// <summary>
        /// 取得當前合成字串
        /// </summary>
        public string Composition {
            get {
                if (_nativeWnd._compositionStrLength <= 0) return string.Empty;
                return _nativeWnd._compositionStr.ToString(0, _nativeWnd._compositionStrLength / 2);
            }
        }

        /// <summary>
        /// 啟用/停用 IME
        /// </summary>
        public bool enabled {
            get {
                return _nativeWnd._enabled;
            }
            set {
                if (value)
                    _nativeWnd.enableIME();
                else
                    _nativeWnd.disableIME();
            }
        }

        /// <summary>
        /// 遊戲物件
        /// </summary>
        public Game GameInstance { get; private set; }
        public void Dispose() {
            _nativeWnd.Dispose();
        }
    }
}
