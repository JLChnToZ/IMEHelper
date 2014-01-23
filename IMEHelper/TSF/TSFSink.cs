using System;
using System.Runtime.InteropServices;
using TSF;

namespace JLChnToZ.IMEHelper.TSF {
    internal class TSFSink : ITfUIElementSink, IDisposable {

        CTfThreadMgr tfThreadMgr;
        uint SinkCookie = 0;

        public string[] Candidates { get; private set; }
        public uint SelectedIndex { get; private set; }

        public event EventHandler onUpdate;

        public TSFSink() {
            tfThreadMgr = CTfThreadMgr.GetThreadMgr();
            Guid guid = typeof(ITfUIElementSink).GUID;
            tfThreadMgr.Source.AdviseSink(ref guid, this, out SinkCookie);
            SelectedIndex = 0;
            Candidates = new string[0];
            System.Diagnostics.Debug.Print("Cookie: {0}", SinkCookie);
        }

        public void BeginUIElement(uint id, ref bool show) {
            show = false;
            OnUIElement(id, true);
            System.Diagnostics.Debug.Print("Begin UI Element");
        }

        public void EndUIElement(uint id) {
            SelectedIndex = 0;
            Candidates = new string[0];
            if (onUpdate != null)
                onUpdate(this, EventArgs.Empty);
            System.Diagnostics.Debug.Print("End UI Element");
        }

        public void UpdateUIElement(uint id) {
            OnUIElement(id, false);
            System.Diagnostics.Debug.Print("Update UI Element");
        }

        void OnUIElement(uint id, bool start) {
            ITfUIElement uiElement = null;
            try {
                tfThreadMgr.UIElementMgr.GetUIElement(id, out uiElement);
                ITfCandidateListUIElementBehavior cList = uiElement as ITfCandidateListUIElementBehavior;
                if (cList != null) {
                    uint count = 0;
                    string desc;
                    cList.GetDescription(out desc);
                    cList.GetCount(out count);
                    string[] candidates = new string[count];
                    for (uint i = 0; i < count; i++)
                        cList.GetString(i, out candidates[i]);
                    Candidates = candidates;
                    uint currentIndex = 0;
                    cList.GetSelection(out currentIndex);
                    SelectedIndex = currentIndex;
                }
            } catch(ExternalException ex) {
                
                System.Diagnostics.Debug.Print("Error:{0}", ex.ErrorCode);
            } finally {
                if (uiElement != null)
                    Marshal.ReleaseComObject(uiElement);
            }
            System.Diagnostics.Debug.Print("Candidates Count: {0}, sel: {1}", Candidates.Length, SelectedIndex);
            if (onUpdate != null)
                onUpdate(this, EventArgs.Empty);
        }

        public void Dispose() {
            if (tfThreadMgr != null) {
                if (SinkCookie != 0)
                    tfThreadMgr.Source.UnadviseSink(SinkCookie);
                tfThreadMgr.Dispose();
                tfThreadMgr = null;
            }
        }
    }
}
