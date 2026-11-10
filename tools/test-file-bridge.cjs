// Tests the shipped browser bridge, including gestures and asynchronous races,
// without launching a browser or running the simulation.
const assert = require("node:assert/strict");
const fs = require("node:fs");
const vm = require("node:vm");
const path = require("node:path");
const source = fs.readFileSync(path.join(__dirname, "../EvoSim/Assets/Plugins/WebGL/FileBridge.jslib"), "utf8");
let passed = 0;
function fixture() {
    const listeners = {}, exitListeners = new Set(), inputs = [], messages = [];
    const canvas = { getBoundingClientRect: () => ({ left: 10, top: 20, width: 1600, height: 900 }) };
    const readers = [];
    const body = {
        appendChild(input) { input.parentNode = this; },
        removeChild(input) { input.parentNode = null; }
    };
    const context = {
        Module: { canvas }, LibraryManager: { library: {} },
        mergeInto(target, value) { Object.assign(target, value); },
        SendMessage(object, method, value) { messages.push({ method, value }); },
        document: {
            addEventListener(name, callback) { listeners[name] = callback; },
            getElementById() { return canvas; },
            createElement() {
                const handlers = {};
                const input = { style: {}, files: [], addEventListener(n, cb) { handlers[n] = cb; }, click() { this.opened = true; }, handlers };
                inputs.push(input);
                return input;
            }, body
        },
        FileReader: class {
            constructor() { readers.push(this); this.readyState = 0; }
            readAsText(file) { this.readyState = 1; this.file = file; }
            abort() { this.readyState = 2; }
        },
        window: { devicePixelRatio: 0.8,
            addEventListener(name, callback) { assert.equal(name, "beforeunload"); exitListeners.add(callback); },
            removeEventListener(name, callback) { assert.equal(name, "beforeunload"); exitListeners.delete(callback); }
        }, JSON
    };
    vm.createContext(context);
    vm.runInContext(source, context);
    const library = context.LibraryManager.library;
    context.EvoPicker = library.$EvoPicker;
    context.EvoRunExit = library.$EvoRunExit;
    library.PickerRegion(1, 0.5, 0, 0.1, 0.1, 1);
    const event = (x, y, extra = {}) => ({ clientX: x, clientY: y, target: canvas, pointerId: 1, button: 0, ...extra });
    const emit = (type, x = 850, y = 50, extra) => listeners[type](event(x, y, extra));
    const click = () => { emit("pointerdown"); emit("pointerup"); emit("click"); };
    return { library, picker: context.EvoPicker, inputs, readers, messages, emit, click, exitListeners };
}
function test(name, check) {
    check(fixture());
    passed++;
    console.log("PASS " + name);
}
test("Only a matching press and release opens the chooser", f => {
    f.click(); assert.equal(f.inputs.length, 1); assert.equal(f.inputs[0].opened, true);
    assert.equal(f.messages[0].method, "OnPickerRequested");
});
test("Releasing outside Load cancels and cannot arm a later click", f => {
    f.emit("pointerdown"); f.emit("pointerup", 400); f.emit("click", 400);
    f.emit("click"); assert.equal(f.inputs.length, 0);
});
test("A click without the eligible press cannot open Load", f => {
    f.emit("click"); assert.equal(f.inputs.length, 0);
});
test("Pointer cancellation clears the gesture", f => {
    f.emit("pointerdown"); f.emit("pointercancel"); f.emit("pointerup"); f.emit("click");
    assert.equal(f.inputs.length, 0);
});
test("Right click cannot open Load", f => {
    f.emit("pointerdown", 850, 50, { button: 2 }); f.emit("pointerup", 850, 50, { button: 2 }); f.emit("click");
    assert.equal(f.inputs.length, 0);
});
test("A disabled or hidden region cannot open Load", f => {
    f.library.PickerRegion(1, 0.5, 0, 0.1, 0.1, 0);
    f.click(); assert.equal(f.inputs.length, 0);
});
test("Duplicate Loads are rejected while a read is pending", f => {
    f.click(); f.inputs[0].files = [{}]; f.inputs[0].handlers.change(); f.click();
    assert.equal(f.inputs.length, 1);
});
test("Stale read completion cannot replace a newer request", f => {
    f.click(); f.inputs[0].files = [{}]; f.inputs[0].handlers.change();
    const stale = f.readers[0]; f.library.CancelPicker(1); f.click();
    const count = f.messages.length;
    stale.result = '{"old":true}'; stale.onload();
    assert.equal(f.messages.length, count); assert.equal(f.picker.active.id, 2);
});
test("Empty selected files are successes for parser validation, not Cancel", f => {
    f.click(); f.inputs[0].files = [{}]; f.inputs[0].handlers.change();
    f.readers[0].result = ""; f.readers[0].onload();
    const result = JSON.parse(f.messages.at(-1).value);
    assert.equal(result.status, "success"); assert.equal(result.text, "");
});
test("Read errors have an explicit error status", f => {
    f.click(); f.inputs[0].files = [{}]; f.inputs[0].handlers.change();
    f.readers[0].onerror();
    assert.equal(JSON.parse(f.messages.at(-1).value).status, "error");
});
test("Oversized files fail before allocating a reader and leave Load available", f => {
    f.click(); f.inputs[0].files = [{ size: 64 * 1024 * 1024 + 1 }]; f.inputs[0].handlers.change();
    assert.equal(f.readers.length, 0);
    assert.equal(JSON.parse(f.messages.at(-1).value).status, "error");
    assert.equal(f.inputs[0].parentNode, null);
    f.click(); assert.equal(f.inputs.length, 2);
});
test("Cancel has a distinct status and cleans the input", f => {
    f.click(); f.inputs[0].handlers.cancel();
    assert.equal(JSON.parse(f.messages.at(-1).value).status, "cancelled");
    assert.equal(f.inputs[0].parentNode, null);
});
test("Fractional device pixel ratios are preserved", f => assert.equal(f.library.PixelRatio(), 0.8));
test("Only an existing Run protects browser exit, including repeated replacements", f => {
    assert.equal(f.exitListeners.size, 0);
    f.library.ProtectRunOnExit(1);
    f.library.ProtectRunOnExit(1);
    assert.equal(f.exitListeners.size, 1);
    const event = { prevented: false, preventDefault() { this.prevented = true; } };
    [...f.exitListeners][0](event);
    assert.equal(event.prevented, true); assert.equal(event.returnValue, "");
    f.library.ProtectRunOnExit(0);
    assert.equal(f.exitListeners.size, 0);
});
console.log(`${passed}/${passed} browser bridge checks passed`);
