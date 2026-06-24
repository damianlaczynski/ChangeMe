import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";

const TOKEN_NAME = "changeme-local";
const INITIAL_PASSWORD = "admin";

export function loadSonarConfig(root = process.cwd()) {
  const config = {
    SONAR_HOST_URL: "http://localhost:9000",
    SONAR_ADMIN_USER: "admin",
    SONAR_ADMIN_PASSWORD: "StrongPass123!",
  };

  const configPath = join(root, "config", "sonar.env");
  if (!existsSync(configPath)) {
    return config;
  }

  for (const line of readFileSync(configPath, "utf8").split("\n")) {
    const trimmed = line.trim();
    if (!trimmed || trimmed.startsWith("#")) {
      continue;
    }

    const separator = trimmed.indexOf("=");
    if (separator === -1) {
      continue;
    }

    const key = trimmed.slice(0, separator).trim();
    const value = trimmed.slice(separator + 1).trim();
    config[key] = value;
  }

  return config;
}

function basicAuth(user, password) {
  return {
    Authorization: `Basic ${Buffer.from(`${user}:${password}`).toString("base64")}`,
  };
}

function tokenPath(root) {
  return join(root, "artifacts", "sonar", "token");
}

async function waitForSonar(url, attempts = 60) {
  for (let attempt = 1; attempt <= attempts; attempt += 1) {
    try {
      const response = await fetch(`${url}/api/system/status`);
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }

      const status = await response.json();
      if (status.status === "UP") {
        return;
      }
    } catch {
      // SonarQube is still starting.
    }

    await new Promise((resolve) => setTimeout(resolve, 5000));
  }

  throw new Error(`SonarQube did not become ready at ${url}`);
}

async function validateToken(url, token) {
  const response = await fetch(`${url}/api/authentication/validate`, {
    headers: { Authorization: `Bearer ${token}` },
  });

  return response.ok;
}

async function login(url, user, password) {
  const response = await fetch(`${url}/api/authentication/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
      ...basicAuth(user, password),
    },
    body: new URLSearchParams({ login: user, password }),
  });

  return response.ok;
}

async function changePassword(url, user, previousPassword, newPassword) {
  const response = await fetch(`${url}/api/users/change_password`, {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
      ...basicAuth(user, previousPassword),
    },
    body: new URLSearchParams({
      login: user,
      previousPassword,
      password: newPassword,
    }),
  });

  if (!response.ok) {
    throw new Error(
      `Failed to change SonarQube password: ${await response.text()}`,
    );
  }
}

async function revokeTokenIfExists(url, user, password) {
  const search = await fetch(`${url}/api/user_tokens/search`, {
    headers: basicAuth(user, password),
  });

  if (!search.ok) {
    return;
  }

  const data = await search.json();
  const exists = data.userTokens?.some((token) => token.name === TOKEN_NAME);
  if (!exists) {
    return;
  }

  await fetch(`${url}/api/user_tokens/revoke`, {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
      ...basicAuth(user, password),
    },
    body: new URLSearchParams({ name: TOKEN_NAME }),
  });
}

async function generateToken(url, user, password) {
  await revokeTokenIfExists(url, user, password);

  const response = await fetch(`${url}/api/user_tokens/generate`, {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
      ...basicAuth(user, password),
    },
    body: new URLSearchParams({
      name: TOKEN_NAME,
      type: "GLOBAL_ANALYSIS_TOKEN",
    }),
  });

  if (!response.ok) {
    throw new Error(
      `Failed to generate SonarQube token: ${await response.text()}`,
    );
  }

  const data = await response.json();
  return data.token;
}

async function resolveAdminPassword(url, user, targetPassword) {
  if (await login(url, user, targetPassword)) {
    return targetPassword;
  }

  if (targetPassword === INITIAL_PASSWORD) {
    throw new Error(
      `Cannot authenticate to SonarQube as ${user}. Check config/sonar.env (SONAR_ADMIN_PASSWORD).`,
    );
  }

  if (!(await login(url, user, INITIAL_PASSWORD))) {
    throw new Error(
      `Cannot authenticate to SonarQube as ${user}. Expected ${targetPassword} or first-run ${INITIAL_PASSWORD}.`,
    );
  }

  await changePassword(url, user, INITIAL_PASSWORD, targetPassword);

  if (!(await login(url, user, targetPassword))) {
    throw new Error(
      "SonarQube password change succeeded but login with the new password failed.",
    );
  }

  console.log(`SonarQube admin password set to value from config/sonar.env.`);
  return targetPassword;
}

export async function ensureSonarToken(root = process.cwd()) {
  const config = loadSonarConfig(root);
  const { SONAR_HOST_URL, SONAR_ADMIN_USER, SONAR_ADMIN_PASSWORD } = config;
  const tokenFile = tokenPath(root);

  mkdirSync(join(root, "artifacts", "sonar"), { recursive: true });

  await waitForSonar(SONAR_HOST_URL);

  if (existsSync(tokenFile)) {
    const cachedToken = readFileSync(tokenFile, "utf8").trim();
    if (cachedToken && (await validateToken(SONAR_HOST_URL, cachedToken))) {
      return cachedToken;
    }
  }

  const adminPassword = await resolveAdminPassword(
    SONAR_HOST_URL,
    SONAR_ADMIN_USER,
    SONAR_ADMIN_PASSWORD,
  );
  const token = await generateToken(
    SONAR_HOST_URL,
    SONAR_ADMIN_USER,
    adminPassword,
  );
  writeFileSync(tokenFile, `${token}\n`, "utf8");
  console.log(
    `SonarQube analysis token ready (cached in artifacts/sonar/token).`,
  );

  return token;
}
