#!/usr/bin/env node
import { spawnSync } from "node:child_process";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const shell = process.platform === "win32";

function run(command, args) {
  const result = spawnSync(command, args, {
    cwd: root,
    stdio: "inherit",
    shell,
  });

  if (result.status !== 0) {
    process.exit(result.status ?? 1);
  }
}

function checkPrerequisite(command, args, label) {
  const result = spawnSync(command, args, {
    cwd: root,
    encoding: "utf8",
    shell,
  });

  if (result.status !== 0) {
    console.error(`Missing prerequisite: ${label}`);
    process.exit(1);
  }

  const version = (result.stdout ?? result.stderr ?? "").trim().split("\n")[0];
  console.log(`✓ ${label}${version ? ` (${version})` : ""}`);
}

console.log("ChangeMe setup\n");

checkPrerequisite("node", ["--version"], "Node.js");
checkPrerequisite("dotnet", ["--version"], ".NET SDK");

const dockerCheck = spawnSync(
  "docker",
  ["version", "--format", "{{.Server.Version}}"],
  {
    cwd: root,
    encoding: "utf8",
    shell,
  },
);

if (dockerCheck.status === 0) {
  console.log(`✓ Docker (${dockerCheck.stdout.trim()})`);
} else {
  console.warn(
    "! Docker not detected — integration tests and E2E need a running Docker engine.",
  );
}

console.log("\nInstalling dependencies...");
run("npm", ["install"]);
run("npm", ["run", "install:frontend"]);
run("dotnet", ["restore", "src/ChangeMe.Backend/ChangeMe.Backend.slnx"]);

console.log("\nSetup complete.");
console.log("Next steps:");
console.log("  - PostgreSQL: docker compose up postgres mailhog -d");
console.log("  - Dev servers: npm run start:all");
console.log("  - Full stack:  npm run docker:up");
