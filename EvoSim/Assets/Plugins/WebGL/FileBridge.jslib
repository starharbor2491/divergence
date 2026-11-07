// Browser side of Evo.UnityLayer.FileBridge: download bytes as a file, and pick a text file from disk.
//
// Browsers only open a file dialog during a real user gesture. Unity handles the pointer event a frame later,
// so the Load button arms the picker on pointer down (ArmPicker) and the browser's own click event opens the dialog.
// PickFile, called later by Unity, only registers where to send the result unless the dialog is not open yet.
mergeInto(LibraryManager.library, {

    $EvoPicker: {
        armedAt: 0,
        openedAt: 0,
        accept: '.json',
        deliver: null,
        listening: false,
        ensure: function () {
            if (EvoPicker.listening) return;
            EvoPicker.listening = true;
            document.addEventListener('click', function () {
                if (EvoPicker.armedAt > 0 && performance.now() - EvoPicker.armedAt < 3000) {
                    EvoPicker.armedAt = 0;
                    EvoPicker.open();
                }
            }, true);
        },
        open: function () {
            EvoPicker.openedAt = performance.now();
            var input = document.createElement('input');
            input.type = 'file';
            input.accept = EvoPicker.accept;
            input.style.display = 'none';
            var finish = function (text) {
                if (input.parentNode) input.parentNode.removeChild(input);
                if (EvoPicker.deliver) EvoPicker.deliver(text);
            };
            input.addEventListener('change', function () {
                var file = input.files && input.files[0];
                if (!file) { finish(''); return; }
                var reader = new FileReader();
                reader.onload = function () { finish(String(reader.result)); };
                reader.onerror = function () { finish(''); };
                reader.readAsText(file);
            });
            input.addEventListener('cancel', function () { finish(''); });
            document.body.appendChild(input);
            input.click();
        }
    },

    DownloadBytes: function (namePtr, mimePtr, dataPtr, length) {
        var name = UTF8ToString(namePtr);
        var mime = UTF8ToString(mimePtr);
        var bytes = new Uint8Array(HEAPU8.buffer, dataPtr, length).slice();
        var blob = new Blob([bytes], { type: mime });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = name;
        a.style.display = 'none';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        setTimeout(function () { URL.revokeObjectURL(url); }, 10000);
    },

    PixelRatio: function () {
        return window.devicePixelRatio || 1;
    },

    ArmPicker: function (acceptPtr) {
        EvoPicker.ensure();
        EvoPicker.accept = UTF8ToString(acceptPtr);
        EvoPicker.armedAt = performance.now();
    },
    ArmPicker__deps: ['$EvoPicker'],

    PickFile: function (objectPtr, methodPtr, acceptPtr) {
        EvoPicker.ensure();
        var objectName = UTF8ToString(objectPtr);
        var method = UTF8ToString(methodPtr);
        EvoPicker.accept = UTF8ToString(acceptPtr);
        EvoPicker.deliver = function (text) {
            var send = (typeof SendMessage === 'function') ? SendMessage
                : (typeof Module !== 'undefined' && Module.SendMessage) ? Module.SendMessage
                : (window.unityInstance && window.unityInstance.SendMessage) ? function (o, m, v) { window.unityInstance.SendMessage(o, m, v); }
                : null;
            if (send) send(objectName, method, text);
        };
        if (EvoPicker.openedAt > 0 && performance.now() - EvoPicker.openedAt < 3000) return;
        EvoPicker.armedAt = 0;
        EvoPicker.open();
    },
    PickFile__deps: ['$EvoPicker']
});
