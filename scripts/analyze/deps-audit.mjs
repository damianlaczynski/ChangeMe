import { execSync } from "node:child_process";
import { mkdirSync, writeFileSync } from "node:fs";
import { join } from "node:path";

const root = process.cwd();
const outDir = join(root, "artifacts", "audit");
mkdirSync(outDir, { recursive: true });

let exitCode = 0;

try {
  const npmAudit = execSync("npm audit --json --prefix src/ChangeMe.Frontend", {
    cwd: root,
    encoding: "utf8",
    stdio: ["ignore", "pipe", "pipe"],
  });
  writeFileSync(join(outDir, "npm-audit.json"), npmAudit);
} catch (error) {
  const output = error.stdout?.toString() ?? error.message;
  writeFileSync(join(outDir, "npm-audit.json"), output);
  exitCode = error.status ?? 1;
}

try {
  const dotnetVulnerable = execSync(
    "dotnet list src/ChangeMe.Backend/ChangeMe.Backend.slnx package --vulnerable",
    { cwd: root, encoding: "utf8", stdio: ["ignore", "pipe", "pipe"] },
  );
  writeFileSync(join(outDir, "dotnet-vulnerable.txt"), dotnetVulnerable);
} catch (error) {
  const output = `${error.stdout?.toString() ?? ""}${error.stderr?.toString() ?? ""}${error.message}`;
  writeFileSync(join(outDir, "dotnet-vulnerable.txt"), output);
  exitCode = error.status ?? 1;
}

console.log(`Wrote ${join(outDir, "npm-audit.json")}`);
console.log(`Wrote ${join(outDir, "dotnet-vulnerable.txt")}`);

process.exit(exitCode);
