import { execSync } from "node:child_process";
import { mkdirSync, writeFileSync } from "node:fs";
import { join } from "node:path";

const OFFLINE_STEPS = [
  { id: "sca-trivy", layer: "SCA", script: "security:trivy" },
  { id: "sca-audit", layer: "SCA", script: "security:audit" },
  { id: "secrets", layer: "Secrets", script: "security:secrets" },
  { id: "sast", layer: "SAST", script: "security:sast" },
];

const RUNTIME_STEPS = [
  { id: "fuzz", layer: "Fuzzing", script: "security:fuzz", requires: "backend" },
  { id: "dast-frontend", layer: "DAST", script: "security:dast", requires: "frontend" },
];

const args = new Set(process.argv.slice(2));
const quick = args.has("--quick");
const deep = args.has("--deep");
const requireRuntime = args.has("--require-runtime");

const artifactsDir = join(process.cwd(), "artifacts");
mkdirSync(artifactsDir, { recursive: true });

const stepResults = [];

async function probe(url) {
  try {
    const response = await fetch(url, { signal: AbortSignal.timeout(5000) });
    return response.ok;
  } catch {
    return false;
  }
}

async function detectRuntime() {
  const backend =
    (await probe("http://localhost:5000/swagger/v1/swagger.json")) ||
    (await probe("http://127.0.0.1:5000/swagger/v1/swagger.json"));
  const frontend =
    (await probe("http://localhost:4200/")) ||
    (await probe("http://127.0.0.1:4200/"));
  return { backend, frontend };
}

function runStep(step, extraEnv = {}) {
  console.log(`\n=== ${step.script} (${step.layer}) ===\n`);
  try {
    execSync(`npm run ${step.script}`, {
      stdio: "inherit",
      shell: true,
      cwd: process.cwd(),
      env: { ...process.env, ...extraEnv },
    });
    stepResults.push({ ...step, status: "ok" });
  } catch {
    stepResults.push({ ...step, status: "failed" });
    console.error(
      `[security] ${step.script} finished with errors or findings — continuing.\n`,
    );
  }
}

function artifactHints() {
  return {
    "sca-trivy": ["artifacts/sca/trivy-report.json", "artifacts/sca/trivy-report.txt"],
    "sca-audit": [
      "artifacts/sca/npm-audit.json",
      "artifacts/sca/dotnet-vulnerable.txt",
    ],
    secrets: ["artifacts/secrets/report.json", "artifacts/secrets/scan.log"],
    sast: ["artifacts/sast/report.json", "artifacts/sast/report.sarif"],
    fuzz: ["artifacts/fuzz/summary.txt", "artifacts/fuzz/scan.log"],
    "dast-frontend": [
      "artifacts/dast/frontend/report.html",
      "artifacts/dast/frontend/report.json",
    ],
  };
}

function writeSummary(runtime) {
  const hints = artifactHints();
  const summary = {
    generatedAt: new Date().toISOString(),
    mode: quick ? "quick" : deep ? "full-deep" : "full",
    runtime,
    steps: stepResults.map((step) => ({
      id: step.id,
      layer: step.layer,
      script: step.script,
      status: step.status,
      artifacts: hints[step.id] ?? [],
    })),
  };

  const failed = stepResults.filter((step) => step.status === "failed");
  summary.failed = failed.map((step) => step.id);

  const lines = [
    `Security summary (${summary.mode})`,
    `Generated: ${summary.generatedAt}`,
    "",
    `Runtime: backend=${runtime.backend ? "up" : "down"}, frontend=${runtime.frontend ? "up" : "down"}`,
    "",
    "Steps:",
    ...stepResults.map(
      (step) =>
        `  [${step.status}] ${step.layer} — ${step.script} (${step.id})`,
    ),
  ];

  if (failed.length > 0) {
    lines.push("", `Failed: ${failed.map((step) => step.id).join(", ")}`);
    lines.push("Review artifacts listed in security-summary.json");
  } else {
    lines.push("", "All executed steps completed without errors.");
  }

  writeFileSync(
    join(artifactsDir, "security-summary.json"),
    `${JSON.stringify(summary, null, 2)}\n`,
  );
  writeFileSync(join(artifactsDir, "security-summary.txt"), `${lines.join("\n")}\n`);

  console.log("\n=== security summary ===");
  console.log(lines.join("\n"));
  console.log("\nWrote artifacts/security-summary.json");

  return failed.length;
}

const runtime = await detectRuntime();

for (const step of OFFLINE_STEPS) {
  runStep(step);
}

if (!quick) {
  if (requireRuntime && (!runtime.backend || !runtime.frontend)) {
    writeSummary(runtime);
    console.error(
      "--require-runtime: backend and frontend must be reachable (npm run docker:up:detached).",
    );
    process.exit(1);
  }

  for (const step of RUNTIME_STEPS) {
    const available =
      step.requires === "backend" ? runtime.backend : runtime.frontend;

    if (!available) {
      stepResults.push({ ...step, status: "skipped" });
      console.log(
        `\n=== ${step.script} (${step.layer}) — skipped (stack not reachable) ===\n`,
      );
      continue;
    }

    const extraEnv =
      step.id === "fuzz" && deep ? { RESTLER_MODE: "fuzz" } : undefined;
    runStep(step, extraEnv);
  }
}

const failedCount = writeSummary(runtime);
process.exit(failedCount > 0 ? 1 : 0);
