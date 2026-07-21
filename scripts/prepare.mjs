#!/usr/bin/env node
import { spawnSync } from "node:child_process";

if (
  process.env.CI === "true" ||
  process.env.CI === "1" ||
  process.env.LEFTHOOK === "0"
) {
  process.exit(0);
}

const result = spawnSync("lefthook", ["install"], {
  stdio: "inherit",
  shell: process.platform === "win32",
});

process.exit(result.status ?? 0);
