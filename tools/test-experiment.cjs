// Short script checks with a stub SDK. The real Unity-SDK fallback is smoke-tested with zero minutes.
const assert = require("node:assert/strict");
const fs = require("node:fs");
const os = require("node:os");
const path = require("node:path");
const { spawnSync } = require("node:child_process");
const directory = fs.mkdtempSync(path.join(os.tmpdir(), "divergence-sdk-"));
try {
    const calls = path.join(directory, "calls");
    fs.writeFileSync(path.join(directory, "dotnet"), '#!/bin/bash\nif [[ "$1" == "--list-sdks" ]]; then printf "%s\\n" "$SDK_LIST"; exit 0; fi\nprintf "%s\\n" "$@" >> "$SDK_CALLS"\n');
    fs.chmodSync(path.join(directory, "dotnet"), 0o755);
    const init = path.join(directory, "init");
    // Hide installed Unity candidates only for the missing-SDK prerequisite case.
    fs.writeFileSync(init, '[() { if [[ "$1" == "-x" && "$2" == /Applications/Unity/Hub/Editor/*/Unity.app/Contents/Resources/Scripting/DotNetSdk/dotnet ]]; then return 1; fi; builtin [ "$@"; }\n');
    const run = (args, sdk) => spawnSync("/bin/bash", [path.join(__dirname, "experiment.sh"), ...args], {
        env: { ...process.env, PATH: directory + ":" + process.env.PATH, SDK_LIST: sdk, SDK_CALLS: calls, BASH_ENV: init },
        encoding: "utf8", timeout: 5000
    });
    let result = run(["Mild world", "0", "/tmp/Events with spaces.txt"], "8.0.408 [/sdk]");
    assert.equal(result.status, 0, result.stderr);
    let args = fs.readFileSync(calls, "utf8");
    assert.ok(args.includes("Mild world\n0\n120\n"));
    assert.ok(args.includes("events=/tmp/Events with spaces.txt\n"));
    fs.unlinkSync(calls);
    for (const sdk of ["", "7.0.100 [/sdk]", "9.0.100 [/sdk]"]) {
        result = run(["Mild world", "0"], sdk);
        assert.equal(result.status, 1);
        assert.match(result.stderr, /A \.NET 8 SDK is required/);
        assert.equal(fs.existsSync(calls), false, "Must not build or run without a supported SDK");
    }
    result = run([], "8.0.408 [/sdk]");
    assert.equal(result.status, 2);
    assert.match(result.stderr, /Usage:/);
    console.log("5/5 experiment script checks passed");
} finally {
    fs.rmSync(directory, { recursive: true, force: true });
}
