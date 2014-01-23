/* 
 * The MIT License (MIT)
 * 
 * Copyright (c) NyaRuRu
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Runtime.InteropServices;
using TSF;

namespace JLChnToZ.IMEHelper.TSF {
    internal class TSFSink : ITfUIElementSink, IDisposable {
        private CTfThreadMgr tfThreadMgr;
        private uint SinkCookie = 0;
        private bool showWindow;

        public string[] Candidates { get; private set; }
        public uint SelectedIndex { get; private set; }
        
        public event EventHandler onUpdate;

        public TSFSink(bool showWindow = false) {
            Guid guid = typeof(ITfUIElementSink).GUID;
            this.tfThreadMgr = CTfThreadMgr.GetThreadMgr();
            this.tfThreadMgr.Source.AdviseSink(ref guid, this, out SinkCookie);
            this.showWindow = showWindow;
            this.SelectedIndex = 0;
            this.Candidates = new string[0];
        }

        public void BeginUIElement(uint id, ref bool show) {
            show = showWindow;
            OnUIElement(id, true);
        }

        public void EndUIElement(uint id) {
            SelectedIndex = 0;
            Candidates = new string[0];
            if (onUpdate != null)
                onUpdate(this, EventArgs.Empty);
        }

        public void UpdateUIElement(uint id) {
            OnUIElement(id, false);
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
            } catch(ExternalException) {
            } finally {
                if (uiElement != null)
                    Marshal.ReleaseComObject(uiElement);
            }
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
