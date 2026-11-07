#!/usr/bin/env bash
# Publishes Builds/WebGL to the gh-pages branch through a git worktree, then prints the Pages URL.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
[ -f Builds/WebGL/index.html ] || { echo "Run tools/build-webgl.sh first"; exit 1; }
WT="$ROOT/Builds/gh-pages-worktree"
git fetch origin gh-pages 2>/dev/null || true
rm -rf "$WT"; git worktree prune
if git show-ref --verify --quiet refs/remotes/origin/gh-pages; then
  git worktree add -B gh-pages "$WT" origin/gh-pages
else
  git worktree add --detach "$WT"; (cd "$WT" && git checkout --orphan gh-pages && git rm -rf . >/dev/null 2>&1 || true)
fi
find "$WT" -mindepth 1 -maxdepth 1 ! -name .git -exec rm -rf {} +
cp -R Builds/WebGL/. "$WT/"
touch "$WT/.nojekyll"
(cd "$WT" && git add -A && git commit -m "Deploy WebGL build" && git push origin gh-pages)
git worktree remove --force "$WT"
echo "Published: https://$(git remote get-url origin | sed -E 's#.*github.com[:/]([^/]+)/([^/.]+)(\.git)?#\1.github.io/\2#')/"
