using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using TSF;

namespace JLChnToZ.IMEHelper {

    internal class CTfThreadMgr : IDisposable {
        private ITfThreadMgr _ThreadMgr;
        private ITfThreadMgrEx _ThreadMgrEx;
        private ITfSource _Source;
        private ITfSourceSingle _SourceSingle;
        private ITfUIElementMgr _UIElementMgr;
        private ITfCompartmentMgr _CompartmentMgr;

        public ITfThreadMgr ThreadMgr { get { return _ThreadMgr; } }
        public ITfThreadMgrEx ThreadMgrEx { get { return _ThreadMgrEx; } }
        public ITfSource Source { get { return _Source; } }
        public ITfSourceSingle SourceSingle { get { return _SourceSingle; } }
        public ITfUIElementMgr UIElementMgr { get { return _UIElementMgr; } }
        public ITfCompartmentMgr CompartmentMgr { get { return _CompartmentMgr; } }

        private CTfThreadMgr(ITfThreadMgr threadMgr) {
            this._ThreadMgr = threadMgr;
            this._ThreadMgrEx = threadMgr as ITfThreadMgrEx;
            this._Source = threadMgr as ITfSource;
            this._SourceSingle = threadMgr as ITfSourceSingle;
            this._UIElementMgr = threadMgr as ITfUIElementMgr;
            this._CompartmentMgr = threadMgr as ITfCompartmentMgr;
        }

        public static CTfThreadMgr GetThreadMgr() {
            ITfThreadMgr mgr;
            TextFrameworkFunctions.TF_GetThreadMgr(out mgr);
            if (mgr == null) {
                Type clsid = Type.GetTypeFromCLSID(TextFrameworkDeclarations.CLSID_TF_ThreadMgr);
                mgr = Activator.CreateInstance(clsid) as ITfThreadMgr;
            }
            return new CTfThreadMgr(mgr);
        }

        public void Dispose() {
            if (_ThreadMgr != null) {
                Marshal.ReleaseComObject(_ThreadMgr);
                _ThreadMgr = null;
                _ThreadMgrEx = null;
                _Source = null;
                _SourceSingle = null;
                _UIElementMgr = null;
                _CompartmentMgr = null;
            }
        }
    }
}
