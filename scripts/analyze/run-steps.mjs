import { execSync } from "node:child_process";

const QUICK_STEPS = [
  "analyze:deps",
  "analyze:secrets",
  "analyze:deps:audit",
  "analyze:sast",
];

const ALL_STEPS = [
  ...QUICK_STEPS,
  "analyze:deps:images",
  "analyze:dast",
  "analyze:sonar",
];

function runSteps(stepNames, label) {
  const failed = [];

  for (const name of stepNames) {
    console.log(`\n=== ${name} ===\n`);
    try {
      execSync(`npm run ${name}`, {
        stdio: "inherit",
        shell: true,
        cwd: process.cwd(),
      });
    } catch {
      failed.push(name);
      console.error(
        `[${label}] ${name} finished with errors or findings — continuing so remaining tools still write reports.\n`,
      );
    }
  }

  console.log(`\n=== ${label} summary ===`);
  console.log(
    "Reports are under artifacts/ (each tool writes even when it exits non-zero).",
  );

  if (failed.length === 0) {
    console.log("All steps completed without errors.");
    process.exit(0);
  }

  console.error(`Steps with non-zero exit: ${failed.join(", ")}`);
  console.error(
    "Review the matching files under artifacts/ before merging or releasing.",
  );
  process.exit(1);
}

const mode = process.argv[2] ?? "all";

if (mode === "quick") {
  runSteps(QUICK_STEPS, "analyze:quick");
} else if (mode === "all") {
  runSteps(ALL_STEPS, "analyze:all");
} else {
  console.error("Usage: node scripts/analyze/run-steps.mjs [quick|all]");
  process.exit(1);
}
