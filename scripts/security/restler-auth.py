"""RESTler authentication module — logs in and returns a Bearer token header."""

from __future__ import annotations

import json
import os
import urllib.error
import urllib.request


def _load_restler_env() -> dict[str, str]:
    values: dict[str, str] = {}
    env_path = "/repo/config/restler.env"
    if not os.path.isfile(env_path):
        return values

    with open(env_path, encoding="utf-8") as handle:
        for line in handle:
            stripped = line.strip()
            if not stripped or stripped.startswith("#"):
                continue
            if "=" not in stripped:
                continue
            key, value = stripped.split("=", 1)
            values[key.strip()] = value.strip()
    return values


def _resolve_api_base(data: dict) -> str:
    for source in (
        os.environ.get("RESTLER_API_BASE"),
        data.get("api_base"),
        _load_restler_env().get("RESTLER_API_BASE"),
    ):
        if source:
            return str(source).rstrip("/")
    return "http://backend:8080/api/v1"


def _resolve_credentials(data: dict) -> tuple[str, str]:
    file_env = _load_restler_env()
    email = (
        os.environ.get("RESTLER_LOGIN_EMAIL")
        or data.get("email")
        or file_env.get("RESTLER_LOGIN_EMAIL")
        or "admin@example.local"
    )
    password = (
        os.environ.get("RESTLER_LOGIN_PASSWORD")
        or data.get("password")
        or file_env.get("RESTLER_LOGIN_PASSWORD")
        or "admin123"
    )
    return email, password


def acquire_token(data, log):
    api_base = _resolve_api_base(data or {})
    email, password = _resolve_credentials(data or {})

    payload = json.dumps({"email": email, "password": password}).encode("utf-8")
    request = urllib.request.Request(
        f"{api_base}/auth/login",
        data=payload,
        headers={"Content-Type": "application/json"},
        method="POST",
    )

    try:
        with urllib.request.urlopen(request, timeout=30) as response:
            body = json.loads(response.read().decode("utf-8"))
    except urllib.error.HTTPError as error:
        detail = error.read().decode("utf-8", errors="replace")
        log(f"Login failed ({error.code}): {detail[:500]}")
        raise

    root = body.get("value") or body
    session = root.get("authSession") or root.get("AuthSession")
    if not session:
        raise RuntimeError("Login JSON missing authSession")

    token = session.get("token") or session.get("Token")
    if not token:
        raise RuntimeError("Login JSON missing authSession.token")

    log(f"Authenticated as {email}")

    return "\n".join(
        [
            "{'changeme': {}}",
            f"Authorization: Bearer {token}",
        ]
    )
