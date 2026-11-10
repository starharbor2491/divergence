// Browser-native clicks are checked against current Load-button regions. Unity's
// frame-delayed pointer events must not arm an unrelated later document click.
mergeInto(LibraryManager.library, {
    $EvoRunExit: {
        warn: function (event) {
            event.preventDefault();
            event.returnValue = '';
        }
    },
    ProtectRunOnExit: function (enabled) {
        window.removeEventListener('beforeunload', EvoRunExit.warn);
        if (enabled) window.addEventListener('beforeunload', EvoRunExit.warn);
    },
    ProtectRunOnExit__deps: ['$EvoRunExit'],
    $EvoPicker: {
        regions: {},
        press: null,
        released: null,
        active: null,
        nextId: 0,
        listening: false,
        canvas: function () { return Module.canvas || document.getElementById('unity-canvas'); },
        inside: function (region, event) {
            var bounds = EvoPicker.canvas().getBoundingClientRect();
            var x = (event.clientX - bounds.left) / bounds.width;
            var y = (event.clientY - bounds.top) / bounds.height;
            return region && region.enabled && x >= region.x && y >= region.y &&
                x <= region.x + region.width && y <= region.y + region.height;
        },
        send: function (method, value) {
            if (typeof SendMessage === 'function') SendMessage('EvoFileBridge', method, value);
            else if (Module.SendMessage) Module.SendMessage('EvoFileBridge', method, value);
            else if (window.unityInstance) window.unityInstance.SendMessage('EvoFileBridge', method, value);
        },
        ensure: function () {
            if (EvoPicker.listening) return;
            EvoPicker.listening = true;
            document.addEventListener('pointerdown', function (e) {
                EvoPicker.press = EvoPicker.released = null;
                if (e.button !== 0 || e.target !== EvoPicker.canvas() || EvoPicker.active) return;
                Object.keys(EvoPicker.regions).some(function (slot) {
                    if (!EvoPicker.inside(EvoPicker.regions[slot], e)) return false;
                    EvoPicker.press = { slot: slot, pointer: e.pointerId };
                    return true;
                });
            }, true);
            document.addEventListener('pointerup', function (e) {
                var press = EvoPicker.press;
                EvoPicker.press = null;
                EvoPicker.released = press && press.pointer === e.pointerId && e.button === 0 &&
                    EvoPicker.inside(EvoPicker.regions[press.slot], e) ? press : null;
            }, true);
            document.addEventListener('pointercancel', function () {
                EvoPicker.press = EvoPicker.released = null;
            }, true);
            document.addEventListener('click', function (e) {
                var press = EvoPicker.released;
                EvoPicker.press = EvoPicker.released = null;
                if (!press || e.target !== EvoPicker.canvas() || EvoPicker.active ||
                    !EvoPicker.inside(EvoPicker.regions[press.slot], e)) return;
                var op = { id: ++EvoPicker.nextId, input: null, reader: null };
                EvoPicker.active = op;
                EvoPicker.send('OnPickerRequested', press.slot + ':' + op.id);
                if (EvoPicker.active === op) EvoPicker.open(op);
            }, true);
        },
        finish: function (op, status, text) {
            if (EvoPicker.active !== op) return;
            EvoPicker.active = null;
            if (op.input && op.input.parentNode) op.input.parentNode.removeChild(op.input);
            EvoPicker.send('OnFilePicked', JSON.stringify({ id: op.id, status: status, text: text || '' }));
        },
        open: function (op) {
            var input = document.createElement('input');
            op.input = input;
            input.type = 'file';
            input.accept = '.json,application/json';
            input.style.display = 'none';
            input.addEventListener('change', function () {
                var file = input.files && input.files[0];
                if (!file) { EvoPicker.finish(op, 'cancelled', ''); return; }
                // Same 64 MiB import limit as SnapshotValidation, before allocating a reader.
                if (file.size > 64 * 1024 * 1024) { EvoPicker.finish(op, 'error', ''); return; }
                var reader = new FileReader();
                op.reader = reader;
                reader.onload = function () { EvoPicker.finish(op, 'success', String(reader.result)); };
                reader.onerror = function () { EvoPicker.finish(op, 'error', ''); };
                try { reader.readAsText(file); }
                catch (e) { EvoPicker.finish(op, 'error', ''); }
            });
            input.addEventListener('cancel', function () { EvoPicker.finish(op, 'cancelled', ''); });
            document.body.appendChild(input);
            try { input.click(); }
            catch (e) { EvoPicker.finish(op, 'error', ''); }
        }
    },

    DownloadBytes: function (namePtr, mimePtr, dataPtr, length) {
        var name = UTF8ToString(namePtr), mime = UTF8ToString(mimePtr);
        var blob = new Blob([new Uint8Array(HEAPU8.buffer, dataPtr, length).slice()], { type: mime });
        var url = URL.createObjectURL(blob), a = document.createElement('a');
        a.href = url;
        a.download = name;
        a.style.display = 'none';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        setTimeout(function () { URL.revokeObjectURL(url); }, 10000);
    },
    PixelRatio: function () { return window.devicePixelRatio || 1; },
    PickerRegion: function (slot, x, y, width, height, enabled) {
        EvoPicker.ensure();
        EvoPicker.regions[slot] = { x: x, y: y, width: width, height: height, enabled: !!enabled };
    },
    PickerRegion__deps: ['$EvoPicker'],
    CancelPicker: function (id) {
        var op = EvoPicker.active;
        if (!op || op.id !== id) return;
        EvoPicker.active = null;
        if (op.reader && op.reader.readyState === 1) op.reader.abort();
        if (op.input && op.input.parentNode) op.input.parentNode.removeChild(op.input);
    },
    CancelPicker__deps: ['$EvoPicker']
});
